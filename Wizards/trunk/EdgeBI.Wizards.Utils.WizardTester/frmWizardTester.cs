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

using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;


namespace EdgeBI.Wizards.Utils.WizardTester
{
<<<<<<< .mine
    public partial class frmWizardTester : Form
    {
        int? _sessionID = null;
        string _nextStepName;
        private Process edgeHostService;
        Uri _baseUri = new Uri("http://localhost:8080/wizard");
        Dictionary<string, object> _measures = new Dictionary<string, object>();
        Dictionary<string, object> _combosValues = new Dictionary<string, object>();
        Dictionary<string, object> _strings = new Dictionary<string, object>();
        FrmBookStringValuesReplace _bsvr;
        private const int interval = 1500;
        TesterTimer _testerTimer;
        
        public frmWizardTester()
        {
            InitializeComponent();
            _testerTimer = new TesterTimer();
            _testerTimer.Tick += new EventHandler(testerTimer_Tick);
          
=======
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
>>>>>>> .r192

<<<<<<< .mine
=======
#if RELEASE
			edgeHostService = new Process();
			edgeHostService.StartInfo.FileName = Properties.Settings.Default.ServiceHostPath;
			edgeHostService.StartInfo.Arguments = "/WizardService";
			edgeHostService.Start();
#endif
        }
>>>>>>> .r192

<<<<<<< .mine
            //edgeHostService = new Process();
            //edgeHostService.StartInfo.FileName = Properties.Settings.Default.ServiceHostPath;
            //edgeHostService.StartInfo.Arguments = "/WizardService";
            //edgeHostService.Start();
=======
        private void frmWizardTester_Load(object sender, EventArgs e)
        {
>>>>>>> .r192

        }

<<<<<<< .mine
        void testerTimer_Tick(object sender, EventArgs e)
        {
            StepStatus stepStatus = StepStatus.NotReady;
            ServiceOutcome serviceOutcome;
            _testerTimer.Stop();
            if (_testerTimer.SourceObject != "Execute")
            {
                stepStatus = GetStepStatus();
            }
            else
            {
                WebRequest request = HttpWebRequest.Create(string.Format("{0}/GetExecutorState?sessionID={1}", _baseUri, _sessionID));                
               
                WebResponse response = request.GetResponse();
                DataContractSerializer serializer = new DataContractSerializer(typeof(ServiceOutcome));
                using (XmlReader reader = XmlReader.Create(response.GetResponseStream()))
                {
                     serviceOutcome = (ServiceOutcome)serializer.ReadObject(reader, false);
                }
                    switch (serviceOutcome)
                    {
                        case ServiceOutcome.Unspecified:
                            {
                                _testerTimer.Interval = 3000;
                                _testerTimer.SourceObject = "Execute";
                                _testerTimer.Start();
                                System.Windows.Forms.Application.DoEvents();
                                break;
                            }
                        case ServiceOutcome.Success:
                            {
                                txtLog.Text = "Execution Finished Sucessfully!";
                                break;
                            }
                        case ServiceOutcome.Failure:
                            {
                                txtLog.Text = "Executionfailed!";
                                //request = HttpWebRequest.Create(string.Format("{0}/GetErrors?sessionID={1}", _baseUri, _sessionID));
=======
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
            _measures = new Dictionary<string, object>();
            InitLaizeComboBoxs();
>>>>>>> .r192

<<<<<<< .mine
                                //response = request.GetResponse();
                                //serializer = new DataContractSerializer(typeof(string));
                                //using (XmlReader reader = XmlReader.Create(response.GetResponseStream()))
                                //{
                                //    String errors = (string)serializer.ReadObject(reader, false);
                                //    txtLog.Text = errors;
                                //}
                                break;
                            }
                        case ServiceOutcome.Aborted:
                            break;
                        case ServiceOutcome.CouldNotBeScheduled:
                            break;
                        default:
                            break;
                    
=======
>>>>>>> .r192

                }


            }

            
            //if (true) //cancel option future
            //{
                
            //}
            
            switch (_testerTimer.SourceObject)
            {
                case "FormLoad":
                    {
                        
                        btnStartNewSession.Enabled = true;
                       
                        break;
                    }
                case "grpBoxActiveDirectory":
                    {
                        if (stepStatus == StepStatus.Ready)
                        {
                            grpBoxActiveDirectory.Enabled = true;
                            
                                                   
                        }
                        else
                        {
                            _testerTimer.Interval = interval;
                            _testerTimer.SourceObject = "grpBoxActiveDirectory";
                            _testerTimer.Start();
                            System.Windows.Forms.Application.DoEvents();

                        }
                        break;
                    }
                case "grpRole":
                    {
                        if (stepStatus == StepStatus.Ready)
                        {
                            grpRole.Enabled = true;


                        }
                        else
                        {
                            _testerTimer.Interval = interval;
                            _testerTimer.SourceObject = "grpRole";
                            _testerTimer.Start();
                            System.Windows.Forms.Application.DoEvents();

                        }
                        break;
                    }
                case "grpCube":
                    {
                        if (stepStatus == StepStatus.Ready)
                        {
                            grpCube.Enabled = true;


<<<<<<< .mine
                        }
                        else
                        {
                            _testerTimer.Interval = interval;
                            _testerTimer.SourceObject = "grpCube";
                            _testerTimer.Start();
                            System.Windows.Forms.Application.DoEvents();
=======
>>>>>>> .r192

<<<<<<< .mine
                        }
                        break;
                    }
                case "grpBook":
                    {
                        if (stepStatus == StepStatus.Ready)
                        {
                            grpBook.Enabled = true;
=======
        }
        private void InitLaizeComboBoxs()
        {
            //Clean the combo boxs
            cmbReplaceCPA1.DataSource = null;
            cmbReplaceCPA2.DataSource = null;
>>>>>>> .r192

<<<<<<< .mine
=======
            //Load default values from app.config
            string[] comboDefaultValues = Properties.Settings.Default.CombosValues.Split(',');
            foreach (string str in comboDefaultValues)
            {
           
                _measures.Add("AccountSettings." + str, str);
               
>>>>>>> .r192

<<<<<<< .mine
                        }
                        else
                        {
                            _testerTimer.Interval = interval;
                            _testerTimer.SourceObject = "grpBook";
                            _testerTimer.Start();
                            System.Windows.Forms.Application.DoEvents();
=======
            }
            if (!_measures.ContainsKey("0"))
            {
                _measures.Add("0", " "); 
            }
            
>>>>>>> .r192

<<<<<<< .mine
                        }
                        break;
                    }
                case "grpSSIS":
                    {
                        if (stepStatus == StepStatus.Ready)
                        {
                            grpSSIS.Enabled = true;
=======
            cmbReplaceCPA1.DataSource = _measures.ToList();
            cmbReplaceCPA2.DataSource = _measures.ToList();
            cmbReplaceCPA1.ValueMember = "Key";
            cmbReplaceCPA1.DisplayMember = "Value";
            cmbReplaceCPA2.ValueMember = "Key";
            cmbReplaceCPA2.DisplayMember = "Value";
            cmbReplaceCPA1.SelectedValue = "0";
            cmbReplaceCPA2.SelectedValue = "0";
>>>>>>> .r192

<<<<<<< .mine
=======
        }
>>>>>>> .r192

<<<<<<< .mine
                        }
                        else
                        {
                            _testerTimer.Interval = interval;
                            _testerTimer.SourceObject = "grpSSIS";
                            _testerTimer.Start();
                            System.Windows.Forms.Application.DoEvents();
=======
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
>>>>>>> .r192

<<<<<<< .mine
                        }
                        break;
                    }
=======
                    Log.Write(String.Format("Could not start {0}.", instance.Configuration.Name), ex);
                }
            }
            else if (e.StateAfter == ServiceState.Ended)
            {
                txtLog.Text += "------------------------------";
                txtLog.Text += string.Format("{0} has ended.", instance.Configuration.Name);
            }
        }
