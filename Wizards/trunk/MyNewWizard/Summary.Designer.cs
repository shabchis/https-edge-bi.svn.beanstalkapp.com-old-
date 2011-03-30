namespace MyNewWizard
{
    partial class Summary
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
            this.txtSummary = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // txtSummary
            // 
            this.txtSummary.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtSummary.Location = new System.Drawing.Point(0, 93);
            this.txtSummary.Name = "txtSummary";
            this.txtSummary.ReadOnly = true;
            this.txtSummary.Size = new System.Drawing.Size(789, 292);
            this.txtSummary.TabIndex = 0;
            this.txtSummary.Text = "";
            // 
            // Summary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.txtSummary);
            this.Name = "Summary";
            this.StepDescription = "Summary";
            this.StepName = "Summary";
            this.Load += new System.EventHandler(this.Summary_Load);
            this.VisibleChanged += new System.EventHandler(this.Summary_VisibleChanged);
            this.Controls.SetChildIndex(this.txtSummary, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtSummary;
    }
}
