using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.ComponentModel;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using System.Diagnostics.Eventing.Reader;
using System.IO;

namespace Easynet.Edge.Services.ScheduleManagement
{
    // Remarks:
    // In this version we won't develop the following features: 
    // - max instances for account.
    // - Load configuration when it changed in the middle of the day.

    /// <summary>
    /// This class build and update the ScheduleTable.
    /// </summary>
    /// <author>Yaniv Kahana</author>
    /// <creation_date>01/07/2008</creation_date>
	
    public class ScheduleBuilder 
    {
        #region Fields
        /*=========================*/

        private ScheduleTable _scheduleTable = new ScheduleTable();
		private bool _debugMode = false;
		private Dictionary<string, int> accountList = new Dictionary<string, int>();
		private bool _firstRun = true;

        /*=========================*/
        #endregion

        #region Events
        /*=========================*/

		private EventHandler _outcomeReportedHandler;
		private EventHandler<ServiceStateChangedEventArgs> _stateChangedHandler;
        private EventHandler<ServiceRequestedEventArgs> _childServiceRequestedHandler;


         /*=========================*/
        #endregion
       
        #region Constructor
        /*=========================*/

        /// <summary>
        /// Constructor 
        /// </summary>
        public ScheduleBuilder()
        {
			_stateChangedHandler = new EventHandler<ServiceStateChangedEventArgs>(service_StateChanged);
            _outcomeReportedHandler = new EventHandler(service_OutcomeReported);
            _childServiceRequestedHandler = new EventHandler<ServiceRequestedEventArgs>(service_ChildServiceRequested);
           
			_debugMode = Convert.ToBoolean(AppSettings.GetAbsolute("DebugMode"));
        }

        /*=========================*/
        #endregion

        #region Public Methods
        /*=========================*/

		public void AbortAllservices()
		{
			_scheduleTable.AbortAllservices();
		}

		protected string FormatPath(string path)
		{
			if
			(
				path.Length > 0 &&
				path[path.Length - 1] != Path.DirectorySeparatorChar &&
				path[path.Length - 1] != Path.AltDirectorySeparatorChar
			)
			{
				path += Path.DirectorySeparatorChar;
			}

			return path;
		}

