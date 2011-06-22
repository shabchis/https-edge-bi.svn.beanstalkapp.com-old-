using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EdgeBI.Wizards;
using System.Text.RegularExpressions;
using System.Linq;
using Easynet.Edge.Core.Utilities;
using System.Data.SqlClient;
using System.Data;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;


namespace MyNewWizard
{
	public partial class CreateNewCube : WizardForm.WizardPage
	{
        FrmWizard _parentForm;
        Dictionary<string, object> _BEDataValues=new Dictionary<string, object>();
        Dictionary<string, object> _CPAData = new Dictionary<string, object>();
        Dictionary<string, object> _strings = new Dictionary<string, object>();
        Dictionary<int, string> _beDataUI = new Dictionary<int, string>();
        
		public CreateNewCube()
		{
			InitializeComponent();
            stepReadyTimer.Tick += new EventHandler(stepReadyTimer_Tick);
            
		}

        void stepReadyTimer_Tick(object sender, EventArgs e)
        {
            stepReadyTimer.Stop();
            StepStatus stepStatus = _parentForm.GetStepStatus();
            if (stepStatus == StepStatus.Ready)
            {
                _parentForm.Controls["btnNext"].Visible = true;
                _parentForm.Controls["btnNext"].Enabled = true;
                _parentForm.Controls["btnNewSession"].Enabled = true;
            }
            else
            {
                stepReadyTimer.Interval = interval;
                stepReadyTimer.Start();
                System.Windows.Forms.Application.DoEvents();

            }
        }
        public override Dictionary<string, object> CollectValues()
        {
            Dictionary<string, object> cubeValues = new Dictionary<string, object>();

            if (chkAddMeasure.Checked)
            {
                cubeValues.Add("AccountSettings.CreateMeasure", chkAddMeasure.Checked);
                cubeValues.Add("AccountSettings.MeasureAccountID", txtAccountID.Text.Trim());
            }
            
            // CPA
            bool onlyCalc;
            foreach (ListViewItem item in livCPA.Items)
            {
                if (item.SubItems[2].Text == "Y")
                    onlyCalc = true;
                else
                    onlyCalc = false;
                 if (!cubeValues.ContainsKey(item.Text))
                     cubeValues.Add(item.Text, new Replacment() { ReplaceFrom = string.Format("appsettings:{0}",item.Text), ReplaceTo = item.SubItems[1].Text, CalcMembersOnly = onlyCalc });
                 int targetCPANum = int.Parse(Regex.Match(item.Text, @"\d").Value);
                 if (!cubeValues.ContainsKey("AccountSettings.TargetACQ" + targetCPANum.ToString()))
                 {
                     cubeValues.Add("AccountSettings.TargetACQ" + targetCPANum.ToString(), new Replacment() { ReplaceFrom = string.Format("appsettings:AccountSettings.TargetACQ{0}", targetCPANum), ReplaceTo = item.SubItems[3].Text, CalcMembersOnly = onlyCalc });
                     if (!cubeValues.ContainsKey("AccountSettings.TargetValue"+targetCPANum.ToString()))
                         cubeValues.Add("AccountSettings.TargetValue"+targetCPANum.ToString(),item.SubItems[4].Text.Trim());

                 }

            }
            // Measures/Client specific (Be data + string replacement)
            foreach (ListViewItem item in listViewBeData.Items)
            {
                if (item.SubItems[2].Text == "Y")
                    onlyCalc = true;
                else
                    onlyCalc = false;
                if (item.Text.StartsWith("Client Specific"))
                {
                    if (!cubeValues.ContainsKey("AccountSettings." + item.Text))
                    {

                        cubeValues.Add("AccountSettings." + item.Text, new Replacment() { ReplaceFrom = item.Text, ReplaceTo = item.SubItems[1].Text, CalcMembersOnly = onlyCalc });
                    }

                }
                else
                {
                    if (!cubeValues.ContainsKey("AccountSettings.StringReplacment." + item.Text))
                    {
                        cubeValues.Add("AccountSettings.StringReplacment." + item.Text, new Replacment() { ReplaceFrom = item.Text, ReplaceTo = item.SubItems[1].Text, CalcMembersOnly = onlyCalc });

                    }

                }


            }


            //Only string replacement
            foreach (ListViewItem item in livStrings.Items)
            {
                if (item.SubItems[2].Text == "Y")
                    onlyCalc = true;
                else
                    onlyCalc = false;

                if (!cubeValues.ContainsKey("AccountSettings.StringReplacment." + item.Text))
                {

                    cubeValues.Add("AccountSettings.StringReplacment." + item.Text, new Replacment() { ReplaceFrom = item.Text, ReplaceTo = item.SubItems[1].Text, CalcMembersOnly = onlyCalc });
                }


            }



            
            cubeValues.Add("AccountSettings.CubeName", FrmWizard.AllCollectedValues["AccountSettings.BI_Scope_Name"].ToString().Trim());
            cubeValues.Add("AccountSettings.CubeID", FrmWizard.AllCollectedValues["AccountSettings.BI_Scope_Name"].ToString().Trim());





            if (chkContent.Checked)
                cubeValues.Add("AccountSettings.AddContentCube", bool.Parse("true"));
            else
                cubeValues.Add("AccountSettings.AddContentCube", bool.Parse("false"));

            if (chkProcessCubes.Checked)
                cubeValues.Add("AccountSettings.ProcessCubes", bool.Parse("true"));
            else
                cubeValues.Add("AccountSettings.ProcessCubes", bool.Parse("false"));

            return cubeValues;
        }

