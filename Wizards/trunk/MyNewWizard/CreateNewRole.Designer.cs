namespace MyNewWizard
{
	partial class CreateNewRole
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
            this.label23 = new System.Windows.Forms.Label();
            this.txtRoleMemberName = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.txtRoleID = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtRoleName = new System.Windows.Forms.TextBox();
            this.grpRole = new System.Windows.Forms.GroupBox();
            this.grpRole.SuspendLayout();
            this.SuspendLayout();
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(11, 88);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(101, 13);
            this.label23.TabIndex = 52;
            this.label23.Text = "Role Member Name";
            // 
            // txtRoleMemberName
            // 
            this.txtRoleMemberName.AcceptsTab = true;
            this.txtRoleMemberName.Location = new System.Drawing.Point(118, 88);
            this.txtRoleMemberName.Name = "txtRoleMemberName";
            this.txtRoleMemberName.Size = new System.Drawing.Size(186, 20);
            this.txtRoleMemberName.TabIndex = 48;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(11, 62);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(43, 13);
            this.label22.TabIndex = 51;
            this.label22.Text = "Role ID";
            // 
            // txtRoleID
            // 
            this.txtRoleID.Location = new System.Drawing.Point(118, 62);
            this.txtRoleID.Name = "txtRoleID";
            this.txtRoleID.Size = new System.Drawing.Size(186, 20);
            this.txtRoleID.TabIndex = 47;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(11, 36);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(60, 13);
            this.label10.TabIndex = 50;
            this.label10.Text = "Role Name";
            // 
            // txtRoleName
            // 
            this.txtRoleName.AcceptsTab = true;
            this.txtRoleName.Location = new System.Drawing.Point(118, 36);
            this.txtRoleName.Name = "txtRoleName";
            this.txtRoleName.Size = new System.Drawing.Size(186, 20);
            this.txtRoleName.TabIndex = 46;
            // 
            // grpRole
            // 
            this.grpRole.Controls.Add(this.txtRoleID);
            this.grpRole.Controls.Add(this.txtRoleName);
            this.grpRole.Controls.Add(this.label10);
            this.grpRole.Controls.Add(this.label22);
            this.grpRole.Controls.Add(this.label23);
            this.grpRole.Controls.Add(this.txtRoleMemberName);
            this.grpRole.Location = new System.Drawing.Point(275, 136);
            this.grpRole.Name = "grpRole";
            this.grpRole.Size = new System.Drawing.Size(323, 130);
            this.grpRole.TabIndex = 53;
            this.grpRole.TabStop = false;
            this.grpRole.Text = "Add BI Role";
            // 
            // CreateNewRole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.grpRole);
            this.Name = "CreateNewRole";
            this.StepDescription = "Create new role";
            this.StepName = "CreateRoleStepCollector";
            this.Load += new System.EventHandler(this.CreateNewRole_Load);
            this.VisibleChanged += new System.EventHandler(this.CreateNewRole_VisibleChanged);
            this.Controls.SetChildIndex(this.grpRole, 0);
            this.grpRole.ResumeLayout(false);
            this.grpRole.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label23;
		private System.Windows.Forms.TextBox txtRoleMemberName;
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.TextBox txtRoleID;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.TextBox txtRoleName;
        private System.Windows.Forms.GroupBox grpRole;
	}
}
