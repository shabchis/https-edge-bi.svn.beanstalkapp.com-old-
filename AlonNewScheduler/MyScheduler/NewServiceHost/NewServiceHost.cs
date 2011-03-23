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
using MyScheduler.Objects;

namespace NewServiceHost
{
	public partial class NewServiceHost : ServiceBase
	{
		Scheduler _scheduler;
		System.Timers.Timer _timer;
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
			_scheduler.Start();
			_timer = new System.Timers.Timer(60000);
			_timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
			_timer.Start();
			
			
		}

		void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			_timer.Stop();
			_scheduler.Stop();
			_scheduler.NewSchedule();
			_scheduler.NewSchedule();
			_scheduler.Start();
			
		}

		void _scheduler_TimeToRunEventHandler(object sender, EventArgs e)
		{
			//todo: log2
			_scheduler.Stop();
			TimeToRunEventArgs args = (TimeToRunEventArgs)e;
			foreach (KeyValuePair<SchedulingData, ActiveServiceElement> activeServiceElement in args.ActiveServiceElements)
			{
				//todo: ask system control if it's ok to run the service
				//if ok then
				Easynet.Edge.Core.Services.ServiceInstance serviceInstance = Service.CreateInstance(activeServiceElement.Value);
				serviceInstance.StateChanged += new EventHandler<ServiceStateChangedEventArgs>(serviceInstance_StateChanged);
				serviceInstance.ChildServiceRequested += new EventHandler<ServiceRequestedEventArgs>(serviceInstance_ChildServiceRequested);
				//TODO: SET THE SCHEDULING INFO ON THE SERVICE INSTANCE
				serviceInstance.Initialize();
				_scheduler.SetServiceState(activeServiceElement.Key, serviceStatus.Runing);
				_scheduler.Start();
				
			}
		}

		void serviceInstance_ChildServiceRequested(object sender, ServiceRequestedEventArgs e)
		{
			e.RequestedService.Initialize();
		}

		void serviceInstance_StateChanged(object sender, ServiceStateChangedEventArgs e)
		{
			//todo: log1
			Easynet.Edge.Core.Services.ServiceInstance instance = (Easynet.Edge.Core.Services.ServiceInstance) sender;
			if (e.StateAfter == ServiceState.Ready)
			{
				try
				{
					instance.Start();
				// TODO:	_scheduler.SetServiceState(activeServiceElement.Key, serviceStatus.Runing);
					
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
