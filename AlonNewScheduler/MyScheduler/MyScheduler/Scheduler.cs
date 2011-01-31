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

		public Scheduler(List<ServiceConfigration> services)
		{
			this._toBeScheduleServices = services;
		}
		public void CreateSchedule()
		{


			var toBeScheduledByTimeAndPriority=_toBeScheduleServices.OrderBy(ordered => ordered.Rule.time).ThenByDescending(ordered => ordered.priority);

			foreach (ServiceConfigration service in toBeScheduledByTimeAndPriority)
			{
				DateTime scheduleTime = SchedulePerService(service.ConfigurationID, service.SchedulingProfile.ProfileID, service.Rule.time, service.AverageExecutionTime, service.MaxConcurrentPerConfiguration, service.MaxCuncurrentPerProfile);
				_scheduledServices.Add(service.ID,new ScheduleInfo() {ServiceName=service.Name, ConfigurationID=service.ConfigurationID, EndTime=scheduleTime.Add(service.AverageExecutionTime), ID=service.ID, MaxConcurrentPerConfiguration=service.MaxConcurrentPerConfiguration, MaxCuncurrentPerProfile=service.MaxCuncurrentPerProfile, ProfileID=service.SchedulingProfile.ProfileID, StartTime=scheduleTime, Priority=service.priority});
			}
			PrintSchduleTable();
		}

		private DateTime SchedulePerService(int configurationID, int profileID, DateTime startTime, TimeSpan averageExecutionTime, int MaxConcurrentPerConfiguration, int MaxCuncurrentPerProfile)
		{
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

			DateTime schduledTime = FindFirstFreeTime(servicesWithSameConfiguration, servicesWithSameProfile, startTime, endTime, MaxConcurrentPerConfiguration, MaxCuncurrentPerProfile, averageExecutionTime);			
			return schduledTime;
		}

		private DateTime FindFirstFreeTime(IOrderedEnumerable<KeyValuePair<int, ScheduleInfo>> servicesWithSameConfiguration, IOrderedEnumerable<KeyValuePair<int, ScheduleInfo>> servicesWithSameProfile, DateTime startTime, DateTime endTime, int MaxConcurrentPerConfiguration, int MaxCuncurrentPerProfile, TimeSpan averageExecutionTime)
		{
			DateTime schduledTime = new DateTime();
			bool found = false;

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
						//get the next first place of ending service(next start time
						GetNewStartEndTime(servicesWithSameProfile, ref startTime, ref endTime, ref averageExecutionTime);
						////remove unfree time from servicePerConfiguration and servicePerProfile
						RemoveBusyTime(ref servicesWithSameConfiguration, ref servicesWithSameProfile,  startTime);						
					}
				}
				else
				{					
					//get the next first place of ending service(next start time
					GetNewStartEndTime(servicesWithSameConfiguration, ref startTime, ref endTime, ref averageExecutionTime);
					////remove unfree time from servicePerConfiguration and servicePerProfile
					RemoveBusyTime(ref servicesWithSameConfiguration, ref servicesWithSameProfile, startTime);
				} 
			}
			return schduledTime;

		}

		private static void GetNewStartEndTime(IOrderedEnumerable<KeyValuePair<int, ScheduleInfo>> servicesWithSameProfile, ref DateTime startTime, ref DateTime endTime, ref TimeSpan averageExecutionTime)
		{
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
				Console.WriteLine("Service with ID {0} and name {1} will start execution on: {2} and will finish executing on: {3} it's priority is {4}", scheduled.Key, scheduled.Value.ServiceName, scheduled.Value.StartTime, scheduled.Value.EndTime,scheduled.Value.Priority);
			}
			Console.ReadLine();
		}

		private ServiceConfigration FindFitService(string p, DateTime fromTime, DateTime toTime)
		{
			throw new NotImplementedException();
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
		public int MaxConcurrentPerConfiguration;
		public int MaxCuncurrentPerProfile;
		public int Priority;
		public TimeSpan MaxWaitingTime;

	}
}