        private void btnAddBe_Click(object sender, EventArgs e)
        {
            ListViewItem liv = null;

            string calcOnly = string.Empty; ;
            if (chkBeOnlyCalc.Checked)
                calcOnly = "Y";
            else
                calcOnly = "N";
            switch (cmbBeData.Text)
            {

                case "BEData":
                    {
                        if (_beDataUI.Count == 0)
                        {
                            liv = new ListViewItem(new string[] { string.Format("BO Client Specific{0}", 1), txtBeReplace.Text, calcOnly });
                            listViewBeData.Items.Add(liv);
                            _beDataUI.Add(1, string.Format("BO Client Specific{0}", 1));
                        }
                        else
                        {
                            for (int index = 1; index < _beDataUI.Count; index++)
                            {
                                if (!_beDataUI.ContainsKey(index))
                                {
                                    liv = new ListViewItem(new string[] { string.Format("BO Client Specific{0}", index), txtBeReplace.Text, calcOnly });
                                    listViewBeData.Items.Add(liv);
                                    _beDataUI.Add(index, string.Format("BO Client Specific{0}", index));
                                   

                                }

                            }
                        }
                        break;
                        //int i = 1;
                        //string patern = @"\bClient Specific";
                        //foreach (ListViewItem item in listViewBeData.Items)
                        //{
                        //    if (Regex.IsMatch(item.Text, patern))
                        //    {
                        //        i++;
                        //    }
                        //}

                        //liv = new ListViewItem(new string[] { string.Format("BO Client Specific{0}", i), txtBeReplace.Text, calcOnly });
                        //listViewBeData.Items.Add(liv);
                       
                    }
                case " ":
                    {
                        //Do Nothing
                        break;
                    }
                default:
                    {
                        bool exist = false;
                        liv = new ListViewItem(new string[] { cmbBeData.Text, txtBeReplace.Text, calcOnly });
                        foreach (ListViewItem item in listViewBeData.Items)
                        {
                            if (item.SubItems[0].Text.Trim() == liv.Text)
                            {
                                exist = true;
                                MessageBox.Show("Two items can't be with the same name");
                            }


                        }
                        if (!exist)
                            listViewBeData.Items.Add(liv);


                        break;
                    }
            }
        }

