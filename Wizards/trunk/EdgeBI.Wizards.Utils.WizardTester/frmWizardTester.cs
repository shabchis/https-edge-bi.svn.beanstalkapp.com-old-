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
using System.Text.RegularExpressions;


namespace EdgeBI.Wizards.Utils.WizardTester
{
    public partial class frmWizardTester : Form
    {
        int? _sessionID = null;
        string _nextStepName;
        private Process edgeHostService;
        Uri _baseUri = new Uri("http://localhost:8080/wizard");
        Dictionary<string, object> _measures = new Dictionary<string, object>();
        Dictionary<string, object> _BEDataValues = new Dictionary<string, object>();
        Dictionary<string, object> _CPAData = new Dictionary<string, object>();
        Dictionary<string, object> _strings = new Dictionary<string, object>();
        FrmBookStringValuesReplace _bsvr;
        private const int interval = 1500;
        TesterTimer _testerTimer;

        public frmWizardTester()
        {
            InitializeComponent();
            _testerTimer = new TesterTimer();
            _testerTimer.Tick += new EventHandler(testerTimer_Tick);



            //edgeHostService = new Process();
            //edgeHostService.StartInfo.FileName = Properties.Settings.Default.ServiceHostPath;
            //edgeHostService.StartInfo.Arguments = "/WizardService";
            //edgeHostService.Start();

        }

        void testerTimer_Tick(object sender, EventArgs e)
        {
            StepStatus stepStatus = StepStatus.NotReady;
            ServiceOutcome serviceOutcome;
            ProgressState progressState;
            _testerTimer.Stop();
            if (_testerTimer.SourceObject != "Execute")
            {
                stepStatus = GetStepStatus();
            }
            else
            {

                WebRequest request = HttpWebRequest.Create(string.Format("{0}/Progress?sessionID={1}", _baseUri, _sessionID));
                request.Timeout = 900000;
                WebResponse response = request.GetResponse();

                DataContractSerializer serializer = new DataContractSerializer(typeof(ProgressState));
                using (XmlReader reader = XmlReader.Create(response.GetResponseStream()))
                {
                    try
                    {
                        progressState = (ProgressState)serializer.ReadObject(reader, false);
                      
                        prgOverAll.Value = Convert.ToInt32((System.Math.Round((progressState.OverAllProgess * prgOverAll.Maximum), 0)));

                    }
                    catch (Exception ex)
                    {
                        
                        txtLog.Text=ex.Message;
                        prgOverAll.Value=prgOverAll.Maximum;

                    }
                   
                }

                
                
                if (prgOverAll.Value == prgOverAll.Maximum)
                {
                    prgOverAll.Value = 0;
                    
                    request = HttpWebRequest.Create(string.Format("{0}/GetExecutorState?sessionID={1}", _baseUri, _sessionID));
                    response = request.GetResponse();
                    serializer = new DataContractSerializer(typeof(ServiceOutcome));
                    using (XmlReader reader = XmlReader.Create(response.GetResponseStream()))
                    {
                        serviceOutcome = (ServiceOutcome)serializer.ReadObject(reader, false); 
                    }
                    switch (serviceOutcome)
                    {
                        case ServiceOutcome.Unspecified:
                            {
                                _testerTimer.Interval = 1500;
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
                                //This is a remark because I cant get the right error from the log talked with doron we will make an exception system
                                //request = HttpWebRequest.Create(string.Format("{0}/GetErrors?sessionID={1}", _baseUri, _sessionID));

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


                    }
                }
                else
                {
                    _testerTimer.Interval = 3000;
                    _testerTimer.SourceObject = "Execute";
                    _testerTimer.Start();
                    System.Windows.Forms.Application.DoEvents();

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
                            ((Control)tabGeneral).Enabled = true;
                            ((Control)tabGeneral).Parent.Enabled = true;
                            ((Control)tabGeneral).Parent.Parent.Enabled = true;
                            btnCreateNewCube.Enabled = true;

                        }
                        else
                        {
                            _testerTimer.Interval = interval;
                            _testerTimer.SourceObject = "grpCube";
                            _testerTimer.Start();
                            System.Windows.Forms.Application.DoEvents();

                        }
                        break;
                    }
                case "grpBook":
                    {
                        if (stepStatus == StepStatus.Ready)
                        {
                            // grpBook.Enabled = true;



                        }
                        else
                        {
                            _testerTimer.Interval = interval;
                            _testerTimer.SourceObject = "grpBook";
                            _testerTimer.Start();
                            System.Windows.Forms.Application.DoEvents();

                        }
                        break;
                    }
                
            }
        }



