using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core;
using Easynet.Edge.Core.Configuration;
using MyScheduler.Objects;

namespace MyScheduler
{
	public class Scheduler
	{

		private List<ServiceConfigration> _toBeScheduleServices = new List<ServiceConfigration>();
		private Dictionary<int, ServiceInstance> _scheduledServices = new Dictionary<int, ServiceInstance>();
		private Dictionary<int, ServiceConfigration> _servicesPerConfigurationID = new Dictionary<int, ServiceConfigration>();
		private Dictionary<int, ServiceConfigration> _servicesPerProfileID = new Dictionary<int, ServiceConfigration>();
		private Dictionary<int, ServiceInstance> _unscheduleServices = new Dictionary<int, ServiceInstance>();
		private const int neededTimeLine = 120;
		private double oddsForAverageExecution = 0.8;
		private double oddsForMaxExecution = 1;
		private const double wantedOdds = 0.6;


		public Scheduler(bool getServicesFromConfigFile)
		{
			if (getServicesFromConfigFile)
				GetServicesFromConfigurationFile();


		}
		public void CreateSchedule()
		{
			List<ServiceConfigration> servicesForNextTimeLine = GetServicesForNextTimeLine();

			var toBeScheduledByTimeAndPriority = servicesForNextTimeLine.OrderBy(ordered => ordered.SchedulingRules[0].Hours[0]).ThenByDescending(ordered => ordered.priority);

			foreach (ServiceConfigration service in toBeScheduledByTimeAndPriority)
			{
				ServiceInstance serviceInstance = SchedulePerService(service);
				ScheduleService(serviceInstance);
			}
			PrintSchduleTable();
		}

		private List<ServiceConfigration> GetServicesForNextTimeLine()
		{
			List<ServiceConfigration> nextTimeLineServices = new List<ServiceConfigration>();
			int timeLineInMin = neededTimeLine;
			DateTime from = DateTime.Now;
			DateTime to = DateTime.Now.AddMinutes(neededTimeLine);
			foreach (ServiceConfigration service in _toBeScheduleServices)
			{
				bool added = false;
				foreach (SchedulingRule schedulingRule in service.SchedulingRules)
				{

					if (schedulingRule != null)
					{
						foreach (TimeSpan hour in schedulingRule.Hours)
						{
							if (hour >= from.TimeOfDay && hour <= to.TimeOfDay)
							{

								switch (schedulingRule.Scope)
								{
									case SchedulingScope.Day:
										{
											nextTimeLineServices.Add(service);
											added = true;
											break;
										}
									case SchedulingScope.Week:
										{
											foreach (int day in schedulingRule.Days)
											{
												if (day == (int)from.DayOfWeek + 1 || day == (int)to.DayOfWeek + 1)
												{
													nextTimeLineServices.Add(service);
													added = true;
													break;
												}


											}
											break;
										}

									case SchedulingScope.Month://TODO: CHECK THIS IT IS MORE COMPLICATED I THING I HAVE A BUG HERE
										{
											foreach (int day in schedulingRule.Days)
											{
												if (day == from.Day || day == to.Day)
												{
													nextTimeLineServices.Add(service);
													added = true;
													break;													
												}
												
											}
											break;
										}
									
								}
							}
							if (added)
								break;
						}
						if (added)
							break;
					}
					if (added)
						break;

				}

			}
			return nextTimeLineServices;
		}
		public void GetServicesFromConfigurationFile()
		{

			//get propertis/configuration.....from app.config 
			Dictionary<string, ServiceConfigration> baseConfigurations = new Dictionary<string, ServiceConfigration>();
			Dictionary<string, ServiceConfigration> configurations = new Dictionary<string, ServiceConfigration>();



			foreach (ServiceElement serviceElement in ServicesConfiguration.Services)
			{

				ServiceConfigration serviceConfiguration = new ServiceConfigration();
				serviceConfiguration.Name = serviceElement.Name;
				serviceConfiguration.MaxConcurrent = serviceElement.MaxInstances;
				serviceConfiguration.MaxCuncurrentPerProfile = serviceElement.MaxInstancesPerAccount;
				baseConfigurations.Add(serviceConfiguration.Name, serviceConfiguration);


			}


			foreach (AccountElement account in ServicesConfiguration.Accounts)
			{

				foreach (AccountServiceElement accountService in account.Services)
				{
					ServiceElement serviceUse = accountService.Uses.Element;
					ActiveServiceElement activeServiceElement = new ActiveServiceElement(accountService);

					ServiceConfigration serviceConfiguration = new ServiceConfigration();

					serviceConfiguration.Name = string.Format("{0}-{1}", account.ID, serviceUse.Name);
					serviceConfiguration.MaxConcurrent = activeServiceElement.MaxInstances;
					serviceConfiguration.MaxCuncurrentPerProfile = activeServiceElement.MaxInstancesPerAccount;
					foreach (SchedulingRuleElement schedulingRuleElement in activeServiceElement.SchedulingRules)
					{
						SchedulingRule rule = new SchedulingRule();
						switch (schedulingRuleElement.CalendarUnit)
						{
							case CalendarUnit.AlwaysOn:
								{
									//throw new Exception("UnKnown scheduling Rule");
									break;
								}
							case CalendarUnit.Day:
								rule.Scope = SchedulingScope.Day;
								break;
							case CalendarUnit.Month:
								rule.Scope = SchedulingScope.Month;
								break;
							case CalendarUnit.ReRun:
								{
									//throw new Exception("UnKnown scheduling Rule");
									break;
								}
							case CalendarUnit.Week:
								rule.Scope = SchedulingScope.Week;
								break;
							default:
								break;
						}

						rule.Days = schedulingRuleElement.SubUnits.ToList();
						rule.Hours = schedulingRuleElement.ExactTimes.ToList();
						if (serviceConfiguration.SchedulingRules == null)
							serviceConfiguration.SchedulingRules = new List<SchedulingRule>();
						serviceConfiguration.SchedulingRules.Add(rule);

					}
					serviceConfiguration.BaseConfiguration = baseConfigurations[serviceUse.Name];
					Profile profile = new Profile();
					profile.Name = account.ID.ToString();
					profile.ID = account.ID;
					profile.Settings = new Dictionary<string, object>();
					profile.Settings.Add("AccountID", account.ID);
					serviceConfiguration.SchedulingProfile = profile;
					_toBeScheduleServices.Add(serviceConfiguration);



				}
			}









		}

