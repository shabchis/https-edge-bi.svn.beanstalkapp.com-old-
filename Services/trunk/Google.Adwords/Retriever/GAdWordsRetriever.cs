using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Services.DataRetrieval;
using Easynet.Edge.Services.DataRetrieval.Retriever;
using Easynet.Edge.Services.DataRetrieval.Configuration;
using Easynet.Edge.Services.Google.Adwords.GAdWordsReportServiceV13;
using System.IO;
using System.Data;

namespace Easynet.Edge.Services.Google.Adwords
{
	/// <summary>
	/// This service get creative report from Google AdWords and insert it to the DB.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>21/07/2008</creation_date>	
    public class GAdWordsRetrieverService : RetrieverService
    {
        #region Consts
        /*=========================*/
        /// <summary>
        /// Google access Useragent (can verified by MCC)
        /// </summary>
        private const string UserAgent = "Easynet(seperia) -- C# get clients information";
        /// <summary>
        /// Google access Developer Token (can verified by MCC)
        /// </summary>
        private const string Token = "DJ-sOPT568XvXU_DgwymvA";
        /// <summary>
		/// Google access Application Token (can verified by MCC)
        /// </summary>
        private const string AppToken = "8Uph5HQbsQ4rcrEdmPVFKA";

		/// <summary>
		/// Aggergation types
		/// </summary>
		private string[] DailyAggergation = new String[] {"Daily"};
		private string[] ContetAggergation = new String[] { "AdGroup", "Url", "Daily" };
		private string[] SummaryAggergation = new String[] {"Summary"};
		private string _googleServiceType;
		private const string GoogleContentServiceType = "Google.Adwords.Content";
		private const string GoogleCreativeServiceType = "Google.Adwords.Creative";

        /*=========================*/
        #endregion      

        #region Members
        /*=========================*/

        private string _googleAdWordsDirectory = string.Empty;
		private ArrayList _errorDates = new ArrayList();

        /*=========================*/
        #endregion                

        #region Constructor
        /*=========================*/

		private GAdWordsRetrieverService()
        {          
            // Load application settings.
			System.Net.ServicePointManager.Expect100Continue = false;
        }

        /*=========================*/
        #endregion    

        #region Private Methods
        /*=========================*/

		/// <summary>
		/// Create Google report, Initalize his dats by the Aggergation type.
		/// Get tha data from Google AdWords and insert it to the DB.
		/// </summary>
		/// <param name="fullService">The service that contain the google services 
		/// that will be used to fetch the data from Gogle AdWords.</param>
		/// <param name="date">The date to get the data for daily aggergationType.</param>
		/// <param name="aggergationType">daily or summary</param>
		private bool ExecuteJob(FullService fullService)
		{
			DefinedReportJob googleJob;
			DateTime retrievedDay = DateTime.Now.AddDays(-1);

			// Initalize the report parameters.
			if (Instance.Configuration.Options["ReportType"] == null || Instance.Configuration.Options["ReportType"] != "Content")
			{
				TargetSubDir = @"Google\Creative\";
				ErrorFilePath = @"Google\Creative\Errors";
				_googleServiceType = GoogleCreativeServiceType;
				googleJob = CreateReport("GoogleCreativeReport", DailyAggergation);

			}
			else // content report
			{
				_googleServiceType = GoogleContentServiceType;
				TargetSubDir = @"Google\Content\";
				ErrorFilePath = @"Google\Content\Errors";
				googleJob = CreateReport("GoogleContentReport", ContetAggergation);
				retrievedDay = retrievedDay.AddDays(-3);
				string offsetValue = GetConfigurationOptionsField("OffsetDays");
				if (offsetValue != string.Empty)
				{
					int offset;
					if (Int32.TryParse(offsetValue, out offset) && offset < 0)
						retrievedDay = DateTime.Now.AddDays(offset);				
				}
			}
			ArrayList dates = new ArrayList();

			if (CheckRangeDate(ref dates))
			{
				int i;

				if (dates == null || dates.Count == 0)
					return false;
				for ( i = 0; i < dates.Count && i < _maxInstancesReRuns ; ++i)
					RunReport(fullService, googleJob, (DateTime)dates[i], false);

				// Write to the log all the dates that din;t eun because max instances ReRuns.
				if (i < dates.Count)
				{
					string errorMsg = "Can't write the following dates: ";
					for (int j = i; j < dates.Count; ++j)
						errorMsg += ((DateTime)dates[j]).ToShortDateString() + ", ";

					errorMsg += " because the service exceed the max instances ReRuns."; 
					Log.Write(errorMsg,  LogMessageType.Error);
				}

				if (_errorDates.Count > 0)
				{
					for (i = 0; i < _errorDates.Count; ++i)
						RunReport(fullService, googleJob, (DateTime)dates[i], true);
				}
			}
			else
			{
				// Check if we need to get manual date.
				//CheckManualDate(ref retrievedDay);
				RunReport(fullService, googleJob, retrievedDay, false);
			}

			return true;
			#region Obsolete - Time Aggergation

			// Initalize dates acording to aggergation type.
			//if (aggergationType == DailyAggergation) // daily Aggergation
			//{
			//    googleJob.startDay = retrievedDay;
			//    googleJob.endDay = retrievedDay;
			//}
			//else // month Aggergation
			//{
			//    // first day of last month.
			//    googleJob.startDay = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month, 1);
			//    // last day of last month.
			//    googleJob.endDay = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month,
			//        DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month));
			//}

			#endregion		
		}

