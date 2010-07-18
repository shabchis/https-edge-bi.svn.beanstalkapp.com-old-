using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Timers;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using System.Threading;
using System.Data.SqlClient;
using System.Data;

namespace Easynet.Edge.Services.ScheduleManagement
{
	/// <summary>
	/// This class contain all the service lists and activate them when needed.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>01/07/2008</creation_date>
	[Serializable]
	
	public class ScheduleTable : Dictionary<string, ServiceList>
	{
		#region Members
		/*=========================*/

		private System.Timers.Timer _timer = new System.Timers.Timer();
		private TimeSpan _globalMaxDeviationTime = new TimeSpan(2, 0, 0, 0); // 2 days
		private bool _systemServicesRunning = false;
		//private Dictionary<int, int> _accountList = new Dictionary<int, int>();
		private static Semaphore _runTaskSync;
		private DateTime _keepAliveLastTime;
		private int _keepAliveCheckTime; // in Minutes
		private string _scheduleServiceName = string.Empty;
		private long _serviceInstanceID = -1;

		public long ServiceInstanceID
		{
			get { return _serviceInstanceID; }
			set { _serviceInstanceID = value; }
		}

		public string ScheduleServiceName
		{
			get { return _scheduleServiceName; }
			set { _scheduleServiceName = value; }
		}

		public Semaphore RunTaskSync
		{
			get { return ScheduleTable._runTaskSync; }
			set { ScheduleTable._runTaskSync = value; }
		}

		/*=========================*/
		#endregion

		#region Consts
		/*=========================*/

		private int SystemAccount = -1;

		/*=========================*/
		#endregion

		#region Constructor
		/*=========================*/

		/// <summary>
		/// Constructor - init the timer and the scheduleTable.
		/// </summary>
		public ScheduleTable()
		{
			// Try to get the AtomicTime from AppSettings, otherwise use 1 seconds
			TimeSpan atomicTime = TimeSpan.FromSeconds(1);
			string rawValue = AppSettings.Get(this, "AtomicTime", false);
			if (rawValue != null)
				TimeSpan.TryParse(rawValue, out atomicTime);
			
			// Hook up the Elapsed event for the timer.
			_timer.Elapsed += new ElapsedEventHandler(RunTasks);

			_timer.Interval = atomicTime.TotalMilliseconds;
			_timer.Enabled = true;
		
			_runTaskSync = new Semaphore(1, 1);

			_keepAliveLastTime = DateTime.Now;
			_keepAliveCheckTime = 5; // minutes
			rawValue = AppSettings.Get(this, "KeepAliveCheckTime", false);

			try
			{
				if (rawValue != null)
					int.TryParse(rawValue, out _keepAliveCheckTime);
			}
			catch
			{
			}
		}

		/*=========================*/
		#endregion

		#region Access Methods
		/*=========================*/

		//public void AddAccount(int accountID, int maxInstancesAccount)
		//{
		//    if (!_accountList.ContainsKey(accountID))
		//    {
		//        _accountList.Add(accountID, maxInstancesAccount);
		//    }
		//}

		public System.Timers.Timer Timer
		{
			get
			{
				return _timer;
			}
			set
			{
				_timer = value;
			}
		}

		/*=========================*/
		#endregion

		#region Public Methods
		/*=========================*/

		/// <summary>
		/// Add the service to the relevent service List and if the list don't exist
		/// we creates it.
		/// </summary>
		/// <param name="service">ServiceInstance to add to queue.</param>
		public void AddService(ServiceInstance service)
		{
			// Create new service list if needed.
			if (!this.ContainsKey(service.Configuration.Name))
				this.Add(service.Configuration.Name, new ServiceList(service.Configuration));
			if (!this[service.Configuration.Name].Contains(service))
				this[service.Configuration.Name].Add(service);//, false);
			else
			{
				// dune: log me
			}
			
		}

		public void AbortAllservices()
		{
			_timer.Enabled = false;

			// Loop on service lists in schedule table.
			foreach (string serviceName in this.Keys.ToArray())
			{
				// Loop on services in current service list.
				foreach (ServiceInstance service in this[serviceName].ToArray<ServiceInstance>())
				{
					if (service.ParentInstance == null)
						service.Abort();
				}
			}
			//_timer.Enabled = false;
		}

		/*=========================*/
		#endregion

		#region Private members
		/*=========================*/
		
