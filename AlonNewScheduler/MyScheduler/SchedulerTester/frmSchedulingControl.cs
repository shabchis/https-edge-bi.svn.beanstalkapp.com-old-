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


namespace SchedulerTester
{
	public partial class frmSchedulingControl : Form
	{
		private Scheduler _scheduler;
		private Dictionary<SchedulingData, ServiceInstance> _scheduledServices = new Dictionary<SchedulingData, ServiceInstance>();
		public frmSchedulingControl()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			_scheduler = new Scheduler(true);
			_scheduler.TimeToRunEventHandler += new EventHandler(_scheduler_TimeToRunEventHandler);
			_scheduler.Start();
			
		}

		void _scheduler_TimeToRunEventHandler(object sender, EventArgs e)
		{
			MessageBox.Show("cought");
		}

		private void ScheduleBtn_Click(object sender, EventArgs e)
		{
			_scheduler.NewSchedule();
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
			foreach (var scheduledService in scheduledServices.OrderBy(s=>s.Value.StartTime))
			{
				if (!_scheduledServices.ContainsKey(scheduledService.Key))
				_scheduledServices.Add(scheduledService.Key,scheduledService.Value);
				scheduleInfoGrid.Rows.Add(new object[] { scheduledService.Key.GetHashCode(), scheduledService.Value.ServiceName, scheduledService.Value.ProfileID, scheduledService.Value.StartTime.ToString("dd/MM/yyyy,HH:mm"), scheduledService.Value.EndTime.ToString("dd/MM/yyyy,HH:mm"), scheduledService.Value.State,scheduledService.Key.Rule.Scope });				
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

		private void button1_Click(object sender, EventArgs e)
		{

		}
		private void AddUnplanedServiceConfiguration()
		{

		}
		

	}
}
