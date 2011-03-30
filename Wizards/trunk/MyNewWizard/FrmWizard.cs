using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Runtime.Serialization;
using System.Xml;
using EdgeBI.Wizards;
using WizardForm;
using System.IO;
using System.Text.RegularExpressions;

namespace MyNewWizard
{
    public partial class FrmWizard : Form
    {
        Uri _baseUri = new Uri("http://localhost:8080/wizard");
        public static AccountWizardSettings accountWizardSettings;
        public static Dictionary<string, object> AllCollectedValues = new Dictionary<string, object>();
        protected string _nextStep;
        protected string _previousStep;
        protected WebRequest _collectRequest;
        public string StrSummary;
        public Dictionary<string, object> StartData;
        protected TesterTimer _serviceStateTimer;
        public Uri BaseUri
        {
            get { return _baseUri; }

        }
        protected Dictionary<string, WizardPage> _wizardPages;
        private int _session;
        public static string GetFromWizardSettings(string settingName)
        {
            if (accountWizardSettings.Count == 0)
                throw new Exception("Setting is not initalize!");
            return accountWizardSettings.Get(settingName);
        }
        public static int GetCountLikeSettings(string likeSettings)
        {
            return accountWizardSettings.GetLikeSettings(likeSettings).Count;
        }
        public int Session
        {
            get { return _session; }

        }

