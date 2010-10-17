using System;
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
using System.Runtime.Serialization;
using EdgeBI.Wizards;

using System.DirectoryServices;
namespace EdgeBI.Wizards.Utils.WizardTester
{
	public partial class frmTestWizard : Form
	{
		const string baseXml = "BaseRequestXml.xml";
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
						KeyVal keyval = new KeyVal() { Key = "RoleName", Value = "AlonForTest" };
						list.Add(keyval);
						keyval = new KeyVal() { Key = "RoleID", Value = "Narnia2@" };
						list.Add(keyval);
						keyval = new KeyVal() { Key = "RoleMemberName", Value = "Alon Yaari" };
						list.Add(keyval);
						gvKeyValue.DataSource = list;
						gvKeyValue.Refresh();
						break;
					}
				case "ActiveDirectoryStepCollector":
					{
						List<KeyVal> list = new List<KeyVal>();
						KeyVal keyval = new KeyVal() { Key = "UserName", Value = "AlonForTest" };
						list.Add(keyval);
						keyval = new KeyVal() { Key = "Password", Value = "Narnia2@" };
						list.Add(keyval);
						keyval = new KeyVal() { Key = "FullName", Value = "Alon Yaari" };
						list.Add(keyval);
						gvKeyValue.DataSource = list;
						gvKeyValue.Refresh();
						break;
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
