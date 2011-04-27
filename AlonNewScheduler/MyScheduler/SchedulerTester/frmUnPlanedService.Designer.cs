namespace SchedulerTester
{
    partial class frmUnPlanedService
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
            this.timeToRunPicker = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.addBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
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
            this.servicesCmb.Location = new System.Drawing.Point(120, 9);
            this.servicesCmb.Name = "servicesCmb";
            this.servicesCmb.Size = new System.Drawing.Size(231, 21);
            this.servicesCmb.TabIndex = 1;
            // 
            // priorityCmb
            // 
            this.priorityCmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.priorityCmb.FormattingEnabled = true;
            this.priorityCmb.Location = new System.Drawing.Point(120, 49);
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
            // timeToRunPicker
            // 
            this.timeToRunPicker.Location = new System.Drawing.Point(120, 87);
            this.timeToRunPicker.Name = "timeToRunPicker";
            this.timeToRunPicker.Size = new System.Drawing.Size(200, 20);
            this.timeToRunPicker.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Time To Run";
            // 
            // addBtn
            // 
            this.addBtn.Location = new System.Drawing.Point(21, 123);
            this.addBtn.Name = "addBtn";
            this.addBtn.Size = new System.Drawing.Size(75, 23);
            this.addBtn.TabIndex = 8;
            this.addBtn.Text = "Add";
            this.addBtn.UseVisualStyleBackColor = true;
            this.addBtn.Click += new System.EventHandler(this.addBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.Location = new System.Drawing.Point(120, 123);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 9;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // frmUnPlanedService
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(423, 164);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.addBtn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.timeToRunPicker);
            this.Controls.Add(this.priorityCmb);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.servicesCmb);
            this.Controls.Add(this.label1);
            this.Name = "frmUnPlanedService";
            this.Text = "Add Unplaned Service";
            this.Load += new System.EventHandler(this.frmUnPlanedService_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox servicesCmb;
        private System.Windows.Forms.ComboBox priorityCmb;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker timeToRunPicker;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button addBtn;
        private System.Windows.Forms.Button CancelBtn;
    }
}