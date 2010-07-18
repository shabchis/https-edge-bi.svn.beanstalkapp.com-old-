using System;
using System.Xml;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Scheduling;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using Easynet.Edge.Core;
using Easynet.Edge.Services.DataRetrieval;
using System.Text.RegularExpressions;
using System.Web;
using Easynet.Edge.Services.DataRetrieval.DataReader;
using System.ServiceModel;

namespace Easynet.Edge.Services.DataRetrieval
{
	[ServiceContract]
	public interface IOrganicServiceDelegator
	{
		[OperationContract]
		void RunOrganicProfile(int accountID, int profileID);
	}



	// To configure this service:
	//	1. add it to the schedule as an always on service like the FileSystemWatcher with the name OrganicServiceDelegator
	//  2. add this to the app.config under <system.serviceModel> under <services>:
	//		<service name="Easynet.Edge.Services.DataRetrieval.OrganicServiceDelegatorService" behaviorConfiguration="behavior">
	//			<endpoint binding="wsHttpBinding" bindingConfiguration="edgeServiceWebBinding" contract="Easynet.Edge.Services.DataRetrieval.IOrganicServiceDelegator" address="http://localhost:27334/v2.1/OrganicServiceDelegator" />
	//		</service>


	public class OrganicServiceDelegatorService : Service, IOrganicServiceDelegator
	{
		protected override ServiceOutcome DoWork()
		{
			return ServiceOutcome.Unspecified;	
		}

		public void RunOrganicProfile(int accountID, int profileID)
		{
			List<string> serviceNames = new List<string>();

			
			using (DataManager.Current.OpenConnection())
			{
				// TODO: get names of services as single-column result arrray
                SqlCommand cmd = DataManager.CreateCommand("select distinct SE.ServiceName from dbo.User_GUI_SerpProfileSearchEngine PSE inner join Constant_SearchEngine SE on PSE.Search_Engine_ID = SE.Search_Engine_ID where account_id = @AccountID:Int and Profile_ID = @ProfileID:Int");
                cmd.Parameters["@AccountID"].Value = accountID;
                cmd.Parameters["@ProfileID"].Value = profileID;

				using (SqlDataReader reader = cmd.ExecuteReader())
				{
					while (reader.Read())
						serviceNames.Add(reader[0] as string);
				}
			}

			SettingsCollection settings = new SettingsCollection();
			settings.Add("ProfileID", profileID.ToString());

			using (ServiceClient<IScheduleManager> client = new ServiceClient<IScheduleManager>())
			{
				foreach (string serviceName in serviceNames)
				{
					// Request the manager to build the schedule
					client.Service.AddToSchedule(serviceName, accountID, DateTime.Now, settings);
				}
			}
		}
	}
}


