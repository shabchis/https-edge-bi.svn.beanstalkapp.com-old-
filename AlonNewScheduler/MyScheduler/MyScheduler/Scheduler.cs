﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;
using MyScheduler.Objects;
using System.Threading;
using Easynet.Edge.Core.Configuration;



namespace MyScheduler
{

	/// <summary>
	/// The new scheduler
	/// </summary>
	public class Scheduler
	{
		#region members
		private List<ServiceConfiguration> _toBeScheduleServices = new List<ServiceConfiguration>();
		private Dictionary<SchedulingData, ServiceInstance> _scheduledServices = new Dictionary<SchedulingData, ServiceInstance>();
		private Dictionary<int, ServiceConfiguration> _servicesPerConfigurationID = new Dictionary<int, ServiceConfiguration>();
		private Dictionary<int, ServiceConfiguration> _servicesPerProfileID = new Dictionary<int, ServiceConfiguration>();
		private Dictionary<SchedulingData, ServiceInstance> _unscheduleServices = new Dictionary<SchedulingData, ServiceInstance>();
		DateTime _timeLineFrom;
		DateTime _timeLineTo;
		private const int neededTimeLine = 1440; //scheduling for the next xxx min....
		private const int Percentile = 80; //execution time of specifc service on sprcific Percentile
		private Thread thread;
		public event EventHandler TimeToRunEventHandler;
		public event EventHandler ServiceNotScheduledHandler;



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
		public void NewSchedule()
		{
			List<SchedulingData> servicesForNextTimeLine = GetServicesForNextTimeLine(false);


			var toBeScheduledByTimeAndPriority = servicesForNextTimeLine.OrderBy(s =>s.TimeToRun).ThenBy(s => s.Priority);
			ClearServicesforReschedule(toBeScheduledByTimeAndPriority);
			foreach (SchedulingData schedulingData in toBeScheduledByTimeAndPriority)
			{

				//if key exist then this service is runing or ednededule again
				if (!_scheduledServices.ContainsKey(schedulingData))
				{
					ServiceInstance serviceInstance = ScheduleSpecificService(schedulingData);
					KeyValuePair<SchedulingData, ServiceInstance> serviceInstanceAndRuleHash = new KeyValuePair<SchedulingData, ServiceInstance>(schedulingData, serviceInstance);
					UpdateScheduleTable(serviceInstanceAndRuleHash);
				}
			}
			PrintSchduleTable();
		}

		private void ClearServicesforReschedule(IOrderedEnumerable<Objects.SchedulingData> toBeScheduledByTimeAndPriority)
		{
			foreach (SchedulingData schedulingData in toBeScheduledByTimeAndPriority)
			{
				if (_scheduledServices.ContainsKey(schedulingData))
					_scheduledServices.Remove(schedulingData);


			}

		}

		/// <summary>
		/// reschedule
		/// </summary>
		public void ReSchedule()
		{
			List<SchedulingData> servicesForNextTimeLine = GetServicesForNextTimeLine(true);
			var toBeScheduledByTimeAndPriority = servicesForNextTimeLine.OrderBy(s => s.SelectedDay).ThenBy(s => s.SelectedHour).ThenBy(s => s.Priority);


			foreach (SchedulingData schedulingData in toBeScheduledByTimeAndPriority)
			{
			
				if (_scheduledServices.ContainsKey(schedulingData) && _scheduledServices[schedulingData].State == serviceStatus.Scheduled)
					_scheduledServices.Remove(schedulingData);

				if (!_scheduledServices.ContainsKey(schedulingData)) //if key exist then this service has been ended and we can schedule again or it's runing and we can't use this time line
				{
					ServiceInstance serviceInstance = ScheduleSpecificService(schedulingData);
					KeyValuePair<SchedulingData, ServiceInstance> serviceInstanceAndRuleHash = new KeyValuePair<SchedulingData, ServiceInstance>(schedulingData, serviceInstance);
					UpdateScheduleTable(serviceInstanceAndRuleHash);
				}
			}
			PrintSchduleTable();

		}

		/// <summary>
		/// Return all the services not started to run or did not finished runing
		/// </summary>
		/// <returns></returns>
		public IEnumerable<KeyValuePair<SchedulingData, MyScheduler.Objects.ServiceInstance>> GetScheduldServicesWithStatusNotEnded()
		{
			//Dictionary<SchedulingData, ServiceInstance> returnObject;
			var returnObject = from s in _scheduledServices
							   where s.Value.State != serviceStatus.Ended
							   select s;
			return returnObject;

		}

