using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Text.RegularExpressions;

namespace EdgeBI.Wizards.Utils.WizardTester
{
    public partial class FrmMeasuresList : Form
    {
        Dictionary<string, object> _measures = new Dictionary<string, object>();
        public FrmMeasuresList(Dictionary<string, object> measures)
        {
            int counter = 1;
            InitializeComponent();
            if (measures.Count != 0)
            {
                IDictionaryEnumerator ide = measures.GetEnumerator();
                while (ide.MoveNext())
                {
					Match match = Regex.Match(ide.Key.ToString(),@"\d");
                    if (!string.IsNullOrEmpty(match.ToString()) && match.ToString()!="0")
                    {
                    this.Controls[string.Format("textbox{0}", match.ToString())].Text = ide.Value.ToString();
                    }
                    counter++;
                }
            }
        }

        private void MeasuresList_Load(object sender, EventArgs e)
        {

        }
        public Dictionary<string, object> Measures
        {
            get
            {
                return this._measures;
            }
        }
        private void MeasuresList_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string clinetTempName = string.Empty;
            foreach (Control cntrl in this.Controls)
            {
                if (cntrl.GetType().Name.ToLower().Equals("textbox") && !cntrl.Text.Equals(""))
                {
					_measures.Add("AccountSettings.Client Specific" + cntrl.Name.Substring(7, cntrl.Name.Length - 7), cntrl.Text.Trim());
                }
            }
            this.Close();
            this.Dispose();
        }
    }
}
