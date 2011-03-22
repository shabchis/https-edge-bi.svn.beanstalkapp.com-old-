namespace SchedulerTester
{
	partial class frmSchedulingControl
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
			this.ScheduleBtn = new System.Windows.Forms.Button();
			this.endServiceBtn = new System.Windows.Forms.Button();
			this.scheduleInfoGrid = new System.Windows.Forms.DataGridView();
			this.shceduledID = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.scheduledName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.startOn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.endOn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.status = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.getServicesButton = new System.Windows.Forms.Button();
			this.rescheduleBtn = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.scheduleInfoGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// ScheduleBtn
			// 
			this.ScheduleBtn.Location = new System.Drawing.Point(23, 231);
			this.ScheduleBtn.Name = "ScheduleBtn";
			this.ScheduleBtn.Size = new System.Drawing.Size(122, 23);
			this.ScheduleBtn.TabIndex = 0;
			this.ScheduleBtn.Text = "New Schedule";
			this.ScheduleBtn.UseVisualStyleBackColor = true;
			this.ScheduleBtn.Click += new System.EventHandler(this.ScheduleBtn_Click);
			// 
			// endServiceBtn
			// 
			this.endServiceBtn.Location = new System.Drawing.Point(256, 231);
			this.endServiceBtn.Name = "endServiceBtn";
			this.endServiceBtn.Size = new System.Drawing.Size(75, 23);
			this.endServiceBtn.TabIndex = 1;
			this.endServiceBtn.Text = "End Service";
			this.endServiceBtn.UseVisualStyleBackColor = true;
			this.endServiceBtn.Click += new System.EventHandler(this.endServiceBtn_Click);
			// 
			// scheduleInfoGrid
			// 
			this.scheduleInfoGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.scheduleInfoGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.shceduledID,
            this.scheduledName,
            this.startOn,
            this.endOn,
            this.status});
			this.scheduleInfoGrid.Location = new System.Drawing.Point(23, 12);
			this.scheduleInfoGrid.Name = "scheduleInfoGrid";
			this.scheduleInfoGrid.Size = new System.Drawing.Size(716, 199);
			this.scheduleInfoGrid.TabIndex = 2;
			// 
			// shceduledID
			// 
			this.shceduledID.HeaderText = "Shceduled ID";
			this.shceduledID.Name = "shceduledID";
			this.shceduledID.ReadOnly = true;
			// 
			// scheduledName
			// 
			this.scheduledName.HeaderText = "Scheduled Name";
			this.scheduledName.Name = "scheduledName";
			this.scheduledName.ReadOnly = true;
			// 
			// startOn
			// 
			this.startOn.HeaderText = "Start On";
			this.startOn.Name = "startOn";
			this.startOn.ReadOnly = true;
			// 
			// endOn
			// 
			this.endOn.HeaderText = "End On";
			this.endOn.Name = "endOn";
			this.endOn.ReadOnly = true;
			// 
			// status
			// 
			this.status.HeaderText = "Status";
			this.status.Name = "status";
			this.status.ReadOnly = true;
			// 
			// getServicesButton
			// 
			this.getServicesButton.Location = new System.Drawing.Point(349, 231);
			this.getServicesButton.Name = "getServicesButton";
			this.getServicesButton.Size = new System.Drawing.Size(138, 23);
			this.getServicesButton.TabIndex = 3;
			this.getServicesButton.Text = "Get Schedule Services";
			this.getServicesButton.UseVisualStyleBackColor = true;
			this.getServicesButton.Click += new System.EventHandler(this.getServicesButton_Click);
			// 
			// rescheduleBtn
			// 
			this.rescheduleBtn.Location = new System.Drawing.Point(163, 231);
			this.rescheduleBtn.Name = "rescheduleBtn";
			this.rescheduleBtn.Size = new System.Drawing.Size(75, 23);
			this.rescheduleBtn.TabIndex = 4;
			this.rescheduleBtn.Text = "Reschedule";
			this.rescheduleBtn.UseVisualStyleBackColor = true;
			this.rescheduleBtn.Click += new System.EventHandler(this.rescheduleBtn_Click);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(506, 231);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(186, 23);
			this.button1.TabIndex = 5;
			this.button1.Text = "Add UnPlaned Service to Schedule";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// frmSchedulingControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(828, 437);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.rescheduleBtn);
			this.Controls.Add(this.getServicesButton);
			this.Controls.Add(this.scheduleInfoGrid);
			this.Controls.Add(this.endServiceBtn);
			this.Controls.Add(this.ScheduleBtn);
			this.Name = "frmSchedulingControl";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.scheduleInfoGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button ScheduleBtn;
		private System.Windows.Forms.Button endServiceBtn;
		private System.Windows.Forms.DataGridView scheduleInfoGrid;
		private System.Windows.Forms.Button getServicesButton;
		private System.Windows.Forms.DataGridViewTextBoxColumn shceduledID;
		private System.Windows.Forms.DataGridViewTextBoxColumn scheduledName;
		private System.Windows.Forms.DataGridViewTextBoxColumn startOn;
		private System.Windows.Forms.DataGridViewTextBoxColumn endOn;
		private System.Windows.Forms.DataGridViewTextBoxColumn status;
		private System.Windows.Forms.Button rescheduleBtn;
		private System.Windows.Forms.Button button1;
	}
}

