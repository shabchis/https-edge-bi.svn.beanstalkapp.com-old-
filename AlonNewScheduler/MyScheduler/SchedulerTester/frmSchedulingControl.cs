using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MyScheduler;
using MyScheduler.Objects;
using System.IO;
using legacy = Easynet.Edge.Core.Services;


namespace SchedulerTester
{
	public partial class frmSchedulingControl : Form
	{
		StringBuilder _str = new StringBuilder();
		private Scheduler _scheduler;
		private Dictionary<SchedulingData, ServiceInstance> _scheduledServices = new Dictionary<SchedulingData, ServiceInstance>();
		public delegate void SetLogMethod(string lineText);
		public delegate void UpdateGridMethod(legacy.ServiceInstance serviceInstance);
		SetLogMethod setLogMethod;
		UpdateGridMethod updateGridMethod;
		public frmSchedulingControl()
		{
			InitializeComponent();
			
			setLogMethod = new SetLogMethod(SetLogTextBox);
			updateGridMethod = new UpdateGridMethod(UpdateGridData);
			this.FormClosed += new FormClosedEventHandler(frmSchedulingControl_FormClosed);
		}

		void frmSchedulingControl_FormClosed(object sender, FormClosedEventArgs e)
		{
			_scheduler.Stop();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			_scheduler = new Scheduler(true);
			_scheduler.ServiceRunRequiredEvent += new EventHandler(_scheduler_ServiceRunRequiredEvent);
			_scheduler.NewScheduleCreatedEvent += new EventHandler(_scheduler_NewScheduleCreatedEventHandler);
			//	_scheduler.Start();

		}

		void _scheduler_ServiceRunRequiredEvent(object sender, EventArgs e)
		{
			//todo: log2


			TimeToRunEventArgs args = (TimeToRunEventArgs)e;
			foreach (MyScheduler.Objects.ServiceInstance serviceInstance in args.ServicesToRun)
			{

				this.Invoke(setLogMethod, new Object[] { string.Format("Service: {0} is required for running time {1}\r\n", serviceInstance.ServiceName, DateTime.Now.ToString("dd/MM/yy HH:mm")) });

				//FURTURE: ask system control if it's ok to run the service
				////if ok then
				serviceInstance.LegacyInstance.StateChanged += new EventHandler<Easynet.Edge.Core.Services.ServiceStateChangedEventArgs>(LegacyInstance_StateChanged);
				serviceInstance.LegacyInstance.ChildServiceRequested += new EventHandler<Easynet.Edge.Core.Services.ServiceRequestedEventArgs>(LegacyInstance_ChildServiceRequested);
				serviceInstance.LegacyInstance.Initialize();
				this.Invoke(setLogMethod, new Object[] { string.Format("\nService: {0} initalized {1}\r\n", serviceInstance.ServiceName, DateTime.Now.ToString("dd/MM/yy HH:mm")) });

				Easynet.Edge.Core.Utilities.Log.Write(this.ToString(), string.Format("Service: {0} initalized", serviceInstance.ServiceName), Easynet.Edge.Core.Utilities.LogMessageType.Information);


			}

		}

		void LegacyInstance_ChildServiceRequested(object sender, Easynet.Edge.Core.Services.ServiceRequestedEventArgs e)
		{

			legacy.ServiceInstance instance = (legacy.ServiceInstance)sender;
			this.Invoke(setLogMethod, new Object[] { string.Format("\nChild Service: {0} requestedd {1}\r\n", e.RequestedService.Configuration.Name, DateTime.Now.ToString("dd/MM/yy HH:mm")) });

			e.RequestedService.ChildServiceRequested += new EventHandler<legacy.ServiceRequestedEventArgs>(LegacyInstance_ChildServiceRequested);
			e.RequestedService.StateChanged += new EventHandler<legacy.ServiceStateChangedEventArgs>(LegacyInstance_StateChanged);
			e.RequestedService.Initialize();
		}



		void LegacyInstance_StateChanged(object sender, Easynet.Edge.Core.Services.ServiceStateChangedEventArgs e)
		{

			legacy.ServiceInstance instance = (Easynet.Edge.Core.Services.ServiceInstance)sender;
			this.Invoke(updateGridMethod, new Object[] { instance });


			this.Invoke(setLogMethod, new Object[] { string.Format("\n{0}: {1} is {2} {3}\r\n", instance.AccountID, instance.Configuration.Name, e.StateAfter, DateTime.Now.ToString("dd/MM/yy HH:mm")) });
			if (e.StateAfter == legacy.ServiceState.Ready)
				instance.Start();

		}

