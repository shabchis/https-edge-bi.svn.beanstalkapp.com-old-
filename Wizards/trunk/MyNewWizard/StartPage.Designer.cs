namespace MyNewWizard
{
	partial class StartPage
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
            this.components = new System.ComponentModel.Container();
            this.accountWizardApplicationsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.accountWizardApplicationsBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // accountWizardApplicationsBindingSource
            // 
            this.accountWizardApplicationsBindingSource.DataMember = "Account_Wizard_Applications";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(38, 131);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(710, 221);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "Welcome to Account Wizard.This wizard will bla bla bla\nbl a bla bal bala bla\n\n\n\nP" +
                "lease wait a few seconds untill the Wizard will be ready and the start button wi" +
                "ll be enabled....";
            // 
            // StartPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.richTextBox1);
            this.Name = "StartPage";
            this.StepDescription = "Start New Account Wizard";
            this.StepName = "Start";
            this.Load += new System.EventHandler(this.StartPage_Load);
            this.VisibleChanged += new System.EventHandler(this.StartPage_VisibleChanged);
            this.Controls.SetChildIndex(this.richTextBox1, 0);
            ((System.ComponentModel.ISupportInitialize)(this.accountWizardApplicationsBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.BindingSource accountWizardApplicationsBindingSource;
        private System.Windows.Forms.RichTextBox richTextBox1;


	}
}