		private void RunReport(FullService fullService, DefinedReportJob googleJob, DateTime retrievedDay, bool errorRun)
		{
			Log.Write(string.Format("Run retriever for service {2}, for account {0}, for date {1}.", Instance.AccountID.ToString(), retrievedDay.ToShortDateString(), Instance.ParentInstance.Configuration.Name), LogMessageType.Information);

			string errorMsg = string.Empty;
			googleJob.startDay = retrievedDay;
			googleJob.endDay = retrievedDay;

			// Fetch the report from AdWords, 
			if (!Retrieve(fullService, googleJob, retrievedDay, ref errorMsg) && !errorRun)
				_errorDates.Add(retrievedDay);
			else if (errorRun)
				Log.Write(string.Format("Can't retreieve date {0}, exception mesaage: {1}", retrievedDay, errorMsg), LogMessageType.Error);
		}

		/// <summary>
		/// Return if need to retrieve data for last month and in which format.
		/// </summary>
		/// <remarks>
		/// Check if we need to retrieve data for montly aggregation type.
		/// In case we don't have any AdWords data for last month we get
		/// the data by summary aggregation for the all month.
		/// If we have data in daily aggregation for last month, we will
		/// initalize daysToRetrieve list with all the days 
		/// that need to be reteirve.
		/// </remarks>
		/// <param name="daysToRetrieve">A list that contain all days that 
		/// need to be retrieve if we use daily aggregation.</param>
		/// <param name="summaryAggregation">Aggregation type.
		/// The default is summary report.</param>
		/// <returns>If we alreday have data in summary aggregation 
		/// for last month, we return false. otherwise return true.</returns>
		private bool NeedToReteriveData(List<DateTime> daysToRetrieve, out bool summaryAggregation)
		{
			// Default value for summery report.
			summaryAggregation = true;

			SqlDataReader selectResults = null;

			using (DataManager.Current.OpenConnection())
			{
				// Initalize Select Command
				SqlCommand selectCommand = InitalizeSelectCommandCheckDates();
				// Init insertCommand with the data manger connection 
				DataManager.ApplyConnection(selectCommand);
				try
				{
					// Execute select command.
					selectResults = selectCommand.ExecuteReader();
				}
				catch (Exception ex)
				{
					Log.Write("Error get AdWords data from table Paid_API_AllColumns in edge_oltp DB.", ex);
					return false;
				}
				
				// If we don't have any data for previous month we need
				// to get the data for it.
				if (!selectResults.HasRows)				
					return true;
				

				// If we have data we check if we need to retrieve 
				// data and in which format.
				return CheckDatesFromDB(daysToRetrieve, selectResults,ref summaryAggregation);				
			}
		}

