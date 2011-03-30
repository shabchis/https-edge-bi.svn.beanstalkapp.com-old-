using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EdgeBI.Wizards;

namespace MyNewWizard
{
    public partial class ActiveDirectoryStep : WizardForm.WizardPage
    {
        const string password = "Edge1stpass@";
        FrmWizard _parentForm;
        public ActiveDirectoryStep()
        {
            InitializeComponent();
            stepReadyTimer.Tick += new EventHandler(stepReadyTimer_Tick);
        }

        void stepReadyTimer_Tick(object sender, EventArgs e)
        {
            stepReadyTimer.Stop();
           StepStatus stepStatus=  _parentForm.GetStepStatus();
           if (stepStatus == StepStatus.Ready)
           {
               _parentForm.Controls["btnNext"].Visible = true;
               _parentForm.Controls["btnNext"].Enabled = true;
               _parentForm.Controls["btnNewSession"].Enabled = true;

           }
           else
           {
               stepReadyTimer.Interval = interval;
               stepReadyTimer.Start();
               System.Windows.Forms.Application.DoEvents();

           }
        }

        private void ActiveDirectoryStep_Load(object sender, EventArgs e)
        {


        }
        public override Dictionary<string, object> CollectValues()
        {
            Dictionary<string, object> activeDirectoryValues = new Dictionary<string, object>();
           
            activeDirectoryValues.Add("ActiveDirectory.UserName", txtUserName.Text.Trim());
            activeDirectoryValues.Add("ActiveDirectory.Password", txtPassword.Text.Trim());
            activeDirectoryValues.Add("ActiveDirectory.FullName", txtFullName.Text.Trim());

            return activeDirectoryValues;
        }
        protected override void InitalizePage()
        {

            _parentForm = (FrmWizard)ParentForm;
            _parentForm.Text = this.StepDescription;
            txtUserName.Text = FrmWizard.AllCollectedValues["AccountSettings.BI_Scope_Name"].ToString();
            txtFullName.Text = FrmWizard.AllCollectedValues["AccountSettings.BI_Scope_Name"].ToString();
            txtPassword.Text = password;
            stepReadyTimer.Interval = interval; 
            stepReadyTimer.Start();
          
        }

        private void ActiveDirectoryStep_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == true)
                InitalizePage();
            else
                base.SetButtons();
        }
    }
}