		void _scheduler_NewScheduleCreatedEventHandler(object sender, EventArgs e)
		{
			this.Invoke(setLogMethod, new Object[] { "Schedule Created:" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "\r\n" });

			using (StreamWriter writer = new StreamWriter(Path.Combine(Application.StartupPath, "log.txt"), true, Encoding.Unicode))
			{
				ScheduledInformationEventArgs ee = (ScheduledInformationEventArgs)e;
				_str.Clear();
				_scheduledServices.Clear();



				writer.WriteLine("Schedule Created" + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

				foreach (KeyValuePair<SchedulingData, ServiceInstance> notSchedInfo in ee.NotScheduledInformation)
				{
					_str.AppendLine(string.Format("Service {0} with profile {1} not scheduled", notSchedInfo.Value.ServiceName, notSchedInfo.Key.profileID));

					writer.WriteLine(string.Format("Service {0} with profile {1} not scheduled", notSchedInfo.Value.ServiceName, notSchedInfo.Key.profileID));


				}
				foreach (KeyValuePair<SchedulingData, ServiceInstance> SchedInfo in ee.ScheduleInformation)
				{

					writer.WriteLine(string.Format("ScheduleInfoID:{0}\tService Name:{1}\tAccountID:{2}\tStartTime:{3}\tEndTime\tStatus:{4}\tScope{5}\tDeleted:{6}\tResult:{7}", SchedInfo.Key.GetHashCode(), SchedInfo.Value.ServiceName, SchedInfo.Value.ProfileID, SchedInfo.Value.StartTime.ToString("dd/MM/yyyy,HH:mm"), SchedInfo.Value.EndTime.ToString("dd/MM/yyyy,HH:mm"), SchedInfo.Value.LegacyInstance.State, SchedInfo.Key.Rule.Scope, SchedInfo.Value.Deleted, SchedInfo.Value.LegacyInstance.Outcome));


					_scheduledServices.Add(SchedInfo.Key, SchedInfo.Value);
				}
				GetScheduleServices();

				if (!string.IsNullOrEmpty(_str.ToString()))
				{
					frmNotSched frm = new frmNotSched(_str.ToString());
					frm.Show();
				}
			}

		}

		private void ScheduleBtn_Click(object sender, EventArgs e)
		{
			_str.Clear();
			_scheduledServices.Clear();
			_scheduler.NewSchedule();


		}

		private void GetScheduleServices()
		{
			var scheduledServices = _scheduler.GetAlllScheduldServices();
			this.Invoke(new Action<IEnumerable<KeyValuePair<MyScheduler.Objects.SchedulingData, ServiceInstance>>>(SetGridData), scheduledServices);
		}

		
		private void UpdateGridData(legacy.ServiceInstance serviceInstance)
		{
			foreach (DataGridViewRow row in scheduleInfoGrid.Rows)
			{
				if (Object.Equals(row.Tag, serviceInstance))
				{
					row.Cells["dynamicStaus"].Value = serviceInstance.State;

					Color color = GetColorByState(serviceInstance.State, serviceInstance.Outcome);
					row.DefaultCellStyle.BackColor = color;
				}
			}

		}

		private static Color GetColorByState(legacy.ServiceState state,legacy.ServiceOutcome outCome)
		{
			Color color = Color.White;
			switch (state)
			{

				case Easynet.Edge.Core.Services.ServiceState.Uninitialized:
					break;
				case Easynet.Edge.Core.Services.ServiceState.Initializing:
					color = Color.FromArgb(0xdd, 0xdd, 0xdd); // light gray
					break;
				case Easynet.Edge.Core.Services.ServiceState.Ready:
					color = Color.FromArgb(0xee, 0xee, 0xff); // light blue
					break;
				case Easynet.Edge.Core.Services.ServiceState.Starting:
					color = Color.Green; // green
					break;
				case Easynet.Edge.Core.Services.ServiceState.Running:
					color = Color.FromArgb(0xe0, 0xff, 0xe0); // light green
					break;
				case Easynet.Edge.Core.Services.ServiceState.Waiting:
					color = Color.FromArgb(0xee, 0xff, 0xee); // light green (a little lighter)
					break;
				case Easynet.Edge.Core.Services.ServiceState.Ended:
					{
						if (outCome == legacy.ServiceOutcome.Success)
							color = Color.Turquoise;
						else if (outCome == legacy.ServiceOutcome.Failure)
							color = Color.DarkRed;
						else if (outCome == legacy.ServiceOutcome.Unspecified)
							color = Color.Orange;
						else if (outCome == legacy.ServiceOutcome.Aborted)
							color = Color.Purple;
						break;

					}
					
				case Easynet.Edge.Core.Services.ServiceState.Aborting:
					color = Color.Red;
					break;
				default:
					break;
			}
			return color;
		}

		private void SetGridData(IEnumerable<KeyValuePair<MyScheduler.Objects.SchedulingData, ServiceInstance>> scheduledServices)
		{
			

				scheduleInfoGrid.Rows.Clear();
				foreach (var scheduledService in scheduledServices.OrderBy(s => s.Value.StartTime))
				{
					if (!_scheduledServices.ContainsKey(scheduledService.Key))
						_scheduledServices.Add(scheduledService.Key, scheduledService.Value);
					int row = scheduleInfoGrid.Rows.Add(new object[] { scheduledService.Key.GetHashCode(), scheduledService.Value.ServiceName, scheduledService.Value.ProfileID, scheduledService.Value.StartTime.ToString("dd/MM/yyy HH:mm"), scheduledService.Value.EndTime.ToString("dd/MM/yyy HH:mm"), scheduledService.Value.LegacyInstance.State, scheduledService.Key.Rule.Scope, scheduledService.Value.Deleted, scheduledService.Value.LegacyInstance.Outcome, scheduledService.Value.LegacyInstance.State });
					scheduleInfoGrid.Rows[row].DefaultCellStyle.BackColor = GetColorByState(scheduledService.Value.LegacyInstance.State,scheduledService.Value.LegacyInstance.Outcome);
					scheduleInfoGrid.Rows[row].Tag = scheduledService.Value.LegacyInstance;

				}

			
		}

		private void endServiceBtn_Click(object sender, EventArgs e)
		{

			foreach (DataGridViewRow row in scheduleInfoGrid.SelectedRows)
			{
				var scheduleData = from s in _scheduledServices
								   where s.Key.GetHashCode() == Convert.ToInt32(row.Cells["shceduledID"].Value)
								   select s.Key;

				try
				{
					_scheduler.AbortRuningService(scheduleData.First());
				}
				catch (Exception ex)
				{

					MessageBox.Show(string.Format("You cannot delete service in this state\n{0}", ex.Message));
				}

			}
			GetScheduleServices();

		}

		private void ReSchedule()
		{
			_scheduler.ReSchedule();
		}

		private void getServicesButton_Click(object sender, EventArgs e)
		{
			GetScheduleServices();
		}

		private void rescheduleBtn_Click(object sender, EventArgs e)
		{
			_str.Clear();
			_scheduledServices.Clear();

		}

		private void button1_Click(object sender, EventArgs e)
		{

		}

		private void AddUnplanedServiceConfiguration()
		{

		}

		private void deleteServiceFromScheduleBtn_Click(object sender, EventArgs e)
		{
			foreach (DataGridViewRow row in scheduleInfoGrid.SelectedRows)
			{
				var scheduleData = from s in _scheduledServices
								   where s.Key.GetHashCode() == Convert.ToInt32(row.Cells["shceduledID"].Value)
								   select s.Key;
				if (_scheduledServices[scheduleData.First()].LegacyInstance.State == Easynet.Edge.Core.Services.ServiceState.Uninitialized)
					_scheduler.DeleteScpecificServiceInstance(scheduleData.First());
				else
					MessageBox.Show(string.Format("You can't delete service instance with state {0}", _scheduledServices[scheduleData.First()].LegacyInstance.State));

			}
			GetScheduleServices();

		}

		private void startBtn_Click(object sender, EventArgs e)
		{
			_scheduler.Start();

			logtextBox.Text = logtextBox.Text.Insert(0, "Timer Started:" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "\r\n");
			using (StreamWriter writer = new StreamWriter(Path.Combine(Application.StartupPath, "log.txt"), true, Encoding.Unicode))
			{
				writer.WriteLine("Timer Started" + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
			}
		}

		private void EndBtn_Click(object sender, EventArgs e)
		{
			_scheduler.Stop();
			this.Invoke(setLogMethod, new Object[] { "Timer Stoped" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "r\n" });

			using (StreamWriter writer = new StreamWriter(Path.Combine(Application.StartupPath, "log.txt"), true, Encoding.Unicode))
			{
				writer.WriteLine("Timer Stoped" + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
			}
		}

		private void SetLogTextBox(string lineText)
		{

			lock (logtextBox)
			{
				logtextBox.Text = logtextBox.Text.Insert(0, lineText);
			}
		}


	}
}
