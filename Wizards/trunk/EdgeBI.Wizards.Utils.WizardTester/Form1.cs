﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using Easynet.Edge.Core.Configuration;
using System.Runtime.Serialization;
using EdgeBI.Wizards;

using System.DirectoryServices;
using Microsoft.AnalysisServices;
using System.Xml.Linq;
using Easynet.Edge.Core.Utilities;
namespace EdgeBI.Wizards.Utils.WizardTester
{
	public partial class frmTestWizard : Form
	{
		const string baseXml = "BaseRequestXml.xml";
		private const string AccSettClientSpecific = "AccountSettings.Client Specific";
		private const string AccSettNewUser = "AccountSettings.New Users";
		private const string AccSettNewActiveUser = "AccountSettings.New Active Users";
		int? _sessionID = null;
		string _nextStepName;

		Uri _baseUri = new Uri("http://localhost:8080/wizard");

		public frmTestWizard()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			

		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			try
			{
				if (string.IsNullOrEmpty(txtWizardName.Text))
				{
					MessageBox.Show("Please fill wizard number");

				}
				else
				{

					WizardSession wizardSession = StartWizard(txtWizardName.Text);
					txtResult.Text = string.Format("Account Wizard Service Started:\nWizard ID is:{0}\nSessionID IS:{1}\nNext step name is:{2}\n--------------------", wizardSession.WizardID, wizardSession.SessionID, wizardSession.CurrentStep.StepName);

					gvKeyValue.Enabled = true;
					btnNextStep.Enabled = true;
					btnExecute.Enabled = false;
					btnGetSummary.Enabled = false;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private WizardSession StartWizard(string wizardName)
		{
			WizardSession wizardSession;
			try
			{

				WebRequest request = HttpWebRequest.Create(string.Format(_baseUri + "/start?wizardName={0}", wizardName));
				WebResponse response = request.GetResponse();

				DataContractSerializer serializer = new DataContractSerializer(typeof(WizardSession));
				using (XmlReader reader = XmlReader.Create(response.GetResponseStream()))
				{
					wizardSession = (WizardSession)serializer.ReadObject(reader, false);

				}
				_sessionID = wizardSession.SessionID;
				_nextStepName = wizardSession.CurrentStep.StepName;
				txtStepName.Text = _nextStepName;




			}
			catch (Exception)
			{
				throw;

			}
			return wizardSession;

		}

		private string GetXmlValue(string xmlNodeName, XmlDocument doc)
		{
			try
			{
				return doc.DocumentElement[xmlNodeName].InnerText;

			}
			catch (Exception)
			{

				throw;
			}
		}

		private XmlDocument GetXmlResponse(WebResponse response)
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				{
					doc.LoadXml(reader.ReadToEnd());
				}
			}
			catch (Exception)
			{

				throw;
			}
			return doc;
		}

		private void btnNextStep_Click(object sender, EventArgs e)
		{


			DialogResult result = System.Windows.Forms.DialogResult.Yes;
			WebRequest request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", _baseUri, _sessionID));
			request.ContentType = "application/xml";
			request.Method = "POST";
			request.Timeout = 130000;
			try
			{
				SetBodyForCollectRequest(ref request);
			}
			catch (Exception ex)
			{

				result = MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.YesNo);
			}
			if (result == System.Windows.Forms.DialogResult.Yes)
			{
				StepCollectResponse stepCollectResponse;
				try
				{
					try
					{
						WebResponse response = request.GetResponse();
						DataContractSerializer serializer = new DataContractSerializer(typeof(StepCollectResponse));
						using (XmlReader reader = XmlReader.Create(response.GetResponseStream()))
						{
							stepCollectResponse = (StepCollectResponse)serializer.ReadObject(reader, false);

						}
						//XmlDocument doc = GetXmlResponse(response);
						//string value = GetXmlValue("Result", doc);

						switch (stepCollectResponse.Result)
						{
							case StepResult.Next:
								{

									txtResult.Text = string.Format("\n--------------------\nStep Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
									_nextStepName = stepCollectResponse.NextStep.StepName;
									txtStepName.Text = _nextStepName;

									break;
								}
							case StepResult.HasErrors:
								{
									foreach (KeyValuePair<string,string> error in stepCollectResponse.Errors)
									{
										txtResult.Text += string.Format("Errors:key {0} Value{1}\n", error.Key,error.Value);
									}
									

									break;
								}
							case StepResult.Done:
								{
									txtResult.Text = "\nFinsish collecting from all steps\nready to get summary or execute:";
									btnNextStep.Enabled = false;
									btnGetSummary.Enabled = true;
									btnExecute.Enabled = true;
									break;
								}
						}



					}
					catch (WebException ex)
					{

						using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
						{

							txtResult.Text = string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
						}

					}
				}
				catch (Exception ex)
				{

					txtResult.Text = ex.Message;
				}

			}

		}

