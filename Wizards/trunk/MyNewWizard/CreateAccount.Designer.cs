﻿namespace MyNewWizard
{
    partial class CreateAccount
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmbApplication = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBIScopeID = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.txtBIScopName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.grpBI = new System.Windows.Forms.GroupBox();
            this.chkUseExistingRole = new System.Windows.Forms.CheckBox();
            this.txtRole = new System.Windows.Forms.TextBox();
            this.lblRole = new System.Windows.Forms.Label();
            this.grpCreateAccount = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtScopeID = new System.Windows.Forms.TextBox();
            this.txtClientID = new System.Windows.Forms.TextBox();
            this.txtAccountID = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtScopeName = new System.Windows.Forms.TextBox();
            this.txtAccountName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtClientName = new System.Windows.Forms.TextBox();
            this.cmbWizardType = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.grpBI.SuspendLayout();
            this.grpCreateAccount.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmbApplication
            // 
            this.cmbApplication.FormattingEnabled = true;
            this.cmbApplication.Location = new System.Drawing.Point(109, 106);
            this.cmbApplication.Name = "cmbApplication";
            this.cmbApplication.Size = new System.Drawing.Size(186, 21);
            this.cmbApplication.TabIndex = 47;
            this.cmbApplication.SelectedIndexChanged += new System.EventHandler(this.cmbApplication_SelectedIndexChanged);
            this.cmbApplication.SelectionChangeCommitted += new System.EventHandler(this.cmbApplication_SelectionChangeCommitted);
            this.cmbApplication.DataSourceChanged += new System.EventHandler(this.cmbApplication_DataSourceChanged);
            this.cmbApplication.BindingContextChanged += new System.EventHandler(this.cmbApplication_BindingContextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 107);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 46;
            this.label1.Text = "Application";
            // 
            // txtBIScopeID
            // 
            this.txtBIScopeID.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtBIScopeID.Location = new System.Drawing.Point(101, 19);
            this.txtBIScopeID.Name = "txtBIScopeID";
            this.txtBIScopeID.Size = new System.Drawing.Size(186, 20);
            this.txtBIScopeID.TabIndex = 43;
            this.txtBIScopeID.Validated += new System.EventHandler(this.txtBIScopeID_Validated);
            // 
            // label16
            // 
            this.label16.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(10, 59);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(63, 13);
            this.label16.TabIndex = 44;
            this.label16.Text = "Book Name";
            // 
            // txtBIScopName
            // 
            this.txtBIScopName.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtBIScopName.Location = new System.Drawing.Point(101, 52);
            this.txtBIScopName.Name = "txtBIScopName";
            this.txtBIScopName.Size = new System.Drawing.Size(186, 20);
            this.txtBIScopName.TabIndex = 42;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 13);
            this.label6.TabIndex = 45;
            this.label6.Text = "BI Scope ID";
            // 
            // grpBI
            // 
            this.grpBI.Controls.Add(this.chkUseExistingRole);
            this.grpBI.Controls.Add(this.txtRole);
            this.grpBI.Controls.Add(this.lblRole);
            this.grpBI.Controls.Add(this.txtBIScopName);
            this.grpBI.Controls.Add(this.label6);
            this.grpBI.Controls.Add(this.label16);
            this.grpBI.Controls.Add(this.txtBIScopeID);
            this.grpBI.Enabled = false;
            this.grpBI.Location = new System.Drawing.Point(12, 131);
            this.grpBI.Name = "grpBI";
            this.grpBI.Size = new System.Drawing.Size(329, 241);
            this.grpBI.TabIndex = 48;
            this.grpBI.TabStop = false;
            this.grpBI.Text = "BI";
            // 
            // chkUseExistingRole
            // 
            this.chkUseExistingRole.AutoSize = true;
            this.chkUseExistingRole.Location = new System.Drawing.Point(215, 81);
            this.chkUseExistingRole.Name = "chkUseExistingRole";
            this.chkUseExistingRole.Size = new System.Drawing.Size(84, 17);
            this.chkUseExistingRole.TabIndex = 48;
            this.chkUseExistingRole.Text = "Use Existing";
            this.chkUseExistingRole.UseVisualStyleBackColor = true;
            this.chkUseExistingRole.Visible = false;
            // 
            // txtRole
            // 
            this.txtRole.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtRole.Location = new System.Drawing.Point(101, 81);
            this.txtRole.Name = "txtRole";
            this.txtRole.Size = new System.Drawing.Size(107, 20);
            this.txtRole.TabIndex = 46;
            this.txtRole.Visible = false;
            // 
            // lblRole
            // 
            this.lblRole.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblRole.AutoSize = true;
            this.lblRole.Location = new System.Drawing.Point(10, 88);
            this.lblRole.Name = "lblRole";
            this.lblRole.Size = new System.Drawing.Size(42, 13);
            this.lblRole.TabIndex = 47;
            this.lblRole.Text = "BI Role";
            this.lblRole.Visible = false;
            // 
            // grpCreateAccount
            // 
            this.grpCreateAccount.Controls.Add(this.label5);
            this.grpCreateAccount.Controls.Add(this.txtScopeID);
            this.grpCreateAccount.Controls.Add(this.txtClientID);
            this.grpCreateAccount.Controls.Add(this.txtAccountID);
            this.grpCreateAccount.Controls.Add(this.label4);
            this.grpCreateAccount.Controls.Add(this.txtScopeName);
            this.grpCreateAccount.Controls.Add(this.txtAccountName);
            this.grpCreateAccount.Controls.Add(this.label2);
            this.grpCreateAccount.Controls.Add(this.label3);
            this.grpCreateAccount.Controls.Add(this.txtClientName);
            this.grpCreateAccount.Location = new System.Drawing.Point(347, 131);
            this.grpCreateAccount.Name = "grpCreateAccount";
            this.grpCreateAccount.Size = new System.Drawing.Size(433, 241);
            this.grpCreateAccount.TabIndex = 50;
            this.grpCreateAccount.TabStop = false;
            this.grpCreateAccount.Text = "Create Account Hirarchy";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(310, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(18, 13);
            this.label5.TabIndex = 57;
            this.label5.Text = "ID";
            // 
            // txtScopeID
            // 
            this.txtScopeID.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtScopeID.Enabled = false;
            this.txtScopeID.Location = new System.Drawing.Point(310, 114);
            this.txtScopeID.Name = "txtScopeID";
            this.txtScopeID.Size = new System.Drawing.Size(117, 20);
            this.txtScopeID.TabIndex = 56;
            // 
            // txtClientID
            // 
            this.txtClientID.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtClientID.Enabled = false;
            this.txtClientID.Location = new System.Drawing.Point(310, 74);
            this.txtClientID.Name = "txtClientID";
            this.txtClientID.Size = new System.Drawing.Size(117, 20);
            this.txtClientID.TabIndex = 54;
            // 
            // txtAccountID
            // 
            this.txtAccountID.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtAccountID.Enabled = false;
            this.txtAccountID.Location = new System.Drawing.Point(310, 35);
            this.txtAccountID.Name = "txtAccountID";
            this.txtAccountID.Size = new System.Drawing.Size(117, 20);
            this.txtAccountID.TabIndex = 52;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 121);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 51;
            this.label4.Text = "Scope Name";
            // 
            // txtScopeName
            // 
            this.txtScopeName.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtScopeName.Location = new System.Drawing.Point(99, 114);
            this.txtScopeName.Name = "txtScopeName";
            this.txtScopeName.Size = new System.Drawing.Size(186, 20);
            this.txtScopeName.TabIndex = 50;
            this.txtScopeName.Validated += new System.EventHandler(this.txtScopeName_Validated);
            // 
            // txtAccountName
            // 
            this.txtAccountName.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtAccountName.Location = new System.Drawing.Point(99, 35);
            this.txtAccountName.Name = "txtAccountName";
            this.txtAccountName.Size = new System.Drawing.Size(186, 20);
            this.txtAccountName.TabIndex = 46;
            this.txtAccountName.Validated += new System.EventHandler(this.txtAccountName_Validated);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 81);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 49;
            this.label2.Text = "Client Name";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 48;
            this.label3.Text = "Account Name";
            // 
            // txtClientName
            // 
            this.txtClientName.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtClientName.Location = new System.Drawing.Point(99, 74);
            this.txtClientName.Name = "txtClientName";
            this.txtClientName.Size = new System.Drawing.Size(186, 20);
            this.txtClientName.TabIndex = 47;
            this.txtClientName.Validated += new System.EventHandler(this.txtClientName_Validated);
            // 
            // cmbWizardType
            // 
            this.cmbWizardType.FormattingEnabled = true;
            this.cmbWizardType.Items.AddRange(new object[] {
            "Add new account",
            "Add new BI",
            "Create new BI for active Account"});
            this.cmbWizardType.Location = new System.Drawing.Point(446, 104);
            this.cmbWizardType.Name = "cmbWizardType";
            this.cmbWizardType.Size = new System.Drawing.Size(209, 21);
            this.cmbWizardType.TabIndex = 51;
            this.cmbWizardType.SelectedValueChanged += new System.EventHandler(this.cmbWizardType_SelectedValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(344, 107);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(64, 13);
            this.label7.TabIndex = 52;
            this.label7.Text = "WizardType";
            // 
            // CreateAccount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.cmbWizardType);
            this.Controls.Add(this.grpCreateAccount);
            this.Controls.Add(this.grpBI);
            this.Controls.Add(this.cmbApplication);
            this.Controls.Add(this.label1);
            this.Name = "CreateAccount";
            this.StepDescription = "Create Account Line";
            this.StepName = "CreateNewAccountStepCollector";
            this.VisibleChanged += new System.EventHandler(this.CreateAccount_VisibleChanged);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.cmbApplication, 0);
            this.Controls.SetChildIndex(this.grpBI, 0);
            this.Controls.SetChildIndex(this.grpCreateAccount, 0);
            this.Controls.SetChildIndex(this.cmbWizardType, 0);
            this.Controls.SetChildIndex(this.label7, 0);
            this.grpBI.ResumeLayout(false);
            this.grpBI.PerformLayout();
            this.grpCreateAccount.ResumeLayout(false);
            this.grpCreateAccount.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbApplication;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBIScopeID;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox txtBIScopName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox grpBI;
        private System.Windows.Forms.GroupBox grpCreateAccount;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtScopeName;
        private System.Windows.Forms.TextBox txtAccountName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtClientName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtScopeID;
        private System.Windows.Forms.TextBox txtClientID;
        private System.Windows.Forms.TextBox txtAccountID;
        private System.Windows.Forms.CheckBox chkUseExistingRole;
        private System.Windows.Forms.TextBox txtRole;
        private System.Windows.Forms.Label lblRole;
        private System.Windows.Forms.ComboBox cmbWizardType;
        private System.Windows.Forms.Label label7;
    }
}