        public FrmWizard()
        {
            InitializeComponent();
            _wizardPages = new Dictionary<string, WizardPage>();
            _serviceStateTimer = new TesterTimer();
            _serviceStateTimer.Tick += new EventHandler(CheckServiceStateTimer_Tick);

        }
        public StepStatus GetStepStatus()
        {
            StepStatus stepStatus = StepStatus.NotReady;
            try
            {
                WebRequest request = HttpWebRequest.Create(string.Format("{0}/GetStepState?sessionID={1}", _baseUri, Session));
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

        void CheckServiceStateTimer_Tick(object sender, EventArgs e)
        {
            _serviceStateTimer.Stop();
            btnNewSession.Enabled = true;


        }
        public void AddPage(WizardPage step)
        {
            if (_wizardPages.ContainsKey(step.StepName))
            {
                throw new Exception("Wizard already contain this step");
            }
            else
            {
                this.mainPanel.Controls.Add(step);
                step.Visible = true;


                step.Dock = DockStyle.Fill;

                step.Visible = false;
                _wizardPages.Add(step.StepName, step);
            }
        }


        private void ActivateStep(string stepToActivate)
        {
            _previousStep = _nextStep;
            _nextStep = stepToActivate;
            ActivateStep();
        }
        private void ActivateStep()
        {
            if (_previousStep != null)
                _wizardPages[_previousStep].Visible = false;
            _wizardPages[_nextStep].Visible = true;
            _previousStep = _nextStep;


        }
        private bool IsStepActive(string stepName)
        {
            bool isActive = false;
            if (_wizardPages[stepName].Visible == true)
                isActive = true;
            else
                isActive = false;
            return isActive;
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
                _session = wizardSession.SessionID;
                _nextStep = wizardSession.CurrentStep.StepName;
                _previousStep = "Start";




            }
            catch (Exception ex)
            {
                return new WizardSession() { CurrentStep = new StepConfiguration() { StepName = string.Format("Service did not started yet:{0}", ex.Message) }, SessionID = -1, WizardID = -1 };

            }
            return wizardSession;

        }

        private void btnNewSession_Click(object sender, EventArgs e)
        {
            try
            {
                if (!IsStepActive("Start"))
                {
                    ActivateStep("Start");
                    btnNewSession.Enabled = true;

                }
                else
                {



                    WizardSession wizardSession = StartWizard("AccountWizard");
                    _session = wizardSession.SessionID;
                    _previousStep = "Start";
                    _nextStep = wizardSession.CurrentStep.StepName;
                    WizardPage p = _wizardPages[_previousStep];
                    // StartData = p.CollectValues(); this old before add account step , no values to collect now
                    //if (Regex.IsMatch(StartData["AccountSettings.BI_Scope_ID"].ToString().Trim(), @"\D") || string.IsNullOrEmpty(StartData["AccountSettings.BI_Scope_ID"].ToString()))
                    //{
                    //    MessageBox.Show("Scope ID IS NOT A NUMBER");

                    //}
                    //else
                    ActivateStep();


                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }

        private void FrmWizard_Load(object sender, EventArgs e)
        {
            ActivateStep("Start");
            _serviceStateTimer.Interval = 9000;
            _serviceStateTimer.Start();

        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            try
            {
                if (Validataions())
                {
                    if (IsStepActive(_nextStep))
                    {
                        WizardPage activePage = _wizardPages[_nextStep];
                        if (activePage == null)
                            throw new Exception("Page not active");
                        Dictionary<string, object> collectedValues = activePage.CollectValues();
                        if (!collectedValues.ContainsKey("AccountSettings.ApplicationID"))
                            collectedValues.Add("AccountSettings.ApplicationID", AllCollectedValues["AccountSettings.ApplicationID"]);
                        foreach (KeyValuePair<string, object> item in collectedValues)
                        {
                            if (!AllCollectedValues.ContainsKey(item.Key))
                                AllCollectedValues.Add(item.Key, item.Value);


                        }
                        StepCollectResponse stepCollectResponse = Collect(collectedValues);
                        switch (stepCollectResponse.Result)
                        {
                            case StepResult.Next:
                                {
                                    _nextStep = stepCollectResponse.NextStep.StepName;
                                    ActivateStep();
                                    break;
                                }
                            case StepResult.Done:
                                {
                                    GetSummary();
                                    ActivateStep("Summary");

                                    break;
                                }
                            case StepResult.HasErrors:
                                {
                                    StringBuilder str = new StringBuilder();
                                    foreach (KeyValuePair<string, string> error in stepCollectResponse.Errors)
                                    {
                                        str.AppendLine(string.Format("Errors:key {0} Value{1}\n", error.Key, error.Value));
                                    }
                                    MessageBox.Show(str.ToString());

                                    break;
                                }
                            default:
                                break;
                        }


                    }
                    else
                        MessageBox.Show("Not Active");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private bool Validataions()
        {
            bool pass = true;
            string message=string.Empty;
            switch (_previousStep)
            {
                case "CreateNewAccountStepCollector":
                    {
                        CreateAccount frm = (CreateAccount)_wizardPages["CreateNewAccountStepCollector"];
                        if (!frm.ValidateTxtBiScopeName())
                        {
                            pass = false;
                            message = "ScopeName must be alpha numeric!";                           
                        }
                        break;
                    }
            }
            if (pass == false)
                MessageBox.Show(message);
            return pass;
        }

        

        private void GetSummary()
        {
            try
            {
                WebRequest request = HttpWebRequest.Create(string.Format("{0}/summary?sessionID={1}", _baseUri, _session));
                request.Method = "POST";
                request.ContentType = "application/xml";
                request.ContentLength = 0;
                WebResponse response = request.GetResponse();
                XmlDocument SummaryXml = new XmlDocument();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {

                    SummaryXml.LoadXml(reader.ReadToEnd());
                    StrSummary = SummaryXml.DocumentElement.InnerText;

                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private StepCollectResponse Collect(Dictionary<string, object> collectedValues)
        {
            StepCollectResponse stepCollectResponse;
            WebRequest request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", _baseUri, _session));
            request.ContentType = "application/xml";
            request.Method = "POST";
            request.Timeout = 130000;
            StepCollectRequest stepCollectRequest = new StepCollectRequest();
            stepCollectRequest.StepName = _nextStep;
            stepCollectRequest.CollectedValues = collectedValues;
            try
            {
                SetBodyForCollectRequest(ref request, stepCollectRequest);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.YesNo);
            }
            WebResponse response = request.GetResponse();
            DataContractSerializer serializer = new DataContractSerializer(typeof(StepCollectResponse));
            using (XmlReader reader = XmlReader.Create(response.GetResponseStream()))
            {
                stepCollectResponse = (StepCollectResponse)serializer.ReadObject(reader, false);

            }
            return stepCollectResponse;

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

        private void btnFinish_Click(object sender, EventArgs e)
        {
            WebRequest request = HttpWebRequest.Create(string.Format("{0}/execute?sessionID={1}", _baseUri, _session));
            request.Method = "POST";
            request.ContentType = "application/xml";
            request.ContentLength = 0;
            request.Timeout = 200000;

            WebResponse response = request.GetResponse();
            ActivateStep("Execute");

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {

            Previous();
        }

        private void Previous()
        {
            StepCollectResponse stepCollectResponse;
            WebRequest request = HttpWebRequest.Create(string.Format("{0}/Previous?sessionID={1}", _baseUri, _session));
            request.ContentType = "application/xml";
            request.Method = "POST";
            request.Timeout = 130000;
            StepCollectRequest stepCollectRequest = new StepCollectRequest();
            stepCollectRequest.StepName = _nextStep;

            try
            {
                SetBodyForCollectRequest(ref request, stepCollectRequest);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.YesNo);
            }
            WebResponse response = request.GetResponse();
        }










    }
    public class TesterTimer : System.Windows.Forms.Timer
    {
        public string SourceObject { get; set; }

    }

}
