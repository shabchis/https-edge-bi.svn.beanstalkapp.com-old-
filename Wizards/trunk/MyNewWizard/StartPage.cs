using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;
using Easynet.Edge.Core.Utilities;

namespace MyNewWizard
{
    public partial class StartPage : WizardForm.WizardPage
    {
        FrmWizard _parentForm;

        public StartPage()
        {
            InitializeComponent();
            stepReadyTimer.Tick += new EventHandler(stepReadyTimer_Tick);


        }

        void stepReadyTimer_Tick(object sender, EventArgs e)
        {
            stepReadyTimer.Stop();
            _parentForm.Controls["btnNewSession"].Visible = true;
            _parentForm.Controls["btnNewSession"].Enabled = true;
        }

       

       

        

        private void StartPage_Load(object sender, EventArgs e)
        {

        }
       

        private void StartPage_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == true)
            {
                InitalizePage();
                FrmWizard.AllCollectedValues.Clear();
            }
            
        }
    }
    public struct ApplicationType
    {
        public int ID { get; set; }
        public string ApplicationName { get; set; }
    }
}