		/// <summary>
		/// Return if need to retrieve data for last month in daily Aggrgation.
		/// </summary>
		/// <remarks>
		/// If we have date in monthly aggregation (date format: YYYYMM00)
		/// we don't need to reterive data for this month.
		/// If we have data in daily aggregation for last month, we will
		/// initalize daysToRetrieve list with all the days 
		/// that need to be reteirve.
		/// </remarks>
		/// <param name="daysToRetrieve">A list that contain all days that 
		/// need to be retrieve if we use daily aggregation.</param>
		/// <param name="selectResults">results of the select query that 
		/// contain all the dates in the DB for last month.</param>
		/// <param name="summaryAggregation">Aggregation type.
		/// The default is summary report.</param>
		/// <returns></returns>
		private static bool CheckDatesFromDB(List<DateTime> daysToRetrieve, SqlDataReader selectResults, ref bool summeryReport)
		{
			selectResults.Read();
			string existDate = selectResults["Day_Code"].ToString();
			
			// If the date is in format YYYYMM00 - that means
			// we alreday fetch data for this month and we don't need 
			// to fetch it again.
			if (existDate[6] == '0' && existDate[7] == '0')
			{
				return false;
			}
			else
			{
				int day;

				// Add all days of last month to the daysToRetrieve list.
				int daysInLastMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month);
				for (day = 1; day <= daysInLastMonth; ++day)
				{
					daysToRetrieve.Add(new DateTime(DateTime.Now.Year,
							DateTime.Now.AddMonths(-1).Month, day));
				}

				// Remove all the days that exist in the DB from the list. 
				do
				{
					existDate = selectResults["Day_Code"].ToString();
					// Convert day
					if (!int.TryParse(existDate.Substring(6, 2), out day))					
						continue;					

					// remove the day from the list.
					daysToRetrieve.Remove(new DateTime(DateTime.Now.Year,
						DateTime.Now.AddMonths(-1).Month, day));

				} while (selectResults.Read());

				summeryReport = false;
				return true;
			}
		}

