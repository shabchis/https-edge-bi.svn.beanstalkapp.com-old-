using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using Easynet.Edge.Core.Utilities;

namespace SchedulerTester
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
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(currentDomain_UnhandledException);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);            
			Application.Run(new frmSchedulingControl());
		}

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            
            MessageBox.Show(e.Exception.Message);
            Smtp.Send("SchedulerTester  exception", true, string.Format("Message:\n{0}\nInner Exception:\n{1}\nExeption.ToString():\n{2}\nIsTerminating:{3}\nStack:\n{4}", e.Exception.Message, e.Exception.InnerException, e.Exception, "true", e.Exception.StackTrace), false, string.Empty);
            Log.Write("SchedulerTester", e.Exception.Message, e.Exception, LogMessageType.Error);
            MessageBox.Show(e.Exception.Message);
        }

        static void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

            
            Exception ex=(Exception)e.ExceptionObject;           
            Smtp.Send("SchedulerTester  exception", true, string.Format("Message:\n{0}\nInner Exception:\n{1}\nExeption.ToString():\n{2}\nIsTerminating:{3}\nStack:\n{4}", ex.Message, ex.InnerException, ex, e.IsTerminating,ex.StackTrace), false, string.Empty);
            Log.Write("SchedulerTester", ex.Message, ex, LogMessageType.Error);
            MessageBox.Show(ex.Message);
            

        }
	}
}
