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


namespace SchedulerTester
{
	public partial class Form1 : Form
	{
		private Scheduler _scheduler;
		private Dictionary<SchedulingData, ServiceInstance> _scheduledServices = new Dictionary<SchedulingData, ServiceInstance>();
		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			_scheduler = new Scheduler(true);
			
		}

		private void ScheduleBtn_Click(object sender, EventArgs e)
		{
			_scheduler.CreateSchedule();
			GetScheduleServices();
			
		}

		private void GetScheduleServices()
		{
			var scheduledServices= _scheduler.GetAlllScheduldServices();
			SetGridData(scheduledServices);
			
		}

		private void SetGridData(IEnumerable<KeyValuePair<MyScheduler.Objects.SchedulingData, ServiceInstance>> scheduledServices)
		{
			scheduleInfoGrid.Rows.Clear();
			foreach (var scheduledService in scheduledServices)
			{
				if (!_scheduledServices.ContainsKey(scheduledService.Key))
				_scheduledServices.Add(scheduledService.Key,scheduledService.Value);
				scheduleInfoGrid.Rows.Add(new object[] { scheduledService.Key.GetHashCode(), scheduledService.Value.ServiceName, scheduledService.Value.StartTime.ToString("HH:mm"), scheduledService.Value.EndTime.ToString("HH:mm"), scheduledService.Value.State });				
			}
		}

		private void endServiceBtn_Click(object sender, EventArgs e)
		{
			foreach (DataGridViewRow row in scheduleInfoGrid.SelectedRows)
			{
				var scheduleData = from s in _scheduledServices
								   where s.Key.GetHashCode() == Convert.ToInt32(row.Cells["shceduledID"].Value)
								   select  s.Key;
				_scheduler.SetServiceState(scheduleData.First(), serviceStatus.Ended);

			}
			ReSchedule();
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
			ReSchedule();
			GetScheduleServices();
		}
		

	}
}