using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyScheduler
{
	public class Scheduler
	{
		private List<ServiceConfigration> _toBeScheduleServices = new List<ServiceConfigration>();
		private Dictionary<int, ServiceInstance> _scheduledServices = new Dictionary<int, ServiceInstance>();
		private Dictionary<int, ServiceConfigration> _servicesPerConfigurationID = new Dictionary<int, ServiceConfigration>();
		private Dictionary<int, ServiceConfigration> _servicesPerProfileID = new Dictionary<int, ServiceConfigration>();
		private Dictionary<int, ServiceInstance> _unscheduleServices = new Dictionary<int, ServiceInstance>();
		private  double oddsForAverageExecution = 0.8;
		private  double oddsForMaxExecution = 1;
		private const double wantedOdds = 0.6;

		public Scheduler(List<ServiceConfigration> services)
		{
			
			this._toBeScheduleServices = services;
		}
		public void CreateSchedule()
		{


			var toBeScheduledByTimeAndPriority = _toBeScheduleServices.OrderBy(ordered => ordered.Rule.time).ThenBy(ordered => ordered.Rule.MaxDeviation).ThenByDescending(ordered => ordered.priority);

			foreach (ServiceConfigration service in toBeScheduledByTimeAndPriority)
			{				
				ServiceInstance serviceInstance = SchedulePerService(service);
				ScheduleService(serviceInstance);
			}
			PrintSchduleTable();
		}

		private void ScheduleService(ServiceInstance serviceInstance)
		{
			if (serviceInstance.ActualDeviation>serviceInstance.MaxDeviation)  // check if the waiting time is bigger then max waiting time.
				_unscheduleServices.Add(serviceInstance.ID,serviceInstance);
			else
			{
				_scheduledServices.Add(serviceInstance.ID, serviceInstance);
			}
		}

		private ServiceInstance SchedulePerService(ServiceConfigration service)
		{
			
			

			//Get all services with same configurationID
			var servicesWithSameConfiguration = from s in _scheduledServices
												where s.Value.ConfigurationID ==service.ConfigurationID
												orderby s.Value.StartTime ascending
												select s;

			//Get all services with same profileID
			var servicesWithSameProfile = from s in _scheduledServices
										  where s.Value.ProfileID == service.SchedulingProfile.ProfileID
										  orderby s.Value.StartTime ascending
										  select s;

			ServiceInstance serviceInstance = FindFirstFreeTime(servicesWithSameConfiguration, servicesWithSameProfile, service);

			return serviceInstance;
		}

		private ServiceInstance FindFirstFreeTime(IOrderedEnumerable<KeyValuePair<int, ServiceInstance>> servicesWithSameConfiguration, IOrderedEnumerable<KeyValuePair<int, ServiceInstance>> servicesWithSameProfile, ServiceConfigration service)
		{
			ServiceInstance scheduleInfo=null;

			double proportion = 0;
			int executionTimeInMin=0;
			executionTimeInMin = CalculateExecutionTimeInMin(oddsForMaxExecution, oddsForAverageExecution, service.MaxExecutionTime.TotalMinutes, service.AverageExecutionTime.TotalMinutes, wantedOdds,ref proportion);  
			DateTime baseStartTime = service.Rule.time;
			DateTime baseEndTime = baseStartTime.AddMinutes(executionTimeInMin);
			DateTime calculatedStartTime = baseStartTime;
			DateTime calculatedEndTime = baseEndTime;			
			bool found = false;			

			while (!found)
			{
				int countedPerConfiguration = servicesWithSameConfiguration.Count(s => (calculatedStartTime >= s.Value.StartTime && calculatedStartTime <= s.Value.EndTime) || (calculatedEndTime >= s.Value.StartTime && calculatedEndTime <= s.Value.EndTime));
				if (countedPerConfiguration < service.MaxConcurrentPerConfiguration)
				{
					int countedPerProfile = servicesWithSameProfile.Count(s => (calculatedStartTime >= s.Value.StartTime && calculatedStartTime <= s.Value.EndTime) || calculatedEndTime >= s.Value.StartTime || calculatedEndTime <= s.Value.EndTime);
					if (countedPerProfile <service.MaxCuncurrentPerProfile)
					{
						scheduleInfo = new ServiceInstance();
						scheduleInfo.StartTime = calculatedStartTime;
						scheduleInfo.EndTime = calculatedEndTime;
						scheduleInfo.Odds = wantedOdds;
						scheduleInfo.ActualDeviation = calculatedStartTime.Subtract(baseStartTime);
						scheduleInfo.Priority = service.priority;
						scheduleInfo.ConfigurationID = service.ConfigurationID;
						scheduleInfo.ID = service.ID;
						scheduleInfo.MaxConcurrentPerConfiguration = service.MaxConcurrentPerConfiguration;
						scheduleInfo.MaxCuncurrentPerProfile = service.MaxCuncurrentPerProfile;
						scheduleInfo.MaxDeviation = service.Rule.MaxDeviation;
						scheduleInfo.ProfileID = service.SchedulingProfile.ProfileID;
						scheduleInfo.ServiceName = service.Name;
						
						
						found = true;
					}
					else
					{

						executionTimeInMin = CalculateExecutionTimeInMin(oddsForMaxExecution, oddsForAverageExecution, service.MaxExecutionTime.TotalMinutes, service.AverageExecutionTime.TotalMinutes,wantedOdds, ref proportion);  
						//get the next first place of ending service(next start time
						GetNewStartEndTime(servicesWithSameProfile, ref calculatedStartTime, ref calculatedEndTime, executionTimeInMin);

						////remove unfree time from servicePerConfiguration and servicePerProfile
						RemoveBusyTime(ref servicesWithSameConfiguration, ref servicesWithSameProfile, calculatedStartTime);
					}
				}
				else
				{


					executionTimeInMin = CalculateExecutionTimeInMin(oddsForMaxExecution, oddsForAverageExecution, service.MaxExecutionTime.TotalMinutes, service.AverageExecutionTime.TotalMinutes,wantedOdds, ref proportion);  

					GetNewStartEndTime(servicesWithSameConfiguration, ref calculatedStartTime, ref calculatedEndTime, executionTimeInMin);
					////remove unfree time from servicePerConfiguration and servicePerProfile
					RemoveBusyTime(ref servicesWithSameConfiguration, ref servicesWithSameProfile, calculatedStartTime);
				}
			}
			return scheduleInfo;

		}

		private int CalculateExecutionTimeInMin(double oddsForMaxExecution, double oddsForAverageExecution, double maxExecutionTimeInMin, double averageExecutionTimeInMin,double wantedOdds, ref  double proportion)
		{
			
			//Alon
			int result;
			if (proportion != 0)
				proportion = proportion / oddsForAverageExecution;
			else
				proportion = wantedOdds / oddsForAverageExecution;
			double executionTime;
			executionTime = proportion * averageExecutionTimeInMin; 
			

			return result = Convert.ToInt32(executionTime);
			
			
			
		}

		private static void GetNewStartEndTime(IOrderedEnumerable<KeyValuePair<int, ServiceInstance>> servicesWithSameProfile, ref DateTime startTime, ref DateTime endTime, int ExecutionTime)
		{
			
			startTime = servicesWithSameProfile.Min(s => s.Value.EndTime);
			//Get end time
			endTime = startTime.AddMinutes(ExecutionTime);
		}

		private void RemoveBusyTime(ref IOrderedEnumerable<KeyValuePair<int, ServiceInstance>> servicesWithSameConfiguration, ref IOrderedEnumerable<KeyValuePair<int, ServiceInstance>> servicesWithSameProfile, DateTime startTime)
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
			Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "id", "start", "end", "diff", "odds", "priority");
			Console.WriteLine("---------------------------------------------------------");
			KeyValuePair<int, ServiceInstance>? prev = null;
			foreach (KeyValuePair<int, ServiceInstance> scheduled in _scheduledServices.OrderBy(s => s.Value.StartTime))
			{
				Console.WriteLine("{0}\t{1:hh:mm}\t{2:hh:mm}\t{3:hh\\:mm}\t{4}\t{5}",
					scheduled.Key,
					scheduled.Value.StartTime,
					scheduled.Value.EndTime,
					/*prev != null ? scheduled.Value.StartTime - prev.Value.Value.EndTime : TimeSpan.FromMinutes(0)*/
					scheduled.Value.StartTime.Subtract(scheduled.Value.EndTime),
					Math.Round(scheduled.Value.Odds, 2),
					scheduled.Value.Priority);
				prev = scheduled;

			}
			if (_unscheduleServices.Count > 0)
				Console.WriteLine("---------------------Will not be scheduled--------------------------------------");
			foreach (KeyValuePair<int, ServiceInstance> notScheduled in _unscheduleServices)
			{
				Console.WriteLine("Service with ID {0} and name {1},will not be execute since its start time sholud have been{2},and its schedule time is{3}and is maximum waiting time is{4}", notScheduled.Key, notScheduled.Value.ServiceName, notScheduled.Value.EndTime, notScheduled.Value.StartTime, notScheduled.Value.MaxDeviation);


			}
			Console.ReadLine();
		}
	}
	public class ServiceInstance
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
		public TimeSpan MaxDeviation;
		public TimeSpan ActualDeviation;
		public double Odds;


	}
}
