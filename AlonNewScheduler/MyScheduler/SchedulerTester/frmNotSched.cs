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
			InitializeComponent();
			this.notSched = notSched;
		}

		private void frmNotSched_Load(object sender, EventArgs e)
		{
			textBox1.Text = notSched;
		}

	}
}
