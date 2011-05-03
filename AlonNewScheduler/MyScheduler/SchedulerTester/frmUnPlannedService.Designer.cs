namespace SchedulerTester
{
    partial class frmUnPlannedService
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
            this.label1 = new System.Windows.Forms.Label();
            this.servicesCmb = new System.Windows.Forms.ComboBox();
            this.priorityCmb = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.FromPicker = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.addBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.toPicker = new System.Windows.Forms.DateTimePicker();
            this.schedulingGroupBox = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.dateToRunPicker = new System.Windows.Forms.DateTimePicker();
            this.timeToRunPicker = new System.Windows.Forms.DateTimePicker();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.keyTxt = new System.Windows.Forms.TextBox();
            this.valueTxt = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.optionsListView = new System.Windows.Forms.ListView();
            this.Key = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Value = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.addOptionBtn = new System.Windows.Forms.Button();
            this.removeOptionBtn = new System.Windows.Forms.Button();
            this.schedulingGroupBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Service";
            // 
            // servicesCmb
            // 
            this.servicesCmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.servicesCmb.FormattingEnabled = true;
            this.servicesCmb.Location = new System.Drawing.Point(73, 9);
            this.servicesCmb.Name = "servicesCmb";
            this.servicesCmb.Size = new System.Drawing.Size(231, 21);
            this.servicesCmb.TabIndex = 1;
            // 
            // priorityCmb
            // 
            this.priorityCmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.priorityCmb.FormattingEnabled = true;
            this.priorityCmb.Location = new System.Drawing.Point(73, 49);
            this.priorityCmb.Name = "priorityCmb";
            this.priorityCmb.Size = new System.Drawing.Size(121, 21);
            this.priorityCmb.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Priority";
            // 
            // FromPicker
            // 
            this.FromPicker.Location = new System.Drawing.Point(55, 19);
            this.FromPicker.Name = "FromPicker";
            this.FromPicker.Size = new System.Drawing.Size(200, 20);
            this.FromPicker.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "From";
            // 
            // addBtn
            // 
            this.addBtn.Location = new System.Drawing.Point(18, 458);
            this.addBtn.Name = "addBtn";
            this.addBtn.Size = new System.Drawing.Size(75, 23);
            this.addBtn.TabIndex = 8;
            this.addBtn.Text = "Add";
            this.addBtn.UseVisualStyleBackColor = true;
            this.addBtn.Click += new System.EventHandler(this.addBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.Location = new System.Drawing.Point(117, 458);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 9;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 61);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(20, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "To";
            // 
            // toPicker
            // 
            this.toPicker.Location = new System.Drawing.Point(55, 61);
            this.toPicker.Name = "toPicker";
            this.toPicker.Size = new System.Drawing.Size(200, 20);
            this.toPicker.TabIndex = 10;
            // 
            // schedulingGroupBox
            // 
            this.schedulingGroupBox.Controls.Add(this.timeToRunPicker);
            this.schedulingGroupBox.Controls.Add(this.label6);
            this.schedulingGroupBox.Controls.Add(this.dateToRunPicker);
            this.schedulingGroupBox.Controls.Add(this.label5);
            this.schedulingGroupBox.Location = new System.Drawing.Point(18, 91);
            this.schedulingGroupBox.Name = "schedulingGroupBox";
            this.schedulingGroupBox.Size = new System.Drawing.Size(366, 100);
            this.schedulingGroupBox.TabIndex = 12;
            this.schedulingGroupBox.TabStop = false;
            this.schedulingGroupBox.Text = "Scheduling(when the service/services will run)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 20);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(30, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Date";
            // 
            // dateToRunPicker
            // 
            this.dateToRunPicker.Location = new System.Drawing.Point(55, 19);
            this.dateToRunPicker.Name = "dateToRunPicker";
            this.dateToRunPicker.Size = new System.Drawing.Size(200, 20);
            this.dateToRunPicker.TabIndex = 5;
            // 
            // timeToRunPicker
            // 
            this.timeToRunPicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.timeToRunPicker.Location = new System.Drawing.Point(55, 49);
            this.timeToRunPicker.Name = "timeToRunPicker";
            this.timeToRunPicker.ShowUpDown = true;
            this.timeToRunPicker.Size = new System.Drawing.Size(92, 20);
            this.timeToRunPicker.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 55);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(30, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Time";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.removeOptionBtn);
            this.groupBox1.Controls.Add(this.addOptionBtn);
            this.groupBox1.Controls.Add(this.optionsListView);
            this.groupBox1.Controls.Add(this.valueTxt);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.keyTxt);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.FromPicker);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.toPicker);
            this.groupBox1.Location = new System.Drawing.Point(18, 197);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(626, 239);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Service Options";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 101);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(25, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Key";
            // 
            // keyTxt
            // 
            this.keyTxt.Location = new System.Drawing.Point(55, 98);
            this.keyTxt.Name = "keyTxt";
            this.keyTxt.Size = new System.Drawing.Size(119, 20);
            this.keyTxt.TabIndex = 13;
            // 
            // valueTxt
            // 
            this.valueTxt.Location = new System.Drawing.Point(256, 101);
            this.valueTxt.Name = "valueTxt";
            this.valueTxt.Size = new System.Drawing.Size(119, 20);
            this.valueTxt.TabIndex = 15;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(211, 104);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(34, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "Value";
            // 
            // optionsListView
            // 
            this.optionsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Key,
            this.Value});
            this.optionsListView.FullRowSelect = true;
            this.optionsListView.Location = new System.Drawing.Point(476, 98);
            this.optionsListView.Name = "optionsListView";
            this.optionsListView.Size = new System.Drawing.Size(121, 97);
            this.optionsListView.TabIndex = 16;
            this.optionsListView.UseCompatibleStateImageBehavior = false;
            // 
            // Key
            // 
            this.Key.Text = "Key";
            // 
            // Value
            // 
            this.Value.Text = "Value";
            // 
            // addOptionBtn
            // 
            this.addOptionBtn.Location = new System.Drawing.Point(381, 99);
            this.addOptionBtn.Name = "addOptionBtn";
            this.addOptionBtn.Size = new System.Drawing.Size(75, 23);
            this.addOptionBtn.TabIndex = 17;
            this.addOptionBtn.Text = ">>";
            this.addOptionBtn.UseVisualStyleBackColor = true;
            this.addOptionBtn.Click += new System.EventHandler(this.addOptionBtn_Click);
            // 
            // removeOptionBtn
            // 
            this.removeOptionBtn.Location = new System.Drawing.Point(381, 128);
            this.removeOptionBtn.Name = "removeOptionBtn";
            this.removeOptionBtn.Size = new System.Drawing.Size(75, 23);
            this.removeOptionBtn.TabIndex = 18;
            this.removeOptionBtn.Text = "<<";
            this.removeOptionBtn.UseVisualStyleBackColor = true;
            // 
            // frmUnPlannedService
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(847, 482);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.schedulingGroupBox);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.addBtn);
            this.Controls.Add(this.priorityCmb);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.servicesCmb);
            this.Controls.Add(this.label1);
            this.Name = "frmUnPlannedService";
            this.Text = "Add Unplanned Service";
            this.Load += new System.EventHandler(this.frmUnPlanedService_Load);
            this.schedulingGroupBox.ResumeLayout(false);
            this.schedulingGroupBox.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox servicesCmb;
        private System.Windows.Forms.ComboBox priorityCmb;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker FromPicker;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button addBtn;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker toPicker;
        private System.Windows.Forms.GroupBox schedulingGroupBox;
        private System.Windows.Forms.DateTimePicker timeToRunPicker;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DateTimePicker dateToRunPicker;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button removeOptionBtn;
        private System.Windows.Forms.Button addOptionBtn;
        private System.Windows.Forms.ListView optionsListView;
        private System.Windows.Forms.ColumnHeader Key;
        private System.Windows.Forms.ColumnHeader Value;
        private System.Windows.Forms.TextBox valueTxt;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox keyTxt;
        private System.Windows.Forms.Label label7;
    }
}