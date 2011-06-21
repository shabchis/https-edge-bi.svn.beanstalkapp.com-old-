using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Utilities;
using System.IO;
using Easynet.Edge.Core.Configuration;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace EdgeBI.Wizards.AccountWizard
{
	class CreatePanoramaBookExecutor : StepExecuter
	{
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool TerminateProcess(IntPtr hprocess, uint uExitCode);
		private const string AccSettClientSpecific = "AccountSettings.Client Specific";
        private const string C_AccSettACQ = "AccountSettings.ACQ";
        private const string C_AccSettTargetACQ = "AccountSettings.TargetACQ";
        
		protected override Easynet.Edge.Core.Services.ServiceOutcome DoWork()
		{
			//Log.Write("Getting collected data from suitable collector", LogMessageType.Information);

			Dictionary<string, object> collectedData = this.GetCollectedData(); 
			this.ReportProgress(0.1f);
			Log.Write("Adding new book to panorama", LogMessageType.Information);
            AddNewBook(collectedData);

			this.ReportProgress(0.7f);
			Log.Write("book Created", LogMessageType.Information);

			//Log.Write("Update OLTP datbase", LogMessageType.Information);
			//UpdateOltpDataBASE(collectedData); //since the book data is taken from last steps their is no point update it again.
			this.ReportProgress(1);

			return base.DoWork();
		}

		private void AddNewBook(Dictionary<string,object> collectedData)
		{

            if (collectedData.ContainsKey(ApplicationIDKey))
                SetAccountWizardSettingsByApllicationID(Convert.ToInt32(collectedData[ApplicationIDKey]));

			// COPY THE TEMPLATE BOOK (PATH ON APP.CONFIG FOLDER AND RENAME IT'S NAME TO BO+NEW ACCOUNT NAME
            DirectoryInfo templateSourceDirectory = new DirectoryInfo(Path.Combine(accountWizardSettings.Get("Folder.PanoramaBooks"), accountWizardSettings.Get("Folder.PanoramaBookTemplate"))); //source path
            DirectoryInfo targetDirectory = new DirectoryInfo(Path.Combine(accountWizardSettings.Get("Folder.PanoramaBooks"), accountWizardSettings.Get("Cube.BO.Name.Perfix") + collectedData["AccountSettings.CubeName"].ToString()));  //target path

			// Check if the target directory exists, if not, create it.
			if (!Directory.Exists(targetDirectory.FullName))
			{
				try
				{
					Directory.CreateDirectory(targetDirectory.FullName);
					string newUsersChanged = string.Empty;
                    bool copyContent = false;
					string newActiveUsersChanged = string.Empty;
                    if (collectedData.ContainsKey(C_AccSettACQ + "1"))
                        newUsersChanged = ((Replacment)collectedData[C_AccSettACQ + "1"]).ReplaceTo;
                    if (collectedData.ContainsKey(C_AccSettACQ + "2"))
                        newActiveUsersChanged = ((Replacment)collectedData[C_AccSettACQ + "2"]).ReplaceTo;
                    if (bool.Parse(collectedData["AccountSettings.AddContentCube"].ToString()) == true)
                        copyContent = true;
               
					CopyAllFilesAndFolders(templateSourceDirectory, targetDirectory,newUsersChanged,newActiveUsersChanged,copyContent);					

				}
				catch (Exception ex)
				{
					Log.Write("Problem creating  new book directory", ex);
					if (Directory.Exists(targetDirectory.FullName))
						Directory.Delete(targetDirectory.FullName);
					throw new Exception("Problem creating  new book directory", ex);
				}

			}


			//CHANGE THE RELEVANT PROPERTIES ON ALL XML FILES IN THE DIRECTORY (EXCLUDE THE SCHEMA XML AND PROPERTIES XML
            UpdateRelevantPropertiesOnXml(targetDirectory, collectedData);

			//GET THE APPLICATION.XML FROM DEFINED PATH ON APP.CONFIG AND ADD THE NEW BOOK (CHANGE RELEVANT PROPERTIES ON THIS XML)

            XDocument applicationXml = XDocument.Load(accountWizardSettings.Get("Folder.File.AplicationXml"));
			XElement applicationElement = applicationXml.Element("pnView").Element("Applications");
			int numberOfItemElements = applicationElement.Elements().Count();//since the first element is not an item element but Properties element
			//So the next item element to be add is numberOfItemElements +1

			applicationElement.Add(new XElement(string.Format("Item{0}", numberOfItemElements),
				new XElement("Properties",
                    new XAttribute("Name", accountWizardSettings.Get("Cube.BO.Name.Perfix") + collectedData["AccountSettings.CubeName"].ToString()),
					new XAttribute("Path", Path.Combine(targetDirectory.FullName, "schema.xml")),
					new XAttribute("Description", string.Empty),
					new XAttribute("Flags", "1"),
					new XAttribute("DefPerm", "0")),
				new XElement("Roles",
					new XElement("Properties",
						new XAttribute("Value", "Pn0102{}")))));
			//save and overite th file
            applicationXml.Save(accountWizardSettings.Get("Folder.File.AplicationXml"));

			//REFRESH PANORAMA ADMIN CONNECTION BY OPENING THE FILE WITH PROCCES (THE PATH DEFINE ON APP.CONFIG)

			System.Diagnostics.Process Proc = new System.Diagnostics.Process();
            Proc.StartInfo.FileName = accountWizardSettings.Get("Folder.File.PanoramaMsc");
            Proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			

            try
            {
                Proc.Start();
                System.Threading.Thread.Sleep(4000);
                uint code=(uint)Proc.ExitCode;
                if (!Proc.HasExited)
                    Proc.Kill();
                else
                    TerminateProcess(Proc.Handle, code);
            }
            catch (Exception)
            {
                
                
            }



		}

		private void UpdateRelevantPropertiesOnXml(DirectoryInfo targetDirectory, Dictionary<string, object> lastExecutorStepData)
		{
            string pattern;
			foreach (FileInfo file in targetDirectory.GetFiles())
			{
				if (file.Name.ToLower() == "schema.xml" || file.Name.ToLower() == "properties.xml" || file.Name.ToLower()== "refreshbook")
					continue;
				else
				{
					string fileString = string.Empty;
					using (StreamReader reader=new StreamReader(file.FullName))
					{
						fileString = reader.ReadToEnd();
						//channge CubeAdress,CubeName,CubeDB atributes	


						
					}
                    pattern = RegxUtils.CreateExactMatchWholeWordRegExpression(accountWizardSettings.Get("Panorama.ServerToReplace")); //replace server 
					//CubeAdress
                    fileString = Regex.Replace(fileString, pattern, accountWizardSettings.Get("AnalysisServer.ConnectionString").Replace("DataSource=", string.Empty), RegexOptions.IgnoreCase);
					//CubeName //few options here

                    pattern = RegxUtils.CreateExactMatchWholeWordRegExpression(accountWizardSettings.Get("Panorama.ContentCubeToReplace"));//ContentCubeToReplace

                    fileString = Regex.Replace(fileString, pattern, accountWizardSettings.Get("Cube.Content.Name.Perfix") + lastExecutorStepData["AccountSettings.CubeName"].ToString(), RegexOptions.IgnoreCase);

                    pattern = RegxUtils.CreateExactMatchWholeWordRegExpression(accountWizardSettings.Get("Panorama.BoCubeToReplace")); //BoCubeToReplace
                    fileString = Regex.Replace(fileString, pattern, accountWizardSettings.Get("Cube.BO.Name.Perfix") + lastExecutorStepData["AccountSettings.CubeName"].ToString(), RegexOptions.IgnoreCase);


                    pattern = RegxUtils.CreateExactMatchWholeWordRegExpression(accountWizardSettings.Get("Panorama.CubeDbtoReplace"));//CubeDbtoReplace
                    fileString = Regex.Replace(fileString, pattern, accountWizardSettings.Get("AnalysisServer.Database"), RegexOptions.IgnoreCase);

                    //Replace client specific measures +Acquisitions +target AcquisitionS

					foreach (KeyValuePair<string,object> input in lastExecutorStepData)
					{

                        if (input.Value is Replacment)
                        {
                            Replacment replacment = (Replacment)input.Value;
                            if (input.Key.StartsWith(AccSettClientSpecific, true, null))//measures
                            {
                                pattern = RegxUtils.CreateExactMatchWholeWordRegExpression(replacment.ReplaceFrom);
                                fileString = Regex.Replace(fileString, pattern, replacment.ReplaceTo, RegexOptions.IgnoreCase);

                            }
                            else if (input.Key.StartsWith("AccountSettings.StringReplacment."))//String ReplaceMent
                            {
                                pattern = RegxUtils.CreateExactMatchWholeWordRegExpression( replacment.ReplaceFrom);
                                fileString = Regex.Replace(fileString, pattern, replacment.ReplaceTo, RegexOptions.IgnoreCase);

                            }
                            else if (input.Key.StartsWith(C_AccSettACQ))// Acquisitions
                            {
                                if (input.Value.ToString() != " ")
                                {
                                    string[] acquisitions = accountWizardSettings.Get(input.Key).Split(',');
                                    foreach (string acquisition in acquisitions)
                                    {
                                        pattern = @"\b" + acquisition + @"\b";
                                        fileString = Regex.Replace(fileString, pattern, replacment.ReplaceTo, RegexOptions.IgnoreCase);
                                    }
                                }

                            }
                            else if (input.Key.StartsWith(C_AccSettTargetACQ)) //TARGET Acquisitions
                            {
                                if (input.Value.ToString() != " ")
                                {
                                    string[] targetAcquisitions = accountWizardSettings.Get(input.Key).Split(',');
                                    foreach (string targetAcquisition in targetAcquisitions)
                                    {
                                        pattern = @"\b" + targetAcquisition +@"\b";
                                        fileString = Regex.Replace(fileString, pattern, replacment.ReplaceTo, RegexOptions.IgnoreCase);
                                    }
                                }
                            }						 
                        }
					}				

					using (StreamWriter writer=new StreamWriter(file.FullName,false))
					{
						writer.Write(fileString);
						
					}
				}
				
			}
		}

		private void CopyAllFilesAndFolders(DirectoryInfo templateSourceDirectory, DirectoryInfo targetDirectory,string newUsersChanged,string newActiverUsersChanged ,bool copyContent)
		{
            
            
			foreach (FileInfo file in templateSourceDirectory.GetFiles() )
			{
                if (file.Name.Equals(accountWizardSettings.Get("Panorama.ROIViewsRegs")) && !string.IsNullOrEmpty(newUsersChanged)) //regs				
					file.CopyTo(Path.Combine(targetDirectory.ToString(),string.Format("4. ROI by {0}.xml",newUsersChanged)), true);

                else if (file.Name.Equals(accountWizardSettings.Get("Panorama.ROIViewsActives")) && !string.IsNullOrEmpty(newActiverUsersChanged)) //actives				
                    file.CopyTo(Path.Combine(targetDirectory.ToString(), string.Format("4. ROI by {0}.xml", newActiverUsersChanged)), true);
                else
                {
                    if (copyContent) //if we create content cube then copy content files ,else...not
                    {
                        file.CopyTo(Path.Combine(targetDirectory.ToString(), file.Name), true);
                    }
                    else
                    {
                        string[] contentFiles = accountWizardSettings.Get("Panorama.ContentViews").Split(',');
                        bool copyFile = true;                        
                        foreach (string contentFile in contentFiles)
                        {
                            if (file.Name.Equals(contentFile))
                                copyFile = false;
                            
                        }
                        if (copyFile)
                        {
                            file.CopyTo(Path.Combine(targetDirectory.ToString(), file.Name), true);
                        }
                    }
                   
                }

				
			}
			foreach (DirectoryInfo sourceSubDirectory in templateSourceDirectory.GetDirectories())
			{
				DirectoryInfo nextTargetDirectory = targetDirectory.CreateSubdirectory(sourceSubDirectory.Name);
				CopyAllFilesAndFolders(sourceSubDirectory, nextTargetDirectory, newUsersChanged, newActiverUsersChanged,copyContent);
				
			}
            if (!File.Exists(Path.Combine(targetDirectory.FullName,"refreshbook") ))
            {
                File.Create(Path.Combine(targetDirectory.FullName,"refreshbook"));
                
            }




		}


		
	}
}
