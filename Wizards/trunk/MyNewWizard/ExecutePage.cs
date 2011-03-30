using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EdgeBI.Wizards;
using System.Net;
using System.Runtime.Serialization;
using System.Xml;
using Easynet.Edge.Core.Services;
using System.IO;

namespace MyNewWizard
{
    public partial class ExecutePage : WizardForm.WizardPage
    {
        TesterTimer _executionTimer;
        FrmWizard _parentForm;
        string _lastText;
        public ExecutePage()
        {
            InitializeComponent();
            _executionTimer = new TesterTimer();
            _executionTimer.Tick += new EventHandler(_executionTimer_Tick);
        }

        void _executionTimer_Tick(object sender, EventArgs e)
        {
            GetExecutionProgress();
        }

        private void ExecutePage_Load(object sender, EventArgs e)
        {


        }

        private void GetExecutionProgress()
        {
            ServiceOutcome serviceOutcome;
           
            ProgressState progressState;
            WebResponse response = null;
            _executionTimer.Stop();
            WebRequest request = HttpWebRequest.Create(string.Format("{0}/Progress?sessionID={1}", _parentForm.BaseUri, _parentForm.Session));
            request.Timeout = 900000;

            try
            {
                response = request.GetResponse();
            }
            catch (Exception)
            {

                ///Read top 2 from log
                GetLog();
                progressExecute.Value = progressExecute.Maximum; ;

            }

            DataContractSerializer serializer = new DataContractSerializer(typeof(ProgressState));
            using (XmlReader reader = XmlReader.Create(response.GetResponseStream()))
            {

                progressState = (ProgressState)serializer.ReadObject(reader, false);
                if (!string.IsNullOrEmpty(progressState.text))
                {
                    if (_lastText != progressState.text)
                    {
                        txtLog.Text += string.Format("\n{0}", progressState.text);
                        txtLog.Select(txtLog.Text.Length - 1, 0);
                        txtLog.ScrollToCaret();
                    }
                    _lastText = progressState.text;
                }
                txtLog.Text += "...";
                progressExecute.Value = Convert.ToInt32((System.Math.Round((progressState.OverAllProgess * progressExecute.Maximum), 0)));

            }



            if (progressExecute.Value == progressExecute.Maximum)
            {
                progressExecute.Value = 0;

                request = HttpWebRequest.Create(string.Format("{0}/GetExecutorState?sessionID={1}", _parentForm.BaseUri, _parentForm.Session));

                try
                {
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
                                _executionTimer.Interval = 1500;
                                _executionTimer.SourceObject = "Execute";
                                _executionTimer.Start();
                                System.Windows.Forms.Application.DoEvents();
                                break;
                            }
                        case ServiceOutcome.Success:
                            {
                                MessageBox.Show("Wizard Finished Successfuly!");
                                progressExecute.Value = 0;
                                break;
                            }
                        case ServiceOutcome.Failure:
                            {
                                MessageBox.Show("Execution failed!");
                                GetLog();

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
                catch (Exception)
                {
                    progressExecute.Value = 0;
                    MessageBox.Show("Wizard Finished");
                }
            }
            else
            {
                _executionTimer.Interval = 3000;
                _executionTimer.SourceObject = "Execute";
                _executionTimer.Start();
                System.Windows.Forms.Application.DoEvents();

            }

        }

        private void GetLog()
        {
            WebResponse response = null;
            _executionTimer.Stop();
            WebRequest request = HttpWebRequest.Create(string.Format("{0}/GetErrors?sessionID={1}", _parentForm.BaseUri, _parentForm.Session));
            request.Timeout = 900000;

            try
            {
                response = request.GetResponse();
                DataContractSerializer serializer = new DataContractSerializer(typeof(string));
                using (XmlReader reader = XmlReader.Create(response.GetResponseStream()))
                {

                    txtLog.Text = (string)serializer.ReadObject(reader, false);

                }
            }
            catch (Exception)
            {



            }
        }
        protected override void InitalizePage()
        {
            _parentForm = (FrmWizard)ParentForm;
            _parentForm.Text = this.StepDescription;
            _parentForm.Controls["btnFinish"].Enabled = false;
            _parentForm.Controls["btnNewSession"].Enabled = true;
            _lastText = string.Empty;
            txtLog.Text = string.Empty;
            _executionTimer.Interval = 3000;
            _executionTimer.Start();
        }

        private void ExecutePage_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == true)
                InitalizePage();
            else
                base.SetButtons();
        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