        private void btnRemoveBeData_Click(object sender, EventArgs e)
        {
            //int i = 1;
            //string patern = @"\bClient Specific";
            //foreach (ListViewItem item in listViewBeData.Items)
            //{
            //    if (Regex.IsMatch(item.Text, patern))
            //    {
            //        i++;
            //    }
            //}
            string patern = @"\bClient Specific";
            foreach (ListViewItem item in listViewBeData.SelectedItems)
            {
                if (Regex.IsMatch(item.Text, patern))
                {
                    int index = int.Parse(Regex.Match(item.Text, @"\d").ToString());
                    _beDataUI.Remove(index);
                }
                item.Remove();

            }
        }

        private void btnClearBeData_Click(object sender, EventArgs e)
        {
            listViewBeData.Items.Clear();
            _beDataUI.Clear();
        }

        private void btnAddCPA_Click(object sender, EventArgs e)
        {
            ListViewItem liv = null;

            string calcOnly = string.Empty; ;
            if (chkOnlyCalcCPA.Checked)
                calcOnly = "Y";
            else
                calcOnly = "N";

            bool exist = false;
            int cpaNum = 0;
            cpaNum=int.Parse(Regex.Match(cmbCPA.Text, @"\d").Value);
           // liv = new ListViewItem(new string[] { cmbCPA.Text, cmbCpaReplaceTo.Text, calcOnly,txtTargetAcquisition.Text });
            liv = new ListViewItem(new string[] { string.Format("AccountSettings.ACQ{0}", cpaNum), cmbCpaReplaceTo.Text, calcOnly, txtTargetAcquisitionName.Text ,txtTargetCPA.Text});
            foreach (ListViewItem item in livCPA.Items)
            {
                if (item.SubItems[0].Text.Trim() == liv.Text)
                {
                    exist = true;
                    MessageBox.Show("Two items can't be with the same name");
                }
               

            }
            if (!exist)
                livCPA.Items.Add(liv);

            if (livCPA.Items.Count > 0)
                chkAddMeasure.Enabled = true;
            else
                chkAddMeasure.Enabled = false;
           

          




        
        }

