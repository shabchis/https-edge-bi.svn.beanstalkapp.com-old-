using System;
using System.Collections.Generic;
using Easynet.Edge.Core;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Scheduling;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;

namespace Easynet.Edge.Services.ScheduleManagement
{
	/// <summary>
	/// The main class that manages the service scheduler.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>01/07/2008</creation_date>
	
	public class ScheduleManagerService: Service, IScheduleManager
	{
		//TODO: Develop in later version - add a code that catch an event of changing the settings.

		#region Members
		/*=========================*/

		private ScheduleBuilder _builder = new ScheduleBuilder();	
		//private DateTime _buildScheduleTime;
		private bool _debugMode = false;

		/*=========================*/
		#endregion

		//#region Events
		///*=========================*/
		//private EventHandler _propertyChangedHandler;
		///*=========================*/
		//#endregion

		//// TODO: check if this is the right signutrue for settings PropertyChanged
		//void manager_PropertyChanged(object sender, SettingChangingEventArgs e)
		//{
		//    // TODO: call new funcion in builder to update the schedue table.
		//    //tbStatus.Text = e.SettingName + ": " + e.NewValue;
		//}

		#region Constructor
		/*=========================*/

		/// <summary>
		/// Constructor - Init the log and time and check if there is data to 
		/// recover from the DB.
		/// </summary>
		protected ScheduleManagerService()
		{
			_debugMode = Convert.ToBoolean(AppSettings.GetAbsolute("DebugMode"));

			// TODO: check recovery feature.
			/*
            // Check in the DB if there are any service lists with services that didn't run.           
            using (ConnectionKey key = DataManager.Current.OpenConnection())
            {
                SqlCommand selectCommand = DataManager.CreateCommand("select distinct ServiceName from JobManager_ServiceList");
                SqlDataReader selectResults = selectCommand.ExecuteReader();

                // There are service lists to recover.
                if (selectResults.HasRows)
                {
                    ServicesSection servicesSection;
                    try
                    {
						servicesSection = (ServicesSection)ConfigurationManager.GetSection("edge2.services");
                    }
                    catch (Exception ex)
                    {
                        Log.Write("Error get services section from configuration file. Can't recover services from the DB.", ex, LogMessageType.Error);
                        return;
                    }

                    while (selectResults.Read())
                    {
                        // Get serviceElement from configuration by service name.
                        string serviceListName = selectResults[0].ToString();
                        ServiceElement serviceElement = servicesSection.Services[serviceListName];

                        // check with DORON - create new servicelist instead of binding.
                        // Create new service and add it to the schedule table.
						_builder.AddServicesByRules(serviceElement, -1);
                    }
                }
            }
			*/

		}

		/*=========================*/
		#endregion
	
		#region Service override Methods
		/*=========================*/

		/// <summary>
		/// Overload service function
		/// </summary>
		/// <returns></returns>
		protected override ServiceOutcome DoWork()
		{
			//_builder.FirstRun = true;
			_builder.BuildScheduling(Instance.Configuration.Name,Instance.InstanceID);
			return ServiceOutcome.Unspecified;
		}

		protected override void OnEnded(ServiceOutcome serviceOutcome)
		{
			if (serviceOutcome == ServiceOutcome.Aborted)
				_builder.AbortAllservices();
		}

		/*=========================*/
		#endregion

		#region IScheduleManager Members
		/*=========================*/	

		/// <summary>
		/// Called by ScheduleBuildingCueService and possibly GUI
		/// </summary>
		public void BuildSchedule()
		{
			// YANIV: add check to make sure this can be performed now; if not -
			// throw exception with a message
			//_builder.FirstRun = false;
			_builder.BuildScheduling(string.Empty, -1);
		}

		public bool AddToSchedule(string serviceName, int accountID, DateTime targetTime, SettingsCollection options)
		{
			// YANIV:
			// if cannot perform at requested time, or can only perform after max deviation expires - 
			// throw exception with a message

			ActiveServiceElement startupElement = null;
			if (accountID < 0)
			{
				AccountServiceElement elem = ServicesConfiguration.SystemAccount.Services[serviceName];
				if (elem != null)
					startupElement = new ActiveServiceElement(elem);
				else
					startupElement = new ActiveServiceElement(ServicesConfiguration.Services[serviceName]);
			}
			else
			{
				AccountElement account = ServicesConfiguration.Accounts.GetAccount(accountID);
				if (account == null)
				{
					// EXCEPTION:
					return false;
					//throw new ArgumentException(
					//    String.Format("Account with ID {0} does not exist.", accountID),
					//    "accountID");
				}

				AccountServiceElement accountService = account.Services[serviceName];
				if (accountService == null)
				{
					// EXCEPTION
					// throw new ArgumentException(String.Format("Service {0} is not defined for account {1}", serviceName, account.Name), "serviceName");
					return false;
					// Log.Write(String.Format("Service {0} is not defined for account {1}", serviceName, account.Name), LogMessageType.Warning);
				}
				else
				{
					startupElement = new ActiveServiceElement(accountService);
				}
			}


			// Couldn't find exception
			if (startupElement == null)
			{
				// EXCEPTION:
				return false;
				//throw new ArgumentException(
				//    String.Format("The requested service \"{0}\" could not be found.", serviceName),
				//    "serviceName");
			}

			// Merge the options
			if (options != null)
			{
				foreach(KeyValuePair<string,string> pair in options)
					startupElement.Options[pair.Key] = pair.Value;
			}

			// Add the manual request.
			if (targetTime < DateTime.Now)
				_builder.AddManualRequest(startupElement, accountID);
			else
				_builder.AddManualRequest(startupElement, targetTime, accountID);			

			return true;
		}

		/*=========================*/	
		#endregion
	}
}
