using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WizardForm;

namespace MyNewWizard
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			FrmWizard frmWizard = new FrmWizard();
			frmWizard.AddPage(new StartPage() );
            frmWizard.AddPage(new CreateAccount());
			frmWizard.AddPage(new ActiveDirectoryStep());
			frmWizard.AddPage(new CreateNewRole());
			frmWizard.AddPage(new CreateNewCube());
            frmWizard.AddPage(new Summary());
            frmWizard.AddPage(new ExecutePage());
			Application.Run(frmWizard);
		}
	}
}
