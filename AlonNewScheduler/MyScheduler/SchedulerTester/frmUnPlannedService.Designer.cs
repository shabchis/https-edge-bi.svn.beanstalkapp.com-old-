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
            this.priorityCmb = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.FromPicker = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.addBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.toPicker = new System.Windows.Forms.DateTimePicker();
            this.schedulingGroupBox = new System.Windows.Forms.GroupBox();
            this.timeToRunPicker = new System.Windows.Forms.DateTimePicker();
            this.label6 = new System.Windows.Forms.Label();
            this.dateToRunPicker = new System.Windows.Forms.DateTimePicker();
            this.label5 = new System.Windows.Forms.Label();
            this.serviceOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.clearOptionsBtn = new System.Windows.Forms.Button();
            this.removeOptionBtn = new System.Windows.Forms.Button();
            this.addOptionBtn = new System.Windows.Forms.Button();
            this.optionsListView = new System.Windows.Forms.ListView();
            this.Key = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Value = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.valueTxt = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.keyTxt = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.useOptionsCheckBox = new System.Windows.Forms.CheckBox();
            this.servicesCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.selectAllbtn = new System.Windows.Forms.Button();
            this.unSelectAllBtn = new System.Windows.Forms.Button();
            this.schedulingGroupBox.SuspendLayout();
            this.serviceOptionsGroupBox.SuspendLayout();
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
            // priorityCmb
            // 
            this.priorityCmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.priorityCmb.FormattingEnabled = true;
            this.priorityCmb.Location = new System.Drawing.Point(856, 12);
            this.priorityCmb.Name = "priorityCmb";
            this.priorityCmb.Size = new System.Drawing.Size(121, 21);
            this.priorityCmb.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(798, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Priority";
            // 
            // FromPicker
            // 
            this.FromPicker.Location = new System.Drawing.Point(55, 30);
            this.FromPicker.Name = "FromPicker";
            this.FromPicker.Size = new System.Drawing.Size(200, 20);
            this.FromPicker.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "From";
            // 
            // addBtn
            // 
            this.addBtn.Location = new System.Drawing.Point(73, 612);
            this.addBtn.Name = "addBtn";
            this.addBtn.Size = new System.Drawing.Size(75, 23);
            this.addBtn.TabIndex = 8;
            this.addBtn.Text = "Add";
            this.addBtn.UseVisualStyleBackColor = true;
            this.addBtn.Click += new System.EventHandler(this.addBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.Location = new System.Drawing.Point(172, 612);
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
            this.label4.Location = new System.Drawing.Point(10, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(20, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "To";
            // 
            // toPicker
            // 
            this.toPicker.Location = new System.Drawing.Point(55, 72);
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
            this.schedulingGroupBox.Location = new System.Drawing.Point(73, 245);
            this.schedulingGroupBox.Name = "schedulingGroupBox";
            this.schedulingGroupBox.Size = new System.Drawing.Size(366, 100);
            this.schedulingGroupBox.TabIndex = 12;
            this.schedulingGroupBox.TabStop = false;
            this.schedulingGroupBox.Text = "Scheduling(when the service/services will run)";
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
            // dateToRunPicker
            // 
            this.dateToRunPicker.Location = new System.Drawing.Point(55, 19);
            this.dateToRunPicker.Name = "dateToRunPicker";
            this.dateToRunPicker.Size = new System.Drawing.Size(200, 20);
            this.dateToRunPicker.TabIndex = 5;
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
            // serviceOptionsGroupBox
            // 
            this.serviceOptionsGroupBox.Controls.Add(this.clearOptionsBtn);
            this.serviceOptionsGroupBox.Controls.Add(this.removeOptionBtn);
            this.serviceOptionsGroupBox.Controls.Add(this.addOptionBtn);
            this.serviceOptionsGroupBox.Controls.Add(this.optionsListView);
            this.serviceOptionsGroupBox.Controls.Add(this.valueTxt);
            this.serviceOptionsGroupBox.Controls.Add(this.label8);
            this.serviceOptionsGroupBox.Controls.Add(this.keyTxt);
            this.serviceOptionsGroupBox.Controls.Add(this.label7);
            this.serviceOptionsGroupBox.Controls.Add(this.FromPicker);
            this.serviceOptionsGroupBox.Controls.Add(this.label3);
            this.serviceOptionsGroupBox.Controls.Add(this.label4);
            this.serviceOptionsGroupBox.Controls.Add(this.toPicker);
            this.serviceOptionsGroupBox.Enabled = false;
            this.serviceOptionsGroupBox.Location = new System.Drawing.Point(73, 374);
            this.serviceOptionsGroupBox.Name = "serviceOptionsGroupBox";
            this.serviceOptionsGroupBox.Size = new System.Drawing.Size(626, 216);
            this.serviceOptionsGroupBox.TabIndex = 13;
            this.serviceOptionsGroupBox.TabStop = false;
            this.serviceOptionsGroupBox.Text = "Service Options";
            // 
            // clearOptionsBtn
            // 
            this.clearOptionsBtn.Location = new System.Drawing.Point(381, 168);
            this.clearOptionsBtn.Name = "clearOptionsBtn";
            this.clearOptionsBtn.Size = new System.Drawing.Size(75, 23);
            this.clearOptionsBtn.TabIndex = 19;
            this.clearOptionsBtn.Text = "Clear";
            this.clearOptionsBtn.UseVisualStyleBackColor = true;
            this.clearOptionsBtn.Click += new System.EventHandler(this.clearOptionsBtn_Click);
            // 
            // removeOptionBtn
            // 
            this.removeOptionBtn.Location = new System.Drawing.Point(381, 139);
            this.removeOptionBtn.Name = "removeOptionBtn";
            this.removeOptionBtn.Size = new System.Drawing.Size(75, 23);
            this.removeOptionBtn.TabIndex = 18;
            this.removeOptionBtn.Text = "<<";
            this.removeOptionBtn.UseVisualStyleBackColor = true;
            this.removeOptionBtn.Click += new System.EventHandler(this.removeOptionBtn_Click);
            // 
            // addOptionBtn
            // 
            this.addOptionBtn.Location = new System.Drawing.Point(381, 110);
            this.addOptionBtn.Name = "addOptionBtn";
            this.addOptionBtn.Size = new System.Drawing.Size(75, 23);
            this.addOptionBtn.TabIndex = 17;
            this.addOptionBtn.Text = ">>";
            this.addOptionBtn.UseVisualStyleBackColor = true;
            this.addOptionBtn.Click += new System.EventHandler(this.addOptionBtn_Click);
            // 
            // optionsListView
            // 
            this.optionsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Key,
            this.Value});
            this.optionsListView.FullRowSelect = true;
            this.optionsListView.Location = new System.Drawing.Point(476, 109);
            this.optionsListView.Name = "optionsListView";
            this.optionsListView.Size = new System.Drawing.Size(127, 97);
            this.optionsListView.TabIndex = 16;
            this.optionsListView.UseCompatibleStateImageBehavior = false;
            this.optionsListView.View = System.Windows.Forms.View.Details;
            // 
            // Key
            // 
            this.Key.Text = "Key";
            // 
            // Value
            // 
            this.Value.Text = "Value";
            // 
            // valueTxt
            // 
            this.valueTxt.Location = new System.Drawing.Point(256, 112);
            this.valueTxt.Name = "valueTxt";
            this.valueTxt.Size = new System.Drawing.Size(119, 20);
            this.valueTxt.TabIndex = 15;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(211, 115);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(34, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "Value";
            // 
            // keyTxt
            // 
            this.keyTxt.Location = new System.Drawing.Point(55, 109);
            this.keyTxt.Name = "keyTxt";
            this.keyTxt.Size = new System.Drawing.Size(119, 20);
            this.keyTxt.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 112);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(25, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Key";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(567, 71);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(0, 13);
            this.label9.TabIndex = 14;
            // 
            // useOptionsCheckBox
            // 
            this.useOptionsCheckBox.AutoSize = true;
            this.useOptionsCheckBox.Location = new System.Drawing.Point(73, 352);
            this.useOptionsCheckBox.Name = "useOptionsCheckBox";
            this.useOptionsCheckBox.Size = new System.Drawing.Size(123, 17);
            this.useOptionsCheckBox.TabIndex = 15;
            this.useOptionsCheckBox.Text = "Use Service Options";
            this.useOptionsCheckBox.UseVisualStyleBackColor = true;
            this.useOptionsCheckBox.CheckedChanged += new System.EventHandler(this.useOptionsCheckBox_CheckedChanged);
            // 
            // servicesCheckedListBox
            // 
            this.servicesCheckedListBox.CheckOnClick = true;
            this.servicesCheckedListBox.ColumnWidth = 350;
            this.servicesCheckedListBox.FormattingEnabled = true;
            this.servicesCheckedListBox.Location = new System.Drawing.Point(73, 9);
            this.servicesCheckedListBox.MultiColumn = true;
            this.servicesCheckedListBox.Name = "servicesCheckedListBox";
            this.servicesCheckedListBox.Size = new System.Drawing.Size(719, 229);
            this.servicesCheckedListBox.TabIndex = 16;
            // 
            // selectAllbtn
            // 
            this.selectAllbtn.Location = new System.Drawing.Point(801, 214);
            this.selectAllbtn.Name = "selectAllbtn";
            this.selectAllbtn.Size = new System.Drawing.Size(75, 23);
            this.selectAllbtn.TabIndex = 17;
            this.selectAllbtn.Text = "Select All";
            this.selectAllbtn.UseVisualStyleBackColor = true;
            this.selectAllbtn.Click += new System.EventHandler(this.selectAllbtn_Click);
            // 
            // unSelectAllBtn
            // 
            this.unSelectAllBtn.Location = new System.Drawing.Point(892, 215);
            this.unSelectAllBtn.Name = "unSelectAllBtn";
            this.unSelectAllBtn.Size = new System.Drawing.Size(75, 23);
            this.unSelectAllBtn.TabIndex = 18;
            this.unSelectAllBtn.Text = "UNSelect All";
            this.unSelectAllBtn.UseVisualStyleBackColor = true;
            this.unSelectAllBtn.Click += new System.EventHandler(this.unSelectAllBtn_Click);
            // 
            // frmUnPlannedService
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(989, 645);
            this.Controls.Add(this.unSelectAllBtn);
            this.Controls.Add(this.selectAllbtn);
            this.Controls.Add(this.servicesCheckedListBox);
            this.Controls.Add(this.useOptionsCheckBox);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.serviceOptionsGroupBox);
            this.Controls.Add(this.schedulingGroupBox);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.addBtn);
            this.Controls.Add(this.priorityCmb);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "frmUnPlannedService";
            this.Text = "Add Unplanned Service";
            this.Load += new System.EventHandler(this.frmUnPlanedService_Load);
            this.schedulingGroupBox.ResumeLayout(false);
            this.schedulingGroupBox.PerformLayout();
            this.serviceOptionsGroupBox.ResumeLayout(false);
            this.serviceOptionsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
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
        private System.Windows.Forms.GroupBox serviceOptionsGroupBox;
        private System.Windows.Forms.Button removeOptionBtn;
        private System.Windows.Forms.Button addOptionBtn;
        private System.Windows.Forms.ListView optionsListView;
        private System.Windows.Forms.ColumnHeader Key;
        private System.Windows.Forms.ColumnHeader Value;
        private System.Windows.Forms.TextBox valueTxt;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox keyTxt;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox useOptionsCheckBox;
        private System.Windows.Forms.Button clearOptionsBtn;
        private System.Windows.Forms.CheckedListBox servicesCheckedListBox;
        private System.Windows.Forms.Button selectAllbtn;
        private System.Windows.Forms.Button unSelectAllBtn;
    }
}