		/// <summary>
		/// This function si called by the timer event.
		/// Check all the services list and activate the relevent ones.
		/// </summary>
		private void RunTasks(object source, ElapsedEventArgs e)
		{
			// TODO: Do not allow max isntance > 1 for service 
			// that have public url. (consult Doron)
			bool allSystemServicesRun = true;

			_runTaskSync.WaitOne();

			WriteKeepAlive();

			//_timer.Enabled = false;
			// Loop on service lists in schedule table.
			foreach (string serviceName in this.Keys.ToArray())
			{
				// Sort the list by Time Scheduled.
				TimeScheduledComparer timeScheduledComparer = new TimeScheduledComparer();
				this[serviceName].Sort(timeScheduledComparer);
				int serviceSlotsCounter = 0;

				/// Loop on services in current service list.
				foreach (ServiceInstance service in this[serviceName].ToArray<ServiceInstance>())
				{
					// We activate only system service on the first run.
					// If we didn't run system services yet, we skip every
					// non system service we encouter.
					if (!_systemServicesRunning && service.AccountID != SystemAccount)
						continue;
				
					//ServiceStateComparer serviceStateComparer = new ServiceStateComparer();
					//this[serviceName].Sort(serviceStateComparer);			

					// Check if the system service run
					if (service.AccountID == SystemAccount &&
						!_systemServicesRunning && 
						service.State != ServiceState.Running && 
						service.State != ServiceState.Waiting &&
						service.TimeScheduled <= DateTime.Now)
					{
						allSystemServicesRun = false;
					}

					// Unlimited service instances.
					if (this[serviceName].ServiceMaxSlots == 0)
					{
						// All the remaining service should be run later.
						if (service.TimeScheduled > DateTime.Now)
							break;						

						UnlimitedInstancesServiceHandler(service);
					}
					else // Limited service instances.
						LimitedInstancesServiceHandler(service, ref serviceSlotsCounter);
					
				}

				// Delete empty service lists.
				this.RemoveEmptyList();
			}


			if (allSystemServicesRun && this.Keys.Count > 0)
				_systemServicesRunning = true;
			
			//_timer.Enabled = true;
			_runTaskSync.Release();
		}

		private void WriteKeepAlive()
		{
			// Write each keepAliveCheckTime  minutes to the DB the current time.
			if (DateTime.Now.AddMinutes(-1 * _keepAliveCheckTime) > _keepAliveLastTime)
			{
				using (DataManager.Current.OpenConnection())
				{
					try
					{
						// Check if there is a row with the service name in the DB.
						SqlCommand selectCommand = DataManager.CreateCommand(@"
							select KeepAlive_Time from source.dbo.KeepAlive
							WHERE InstanceID = @InstanceID:BigInt",
							CommandType.Text);

						selectCommand.CommandTimeout = 120;
						selectCommand.Parameters["@InstanceID"].Value = _serviceInstanceID;
						object result = selectCommand.ExecuteScalar();

						if (result == null)
						{
							// Initalize update command.
							SqlCommand insertCommand = DataManager.CreateCommand(@"
								insert INTO source.dbo.KeepAlive
									   (KeepAlive_Time,ServiceName,InstanceID)
								Values (@Day_Code:DateTime,@ServiceName:varchar,@InstanceID:BigInt)",						
								CommandType.Text);

							insertCommand.CommandTimeout = 120;
							insertCommand.Parameters["@Day_Code"].Value = DateTime.Now;
							insertCommand.Parameters["@ServiceName"].Value = _scheduleServiceName;
							insertCommand.Parameters["@InstanceID"].Value = _serviceInstanceID;
							insertCommand.ExecuteNonQuery();
						}
						else
						{
							// Initalize update command.
							SqlCommand updateCommand = DataManager.CreateCommand(@"
								UPDATE source.dbo.KeepAlive
								SET KeepAlive_Time = @Day_Code:DateTime
								WHERE InstanceID = @InstanceID:BigInt",
								CommandType.Text);

							updateCommand.CommandTimeout = 120;
							updateCommand.Parameters["@Day_Code"].Value = DateTime.Now;
							updateCommand.Parameters["@InstanceID"].Value = _serviceInstanceID;
							updateCommand.ExecuteNonQuery();
						}
						_keepAliveLastTime = DateTime.Now;
					}
					catch (Exception ex)
					{
						Log.Write("Failed to write keepalive time to DB.", ex);
					}
				}
			}
		}

		/// <summary>
		/// Scan all the service list and remove the empty ones. not used yet.
		/// </summary>
		private void RemoveEmptyList()
		{
			// Loop on all the service lists in schedule table.
			foreach (string serviceName in this.Keys.ToArray())
			{
				// Check if the service list is empty.
				if (serviceName == null || this[serviceName].Count == 0)
				{
					this.Remove(serviceName);
				}
			}
		}