>>>>>>> .r192

<<<<<<< .mine
            }
        }
=======
        private void txtScopName_Leave(object sender, EventArgs e)
        {
            txtCubeName.Text = txtScopName.Text;
            txtBookName.Text = txtScopName.Text;
            txtReplaceWithCubeName.Text = txtScopName.Text;
>>>>>>> .r192

        

        private void frmWizardTester_Load(object sender, EventArgs e)
        {


<<<<<<< .mine
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
            txtCubeNamePatternToReplace.Text = "From app.config of the service"; //Properties.Settings.Default.CubeNamePattenToReplace;
            txtDatabasePatternToReplace.Text = "From app.config of the service"; // Properties.Settings.Default.DatabaseNamePattenToReplace;
            txtDatabaseToReplace.Text = "From app.config of the service"; // Properties.Settings.Default.AnalysisServerDatabase;
            //txtReplaceWithCubeName.Text = Properties.Settings.Default.CubeNameStartWith;
            _measures = new Dictionary<string, object>();
            _combosValues = new Dictionary<string, object>();
            InitLaizeComboBoxs();
            _testerTimer.Interval = 9000;
            _testerTimer.SourceObject = "FormLoad";
            _testerTimer.Start();
           
            
            System.Windows.Forms.Application.DoEvents();
            
            
=======
        }
>>>>>>> .r192

<<<<<<< .mine
=======
        private void frmWizardTester_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                edgeHostService.Kill();
            }
            catch (Exception)
            {
>>>>>>> .r192


<<<<<<< .mine
=======
            }
>>>>>>> .r192

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
            btnGetSummary.Enabled = false;
            btnExecute.Enabled = false;
            if (!string.IsNullOrEmpty(txtScopName.Text) && !string.IsNullOrEmpty(txtScopeID.Text) || (txtScopeID.Enabled == false && txtScopName.Enabled == false))
            {
                WizardSession wizardSession = StartWizard("AccountWizard");

                if (wizardSession.WizardID == -1)
                {
                    txtLog.Text = string.Format("Account Wizard not Service Started:\nWizard ID is:{0}\nSessionID IS:{1}\nNext step name is:{2}\n--------------------", wizardSession.WizardID, wizardSession.SessionID, wizardSession.CurrentStep.StepName);
                }
                else
                {
                    txtLog.Text = string.Format("Account Wizard Service Started:\nWizard ID is:{0}\nSessionID IS:{1}\nNext step name is:{2}\n--------------------", wizardSession.WizardID, wizardSession.SessionID, wizardSession.CurrentStep.StepName);

<<<<<<< .mine
=======
                    grpBoxActiveDirectory.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Please fill scopeid and scopename");
                txtScopeID.Enabled = true;
                txtScopName.Enabled = true;
            }
>>>>>>> .r192




<<<<<<< .mine
=======
        }
>>>>>>> .r192

<<<<<<< .mine
        }
        private void InitLaizeComboBoxs()
        {
            //Clean the combo boxs
            cmbReplaceCPA1.DataSource = null;
            cmbReplaceCPA2.DataSource = null;
=======
        private WizardSession StartWizard(string wizardName)
        {
            WizardSession wizardSession;
            try
            {
>>>>>>> .r192

<<<<<<< .mine
            //Load default values from app.config
            string[] comboDefaultValues = Properties.Settings.Default.CombosValues.Split(',');
=======
                WebRequest request = HttpWebRequest.Create(string.Format(_baseUri + "/start?wizardName={0}", wizardName));
                WebResponse response = request.GetResponse();
>>>>>>> .r192

<<<<<<< .mine
            foreach (string str in comboDefaultValues)
            {
=======
                DataContractSerializer serializer = new DataContractSerializer(typeof(WizardSession));
                using (XmlReader reader = XmlReader.Create(response.GetResponseStream()))
                {
                    wizardSession = (WizardSession)serializer.ReadObject(reader, false);
>>>>>>> .r192

<<<<<<< .mine
                if (!_combosValues.ContainsKey("AccountSettings." + str))
                {
                    _combosValues.Add("AccountSettings." + str, str); 
                }
               
=======
                }
                _sessionID = wizardSession.SessionID;
                _nextStepName = wizardSession.CurrentStep.StepName;
>>>>>>> .r192

            }
            if (_measures!=null)
            {
                foreach (KeyValuePair<string,object> measure in _measures)
                {
                    if (!_combosValues.ContainsKey(measure.Key))
                    {
                        _combosValues.Add(measure.Key,measure.Value); 
                    }
                    
                }
            }
            if (!_combosValues.ContainsKey("0"))
            {
                _combosValues.Add("0", " "); 
            }


<<<<<<< .mine
            cmbReplaceCPA1.DataSource = _combosValues.ToList();
            cmbReplaceCPA2.DataSource = _combosValues.ToList();
            cmbReplaceCPA1.ValueMember = "Key";
            cmbReplaceCPA1.DisplayMember = "Value";
            cmbReplaceCPA2.ValueMember = "Key";
            cmbReplaceCPA2.DisplayMember = "Value";
            cmbReplaceCPA1.SelectedValue = "0";
            cmbReplaceCPA2.SelectedValue = "0";
=======
>>>>>>> .r192

<<<<<<< .mine
        }
=======
            }
            catch (Exception ex)
            {
                return new WizardSession() { CurrentStep = new StepConfiguration() { StepName = string.Format("Service did not started yet:{0}", ex.Message) }, SessionID = -1, WizardID = -1 };
>>>>>>> .r192

<<<<<<< .mine
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
=======
            }
            return wizardSession;
>>>>>>> .r192

<<<<<<< .mine
                    Log.Write(String.Format("Could not start {0}.", instance.Configuration.Name), ex);
                }
            }
            else if (e.StateAfter == ServiceState.Ended)
            {
                txtLog.Text += "------------------------------";
                txtLog.Text += string.Format("{0} has ended.", instance.Configuration.Name);
            }
        }
