using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace NewServiceHost
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[] 
			{ 
				new NewServiceHost() 
			};
#if DEBUG
			{
				NewServiceHost sh = new NewServiceHost();
				sh.InitalizeService();
			
			}
#else 
			{
			ServiceBase.Run(ServicesToRun);
			}
#endif
		}
	}
}