		private void SetBodyForCollectRequest(ref WebRequest request)
		{
			StepCollectRequest stepCollectRequest = new StepCollectRequest();
			stepCollectRequest.StepName = _nextStepName;
			stepCollectRequest.CollectedValues = new Dictionary<string, object>();
			bool empty = true;
			//XmlDocument doc = new XmlDocument();
			//doc.Load(string.Format(Path.Combine(Application.StartupPath, baseXml)));

			//XmlNode parentNode = doc.DocumentElement["CollectedValues"];			
			foreach (DataGridViewRow row in gvKeyValue.Rows)
			{


				if (row.Cells[0].Value != null && row.Cells[1].Value != null)
				{
					empty = false;
					stepCollectRequest.CollectedValues.Add(row.Cells[0].Value.ToString(), row.Cells[1].Value.ToString());

				}



			}
			if (empty)
			{
				throw new Exception("No data on the grid, do you want to procceed?");

			}
			else
			{

				DataContractSerializer serializer = new DataContractSerializer(typeof(StepCollectRequest));
				using (XmlWriter writer = XmlWriter.Create(request.GetRequestStream()))
				{
					serializer.WriteObject(writer, stepCollectRequest);
					writer.Flush();
				}

			}

		}





		private void btnGetSummary_Click(object sender, EventArgs e)
		{
			try
			{
				WebRequest request = HttpWebRequest.Create(string.Format("{0}/summary?sessionID={1}", _baseUri, _sessionID));
				request.Method = "POST";
				request.ContentType = "application/xml";
				request.ContentLength = 0;
				WebResponse response = request.GetResponse();
				XmlDocument SummaryXml = new XmlDocument();
				using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				{

					SummaryXml.LoadXml(reader.ReadToEnd());
					txtResult.Text = SummaryXml.DocumentElement.InnerText;

				}
			}
			catch (Exception ex)
			{

				txtResult.Text = ex.Message;
			}

		}

		private void btnExecute_Click(object sender, EventArgs e)
		{
			try
			{
				WebRequest request = HttpWebRequest.Create(string.Format("{0}/execute?sessionID={1}", _baseUri, _sessionID));
				request.Method = "POST";
				request.ContentType = "application/xml";
				request.ContentLength = 0;
				WebResponse response = request.GetResponse();
				txtResult.Text = "EXECUTION FINISHED SUCCESSFULY";
			}
			catch (Exception ex)
			{

				txtResult.Text = ex.Message;
			}
			finally
			{
				btnExecute.Enabled = false;
				btnNextStep.Enabled = false;
				btnGetSummary.Enabled = false;
				btnStart.Enabled = true;
			}
		}

		private void btnTestAddUserActiveDirectory_Click(object sender, EventArgs e)
		{
			string guid = CreateUserAccount("LDAP://79.125.11.216/CN=Users,dc=edge,dc=bi", gvKeyValue.Rows[0].Cells[1].Value.ToString(), gvKeyValue.Rows[1].Cells[1].Value.ToString());
		}

