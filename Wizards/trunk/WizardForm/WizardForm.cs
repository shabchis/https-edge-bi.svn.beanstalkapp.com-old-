using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WizardForm
{
	public partial class WizardForm : Form
	{
		protected Dictionary<string, WizardPage> _wizardPages;
		protected int _session;
		protected string _nextStep;
		protected string _previousStep;

		
		public WizardForm()
		{
			InitializeComponent();
			_wizardPages = new Dictionary<string, WizardPage>();
			
			
			
		}

		private void LoadStep(string p)
		{
			foreach (WizardPage page in _wizardPages.Values)
			{
				if (page.StepName==p)
				{
					page.Visible = true;
					page.SetTitle();
					page.SetButtons();
					
				}
				
			}
		}

		public void AddPage(WizardPage step)
		{
			if (_wizardPages.ContainsKey(step.StepName))
			{
				throw new Exception("Wizard already contain this step");
			}
			else
			{
				this.mainPanel.Controls.Add(step);
				step.Visible = true;
				
				
				step.Dock = DockStyle.Fill;
				
				step.Visible = false;
				_wizardPages.Add(step.StepName, step);
			}
		}

		private void WizardForm_Load(object sender, EventArgs e)
		{
			LoadStep("Start");
		}

		
		private void StartNewSession()
		{
			_nextStep = "ActiveDirectoryStepCollector";
			_previousStep = "Start";
			
		}
		private void loadStep()
		{
			_wizardPages[_previousStep].Visible = false;
			_wizardPages[_nextStep].Visible = true;
		}

        private void btnNewSession_Click(object sender, EventArgs e)
        {

        }

	}
}