		/// <summary>
		/// returns all scheduled services
		/// </summary>
		/// <returns></returns>
		public IEnumerable<KeyValuePair<SchedulingData, ServiceInstance>> GetAlllScheduldServices()
		{
			var returnObject = from s in _scheduledServices
							   select s;
			return returnObject;
		}

		/// <summary>
		/// set service state
		/// </summary>
		/// <param name="scheduilngData"></param>
		/// <param name="serviceStatus"></param>
		public void SetServiceState(SchedulingData scheduilngData, serviceStatus serviceStatus)
		{
			_scheduledServices[scheduilngData].State = serviceStatus;
		}

		/// <summary>
		/// add unplanned service to schedule
		/// </summary>
		/// <param name="serviceConfiguration"></param>
		public void AddNewServiceToSchedule(ServiceConfiguration serviceConfiguration)
		{
			_toBeScheduleServices.Add(serviceConfiguration);
			ReSchedule();
		}

		/// <summary>
		/// Get this time line services 
		/// </summary>
		/// <param name="reschedule">if it's for reschedule then the time line is the same as the last schedule</param>
		/// <returns></returns>
		private List<SchedulingData> GetServicesForNextTimeLine(bool reschedule)
		{
			if (!reschedule)
			{

				int timeLineInMin = neededTimeLine;
				_timeLineFrom = DateTime.Now;
				_timeLineTo = DateTime.Now.AddMinutes(timeLineInMin);
			}
			List<SchedulingData> schedulingData = FindSuitableSchedulingRule();
			return schedulingData;
		}

		/// <summary>
		/// return a services that are suitable for the given  time line
		/// </summary>
		/// <returns></returns>
		private List<SchedulingData> FindSuitableSchedulingRule()
		{
			SchedulingData Schedulingdata;
			List<SchedulingData> foundedSchedulingdata = new List<SchedulingData>();
			foreach (ServiceConfiguration service in _toBeScheduleServices)
			{
				foreach (SchedulingRule schedulingRule in service.SchedulingRules)
				{
					if (schedulingRule != null)
					{
						foreach (TimeSpan hour in schedulingRule.Hours)
						{

							switch (schedulingRule.Scope)
							{
								case SchedulingScope.Day:
									{
										if ((_timeLineFrom.TimeOfDay <= _timeLineTo.TimeOfDay && hour >= _timeLineFrom.TimeOfDay) || //same day //todo: check this with doron
											(_timeLineFrom.TimeOfDay >= _timeLineTo.TimeOfDay && (hour <= _timeLineFrom.TimeOfDay && hour >= _timeLineTo.TimeOfDay))) //diffarent day
										{
											Schedulingdata = new SchedulingData() { Configuration = service, Rule = schedulingRule, SelectedDay = (int)(DateTime.Now.DayOfWeek) + 1, SelectedHour = hour, Priority = service.priority, LegacyConfiguration = service.LegacyConfiguration, TimeToRun = DateTime.Today + hour };
											foundedSchedulingdata.Add(Schedulingdata);

										}
										if (_timeLineFrom.DayOfWeek != _timeLineTo.DayOfWeek) //maybe the same service is relevant for tommorw so..if
										{

											int i = _timeLineTo.DayOfWeek - _timeLineFrom.DayOfWeek;
											while (i > 0)
											{												
													if (hour <= _timeLineTo.TimeOfDay)
													{
														Schedulingdata = new SchedulingData() { Configuration = service, Rule = schedulingRule, SelectedDay = (int)(DateTime.Now.DayOfWeek) + 2, SelectedHour = hour, Priority = service.priority, LegacyConfiguration = service.LegacyConfiguration, TimeToRun = DateTime.Today.AddDays(i) + hour };
														foundedSchedulingdata.Add(Schedulingdata);
													}
													i--;
												
											}

										}
										break;
									}
								case SchedulingScope.Week:
									{
										foreach (int day in schedulingRule.Days)
										{
											//todo: get the day to run
											if (day == (int)_timeLineFrom.DayOfWeek + 1 || day == (int)_timeLineTo.DayOfWeek + 1)
											{
												if ((_timeLineFrom.TimeOfDay <= _timeLineTo.TimeOfDay && hour >= _timeLineFrom.TimeOfDay) || //same day
													(_timeLineFrom.TimeOfDay >= _timeLineTo.TimeOfDay && (hour <= _timeLineFrom.TimeOfDay && hour >= _timeLineTo.TimeOfDay))) //diffarent day
												{
													Schedulingdata = new SchedulingData() { Configuration = service, Rule = schedulingRule, SelectedDay = day, SelectedHour = hour, Priority = service.priority, LegacyConfiguration = service.LegacyConfiguration };
													foundedSchedulingdata.Add(Schedulingdata);
												}
											}
										}
										break;
									}
								case SchedulingScope.Month://TODO: 31,30,29 of month can be problematicly
									{
										foreach (int day in schedulingRule.Days)
										{
											if (day == _timeLineFrom.Day + 1 || day == _timeLineTo.Day + 1)
											{
												if ((_timeLineFrom.TimeOfDay <= _timeLineTo.TimeOfDay && hour >= _timeLineFrom.TimeOfDay) || //same day
											(_timeLineFrom.TimeOfDay >= _timeLineTo.TimeOfDay && (hour <= _timeLineFrom.TimeOfDay && hour >= _timeLineTo.TimeOfDay))) //diffarent day
												{
													Schedulingdata = new SchedulingData() { Configuration = service, Rule = schedulingRule, SelectedDay = day, SelectedHour = hour, Priority = service.priority, LegacyConfiguration = service.LegacyConfiguration };
													foundedSchedulingdata.Add(Schedulingdata);
												}
											}
										}
										break;
									}
							}

						}
					}
				}
			}
			return foundedSchedulingdata;
		}