		/// <summary>
		/// Handle a service in a Unlimited Instances list.
		/// </summary>
		/// <param name="service">The service to handle.</param>
		private void UnlimitedInstancesServiceHandler(ServiceInstance service)
		{
			// If the service already run we move to the next service in the list.
			if (service.State != ServiceState.Uninitialized) 				
				return;

			// The service has AlwaysOn Rule.
			if (service.ActiveSchedulingRule != null &&
				service.ActiveSchedulingRule.CalendarUnit ==
				CalendarUnit.AlwaysOn)
			{
				RunService(service);
			}
			// The service pass is deviation time (his local time or global time).
			else if (
						(service.ActiveSchedulingRule != null) &&				
						(
							(service.ActiveSchedulingRule.MaxDeviation.TotalMilliseconds > 0) &&
							(
								(DateTime.Now - service.TimeScheduled > _globalMaxDeviationTime) ||
								(DateTime.Now - service.TimeScheduled > service.ActiveSchedulingRule.MaxDeviation) 
							)
						)
					)
			{
				ServicePassItsMaxDeviation(this[service.Configuration.Name], service);
			}
			else // Activate the service.
				RunService(service);
		}


		/// <summary>
		/// Hanlde all necessary operations to run the service.
		/// </summary>
		/// <param name="service">The service to run.</param>
		private void RunService(ServiceInstance service)
		{
			service.StateChanged += 
				delegate(object sender, ServiceStateChangedEventArgs e)
				{
					if (service.State == ServiceState.Ready)
						service.Start();
				};

			try
			{
				service.Initialize();
			}
			catch(Exception ex)
			{
				Log.Write(String.Format("Exception occured while performing RunService.", service.Configuration.Name), ex);
			}
		}

		/// <summary>
		/// Handle a service in a Limited Instances list.
		/// When there are no more free service slots we dont try to 
		/// activate services in the list and only remove them if they pass
		/// there max deviation time. 
		/// </summary>
		/// <param name="service">The service to handle.</param>
		/// <param name="serviceSlotsCounter">number of slots alreday used in the list.</param>
		/// activate services in the list and only remove them if they pass
		/// there max deviation time.</param>
		private void LimitedInstancesServiceHandler(ServiceInstance service,
			ref int serviceSlotsCounter)
		{
			// If the service already run we move to the next service in the list.
			//if (service.State != ServiceState.Ready)
			//if ((service.State == ServiceState.Running) || (service.State == ServiceState.Waiting))
			if (service.State != ServiceState.Uninitialized)
			{
				++serviceSlotsCounter;
				return;
			}

			// The service has AlwaysOn Rule.
			if (service.ActiveSchedulingRule != null &&
				service.ActiveSchedulingRule.CalendarUnit ==
				CalendarUnit.AlwaysOn)
			{
				RunLimitedService(service, ref serviceSlotsCounter);
			}
			// The service pass his deviation time (his local time or global time).
			else if (
						(service.ActiveSchedulingRule != null) &&				
						(
							(service.ActiveSchedulingRule.MaxDeviation.TotalSeconds > 0) &&
							(
								(DateTime.Now - service.TimeScheduled > _globalMaxDeviationTime) ||
								(DateTime.Now - service.TimeScheduled > service.ActiveSchedulingRule.MaxDeviation) 
							)
						)
					)
			{
				ServicePassItsMaxDeviation(this[service.Configuration.Name], service);				
			}
			// Check if we need to run the service: if it TimeScheduled < now < MaxDeviation
			// and also have empty service slot) 
			else if (serviceSlotsCounter < this[service.Configuration.Name].ServiceMaxSlots &&
					 service.TimeScheduled <= DateTime.Now)
			{
				RunLimitedService(service, ref serviceSlotsCounter);
			}
		}

		private void RunLimitedService(ServiceInstance service, ref int serviceSlotsCounter)
		{
			// Check if there is empty slot to run the service.
			if (serviceSlotsCounter < this[service.Configuration.Name].ServiceMaxSlots)
			{
				// Activate the service
				RunService(service);
				++serviceSlotsCounter;
			}
			//else // Can't activate another service. 
			//{
			//    // Write to log an information log.
			//    Log.Write(String.Format("The service {0} with RuntimeID {1} can't run now because there are no free instance to run the service. The service will try to activate in the next cycle."
			//        , service.Configuration.Name, service.InstanceID), LogMessageType.Information);
			//}
		}

		/// <summary>
		/// Write a warning to the log that the service pass its max Deviation
		/// time and remove it from the list.
		/// </summary>
		/// <param name="serviceList">The service list the sevice belong to.</param>
		/// <param name="service">The service that Pass Its Max Deviation time.</param>
		private void ServicePassItsMaxDeviation(ServiceList serviceList, ServiceInstance service)
		{		
			// Write to log the warning.
			Log.Write(String.Format("The service {0} for account {1} can't run because it has passed its MaxDeviation.", service, service.AccountID), LogMessageType.Warning);

			// Remove service from queue.
			serviceList.Remove(service);

			//if (service.InstanceType == ServiceInstanceType.Normal)
			//{
			//    // Inform parent service that the service could not be scheduled.    
			//    if (service.ParentInstance != null)
			//    {
			//        // Raise the event that will tune the parent.
			//        service.Outcome = ServiceOutcome.CouldNotBeScheduled;
			//    }
			//}
		}

		/*=========================*/
		#endregion

	
	}
}
