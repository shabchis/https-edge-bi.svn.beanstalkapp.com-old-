using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyScheduler
{
	public class Scheduler
	{
		private List<ServiceConfigration> _toBeScheduleServices = new List<ServiceConfigration>();
		private Dictionary<int, ScheduleInfo> _scheduledServices = new Dictionary<int, ScheduleInfo>();
		private Dictionary<int, ServiceConfigration> _servicesPerConfigurationID = new Dictionary<int, ServiceConfigration>();
		private Dictionary<int, ServiceConfigration> _servicesPerProfileID = new Dictionary<int, ServiceConfigration>();
		private Dictionary<int, ScheduleInfo> _unscheduleServices = new Dictionary<int, ScheduleInfo>();
		private const int oddsForAverageExecution = 80;

		public Scheduler(List<ServiceConfigration> services)
		{
			this._toBeScheduleServices = services;
		}
		public void CreateSchedule()
		{


			var toBeScheduledByTimeAndPriority = _toBeScheduleServices.OrderBy(ordered => ordered.Rule.time).ThenBy(ordered => ordered.MaxWaitingTime).ThenByDescending(ordered => ordered.priority);

			foreach (ServiceConfigration service in toBeScheduledByTimeAndPriority)
			{
				double odds;
				TimeSpan incrementByMaxExecutionTime;
				DateTime scheduleTime = SchedulePerService(service.ConfigurationID, service.SchedulingProfile.ProfileID, service.Rule.time, service.AverageExecutionTime, service.MaxConcurrentPerConfiguration, service.MaxCuncurrentPerProfile, out odds, service.MaxExecutionTime, out incrementByMaxExecutionTime);
				ScheduleService(service, scheduleTime, odds, incrementByMaxExecutionTime);
			}
			PrintSchduleTable();
		}

		private void ScheduleService(ServiceConfigration service, DateTime scheduleTime, double odds, TimeSpan incrementByMaxExecutionTime)
		{
			if (scheduleTime.Subtract(service.MaxWaitingTime) > service.Rule.time)
				_unscheduleServices.Add(service.ID, new ScheduleInfo() { ServiceName = service.Name, ConfigurationID = service.ConfigurationID, EndTime = service.Rule.time, ID = service.ID, MaxConcurrentPerConfiguration = service.MaxConcurrentPerConfiguration, MaxCuncurrentPerProfile = service.MaxCuncurrentPerProfile, ProfileID = service.SchedulingProfile.ProfileID, StartTime = scheduleTime, Priority = service.priority, MaxWaitingTime = service.MaxWaitingTime });
			else
			{
				DateTime maxStartTime = scheduleTime.Add(incrementByMaxExecutionTime);
				DateTime maxEndTime = scheduleTime.Add(service.AverageExecutionTime).Add(incrementByMaxExecutionTime);
				_scheduledServices.Add(service.ID, new ScheduleInfo() { ServiceName = service.Name, Odds = odds, ConfigurationID = service.ConfigurationID, EndTime = scheduleTime.Add(service.AverageExecutionTime), ID = service.ID, MaxConcurrentPerConfiguration = service.MaxConcurrentPerConfiguration, MaxCuncurrentPerProfile = service.MaxCuncurrentPerProfile, ProfileID = service.SchedulingProfile.ProfileID, StartTime = scheduleTime, Priority = service.priority,MaxStartTime=maxStartTime,MaxEndTime=maxEndTime });
			}
		}

		private DateTime SchedulePerService(int configurationID, int profileID, DateTime startTime, TimeSpan averageExecutionTime, int MaxConcurrentPerConfiguration, int MaxCuncurrentPerProfile, out double odds, TimeSpan maxExecutionTime,out TimeSpan incrementByMaxExecutionTime)
		{
			odds = 0;
			DateTime endTime = startTime.Subtract(averageExecutionTime);

			//Get all services with same configurationID
			var servicesWithSameConfiguration = from s in _scheduledServices
												where s.Value.ConfigurationID == configurationID 
												orderby s.Value.StartTime ascending
												select s;

			//Get all services with same profileID
			var servicesWithSameProfile = from s in _scheduledServices
										  where s.Value.ProfileID == profileID
										  orderby s.Value.StartTime ascending
										  select s;
			
			DateTime schduledTime = FindFirstFreeTime(servicesWithSameConfiguration, servicesWithSameProfile, startTime, endTime, MaxConcurrentPerConfiguration, MaxCuncurrentPerProfile, averageExecutionTime, out odds, maxExecutionTime, out incrementByMaxExecutionTime);			
			return schduledTime;
		}

		private DateTime FindFirstFreeTime(IOrderedEnumerable<KeyValuePair<int, ScheduleInfo>> servicesWithSameConfiguration, IOrderedEnumerable<KeyValuePair<int, ScheduleInfo>> servicesWithSameProfile, DateTime startTime, DateTime endTime, int MaxConcurrentPerConfiguration, int MaxCuncurrentPerProfile, TimeSpan averageExecutionTime, out double odds, TimeSpan maxExecutionTime, out TimeSpan incrementByMaxExecutionTime)
		{
			DateTime schduledTime = new DateTime();
			bool found = false;			
			odds = 100;
			incrementByMaxExecutionTime = new TimeSpan(0, 0, 0); ;

			while (!found)
			{
				int countedPerConfiguration = servicesWithSameConfiguration.Count(s => (startTime >= s.Value.StartTime && startTime <= s.Value.EndTime) || (endTime >= s.Value.StartTime && endTime <= s.Value.EndTime));
				if (countedPerConfiguration < MaxConcurrentPerConfiguration)
				{
					int countedPerProfile = servicesWithSameProfile.Count(s => (startTime >= s.Value.StartTime && startTime <= s.Value.EndTime) || endTime >= s.Value.StartTime || endTime <= s.Value.EndTime);
					if (countedPerProfile < MaxCuncurrentPerProfile)
					{
						schduledTime = startTime;
						found = true;
					}
					else
					{
						odds = (odds * oddsForAverageExecution) / 100;
						//get the next first place of ending service(next start time
						GetNewStartEndTime(servicesWithSameProfile, ref startTime, ref endTime, averageExecutionTime,ref  incrementByMaxExecutionTime,maxExecutionTime);
						
						////remove unfree time from servicePerConfiguration and servicePerProfile
						RemoveBusyTime(ref servicesWithSameConfiguration, ref servicesWithSameProfile,  startTime);						
					}
				}
				else
				{		
					odds = (odds * oddsForAverageExecution) / 100;
					//get the next first place of ending service(next start time
					GetNewStartEndTime(servicesWithSameConfiguration, ref startTime, ref endTime,averageExecutionTime,ref incrementByMaxExecutionTime,maxExecutionTime);
					////remove unfree time from servicePerConfiguration and servicePerProfile
					RemoveBusyTime(ref servicesWithSameConfiguration, ref servicesWithSameProfile, startTime);
				} 
			}
			return schduledTime;

		}

		private static void GetNewStartEndTime(IOrderedEnumerable<KeyValuePair<int, ScheduleInfo>> servicesWithSameProfile, ref DateTime startTime, ref DateTime endTime, TimeSpan averageExecutionTime, ref TimeSpan incrementByMaxExecutionTime,TimeSpan maxExecutionTime)
		{
			TimeSpan difference = maxExecutionTime.Subtract(averageExecutionTime);
			incrementByMaxExecutionTime = incrementByMaxExecutionTime.Add(difference);
			startTime = servicesWithSameProfile.Min(s => s.Value.EndTime);
			//Get end time
			endTime = startTime + averageExecutionTime;
		}

		private  void RemoveBusyTime(ref IOrderedEnumerable<KeyValuePair<int, ScheduleInfo>> servicesWithSameConfiguration, ref IOrderedEnumerable<KeyValuePair<int, ScheduleInfo>> servicesWithSameProfile,  DateTime startTime)
		{
			servicesWithSameConfiguration = from s in servicesWithSameConfiguration
											where s.Value.EndTime > startTime
											orderby s.Value.StartTime
											select s;

			servicesWithSameProfile = from s in servicesWithSameProfile
									  where s.Value.EndTime > startTime
									  orderby s.Value.StartTime
									  select s;
		}


		private void PrintSchduleTable()
		{
			 foreach (KeyValuePair<int,ScheduleInfo> scheduled in _scheduledServices.OrderBy(s=>s.Value.StartTime))
			{
				Console.WriteLine("Service with ID {0} and name {1},will be executed between:\n{2} and  {3},\n in odds of {4}  and \n100% that the service will be executed until {5}\n and finish until {6}.\n service priority is {7}", scheduled.Key, scheduled.Value.ServiceName, scheduled.Value.StartTime, scheduled.Value.EndTime, scheduled.Value.Odds,scheduled.Value.MaxStartTime,scheduled.Value.MaxEndTime, scheduled.Value.Priority);
				Console.WriteLine("--------------------------------------------------------------------------------");
				
			}
			if (_unscheduleServices.Count>0)
				Console.WriteLine("---------------------Will not be scheduled--------------------------------------");
			 foreach (KeyValuePair<int,ScheduleInfo> notScheduled in _unscheduleServices)
			 {
				 Console.WriteLine("Service with ID {0} and name {1},will not be execute since its start time sholud have been{2},and its schedule time is{3}and is maximum waiting time is{4}", notScheduled.Key, notScheduled.Value.ServiceName, notScheduled.Value.EndTime, notScheduled.Value.StartTime,notScheduled.Value.MaxWaitingTime);

				 
			 }
			Console.ReadLine();
		}
	}
	public class ScheduleInfo
	{
		public int ID;
		public string ServiceName;
		public int ConfigurationID;
		public int ProfileID;
		public DateTime StartTime;
		public DateTime EndTime;
		public DateTime MaxStartTime;
		public DateTime MaxEndTime;		
		public int MaxConcurrentPerConfiguration;
		public int MaxCuncurrentPerProfile;
		public int Priority;
		public TimeSpan MaxWaitingTime;
		public double Odds;


	}
}
