using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Easynet.Edge.Applications;
using System.Reflection;
using System.Threading;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Services;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;

namespace EdgeBI.Wizards.Utils.WizardTester
{
	public partial class frmWizardTester : Form
	{
		int? _sessionID = null;
		string _nextStepName;
		private Process edgeHostService;
		Uri _baseUri = new Uri("http://localhost:8080/wizard");
		Dictionary<string, object> _measures = new Dictionary<string, object>();
		Dictionary<string, object> _strings = new Dictionary<string, object>();
		FrmBookStringValuesReplace _bsvr;
		public frmWizardTester()
		{
			InitializeComponent();

			#if RELEASE
			edgeHostService = new Process();
			edgeHostService.StartInfo.FileName = Properties.Settings.Default.ServiceHostPath;
			edgeHostService.StartInfo.Arguments = "/WizardService";
			edgeHostService.Start();
			#endif
		}

		private void frmWizardTester_Load(object sender, EventArgs e)
		{


			txtDataSourceName.Text = "From app.config of the service"; //Properties.Settings.Default.AnalysisServerConnectionString;
			txtDatabseName.Text = "From app.config of the service"; //Properties.Settings.Default.AnalysisServerDatabase;
			txtRoleMemberName.Text = Properties.Settings.Default.RoleMemberName;
			//txtCubeName.Text = Properties.Settings.Default.CubeNameStartWith;
			txtCubeTemplateID.Text = "From app.config of the service"; //Properties.Settings.Default.CubeTemplatesBOTemplate;
			txtCPA1.Text = Properties.Settings.Default.CPA1;
			txtCPA2.Text = Properties.Settings.Default.CPA2;
			//txtBookName.Text = Properties.Settings.Default.BookName;
			txtApplicationFilePathForAddingBook.Text = "From app.config of the service";//Properties.Settings.Default.ApplicationsXmlPath;
			txtPanoramaAdminRefreshFilePath.Text = "From app.config of the service";// Properties.Settings.Default.PanoramaExecutionPath;
			txtCubeAddressInViewFile.Text = "From app.config of the service"; //Properties.Settings.Default.AnalysisServerConnectionString;
			txtSSISCubeTemplate.Text = "From app.config of the service"; //Properties.Settings.Default.DtsxTemplatePath;
			txtSSISAllCubeTemplate.Text = "From app.config of the service";// Properties.Settings.Default.DtsxPath;
			txtBooksXmlTemplatePath.Text = "From app.config of the service";// Properties.Settings.Default.BookTemplatePath;
			txtCubeNamePatternToReplace.Text = Properties.Settings.Default.CubeNamePattenToReplace;
			txtDatabasePatternToReplace.Text = Properties.Settings.Default.DatabaseNamePattenToReplace;
			txtDatabaseToReplace.Text = Properties.Settings.Default.AnalysisServerDatabase;
			//txtReplaceWithCubeName.Text = Properties.Settings.Default.CubeNameStartWith;

			txtLog.Text += "service started";
			txtLog.Text += "------------------------------";









		}

		void instance_StateChanged(object sender, ServiceStateChangedEventArgs e)
		{
			ServiceInstance instance = (ServiceInstance)sender;
			if (e.StateAfter == ServiceState.Ready)
			{
				txtLog.Text += string.Format("Starting {0}\n", instance.Configuration.Name);
				txtLog.Text += "------------------------------";
				try
				{
					instance.Start();
				}
				catch (Exception ex)
				{
					txtLog.Text += "------------------------------";
					txtLog.Text += string.Format("Could not start {0}: {1} ({2})",
						instance.Configuration.Name,
						ex.Message,
						ex.GetType().FullName);
					txtLog.Text += "------------------------------";
					txtLog.Text += string.Format("See the event log for further details.");

					Log.Write(String.Format("Could not start {0}.", instance.Configuration.Name), ex);
				}
			}
			else if (e.StateAfter == ServiceState.Ended)
			{
				txtLog.Text += "------------------------------";
				txtLog.Text += string.Format("{0} has ended.", instance.Configuration.Name);
			}
		}

		private void txtScopName_Leave(object sender, EventArgs e)
		{
			txtCubeName.Text = txtScopName.Text;
			txtBookName.Text = txtScopName.Text;
			txtReplaceWithCubeName.Text = txtScopName.Text;


		

		}

