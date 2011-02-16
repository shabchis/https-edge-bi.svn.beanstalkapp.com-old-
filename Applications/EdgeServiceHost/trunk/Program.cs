using System;
using System.Reflection;
using System.Threading;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;

namespace Easynet.Edge.Applications 
{
    
	// blah blah ALON
	class EdgeServiceHost
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Edge Service Host v. {0}", Assembly.GetExecutingAssembly().GetName().Version);
			Console.WriteLine("===================================");
			Console.WriteLine();

			// Check command line
			if (args.Length < 1 || !args[0].StartsWith("/"))
			{
				Console.WriteLine("Please start the program with a service name as an argument, e.g.: EdgeServiceHost.exe /ScheduleManager");
				Console.ReadLine();
				return;
			}
			string serviceName = args[0].Substring(1);

            bool firstInstance = false;
            Mutex mutex = new Mutex(false, String.Format("Global\\EdgeServiceHost-{0}", serviceName), out firstInstance);

            if (!firstInstance)
            {
                Log.Write(typeof(EdgeServiceHost).Name, String.Format("Trying to start a service host which is already running {0}; exiting.", serviceName), LogMessageType.Information);
                return;
            }

			// Check if the service exists
			ServiceElement serviceToRun = ServicesConfiguration.Services.GetService(serviceName);
			if (serviceToRun == null)
			{ 
				Console.WriteLine("Invalid service name (" + serviceName+ " ). Press any key to exit.");
				Console.ReadLine();
				return;
			}

			// Init the service
			Console.WriteLine("Initializing {0}...", serviceToRun.Name);

			ServiceInstance instance;
			SchedulingRuleElement alwaysOn;
			try 
			{
				instance = Service.CreateInstance(serviceToRun);
				alwaysOn = new SchedulingRuleElement();
				alwaysOn.CalendarUnit = CalendarUnit.AlwaysOn;
				instance.ActiveSchedulingRule = alwaysOn;
				instance.StateChanged += new EventHandler<ServiceStateChangedEventArgs>(instance_StateChanged);
				instance.Initialize();
			}
			catch (Exception ex)
			{
				Log.Write(typeof(EdgeServiceHost).Name, String.Format("Failed to initialize the service. Service name: {0}.", serviceToRun.Name), ex);
				Console.WriteLine(String.Format("Failed to initialize the service. Service name: {0}. + Exception message: {1}", serviceToRun.Name, ex.Message.ToString()));
				Console.ReadLine();
				return;
			}

			// Wait for exit input
			while (Console.ReadKey().Key != ConsoleKey.Escape);

			// Abort the service if it is still running
			if (instance.State != ServiceState.Ended)
			{
				Console.WriteLine("\nAborting {0}...", serviceToRun.Name);

				try
				{
					instance.Abort();
				}
				catch (Exception ex)
				{
					//Log.Write(typeof(EdgeServiceHost).Name, String.Format("Could not abort {0}.", instance.Configuration.Name), ex);
				}
			}
		}

		// Catch state change events
		static void instance_StateChanged(object sender, ServiceStateChangedEventArgs e)
		{
			ServiceInstance instance = (ServiceInstance) sender;
			if (e.StateAfter == ServiceState.Ready)
			{
				Console.WriteLine("Starting {0}\n", instance.Configuration.Name);

				try
				{
					instance.Start();
				}
				catch (Exception ex)
				{
					Console.WriteLine("Could not start {0}: {1} ({2})", 
						instance.Configuration.Name,
						ex.Message,
						ex.GetType().FullName);
					Console.WriteLine("See the event log for further details.");

					Log.Write(typeof(EdgeServiceHost).Name, String.Format("Could not start {0}.", instance.Configuration.Name), ex);
				}
			}
			else if (e.StateAfter == ServiceState.Ended)
			{
				Console.WriteLine("{0} has ended.", instance.Configuration.Name);
			}
		}
	}
}