        private void frmWizardTester_Load(object sender, EventArgs e)
        {


            txtDataSourceName.Text = "From app.config of the service"; //Properties.Settings.Default.AnalysisServerConnectionString;
            txtDatabseName.Text = "From app.config of the service"; //Properties.Settings.Default.AnalysisServerDatabase;
            txtRoleMemberName.Text = Properties.Settings.Default.RoleMemberName;
            //txtCubeName.Text = Properties.Settings.Default.CubeNameStartWith;
            txtCubeTemplateID.Text = "From app.config of the service"; //Properties.Settings.Default.CubeTemplatesBOTemplate;

            //txtBookName.Text = Properties.Settings.Default.BookName;
            
            //txtReplaceWithCubeName.Text = Properties.Settings.Default.CubeNameStartWith;
            _measures = new Dictionary<string, object>();
            _BEDataValues = new Dictionary<string, object>();
            InitLaizeComboBoxs();
            _testerTimer.Interval = 9000;
            _testerTimer.SourceObject = "FormLoad";
            _testerTimer.Start();


            System.Windows.Forms.Application.DoEvents();














        }
        private void InitLaizeComboBoxs()
        {
            //Clean the combo boxs

            cmbBeData.DataSource = null;


            //Load default values from app.config
            string[] googleDefaultValues = Properties.Settings.Default.GoogleValues.Split(',');

            foreach (string str in googleDefaultValues)
            {

                if (!_BEDataValues.ContainsKey("AccountSettings." + str))
                {
                    _BEDataValues.Add("AccountSettings." + str, str);
                }

            }

            _BEDataValues.Add("BEData", "BEData");
            string[] CPADefaultValues = Properties.Settings.Default.CPAValues.Split(',');
            foreach (string str in CPADefaultValues)
            {
                if (!_CPAData.ContainsKey("AccountSettings." + str))
                {
                    _CPAData.Add("AccountSettings." + str, str);
                }
            }


            cmbBeData.DataSource = _BEDataValues.ToList();
            cmbBeData.ValueMember = "Key";
            cmbBeData.DisplayMember = "Value";

            cmbCPA.DataSource = _CPAData.ToList();
            cmbCPA.ValueMember = "Key";
            cmbCPA.DisplayMember = "Value";


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
            




        }

        private void frmWizardTester_FormClosing(object sender, FormClosingEventArgs e)
        {
            //try
            //{
            //    edgeHostService.Kill();
            //}
            //catch (Exception)
            //{


            //}

        }

