using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using MyScheduler;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Services;

namespace NewServiceHost
{
	public partial class NewServiceHost : ServiceBase
	{
		Scheduler _scheduler;
		public NewServiceHost()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			InitalizeService();
		}

		public void InitalizeService()
		{
			_scheduler = new Scheduler(true);
			_scheduler.ServiceNotScheduledHandler += new EventHandler(_scheduler_ServiceNotScheduledHandler);
			_scheduler.TimeToRunEventHandler += new EventHandler(_scheduler_TimeToRunEventHandler);
			_scheduler.NewSchedule();

			
		}

		void _scheduler_TimeToRunEventHandler(object sender, EventArgs e)
		{
			//todo: log2
			TimeToRunEventArgs args = (TimeToRunEventArgs)e;
			foreach (ActiveServiceElement activeServiceElement in args.ActiveServiceElements)
			{
				//todo: ask system control if it's ok to run the service
				//if ok then
				ServiceInstance serviceInstance = Service.CreateInstance(activeServiceElement);
				serviceInstance.StateChanged += new EventHandler<ServiceStateChangedEventArgs>(serviceInstance_StateChanged);
				serviceInstance.ChildServiceRequested += new EventHandler<ServiceRequestedEventArgs>(serviceInstance_ChildServiceRequested);
				serviceInstance.Initialize();
				
			}
		}

		void serviceInstance_ChildServiceRequested(object sender, ServiceRequestedEventArgs e)
		{
			throw new NotImplementedException();
		}

		void serviceInstance_StateChanged(object sender, ServiceStateChangedEventArgs e)
		{
			//todo: log1
			ServiceInstance instance = (ServiceInstance) sender;
			if (e.StateAfter == ServiceState.Ready)
			{
				try
				{
					instance.Start();
				}
				catch (Exception ex)
				{
					
				}
			}
			else if (e.StateAfter == ServiceState.Ended)
			{
				
			}
		}

		void _scheduler_ServiceNotScheduledHandler(object sender, EventArgs e)
		{
			//todo: what to do if service not schedule
		}

		protected override void OnStop()
		{

		}
	}
}
