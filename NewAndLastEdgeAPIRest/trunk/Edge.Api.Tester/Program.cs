using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NewRestApiTester__
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += new UnhandledExceptionEventHandler(Form1.currentDomain_UnhandledException);
			Application.EnableVisualStyles(); 
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}

		
	}
}