		/// <summary>
		/// Find suitacbe scheduling rule
		/// </summary>
		/// <param name="SchedulingRules"></param>
		/// <returns>scheduling rule</returns>

		/// <summary>
		/// Load and translate the services from app.config
		/// </summary>
		public void GetServicesFromConfigurationFile()
		{
			Dictionary<string, ServiceConfiguration> baseConfigurations = new Dictionary<string, ServiceConfiguration>();
			Dictionary<string, ServiceConfiguration> configurations = new Dictionary<string, ServiceConfiguration>();
			//base configuration
			foreach (ServiceElement serviceElement in ServicesConfiguration.Services)
			{
				ServiceConfiguration serviceConfiguration = new ServiceConfiguration();
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
					ServiceConfiguration serviceConfiguration = new ServiceConfiguration();
					serviceConfiguration.Name = string.Format("{0}-{1}", account.ID, serviceUse.Name);
					serviceConfiguration.ID = GetServceConfigruationIDByName(serviceConfiguration.Name);
					serviceConfiguration.MaxConcurrent = (activeServiceElement.MaxInstances == 0) ? 9999 : activeServiceElement.MaxInstances;
					serviceConfiguration.MaxCuncurrentPerProfile = (activeServiceElement.MaxInstancesPerAccount == 0) ? 9999 : activeServiceElement.MaxInstancesPerAccount;
					serviceConfiguration.LegacyConfiguration = activeServiceElement;
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
			int serviceConfigration = 0;
			using (DataManager.Current.OpenConnection())
			{
				using (SqlCommand sqlCommand = DataManager.CreateCommand("ServiceConfigration_GetIDByName(@serviceConfigurationName:NvarChar)", System.Data.CommandType.StoredProcedure))
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
		/// <param name="serviceInstanceAndRuleHash"></param>
		private void UpdateScheduleTable(KeyValuePair<SchedulingData, ServiceInstance> serviceInstanceAndRuleHash)
		{
			if (serviceInstanceAndRuleHash.Value.ActualDeviation > serviceInstanceAndRuleHash.Value.MaxDeviationAfter)
			{
				// check if the waiting time is bigger then max waiting time.
				_unscheduleServices.Add(serviceInstanceAndRuleHash.Key, serviceInstanceAndRuleHash.Value);
				OnServiceNotScheduled(new ServiceNotScheduledEventArgs() { NotScheduledInformation = serviceInstanceAndRuleHash });
			}
			else
			{
				_scheduledServices.Add(serviceInstanceAndRuleHash.Key, serviceInstanceAndRuleHash.Value);
			}
		}

		/// <summary>
		/// Schedule per service
		/// </summary>
		/// <param name="schedulingData"></param>
		/// <returns>service instance with scheduling data and more</returns>
		private ServiceInstance ScheduleSpecificService(SchedulingData schedulingData)
		{
			//Get all services with same configurationID
			var servicesWithSameConfiguration = from s in _scheduledServices
												where s.Value.BaseConfigurationID == schedulingData.Configuration.BaseConfiguration.ID && (s.Value.State != serviceStatus.Ended) //runnig or not started yet
												orderby s.Value.StartTime ascending
												select s;

			//Get all services with same profileID

			var servicesWithSameProfile = from s in _scheduledServices
										  where s.Value.ProfileID == schedulingData.Configuration.SchedulingProfile.ID &&
										  s.Value.BaseConfigurationID == schedulingData.Configuration.BaseConfiguration.ID &&
										  (s.Value.State != serviceStatus.Ended) //runnig or not started yet
										  orderby s.Value.StartTime ascending
										  select s;


			//this is old before fixing the same account bug
			//var servicesWithSameProfile = from s in _scheduledServices
			//                              where s.Value.ProfileID == schedulingData.Configuration.SchedulingProfile.ID && (s.Value.State != serviceStatus.Ended) //runnig or not started yet
			//                              orderby s.Value.StartTime ascending
			//                              select s;

			ServiceInstance serviceInstance = FindFirstFreeTime(servicesWithSameConfiguration, servicesWithSameProfile, schedulingData);

			return serviceInstance;
		}

		/// <summary>
		/// The algoritm of finding the the right time for service
		/// </summary>
		/// <param name="servicesWithSameConfiguration"></param>
		/// <param name="servicesWithSameProfile"></param>
		/// <param name="schedulingData"></param>
		/// <returns></returns>
		private ServiceInstance FindFirstFreeTime(IOrderedEnumerable<KeyValuePair<SchedulingData, ServiceInstance>> servicesWithSameConfiguration, IOrderedEnumerable<KeyValuePair<SchedulingData, ServiceInstance>> servicesWithSameProfile, SchedulingData schedulingData)
		{
			ServiceInstance scheduleInfo = null;
			TimeSpan suitableHour = schedulingData.SelectedHour;

			int executionTimeInMin = 0;
			executionTimeInMin = GetAverageExecutionTime(schedulingData.Configuration.ID);
		
			DateTime baseStartTime = schedulingData.TimeToRun;
			DateTime baseEndTime = baseStartTime.AddMinutes(executionTimeInMin);
			DateTime calculatedStartTime = baseStartTime;
			DateTime calculatedEndTime = baseEndTime;
			bool found = false;


			while (!found)
			{
				int countedPerConfiguration = servicesWithSameConfiguration.Count(s => (calculatedStartTime >= s.Value.StartTime && calculatedStartTime <= s.Value.EndTime) || (calculatedEndTime >= s.Value.StartTime && calculatedEndTime <= s.Value.EndTime));
				if (countedPerConfiguration < schedulingData.Configuration.MaxConcurrent)
				{
					int countedPerProfile = servicesWithSameProfile.Count(s => (calculatedStartTime >= s.Value.StartTime && calculatedStartTime <= s.Value.EndTime) || (calculatedEndTime >= s.Value.StartTime && calculatedEndTime <= s.Value.EndTime));
					if (countedPerProfile < schedulingData.Configuration.MaxCuncurrentPerProfile)
					{
						scheduleInfo = new ServiceInstance();
						scheduleInfo.StartTime = calculatedStartTime;
						scheduleInfo.EndTime = calculatedEndTime;
						scheduleInfo.Odds = Percentile;
						scheduleInfo.ActualDeviation = calculatedStartTime.Subtract(baseStartTime);
						scheduleInfo.Priority = schedulingData.Priority;
						scheduleInfo.BaseConfigurationID = schedulingData.Configuration.BaseConfiguration.ID;
						scheduleInfo.ID = schedulingData.Configuration.ID;
						scheduleInfo.MaxConcurrentPerConfiguration = schedulingData.Configuration.MaxConcurrent;
						scheduleInfo.MaxCuncurrentPerProfile = schedulingData.Configuration.MaxCuncurrentPerProfile;
						scheduleInfo.MaxDeviationAfter = schedulingData.Rule.MaxDeviationAfter;
						scheduleInfo.ActualDeviation = calculatedStartTime.Subtract(baseStartTime);
						scheduleInfo.MaxDeviationBefore = schedulingData.Rule.MaxDeviationBefore;
						scheduleInfo.ProfileID = schedulingData.Configuration.SchedulingProfile.ID;
						scheduleInfo.LegacyConfiguration = schedulingData.LegacyConfiguration;
						scheduleInfo.ServiceName = schedulingData.Configuration.Name;
						scheduleInfo.State = serviceStatus.Scheduled;
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
				using (SqlCommand sqlCommand = DataManager.CreateCommand("ServiceConfiguration_GetExecutionTime(@ConfigID:Int,@Percentile:Int)", System.Data.CommandType.StoredProcedure))
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
		private static void GetNewStartEndTime(IOrderedEnumerable<KeyValuePair<SchedulingData, ServiceInstance>> servicesWithSameProfile, ref DateTime startTime, ref DateTime endTime, int ExecutionTime)
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
		private void RemoveBusyTime(ref IOrderedEnumerable<KeyValuePair<SchedulingData, ServiceInstance>> servicesWithSameConfiguration, ref IOrderedEnumerable<KeyValuePair<SchedulingData, ServiceInstance>> servicesWithSameProfile, DateTime startTime)
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
			Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", "SchedulidDataID", "Name".PadRight(25, ' '), "start", "end", "diff", "odds", "priority");
			Console.WriteLine("---------------------------------------------------------");
			KeyValuePair<SchedulingData, ServiceInstance>? prev = null;
			foreach (KeyValuePair<SchedulingData, ServiceInstance> scheduled in _scheduledServices.OrderBy(s => s.Value.StartTime))
			{
				Console.WriteLine("{0}\t{1:HH:mm}\t{2:HH:mm}\t{3:hh\\:mm}\t{4}\t{5}\t{6}",
					scheduled.Key.GetHashCode(),
					scheduled.Value.ServiceName.PadRight(25, ' '),
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
			foreach (KeyValuePair<SchedulingData, ServiceInstance> notScheduled in _unscheduleServices)
			{
				Console.WriteLine("Service name: {0}\tBase start time:{1:hh:mm}\tschedule time is{2:hh:mm}\t maximum waiting time is{3}", notScheduled.Value.ServiceName, notScheduled.Value.EndTime, notScheduled.Value.StartTime, notScheduled.Value.MaxDeviationAfter);
			}
			Console.ReadLine();
		}
		public void Start()
		{
			thread = new Thread(new ThreadStart(CheckIfItsTimeToRun));
			thread.Start();

		}
		public void Stop()
		{
			thread.Abort();

		}
		private void CheckIfItsTimeToRun()
		{
			Dictionary<SchedulingData, ActiveServiceElement> activeServiceElements = new Dictionary<SchedulingData, ActiveServiceElement>();
			while (true)
			{
				//DO some checks

				foreach (var scheduleService in _scheduledServices)
				{
					if (scheduleService.Key.SelectedDay == ((int)DateTime.Now.DayOfWeek) + 1) //same day
					{
						TimeSpan now = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
						if (scheduleService.Key.SelectedHour == now)
						{
							activeServiceElements.Add(scheduleService.Key,scheduleService.Value.LegacyConfiguration);
						}
					}
				}
				if (activeServiceElements.Count > 0)
					OnTimeToRun(new TimeToRunEventArgs() { ActiveServiceElements = activeServiceElements });


				Thread.Sleep(6000);
				activeServiceElements.Clear();
			}
		}
		public void OnTimeToRun(TimeToRunEventArgs e)
		{
			TimeToRunEventHandler(this, e);


		}
		public void OnServiceNotScheduled(ServiceNotScheduledEventArgs e)
		{
			ServiceNotScheduledHandler(this, e);
		}




	}
	public class TimeToRunEventArgs : EventArgs
	{
		public  Dictionary<SchedulingData,ActiveServiceElement> ActiveServiceElements;
	}
	public class ServiceNotScheduledEventArgs : EventArgs
	{
		public KeyValuePair<SchedulingData, ServiceInstance> NotScheduledInformation;
	}



}
