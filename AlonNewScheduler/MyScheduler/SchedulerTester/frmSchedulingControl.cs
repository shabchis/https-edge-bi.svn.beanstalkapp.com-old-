﻿using System;
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


namespace SchedulerTester
{
	public partial class frmSchedulingControl : Form
	{
		StringBuilder _str = new StringBuilder();
		private Scheduler _scheduler;
		private Dictionary<SchedulingData, ServiceInstance> _scheduledServices = new Dictionary<SchedulingData, ServiceInstance>();
		public frmSchedulingControl()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			_scheduler = new Scheduler(true);
			_scheduler.ServiceRunRequiredEvent += new EventHandler(_scheduler_TimeToRunEventHandler);
			_scheduler.NewScheduleCreatedEvent += new EventHandler(_scheduler_NewScheduleCreatedEventHandler);
			//	_scheduler.Start();

		}

		void _scheduler_NewScheduleCreatedEventHandler(object sender, EventArgs e)
		{
			using (StreamWriter writer = new StreamWriter(Path.Combine(Application.StartupPath, "log.txt"), true, Encoding.Unicode))
			{
				ScheduledInformationEventArgs ee = (ScheduledInformationEventArgs)e;
				_str.Clear();
				_scheduledServices.Clear();
				logtextBox.Text.Insert(0, "Schedule Created" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "\n");


				writer.WriteLine("Schedule Created" + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

				foreach (KeyValuePair<SchedulingData, ServiceInstance> notSchedInfo in ee.NotScheduledInformation)
				{
					_str.AppendLine(string.Format("Service {0} with profile {1} not scheduled", notSchedInfo.Value.ServiceName, notSchedInfo.Key.profileID));

					writer.WriteLine(string.Format("Service {0} with profile {1} not scheduled", notSchedInfo.Value.ServiceName, notSchedInfo.Key.profileID));


				}
				foreach (KeyValuePair<SchedulingData, ServiceInstance> SchedInfo in ee.ScheduleInformation)
				{

					writer.WriteLine(string.Format("ScheduleInfoID:{0}\tService Name:{1}\tAccountID:{2}\tStartTime:{3}\tEndTime\tStatus:{4}\tScope{5}\tDeleted:{6}",SchedInfo.Key.GetHashCode(), SchedInfo.Value.ServiceName, SchedInfo.Value.ProfileID, SchedInfo.Value.StartTime.ToString("dd/MM/yyyy,HH:mm"), SchedInfo.Value.EndTime.ToString("dd/MM/yyyy,HH:mm"), SchedInfo.Value.LegacyInstance.State, SchedInfo.Key.Rule.Scope, SchedInfo.Value.Deleted));


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

		void _scheduler_TimeToRunEventHandler(object sender, EventArgs e)
		{
			TimeToRunEventArgs ee = (TimeToRunEventArgs)e;

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
			SetGridData(scheduledServices);

		}

		private void SetGridData(IEnumerable<KeyValuePair<MyScheduler.Objects.SchedulingData, ServiceInstance>> scheduledServices)
		{
			scheduleInfoGrid.Rows.Clear();
			foreach (var scheduledService in scheduledServices.OrderBy(s => s.Value.StartTime))
			{
				if (!_scheduledServices.ContainsKey(scheduledService.Key))
					_scheduledServices.Add(scheduledService.Key, scheduledService.Value);
				scheduleInfoGrid.Rows.Add(new object[] { scheduledService.Key.GetHashCode(), scheduledService.Value.ServiceName, scheduledService.Value.ProfileID, scheduledService.Value.StartTime.ToString("dd/MM/yyyy,HH:mm"), scheduledService.Value.EndTime.ToString("dd/MM/yyyy,HH:mm"), scheduledService.Value.LegacyInstance.State, scheduledService.Key.Rule.Scope, scheduledService.Value.Deleted });
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
			logtextBox.Text.Insert(0, "Timer Started" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "\n");
			using (StreamWriter writer = new StreamWriter(Path.Combine(Application.StartupPath, "log.txt"), true, Encoding.Unicode))
			{
				writer.WriteLine("Timer Started" + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
			}
		}

		private void EndBtn_Click(object sender, EventArgs e)
		{
			_scheduler.Stop();
			logtextBox.Text.Insert(0, "Timer Stoped" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "\n");
			using (StreamWriter writer = new StreamWriter(Path.Combine(Application.StartupPath, "log.txt"), true, Encoding.Unicode))
			{
				writer.WriteLine("Timer Stoped" + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
			}
		}


	}
}