		/// <summary>
		/// Initalize Select Command that check if there is any AdWords data 
		/// from last month for the accountID.
		/// </summary>
		/// <param name="manualConnection">Connection string for the command.</param>
		/// <returns>Initalize select command</returns>
		private SqlCommand InitalizeSelectCommandCheckDates()
		{
			// Initalize the last day of the month that come before the month we want to get.
			// For example if we want to get data for may we init lastDay with 20080431.
			DateTime lastDay = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-2).Month,
				DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.AddMonths(-2).Month));

			// Initalize select command to table Paid_API_AllColumns to check 
			// if there is any data in DB for the Previous month.
			SqlCommand selectCommand = DataManager.CreateCommand(@"
                    select distinct Day_Code
                        from Paid_API_AllColumns 
                        where Account_ID = @accountID:Int and
						Day_Code > @dayCode:Int
						order by Day_Code");

			// Initalize connections paramters.
			//selectCommand.Connection = manualConnection;
			//selectCommand.CommandTimeout = 600; // 10 minutes

			// Initalize select command parameters.
			selectCommand.Parameters["@accountID"].Value = Instance.AccountID;
			selectCommand.Parameters["@dayCode"].Value = GetDayCode(lastDay);

			return selectCommand;
		}

		/// <summary>
		/// Get the data from google AdWords for goolge job.
		/// and save the data into xml file and save a link to the file in the DB.
		/// </summary>
		/// <param name="service"></param>
		/// <param name="googleJob">The job to get his data fro AdWords.</param>
		/// <returns>The name of the xml file that contain the data from AdWords.</returns>
		private bool Retrieve(FullService fullService, DefinedReportJob googleJob, DateTime retrievedDay, ref string errorMsg)
        {			
			int numOfRetries = 0;
			bool reportSaved = false;
			
			while (!reportSaved)
			{
				try
				{
					reportSaved = SaveReport(fullService, googleJob, ref retrievedDay);
				}
				catch (Exception ex)
				{
					if (numOfRetries >= MaxRetries)	// too many retries, bail out					
					{
						errorMsg = ex.Message.ToString();
						return false;
					}
										
					++numOfRetries;
				}
			}
			return true;
        }

		private bool SaveReport(FullService fullService, DefinedReportJob googleJob, ref DateTime retrievedDay)
		{
			long ID;
			string fileName = string.Empty;
			// Get Data from Google AdWords
			ID = fullService.ScheduleReportJob(googleJob, Instance.AccountID);

			// Invaild report.
			if (ID == 0)
				throw new Exception("Can't get data from Google AdWords. return ID from google = 0");		

			string url = fullService.GetReportDownloadUrl(ID, Instance.AccountID);

			if (url == "failed")
				throw new Exception("Can't get data from Google AdWords. return Url from google = 'failed'.");			

			fileName = WriteResultToFile(url, retrievedDay, googleJob.name, true);

			return SaveFilePathToDB(_googleServiceType, fileName, retrievedDay, _adwordsFile);
		}


	

        /// <summary>
        /// New function for google adwords V13 that also use the app.config.
        /// The function get the report name and fetch the proper report type 
        /// section from app.config with his info and his columns. 
        /// </summary>
        /// <param name="type">The report name that exist in the app.config</param>
        /// <returns>A report with the relevent parameters</returns>
		private DefinedReportJob CreateReport(string name, string[] aggregationTypes)
        {
            Hashtable fields = new Hashtable();
            DefinedReportJob job = new DefinedReportJob();

            try
            {
                ReportTypesElementSection rtes = (ReportTypesElementSection)ConfigurationManager.GetSection("ReportTypes");
		
				// Scan all the reports types till we find the chosen report.
				foreach (ReportTypesElement rte in rtes.Fields)
				{
					if (rte.Enabled && rte.Name == name)
					{
						// Load the proper report paramters by the report name.
						FieldElementSection fes = (FieldElementSection)ConfigurationManager.GetSection(rte.Name);

						// Initalize the hash table with the fields of the the report.
						foreach (FieldElement fe in fes.Fields)
						{
							if (fe.Enabled)
								fields.Add(fe.Key, fe.Key);
							
						}

						// Add the fields report to and string array useing IDictionaryEnumerator
						string[] columns = new string[fields.Count];
						int count = 0;
						IDictionaryEnumerator dic = fields.GetEnumerator();
						while (dic.MoveNext())
						{
							columns[count] = dic.Value.ToString();
							count++;
						}

						// Initalize the report with his info.
						job.selectedReportType = rte.Type;
						job.name = rte.Name;
						//job.aggregationTypes = new String[] { rte.Aggregation };
						job.aggregationTypes = aggregationTypes;
						job.includeZeroImpression = true;
						job.includeZeroImpressionSpecified = true;
						job.selectedColumns = columns;

						break;
					}					
				}
            }
            catch (Exception ex)
            {
				Log.Write("Error get configuration Data for the report.", ex);
            }
            return job;
        }

        /// <summary>
        /// Search account settings by account id and retrun 
        /// the access data to this account.
        /// </summary>
        /// <returns>account setings</returns>
		// private bool GetAccountAccessData(out AdWordsAccess adWordsAccess)
		private void GetAccountAccessData(out string accountEmail)
        {
            //adWordsAccess = new AdWordsAccess(string.Empty, string.Empty, string.Empty);
			accountEmail = string.Empty;
            SqlDataReader selectResults;

			using (DataManager.Current.OpenConnection())
			{
                // Initalize select command to table User_GUI_SettingsAdwords to fetch 
                // account access data by accountID.
                SqlCommand selectCommand = DataManager.CreateCommand(@"
                    select Email
                        from User_GUI_SettingsAdwords 
                        where Account_ID = @accountID:Int");

				// Init insertCommand with the data manger connection 
				DataManager.ApplyConnection(selectCommand);
				
                // Initalize select command parameters.
                selectCommand.Parameters["@accountID"].Value = Instance.AccountID;

                try
                {
                    // Execute select command.
                    selectResults = selectCommand.ExecuteReader();
                }
                catch (Exception ex)
                {
					throw new Exception("Error get account email from DB.", ex);
                }

                // Check if we got data from the DB.
                if (!selectResults.HasRows)               
					throw new Exception("There is no row with google account email in the DB.");
                

                // Get the account access data from the DB.
                selectResults.Read();

                try
                {
					accountEmail = selectResults["Email"].ToString();
                }
                catch (Exception ex)
                {
					throw new Exception("The field 'Email' don't exist in the row. Can't get account email from DB.", ex);
                }
            }
        }

        /*=========================*/
        #endregion

        #region Service Override Methods
        /*=========================*/     

        /// <summary>
        /// Main entry point of the service.
        /// </summary>
        /// <returns></returns>
		protected override ServiceOutcome DoWork()
		{
			// Check if we want to Load historic file.
			if (Instance.ParentInstance.Configuration.Options["File"] != null)
			{
				if (File.Exists(Instance.ParentInstance.Configuration.Options["File"]))				
					return ServiceOutcome.Success;
			}

			// Initalize FullService with his account access data.
			string accountEmail;
			GetAccountAccessData(out accountEmail);
		
			AccountData accountData = new AccountData(UserAgent,
                GetConfigurationOptionsField("User"), Encryptor.Decrypt(GetConfigurationOptionsField("Password"), GetConfigurationOptionsField("Password")),
				accountEmail, Token, AppToken);

			FullService fullService = new FullService(accountData);
			fullService.Update();

			// Get data from AdWords and write it to the DB.
			if (!ExecuteJob(fullService))
				return ServiceOutcome.Failure;

			return ServiceOutcome.Success;
		}

        /*=========================*/
        #endregion
    }
}