		private void ScheduleService(ServiceInstance serviceInstance)
		{
			if (serviceInstance.ActualDeviation > serviceInstance.MaxDeviationAfter)  // check if the waiting time is bigger then max waiting time.
				_unscheduleServices.Add(serviceInstance.ID, serviceInstance);
			else
			{
				_scheduledServices.Add(serviceInstance.ID, serviceInstance);
			}
		}

		private ServiceInstance SchedulePerService(ServiceConfigration service)
		{



			//Get all services with same configurationID
			var servicesWithSameConfiguration = from s in _scheduledServices
												where s.Value.ConfigurationID == service.ConfigurationID
												orderby s.Value.StartTime ascending
												select s;

			//Get all services with same profileID
			var servicesWithSameProfile = from s in _scheduledServices
										  where s.Value.ProfileID == service.SchedulingProfile.ID
										  orderby s.Value.StartTime ascending
										  select s;

			ServiceInstance serviceInstance = FindFirstFreeTime(servicesWithSameConfiguration, servicesWithSameProfile, service);

			return serviceInstance;
		}

		private ServiceInstance FindFirstFreeTime(IOrderedEnumerable<KeyValuePair<int, ServiceInstance>> servicesWithSameConfiguration, IOrderedEnumerable<KeyValuePair<int, ServiceInstance>> servicesWithSameProfile, ServiceConfigration service)
		{
			ServiceInstance scheduleInfo = null;
			
			double proportion = 0;
			int executionTimeInMin = 0;
			executionTimeInMin = CalculateExecutionTimeInMin(oddsForMaxExecution, oddsForAverageExecution, service.MaxExecutionTime.TotalMinutes, service.AverageExecutionTime.TotalMinutes, wantedOdds, ref proportion);
			//TODO: CHECK WITH DORON LEAKING YEARS MONTH DAY FOR THE NEXT ROW
			DateTime baseStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, service.SchedulingRules[0].Hours[0].Hours, service.SchedulingRules[0].Hours[0].Minutes,0);
			DateTime baseEndTime = baseStartTime.AddMinutes(executionTimeInMin);
			DateTime calculatedStartTime = baseStartTime;
			DateTime calculatedEndTime = baseEndTime;
			bool found = false;

