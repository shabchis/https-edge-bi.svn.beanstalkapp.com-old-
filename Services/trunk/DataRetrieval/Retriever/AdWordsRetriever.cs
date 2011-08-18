using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Services.DataRetrieval.GAdWordsReportServiceV13;
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Utilities;
using System.Collections;
using System.Configuration;
using Easynet.Edge.Services.DataRetrieval.Configuration;

namespace Easynet.Edge.Services.DataRetrieval.Retriever
{
	/// <summary>
	/// This service get creative report from Google AdWords and insert it to the DB.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>1/08/2008</creation_date>	
	class AdWordsRetriever : BaseRetriever
	{
		#region Consts
		/*=========================*/

		// Goggle Adwords authentication parameters. (email and password are 
		// taken from the configuration.
		private const string UserAgent = "Easynet(seperia) -- C# get clients information";
		//private const string DeveloperToken = "DJ-sOPT568XvXU_DgwymvA";
		//private const string ApplicationToken = "8Uph5HQbsQ4rcrEdmPVFKA";

		//Aggergation types	
		private string[] DailyAggergation = new String[] { "Daily" };
		private string[] SummaryAggergation = new String[] { "Summary" };
		private string[] ContetAggergation = new String[] { "AdGroup", "Url", "Daily" };
	
		/*=========================*/
		#endregion

		#region Fields
		/*=========================*/

		FullService _fullService;
		AccountData _accountData;
		DefinedReportJob _googleJob;
		string _accountEmail = string.Empty;
		string _reportUrl = string.Empty;

		/*=========================*/
		#endregion
		
		#region Constructor
		/*=========================*/

		public AdWordsRetriever()
		{
			_adwordsFile = true;
		}

		/*=========================*/
		#endregion

		#region Private Members
		/*=========================*/

		/// <summary>
		/// Search account settings by account id and retrun 
		/// the access data to this account.
		/// </summary>
		/// <returns>account setings</returns>
		private void GetAccountAccessData()
		{
			_accountEmail = Instance.ParentInstance.Configuration.Options["AccountEmail"];
			if (_accountEmail == null || _accountEmail.Trim().Length < 1)
				throw new Easynet.Edge.Core.Data.ConfigurationException("Adwords API account email is missing from the service configuration (\"AccountEmail\").");
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

		/*=========================*/
		#endregion

		#region Override Protected Methods
		/*=========================*/

		protected override string WriteResultToFile()
		{
			return WriteResultToFile(_reportUrl, _requiredDay, _googleJob.name, true);
		}

		protected override void InitalzieReportData()
		{
			_googleJob.startDay = _requiredDay;
			_googleJob.endDay = _requiredDay;
		}

		protected override void InitalizeReportDate()
		{
			_requiredDay = DateTime.Today.AddDays(-1);

			string reportName = GetConfigurationOptionsField("ReportType");

			if (reportName == "Content")
			{
				_requiredDay = _requiredDay.AddDays(-3);
				string offsetValue = GetConfigurationOptionsField("OffsetDays");
				if (!String.IsNullOrEmpty(offsetValue))
				{
					int offset;
					if (Int32.TryParse(offsetValue, out offset) && offset < 0)
						_requiredDay = DateTime.Today.AddDays(offset);
				}
			}
		}

		protected override void GetReportData()
		{
			long reportID = _fullService.ScheduleReportJob(_googleJob, _accountID);
			_reportUrl = _fullService.GetReportDownloadUrl(reportID, _accountID);
		}

		protected override bool InitalizeServiceData()
		{
			if (!base.InitalizeServiceData())
				return false;

			System.Net.ServicePointManager.Expect100Continue = false;

			// Initalize FullService with his account access data.			
			GetAccountAccessData();

			_accountData = new AccountData(UserAgent,
                GetConfigurationOptionsField("User"), Encryptor.Decrypt(GetConfigurationOptionsField("Password")),
				_accountEmail, GetConfigurationOptionsField("DeveloperToken"), GetConfigurationOptionsField("ApplicationToken"));

			_fullService = new FullService(_accountData);
			_fullService.Update();

			// Initalize the report parameters.
			string reportName = GetConfigurationOptionsField("ReportType");

			if (reportName == string.Empty)
			{
				Log.Write("Adwords retriever doesn't have ReportType parameter.", LogMessageType.Error);
				return false;
			}

			TargetSubDir = @"Google\" + reportName + @"\";
			ErrorFilePath = @"Google\" + reportName + @"\Errors";
			serviceType = "Google.Adwords." + reportName;

			_googleJob = CreateReport("Google" + reportName + "Report", reportName != "Content" ? DailyAggergation : ContetAggergation);
			return true;
		}

		/*=========================*/
		#endregion

		#region Obsolete Code - Use to fetch monthly data.
		/*=========================*/

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
			selectCommand.Parameters["@accountID"].Value = _accountID;
			selectCommand.Parameters["@dayCode"].Value = GetDayCode(lastDay);

			return selectCommand;
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
				DataManager.Current.AssociateCommands(selectCommand);
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
				return CheckDatesFromDB(daysToRetrieve, selectResults, ref summaryAggregation);
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
		

		/*=========================*/
		#endregion
	}
}