=======
        }
>>>>>>> .r192

<<<<<<< .mine
        private void txtScopName_Leave(object sender, EventArgs e)
        {
            txtCubeName.Text = txtScopName.Text;
            txtBookName.Text = txtScopName.Text;
            txtReplaceWithCubeName.Text = txtScopName.Text;
=======
        private void btnActiveDirectory_Click(object sender, EventArgs e)
        {
            DialogResult result = System.Windows.Forms.DialogResult.Yes;
            WebRequest request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", _baseUri, _sessionID));
            request.ContentType = "application/xml";
            request.Method = "POST";
            request.Timeout = 130000;
>>>>>>> .r192

<<<<<<< .mine
=======
            //Collect values
            StepCollectRequest stepCollectRequest = new StepCollectRequest();
            stepCollectRequest.StepName = _nextStepName;
            stepCollectRequest.CollectedValues = new Dictionary<string, object>();
            Dictionary<string, object> collectedValues = new Dictionary<string, object>();
            collectedValues.Add("ActiveDirectory.UserName", txtUserName.Text.Trim());
            collectedValues.Add("ActiveDirectory.Password", txtPassword.Text.Trim());
            collectedValues.Add("ActiveDirectory.FullName", txtFullName.Text.Trim());
            stepCollectRequest.CollectedValues = collectedValues;
>>>>>>> .r192


            try
            {
                SetBodyForCollectRequest(ref request, stepCollectRequest);
            }
            catch (Exception ex)
            {

<<<<<<< .mine
        }
=======
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
>>>>>>> .r192

<<<<<<< .mine
        private void frmWizardTester_FormClosing(object sender, FormClosingEventArgs e)
        {
            //try
            //{
            //    edgeHostService.Kill();
            //}
            //catch (Exception)
            //{
=======
                        }
>>>>>>> .r192


<<<<<<< .mine
            //}
=======
                        switch (stepCollectResponse.Result)
                        {
                            case StepResult.Next:
                                {
>>>>>>> .r192

<<<<<<< .mine
        }
=======
                                    txtLog.Text += string.Format("\n--------------------\nStep Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
                                    _nextStepName = stepCollectResponse.NextStep.StepName;
                                    grpRole.Enabled = true;
                                    ((Control)sender).Parent.Enabled = false;
>>>>>>> .r192

<<<<<<< .mine
        private void btnStartNewSession_Click(object sender, EventArgs e)
        {
            foreach (Control control in this.Controls)
            {
                if (control.GetType() == typeof(GroupBox))
                {
                    control.Enabled = false;
                }
            }
            btnGetSummary.Enabled = false;
            btnExecute.Enabled = false;
            if (!string.IsNullOrEmpty(txtScopName.Text) && !string.IsNullOrEmpty(txtScopeID.Text) || (txtScopeID.Enabled == false && txtScopName.Enabled == false))
            {
                WizardSession wizardSession = StartWizard("AccountWizard");
=======
>>>>>>> .r192

<<<<<<< .mine
                if (wizardSession.WizardID == -1)
                {
                    txtLog.Text = string.Format("Account Wizard not Service Started:\nWizard ID is:{0}\nSessionID IS:{1}\nNext step name is:{2}\n--------------------", wizardSession.WizardID, wizardSession.SessionID, wizardSession.CurrentStep.StepName);
                }
                else
                {
                    txtLog.Text = string.Format("Account Wizard Service Started:\nWizard ID is:{0}\nSessionID IS:{1}\nNext step name is:{2}\n--------------------", wizardSession.WizardID, wizardSession.SessionID, wizardSession.CurrentStep.StepName);
=======
                                    break;
                                }
                            case StepResult.HasErrors:
                                {
                                    foreach (KeyValuePair<string, string> error in stepCollectResponse.Errors)
                                    {
                                        txtLog.Text += string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value);
                                    }
>>>>>>> .r192

<<<<<<< .mine
                    _testerTimer.Interval = interval;
                    _testerTimer.SourceObject = "grpBoxActiveDirectory";
                    _testerTimer.Start();
                    System.Windows.Forms.Application.DoEvents();
=======
>>>>>>> .r192

<<<<<<< .mine
                }
            }
            else
            {
                MessageBox.Show("Please fill scopeid and scopename");
                txtScopeID.Enabled = true;
                txtScopName.Enabled = true;
            }
=======
                                    break;
                                }
                            case StepResult.Done:
                                {
                                    txtLog.Text = "\nFinsish collecting from all steps\nready to get summary or execute:";
>>>>>>> .r192

                                    break;
                                }
                        }



<<<<<<< .mine
        }
=======
                    }
                    catch (WebException ex)
                    {
>>>>>>> .r192

<<<<<<< .mine
        private WizardSession StartWizard(string wizardName)
        {
            WizardSession wizardSession;
            try
            {
=======
                        using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                        {
>>>>>>> .r192

<<<<<<< .mine
                WebRequest request = HttpWebRequest.Create(string.Format(_baseUri + "/start?wizardName={0}", wizardName));
                WebResponse response = request.GetResponse();
=======
                            txtLog.Text += string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
                        }
>>>>>>> .r192

<<<<<<< .mine
                DataContractSerializer serializer = new DataContractSerializer(typeof(WizardSession));
                using (XmlReader reader = XmlReader.Create(response.GetResponseStream()))
                {
                    wizardSession = (WizardSession)serializer.ReadObject(reader, false);
=======
                    }
                }
                catch (Exception ex)
                {
>>>>>>> .r192

<<<<<<< .mine
                }
                _sessionID = wizardSession.SessionID;
                _nextStepName = wizardSession.CurrentStep.StepName;
=======
                    txtLog.Text += ex.Message;
                }
>>>>>>> .r192

<<<<<<< .mine
=======
            }
        }
>>>>>>> .r192

        private void SetBodyForCollectRequest(ref WebRequest request, StepCollectRequest stepCollectRequest)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(StepCollectRequest));
            using (XmlWriter writer = XmlWriter.Create(request.GetRequestStream()))
            {
                serializer.WriteObject(writer, stepCollectRequest);
                writer.Flush();
            }
        }

