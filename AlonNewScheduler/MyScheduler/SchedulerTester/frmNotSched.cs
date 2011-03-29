using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SchedulerTester
{
	public partial class frmNotSched : Form
	{
		string notSched;
		public frmNotSched(string notSched)
		{
			try
			{
				InitializeComponent();
				this.notSched = notSched;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}


		}

		private void frmNotSched_Load(object sender, EventArgs e)
		{
			try
			{
				textBox1.Text = notSched;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

		}

	}
}
