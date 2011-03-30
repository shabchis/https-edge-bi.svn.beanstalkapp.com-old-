using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EdgeBI.Wizards;

namespace MyNewWizard
{
    public partial class Summary : WizardForm.WizardPage
    {
        FrmWizard _parentForm;
        public Summary()
        {
            InitializeComponent();
            stepReadyTimer.Tick += new EventHandler(stepReadyTimer_Tick);
        }

        void stepReadyTimer_Tick(object sender, EventArgs e)
        {
            stepReadyTimer.Stop();
            
            
                _parentForm.Controls["btnFinish"].Visible = true;
                _parentForm.Controls["btnFinish"].Enabled = true;
                _parentForm.Controls["btnNewSession"].Enabled = true;
           
        }

        private void Summary_Load(object sender, EventArgs e)
        {
           
        }
        protected override void InitalizePage()
        {
            _parentForm = (FrmWizard)ParentForm;
            _parentForm.Text = this.StepDescription;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(((FrmWizard)Parent.Parent).StrSummary);
            stringBuilder.AppendLine("\n Collected Data:\n");
           
            foreach (KeyValuePair<string,object> item in FrmWizard.AllCollectedValues)
            {
                stringBuilder.AppendLine(string.Format("Key: {0} : Value: {1}",item.Key,item.Value));
                
                
            }
            txtSummary.Text=stringBuilder.ToString();
            
            stepReadyTimer.Interval = 6000;
            stepReadyTimer.Start();
        }

        private void Summary_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == true)
                InitalizePage();
            else
                base.SetButtons();
        }
    }
}