<<<<<<< .mine
=======
        private void btnCreateRole_Click(object sender, EventArgs e)
        {
            DialogResult result = System.Windows.Forms.DialogResult.Yes;
            WebRequest request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", _baseUri, _sessionID));
            request.ContentType = "application/xml";
            request.Method = "POST";
            request.Timeout = 130000;
>>>>>>> .r192

<<<<<<< .mine
            }
            catch (Exception ex)
            {
                return new WizardSession() { CurrentStep = new StepConfiguration() { StepName = string.Format("Service did not started yet:{0}", ex.Message) }, SessionID = -1, WizardID = -1 };
=======
            //Collect values
            StepCollectRequest stepCollectRequest = new StepCollectRequest();
            stepCollectRequest.StepName = _nextStepName;
            stepCollectRequest.CollectedValues = new Dictionary<string, object>();
            Dictionary<string, object> collectedValues = new Dictionary<string, object>();
            collectedValues.Add("AccountSettings.RoleName", txtRoleName.Text.Trim());
            collectedValues.Add("AccountSettings.RoleID", txtRoleID.Text.Trim());
            collectedValues.Add("AccountSettings.RoleMemberName", txtRoleMemberName.Text.Trim());
            stepCollectRequest.CollectedValues = collectedValues;
>>>>>>> .r192

            }
            return wizardSession;

<<<<<<< .mine
        }
=======
            try
            {
                SetBodyForCollectRequest(ref request, stepCollectRequest);
            }
            catch (Exception ex)
            {
>>>>>>> .r192

<<<<<<< .mine
        private void btnActiveDirectory_Click(object sender, EventArgs e)
        {
            DialogResult result = System.Windows.Forms.DialogResult.Yes;
            WebRequest request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", _baseUri, _sessionID));
            request.ContentType = "application/xml";
            request.Method = "POST";
            request.Timeout = 130000;
=======
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
>>>>>>> .r192

<<<<<<< .mine
            //Collect values
            StepCollectRequest stepCollectRequest = new StepCollectRequest();
            stepCollectRequest.StepName = _nextStepName;
            stepCollectRequest.CollectedValues = new Dictionary<string, object>();
            Dictionary<string, object> collectedValues = new Dictionary<string, object>();
            collectedValues.Add("ActiveDirectory.UserName", txtUserName.Text.Trim());
            collectedValues.Add("ActiveDirectory.Password", txtPassword.Text.Trim());
            collectedValues.Add("ActiveDirectory.FullName", txtFullName.Text.Trim());
            stepCollectRequest.CollectedValues = collectedValues;
=======
                        }
>>>>>>> .r192


<<<<<<< .mine
            try
            {
                SetBodyForCollectRequest(ref request, stepCollectRequest);
            }
            catch (Exception ex)
            {
=======
                        switch (stepCollectResponse.Result)
                        {
                            case StepResult.Next:
                                {
>>>>>>> .r192

<<<<<<< .mine
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
=======
                                    txtLog.Text += string.Format("\n--------------------\nStep Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
                                    _nextStepName = stepCollectResponse.NextStep.StepName;
                                    grpCube.Enabled = true;
                                    ((Control)sender).Parent.Enabled = false;
>>>>>>> .r192

<<<<<<< .mine
                        }
=======
                                    break;
                                }
                            case StepResult.HasErrors:
                                {
                                    foreach (KeyValuePair<string, string> error in stepCollectResponse.Errors)
                                    {
                                        txtLog.Text += string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value);
                                    }
>>>>>>> .r192


<<<<<<< .mine
                        switch (stepCollectResponse.Result)
                        {
                            case StepResult.Next:
                                {
=======
                                    break;
                                }
                            case StepResult.Done:
                                {
                                    txtLog.Text = "\nFinsish collecting from all steps\nready to get summary or execute:";
>>>>>>> .r192

<<<<<<< .mine
                                    txtLog.Text += string.Format("\n--------------------\nStep Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
                                    _nextStepName = stepCollectResponse.NextStep.StepName;
                                   
                                    ((Control)sender).Parent.Enabled = false;
                                    _testerTimer.Interval = interval;
                                    _testerTimer.SourceObject = "grpRole";
                                    _testerTimer.Start();
                                    System.Windows.Forms.Application.DoEvents();
=======
                                    break;
                                }
                        }
>>>>>>> .r192


                                    break;
                                }
                            case StepResult.HasErrors:
                                {
                                    foreach (KeyValuePair<string, string> error in stepCollectResponse.Errors)
                                    {
                                        txtLog.Text += string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value);
                                    }

                    }
                    catch (WebException ex)
                    {

<<<<<<< .mine
                                    break;
                                }
                            case StepResult.Done:
                                {
                                    txtLog.Text = "\nFinsish collecting from all steps\nready to get summary or execute:";
=======
                        using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                        {
>>>>>>> .r192

<<<<<<< .mine
                                    break;
                                }
                        }
=======
                            txtLog.Text += string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
                        }
>>>>>>> .r192

                    }
                }
                catch (Exception ex)
                {

                    txtLog.Text += ex.Message;
                }

<<<<<<< .mine
                    }
                    catch (WebException ex)
                    {
=======
            }
        }
>>>>>>> .r192

<<<<<<< .mine
                        using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                        {
=======
        private void btnCreateNewCube_Click(object sender, EventArgs e)
        {
            DialogResult result = System.Windows.Forms.DialogResult.Yes;
            WebRequest request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", _baseUri, _sessionID));
            request.ContentType = "application/xml";
            request.Method = "POST";
            request.Timeout = 130000;
>>>>>>> .r192

<<<<<<< .mine
                            txtLog.Text += string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
                        }
=======
            //Collect values
            StepCollectRequest stepCollectRequest = new StepCollectRequest();
            stepCollectRequest.StepName = _nextStepName;
            stepCollectRequest.CollectedValues = new Dictionary<string, object>();
            Dictionary<string, object> collectedValues = new Dictionary<string, object>();
>>>>>>> .r192

<<<<<<< .mine
                    }
                }
                catch (Exception ex)
                {
=======
            if (string.IsNullOrEmpty(cmbReplaceCPA1.Text))
                collectedValues.Add("AccountSettings.New Users", "0");
            else
                collectedValues.Add("AccountSettings.New Users", ((KeyValuePair<string, object>)cmbReplaceCPA1.SelectedItem).Value.ToString());
            if (string.IsNullOrEmpty(cmbReplaceCPA2.Text))
                collectedValues.Add("AccountSettings.New Active Users", "0");
            else
                collectedValues.Add("AccountSettings.New Active Users", ((KeyValuePair<string, object>)cmbReplaceCPA2.SelectedItem).Value.ToString());
>>>>>>> .r192

<<<<<<< .mine
                    txtLog.Text += ex.Message;
                }