			while (!found)
			{
				int countedPerConfiguration = servicesWithSameConfiguration.Count(s => (calculatedStartTime >= s.Value.StartTime && calculatedStartTime <= s.Value.EndTime) || (calculatedEndTime >= s.Value.StartTime && calculatedEndTime <= s.Value.EndTime));
				if (countedPerConfiguration < service.MaxConcurrent)
				{
					int countedPerProfile = servicesWithSameProfile.Count(s => (calculatedStartTime >= s.Value.StartTime && calculatedStartTime <= s.Value.EndTime) || calculatedEndTime >= s.Value.StartTime || calculatedEndTime <= s.Value.EndTime);
					if (countedPerProfile < service.MaxCuncurrentPerProfile)
					{
						scheduleInfo = new ServiceInstance();
						scheduleInfo.StartTime = calculatedStartTime;
						scheduleInfo.EndTime = calculatedEndTime;
						scheduleInfo.Odds = wantedOdds;
						scheduleInfo.ActualDeviation = calculatedStartTime.Subtract(baseStartTime);
						scheduleInfo.Priority = service.priority;
						scheduleInfo.ConfigurationID = service.ConfigurationID;
						scheduleInfo.ID = service.ID;
						scheduleInfo.MaxConcurrentPerConfiguration = service.MaxConcurrent;
						scheduleInfo.MaxCuncurrentPerProfile = service.MaxCuncurrentPerProfile;
						scheduleInfo.MaxDeviationAfter = service.SchedulingRules[0].MaxDeviationAfter;
						scheduleInfo.MaxDeviationBefore = service.SchedulingRules[0].MaxDeviationBefore;
						scheduleInfo.ProfileID = service.SchedulingProfile.ID;
						scheduleInfo.ServiceName = service.Name;


						found = true;
					}
					else
					{

						executionTimeInMin = CalculateExecutionTimeInMin(oddsForMaxExecution, oddsForAverageExecution, service.MaxExecutionTime.TotalMinutes, service.AverageExecutionTime.TotalMinutes, wantedOdds, ref proportion);
						//get the next first place of ending service(next start time
						GetNewStartEndTime(servicesWithSameProfile, ref calculatedStartTime, ref calculatedEndTime, executionTimeInMin);

						////remove unfree time from servicePerConfiguration and servicePerProfile
						RemoveBusyTime(ref servicesWithSameConfiguration, ref servicesWithSameProfile, calculatedStartTime);
					}
				}
				else
				{


					executionTimeInMin = CalculateExecutionTimeInMin(oddsForMaxExecution, oddsForAverageExecution, service.MaxExecutionTime.TotalMinutes, service.AverageExecutionTime.TotalMinutes, wantedOdds, ref proportion);

					GetNewStartEndTime(servicesWithSameConfiguration, ref calculatedStartTime, ref calculatedEndTime, executionTimeInMin);
					////remove unfree time from servicePerConfiguration and servicePerProfile
					RemoveBusyTime(ref servicesWithSameConfiguration, ref servicesWithSameProfile, calculatedStartTime);
				}
			}
			return scheduleInfo;

		}

		private int CalculateExecutionTimeInMin(double oddsForMaxExecution, double oddsForAverageExecution, double maxExecutionTimeInMin, double averageExecutionTimeInMin, double wantedOdds, ref  double proportion)
		{

			//Alon
			int result;
			//if (proportion != 0)
			//    proportion = proportion / oddsForAverageExecution;
			//else
			//    proportion = wantedOdds / oddsForAverageExecution;
			//double executionTime;
			//executionTime = proportion * averageExecutionTimeInMin; 



			return result = Convert.ToInt32(averageExecutionTimeInMin);



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
			Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "Name", "start", "end", "diff", "odds", "priority");
			Console.WriteLine("---------------------------------------------------------");
			KeyValuePair<int, ServiceInstance>? prev = null;
			foreach (KeyValuePair<int, ServiceInstance> scheduled in _scheduledServices.OrderBy(s => s.Value.StartTime))
			{
				Console.WriteLine("{0}\t{1:hh:mm}\t{2:hh:mm}\t{3:hh\\:mm}\t{4}\t{5}",
					scheduled.Value.ServiceName,
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
				Console.WriteLine("Service with ID {0} and name {1},will not be execute since its start time sholud have been{2},and its schedule time is{3}and is maximum waiting time is{4}", notScheduled.Key, notScheduled.Value.ServiceName, notScheduled.Value.EndTime, notScheduled.Value.StartTime, notScheduled.Value.MaxDeviationAfter);


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
		public TimeSpan MaxDeviationBefore;
		public TimeSpan MaxDeviationAfter;
		public TimeSpan ActualDeviation;
		public double Odds;


	}
}