		private void frmWizardTester_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				edgeHostService.Kill();
			}
			catch (Exception)
			{
				
				
			}
			
		}

		private void btnStartNewSession_Click(object sender, EventArgs e)
		{
			foreach (Control control in this.Controls)
			{
				if (control.GetType() == typeof(GroupBox))
				{
					control.Enabled = false;
				}
			}
			if (!string.IsNullOrEmpty(txtScopName.Text) && !string.IsNullOrEmpty(txtScopeID.Text))
			{
				WizardSession wizardSession = StartWizard("AccountWizard");
				
				txtLog.Text = string.Format("Account Wizard Service Started:\nWizard ID is:{0}\nSessionID IS:{1}\nNext step name is:{2}\n--------------------", wizardSession.WizardID, wizardSession.SessionID, wizardSession.CurrentStep.StepName);
				grpBoxActiveDirectory.Enabled = true;
			}
			else
			{
				MessageBox.Show("Please fill scopeid and scopename");
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




			}
			catch (Exception)
			{
				throw;

			}
			return wizardSession;

		}

		private void btnActiveDirectory_Click(object sender, EventArgs e)
		{
			DialogResult result = System.Windows.Forms.DialogResult.Yes;
			WebRequest request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", _baseUri, _sessionID));
			request.ContentType = "application/xml";
			request.Method = "POST";
			request.Timeout = 130000;

			//Collect values
			StepCollectRequest stepCollectRequest = new StepCollectRequest();
			stepCollectRequest.StepName = _nextStepName;
			stepCollectRequest.CollectedValues = new Dictionary<string, object>();
			Dictionary<string, object> collectedValues = new Dictionary<string, object>();
			collectedValues.Add("ActiveDirectory.UserName", txtUserName.Text.Trim());
			collectedValues.Add("ActiveDirectory.Password", txtPassword.Text.Trim());
			collectedValues.Add("ActiveDirectory.FullName", txtFullName.Text.Trim());
			stepCollectRequest.CollectedValues = collectedValues;


			try
			{
				SetBodyForCollectRequest(ref request, stepCollectRequest);
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


						switch (stepCollectResponse.Result)
						{
							case StepResult.Next:
								{

									txtLog.Text += string.Format("\n--------------------\nStep Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
									_nextStepName = stepCollectResponse.NextStep.StepName;
									grpRole.Enabled = true;


									break;
								}
							case StepResult.HasErrors:
								{
									foreach (KeyValuePair<string, string> error in stepCollectResponse.Errors)
									{
										txtLog.Text += string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value);
									}


									break;
								}
							case StepResult.Done:
								{
									txtLog.Text = "\nFinsish collecting from all steps\nready to get summary or execute:";

									break;
								}
						}



					}
					catch (WebException ex)
					{

						using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
						{

							txtLog.Text += string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
						}

					}
				}
				catch (Exception ex)
				{

					txtLog.Text += ex.Message;
				}

			}
		}

		private void SetBodyForCollectRequest(ref WebRequest request, StepCollectRequest stepCollectRequest)
		{
			DataContractSerializer serializer = new DataContractSerializer(typeof(StepCollectRequest));
			using (XmlWriter writer = XmlWriter.Create(request.GetRequestStream()))
			{
				serializer.WriteObject(writer, stepCollectRequest);
				writer.Flush();
			}
		}

		private void btnCreateRole_Click(object sender, EventArgs e)
		{
			DialogResult result = System.Windows.Forms.DialogResult.Yes;
			WebRequest request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", _baseUri, _sessionID));
			request.ContentType = "application/xml";
			request.Method = "POST";
			request.Timeout = 130000;

			//Collect values
			StepCollectRequest stepCollectRequest = new StepCollectRequest();
			stepCollectRequest.StepName = _nextStepName;
			stepCollectRequest.CollectedValues = new Dictionary<string, object>();
			Dictionary<string, object> collectedValues = new Dictionary<string, object>();
			collectedValues.Add("AccountSettings.RoleName", txtRoleName.Text.Trim());
			collectedValues.Add("AccountSettings.RoleID", txtRoleID.Text.Trim());
			collectedValues.Add("AccountSettings.RoleMemberName", txtRoleMemberName.Text.Trim());
			stepCollectRequest.CollectedValues = collectedValues;


			try
			{
				SetBodyForCollectRequest(ref request, stepCollectRequest);
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


						switch (stepCollectResponse.Result)
						{
							case StepResult.Next:
								{

									txtLog.Text += string.Format("\n--------------------\nStep Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
									_nextStepName = stepCollectResponse.NextStep.StepName;
									grpCube.Enabled = true;


									break;
								}
							case StepResult.HasErrors:
								{
									foreach (KeyValuePair<string, string> error in stepCollectResponse.Errors)
									{
										txtLog.Text += string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value);
									}


									break;
								}
							case StepResult.Done:
								{
									txtLog.Text = "\nFinsish collecting from all steps\nready to get summary or execute:";

									break;
								}
						}



					}
					catch (WebException ex)
					{

						using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
						{

							txtLog.Text += string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
						}

					}
				}
				catch (Exception ex)
				{

					txtLog.Text += ex.Message;
				}

			}
		}

		private void btnCreateNewCube_Click(object sender, EventArgs e)
		{
			DialogResult result = System.Windows.Forms.DialogResult.Yes;
			WebRequest request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", _baseUri, _sessionID));
			request.ContentType = "application/xml";
			request.Method = "POST";
			request.Timeout = 130000;

			//Collect values
			StepCollectRequest stepCollectRequest = new StepCollectRequest();
			stepCollectRequest.StepName = _nextStepName;
			stepCollectRequest.CollectedValues = new Dictionary<string, object>();
			Dictionary<string, object> collectedValues = new Dictionary<string, object>();
			collectedValues.Add("AccountSettings.New Users", ((KeyValuePair<string,object>)cmbReplaceCPA1.SelectedItem).Value);
			collectedValues.Add("AccountSettings.New Active Users", ((KeyValuePair<string, object>)cmbReplaceCPA2.SelectedItem).Value);
			collectedValues.Add("AccountSettings.Scope_ID", txtScopeID.Text.Trim());
			collectedValues.Add("AccountSettings.CubeName", txtCubeName.Text.Trim());
			collectedValues.Add("AccountSettings.CubeID", txtCubeName.Text.Trim());
			//add the measures to replace

			foreach (KeyValuePair<string, object> measure in _measures)
			{
				if (collectedValues.ContainsKey(measure.Key))
					collectedValues.Remove(measure.Key);
				collectedValues.Add(measure.Key, measure.Value);
			}
			//add the string values to replace
			foreach (KeyValuePair<string, object> str in _strings)
			{
				if (collectedValues.ContainsKey(str.Key))
					collectedValues.Remove(str.Key);
				collectedValues.Add(str.Key, str.Value);

			}




			if (chkContent.Checked)
				collectedValues.Add("AccountSettings.AddContentCube", true);
			else
				collectedValues.Add("AccountSettings.AddContentCube", false);
			stepCollectRequest.CollectedValues = collectedValues;



			try
			{
				SetBodyForCollectRequest(ref request, stepCollectRequest);
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


						switch (stepCollectResponse.Result)
						{
							case StepResult.Next:
								{

									txtLog.Text += string.Format("\n--------------------\nStep Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
									_nextStepName = stepCollectResponse.NextStep.StepName;
									grpBook.Enabled = true;


									break;
								}
							case StepResult.HasErrors:
								{
									foreach (KeyValuePair<string, string> error in stepCollectResponse.Errors)
									{
										txtLog.Text += string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value);
									}


									break;
								}
							case StepResult.Done:
								{
									txtLog.Text = "\nFinsish collecting from all steps\nready to get summary or execute:";

									break;
								}
						}



					}
					catch (WebException ex)
					{

						using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
						{

							txtLog.Text += string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
						}

					}
				}
				catch (Exception ex)
				{

					txtLog.Text += ex.Message;
				}

			}

		}

		private void button4_Click(object sender, EventArgs e)
		{
			FrmMeasuresList frmMeasuresList = new FrmMeasuresList(_measures);
			frmMeasuresList.ShowDialog();
			_measures = frmMeasuresList.Measures;
			_measures.Add("0", "0");


			cmbReplaceCPA1.Items.Clear();
			cmbReplaceCPA2.Items.Clear();
			cmbReplaceCPA1.DataSource = _measures.ToList();
			cmbReplaceCPA2.DataSource = _measures.ToList();
			cmbReplaceCPA1.ValueMember = "Key";
			cmbReplaceCPA1.DisplayMember = "Value";
			cmbReplaceCPA2.ValueMember = "Key";
			cmbReplaceCPA2.DisplayMember = "Value";
			cmbReplaceCPA1.SelectedValue = "0";
			cmbReplaceCPA2.SelectedValue = "0";
			
			//foreach (KeyValuePair<string, object> measure in _measures)
			//{
				
			//    cmbReplaceCPA1.Items.Add(measure);
			//    cmbReplaceCPA2.Items.Add(measure);
			//    cmbReplaceCPA1.ValueMember = measure.Key;
			//    cmbReplaceCPA1.DisplayMember = measure.Value.ToString();
			//    cmbReplaceCPA2.ValueMember = measure.Key;
			//    cmbReplaceCPA2.DisplayMember = measure.Value.ToString();
			//}


			//cmbReplaceCPA1.Items.Insert(0, " ");
			//cmbReplaceCPA2.Items.Insert(0, " ");
			//cmbReplaceCPA1.SelectedItem = cmbReplaceCPA1.Items[0];
			//cmbReplaceCPA2.SelectedItem = cmbReplaceCPA2.Items[0];
		}

		private void button8_Click(object sender, EventArgs e)
		{
			if (_bsvr == null)
				_bsvr = new FrmBookStringValuesReplace(_strings);
			_bsvr.ShowDialog();
			_strings = _bsvr.Strings;
		}

		private void button7_Click(object sender, EventArgs e)
		{
			if (_bsvr == null)
				_bsvr = new FrmBookStringValuesReplace(_strings);
			_bsvr.ShowDialog();
			_strings = _bsvr.Strings;
		}

		private void txtScopeID_Leave(object sender, EventArgs e)
		{

		}

		private void button3_Click(object sender, EventArgs e)
		{
			DialogResult result = System.Windows.Forms.DialogResult.Yes;
			WebRequest request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", _baseUri, _sessionID));
			request.ContentType = "application/xml";
			request.Method = "POST";
			request.Timeout = 130000;

			//Collect values
			StepCollectRequest stepCollectRequest = new StepCollectRequest();
			stepCollectRequest.StepName = _nextStepName;
			stepCollectRequest.CollectedValues = new Dictionary<string, object>();
			Dictionary<string, object> collectedValues = new Dictionary<string, object>();


			//add the string values to replace
			foreach (KeyValuePair<string, object> str in _strings)
			{
				if (collectedValues.ContainsKey(str.Key))
					collectedValues.Remove(str.Key);
				collectedValues.Add(str.Key, str.Value);

			}







			try
			{
				SetBodyForCollectRequest(ref request, stepCollectRequest);
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


						switch (stepCollectResponse.Result)
						{
							case StepResult.Next:
								{

									txtLog.Text += string.Format("\n--------------------\nStep Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
									_nextStepName = stepCollectResponse.NextStep.StepName;
									grpSSIS.Enabled = true;


									break;
								}
							case StepResult.HasErrors:
								{
									foreach (KeyValuePair<string, string> error in stepCollectResponse.Errors)
									{
										txtLog.Text += string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value);
									}


									break;
								}
							case StepResult.Done:
								{
									txtLog.Text = "\nFinsish collecting from all steps\nready to get summary or execute:";

									break;
								}
						}



					}
					catch (WebException ex)
					{

						using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
						{

							txtLog.Text += string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
						}

					}
				}
				catch (Exception ex)
				{

					txtLog.Text += ex.Message;
				}

			}
		}

		private void btnExecute_Click(object sender, EventArgs e)
		{
			foreach (Control control in this.Controls)
			{
				if (control.GetType() == typeof(GroupBox))
				{
					control.Enabled = false;
				}
			}
			try
			{
				WebRequest request = HttpWebRequest.Create(string.Format("{0}/execute?sessionID={1}", _baseUri, _sessionID));
				request.Method = "POST";
				request.ContentType = "application/xml";
				request.ContentLength = 0;
				WebResponse response = request.GetResponse();
				txtLog.Text += "------------------------------";
				txtLog.Text+= "EXECUTION FINISHED SUCCESSFULY";
			}
			catch (Exception ex)
			{
				txtLog.Text += "------------------------------";
				txtLog.Text+=ex.Message;
			}
			finally
			{
				
			}
		}

		private void button5_Click(object sender, EventArgs e)
		{
			//AccountSettings.CubeName
			DialogResult result = System.Windows.Forms.DialogResult.Yes;
			WebRequest request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", _baseUri, _sessionID));
			request.ContentType = "application/xml";
			request.Method = "POST";
			request.Timeout = 130000;

			//Collect values
			StepCollectRequest stepCollectRequest = new StepCollectRequest();
			stepCollectRequest.StepName = _nextStepName;
			stepCollectRequest.CollectedValues = new Dictionary<string, object>();
			Dictionary<string, object> collectedValues = new Dictionary<string, object>();
			collectedValues.Add("AccountSettings.CubeName", txtReplaceWithCubeName.Text);
			
			stepCollectRequest.CollectedValues = collectedValues;


			try
			{
				SetBodyForCollectRequest(ref request, stepCollectRequest);
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


						switch (stepCollectResponse.Result)
						{
							case StepResult.Next:
								{

									txtLog.Text += string.Format("\n--------------------\nStep Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
									_nextStepName = stepCollectResponse.NextStep.StepName;
									btnExecute.Enabled = true;


									break;
								}
							case StepResult.HasErrors:
								{
									foreach (KeyValuePair<string, string> error in stepCollectResponse.Errors)
									{
										txtLog.Text += "------------------------------";
										txtLog.Text += string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value);
									}


									break;
								}
							case StepResult.Done:
								{
									txtLog.Text += "------------------------------";
									txtLog.Text += "\nFinsish collecting from all steps\nready to get summary or execute:";

									break;
								}
						}



					}
					catch (WebException ex)
					{

						using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
						{
							txtLog.Text += "------------------------------";
							txtLog.Text += string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
						}

					}
				}
				catch (Exception ex)
				{
					txtLog.Text += "------------------------------";
					txtLog.Text += ex.Message;
				}

			}
		}













	}
}