=======
            collectedValues.Add("AccountSettings.Scope_ID", txtScopeID.Text.Trim());
            collectedValues.Add("AccountSettings.CubeName", txtCubeName.Text.Trim());
            collectedValues.Add("AccountSettings.CubeID", txtCubeName.Text.Trim());
            //add the measures to replace
>>>>>>> .r192

<<<<<<< .mine
            }
        }
=======
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
>>>>>>> .r192

<<<<<<< .mine
        private void SetBodyForCollectRequest(ref WebRequest request, StepCollectRequest stepCollectRequest)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(StepCollectRequest));
            using (XmlWriter writer = XmlWriter.Create(request.GetRequestStream()))
            {
                serializer.WriteObject(writer, stepCollectRequest);
                writer.Flush();
            }
        }
=======
            }
>>>>>>> .r192

<<<<<<< .mine
        private void btnCreateRole_Click(object sender, EventArgs e)
        {
            DialogResult result = System.Windows.Forms.DialogResult.Yes;
            WebRequest request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", _baseUri, _sessionID));
            request.ContentType = "application/xml";
            request.Method = "POST";
            request.Timeout = 130000;
=======
>>>>>>> .r192

            //Collect values
            StepCollectRequest stepCollectRequest = new StepCollectRequest();
            stepCollectRequest.StepName = _nextStepName;
            stepCollectRequest.CollectedValues = new Dictionary<string, object>();
            Dictionary<string, object> collectedValues = new Dictionary<string, object>();
            collectedValues.Add("AccountSettings.RoleName", txtRoleName.Text.Trim());
            collectedValues.Add("AccountSettings.RoleID", txtRoleID.Text.Trim());
            collectedValues.Add("AccountSettings.RoleMemberName", txtRoleMemberName.Text.Trim());
            stepCollectRequest.CollectedValues = collectedValues;


<<<<<<< .mine
            try
            {
                SetBodyForCollectRequest(ref request, stepCollectRequest);
            }
            catch (Exception ex)
            {
=======
            if (chkContent.Checked)
                collectedValues.Add("AccountSettings.AddContentCube", bool.Parse("true"));
            else
                collectedValues.Add("AccountSettings.AddContentCube", bool.Parse("false"));
            stepCollectRequest.CollectedValues = collectedValues;
>>>>>>> .r192

<<<<<<< .mine
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
=======
            if (chkCpa1.Checked  || IsgooleValueSelected(cmbReplaceCPA1))
                collectedValues.Add("AccountSettings.Cpa1.OnlyCalC", bool.Parse("true"));
            else
                collectedValues.Add("AccountSettings.Cpa1.OnlyCalC", bool.Parse("false"));
>>>>>>> .r192

<<<<<<< .mine
                        }
=======
            if (chkCpa1.Checked || IsgooleValueSelected(cmbReplaceCPA2))
                collectedValues.Add("AccountSettings.Cpa2.OnlyCalC", bool.Parse("true"));
            else
                collectedValues.Add("AccountSettings.Cpa2.OnlyCalC", bool.Parse("false"));
>>>>>>> .r192


<<<<<<< .mine
                        switch (stepCollectResponse.Result)
                        {
                            case StepResult.Next:
                                {
=======
>>>>>>> .r192

<<<<<<< .mine
                                    txtLog.Text += string.Format("\n--------------------\nStep Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
                                    _nextStepName = stepCollectResponse.NextStep.StepName;
                                    
                                    ((Control)sender).Parent.Enabled = false;
                                    _testerTimer.Interval = interval;
                                    _testerTimer.SourceObject = "grpCube";
                                    _testerTimer.Start();
                                    System.Windows.Forms.Application.DoEvents();
=======
>>>>>>> .r192

<<<<<<< .mine
                                    break;
                                }
                            case StepResult.HasErrors:
                                {
                                    foreach (KeyValuePair<string, string> error in stepCollectResponse.Errors)
                                    {
                                        txtLog.Text += string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value);
                                    }
=======
>>>>>>> .r192


<<<<<<< .mine
                                    break;
                                }
                            case StepResult.Done:
                                {
                                    txtLog.Text = "\nFinsish collecting from all steps\nready to get summary or execute:";
=======
            try
            {
                SetBodyForCollectRequest(ref request, stepCollectRequest);
            }
            catch (Exception ex)
            {
>>>>>>> .r192

<<<<<<< .mine
                                    break;
                                }
                        }
=======
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
>>>>>>> .r192

<<<<<<< .mine
=======
                        }
>>>>>>> .r192


<<<<<<< .mine
                    }
                    catch (WebException ex)
                    {
=======
                        switch (stepCollectResponse.Result)
                        {
                            case StepResult.Next:
                                {
>>>>>>> .r192

<<<<<<< .mine
                        using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                        {
=======
                                    txtLog.Text += string.Format("\n--------------------\nStep Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
                                    _nextStepName = stepCollectResponse.NextStep.StepName;
                                    grpBook.Enabled = true;
                                    ((Control)sender).Parent.Enabled = false;
>>>>>>> .r192

<<<<<<< .mine
                            txtLog.Text += string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
                        }
=======
>>>>>>> .r192

<<<<<<< .mine
                    }
                }
                catch (Exception ex)
                {
=======
                                    break;
                                }
                            case StepResult.HasErrors:
                                {
                                    foreach (KeyValuePair<string, string> error in stepCollectResponse.Errors)
                                    {
                                        txtLog.Text += string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value);
                                    }
>>>>>>> .r192

<<<<<<< .mine
                    txtLog.Text += ex.Message;
                }
=======
>>>>>>> .r192

<<<<<<< .mine
            }
        }
=======
                                    break;
                                }
                            case StepResult.Done:
                                {
                                    txtLog.Text = "\nFinsish collecting from all steps\nready to get summary or execute:";
>>>>>>> .r192

<<<<<<< .mine
        private void btnCreateNewCube_Click(object sender, EventArgs e)
        {
            DialogResult result = System.Windows.Forms.DialogResult.Yes;
            WebRequest request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", _baseUri, _sessionID));
            request.ContentType = "application/xml";
            request.Method = "POST";
            request.Timeout = 130000;
=======
                                    break;
                                }
                        }
>>>>>>> .r192