		public string CreateUserAccount(string ldapPath, string userName, string userPassword)
		{
			string oGUID = string.Empty;
			try
			{

				string connectionPrefix =  ldapPath;
				DirectoryEntry dirEntry = new DirectoryEntry("LDAP://79.125.11.216/CN=Users,dc=edge,dc=bi", "administrator", "Edgebihas1fish");
				DirectoryEntry newUser = dirEntry.Children.Add
					("CN=" + userName, "user");
				newUser.Properties["samAccountName"].Value = userName;

				//newUser.Properties["userAccountControl"].Value = 512;
				newUser.CommitChanges();
				oGUID = newUser.Guid.ToString();

				//byte[] oPassword = System.Text.Encoding.Unicode.GetBytes(userPassword);
				//newUser.Properties["unicodePwd"].Value = oPassword;
				newUser.Invoke("SetPassword", new object[] { userPassword });
				newUser.CommitChanges();
				dirEntry.Close();
				newUser.Close();
			}
			catch (System.DirectoryServices.DirectoryServicesCOMException E)
			{
				//DoSomethingwith --> E.Message.ToString();

			}
			return oGUID;
		}

		private void btnLoadStepData_Click(object sender, EventArgs e)
		{
			switch (txtStepName.Text)
			{
				case "CreateRoleStepCollector":
					{
						List<KeyVal> list = new List<KeyVal>();
						KeyVal keyval = new KeyVal() { Key = "AccountSettings.RoleName", Value = "AlonForTestRole" };
						list.Add(keyval);
						keyval = new KeyVal() { Key = "AccountSettings.RoleID", Value = "AlonForTestRoleID" };
						list.Add(keyval);
						keyval = new KeyVal() { Key = "AccountSettings.RoleMemberName", Value = @"SEPERIA\alonya" };
						list.Add(keyval);
						gvKeyValue.DataSource = list;
						gvKeyValue.Refresh();
						break;
					}
				case "ActiveDirectoryStepCollector":
					{
						List<KeyVal> list = new List<KeyVal>();
						KeyVal keyval = new KeyVal() { Key = "ActiveDirectory.UserName", Value = "AlonForTest" };
						list.Add(keyval);
						keyval = new KeyVal() { Key = "ActiveDirectory.Password", Value = "Narnia2@" };
						list.Add(keyval);
						keyval = new KeyVal() { Key = "ActiveDirectory.FullName", Value = "Alon Yaari" };
						list.Add(keyval);
						gvKeyValue.DataSource = list;
						gvKeyValue.Refresh();
						break;
					}
				case "CreateNewCubeCollector":
					{
						List<KeyVal> list = new List<KeyVal>();
						KeyVal keyval;
						for (int i = 1; i < 33; i++)
						{
							keyval = new KeyVal() { Key = string.Format("AccountSettings.Client Specific{0}", i), Value = string.Format("newname{0}", i) };
							list.Add(keyval);

						}
						keyval = new KeyVal() { Key = "AccountSettings.New Active Users",Value= "replaced act us" };
						list.Add(keyval);
						keyval = new KeyVal() { Key = "AccountSettings.New Users", Value = "replaced ne us" };
						list.Add(keyval);
						keyval = new KeyVal() { Key = "AccountSettings.Scope_ID", Value = "6" };
						list.Add(keyval);
						gvKeyValue.DataSource = list;
						gvKeyValue.Refresh();
						
						break;
					}
			}

		}

		private void button1_Click(object sender, EventArgs e)
		{
			using (Server analysisServer = new Server())
			{


				analysisServer.Connect("DataSource=qa");
				
				//Get the database
				Database analysisDatabase = analysisServer.Databases.GetByName("easynet_UDM");

				//Create new cube
					
					//Get template cube

				Cube existingCube = analysisDatabase.Cubes["BOPPCCube1"];

				
				
				Cube newCube = existingCube.Clone();
				//change cube name and id
				newCube.Name = "BO123"; //BO ++++
				// Update scope id
				newCube.ID = "222";
				foreach (MeasureGroup measureGroup in newCube.MeasureGroups)
				{
					foreach (Partition partition in measureGroup.Partitions)
					{
						QueryBinding queryBinding = partition.Source as QueryBinding;
						
						
					}
					
				}
				foreach (MdxScript script in newCube.MdxScripts)
				{
					foreach (Command command in script.Commands)
					{
						command.Text.Replace("new user", "changed new user");
						
					}

					
				}

				analysisDatabase.Cubes.Add(newCube);
				






			}
		}

