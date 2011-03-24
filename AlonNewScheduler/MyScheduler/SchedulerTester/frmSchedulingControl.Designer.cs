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
			this.getServicesButton = new System.Windows.Forms.Button();
			this.rescheduleBtn = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.deleteServiceFromScheduleBtn = new System.Windows.Forms.Button();
			this.shceduledID = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.scheduledName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.accountID = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.startOn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.endOn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.status = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.scope = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.deleted = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.startBtn = new System.Windows.Forms.Button();
			this.EndBtn = new System.Windows.Forms.Button();
			this.logtextBox = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.scheduleInfoGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// ScheduleBtn
			// 
			this.ScheduleBtn.Location = new System.Drawing.Point(166, 291);
			this.ScheduleBtn.Name = "ScheduleBtn";
			this.ScheduleBtn.Size = new System.Drawing.Size(122, 23);
			this.ScheduleBtn.TabIndex = 0;
			this.ScheduleBtn.Text = "New Schedule";
			this.ScheduleBtn.UseVisualStyleBackColor = true;
			this.ScheduleBtn.Click += new System.EventHandler(this.ScheduleBtn_Click);
			// 
			// endServiceBtn
			// 
			this.endServiceBtn.Location = new System.Drawing.Point(375, 291);
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
            this.accountID,
            this.startOn,
            this.endOn,
            this.status,
            this.scope,
            this.deleted});
			this.scheduleInfoGrid.Location = new System.Drawing.Point(3, 12);
			this.scheduleInfoGrid.Name = "scheduleInfoGrid";
			this.scheduleInfoGrid.Size = new System.Drawing.Size(968, 273);
			this.scheduleInfoGrid.TabIndex = 2;
			// 
			// getServicesButton
			// 
			this.getServicesButton.Location = new System.Drawing.Point(456, 291);
			this.getServicesButton.Name = "getServicesButton";
			this.getServicesButton.Size = new System.Drawing.Size(138, 23);
			this.getServicesButton.TabIndex = 3;
			this.getServicesButton.Text = "Get Schedule Services";
			this.getServicesButton.UseVisualStyleBackColor = true;
			this.getServicesButton.Click += new System.EventHandler(this.getServicesButton_Click);
			// 
			// rescheduleBtn
			// 
			this.rescheduleBtn.Location = new System.Drawing.Point(294, 291);
			this.rescheduleBtn.Name = "rescheduleBtn";
			this.rescheduleBtn.Size = new System.Drawing.Size(75, 23);
			this.rescheduleBtn.TabIndex = 4;
			this.rescheduleBtn.Text = "Reschedule";
			this.rescheduleBtn.UseVisualStyleBackColor = true;
			this.rescheduleBtn.Click += new System.EventHandler(this.rescheduleBtn_Click);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(600, 291);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(186, 23);
			this.button1.TabIndex = 5;
			this.button1.Text = "Add UnPlaned Service to Schedule";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// deleteServiceFromScheduleBtn
			// 
			this.deleteServiceFromScheduleBtn.Location = new System.Drawing.Point(792, 291);
			this.deleteServiceFromScheduleBtn.Name = "deleteServiceFromScheduleBtn";
			this.deleteServiceFromScheduleBtn.Size = new System.Drawing.Size(179, 23);
			this.deleteServiceFromScheduleBtn.TabIndex = 6;
			this.deleteServiceFromScheduleBtn.Text = "Delete Service From Schedule";
			this.deleteServiceFromScheduleBtn.UseVisualStyleBackColor = true;
			this.deleteServiceFromScheduleBtn.Click += new System.EventHandler(this.deleteServiceFromScheduleBtn_Click);
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
			// accountID
			// 
			this.accountID.HeaderText = "Account ID";
			this.accountID.Name = "accountID";
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
			// scope
			// 
			this.scope.HeaderText = "Scope";
			this.scope.Name = "scope";
			// 
			// deleted
			// 
			this.deleted.HeaderText = "Deleted";
			this.deleted.Name = "deleted";
			// 
			// startBtn
			// 
			this.startBtn.Location = new System.Drawing.Point(4, 291);
			this.startBtn.Name = "startBtn";
			this.startBtn.Size = new System.Drawing.Size(75, 23);
			this.startBtn.TabIndex = 7;
			this.startBtn.Text = "StartTimer";
			this.startBtn.UseVisualStyleBackColor = true;
			this.startBtn.Click += new System.EventHandler(this.startBtn_Click);
			// 
			// EndBtn
			// 
			this.EndBtn.Location = new System.Drawing.Point(85, 291);
			this.EndBtn.Name = "EndBtn";
			this.EndBtn.Size = new System.Drawing.Size(75, 23);
			this.EndBtn.TabIndex = 8;
			this.EndBtn.Text = "StopTimer";
			this.EndBtn.UseVisualStyleBackColor = true;
			this.EndBtn.Click += new System.EventHandler(this.EndBtn_Click);
			// 
			// logtextBox
			// 
			this.logtextBox.Location = new System.Drawing.Point(4, 321);
			this.logtextBox.Multiline = true;
			this.logtextBox.Name = "logtextBox";
			this.logtextBox.Size = new System.Drawing.Size(590, 227);
			this.logtextBox.TabIndex = 9;
			// 
			// frmSchedulingControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1000, 560);
			this.Controls.Add(this.logtextBox);
			this.Controls.Add(this.EndBtn);
			this.Controls.Add(this.startBtn);
			this.Controls.Add(this.deleteServiceFromScheduleBtn);
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
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button ScheduleBtn;
		private System.Windows.Forms.Button endServiceBtn;
		private System.Windows.Forms.DataGridView scheduleInfoGrid;
		private System.Windows.Forms.Button getServicesButton;
		private System.Windows.Forms.Button rescheduleBtn;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button deleteServiceFromScheduleBtn;
		private System.Windows.Forms.DataGridViewTextBoxColumn shceduledID;
		private System.Windows.Forms.DataGridViewTextBoxColumn scheduledName;
		private System.Windows.Forms.DataGridViewTextBoxColumn accountID;
		private System.Windows.Forms.DataGridViewTextBoxColumn startOn;
		private System.Windows.Forms.DataGridViewTextBoxColumn endOn;
		private System.Windows.Forms.DataGridViewTextBoxColumn status;
		private System.Windows.Forms.DataGridViewTextBoxColumn scope;
		private System.Windows.Forms.DataGridViewTextBoxColumn deleted;
		private System.Windows.Forms.Button startBtn;
		private System.Windows.Forms.Button EndBtn;
		private System.Windows.Forms.TextBox logtextBox;
	}
}