<<<<<<< .mine
            //Collect values
            StepCollectRequest stepCollectRequest = new StepCollectRequest();
            stepCollectRequest.StepName = _nextStepName;
            stepCollectRequest.CollectedValues = new Dictionary<string, object>();
            Dictionary<string, object> collectedValues = new Dictionary<string, object>();
=======
>>>>>>> .r192

<<<<<<< .mine
            if (string.IsNullOrEmpty(cmbReplaceCPA1.Text))
                collectedValues.Add("AccountSettings.New Users", "0");
            else
            {
                collectedValues.Add("AccountSettings.New Users", cmbReplaceCPA1.Text);
            }
            if (string.IsNullOrEmpty(cmbReplaceCPA2.Text))
                collectedValues.Add("AccountSettings.New Active Users", "0");
            else
                collectedValues.Add("AccountSettings.New Active Users", cmbReplaceCPA2.Text);
=======
>>>>>>> .r192

<<<<<<< .mine
            collectedValues.Add("AccountSettings.Scope_ID", txtScopeID.Text.Trim());
            collectedValues.Add("AccountSettings.CubeName", txtCubeName.Text.Trim());
            collectedValues.Add("AccountSettings.CubeID", txtCubeName.Text.Trim());
            //add the measures to replace
=======
                    }
                    catch (WebException ex)
                    {
>>>>>>> .r192

<<<<<<< .mine
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
=======
                        using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                        {
>>>>>>> .r192

<<<<<<< .mine
            }
=======
                            txtLog.Text += string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
                        }
>>>>>>> .r192

                    }
                }
                catch (Exception ex)
                {

                    txtLog.Text += ex.Message;
                }

            }

<<<<<<< .mine
            if (chkContent.Checked)
                collectedValues.Add("AccountSettings.AddContentCube", bool.Parse("true"));
            else
                collectedValues.Add("AccountSettings.AddContentCube", bool.Parse("false"));
            stepCollectRequest.CollectedValues = collectedValues;
=======
        }
>>>>>>> .r192

<<<<<<< .mine
            if (chkCpa1.Checked  || IsgooleValueSelected(cmbReplaceCPA1))
                collectedValues.Add("AccountSettings.Cpa1.OnlyCalC", bool.Parse("true"));
            else
                collectedValues.Add("AccountSettings.Cpa1.OnlyCalC", bool.Parse("false"));
=======
        private void button4_Click(object sender, EventArgs e)
        {
            
            FrmMeasuresList frmMeasuresList = new FrmMeasuresList(_measures);
            frmMeasuresList.ShowDialog();
            _measures = frmMeasuresList.Measures;
            //_measures.Add("0", "0");
            InitLaizeComboBoxs();
>>>>>>> .r192

<<<<<<< .mine
            if (chkCpa1.Checked || IsgooleValueSelected(cmbReplaceCPA2))
                collectedValues.Add("AccountSettings.Cpa2.OnlyCalC", bool.Parse("true"));
            else
                collectedValues.Add("AccountSettings.Cpa2.OnlyCalC", bool.Parse("false"));
=======
            //cmbReplaceCPA1.Items.Clear();
            //cmbReplaceCPA2.Items.Clear();
            //cmbReplaceCPA1.DataSource = _measures.ToList();
            //cmbReplaceCPA2.DataSource = _measures.ToList();
            //cmbReplaceCPA1.ValueMember = "Key";
            //cmbReplaceCPA1.DisplayMember = "Value";
            //cmbReplaceCPA2.ValueMember = "Key";
            //cmbReplaceCPA2.DisplayMember = "Value";
            //cmbReplaceCPA1.SelectedValue = "0";
            //cmbReplaceCPA2.SelectedValue = "0";
>>>>>>> .r192

<<<<<<< .mine
=======
            //foreach (KeyValuePair<string, object> measure in _measures)
            //{
>>>>>>> .r192

<<<<<<< .mine
=======
            //    cmbReplaceCPA1.Items.Add(measure);
            //    cmbReplaceCPA2.Items.Add(measure);
            //    cmbReplaceCPA1.ValueMember = measure.Key;
            //    cmbReplaceCPA1.DisplayMember = measure.Value.ToString();
            //    cmbReplaceCPA2.ValueMember = measure.Key;
            //    cmbReplaceCPA2.DisplayMember = measure.Value.ToString();
            //}
>>>>>>> .r192


            //cmbReplaceCPA1.Items.Insert(0, " ");
            //cmbReplaceCPA2.Items.Insert(0, " ");
            //cmbReplaceCPA1.SelectedItem = cmbReplaceCPA1.Items[0];
            //cmbReplaceCPA2.SelectedItem = cmbReplaceCPA2.Items[0];
        }

<<<<<<< .mine
=======
        private void button8_Click(object sender, EventArgs e)
        {
            if (_bsvr == null)
                _bsvr = new FrmBookStringValuesReplace(_strings);
            _bsvr.ShowDialog();
            _strings = _bsvr.Strings;
        }
>>>>>>> .r192

<<<<<<< .mine
            try
            {
                SetBodyForCollectRequest(ref request, stepCollectRequest);
            }
            catch (Exception ex)
            {
=======
        private void button7_Click(object sender, EventArgs e)
        {
            if (_bsvr == null)
                _bsvr = new FrmBookStringValuesReplace(_strings);
            _bsvr.ShowDialog();
            _strings = _bsvr.Strings;
        }
>>>>>>> .r192

<<<<<<< .mine
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
=======
        private void txtScopeID_Leave(object sender, EventArgs e)
        {
>>>>>>> .r192

<<<<<<< .mine
                        }
=======
        }
>>>>>>> .r192

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = System.Windows.Forms.DialogResult.Yes;
            WebRequest request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", _baseUri, _sessionID));
            request.ContentType = "application/xml";
            request.Method = "POST";
            request.Timeout = 130000;

<<<<<<< .mine
                        switch (stepCollectResponse.Result)
                        {
                            case StepResult.Next:
                                {
=======
            //Collect values
            StepCollectRequest stepCollectRequest = new StepCollectRequest();
            stepCollectRequest.StepName = _nextStepName;
            stepCollectRequest.CollectedValues = new Dictionary<string, object>();
            Dictionary<string, object> collectedValues = new Dictionary<string, object>();
>>>>>>> .r192

<<<<<<< .mine
                                    txtLog.Text += string.Format("\n--------------------\nStep Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
                                    _nextStepName = stepCollectResponse.NextStep.StepName;
                                   
                                    ((Control)sender).Parent.Enabled = false;
                                    _testerTimer.Interval = interval;
                                    _testerTimer.SourceObject = "grpBook";
                                    _testerTimer.Start();
                                    System.Windows.Forms.Application.DoEvents();
=======
>>>>>>> .r192

            //add the string values to replace
            foreach (KeyValuePair<string, object> str in _strings)
            {
                if (collectedValues.ContainsKey(str.Key))
                    collectedValues.Remove(str.Key);
                collectedValues.Add(str.Key, str.Value);

            }

<<<<<<< .mine
                                    break;
                                }
                            case StepResult.HasErrors:
                                {
                                    foreach (KeyValuePair<string, string> error in stepCollectResponse.Errors)
                                    {
                                        txtLog.Text += string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value);
                                    }
=======
>>>>>>> .r192


<<<<<<< .mine
                                    break;
                                }
                            case StepResult.Done:
                                {
                                    txtLog.Text = "\nFinsish collecting from all steps\nready to get summary or execute:";
=======
>>>>>>> .r192

<<<<<<< .mine
                                    break;
                                }
                        }
