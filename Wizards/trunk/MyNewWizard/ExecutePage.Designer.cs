namespace MyNewWizard
{
    partial class ExecutePage
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
            this.progressExecute = new System.Windows.Forms.ProgressBar();
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progressExecute
            // 
            this.progressExecute.Location = new System.Drawing.Point(80, 255);
            this.progressExecute.Name = "progressExecute";
            this.progressExecute.Size = new System.Drawing.Size(665, 23);
            this.progressExecute.TabIndex = 0;
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(80, 99);
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.Size = new System.Drawing.Size(665, 124);
            this.txtLog.TabIndex = 1;
            this.txtLog.Text = "Executing.....";
            this.txtLog.TextChanged += new System.EventHandler(this.txtLog_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(80, 236);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Executing Progress:";
            // 
            // ExecutePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.progressExecute);
            this.Name = "ExecutePage";
            this.StepDescription = "Execution";
            this.StepName = "Execute";
            this.Load += new System.EventHandler(this.ExecutePage_Load);
            this.VisibleChanged += new System.EventHandler(this.ExecutePage_VisibleChanged);
            this.Controls.SetChildIndex(this.progressExecute, 0);
            this.Controls.SetChildIndex(this.txtLog, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressExecute;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.Label label1;
    }
}
