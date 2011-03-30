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
			_scheduler.NewScheduleCreatedEvent += new EventHandler(_scheduler_NewScheduleCreatedEvent);
			_scheduler.ServiceRunRequiredEvent += new EventHandler(_scheduler_ServiceRequired);			
			_scheduler.Start();
			
			
		}

		void _scheduler_NewScheduleCreatedEvent(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}


		void _scheduler_ServiceRequired(object sender, EventArgs e)
		{
			//todo: log2
			_scheduler.Stop();
			TimeToRunEventArgs args = (TimeToRunEventArgs)e;
			foreach (MyScheduler.Objects.ServiceInstance serviceInstance in args.ServicesToRun)
			{
				//FURTURE: ask system control if it's ok to run the service
				////if ok then
				serviceInstance.LegacyInstance.StateChanged += new EventHandler<ServiceStateChangedEventArgs>(LegacyInstance_StateChanged);
				serviceInstance.LegacyInstance.ChildServiceRequested += new EventHandler<ServiceRequestedEventArgs>(LegacyInstance_ChildServiceRequested);
				serviceInstance.LegacyInstance.Initialize();
			
				
			}
		}

		void LegacyInstance_ChildServiceRequested(object sender, ServiceRequestedEventArgs e)
		{
			e.RequestedService.Initialize();
		}

		void LegacyInstance_StateChanged(object sender, ServiceStateChangedEventArgs e)
		{
			Easynet.Edge.Core.Services.ServiceInstance instance = (Easynet.Edge.Core.Services.ServiceInstance)sender;
			if (e.StateAfter == ServiceState.Ready)
			{				
				instance.Start();	//TODO: TRY CATCH

				
			}
			
		}	

		

		protected override void OnStop()
		{

		}
	}
}
