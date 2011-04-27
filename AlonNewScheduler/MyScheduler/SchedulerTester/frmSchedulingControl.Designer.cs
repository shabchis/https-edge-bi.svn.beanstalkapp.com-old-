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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSchedulingControl));
            this.ScheduleBtn = new System.Windows.Forms.Button();
            this.endServiceBtn = new System.Windows.Forms.Button();
            this.scheduleInfoGrid = new System.Windows.Forms.DataGridView();
            this.shceduledID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.scheduledName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.accountID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.startOn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.endOn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.scope = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.deleted = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.outCome = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dynamicStaus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rescheduleBtn = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.deleteServiceFromScheduleBtn = new System.Windows.Forms.Button();
            this.startBtn = new System.Windows.Forms.Button();
            this.EndBtn = new System.Windows.Forms.Button();
            this.logtextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.scheduleInfoGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // ScheduleBtn
            // 
            this.ScheduleBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
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
            this.endServiceBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.endServiceBtn.Location = new System.Drawing.Point(375, 291);
            this.endServiceBtn.Name = "endServiceBtn";
            this.endServiceBtn.Size = new System.Drawing.Size(101, 23);
            this.endServiceBtn.TabIndex = 1;
            this.endServiceBtn.Text = "Abort  Service";
            this.endServiceBtn.UseVisualStyleBackColor = true;
            this.endServiceBtn.Click += new System.EventHandler(this.endServiceBtn_Click);
            // 
            // scheduleInfoGrid
            // 
            this.scheduleInfoGrid.AllowUserToAddRows = false;
            this.scheduleInfoGrid.AllowUserToDeleteRows = false;
            this.scheduleInfoGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.scheduleInfoGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.scheduleInfoGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.shceduledID,
            this.scheduledName,
            this.accountID,
            this.startOn,
            this.endOn,
            this.status,
            this.scope,
            this.deleted,
            this.outCome,
            this.dynamicStaus});
            this.scheduleInfoGrid.Location = new System.Drawing.Point(3, 12);
            this.scheduleInfoGrid.Name = "scheduleInfoGrid";
            this.scheduleInfoGrid.ReadOnly = true;
            this.scheduleInfoGrid.Size = new System.Drawing.Size(1100, 273);
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
            // accountID
            // 
            this.accountID.HeaderText = "Account ID";
            this.accountID.Name = "accountID";
            this.accountID.ReadOnly = true;
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
            this.scope.ReadOnly = true;
            // 
            // deleted
            // 
            this.deleted.HeaderText = "Deleted";
            this.deleted.Name = "deleted";
            this.deleted.ReadOnly = true;
            // 
            // outCome
            // 
            this.outCome.HeaderText = "OutCome";
            this.outCome.Name = "outCome";
            this.outCome.ReadOnly = true;
            // 
            // dynamicStaus
            // 
            this.dynamicStaus.HeaderText = "Dynamic Status";
            this.dynamicStaus.Name = "dynamicStaus";
            this.dynamicStaus.ReadOnly = true;
            // 
            // rescheduleBtn
            // 
            this.rescheduleBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
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
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(667, 291);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(186, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "Add UnPlaned Service to Schedule";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // deleteServiceFromScheduleBtn
            // 
            this.deleteServiceFromScheduleBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.deleteServiceFromScheduleBtn.Location = new System.Drawing.Point(482, 291);
            this.deleteServiceFromScheduleBtn.Name = "deleteServiceFromScheduleBtn";
            this.deleteServiceFromScheduleBtn.Size = new System.Drawing.Size(179, 23);
            this.deleteServiceFromScheduleBtn.TabIndex = 6;
            this.deleteServiceFromScheduleBtn.Text = "Delete Service From Schedule";
            this.deleteServiceFromScheduleBtn.UseVisualStyleBackColor = true;
            this.deleteServiceFromScheduleBtn.Click += new System.EventHandler(this.deleteServiceFromScheduleBtn_Click);
            // 
            // startBtn
            // 
            this.startBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.startBtn.Location = new System.Drawing.Point(4, 291);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(75, 23);
            this.startBtn.TabIndex = 7;
            this.startBtn.Text = "Start";
            this.startBtn.UseVisualStyleBackColor = true;
            this.startBtn.Click += new System.EventHandler(this.startBtn_Click);
            // 
            // EndBtn
            // 
            this.EndBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.EndBtn.Location = new System.Drawing.Point(85, 291);
            this.EndBtn.Name = "EndBtn";
            this.EndBtn.Size = new System.Drawing.Size(75, 23);
            this.EndBtn.TabIndex = 8;
            this.EndBtn.Text = "Stop";
            this.EndBtn.UseVisualStyleBackColor = true;
            this.EndBtn.Click += new System.EventHandler(this.EndBtn_Click);
            // 
            // logtextBox
            // 
            this.logtextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.logtextBox.Location = new System.Drawing.Point(4, 321);
            this.logtextBox.Multiline = true;
            this.logtextBox.Name = "logtextBox";
            this.logtextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logtextBox.Size = new System.Drawing.Size(1099, 227);
            this.logtextBox.TabIndex = 9;
            // 
            // frmSchedulingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1115, 560);
            this.Controls.Add(this.logtextBox);
            this.Controls.Add(this.EndBtn);
            this.Controls.Add(this.startBtn);
            this.Controls.Add(this.deleteServiceFromScheduleBtn);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.rescheduleBtn);
            this.Controls.Add(this.scheduleInfoGrid);
            this.Controls.Add(this.endServiceBtn);
            this.Controls.Add(this.ScheduleBtn);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmSchedulingControl";
            this.Text = "Edge Scheduluer- Scheduled Services";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSchedulingControl_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.scheduleInfoGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button ScheduleBtn;
		private System.Windows.Forms.Button endServiceBtn;
        private System.Windows.Forms.DataGridView scheduleInfoGrid;
		private System.Windows.Forms.Button rescheduleBtn;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button deleteServiceFromScheduleBtn;
		private System.Windows.Forms.Button startBtn;
		private System.Windows.Forms.Button EndBtn;
        private System.Windows.Forms.TextBox logtextBox;
        private System.Windows.Forms.DataGridViewTextBoxColumn shceduledID;
        private System.Windows.Forms.DataGridViewTextBoxColumn scheduledName;
        private System.Windows.Forms.DataGridViewTextBoxColumn accountID;
        private System.Windows.Forms.DataGridViewTextBoxColumn startOn;
        private System.Windows.Forms.DataGridViewTextBoxColumn endOn;
        private System.Windows.Forms.DataGridViewTextBoxColumn status;
        private System.Windows.Forms.DataGridViewTextBoxColumn scope;
        private System.Windows.Forms.DataGridViewTextBoxColumn deleted;
        private System.Windows.Forms.DataGridViewTextBoxColumn outCome;
        private System.Windows.Forms.DataGridViewTextBoxColumn dynamicStaus;
	}
}

