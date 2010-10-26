using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace EdgeBI.Wizards.Utils.WizardTester
{
    public partial class FrmBookStringValuesReplace : Form
    {

        Dictionary<string, object> _strings = new Dictionary<string, object>();
        public FrmBookStringValuesReplace(Dictionary<string, object> strings)
        {
            int counter = 1;
            InitializeComponent();
            if (strings.Count != 0)
            {
                IDictionaryEnumerator ide = strings.GetEnumerator();
                while (ide.MoveNext())
                {
                    this.Controls["textbox" + counter].Text = ide.Value.ToString();
                    counter++;
                }
            }
        }
        public Dictionary<string, object> Strings
        {
            get
            {
                return this._strings;
            }
        }
        public FrmBookStringValuesReplace()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _strings.Clear();
            //string clinetTempName = string.Empty;
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if(!_strings.ContainsKey(listView1.Items[i].Text))
                    _strings.Add("AccountSettings.StringReplacment." + listView1.Items[i].Text, (string)listView1.Items[i].Tag);
            }
            //foreach (Control cntrl in this.Controls)
            //{
            //    if (cntrl.GetType().Name.ToLower().Equals("textbox") && !cntrl.Text.Equals(""))
            //    {
            //        _strings.Add("Client Specific" + cntrl.Name.Substring(7, cntrl.Name.Length - 7), cntrl.Text.Trim());
            //    }
            //}
            this.Close();
            //this.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ListViewItem lvi = new ListViewItem(new string[] {textBox1.Text, textBox2.Text});
            lvi.Tag = textBox2.Text;
            listView1.Items.Add(lvi);
			textBox1.Text = string.Empty;
			textBox2.Text = string.Empty;

        }

        private void button4_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listView1.SelectedItems.Count; )
            {
                listView1.Items.Remove(listView1.SelectedItems[i]);
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
        }

		private void BookStringValuesReplace_Load(object sender, EventArgs e)
		{

		}
    }
}
