namespace EdgeBI.Wizards.Utils.WizardTester
{
	partial class frmTestWizard
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			this.gvKeyValue = new System.Windows.Forms.DataGridView();
			this.btnStart = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.txtResult = new System.Windows.Forms.RichTextBox();
			this.btnNextStep = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.txtWizardName = new System.Windows.Forms.TextBox();
			this.btnGetSummary = new System.Windows.Forms.Button();
			this.btnExecute = new System.Windows.Forms.Button();
			this.btnTestAddUserActiveDirectory = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.gvKeyValue)).BeginInit();
			this.SuspendLayout();
			// 
			// gvKeyValue
			// 
			this.gvKeyValue.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gvKeyValue.Location = new System.Drawing.Point(12, 12);
			this.gvKeyValue.Name = "gvKeyValue";
			dataGridViewCellStyle1.NullValue = " ";
			this.gvKeyValue.RowsDefaultCellStyle = dataGridViewCellStyle1;
			this.gvKeyValue.Size = new System.Drawing.Size(240, 378);
			this.gvKeyValue.TabIndex = 0;
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(272, 25);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(75, 23);
			this.btnStart.TabIndex = 1;
			this.btnStart.Text = "Start Wizard";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(536, -1);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(28, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Log:";
			// 
			// txtResult
			// 
			this.txtResult.Location = new System.Drawing.Point(539, 25);
			this.txtResult.Name = "txtResult";
			this.txtResult.ReadOnly = true;
			this.txtResult.Size = new System.Drawing.Size(199, 365);
			this.txtResult.TabIndex = 4;
			this.txtResult.Text = "";
			// 
			// btnNextStep
			// 
			this.btnNextStep.Enabled = false;
			this.btnNextStep.Location = new System.Drawing.Point(272, 54);
			this.btnNextStep.Name = "btnNextStep";
			this.btnNextStep.Size = new System.Drawing.Size(75, 23);
			this.btnNextStep.TabIndex = 5;
			this.btnNextStep.Text = "Next Step";
			this.btnNextStep.UseVisualStyleBackColor = true;
			this.btnNextStep.Click += new System.EventHandler(this.btnNextStep_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(354, 34);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(62, 13);
			this.label2.TabIndex = 7;
			this.label2.Text = "WizardNum";
			// 
			// txtWizardName
			// 
			this.txtWizardName.Location = new System.Drawing.Point(422, 27);
			this.txtWizardName.Name = "txtWizardName";
			this.txtWizardName.Size = new System.Drawing.Size(100, 20);
			this.txtWizardName.TabIndex = 8;
			this.txtWizardName.Text = "AccountWizard";
			// 
			// btnGetSummary
			// 
			this.btnGetSummary.Enabled = false;
			this.btnGetSummary.Location = new System.Drawing.Point(357, 53);
			this.btnGetSummary.Name = "btnGetSummary";
			this.btnGetSummary.Size = new System.Drawing.Size(95, 23);
			this.btnGetSummary.TabIndex = 9;
			this.btnGetSummary.Text = "Get Summary";
			this.btnGetSummary.UseVisualStyleBackColor = true;
			this.btnGetSummary.Click += new System.EventHandler(this.btnGetSummary_Click);
			// 
			// btnExecute
			// 
			this.btnExecute.Enabled = false;
			this.btnExecute.Location = new System.Drawing.Point(459, 53);
			this.btnExecute.Name = "btnExecute";
			this.btnExecute.Size = new System.Drawing.Size(75, 23);
			this.btnExecute.TabIndex = 10;
			this.btnExecute.Text = "Execute";
			this.btnExecute.UseVisualStyleBackColor = true;
			this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
			// 
			// btnTestAddUserActiveDirectory
			// 
			this.btnTestAddUserActiveDirectory.Location = new System.Drawing.Point(272, 98);
			this.btnTestAddUserActiveDirectory.Name = "btnTestAddUserActiveDirectory";
			this.btnTestAddUserActiveDirectory.Size = new System.Drawing.Size(96, 23);
			this.btnTestAddUserActiveDirectory.TabIndex = 11;
			this.btnTestAddUserActiveDirectory.Text = "Add User";
			this.btnTestAddUserActiveDirectory.UseVisualStyleBackColor = true;
			this.btnTestAddUserActiveDirectory.Click += new System.EventHandler(this.btnTestAddUserActiveDirectory_Click);
			// 
			// frmTestWizard
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(750, 402);
			this.Controls.Add(this.btnTestAddUserActiveDirectory);
			this.Controls.Add(this.btnExecute);
			this.Controls.Add(this.btnGetSummary);
			this.Controls.Add(this.txtWizardName);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btnNextStep);
			this.Controls.Add(this.txtResult);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.gvKeyValue);
			this.Name = "frmTestWizard";
			this.Text = "Wizards Testers";
			this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.gvKeyValue)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.DataGridView gvKeyValue;
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RichTextBox txtResult;
		private System.Windows.Forms.Button btnNextStep;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtWizardName;
		private System.Windows.Forms.Button btnGetSummary;
		private System.Windows.Forms.Button btnExecute;
		private System.Windows.Forms.Button btnTestAddUserActiveDirectory;
	}
}