        /// <summary>
        /// Main function - build the services lists from the data in the 
        /// configuration file.
        /// </summary>
        /// <remarks>
        /// We schedule the table for next day. 
        /// For example on sunday 23:00 we schedule the table for monday.
        /// </remarks>
		public void BuildScheduling(string scheduleServiceName, long instanceID)
		{
			// We write this log as warning to make it sticking out in the event viewer.
			if (_debugMode)
				Log.Write("Begin building scheduling table.", LogMessageType.Warning);
			
			_scheduleTable.RunTaskSync.WaitOne();
			//_scheduleTable.Timer.Enabled = false;

			// Initalize scheduleServiceName only once.
			if (!String.IsNullOrEmpty(scheduleServiceName))
			{
				_scheduleTable.ScheduleServiceName = scheduleServiceName;
				_scheduleTable.ServiceInstanceID = instanceID;
			}

			// Loop on all the accounts in the configuratin file.
			foreach (AccountElement account in ServicesConfiguration.Accounts)
			{
				// Validate each account.
				if (!ValidateAccount(account))				
					continue;			

				// Loop on all the services of the account.
				foreach (AccountServiceElement accountServiceElement in account.Services)
				{
					if (!accountServiceElement.IsEnabled)
						continue;
					
					ServiceElement serviceElement = accountServiceElement.Uses.Element;
					// Validate each service
					if (serviceElement == null || !ValidateService(serviceElement))					
						continue;				

					if (serviceElement.IsEnabled)
						AddServicesByRules(accountServiceElement, account.ID);				
				}
			}

			// Write the scheduling to a file.
			try
			{
				string directory = string.Format(FormatPath(AppSettings.Get(this, "ResultsRoot")) + @"ScheduleManager\{0:yyyy}\{0:MM}\{0:dd}", DateTime.Now);

				if (!Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				string fileName = directory + string.Format(@"\" + _scheduleTable.ScheduleServiceName + "_service_schedule_{0:yyyyMMdd}@{0:t}.txt", DateTime.Now);
				fileName = fileName.Remove(fileName.LastIndexOf(':'), 1);
				//File.Create(fileName);
				StreamWriter sw = File.AppendText(fileName);

				// Write the schdeluing to a file
				foreach (string serviceName in _scheduleTable.Keys.ToArray())
				{
					/// Loop on services in current service list.
					foreach (ServiceInstance service in _scheduleTable[serviceName].ToArray<ServiceInstance>())
					{
						sw.WriteLine(serviceName + " for account " + ServicesConfiguration.Accounts.GetAccount(service.AccountID).Name + " (" + service.AccountID + ") " + " scheduled to:");
						sw.WriteLine("CalendarUnit: " + service.ActiveSchedulingRule.CalendarUnit);
						sw.WriteLine("ExactTimes: " + (service.ActiveSchedulingRule.ExactTimes.Length > 0 ? service.ActiveSchedulingRule.ExactTimes[0].ToString() : "None"));
						sw.WriteLine("SubUnits: " + (service.ActiveSchedulingRule.SubUnits.Length > 0 ? service.ActiveSchedulingRule.SubUnits[0].ToString() : "None"));
						sw.WriteLine(string.Empty);
					}
				}
				sw.Flush();
				sw.Close();
				sw.Dispose();
			}
			catch (Exception ex)
			{
				Log.Write("Can't write services schedule file", ex, LogMessageType.Error);
			}

			_firstRun = false;
			//_scheduleTable.Timer.Enabled = true;
			_scheduleTable.RunTaskSync.Release();
		}


		/// <summary>
		/// Create a service by serviceElement and add it as a manual request. 
		/// </summary>
		/// <param name="serviceElement">the service will be created by this service element settings.</param>
		public void AddManualRequest(EnabledConfigurationElement serviceElement,DateTime targetTime, int accountID)
		{
			//_scheduleTable.Timer.Enabled = false;
			_scheduleTable.RunTaskSync.WaitOne();

			ServiceInstance service;
			try
			{
				service = CreateNewService(serviceElement, targetTime, null, accountID);
			}
			catch (Exception ex)
			{
				Log.Write(string.Format("Can't add the service {0} for accoutID {1}", serviceElement.ToString(), accountID.ToString()), ex, LogMessageType.Error);
				_scheduleTable.RunTaskSync.Release();
				//_scheduleTable.Timer.Enabled = true;
				return;
			}

			if (service != null)		
				_scheduleTable.AddService(service);
			

			//_scheduleTable.Timer.Enabled = true;
			_scheduleTable.RunTaskSync.Release();
		}

        /// <summary>
        /// Create a service by serviceElement and add it as a manual request. 
        /// </summary>
        /// <param name="serviceElement">the service will be created by this service element settings.</param>
        public void AddManualRequest(EnabledConfigurationElement serviceElement, int accountID)
        {		
			//_scheduleTable.Timer.Enabled = false;
			_scheduleTable.RunTaskSync.WaitOne();
			ServiceInstance service;
			try
			{
				service = CreateNewService(serviceElement, DateTime.Now, null, accountID);				
			}
			catch (Exception ex)
			{
				Log.Write(string.Format("Can't add the service {0} for accoutID {1}", serviceElement.ToString(), accountID.ToString()), ex, LogMessageType.Error);
				//_scheduleTable.Timer.Enabled = true;
				_scheduleTable.RunTaskSync.Release();
				return;
			}

			if (service != null)
				_scheduleTable.AddService(service);
			

			//_scheduleTable.Timer.Enabled = true;
			_scheduleTable.RunTaskSync.Release();
        }

        /// <summary>
        /// Check the service rule and add it if needed.
        /// </summary>
        /// <param name="serviceElement"></param>
        //public void AddServicesByRules(ServiceElement serviceElement,int accountID)
		public void AddServicesByRules(AccountServiceElement accountServiceElement, int accountID)
        {
			AddServicesByRules(accountServiceElement, accountID, null);
        }

		public void AddServicesByRules(ServiceInstance childService)
		{
			// ServiceInstance's rules loop.
			foreach (SchedulingRuleElement rule in childService.Configuration.SchedulingRules)
			{
				HandleRule(null, childService.AccountID, childService, rule);
			}
		}

        /// <summary>
        /// Check the service rule and add it if needed.
        /// </summary>
        /// <param name="serviceElement"></param>
        /// <remarks>
        /// We schedule the table for next day. 
        /// For example on sunday 23:00 we schedule the table for monday.
        /// </remarks>
        /// <param name="childService">if we add a service request this service is the requested child service to schedule.</param>
		public void AddServicesByRules(AccountServiceElement accountServiceElement, int accountID, ServiceInstance childService)
        {
			ActiveServiceElement element = new ActiveServiceElement(accountServiceElement);

            //TODO: check if there is parent to the service

			foreach (SchedulingRuleElement rule in element.SchedulingRules)
			{
				// TODO: to check if there are same scheduling rules to account and service.
				HandleRule(accountServiceElement, accountID, childService, rule);
			}
        }

		/// <summary>
		/// Create a service According to the service rule.
		/// </summary>
		/// <param name="accountServiceElement"></param>
		/// <param name="accountID"></param>
		/// <param name="childService"></param>
		/// <param name="rule"></parammmmm>
		private void HandleRule(AccountServiceElement accountServiceElement, int accountID, ServiceInstance childService, SchedulingRuleElement rule)
		{
			switch (rule.CalendarUnit)
			{
				case CalendarUnit.ReRun:
					if (ScheduleConvertor.CheckFullSchedule(rule.FullSchedule))
						AddServiceRule(accountServiceElement, rule, childService, accountID);

					break;
				// Month 
				case CalendarUnit.Month:
					// Loop on all the days values in the rule.
					foreach (int day in rule.SubUnits)
					{
						if (day == DateTime.Now.Day)
							AddServiceRule(accountServiceElement, rule, childService, accountID);
					}
					break;

				// Week 
				case CalendarUnit.Week:
					// Loop on all the days values in the rule.
					foreach (int day in rule.SubUnits)
					{
						// DayOfWeek return values of 0-6 and because of it we -1 on left side.
						if (day == (int)DateTime.Now.DayOfWeek + 1)
							AddServiceRule(accountServiceElement, rule, childService, accountID);
					}
					break;

				// Day 
				case CalendarUnit.Day:
					AddServiceRule(accountServiceElement, rule, childService, accountID);
					break;

				// AlwaysOn 
				case CalendarUnit.AlwaysOn:
					
					ServiceInstance service;
					if (childService == null && _firstRun)
					{
						try
						{
							service = CreateNewService(accountServiceElement, DateTime.Now, rule, accountID);
						}
						catch (Exception ex)
						{
							Log.Write(string.Format("Can't add the always on service {0} for accoutID {1}", accountServiceElement.ToString(), accountID.ToString()), ex, LogMessageType.Error);
							return;
						}
					}
					else
					{
						service = childService;
					}

					// Add the service to the scheduleTable.   
					if (service != null)
						_scheduleTable.AddService(service);
					break;

				// Never should be here 
				default:				
					Log.Write(String.Format("The service {0}  don't have calendar unit, can't schedule the service."
						, accountServiceElement != null ? accountServiceElement.Uses.Element.Name : childService.Configuration.Name), LogMessageType.Warning);
					break;
			}
		}

        /*=========================*/

        #endregion
       
        #region Events handlers
        /*=========================*/

		static string Header(ServiceInstance instance)
		{

			string output = string.Format("{0} {1} - {2,3} - ",
				DateTime.Now.ToString("dd/MM"),
				DateTime.Now.ToShortTimeString(),
				instance.AccountID);

			for (ServiceInstance parent = instance.ParentInstance; parent != null; parent = parent.ParentInstance)
				output += "    ";

			output += instance.ToString();

			return output;
		}

		void service_StateChanged(object sender, ServiceStateChangedEventArgs e)
		{
			ServiceInstance service = (ServiceInstance)sender;
			//Console.WriteLine("{0} is {1} - [{2}]",
			//    Header(service),
			//    e.StateAfter,
			//    service.Configuration.Options);

			Console.WriteLine("{0} is {1}",
				Header(service),
				e.StateAfter);

			// Write to log
			//Log.Write(String.Format("{0} is {1}",
			//    service,
			//    e.StateAfter),
			//    LogMessageType.Information,
			//    service.AccountID);
		}

        /// <summary>
        /// Handle the service outcome: report to the service parent (if exist)
        /// and remove the service from the service list.
        /// </summary>
        /// <param name="sender">The service that raise the outcome event</param>
        /// <param name="e">Arguments of the event</param>
        void service_OutcomeReported(object sender, EventArgs e)
		{
			ServiceInstance service = (ServiceInstance)sender;

			Console.WriteLine("{0} reported {1}",
				Header(service),
				service.Outcome);

			// Write to log
			Log.Write(String.Format("{0} reported {1}.\n",
				service,
				service.Outcome),
				service.Outcome == ServiceOutcome.Success ? LogMessageType.Information : LogMessageType.Error);

			//_scheduleTable.Timer.Enabled = false;
			_scheduleTable.RunTaskSync.WaitOne();

			if ((service.Configuration.SchedulingRules.Count > 0) && 
				(service.Configuration.SchedulingRules[0].CalendarUnit == CalendarUnit.AlwaysOn))
			{
				ServiceInstance newService;
				try
				{
					newService = CreateNewService(service.Configuration, DateTime.Now, service.ActiveSchedulingRule, service.AccountID);
				}
				catch (Exception ex)
				{
					Log.Write(string.Format("Can't add the always on service {0} for accoutID {1}", service.ToString(), service.AccountID.ToString()), ex, LogMessageType.Error);
					return;
				}
				           
                // Add the service to the schedule table.
				if (newService != null)
					_scheduleTable.AddService(newService);
			}
			
			// Remove the service' events and remove it from the relevent service List.                
			service.OutcomeReported -= _outcomeReportedHandler;
			service.ChildServiceRequested -= _childServiceRequestedHandler;

			try
			{
				_scheduleTable[service.Configuration.Name].Remove(service);
			}
			catch (Exception ex)
			{
				Log.Write(string.Format("Can't remove {0} from service list", service), ex);
			}

			// Delete the list if empty.       
			if (_scheduleTable[service.Configuration.Name].Count == 0)
				_scheduleTable.Remove(service.Configuration.Name);

			//_scheduleTable.Timer.Enabled = true;
			_scheduleTable.RunTaskSync.Release();
		}


        /// <summary>
        /// Add a service 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void service_ChildServiceRequested(object sender, ServiceRequestedEventArgs e)
		{			
			//_scheduleTable.Timer.Enabled = false;
			_scheduleTable.RunTaskSync.WaitOne();

			// Init service          
			ServiceInstance service = e.RequestedService;

			if (service.Configuration.IsEnabled)
			{
				// DORON - we might lose this wat the connection to the father.
				// If the service have no rule we add it with TimeScheduled = now.
				if (service.Configuration.SchedulingRules.Count == 0)
				{
					//service.InstanceType = ServiceInstanceType.Normal;
					service.TimeScheduled = DateTime.Now;
					service.ActiveSchedulingRule = null;

					// Remove the service' events and remove it from the relevent service List.                
					service.StateChanged += _stateChangedHandler;
					service.OutcomeReported += _outcomeReportedHandler;
					service.ChildServiceRequested += _childServiceRequestedHandler;
					
					_scheduleTable.AddService(service);
				}
				else
					AddServicesByRules(service);
				
			}
			else
			{
				Log.Write(string.Format("Parent service: {0} with Instance ID : {1} CAN'T activated service: {2} with instance ID: {3} because it is no enable.", service.ParentInstance.Configuration.Name, service.ParentInstance.InstanceID, service.Configuration.Name, service.InstanceID), LogMessageType.Information);
			}

			//_scheduleTable.Timer.Enabled = true;
			_scheduleTable.RunTaskSync.Release();
		}

        /*=========================*/
        #endregion

        #region Private Methods
        /*=========================*/
   
        /// <summary>
        /// Create a service object by the function parameters.
        /// </summary>
        /// <param name="serviceElement"></param>
        /// <param name="serviceInstanceType"></param>
        /// <param name="serviceTimeScheduled"></param>
        /// <returns></returns>
		private ServiceInstance CreateNewService(AccountServiceElement accountServiceElement, 
			DateTime serviceTimeScheduled, 
			SchedulingRuleElement activeSchedulingRule, int accountID)
        {
			//// Create new service list if needed.
			//if (!_scheduleTable.ContainsKey(accountServiceElement.Uses.Element.Name))
			//{
			//    _scheduleTable.Add(accountServiceElement.Uses.Element.Name, new ServiceList(accountServiceElement.Uses.Element));
			//}
			ServiceInstance service;
			try
			{
				service = Service.CreateInstance(accountServiceElement, accountID);
			}
			catch (Exception ex)
			{
				Log.Write(String.Format("Exception occured while performing CreateInstance.", accountServiceElement.Uses.Element != null ? accountServiceElement.Uses.Element.Name : string.Empty), ex);
				return null;
			}

			service.StateChanged += _stateChangedHandler;
            service.OutcomeReported += _outcomeReportedHandler;
            service.ChildServiceRequested += _childServiceRequestedHandler;
            service.ActiveSchedulingRule = activeSchedulingRule;
            service.TimeScheduled = serviceTimeScheduled;
            
			return service;
        }

		/// <summary>
		/// Create a service object by the function parameters.
		/// </summary>
		/// <param name="serviceElement"></param>
		/// <param name="serviceInstanceType"></param>
		/// <param name="serviceTimeScheduled"></param>
		/// <returns></returns>
		private ServiceInstance CreateNewService(EnabledConfigurationElement enabledConfigurationElement,
			DateTime serviceTimeScheduled,
			SchedulingRuleElement activeSchedulingRule, int accountID)
		{
			//// Create new service list if needed.
			//if (!_scheduleTable.ContainsKey(accountServiceElement.Uses.Element.Name))
			//{
			//    _scheduleTable.Add(accountServiceElement.Uses.Element.Name, new ServiceList(accountServiceElement.Uses.Element));
			//}
			ServiceInstance service;
			try
			{
				service = Service.CreateInstance(enabledConfigurationElement, accountID);
			}
			catch (Exception ex)
			{
				Log.Write("Exception occured while performing CreateInstance.",ex);
				return null;
			}

			service.StateChanged += _stateChangedHandler;
			service.OutcomeReported += _outcomeReportedHandler;
			service.ChildServiceRequested += _childServiceRequestedHandler;
			service.ActiveSchedulingRule = activeSchedulingRule;
			service.TimeScheduled = serviceTimeScheduled;

			return service;
		}


        /// <summary>
        /// Add the service to scheduleTable.
        /// </summary>
        /// <remarks>We schedule the table for next day. 
        /// For example on sunday 23:00 we schedule the table for monday.</remarks>
        /// <param name="serviceElement">The service to add</param>
        /// <param name="rule">The specific rule of the service that will add for this service instance.</param>
        /// <param name="childService">If we add a service request this service is the requested child service to schedule.</param>
		// 		private void AddServiceRule(AccountServiceElement accountServiceElement,
		//	SchedulingRuleElement rule, ServiceInstance childService, int accountID)
		private void AddServiceRule(AccountServiceElement accountServiceElement,
			SchedulingRuleElement rule, ServiceInstance childService, int accountID)
        {
            ServiceInstance service;
            DateTime serviceScheduledTime;

            // ExactTimes case 
            if (rule.ExactTimes.Length > 0)
            {
                foreach (TimeSpan hour in rule.ExactTimes)
                {
                    // Convet TimeSpan to datetime.
                    // We schedule the table for next day - we add 1 day.

					if (!rule.NextDay)
					{
						serviceScheduledTime = DateTime.Today;
						serviceScheduledTime = serviceScheduledTime.Add(hour);

						// The service pass is deviation time. 
						if ((rule.MaxDeviation.TotalMilliseconds > 0) &&
							(DateTime.Now - serviceScheduledTime > rule.MaxDeviation))
							continue;
					}
					else
					{
						serviceScheduledTime = DateTime.Today.AddDays(1);
						serviceScheduledTime = serviceScheduledTime.Add(hour);
					}
                    
                    if (childService == null)
						service = CreateNewService(accountServiceElement, serviceScheduledTime, rule, accountID);
                    else
                        service = childService;

                    // Add the service to the schedule table.
					if (service != null)
						_scheduleTable.AddService(service);

                    // Relevent for service request which have rules that were 
                    // suppose to be shceduled eariler.
                    if (serviceScheduledTime < DateTime.Now)
                    {
						
						// TODO: maybe add this log to error file.
                        // TODO: might need more info on the log like service runtime ID
						//Log.Write(String.Format("The service {0} of account {1} can't schedule the service cause its scheduled for eariler time."
						//     , accountServiceElement != null ? accountServiceElement.Uses.Element.Name : childService.Configuration.Name,
						//     accountServiceElement != null ? string.Empty : childService.AccountID.ToString()),
						//     LogMessageType.Warning);

                        continue;
                    }
                    
                }
            }
            else // Frequency case
            {
                // Get last run from the DB for this service.
				DateTime lastServiceRun = GetLastServiceRun
					(accountServiceElement != null ? accountServiceElement.Uses.Element.Name : childService.Configuration.Name);

                // Init time with the end of the next day.
                DateTime nextDayEnd = DateTime.Today.AddDays(2).AddTicks(-1);

                if (lastServiceRun.AddMinutes(rule.Frequency.TotalMinutes) > DateTime.Now)
                    serviceScheduledTime = lastServiceRun.AddMinutes(rule.Frequency.TotalMinutes);
                else
                    serviceScheduledTime = DateTime.Now;

                // We keep to schedule sevices till we pass the current day.
                while (serviceScheduledTime < nextDayEnd)
                {
                    if (childService == null)
						service = CreateNewService(accountServiceElement, serviceScheduledTime, rule, accountID);
                    else
                        service = childService;

                    // Add the service to the schedule table.
					if (service != null)
						_scheduleTable.AddService(service);

                    // Increase time with Frequency value.
                    serviceScheduledTime = serviceScheduledTime.AddMinutes(rule.Frequency.TotalMinutes);
                }
            }
        }

        /*=========================*/
        #endregion

        #region Private Validation Methods
        /*=========================*/

        /// <summary>
        /// Validate ServiceInstance if he is enabled public and have valid referance.
        /// </summary>
        /// <param name="serviceElement">The service to validate.</param>
        /// <returns>If the service valid or not.</returns>
        private bool ValidateService(ServiceElement serviceElement)
        {
            // Check IsEnabled.
            if (!serviceElement.IsEnabled)
            {
                // Write to the log.
                Log.Write(String.Format("The service {0}  isn't enabled, can't schedule the service."
                     , serviceElement.Name), LogMessageType.Warning);

                return false;
            }

            // Check IsPublic.
            if (!serviceElement.IsPublic)
            {
                // Write to the log.
                Log.Write(String.Format("The service {0} isn't public, can't schedule the service."
                     , serviceElement.Name), LogMessageType.Warning);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Validate if the account exist in the DB.
        /// </summary>
        /// <param name="account">The account to validate.</param>
        /// <returns>If the account valid or not.</returns>
        private bool ValidateAccount(AccountElement account)
        {
            // Check if the account exist in the DB, account system (value -1 ) we always run.
            if (account.ID != -1 && !IsAccountExist(account.ID))
            {
                // Write to the log.
                Log.Write(String.Format("The account ID {0} don't exist in the DB, can't schedule his services."
                     , account.ID), LogMessageType.Warning);
				
                return false;
            }

            return true;
        }

		/// <summary>
		/// Return boolean answer if the accountID exist in the DB.
		/// </summary>
		/// <param name="accountID">The account that need to be check.</param>
		/// <returns></returns>
		public static bool IsAccountExist(int accountID)
		{
			//TODO: check if the account exist in the DB.			
			return true;
		}

		/// <summary>
		/// Return the last time this service have run. 
		/// Used for cacluating the scheduling of the service in frequency option.
		/// </summary>
		/// <param name="serviceName">The service that need to be check.</param>
		/// <param name="accountID">The account that the service belong to.</param>
		/// <returns>The last Time the service has run.</returns>
		public static DateTime GetLastServiceRun(string serviceName)
		{
			return DateTime.Today;
		}

        /*=========================*/
        #endregion
    }

}

   
