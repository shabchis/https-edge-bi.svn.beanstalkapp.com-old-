using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Utilities;
using System.IO;
using Easynet.Edge.Core.Configuration;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace EdgeBI.Wizards.AccountWizard
{
	class CreatePanoramaBookExecutor : StepExecuter
	{
		private const string AccSettClientSpecific = "AccountSettings.Client Specific";
		private const string AccSettNewUser = "AccountSettings.New Users";
		private const string AccSettNewActiveUser = "AccountSettings.New Active Users";
		protected override Easynet.Edge.Core.Services.ServiceOutcome DoWork()
		{
			//Log.Write("Getting collected data from suitable collector", LogMessageType.Information);

			/*Dictionary<string, object> collectedData = this.GetStepCollectedData(Instance.Configuration.Options["CollectorStep"]); no data to collect from this
			 * collector since the data came from other step and executor*/
			this.ReportProgress(0.1f);
			Log.Write("Adding new book to panorama", LogMessageType.Information);
			AddNewBook();

			this.ReportProgress(0.7f);
			Log.Write("book Created", LogMessageType.Information);

			//Log.Write("Update OLTP datbase", LogMessageType.Information);
			//UpdateOltpDataBASE(collectedData); //since the book data is taken from last steps their is no point update it again.
			this.ReportProgress(1);

			return base.DoWork();
		}

		private void AddNewBook()
		{
			Dictionary<string, object> lastExecutorStepData = GetExecutorData("CreateNewCubeExecutor");

			// COPY THE TEMPLATE BOOK (PATH ON APP.CONFIG FOLDER AND RENAME IT'S NAME TO BO+NEW ACCOUNT NAME
			DirectoryInfo templateSourceDirectory = new DirectoryInfo(Path.Combine(AppSettings.Get(this, "Folder.PanoramaBooks"), AppSettings.Get(this, "Folder.PanoramaBookTemplate"))); //source path
            DirectoryInfo targetDirectory = new DirectoryInfo(Path.Combine(AppSettings.Get(this, "Folder.PanoramaBooks"), AppSettings.GetAbsolute("EdgeBI.Wizards.StepExecuter.Cube.BO.Name.Perfix") + lastExecutorStepData["AccountSettings.CubeName"].ToString()));  //target path

			// Check if the target directory exists, if not, create it.
			if (!Directory.Exists(targetDirectory.FullName))
			{
				try
				{
					Directory.CreateDirectory(targetDirectory.FullName);
					string newUsersChanged = string.Empty;
                    bool copyContent = false;
					string newActiveUsersChanged = string.Empty;
					if (lastExecutorStepData.ContainsKey("AccountSettings.New Users"))
						newUsersChanged = ((Replacment)lastExecutorStepData["AccountSettings.New Users"]).ReplaceTo;
                    if (lastExecutorStepData.ContainsKey("AccountSettings.New Active Users"))
                        newActiveUsersChanged = ((Replacment)lastExecutorStepData["AccountSettings.New Active Users"]).ReplaceTo;
                    if (bool.Parse(lastExecutorStepData["AccountSettings.AddContentCube"].ToString()) == true)
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
			UpdateRelevantPropertiesOnXml(targetDirectory, lastExecutorStepData);

			//GET THE APPLICATION.XML FROM DEFINED PATH ON APP.CONFIG AND ADD THE NEW BOOK (CHANGE RELEVANT PROPERTIES ON THIS XML)

			XDocument applicationXml = XDocument.Load(AppSettings.Get(this, "Folder.File.AplicationXml"));
			XElement applicationElement = applicationXml.Element("pnView").Element("Applications");
			int numberOfItemElements = applicationElement.Elements().Count();//since the first element is not an item element but Properties element
			//So the next item element to be add is numberOfItemElements +1

			applicationElement.Add(new XElement(string.Format("Item{0}", numberOfItemElements),
				new XElement("Properties",
                    new XAttribute("Name", AppSettings.GetAbsolute("EdgeBI.Wizards.StepExecuter.Cube.BO.Name.Perfix") + lastExecutorStepData["AccountSettings.CubeName"].ToString()),
					new XAttribute("Path", Path.Combine(targetDirectory.FullName, "schema.xml")),
					new XAttribute("Description", string.Empty),
					new XAttribute("Flags", "1"),
					new XAttribute("DefPerm", "0")),
				new XElement("Roles",
					new XElement("Properties",
						new XAttribute("Value", "Pn0102{}")))));
			//save and overite th file
			applicationXml.Save(AppSettings.Get(this, "Folder.File.AplicationXml"));

			//REFRESH PANORAMA ADMIN CONNECTION BY OPENING THE FILE WITH PROCCES (THE PATH DEFINE ON APP.CONFIG)

			System.Diagnostics.Process Proc = new System.Diagnostics.Process();
			Proc.StartInfo.FileName = AppSettings.Get(this, "Folder.File.PanoramaMsc");
			

            try
            {
                Proc.Start();
                System.Threading.Thread.Sleep(4000);
                Proc.Kill();
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
				if (file.Name.ToLower() == "schema.xml" || file.Name.ToLower() == "properties.xml")
					continue;
				else
				{
					string fileString = string.Empty;
					using (StreamReader reader=new StreamReader(file.FullName))
					{
						fileString = reader.ReadToEnd();
						//channge CubeAdress,CubeName,CubeDB atributes	


						
					}
                    pattern = RegxUtils.CreateExactMatchWholeWordRegExpression(AppSettings.Get(this, "Panorama.ServerToReplace")); //replace server 
					//CubeAdress
					fileString = Regex.Replace(fileString, pattern, AppSettings.Get(this, "AnalysisServer.ConnectionString").Replace("DataSource=", string.Empty), RegexOptions.IgnoreCase);
					//CubeName //few options here

                    pattern = RegxUtils.CreateExactMatchWholeWordRegExpression(AppSettings.Get(this, "Panorama.ContentCubeToReplace"));//ContentCubeToReplace

                    fileString = Regex.Replace(fileString, pattern, AppSettings.GetAbsolute("EdgeBI.Wizards.StepExecuter.Cube.Content.Name.Perfix") + lastExecutorStepData["AccountSettings.CubeName"].ToString(), RegexOptions.IgnoreCase);

                    pattern = RegxUtils.CreateExactMatchWholeWordRegExpression( AppSettings.Get(this, "Panorama.BoCubeToReplace")); //BoCubeToReplace
                    fileString = Regex.Replace(fileString, pattern, AppSettings.GetAbsolute("EdgeBI.Wizards.StepExecuter.Cube.BO.Name.Perfix") + lastExecutorStepData["AccountSettings.CubeName"].ToString(), RegexOptions.IgnoreCase);


                    pattern = RegxUtils.CreateExactMatchWholeWordRegExpression(AppSettings.Get(this, "Panorama.CubeDbtoReplace"));//CubeDbtoReplace
					fileString = Regex.Replace(fileString, pattern, AppSettings.Get(this,"AnalysisServer.Database"), RegexOptions.IgnoreCase);
					
					//Replace client specific measures +new active users+new users

					foreach (KeyValuePair<string,object> input in lastExecutorStepData)
					{

                        if (input.Value is Replacment)
                        {
                            Replacment replacment = (Replacment)input.Value;
                            if (input.Key.StartsWith(AccSettClientSpecific, true, null))//measures
                            {
                                pattern = RegxUtils.CreateExactMatchWholeWordRegExpression( "BO " + replacment.ReplaceFrom);
                                fileString = Regex.Replace(fileString, pattern, replacment.ReplaceTo, RegexOptions.IgnoreCase);

                            }
                            else if (input.Key.StartsWith("AccountSettings.StringReplacment."))//String ReplaceMent
                            {
                                pattern = RegxUtils.CreateExactMatchWholeWordRegExpression( replacment.ReplaceFrom);
                                fileString = Regex.Replace(fileString, pattern, replacment.ReplaceTo, RegexOptions.IgnoreCase);

                            }
                            else if (input.Key == "AccountSettings.New Active Users")// replace new active users
                            {
                                if (input.Value.ToString() != " ")
                                {

                                    pattern = @"\bActives\b";
                                    fileString = Regex.Replace(fileString, pattern, replacment.ReplaceTo, RegexOptions.IgnoreCase);

                                }

                            }
                            else if (input.Key == "AccountSettings.New Users") //new users Regs!!!!
                            {
                                if (input.Value.ToString() != " ")
                                {
                                    pattern = @"\bRegs\b";
                                    fileString = Regex.Replace(fileString, pattern, replacment.ReplaceTo, RegexOptions.IgnoreCase);
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
				if  ( file.Name.Equals("2. ROI by Actives.xml") && !string.IsNullOrEmpty(newUsersChanged)) //regs				
					file.CopyTo(Path.Combine(targetDirectory.ToString(),string.Format("2. ROI by {0}.xml",newUsersChanged)), true);

                else if (file.Name.Equals("2. ROI by Regs.xml") && !string.IsNullOrEmpty(newActiverUsersChanged)) //actives				
                    file.CopyTo(Path.Combine(targetDirectory.ToString(), string.Format("2. ROI by {0}.xml", newActiverUsersChanged)), true);
                else
                {
                    if (copyContent) //if we create content cube then copy content files ,else...not
                    {
                        file.CopyTo(Path.Combine(targetDirectory.ToString(), file.Name), true);
                    }
                    else
                    {
                        string[] contentFiles = AppSettings.Get(this, "Panorama.ContentViews").Split(',');
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
