using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using EdgeBI.Wizards;
using Easynet.Edge.Core.Configuration;
namespace MyNewWizard
{
    public partial class CreateAccount : WizardForm.WizardPage
    {
        FrmWizard _parentForm;
        public CreateAccount()
        {
            InitializeComponent();
            stepReadyTimer.Tick += new EventHandler(stepReadyTimer_Tick);
        }
        public override Dictionary<string, object> CollectValues()
        {

            Dictionary<string, object> AccountData = new Dictionary<string, object>();
            AccountData.Add("AccountSettings.ApplicationID", cmbApplication.SelectedValue);
            AccountData.Add("AccountSettings.BI_Scope_ID", txtBIScopeID.Text.Trim());
            AccountData.Add("AccountSettings.BI_Scope_Name", txtBIScopName.Text.Trim());
            AccountData.Add("AccountSettings.CreateAccount", chkCreateAccount.Checked);
            AccountData.Add("AccountSettings.AccountName", txtAccountName.Text.Trim());
            AccountData.Add("AccountSettings.AccountID", txtAccountID.Text.Trim());
            AccountData.Add("AccountSettings.ClientName", txtClientName.Text.Trim());
            AccountData.Add("AccountSettings.ClientID", txtClientID.Text.Trim());
            AccountData.Add("AccountSettings.ScopeName", txtScopeName.Text.Trim());
            AccountData.Add("AccountSettings.ScopeID", txtScopeID.Text.Trim());


            return AccountData;
        }
        private void txtScopName_Validated(object sender, EventArgs e)
        {
            if (Regex.IsMatch(txtBIScopeID.Text.Trim(), @"\D"))
            {
                txtBIScopeID.Text = string.Empty;
                txtBIScopeID.Focus();

            }
        }
        private void txtScopeID_Validated(object sender, EventArgs e)
        {
            if (Regex.IsMatch(txtBIScopeID.Text.Trim(), @"\D"))
            {
                txtBIScopeID.Text = string.Empty;
                txtBIScopeID.Focus();

            }
        }
        protected override void InitalizePage()
        {
            _parentForm = (FrmWizard)ParentForm;
            _parentForm.Text = this.StepDescription;
            _parentForm.Controls["btnNewSession"].Visible = true;
            _parentForm.Controls["btnNewSession"].Enabled = true;

            stepReadyTimer.Interval = 9000; ;
            stepReadyTimer.Start();

            FillPageValues();


        }

        private void FillPageValues()
        {

            fillApplicationComboBox();

        }

        private void fillIDS()
        {

            if (chkCreateAccount.Checked)
            {
                using (SqlConnection sqlConnection = new SqlConnection(FrmWizard.GetFromWizardSettings("OLTP.Connection.string")))
                {
                    sqlConnection.Open();
                    using (SqlCommand sqlCommand = new SqlCommand("SELECT (MAX(Account_ID)+1) FROM User_GUI_Account", sqlConnection))
                    {
                        txtAccountID.Text = sqlCommand.ExecuteScalar().ToString();

                    }
                }
                FillClientID(int.Parse(txtAccountID.Text));
                FillScopeID(int.Parse(txtAccountID.Text));
            }

        }

        private void FillScopeID(int accountID)
        {
            txtScopeID.Text = (accountID + 1000000).ToString();
        }

        private void FillClientID(int accountID)
        {
            if (!string.IsNullOrEmpty(txtClientName.Text))
            {
                if (txtClientName.Text == txtAccountName.Text)
                    txtClientID.Text = accountID.ToString();
                else
                    txtClientID.Text = (accountID + 100000).ToString();

            }
        }

        private void fillApplicationComboBox()
        {
            if (cmbApplication.Items.Count == 0)
            {
                List<ApplicationType> apllicationsList = null;
                using (SqlConnection sqlConnection = new SqlConnection(AppSettings.GetAbsolute("Easynet.Edge.Core.Data.DataManager.Connection.String")))
                {
                    sqlConnection.Open();
                    using (SqlCommand sqlCommand = new SqlCommand("SELECT ID, ApplicationName FROM dbo.Account_Wizard_Applications", sqlConnection))
                    {
                        using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                        {


                            apllicationsList = new List<ApplicationType>();
                            apllicationsList.Add(new ApplicationType() { ID = 0, ApplicationName = string.Empty });

                            while (sqlDataReader.Read())
                            {
                                apllicationsList.Add(new ApplicationType() { ID = Convert.ToInt32(sqlDataReader["ID"]), ApplicationName = sqlDataReader["ApplicationName"].ToString() });
                            }


                        }
                    }
                }

                cmbApplication.DataSource = apllicationsList;
                cmbApplication.DisplayMember = "ApplicationName";
                cmbApplication.ValueMember = "ID";
                cmbApplication.Refresh();
            }
        }

        private void CreateAccount_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == true)
                InitalizePage();
            else
                base.SetButtons();
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

        private void chkCreateAccount_CheckedChanged(object sender, EventArgs e)
        {
            if (chkCreateAccount.Checked)
            {
                grpBI.Enabled = false;
                grpCreateAccount.Enabled = true;
            }
            else
            {
                grpBI.Enabled = true;
                grpCreateAccount.Enabled = false;
            }
        }



        private void cmbApplication_SelectionChangeCommitted(object sender, EventArgs e)
        {
            int applicationID = Convert.ToInt32(cmbApplication.SelectedValue);
            if (applicationID != 0)
            {
                try
                {
                    FrmWizard.accountWizardSettings = new AccountWizardSettings(applicationID);
                    fillIDS();
                    FillBiFields();
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message); ;
                }
            }
        }

        private void cmbApplication_DataSourceChanged(object sender, EventArgs e)
        {

        }

        private void cmbApplication_BindingContextChanged(object sender, EventArgs e)
        {
            if (cmbApplication.SelectedValue != null)
            {
                int applicationID = Convert.ToInt32(cmbApplication.SelectedValue);
                if (applicationID != 0)
                {
                    try
                    {
                        FrmWizard.accountWizardSettings = new AccountWizardSettings(applicationID);
                        fillIDS();
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show(ex.Message); ;
                    }

                }
            }
        }

        private void txtAccountName_Validated(object sender, EventArgs e)
        {
            if (cmbApplication.SelectedValue.ToString() != "0")
                fillIDS();
        }

        private void txtClientName_Validated(object sender, EventArgs e)
        {
            if (cmbApplication.SelectedValue.ToString() != "0")
                fillIDS();
        }

        private void txtScopeName_Validated(object sender, EventArgs e)
        {

            if (cmbApplication.SelectedValue.ToString() != "0")
            {
                fillIDS();
                FillBiFields();
            }
        }
        public bool ValidateTxtBiScopeName()
        {
            bool ok = true;
            Regex r = new Regex("^[a-zA-Z0-9]*$");
            if (!r.IsMatch(txtBIScopName.Text.Trim()))
            {
                ok = false;
            }
            return ok;

        }
        private void FillBiFields()
        {
            txtBIScopeID.Text = txtScopeID.Text;
            txtBIScopName.Text = txtScopeName.Text;
        }

        private void cmbApplication_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


    }
}