        private void btnRemoveCpa_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in livCPA.SelectedItems)
            {
                item.Remove();

            }
            if (livCPA.Items.Count > 0)
                chkAddMeasure.Enabled = true;
            else
                chkAddMeasure.Enabled = false;
        }

        private void btnClearCPA_Click(object sender, EventArgs e)
        {
            livCPA.Items.Clear();
            if (livCPA.Items.Count > 0)
                chkAddMeasure.Enabled = true;
            else
                chkAddMeasure.Enabled = false;
        }

        private void btnAddStringReplace_Click(object sender, EventArgs e)
        {
            ListViewItem liv = null;

            string calcOnly = string.Empty; ;
            if (chkCalcOnlyString.Checked)
                calcOnly = "Y";
            else
                calcOnly = "N";

            bool exist = false;
            liv = new ListViewItem(new string[] { txtString.Text, txtStringReplace.Text, calcOnly });
            foreach (ListViewItem item in livStrings.Items)
            {
                if (item.SubItems[0].Text.Trim() == liv.Text)
                {
                    exist = true;
                    MessageBox.Show("Two items can't be with the same name");
                }


            }
            if (!exist)
                livStrings.Items.Add(liv);
            
        }

        private void btnRemoveStringReplace_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in livStrings.SelectedItems)
            {
                item.Remove();

            }
        }

        private void btnClearStrings_Click(object sender, EventArgs e)
        {
            livStrings.Items.Clear();
        }

        void CreateNewCube_Load(object sender, System.EventArgs e)
        {

          
        }
        private void InitLaizeComboBoxs()
        {
            //Clean the combo boxs

            cmbBeData.DataSource = null;

            _BEDataValues.Clear();
            //Load default values from app.config
            string[] googleDefaultValues = FrmWizard.GetFromWizardSettings("EdgeBI.Wizards.GoogleConversions").Split(',');

            foreach (string str in googleDefaultValues)
            {

                if (!_BEDataValues.ContainsKey("AccountSettings." + str))
                {
                    _BEDataValues.Add("AccountSettings." + str, str);
                }

            }

            _BEDataValues.Add("BEData", "BEData");
            int numOfCpa= FrmWizard.GetCountLikeSettings("AccountSettings.ACQ");
           for (int i = 1; i <= numOfCpa; i++)
			{
               string value="AccountSettings.ACQ"+ i.ToString();
               if (!_CPAData.ContainsKey("AccountSettings." + value))
                {
                    _CPAData.Add("AccountSettings." + value, "Acquisition" + i.ToString());
                }
			 
			}
            
                
           


            cmbBeData.DataSource = _BEDataValues.ToList();
            cmbBeData.ValueMember = "Key";
            cmbBeData.DisplayMember = "Value";

            cmbCPA.DataSource = _CPAData.ToList();
            cmbCPA.ValueMember = "Key";
            cmbCPA.DisplayMember = "Value";


        }

        private void tabsCreateCube_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage.Name == "tabCPAS")
            {
                FillCPAReplaceComboBox();
            }
        }

       
        private void FillCPAReplaceComboBox()
        {
            cmbCpaReplaceTo.DataSource = null;
            List<string> comboValues = new List<string>();


            //Load default values from app.config
            string[] googleDefaultValues = FrmWizard.GetFromWizardSettings("EdgeBI.Wizards.GoogleConversions").Split(',');

            foreach (string str in googleDefaultValues)
            {

                string itemReplaced = str;
                foreach (ListViewItem item in listViewBeData.Items)
                {

                    if (item.Text == str)
                    {
                        itemReplaced = item.SubItems[1].Text;
                        break;


                    }
                }
                comboValues.Add(itemReplaced);

            }

            foreach (ListViewItem item in listViewBeData.Items)
            {
                if (item.SubItems[0].Text.StartsWith("BO Client Specific"))
                {
                    comboValues.Add(item.SubItems[1].Text.Trim());

                }
            }



            cmbCpaReplaceTo.DataSource = comboValues;
            //cmbBeData.ValueMember = "Key";
            //cmbBeData.DisplayMember = "Value";

            //cmbCPA.DataSource = _CPAData.ToList();
            //cmbCPA.ValueMember = "Key";
            //cmbCPA.DisplayMember = "Value";
        }

        protected override void InitalizePage()
        {
            _parentForm = (FrmWizard)ParentForm;
            _parentForm.Text = this.StepDescription;
            InitLaizeComboBoxs();
            if (FrmWizard.AllCollectedValues.ContainsKey("AccountSettings.AccountID"))
                txtAccountID.Text = FrmWizard.AllCollectedValues["AccountSettings.AccountID"].ToString();
            if (chkAddMeasure.Checked)
                txtAccountID.Enabled = true;
            else
                txtAccountID.Enabled = false;
          
            stepReadyTimer.Interval = interval;
            stepReadyTimer.Start();
        }

        private void CreateNewCube_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == true)
                InitalizePage();
            else
                base.SetButtons();
        }

        private void cmbBeData_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cmbBeData.Text == "BEData")
            {
                chkBeOnlyCalc.Checked = false;

            }
        }

        private void cmbCpaReplaceTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void cmbCpaReplaceTo_Validated(object sender, EventArgs e)
        {
            txtTargetAcquisitionName.Text = string.Format(Properties.Settings.Default.targetCPADefault, cmbCpaReplaceTo.Text);
        }

        private void tabsCreateCube_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void tabsCreateCube_TabIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = System.IO.Path.Combine(Application.StartupPath, Properties.Settings.Default.FloatDiagram);
            p.Start();
        }

        private void livCPA_Validating(object sender, CancelEventArgs e)
        {
            if (livCPA.Items.Count > 0)
            {
                chkAddMeasure.Enabled = true;
                txtAccountID.Enabled = true;
            }
            else
            {
                chkAddMeasure.Enabled = false;
                
            }

        }

        private void chkAddMeasure_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAddMeasure.Checked)
                txtAccountID.Enabled = true;
            else
                txtAccountID.Enabled = false;
        }

        private void txtTargetCPA_Validated(object sender, EventArgs e)
        {
            if (txtTargetCPA.Text.Length > 0)
                txtAccountID.Enabled = true;
        }

       
        
        

	}
}
