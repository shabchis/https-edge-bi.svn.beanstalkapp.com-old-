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
		StringBuilder _strNotScheduled = new StringBuilder();
		private Scheduler _scheduler;
		private Dictionary<SchedulingData, ServiceInstance> _scheduledServices = new Dictionary<SchedulingData, ServiceInstance>();
		public delegate void SetLogMethod(string lineText);
		public delegate void UpdateGridMethod(legacy.ServiceInstance serviceInstance);
		SetLogMethod setLogMethod;
		UpdateGridMethod updateGridMethod;
		public frmSchedulingControl()
		{
			try
			{
				InitializeComponent();

				setLogMethod = new SetLogMethod(SetLogTextBox);
				updateGridMethod = new UpdateGridMethod(UpdateGridData);
				this.FormClosed += new FormClosedEventHandler(frmSchedulingControl_FormClosed);
			}
			catch (Exception ex)
			{

                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}
		}

		void frmSchedulingControl_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				_scheduler.Stop();
			}
			catch (Exception ex)
			{

				 Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			try
			{
				_scheduler = new Scheduler(true);
				_scheduler.ServiceRunRequiredEvent += new EventHandler(_scheduler_ServiceRunRequiredEvent);
				_scheduler.NewScheduleCreatedEvent += new EventHandler(_scheduler_NewScheduleCreatedEventHandler);
				//	_scheduler.Start();
			}
			catch (Exception ex)
			{
                MessageBox.Show(ex.Message);
				 Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}
		}

		void _scheduler_ServiceRunRequiredEvent(object sender, EventArgs e)
		{
			//todo: log2
			try
			{

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

					Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", string.Format("Service: {0} initalized", serviceInstance.ServiceName), Easynet.Edge.Core.Utilities.LogMessageType.Information);


				}
			}
			catch (Exception ex)
			{

				 Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}

		}

		void LegacyInstance_ChildServiceRequested(object sender, Easynet.Edge.Core.Services.ServiceRequestedEventArgs e)
		{
			try
			{
				legacy.ServiceInstance instance = (legacy.ServiceInstance)sender;
				this.Invoke(setLogMethod, new Object[] { string.Format("\nChild Service: {0} requestedd {1}\r\n", e.RequestedService.Configuration.Name, DateTime.Now.ToString("dd/MM/yy HH:mm")) });
                
				e.RequestedService.ChildServiceRequested += new EventHandler<legacy.ServiceRequestedEventArgs>(LegacyInstance_ChildServiceRequested);
				e.RequestedService.StateChanged += new EventHandler<legacy.ServiceStateChangedEventArgs>(LegacyInstance_StateChanged);
				e.RequestedService.Initialize();
			}
			catch (Exception ex)
			{

				 Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}
		}



		void LegacyInstance_StateChanged(object sender, Easynet.Edge.Core.Services.ServiceStateChangedEventArgs e)
		{
			try
			{
				legacy.ServiceInstance instance = (Easynet.Edge.Core.Services.ServiceInstance)sender;
                instance.OutcomeReported += new EventHandler(instance_OutcomeReported);
				this.Invoke(updateGridMethod, new Object[] { instance });


				this.Invoke(setLogMethod, new Object[] { string.Format("\n{0}: {1} is {2} {3}\r\n", instance.AccountID, instance.Configuration.Name, e.StateAfter, DateTime.Now.ToString("dd/MM/yy HH:mm")) });
				if (e.StateAfter == legacy.ServiceState.Ready)
					instance.Start();
			}
			catch (Exception ex)
			{

				 Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}

		}

        void instance_OutcomeReported(object sender, EventArgs e)
        {
            legacy.ServiceInstance instance = (Easynet.Edge.Core.Services.ServiceInstance)sender;
            this.Invoke(updateGridMethod, new Object[] { instance });
        }

		void _scheduler_NewScheduleCreatedEventHandler(object sender, EventArgs e)
		{
			try
			{
				this.Invoke(setLogMethod, new Object[] { "Schedule Created:" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "\r\n" });

				using (StreamWriter writer = new StreamWriter(Path.Combine(Application.StartupPath, "log.txt"), true, Encoding.Unicode))
				{
					ScheduledInformationEventArgs ee = (ScheduledInformationEventArgs)e;
				
					_scheduledServices.Clear();

                    _strNotScheduled.Clear();

					writer.WriteLine("Schedule Created" + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

					foreach (KeyValuePair<SchedulingData, ServiceInstance> notSchedInfo in ee.NotScheduledInformation)
					{
						_strNotScheduled.AppendLine(string.Format("Service {0} with profile {1} not scheduled", notSchedInfo.Value.ServiceName, notSchedInfo.Key.profileID));

						writer.WriteLine(string.Format("Service {0} with profile {1} not scheduled", notSchedInfo.Value.ServiceName, notSchedInfo.Key.profileID));


					}
					foreach (KeyValuePair<SchedulingData, ServiceInstance> SchedInfo in ee.ScheduleInformation)
					{

						writer.WriteLine(string.Format("ScheduleInfoID:{0}\tService Name:{1}\tAccountID:{2}\tStartTime:{3}\tEndTime\tStatus:{4}\tScope{5}\tDeleted:{6}\tResult:{7}", SchedInfo.Key.GetHashCode(), SchedInfo.Value.ServiceName, SchedInfo.Value.ProfileID, SchedInfo.Value.StartTime.ToString("dd/MM/yyyy,HH:mm"), SchedInfo.Value.EndTime.ToString("dd/MM/yyyy,HH:mm"), SchedInfo.Value.LegacyInstance.State, SchedInfo.Key.Rule.Scope, SchedInfo.Value.Deleted, SchedInfo.Value.LegacyInstance.Outcome));


						_scheduledServices.Add(SchedInfo.Key, SchedInfo.Value);
					}
					GetScheduleServices();

					if (!string.IsNullOrEmpty(_strNotScheduled.ToString()))
					{
                        this.Invoke(setLogMethod, new Object[] { _strNotScheduled.ToString()});
					}
				}
			}
			catch (Exception ex)
			{

				 Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}

		}

		private void ScheduleBtn_Click(object sender, EventArgs e)
		{
			try
			{
				_strNotScheduled.Clear();
				_scheduledServices.Clear();
				_scheduler.NewSchedule();
			}
			catch (Exception ex)
			{

				 Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}


		}

		private void GetScheduleServices()
		{
			try
			{
				var scheduledServices = _scheduler.GetAlllScheduldServices();
				this.Invoke(new Action<IEnumerable<KeyValuePair<MyScheduler.Objects.SchedulingData, ServiceInstance>>>(SetGridData), scheduledServices);
			}
			catch (Exception ex)
			{

				 Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}
		}


		private void UpdateGridData(legacy.ServiceInstance serviceInstance)
		{
			try
			{
				foreach (DataGridViewRow row in scheduleInfoGrid.Rows)
				{
					if (Object.Equals(row.Tag, serviceInstance))
					{
                        row.Cells["dynamicStaus"].Value = serviceInstance.State;
                        row.Cells["outCome"].Value = serviceInstance.Outcome;

                        Color color = GetColorByState(serviceInstance.State, serviceInstance.Outcome);
						row.DefaultCellStyle.BackColor = color;
					}
				}
			}
			catch (Exception ex)
			{

				 Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}

		}

		private static Color GetColorByState(legacy.ServiceState state, legacy.ServiceOutcome outCome)
		{
			Color color = Color.White;
			try
			{

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

			}
			catch (Exception ex)
			{

				 Easynet.Edge.Core.Utilities.Log.Write("GetColorByState", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}
			return color;
		}

		private void SetGridData(IEnumerable<KeyValuePair<MyScheduler.Objects.SchedulingData, ServiceInstance>> scheduledServices)
		{
			try
			{

				scheduleInfoGrid.Rows.Clear();
				foreach (var scheduledService in scheduledServices.OrderBy(s => s.Value.StartTime))
				{
					if (!_scheduledServices.ContainsKey(scheduledService.Key))
						_scheduledServices.Add(scheduledService.Key, scheduledService.Value);
					int row = scheduleInfoGrid.Rows.Add(new object[] { scheduledService.Key.GetHashCode(), scheduledService.Value.ServiceName, scheduledService.Value.ProfileID, scheduledService.Value.StartTime.ToString("dd/MM/yyy HH:mm"), scheduledService.Value.EndTime.ToString("dd/MM/yyy HH:mm"), scheduledService.Value.LegacyInstance.State, scheduledService.Key.Rule.Scope, scheduledService.Value.Deleted, scheduledService.Value.LegacyInstance.Outcome, scheduledService.Value.LegacyInstance.State });
					scheduleInfoGrid.Rows[row].DefaultCellStyle.BackColor = GetColorByState(scheduledService.Value.LegacyInstance.State, scheduledService.Value.LegacyInstance.Outcome);
					scheduleInfoGrid.Rows[row].Tag = scheduledService.Value.LegacyInstance;

				}

			}
			catch (Exception ex)
			{

				 Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}
		}

		private void endServiceBtn_Click(object sender, EventArgs e)
		{
			try
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
			catch (Exception ex)
			{

				 Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}

		}

		private void ReSchedule()
		{
			try
			{
				_scheduler.ReSchedule();
			}
			catch (Exception ex)
			{

				 Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}
		}

		private void getServicesButton_Click(object sender, EventArgs e)
		{
			try
			{
				GetScheduleServices();
			}
			catch (Exception ex)
			{

				 Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}
		}

		private void rescheduleBtn_Click(object sender, EventArgs e)
		{
			try
			{
				_strNotScheduled.Clear();
				_scheduledServices.Clear();
			}
			catch (Exception ex)
			{

				 Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}

		}

		private void button1_Click(object sender, EventArgs e)
		{

		}

		private void AddUnplanedServiceConfiguration()
		{

		}

		private void deleteServiceFromScheduleBtn_Click(object sender, EventArgs e)
		{
			try
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
			catch (Exception ex)
			{

				 Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}

		}

		private void startBtn_Click(object sender, EventArgs e)
		{
			try
			{
				_scheduler.Start();

				logtextBox.Text = logtextBox.Text.Insert(0, "Timer Started:" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "\r\n");
				using (StreamWriter writer = new StreamWriter(Path.Combine(Application.StartupPath, "log.txt"), true, Encoding.Unicode))
				{
					writer.WriteLine("Timer Started" + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
				}
			}
			catch (Exception ex)
			{

				 Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}
		}

		private void EndBtn_Click(object sender, EventArgs e)
		{
			try
			{
				_scheduler.Stop();
				this.Invoke(setLogMethod, new Object[] { "Timer Stoped" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "r\n" });

				using (StreamWriter writer = new StreamWriter(Path.Combine(Application.StartupPath, "log.txt"), true, Encoding.Unicode))
				{
					writer.WriteLine("Timer Stoped" + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
				}
			}
			catch (Exception ex)
			{

				 Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}
		}

		private void SetLogTextBox(string lineText)
		{
			try
			{
				lock (logtextBox)
				{
					logtextBox.Text = logtextBox.Text.Insert(0, lineText);
				}
			}
			catch (Exception ex)
			{

				 Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
			}
		}


	}
}
