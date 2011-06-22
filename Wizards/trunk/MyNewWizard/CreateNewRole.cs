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
	public partial class CreateNewRole : WizardForm.WizardPage
	{
        FrmWizard _parentForm;
		public CreateNewRole()
		{
			InitializeComponent();
            stepReadyTimer.Tick += new EventHandler(stepReadyTimer_Tick);
		}

        void stepReadyTimer_Tick(object sender, EventArgs e)
        {
            stepReadyTimer.Stop();
            StepStatus stepStatus = _parentForm.GetStepStatus();
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
        public override Dictionary<string, object> CollectValues()
        {
            Dictionary<string, object> roleValues = new Dictionary<string, object>();

            if (((bool)FrmWizard.AllCollectedValues["AccountSettings.UseExistingRole"]) == false)
            {
                roleValues.Add("AccountSettings.UseExistingRole", false);
                roleValues.Add("AccountSettings.RoleName", txtRoleName.Text.Trim());
                roleValues.Add("AccountSettings.RoleID", txtRoleID.Text.Trim());
                if (!string.IsNullOrEmpty(txtRoleMemberName.Text.Trim()))
                {
                    roleValues.Add("AccountSettings.RoleMemberName", txtRoleMemberName.Text.Trim());
                }
            }
            else
                roleValues.Add("AccountSettings.UseExistingRole", true);
            return roleValues;
        }

        private void CreateNewRole_Load(object sender, EventArgs e)
        {
           
        }
        protected override void InitalizePage()
        {
            
            _parentForm = (FrmWizard)ParentForm;
            _parentForm.Text = this.StepDescription;
            if (((bool)FrmWizard.AllCollectedValues["AccountSettings.UseExistingRole"]) == true)
                grpRole.Enabled = false;
            else
            {
                grpRole.Enabled = true;
            txtRoleMemberName.Text = string.Format(@"EDGE\{0}", FrmWizard.AllCollectedValues["AccountSettings.BI_Scope_Name"]);
            txtRoleName.Text = string.Format(@"UDM {0}", FrmWizard.AllCollectedValues["AccountSettings.BI_Scope_Name"]);
            txtRoleID.Text = string.Format("Role {0}", FrmWizard.AllCollectedValues["AccountSettings.BI_Scope_ID"]);
                }
            
            stepReadyTimer.Interval = interval;
            stepReadyTimer.Start();
        }

        private void CreateNewRole_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == true)
                InitalizePage();
            else
                base.SetButtons();
           
                
        }

	}
}