=======
>>>>>>> .r192



<<<<<<< .mine
                    }
                    catch (WebException ex)
                    {
=======
            try
            {
                SetBodyForCollectRequest(ref request, stepCollectRequest);
            }
            catch (Exception ex)
            {
>>>>>>> .r192

<<<<<<< .mine
                        using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                        {
=======
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
>>>>>>> .r192

<<<<<<< .mine
                            txtLog.Text += string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
                        }
=======
                        }
>>>>>>> .r192

                    }
                }
                catch (Exception ex)
                {

<<<<<<< .mine
                    txtLog.Text += ex.Message;
                }
=======
                        switch (stepCollectResponse.Result)
                        {
                            case StepResult.Next:
                                {
>>>>>>> .r192

<<<<<<< .mine
            }
=======
                                    txtLog.Text += string.Format("\n--------------------\nStep Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
                                    _nextStepName = stepCollectResponse.NextStep.StepName;
                                    grpSSIS.Enabled = true;
                                    ((Control)sender).Parent.Enabled = false;
>>>>>>> .r192

<<<<<<< .mine
        }
=======
>>>>>>> .r192

<<<<<<< .mine
        private void button4_Click(object sender, EventArgs e)
        {
            
            FrmMeasuresList frmMeasuresList = new FrmMeasuresList(_measures);
            frmMeasuresList.ShowDialog();
            _measures = frmMeasuresList.Measures;
            //_measures.Add("0", "0");
            InitLaizeComboBoxs();
=======
                                    break;
                                }
                            case StepResult.HasErrors:
                                {
                                    foreach (KeyValuePair<string, string> error in stepCollectResponse.Errors)
                                    {
                                        txtLog.Text += string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value);
                                    }
>>>>>>> .r192

<<<<<<< .mine
            //cmbReplaceCPA1.Items.Clear();
            //cmbReplaceCPA2.Items.Clear();
            //cmbReplaceCPA1.DataSource = _measures.ToList();
            //cmbReplaceCPA2.DataSource = _measures.ToList();
            //cmbReplaceCPA1.ValueMember = "Key";
            //cmbReplaceCPA1.DisplayMember = "Value";
            //cmbReplaceCPA2.ValueMember = "Key";
            //cmbReplaceCPA2.DisplayMember = "Value";
            //cmbReplaceCPA1.SelectedValue = "0";
            //cmbReplaceCPA2.SelectedValue = "0";
=======
>>>>>>> .r192

<<<<<<< .mine
            //foreach (KeyValuePair<string, object> measure in _measures)
            //{
=======
                                    break;
                                }
                            case StepResult.Done:
                                {
                                    txtLog.Text = "\nFinsish collecting from all steps\nready to get summary or execute:";
>>>>>>> .r192

<<<<<<< .mine
            //    cmbReplaceCPA1.Items.Add(measure);
            //    cmbReplaceCPA2.Items.Add(measure);
            //    cmbReplaceCPA1.ValueMember = measure.Key;
            //    cmbReplaceCPA1.DisplayMember = measure.Value.ToString();
            //    cmbReplaceCPA2.ValueMember = measure.Key;
            //    cmbReplaceCPA2.DisplayMember = measure.Value.ToString();
            //}
=======
                                    break;
                                }
                        }
>>>>>>> .r192


            //cmbReplaceCPA1.Items.Insert(0, " ");
            //cmbReplaceCPA2.Items.Insert(0, " ");
            //cmbReplaceCPA1.SelectedItem = cmbReplaceCPA1.Items[0];
            //cmbReplaceCPA2.SelectedItem = cmbReplaceCPA2.Items[0];
        }

<<<<<<< .mine
        private void button8_Click(object sender, EventArgs e)
        {
            if (_bsvr == null)
                _bsvr = new FrmBookStringValuesReplace(_strings);
            _bsvr.ShowDialog();
            _strings = _bsvr.Strings;
        }
=======
                    }
                    catch (WebException ex)
                    {
>>>>>>> .r192

<<<<<<< .mine
        private void button7_Click(object sender, EventArgs e)
        {
            if (_bsvr == null)
                _bsvr = new FrmBookStringValuesReplace(_strings);
            _bsvr.ShowDialog();
            _strings = _bsvr.Strings;
        }
=======
                        using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                        {
>>>>>>> .r192

<<<<<<< .mine
        private void txtScopeID_Leave(object sender, EventArgs e)
        {
=======
                            txtLog.Text += string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
                        }
>>>>>>> .r192

<<<<<<< .mine
        }
=======
                    }
                }
                catch (Exception ex)
                {
>>>>>>> .r192

<<<<<<< .mine
        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = System.Windows.Forms.DialogResult.Yes;
            WebRequest request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", _baseUri, _sessionID));
            request.ContentType = "application/xml";
            request.Method = "POST";
            request.Timeout = 130000;
=======
                    txtLog.Text += ex.Message;
                }
>>>>>>> .r192

<<<<<<< .mine
            //Collect values
            StepCollectRequest stepCollectRequest = new StepCollectRequest();
            stepCollectRequest.StepName = _nextStepName;
            stepCollectRequest.CollectedValues = new Dictionary<string, object>();
            Dictionary<string, object> collectedValues = new Dictionary<string, object>();
=======
            }
        }
>>>>>>> .r192

<<<<<<< .mine
=======
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
                txtLog.Text += "EXECUTION FINISHED SUCCESSFULY";
            }
            catch (Exception ex)
            {
                txtLog.Text += "------------------------------";
                txtLog.Text += ex.Message;
            }
            finally
            {
>>>>>>> .r192

<<<<<<< .mine
            //add the string values to replace
            foreach (KeyValuePair<string, object> str in _strings)
            {
                if (collectedValues.ContainsKey(str.Key))
                    collectedValues.Remove(str.Key);
                collectedValues.Add(str.Key, str.Value);
=======
            }
        }
