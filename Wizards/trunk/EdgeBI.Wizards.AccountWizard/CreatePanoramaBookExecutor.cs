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
			this.ReportProgress(0.9f);

			return base.DoWork();
		}

		private void AddNewBook()
		{
			Dictionary<string, object> lastExecutorStepData = GetExecutorData("CreateNewCubeExecutor");

			// COPY THE TEMPLATE BOOK (PATH ON APP.CONFIG FOLDER AND RENAME IT'S NAME TO BO+NEW ACCOUNT NAME
			DirectoryInfo templateSourceDirectory = new DirectoryInfo(Path.Combine(AppSettings.Get(this, "Folder.PanoramaBooks"), AppSettings.Get(this, "Folder.PanoramaBookTemplate"))); //source path
			DirectoryInfo targetDirectory = new DirectoryInfo(Path.Combine(AppSettings.Get(this, "Folder.PanoramaBooks"), lastExecutorStepData["AccountSettings.CubeName"].ToString()));  //target path

			// Check if the target directory exists, if not, create it.
			if (!Directory.Exists(targetDirectory.FullName))
			{
				try
				{
					Directory.CreateDirectory(targetDirectory.FullName);
					string newUsersChanged = string.Empty;
					string newActiveUsersChanged = string.Empty;
					if (lastExecutorStepData.ContainsKey("AccountSettings.New Users"))
						newUsersChanged = lastExecutorStepData["AccountSettings.New Users"].ToString();
					if (lastExecutorStepData.ContainsKey("AccountSettings.New Active Users"))
						newActiveUsersChanged = lastExecutorStepData["AccountSettings.New Active Users"].ToString();

					CopyAllFilesAndFolders(templateSourceDirectory, targetDirectory,newUsersChanged,newActiveUsersChanged);					

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
			int numberOfItemElements = applicationElement.Elements().Count() - 1;//since the first element is not an item element but Properties element
			//So the next item element to be add is numberOfItemElements +1

			applicationElement.Add(new XElement(string.Format("Item{0}", numberOfItemElements),
				new XElement("Properties",
					new XAttribute("Name", lastExecutorStepData["AccountSettings.CubeName"].ToString()),
					new XAttribute("Path", Path.Combine(targetDirectory.FullName, "schema.xml")),
					new XAttribute("Description", string.Empty),
					new XAttribute("Flags", "3"),
					new XAttribute("DefPerm", "0")),
				new XElement("Roles",
					new XElement("Properties",
						new XAttribute("Value", "Pn0102{}")))));
			//save and overite th file
			applicationXml.Save(AppSettings.Get(this, "Folder.File.AplicationXml"));

			//REFRESH PANORAMA ADMIN CONNECTION BY OPENING THE FILE WITH PROCCES (THE PATH DEFINE ON APP.CONFIG)

			System.Diagnostics.Process Proc = new System.Diagnostics.Process();
			Proc.StartInfo.FileName = AppSettings.Get(this, "Folder.File.PanoramaMsc");
			Proc.Start();

            try
            {
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
					pattern = string.Format(@"\b{0}\b",AppSettings.Get(this,"Panorama.ServerToReplace")); //replace server 
					//CubeAdress
					fileString = Regex.Replace(fileString, pattern, AppSettings.Get(this, "AnalysisServer.ConnectionString").Replace("DataSource=", string.Empty), RegexOptions.IgnoreCase);
					//CubeName //few options here

                    pattern = string.Format(@"\b{0}\b", AppSettings.Get(this, "Panorama.ContentCubeToReplace"));//ContentCubeToReplace

					fileString = Regex.Replace(fileString, pattern, lastExecutorStepData["AccountSettings.CubeName"].ToString(), RegexOptions.IgnoreCase);

                    pattern = string.Format(@"\b{0}\b", AppSettings.Get(this, "Panorama.BoCubeToReplace")); //BoCubeToReplace
					fileString = Regex.Replace(fileString, pattern, lastExecutorStepData["AccountSettings.CubeName"].ToString(), RegexOptions.IgnoreCase);

<<<<<<< .mine

                    pattern = string.Format(@"\b{0}\b", AppSettings.Get(this, "Panorama.CubeDbtoReplace"));//CubeDbtoReplace
					fileString = Regex.Replace(fileString, pattern, AppSettings.Get(this,"AnalysisServer.Database"), RegexOptions.IgnoreCase);
=======
					//CubeDb
					pattern = @"\beasynet_UDM\b";
					fileString = Regex.Replace(fileString, pattern, AppSettings.Get(this,"AnalysisServer.Database"), RegexOptions.IgnoreCase);
>>>>>>> .r192
					
					//Replace client specific measures +new active users+new users

					foreach (KeyValuePair<string,object> input in lastExecutorStepData)
					{
<<<<<<< .mine
						if (input.Key.StartsWith(AccSettClientSpecific, true, null))//measures
=======
						if (input.Key.StartsWith(AccSettClientSpecific, true, null))
>>>>>>> .r192
						{
<<<<<<< .mine
                            pattern = string.Format(@"\b{0}\b", "BO " + input.Key.Replace("AccountSettings.", string.Empty));
                            fileString = Regex.Replace(fileString, pattern, input.Value.ToString(), RegexOptions.IgnoreCase);
=======
							string patern = string.Format(@"\b{0}\b","BO " + input.Key.Replace("AccountSettings.", string.Empty));
							fileString = Regex.Replace(fileString, patern, input.Value.ToString(), RegexOptions.IgnoreCase);
>>>>>>> .r192
							
						}                             
                        else if (input.Key.StartsWith("AccountSettings.StringReplacment."))//String ReplaceMent
                        {
                            pattern = string.Format(@"\b{0}\b", input.Key.Replace("AccountSettings.StringReplacment.", string.Empty));
                            fileString = Regex.Replace(fileString, pattern, input.Value.ToString(), RegexOptions.IgnoreCase);
                        else if (input.Key.StartsWith("AccountSettings.StringReplacment."))
                        {
                            string patern = string.Format(@"\b{0}\b", input.Key.Replace("AccountSettings.StringReplacment.", string.Empty));
                            fileString = Regex.Replace(fileString, patern, input.Value.ToString(), RegexOptions.IgnoreCase);

<<<<<<< .mine
                        }
                        else if (input.Key == "AccountSettings.New Active Users")// replace new active users
                        {
                            if (input.Value.ToString() != " ")
                            {
=======
                        }

						
					}
					// replace new active users
                    if (lastExecutorStepData.ContainsKey("AccountSettings.New Active Users"))
                    {
                        if (lastExecutorStepData["AccountSettings.New Active Users"].ToString() != " ")
                        {
                            pattern = "\b% of activations\b";
                            fileString = Regex.Replace(fileString, pattern, lastExecutorStepData["AccountSettings.New Active Users"].ToString(), RegexOptions.IgnoreCase);
>>>>>>> .r192

<<<<<<< .mine
                                pattern = @"\bActives\b";
                                fileString = Regex.Replace(fileString, pattern, input.Value.ToString(), RegexOptions.IgnoreCase);

                            }
=======

                            pattern = "\bBO new activations\b";
                            fileString = Regex.Replace(fileString, pattern, lastExecutorStepData["AccountSettings.New Active Users"].ToString(), RegexOptions.IgnoreCase);

                            pattern = "\bTarget activations\b";
                            fileString = Regex.Replace(fileString, pattern, lastExecutorStepData["AccountSettings.New Active Users"].ToString(), RegexOptions.IgnoreCase);


                            pattern = "\bActives\b";
                            fileString = Regex.Replace(fileString, pattern, lastExecutorStepData["AccountSettings.New Active Users"].ToString(), RegexOptions.IgnoreCase);

                        } 
                    }
					
					//replace new users
                    if (lastExecutorStepData.ContainsKey("AccountSettings.New Active Users"))
                    {
                        if (lastExecutorStepData["AccountSettings.New Active Users"].ToString() != " ")
                        {
                            pattern = "\bBO New users\b";
                            fileString = Regex.Replace(fileString, pattern, lastExecutorStepData["AccountSettings.New Users"].ToString(), RegexOptions.IgnoreCase);
>>>>>>> .r192

<<<<<<< .mine
                        }
                        else if(input.Key=="AccountSettings.New Users") //new users Regs!!!!
                        {
                            if (input.Value.ToString() != " ")
                            {
                                pattern = @"\bRegs\b";
                                fileString = Regex.Replace(fileString, pattern, input.Value.ToString(), RegexOptions.IgnoreCase);
                            } 

                        }						
					}				

=======
                            pattern = "\bRegs\b";
                            fileString = Regex.Replace(fileString, pattern, lastExecutorStepData["AccountSettings.New Users"].ToString(), RegexOptions.IgnoreCase);
                        } 
                    }

>>>>>>> .r192
					using (StreamWriter writer=new StreamWriter(file.FullName,false))
					{
						writer.Write(fileString);
						
					}
				}
				
			}
		}

		private void CopyAllFilesAndFolders(DirectoryInfo templateSourceDirectory, DirectoryInfo targetDirectory,string newUsersChanged,string newActiverUsersChanged )
		{
			foreach (FileInfo file in templateSourceDirectory.GetFiles() )
			{
				if  ( file.Name.Equals("2. ROI by Actives.xml") && !string.IsNullOrEmpty(newUsersChanged)) //regs				
					file.CopyTo(Path.Combine(targetDirectory.ToString(),string.Format("2. ROI by {0}.xml",newUsersChanged)), true);	
			
				else if (file.Name.Equals("2. ROI by Regs.xml") && !string.IsNullOrEmpty(newActiverUsersChanged)) //actives				
					file.CopyTo(Path.Combine(targetDirectory.ToString(),string.Format("2. ROI by {0}.xml",newActiverUsersChanged)), true);
					
				else
					file.CopyTo(Path.Combine(targetDirectory.ToString(), file.Name), true);
				
			}
			foreach (DirectoryInfo sourceSubDirectory in templateSourceDirectory.GetDirectories())
			{
				DirectoryInfo nextTargetDirectory = targetDirectory.CreateSubdirectory(sourceSubDirectory.Name);
				CopyAllFilesAndFolders(sourceSubDirectory, nextTargetDirectory, newUsersChanged, newActiverUsersChanged);
				
			}




		}


		
	}
}