        private void btnStartNewSession_Click(object sender, EventArgs e)
        {
            foreach (Control control in this.Controls)
            {
                if (control.GetType() == typeof(GroupBox))
                {
                    if (control.Name != "grpCube")
                    {
                        control.Enabled = false;
                    }
                }
            }
            btnGetSummary.Enabled = false;
            btnExecute.Enabled = false;
            btnCreateNewCube.Enabled = false;
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

                    _testerTimer.Interval = interval;
                    _testerTimer.SourceObject = "grpBoxActiveDirectory";
                    _testerTimer.Start();
                    System.Windows.Forms.Application.DoEvents();

                }
            }
            else
            {
                MessageBox.Show("Please fill scopeid and scopename");
                txtScopeID.Enabled = true;
                txtScopName.Enabled = true;
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
            catch (Exception ex)
            {
                return new WizardSession() { CurrentStep = new StepConfiguration() { StepName = string.Format("Service did not started yet:{0}", ex.Message) }, SessionID = -1, WizardID = -1 };

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
            collectedValues.Add("AccountSettings.BI_Scope_ID", txtScopeID.Text.Trim());
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

                                    txtLog.Text = string.Format("Step Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
                                    _nextStepName = stepCollectResponse.NextStep.StepName;

                                    ((Control)sender).Parent.Enabled = false;
                                    _testerTimer.Interval = interval;
                                    _testerTimer.SourceObject = "grpRole";
                                    _testerTimer.Start();
                                    System.Windows.Forms.Application.DoEvents();


                                    break;
                                }
                            case StepResult.HasErrors:
                                {
                                    foreach (KeyValuePair<string, string> error in stepCollectResponse.Errors)
                                    {
                                        txtLog.Text = string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value);
                                    }


                                    break;
                                }
                            case StepResult.Done:
                                {
                                    txtLog.Text = "Finsish collecting from all steps\nready to get summary or execute:";

                                    break;
                                }
                        }



                    }
                    catch (WebException ex)
                    {

                        using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                        {

                            txtLog.Text = string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
                        }

                    }
                }
                catch (Exception ex)
                {

                    txtLog.Text = ex.Message;
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
            collectedValues.Add("AccountSettings.BI_Scope_ID", txtScopeID.Text.Trim());
            collectedValues.Add("AccountSettings.RoleName", txtRoleName.Text.Trim());
            collectedValues.Add("AccountSettings.RoleID", txtRoleID.Text.Trim());
            if (!string.IsNullOrEmpty(txtRoleMemberName.Text.Trim()))
            {
                collectedValues.Add("AccountSettings.RoleMemberName", txtRoleMemberName.Text.Trim());
                
            }
           
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

                                    txtLog.Text = string.Format("Step Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
                                    _nextStepName = stepCollectResponse.NextStep.StepName;

                                    ((Control)sender).Parent.Enabled = false;
                                    _testerTimer.Interval = interval;
                                    _testerTimer.SourceObject = "grpCube";
                                    _testerTimer.Start();
                                    System.Windows.Forms.Application.DoEvents();

                                    break;
                                }
                            case StepResult.HasErrors:
                                {
                                    foreach (KeyValuePair<string, string> error in stepCollectResponse.Errors)
                                    {
                                        txtLog.Text = string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value);
                                    }


                                    break;
                                }
                            case StepResult.Done:
                                {
                                    

                                    break;
                                }
                        }



                    }
                    catch (WebException ex)
                    {

                        using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                        {

                            txtLog.Text = string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
                        }

                    }
                }
                catch (Exception ex)
                {

                    txtLog.Text = ex.Message;
                }

            }
        }

        private void btnCreateNewCube_Click(object sender, EventArgs e)
        {
            //TODO FOR YARON CHECK THAT THE USER IS NOT PASSING THE SAME REPLACEMT TWICE
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
            // CPA
            bool onlyCalc;
            foreach (ListViewItem item in livCPA.Items)
            {
                switch (item.Text)
                {


                    case "CPA1":
                        {
                            if (item.SubItems[2].Text == "Y")
                                onlyCalc = true;
                            else
                                onlyCalc = false;
                            if (!collectedValues.ContainsKey("AccountSettings.New Users"))
                            {
                                collectedValues.Add("AccountSettings.New Users", new Replacment() { ReplaceFrom = "New Users", ReplaceTo = item.SubItems[1].Text, CalcMembersOnly = onlyCalc });
                            }                            
                            break;
                        }
                    case "CPA2":
                        {
                            if (item.SubItems[2].Text == "Y")
                                onlyCalc = true;
                            else
                                onlyCalc = false;
                            if (!collectedValues.ContainsKey("AccountSettings.New Active Users"))
                            {
                                collectedValues.Add("AccountSettings.New Active Users", new Replacment() { ReplaceFrom = "New Active Users", ReplaceTo = item.SubItems[1].Text, CalcMembersOnly = onlyCalc });
                            }
                            break;

                        }
                }

            }
            // Measures/Client specific (Be data + string replacement)
            foreach (ListViewItem item in listViewBeData.Items)
            {
                if (item.SubItems[2].Text == "Y")
                    onlyCalc = true;
                else
                    onlyCalc = false;
                if (item.Text.StartsWith("Client Specific"))
                {
                    if (!collectedValues.ContainsKey("AccountSettings." + item.Text))
                    {

                        collectedValues.Add("AccountSettings." + item.Text, new Replacment() { ReplaceFrom = item.Text, ReplaceTo = item.SubItems[1].Text, CalcMembersOnly = onlyCalc });
                    }

                }
                else
                {
                    if (!collectedValues.ContainsKey("AccountSettings.StringReplacment." + item.Text))
                    {
                        collectedValues.Add("AccountSettings.StringReplacment." + item.Text, new Replacment() { ReplaceFrom = item.Text, ReplaceTo = item.SubItems[1].Text, CalcMembersOnly = onlyCalc });

                    }

                }


            }


            //Only string replacement
            foreach (ListViewItem item in livStrings.Items)
            {
                if (item.SubItems[2].Text == "Y")
                    onlyCalc = true;
                else
                    onlyCalc = false;

                if (!collectedValues.ContainsKey("AccountSettings.StringReplacment." + item.Text))
                {

                    collectedValues.Add("AccountSettings.StringReplacment." + item.Text, new Replacment() { ReplaceFrom = item.Text, ReplaceTo = item.SubItems[1].Text, CalcMembersOnly = onlyCalc });
                }


            }



            collectedValues.Add("AccountSettings.BI_Scope_ID", txtScopeID.Text.Trim());
            collectedValues.Add("AccountSettings.CubeName", txtCubeName.Text.Trim());
            collectedValues.Add("AccountSettings.CubeID", txtCubeName.Text.Trim());





            if (chkContent.Checked)
                collectedValues.Add("AccountSettings.AddContentCube", bool.Parse("true"));
            else
                collectedValues.Add("AccountSettings.AddContentCube", bool.Parse("false"));

            if (chkProcessCubes.Checked)
                collectedValues.Add("AccountSettings.ProcessCubes", bool.Parse("true"));
            else
                collectedValues.Add("AccountSettings.ProcessCubes", bool.Parse("false"));
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

                                    txtLog.Text = string.Format("Step Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
                                    _nextStepName = stepCollectResponse.NextStep.StepName;

                                    btnCreateNewCube.Enabled = false;
                                    _testerTimer.Interval = interval;
                                    _testerTimer.SourceObject = "grpBook";
                                    _testerTimer.Start();
                                    System.Windows.Forms.Application.DoEvents();



                                    break;
                                }
                            case StepResult.HasErrors:
                                {
                                    txtLog.Text = string.Empty;
                                    foreach (KeyValuePair<string, string> error in stepCollectResponse.Errors)
                                    {
                                        txtLog.Text += string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value);
                                    }


                                    break;
                                }
                            case StepResult.Done:
                                {
                                    txtLog.Text = "Finsish collecting from all steps\nready to get summary or execute:";
                                   
                                    btnCreateNewCube.Enabled = false;
                                    System.Threading.Thread.Sleep(6000);
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

                            txtLog.Text = string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
                        }

                    }
                }
                catch (Exception ex)
                {

                    txtLog.Text = ex.Message;
                }

            }

        }

        private void button4_Click(object sender, EventArgs e)
        {

            FrmMeasuresList frmMeasuresList = new FrmMeasuresList(_measures);
            frmMeasuresList.ShowDialog();
            _measures = frmMeasuresList.Measures;
            //_measures.Add("0", "0");
            InitLaizeComboBoxs();

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

                                    txtLog.Text = string.Format("Step Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
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
                                        txtLog.Text = string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value);
                                    }


                                    break;
                                }
                            case StepResult.Done:
                                {
                                    txtLog.Text = "Finsish collecting from all steps\nready to get summary or execute:";

                                    break;
                                }
                        }



                    }
                    catch (WebException ex)
                    {

                        using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                        {

                            txtLog.Text = string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
                        }

                    }
                }
                catch (Exception ex)
                {

                    txtLog.Text = ex.Message;
                }

            }
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            foreach (Control control in this.Controls)
            {
                if (control.GetType() == typeof(GroupBox))
                {
                    if (control.Name != "grpCube")
                    {
                        control.Enabled = false;
                    }
                }
            }
            try
            {
                WebRequest request = HttpWebRequest.Create(string.Format("{0}/execute?sessionID={1}", _baseUri, _sessionID));
                request.Method = "POST";
                request.ContentType = "application/xml";
                request.ContentLength = 0;
                request.Timeout = 200000;
                
                WebResponse response = request.GetResponse();
              
                _testerTimer.Interval = 10000;
                _testerTimer.SourceObject = "Execute";
                _testerTimer.Start();
                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                
                txtLog.Text = ex.Message;
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
            //  collectedValues.Add("AccountSettings.CubeName", txtReplaceWithCubeName.Text);

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

                                    txtLog.Text += string.Format("Step Collect Finish\nReady for next step: Step {0}", stepCollectResponse.NextStep.StepName);
                                    _nextStepName = stepCollectResponse.NextStep.StepName;
                                    btnExecute.Enabled = true;
                                    ((Control)sender).Parent.Enabled = false;


                                    break;
                                }
                            case StepResult.HasErrors:
                                {
                                    foreach (KeyValuePair<string, string> error in stepCollectResponse.Errors)
                                    {
                                       
                                        txtLog.Text = string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value);
                                    }


                                    break;
                                }
                            case StepResult.Done:
                                {
                                    
                                    txtLog.Text = "Finsish collecting from all steps\nready to get summary or execute:";
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
                            
                            txtLog.Text = string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
                        }

                    }
                }
                catch (Exception ex)
                {
                   
                    txtLog.Text = ex.Message;
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

        //private void IfGoogleValueSelected(object sender)
        //{
        //    ComboBox selectedComboBox = (ComboBox)sender;
        //    bool isSelected = IsgooleValueSelected(selectedComboBox);
        //    CheckBox selectedCheckBox = null;
        //    switch (selectedComboBox.Name)
        //    {
        //        case "cmbReplaceCPA1":
        //            {
        //                selectedCheckBox = this.chkCpa1;
        //                break;
        //            }
        //        case "cmbReplaceCPA2":
        //            {
        //                selectedCheckBox = this.chkCpa2;
        //                break;
        //            }

        //    }
        //    if (isSelected)
        //    {
        //        selectedCheckBox.Checked = isSelected;
        //        selectedCheckBox.Enabled = false;
        //    }
        //    else
        //        selectedCheckBox.Enabled = true;

        //}
        private bool IsgooleValueSelected(ComboBox sender)
        {
            string[] comboValues = Properties.Settings.Default.GoogleValues.Split(',');


            bool isSelected = false;


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

        private void btnAddBe_Click(object sender, EventArgs e)
        {
            ListViewItem liv = null;

            string calcOnly = string.Empty; ;
            if (chkBeOnlyCalc.Checked)
                calcOnly = "Y";
            else
                calcOnly = "N";
            switch (cmbBeData.Text)
            {

                case "BEData":
                    {
                        int i = 1;
                        string patern = @"\bClient Specific";
                        foreach (ListViewItem item in listViewBeData.Items)
                        {
                            if (Regex.IsMatch(item.Text, patern))
                            {
                                i++;
                            }
                        }

                        liv = new ListViewItem(new string[] { string.Format("Client Specific{0}", i), txtBeReplace.Text, calcOnly });
                        listViewBeData.Items.Add(liv);
                        break;
                    }
                case " ":
                    {
                        //Do Nothing
                        break;
                    }
                default:
                    {
                        bool exist = false;
                        liv = new ListViewItem(new string[] { cmbBeData.Text, txtBeReplace.Text, calcOnly });
                        foreach (ListViewItem item in listViewBeData.Items)
                        {
                            if (item.SubItems[0].Text.Trim() == liv.Text)
                            {
                                exist = true;
                                MessageBox.Show("Two items can't be with the same name");
                            }
                           
                            
                        }
                        if (!exist)                       
                            listViewBeData.Items.Add(liv);
                        

                        break;
                    }
            }
        }

        private void btnClearBeData_Click(object sender, EventArgs e)
        {
            listViewBeData.Items.Clear();
        }

        private void btnRemoveBeData_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewBeData.SelectedItems)
            {
                item.Remove();

            }
        }

        private void btnAddCPA_Click(object sender, EventArgs e)
        {
            ListViewItem liv = null;

            string calcOnly = string.Empty; ;
            if (chkOnlyCalcCPA.Checked)
                calcOnly = "Y";
            else
                calcOnly = "N";

            bool exist = false;
            liv = new ListViewItem(new string[] { cmbCPA.Text, cmbCpaReplaceTo.Text, calcOnly });
            foreach (ListViewItem item in livCPA.Items)
            {
                if (item.SubItems[0].Text.Trim() == liv.Text)
                {
                    exist = true;
                    MessageBox.Show("Two items can't be with the same name");
                }
               

            }
            if (!exist)
                livCPA.Items.Add(liv);
                        
           

          




        }

        private void btnClearCPA_Click(object sender, EventArgs e)
        {
            livCPA.Items.Clear();
        }

        private void btnRemoveCpa_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in livCPA.SelectedItems)
            {
                item.Remove();

            }

        }

        private void btnAddStringReplace_Click(object sender, EventArgs e)
        {
            ListViewItem liv = null;

            string calcOnly = string.Empty; ;
            if (chkCalcOnlyString.Checked)
                calcOnly = "Y";
            else
                calcOnly = "N";

            bool exist = false;
            liv = new ListViewItem(new string[] { txtString.Text, txtStringReplace.Text, calcOnly });
            foreach (ListViewItem item in livStrings.Items)
            {
                if (item.SubItems[0].Text.Trim() == liv.Text)
                {
                    exist = true;
                    MessageBox.Show("Two items can't be with the same name");
                }


            }
            if (!exist)
                livStrings.Items.Add(liv);
               
                        
            

           
        }

        private void btnRemoveStringReplace_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in livStrings.SelectedItems)
            {
                item.Remove();

            }
        }

        private void btnClearStrings_Click(object sender, EventArgs e)
        {
            livStrings.Items.Clear();
        }

        private void tabsCreateCube_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tabsCreateCube_TabIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void FillCPAReplaceComboBox()
        {
            cmbCpaReplaceTo.DataSource = null;
            List<string> comboValues = new List<string>();


            //Load default values from app.config
            string[] googleDefaultValues = Properties.Settings.Default.GoogleValues.Split(',');
           
            foreach (string str in googleDefaultValues)
            {

                string itemReplaced = str;
                foreach (ListViewItem item in listViewBeData.Items)
                {
                   
                    if (item.Text==str)
                    {
                        itemReplaced = item.SubItems[1].Text;
                        break;

                        
                    }
                }
                comboValues.Add(itemReplaced);

            }

            foreach (ListViewItem item in listViewBeData.Items)
            {
                if (item.SubItems[0].Text.StartsWith("Client Specific"))
                {
                    comboValues.Add(item.SubItems[1].Text.Trim());
                    
                }
            }



            cmbCpaReplaceTo.DataSource = comboValues;
            //cmbBeData.ValueMember = "Key";
            //cmbBeData.DisplayMember = "Value";

            //cmbCPA.DataSource = _CPAData.ToList();
            //cmbCPA.ValueMember = "Key";
            //cmbCPA.DisplayMember = "Value";
        }

        private void tabsCreateCube_Selected(object sender, TabControlEventArgs e)
        {

            if (e.TabPage.Name == "tabCPAS")
            {
                FillCPAReplaceComboBox();
            }
        }

        private void cmbBeData_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cmbBeData.Text=="BEData")
            {
                chkBeOnlyCalc.Checked = false;
                
            }
           

        }

        private void txtScopeID_Validated(object sender, EventArgs e)
        {
            if (Regex.IsMatch(txtScopeID.Text.Trim(), @"\D"))
            {
                txtScopeID.Text = string.Empty;
                txtScopeID.Focus();

            }
           
            
        }

        private void txtScopName_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtScopeID_TextChanged(object sender, EventArgs e)
        {

        }
       

       

    }
    public class TesterTimer : System.Windows.Forms.Timer
    {
        public string SourceObject { get; set; }

    }
}
