namespace MyNewWizard
{
	partial class ActiveDirectoryStep
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
            this.txtFullName = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.grpActiveDirectory = new System.Windows.Forms.GroupBox();
            this.grpActiveDirectory.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtFullName
            // 
            this.txtFullName.Location = new System.Drawing.Point(124, 76);
            this.txtFullName.Name = "txtFullName";
            this.txtFullName.Size = new System.Drawing.Size(186, 20);
            this.txtFullName.TabIndex = 8;
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(124, 48);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(186, 20);
            this.txtPassword.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Full Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Password";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "User Name";
            // 
            // txtUserName
            // 
            this.txtUserName.Location = new System.Drawing.Point(124, 19);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(186, 20);
            this.txtUserName.TabIndex = 5;
            // 
            // grpActiveDirectory
            // 
            this.grpActiveDirectory.Controls.Add(this.txtPassword);
            this.grpActiveDirectory.Controls.Add(this.txtFullName);
            this.grpActiveDirectory.Controls.Add(this.label1);
            this.grpActiveDirectory.Controls.Add(this.label2);
            this.grpActiveDirectory.Controls.Add(this.label3);
            this.grpActiveDirectory.Controls.Add(this.txtUserName);
            this.grpActiveDirectory.Location = new System.Drawing.Point(233, 129);
            this.grpActiveDirectory.Name = "grpActiveDirectory";
            this.grpActiveDirectory.Size = new System.Drawing.Size(339, 118);
            this.grpActiveDirectory.TabIndex = 12;
            this.grpActiveDirectory.TabStop = false;
            this.grpActiveDirectory.Text = "Add Active Directory User";
            // 
            // ActiveDirectoryStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.grpActiveDirectory);
            this.Name = "ActiveDirectoryStep";
            this.StepDescription = "Create new active directory user";
            this.StepName = "ActiveDirectoryStepCollector";
            this.Load += new System.EventHandler(this.ActiveDirectoryStep_Load);
            this.VisibleChanged += new System.EventHandler(this.ActiveDirectoryStep_VisibleChanged);
            this.Controls.SetChildIndex(this.grpActiveDirectory, 0);
            this.grpActiveDirectory.ResumeLayout(false);
            this.grpActiveDirectory.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtFullName;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.GroupBox grpActiveDirectory;
	}
}