>>>>>>> .r192

<<<<<<< .mine
            }
=======
        private void button5_Click(object sender, EventArgs e)
        {
            //AccountSettings.CubeName
            DialogResult result = System.Windows.Forms.DialogResult.Yes;
            WebRequest request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", _baseUri, _sessionID));
            request.ContentType = "application/xml";
            request.Method = "POST";
            request.Timeout = 130000;
>>>>>>> .r192

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

            try
            {
                SetBodyForCollectRequest(ref request, stepCollectRequest);
            }
            catch (Exception ex)
            {

<<<<<<< .mine
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
=======
                        switch (stepCollectResponse.Result)
                        {
                            case StepResult.Next:
                                {
>>>>>>> .r192

<<<<<<< .mine
                        }
=======
                                    txtLog.Text += string.Format("\n--------------------\nStep Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
                                    _nextStepName = stepCollectResponse.NextStep.StepName;
                                    btnExecute.Enabled = true;
                                    ((Control)sender).Parent.Enabled = false;
>>>>>>> .r192


<<<<<<< .mine
                        switch (stepCollectResponse.Result)
                        {
                            case StepResult.Next:
                                {
=======
                                    break;
                                }
                            case StepResult.HasErrors:
                                {
                                    foreach (KeyValuePair<string, string> error in stepCollectResponse.Errors)
                                    {
                                        txtLog.Text += "------------------------------";
                                        txtLog.Text += string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value);
                                    }
>>>>>>> .r192

<<<<<<< .mine
                                    txtLog.Text += string.Format("\n--------------------\nStep Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
                                    _nextStepName = stepCollectResponse.NextStep.StepName;
                                  
                                    
                                    ((Control)sender).Parent.Enabled = false;
                                    _testerTimer.Interval = interval;
                                    _testerTimer.SourceObject = "grpSSIS";
                                    _testerTimer.Start();
                                    System.Windows.Forms.Application.DoEvents();


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
                _testerTimer.Interval = 10000;
                _testerTimer.SourceObject = "Execute";
                _testerTimer.Start();
                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                txtLog.Text += "------------------------------";
                txtLog.Text += ex.Message;
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
                                    ((Control)sender).Parent.Enabled = false;


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
                                    ((Control)sender).Parent.Enabled = false;
                                    btnExecute.Enabled = true;
                                    btnGetSummary.Enabled = true;
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
                    txtLog.Text = SummaryXml.DocumentElement.InnerText;

                }
            }
            catch (Exception ex)
            {

                txtLog.Text = ex.Message;
            }
        }

        private void IfGoogleValueSelected(object sender)
        {
             ComboBox selectedComboBox = (ComboBox)sender;
           bool isSelected= IsgooleValueSelected(selectedComboBox);
            CheckBox selectedCheckBox=null;
            switch (selectedComboBox.Name)
            {
                case "cmbReplaceCPA1":
                    {
                        selectedCheckBox = this.chkCpa1;
                        break;
                    }
                case "cmbReplaceCPA2":
                    {
                        selectedCheckBox = this.chkCpa2;
                        break;
                    }

            }
            if (isSelected)
            {
                selectedCheckBox.Checked = isSelected;
                selectedCheckBox.Enabled = false;
            }
            else
                selectedCheckBox.Enabled = true;
            
        }
        private bool IsgooleValueSelected(ComboBox sender)
        {
            string[] comboValues = Properties.Settings.Default.CombosValues.Split(',');
           
            
            bool isSelected=false;
            

            foreach (string str in comboValues)
            {
                if (sender.Text.Trim() == str.Trim())
                {
                    isSelected = true;
                    
                }
              

            }
            return isSelected;

            

        }

        

       

       

        private StepStatus GetStepStatus()
        {
            StepStatus stepStatus = StepStatus.NotReady;
            try
            {
                WebRequest request = HttpWebRequest.Create(string.Format("{0}/GetStepState?sessionID={1}", _baseUri, _sessionID));
                WebResponse response = request.GetResponse();
                DataContractSerializer serializer = new DataContractSerializer(typeof(StepStatus));
                using (XmlReader reader = XmlReader.Create(response.GetResponseStream()))
                {
                    stepStatus = (StepStatus)serializer.ReadObject(reader, false);

                }
            }
            catch (Exception ex)
            {
               
                stepStatus = StepStatus.NotReady;
            }
           
            return stepStatus;
            
            
        }

        private void grpBook_Enter(object sender, EventArgs e)
        {

        }
    }
    public class TesterTimer : System.Windows.Forms.Timer
    {
        public string SourceObject { get; set; }

    }
=======

                                    break;
                                }
                            case StepResult.Done:
                                {
                                    txtLog.Text += "------------------------------";
                                    txtLog.Text += "\nFinsish collecting from all steps\nready to get summary or execute:";
                                    ((Control)sender).Parent.Enabled = false;
                                    btnExecute.Enabled = true;
                                    btnGetSummary.Enabled = true;
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
                    txtLog.Text = SummaryXml.DocumentElement.InnerText;

                }
            }
            catch (Exception ex)
            {

                txtLog.Text = ex.Message;
            }
        }

        private void IfGoogleValueSelected(object sender)
        {
             ComboBox selectedComboBox = (ComboBox)sender;
           bool isSelected= IsgooleValueSelected(selectedComboBox);
            CheckBox selectedCheckBox=null;
            switch (selectedComboBox.Name)
            {
                case "cmbReplaceCPA1":
                    {
                        selectedCheckBox = this.chkCpa1;
                        break;
                    }
                case "cmbReplaceCPA2":
                    {
                        selectedCheckBox = this.chkCpa2;
                        break;
                    }

            }
            if (isSelected)
            {
                selectedCheckBox.Checked = isSelected;
                selectedCheckBox.Enabled = false;
            }
            else
                selectedCheckBox.Enabled = true;
            
        }
        private bool IsgooleValueSelected(ComboBox sender)
        {
            string[] comboValues = Properties.Settings.Default.CombosValues.Split(',');
           
            
            bool isSelected=false;
            

            foreach (string str in comboValues)
            {
                if (sender.Text.Trim() == str.Trim())
                {
                    isSelected = true;
                    
                }
              

            }
            return isSelected;

            

        }

        

        private void cmbReplaceCPA1_Validated(object sender, EventArgs e)
        {
            IfGoogleValueSelected(sender);
        }

        private void cmbReplaceCPA2_Validated(object sender, EventArgs e)
        {
            IfGoogleValueSelected(sender);
        }

        

        














    }
>>>>>>> .r192
}
