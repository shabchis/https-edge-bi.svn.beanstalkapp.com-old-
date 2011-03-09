using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core;
using Easynet.Edge.Core.Configuration;
using MyScheduler.Objects;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;

namespace MyScheduler
{
	/// <summary>
	/// The new scheduler
	/// </summary>
	public class Scheduler
	{
		#region members
		private List<ServiceConfigration> _toBeScheduleServices = new List<ServiceConfigration>();
		private Dictionary<string, ServiceInstance> _scheduledServices = new Dictionary<string, ServiceInstance>();
		private Dictionary<int, ServiceConfigration> _servicesPerConfigurationID = new Dictionary<int, ServiceConfigration>();
		private Dictionary<int, ServiceConfigration> _servicesPerProfileID = new Dictionary<int, ServiceConfigration>();
		private Dictionary<string, ServiceInstance> _unscheduleServices = new Dictionary<string, ServiceInstance>();
		DateTime _scheduleFrom;
		DateTime _scheduleTo;
		private const int neededTimeLine = 120; //scheduling for the next xxx min....
		private const int Percentile = 80; //execution time of specifc service on sprcific Percentile
		#endregion

		/// <summary>
		/// Initialize all the services from configuration file or db4o
		/// </summary>
		/// <param name="getServicesFromConfigFile"></param>
		public Scheduler(bool getServicesFromConfigFile)
		{
			if (getServicesFromConfigFile)
				GetServicesFromConfigurationFile();


		}
		/// <summary>
		/// The main method of creating scheduler 
		/// </summary>
		public void CreateSchedule()
		{
			List<ServiceConfigration> servicesForNextTimeLine = GetServicesForNextTimeLine();

			List<ServiceConfigration> toBeScheduledByTimeAndPriority = SortBySuitableSchedulingRuleAndPriority(servicesForNextTimeLine); //servicesForNextTimeLine.OrderBy(ordered => ordered.SchedulingRules[0].Hours[0]).ThenByDescending(ordered => ordered.priority);

			foreach (ServiceConfigration service in toBeScheduledByTimeAndPriority)
			{
				ServiceInstance serviceInstance = SchedulePerService(service);
				ScheduleService(serviceInstance);
			}
			PrintSchduleTable();
		}
		/// <summary>
		/// Sort service by time and priotiry from desc
		/// </summary>
		/// <param name="servicesForNextTimeLine"></param>
		/// <returns>sorted list of service configurations</returns>
		private List<ServiceConfigration> SortBySuitableSchedulingRuleAndPriority(List<ServiceConfigration> servicesForNextTimeLine)
		{
			//TODO: IF SAME HOUR SORT BY PRIORITY
			List<ServiceConfigration> serviceSortedByTimeAndPriority = new List<ServiceConfigration>();
			Stack<ServiceHourStruct> stack = new Stack<ServiceHourStruct>();

			foreach (ServiceConfigration service in servicesForNextTimeLine)
			{
				TimeSpan hour = FindSuitableSchedulingHour(service.SchedulingRules);
				ServiceHourStruct serviceHour;
				serviceHour.SuitableHour = hour;
				serviceHour.Service = service;

				if (stack.Count == 0 || stack.Peek().SuitableHour > hour)
				{
					stack.Push(serviceHour);
				}
				else
				{
					while (stack.Count != 0 && stack.Peek().SuitableHour <hour)
					{
						serviceSortedByTimeAndPriority.Add(stack.Pop().Service);
					}
					stack.Push(serviceHour);
				}				
			}
			while (stack.Count != 0)
			{
				serviceSortedByTimeAndPriority.Add(stack.Pop().Service);
			}
			return serviceSortedByTimeAndPriority;

		}
		/// <summary>
		/// Since a rule can have a few hours we want only the hour in hour time line get scheduling rules
		/// </summary>
		/// <param name="schedulingRules"></param>
		/// <returns>hour from rule in needed time line</returns>
		private TimeSpan FindSuitableSchedulingHour(List<SchedulingRule> schedulingRules)
		{
			TimeSpan suitHour;
			SchedulingRule schedulingRule = FindSuitableSchedulingRule(schedulingRules);
			suitHour = FindSuitableSchedulingHour(schedulingRule.Hours);
			return suitHour;
			
		}
		/// <summary>
		/// Since a rule can have a few hours we want only the hour in hour time line get scheduling hours
		/// </summary>
		/// <param name="schedulingHours"></param>
		/// <returns>hour from hours in needed time line</returns>
		private TimeSpan FindSuitableSchedulingHour(List<TimeSpan> schedulingHours)
		{
			TimeSpan suitHour = new TimeSpan();
			foreach (TimeSpan hour in schedulingHours)
			{
				if (hour >= _scheduleFrom.TimeOfDay && hour <= _scheduleTo.TimeOfDay)
				{
					suitHour = hour;
					break;
				} 
			}
			return suitHour;
		}		
		/// <summary>
		/// Get only the services that in our time line by their scheduling rules hours get a list of service
		/// </summary>
		/// <returns>Return a list of services suit the time line</returns>
		private List<ServiceConfigration> GetServicesForNextTimeLine()
		{
			List<ServiceConfigration> nextTimeLineServices = new List<ServiceConfigration>();
			int timeLineInMin = neededTimeLine;
			 _scheduleFrom = DateTime.Now;
			 _scheduleTo = DateTime.Now.AddMinutes(timeLineInMin);
			foreach (ServiceConfigration service in _toBeScheduleServices)
			{				
				SchedulingRule schedulingRule = FindSuitableSchedulingRule(service.SchedulingRules);
				if (schedulingRule!=null)
					nextTimeLineServices.Add(service);			

			}
			return nextTimeLineServices;
		}
		/// <summary>
		/// Find suitacbe scheduling rule
		/// </summary>
		/// <param name="SchedulingRules"></param>
		/// <returns>scheduling rule</returns>
		private SchedulingRule FindSuitableSchedulingRule(List<SchedulingRule> SchedulingRules)
		{
			SchedulingRule foundedSchedulingRule = null;
			foreach (SchedulingRule schedulingRule in SchedulingRules)
			{
				if (schedulingRule != null)
				{
					foreach (TimeSpan hour in schedulingRule.Hours)
					{
						if (hour >= _scheduleFrom.TimeOfDay && hour <= _scheduleTo.TimeOfDay)
						{
							switch (schedulingRule.Scope)
							{
								case SchedulingScope.Day:
									{
										foundedSchedulingRule = schedulingRule;
										break;//TODO:IF ONE scheduling ROLE take place then continue to other scheduling rules? service per account more then once? 
									}
								case SchedulingScope.Week:
									{
										foreach (int day in schedulingRule.Days)
										{
											if (day == (int)_scheduleFrom.DayOfWeek + 1 || day == (int)_scheduleTo.DayOfWeek + 1)
											{
												foundedSchedulingRule = schedulingRule;
												break;
											}
										}
										break;
									}
								case SchedulingScope.Month://TODO: CHECK THIS IT IS MORE COMPLICATED I THING I HAVE A BUG HERE
									{
										foreach (int day in schedulingRule.Days)
										{
											if (day == _scheduleFrom.Day || day == _scheduleTo.Day)
											{
												foundedSchedulingRule = schedulingRule;
												break;
											}
										}
										break;
									}
							}
						}						
					}					
				}
			}
			return foundedSchedulingRule;
		}
		/// <summary>
		/// Load and translate the services from app.config
		/// </summary>
		public void GetServicesFromConfigurationFile()
		{			
			Dictionary<string, ServiceConfigration> baseConfigurations = new Dictionary<string, ServiceConfigration>();
			Dictionary<string, ServiceConfigration> configurations = new Dictionary<string, ServiceConfigration>();
			//base configuration
			foreach (ServiceElement serviceElement in ServicesConfiguration.Services)
			{
				ServiceConfigration serviceConfiguration = new ServiceConfigration();
				serviceConfiguration.Name = serviceElement.Name;				
				serviceConfiguration.MaxConcurrent = serviceElement.MaxInstances;
				serviceConfiguration.MaxCuncurrentPerProfile = serviceElement.MaxInstancesPerAccount;
				serviceConfiguration.ID = GetServceConfigruationIDByName(serviceConfiguration.Name);
				baseConfigurations.Add(serviceConfiguration.Name, serviceConfiguration);
			}
			//profiles=account and specific aconfiguration
			foreach (AccountElement account in ServicesConfiguration.Accounts)
			{
				foreach (AccountServiceElement accountService in account.Services)
				{
					ServiceElement serviceUse = accountService.Uses.Element;
					//active element is the calculated configuration 
					ActiveServiceElement activeServiceElement = new ActiveServiceElement(accountService);
					ServiceConfigration serviceConfiguration = new ServiceConfigration();					
					serviceConfiguration.Name = string.Format("{0}-{1}", account.ID, serviceUse.Name);
					serviceConfiguration.ID = GetServceConfigruationIDByName(serviceConfiguration.Name);
					serviceConfiguration.MaxConcurrent = activeServiceElement.MaxInstances;
					serviceConfiguration.MaxCuncurrentPerProfile = activeServiceElement.MaxInstancesPerAccount;					
					//scheduling rules 
					foreach (SchedulingRuleElement schedulingRuleElement in activeServiceElement.SchedulingRules)
					{
						SchedulingRule rule = new SchedulingRule();
						switch (schedulingRuleElement.CalendarUnit)
						{
							case CalendarUnit.AlwaysOn:
								{
									throw new Exception("UnKnown scheduling Rule");
									
								}
							case CalendarUnit.Day:
								rule.Scope = SchedulingScope.Day;
								break;
							case CalendarUnit.Month:
								rule.Scope = SchedulingScope.Month;
								break;
							case CalendarUnit.ReRun:
								{
									throw new Exception("UnKnown scheduling Rule");
									
								}
							case CalendarUnit.Week:
								rule.Scope = SchedulingScope.Week;
								break;
							default:
								break;
						}

						//subunits= weekday,monthdays
						rule.Days = schedulingRuleElement.SubUnits.ToList();
						rule.Hours = schedulingRuleElement.ExactTimes.ToList();
						rule.MaxDeviationAfter = schedulingRuleElement.MaxDeviation;
						if (serviceConfiguration.SchedulingRules == null)
							serviceConfiguration.SchedulingRules = new List<SchedulingRule>();
						serviceConfiguration.SchedulingRules.Add(rule);
					}
					serviceConfiguration.BaseConfiguration = baseConfigurations[serviceUse.Name];
					//profile settings
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
		/// <summary>
		/// Get the serviceconfigurationid by name
		/// </summary>
		/// <param name="serviceConfigurationName"></param>
		/// <returns>service configuration id=int</returns>
		private int GetServceConfigruationIDByName(string serviceConfigurationName)
		{
			int serviceConfigration=0;
			using (DataManager.Current.OpenConnection())
			{
				using (SqlCommand sqlCommand=DataManager.CreateCommand("ServiceConfigration_GetIDByName(@serviceConfigurationName:NvarChar)",System.Data.CommandType.StoredProcedure))
				{
					sqlCommand.Parameters["@serviceConfigurationName"].Value = serviceConfigurationName.Trim();					
					serviceConfigration = System.Convert.ToInt32(sqlCommand.ExecuteScalar());
				}				
			}
			return serviceConfigration;
		}
		/// <summary>
		/// set the service instance on the right time get the service instance with all the data of scheduling and more
		/// </summary>
		/// <param name="serviceInstance"></param>
		private void ScheduleService(ServiceInstance serviceInstance)
		{
			if (serviceInstance.ActualDeviation > serviceInstance.MaxDeviationAfter)  // check if the waiting time is bigger then max waiting time.
				_unscheduleServices.Add(serviceInstance.ServiceName, serviceInstance);
			else
			{
				_scheduledServices.Add(serviceInstance.ServiceName, serviceInstance);
			}
		}
		/// <summary>
		/// Schedule per service
		/// </summary>
		/// <param name="service"></param>
		/// <returns>service instance with scheduling data and more</returns>
		private ServiceInstance SchedulePerService(ServiceConfigration service)
		{



			//Get all services with same configurationID
			var servicesWithSameConfiguration = from s in _scheduledServices
												where s.Value.ConfigurationID == service.BaseConfiguration.ConfigurationID
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
		/// <summary>
		/// The algoritm of finding the the right time for service
		/// </summary>
		/// <param name="servicesWithSameConfiguration"></param>
		/// <param name="servicesWithSameProfile"></param>
		/// <param name="service"></param>
		/// <returns></returns>
		private ServiceInstance FindFirstFreeTime(IOrderedEnumerable<KeyValuePair<string, ServiceInstance>> servicesWithSameConfiguration, IOrderedEnumerable<KeyValuePair<string, ServiceInstance>> servicesWithSameProfile, ServiceConfigration service)
		{
			ServiceInstance scheduleInfo = null;
			TimeSpan suitableHour = FindSuitableSchedulingHour(service.SchedulingRules);			
			int executionTimeInMin = 0;
			executionTimeInMin = GetAverageExecutionTime(service.ID);
			//TODO: CHECK WITH DORON LEAKING YEARS MONTH DAY FOR THE NEXT ROW
			DateTime baseStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, suitableHour.Hours, suitableHour.Minutes, 0);
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
						scheduleInfo.Odds = Percentile;
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
						//get the next first place of ending service(next start time
						GetNewStartEndTime(servicesWithSameProfile, ref calculatedStartTime, ref calculatedEndTime, executionTimeInMin);
						////remove unfree time from servicePerConfiguration and servicePerProfile
						RemoveBusyTime(ref servicesWithSameConfiguration, ref servicesWithSameProfile, calculatedStartTime);
					}
				}
				else
				{
					GetNewStartEndTime(servicesWithSameConfiguration, ref calculatedStartTime, ref calculatedEndTime, executionTimeInMin);
					////remove unfree time from servicePerConfiguration and servicePerProfile
					RemoveBusyTime(ref servicesWithSameConfiguration, ref servicesWithSameProfile, calculatedStartTime);
				}
			}
			return scheduleInfo;
		}
		/// <summary>
		/// Get the average time of service run by configuration id and wanted percentile
		/// </summary>
		/// <param name="configurationID"></param>
		/// <returns></returns>
		private int GetAverageExecutionTime(int configurationID)
		{
			int averageExacutionTime;
			using (DataManager.Current.OpenConnection())
			{
				using (SqlCommand sqlCommand=DataManager.CreateCommand("ServiceConfiguration_GetExecutionTime(@ConfigID:Int,@Percentile:Int)",System.Data.CommandType.StoredProcedure))
				{
					sqlCommand.Parameters["@ConfigID"].Value = configurationID;
					sqlCommand.Parameters["@Percentile"].Value = Percentile;
					averageExacutionTime = System.Convert.ToInt32(sqlCommand.ExecuteScalar());
				}				
			}
			return averageExacutionTime;
		}
		/// <summary>
		/// if the schedule time is occupied then take first free time (minimum time)
		/// </summary>
		/// <param name="servicesWithSameProfile"></param>
		/// <param name="startTime"></param>
		/// <param name="endTime"></param>
		/// <param name="ExecutionTime"></param>
		private static void GetNewStartEndTime(IOrderedEnumerable<KeyValuePair<string, ServiceInstance>> servicesWithSameProfile, ref DateTime startTime, ref DateTime endTime, int ExecutionTime)
		{
			startTime = servicesWithSameProfile.Min(s => s.Value.EndTime);
			//Get end time
			endTime = startTime.AddMinutes(ExecutionTime);
		}
		/// <summary>
		/// remove busy time 
		/// </summary>
		/// <param name="servicesWithSameConfiguration"></param>
		/// <param name="servicesWithSameProfile"></param>
		/// <param name="startTime"></param>
		private void RemoveBusyTime(ref IOrderedEnumerable<KeyValuePair<string, ServiceInstance>> servicesWithSameConfiguration, ref IOrderedEnumerable<KeyValuePair<string, ServiceInstance>> servicesWithSameProfile, DateTime startTime)
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
		/// <summary>
		/// Print the schedule
		/// </summary>
		private void PrintSchduleTable()
		{
			Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "Name".PadRight(25,' '), "start", "end", "diff", "odds", "priority");
			Console.WriteLine("---------------------------------------------------------");
			KeyValuePair<string, ServiceInstance>? prev = null;
			foreach (KeyValuePair<string, ServiceInstance> scheduled in _scheduledServices.OrderBy(s => s.Value.StartTime))
			{
				Console.WriteLine("{0}\t{1:HH:mm}\t{2:HH:mm}\t{3:hh\\:mm}\t{4}\t{5}",
					scheduled.Value.ServiceName.PadRight(25,' '),
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
			foreach (KeyValuePair<string, ServiceInstance> notScheduled in _unscheduleServices)
			{
				Console.WriteLine("Service name: {0}\tBase start time:{1:hh:mm}\tschedule time is{2:hh:mm}\t maximum waiting time is{3}", notScheduled.Value.ServiceName, notScheduled.Value.EndTime, notScheduled.Value.StartTime, notScheduled.Value.MaxDeviationAfter);
			}
			Console.ReadLine();
		}
	}
	/// <summary>
	/// Date of scheduling
	/// </summary>
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
	/// <summary>
	/// service-hour 
	/// </summary>
	public struct ServiceHourStruct
	{
		public TimeSpan SuitableHour;
		public ServiceConfigration Service;
	}
}