		private void btnCreateBook_Click(object sender, EventArgs e)
		{
			AddNewBook();
		}

		private void AddNewBook()
		{
			//Dictionary<string, object> lastExecutorStepData = GetExecutorData("CreateNewCubeExecutor"); cannot be done from here 
			
			
			List<KeyVal> list = new List<KeyVal>();//for test
			KeyVal keyval;
			for (int i = 1; i < 33; i++)
			{
				keyval = new KeyVal() { Key = string.Format("AccountSettings.Client Specific{0}", i), Value = string.Format("newname{0}", i) };
				list.Add(keyval);

			}
			keyval = new KeyVal() { Key = "AccountSettings.New Active Users", Value = "replaced act us" };
			list.Add(keyval);
			keyval = new KeyVal() { Key = "AccountSettings.New Users", Value = "replaced ne us" };
			list.Add(keyval);
			keyval = new KeyVal() { Key = "AccountSettings.Scope_ID", Value = "6" };
			list.Add(keyval);
			gvKeyValue.DataSource = list;
			gvKeyValue.Refresh();

			Dictionary<string, object> lastExecutorStepData = new Dictionary<string, object>();
			foreach (DataGridViewRow row in gvKeyValue.Rows)
			{


				if (row.Cells[0].Value != null && row.Cells[1].Value != null)
				{

					lastExecutorStepData.Add(row.Cells[0].Value.ToString(), row.Cells[1].Value.ToString());

				}



			}


			// COPY THE TEMPLATE BOOK (PATH ON APP.CONFIG FOLDER AND RENAME IT'S NAME TO BO+NEW ACCOUNT NAME
			DirectoryInfo templateSourceDirectory = new DirectoryInfo(Path.Combine(@"D:\TestPanorma","BO PPC VS BCK")); //source path
			DirectoryInfo targetDirectory = new DirectoryInfo(Path.Combine(@"D:\TestPanorma", "testCubeName"));  //target path

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

			XDocument applicationXml = XDocument.Load(@"D:\TestPanorma\Applications.xml");
			XElement applicationElement = applicationXml.Element("pnView").Element("Applications");
			int numberOfItemElements = applicationElement.Elements().Count() - 1;//since the first element is not an item element but Properties element
			//So the next item element to be add is numberOfItemElements +1

			applicationElement.Add(new XElement(string.Format("Item{0}", numberOfItemElements+1),
				new XElement("Properties",
					new XAttribute("Name", "testCubeName"),
					new XAttribute("Path", Path.Combine(targetDirectory.FullName, "schema.xml")),
					new XAttribute("Description", string.Empty),
					new XAttribute("Flags", "3"),
					new XAttribute("DefPerm", "0")),
				new XElement("Roles",
					new XElement("Properties",
						new XAttribute("Value", "Pn0102{}")))));
			//save and overite th file
			applicationXml.Save(@"D:\TestPanorma\Applications.xml");

			//REFRESH PANORAMA ADMIN CONNECTION BY OPENING THE FILE WITH PROCCES (THE PATH DEFINE ON APP.CONFIG)
		}
		private void CopyAllFilesAndFolders(DirectoryInfo templateSourceDirectory, DirectoryInfo targetDirectory, string newUsersChanged, string newActiverUsersChanged)
		{
			foreach (FileInfo file in templateSourceDirectory.GetFiles())
			{
				if (file.Name.Equals("2. ROI by Actives.xml") && !string.IsNullOrEmpty(newUsersChanged)) //regs				
					file.CopyTo(Path.Combine(targetDirectory.ToString(), string.Format("2. ROI by {0}.xml", newUsersChanged)), true);

				else if (file.Name.Equals("2. ROI by Regs.xml") && !string.IsNullOrEmpty(newActiverUsersChanged)) //actives				
					file.CopyTo(Path.Combine(targetDirectory.ToString(), string.Format("2. ROI by {0}.xml", newActiverUsersChanged)), true);

				else
					file.CopyTo(Path.Combine(targetDirectory.ToString(), file.Name), true);

			}
			foreach (DirectoryInfo sourceSubDirectory in templateSourceDirectory.GetDirectories())
			{
				DirectoryInfo nextTargetDirectory = targetDirectory.CreateSubdirectory(sourceSubDirectory.Name);
				CopyAllFilesAndFolders(sourceSubDirectory, nextTargetDirectory, newUsersChanged, newActiverUsersChanged);

			}




		}
		private void UpdateRelevantPropertiesOnXml(DirectoryInfo targetDirectory, Dictionary<string, object> lastExecutorStepData)
		{
			foreach (FileInfo file in targetDirectory.GetFiles())
			{
				if (file.Name.ToLower() == "schema.xml" || file.Name.ToLower() == "properties.xml")
					continue;
				else
				{
					string fileString = string.Empty;
					using (StreamReader reader = new StreamReader(file.FullName))
					{
						fileString = reader.ReadToEnd();
						//channge CubeAdress,CubeName,CubeDB atributes	



					}
					string pattern = @"\bhal\b"; //replace server hal
					//CubeAdress
					fileString = Regex.Replace(fileString, pattern,"connectionstringreplaced", RegexOptions.IgnoreCase);
					//CubeName //few options here
					pattern = @"\bEasyNet-New Cube\b";
					fileString = Regex.Replace(fileString, pattern, "NewCubeName", RegexOptions.IgnoreCase);
					pattern = @"\bContent Cube\b";
					fileString = Regex.Replace(fileString, pattern, "NewCubeName", RegexOptions.IgnoreCase);
					pattern = @"\bBO PPC Cube\b";
					fileString = Regex.Replace(fileString, pattern, "NewCubeName", RegexOptions.IgnoreCase);

					//CubeDb
					pattern = @"\beasynet_UDM\b";
					fileString = Regex.Replace(fileString, pattern,"Some DataBase", RegexOptions.IgnoreCase);

					//Replace client specific measures +new active users+new users

					foreach (KeyValuePair<string, object> input in lastExecutorStepData)
					{
						if (input.Key.StartsWith(AccSettClientSpecific, true, null) ||
							input.Key.ToUpper() == AccSettNewUser.ToUpper() ||
							input.Key.ToUpper() == AccSettNewActiveUser.ToUpper())
						{
							string patern = string.Format(@"\b{0}\b", input.Key.Replace("AccountSettings.", string.Empty));
							fileString = Regex.Replace(fileString, patern, input.Value.ToString(), RegexOptions.IgnoreCase);

						}


					}
					// replace new active users
					pattern = "\b% of activations\b";
					fileString = Regex.Replace(fileString, pattern, lastExecutorStepData["AccountSettings.New Active Users"].ToString(), RegexOptions.IgnoreCase);


					pattern = "\bBO new activations\b";
					fileString = Regex.Replace(fileString, pattern, lastExecutorStepData["AccountSettings.New Active Users"].ToString(), RegexOptions.IgnoreCase);

					pattern = "\bTarget activations\b";
					fileString = Regex.Replace(fileString, pattern, lastExecutorStepData["AccountSettings.New Active Users"].ToString(), RegexOptions.IgnoreCase);

					//replace new users
					pattern = "\bBO New users\b";
					fileString = Regex.Replace(fileString, pattern, lastExecutorStepData["AccountSettings.New Users"].ToString(), RegexOptions.IgnoreCase);

					using (StreamWriter writer = new StreamWriter(file.FullName, false))
					{
						writer.Write(fileString);

					}
				}

			}
		}


	}
	public class KeyVal
	{
		public string Key { get; set; }
		public string Value { get; set; }
	}


}
