using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.BusinessObjects;
using Easynet.Edge.Services.DataRetrieval;
using Easynet.Edge.Services.DataRetrieval.Retriever;
using System.Collections;

namespace Easynet.Edge.Services.BackOffice.EasyForex
{
	
	public class EasyForexBackOfficeRetrieverService : RetrieverService
	{
		#region Consts
		/*=========================*/
		private const string BackOfficeServiceType = "BackOffice";
		private const string BackOfficeTable = "BackOffice_Client_Gateway";

		/*=========================*/
		#endregion

        #region Members
        /*=========================*/
	
		private ArrayList _errorDates = new ArrayList();
		private DateTime _requiredDay = DateTime.Today; 
		private EasyForexBackOfficeAPI.Marketing _easyForexBackOffice = null;

		/*=========================*/
        #endregion                

		#region Private Methods
		/*=========================*/

		/// <summary>
		/// Validate the data we got from EasyForex BackOffice by checking
		/// the total hits of Major gateway ID that always suppose to
		/// be above 0.
		/// </summary>
		/// <remarks>Insert to app.config the attribure "ValidateString"
		/// with the value of a major gateway ID (see example) in 
		/// each BackOffice Retriever Service.</remarks>
		/// <example>Example for attribute line to insert to app.config:
		/// ValidateString="GID = 23045"</example>
		/// <param name="dataFromBO">The result data we got from EasyForex
		/// BackOffice web service.</param>
		/// <returns>True for valid BO result, false for invalid BO result.</returns>
		private void ValidateReport(DataSet dataFromBO)
		{			
			// Check if the attribure "ValidateString" found in the 
			// configuration, if not we write warning and return true.
			if (Instance.Configuration.Options["ValidateString"] == null && ((Instance.ParentInstance != null) &&
				Instance.ParentInstance.Configuration.Options["ValidateString"] == null))
			{
				Log.Write("There isn't a validate string for EasyForex BO Service.", LogMessageType.Warning);
				return;
			}

			DataRow[] rows = dataFromBO.Tables[0].Select(GetConfigurationOptionsField("ValidateString"));
			
			if (Convert.ToInt32(rows[0]["TotalHits"]) > 0)
			{
				return;
			}

			throw new Exception("The data that Retrievered from EasyForex BackOffice is incorrect.");
		}	

		/// <summary>
		/// Get the data from EasyForex BackOffice using account info and 
		/// return the data in dataFromBO data set.
		/// </summary>
		/// <param name="userName">Account name (for example "Amiry")</param>
		/// <param name="password">Password (for example "wretg2gad")</param>
		/// <param name="dataFromBO">output data set variable that contain 
		/// the data we got from EasyForex BackOffice</param>
		/// <returns>Bool value that indicates if we able get data from EasyForex BackOffice.</returns>
		private void Retrieve(string userName, string password, DataSet dataFromBO)
		{
			// Init BackOffice object		
			_easyForexBackOffice = new EasyForexBackOfficeAPI.Marketing();
			_easyForexBackOffice.Timeout = 10 * 60 * 1000; // 10 minutes.

			// Init access data to easy forex.
			_easyForexBackOffice.AuthHeaderValue = InitBOAccess(userName, password);
			DataTable tempDataTable;
			// Initalize _requiredDay with format that valid for EasyForex BackOffice.
			_requiredDay = new DateTime(_requiredDay.Year, _requiredDay.Month, _requiredDay.Day);

			// Get the data from EasyForex BO. 
			lock (_easyForexBackOffice)
			{
				try
				{
					//tempDataTable = _easyForexBackOffice.GetCampaignStatistics(1, 1000000, _requiredDay, _requiredDay.AddDays(1).AddTicks(-1)).Tables[0];
					tempDataTable = _easyForexBackOffice.GetCampaignStatisticsNEW(1, 1000000, _requiredDay, _requiredDay.AddDays(1).AddTicks(-1)).Tables[0];					
				}
				catch (Exception ex)
				{
					throw new Exception(string.Format("EASY FOREX BUG - Error get data from EasyForex BackOffice for user {0}.", userName), ex);
				}
			}

			// Update DataTable name and insert it to dataFromBO.
			tempDataTable.DataSet.Tables.Remove(tempDataTable);
			tempDataTable.TableName = tempDataTable.TableName + userName;
			dataFromBO.Tables.Add(tempDataTable);

			// Yaniv: remove the remark
			ValidateReport(dataFromBO);
		}

		/// <summary>
		/// Creates access property to Easyforex API object.
		/// </summary>
		/// <param name="user">Account name (for example "Amiry")</param>
		/// <param name="password">Password (for example "wretg2gad")</param>
		/// <returns>Easyforex API access class</returns>
		private EasyForexBackOfficeAPI.AuthHeader InitBOAccess(string user, string password)
		{
			EasyForexBackOfficeAPI.AuthHeader accessAccount = new EasyForexBackOfficeAPI.AuthHeader();
			accessAccount.Password = password;
			accessAccount.Username = user;
			return accessAccount;
		}

		/*=========================*/
		#endregion   

		#region Service Override Methods
		/*=========================*/

		/// <summary>
		/// Handle abort event.
		/// </summary>
		protected override void OnEnded(ServiceOutcome serviceOutcome)
		{
			if ((serviceOutcome == ServiceOutcome.Aborted) ||
				(serviceOutcome == ServiceOutcome.Failure))
			{
				if (_easyForexBackOffice != null)
					_easyForexBackOffice.Abort();				
			}			
		}

		/// <summary>
		/// Main entry point of the service.
		/// </summary>
		/// <returns>True for success. False for failure</returns>
		protected override ServiceOutcome DoWork()
		{
			// Check if we want to Load historic file.
			if (Instance.ParentInstance.ParentInstance.Configuration.Options["File"] != null && File.Exists(Instance.ParentInstance.ParentInstance.Configuration.Options["File"]))
				return ServiceOutcome.Success;				

			//// Return all data of last day
			//if (
			//    (Instance.Configuration.Options["LastDay"] != null && 
			//     Convert.ToBoolean(Instance.Configuration.Options["LastDay"])
			//    )
			//    ||
			//    (Instance.ParentInstance != null && Instance.ParentInstance.Configuration.Options["LastDay"] != null &&
			//     Convert.ToBoolean(Instance.ParentInstance.Configuration.Options["LastDay"])
			//    )
			//   )
			//{
			//    Log.Write("Fetch last day data.", LogMessageType.Information);
			//    _requiredDay = _requiredDay.AddDays(-1);
			//}
			//else if (Instance.ParentInstance.ParentInstance.Configuration.Options["Date"] != null)
			//{
			//    Console.WriteLine("{0} EasyForex BO Retriever service run for date {1}",
			//        ShortHeader(),
			//        Instance.ParentInstance.ParentInstance.Configuration.Options["Date"].ToString());

			//    Log.Write(string.Format("Fetch data for date {0}.", Instance.ParentInstance.ParentInstance.Configuration.Options["Date"].ToString()), LogMessageType.Information);
			//    StringToDate(Instance.ParentInstance.ParentInstance.Configuration.Options["Date"].ToString(), out _requiredDay);
			//}

			DataSet dataFromBO = new DataSet();

			ArrayList dates = new ArrayList();

			if (CheckRangeDate(ref dates))
			{
			    int i;

				if (dates == null || dates.Count == 0)
					return ServiceOutcome.Failure;

				for (i = 0; i < dates.Count && i < _maxInstancesReRuns; ++i)
				{
					_requiredDay = (DateTime)dates[i];
					GetReport(dataFromBO);
				}

			    // Write to the log all the dates that din;t eun because max instances ReRuns.
			    if (i < dates.Count)
			    {
			        string errorMsg = "Can't write the following dates: ";
			        for (int j = i; j < dates.Count; ++j)
			            errorMsg += ((DateTime)dates[j]).ToShortDateString() + ", ";

			        errorMsg += " because the service exceed the max instances ReRuns.";
			        Log.Write(errorMsg, LogMessageType.Error);
			    }

			    if (_errorDates.Count > 0)
			    {
			        for (i = 0; i < _errorDates.Count; ++i)
			            GetReport(dataFromBO);
			    }
			}
			else
			{
			    _requiredDay = DateTime.Now;

			    // Check if we need to get manual date.
			    CheckManualDate();
				if (!GetReport(dataFromBO))
					return ServiceOutcome.Failure;
			}

			return ServiceOutcome.Success;
		}

		private bool GetReport(DataSet dataFromBO)
		{
			Log.Write(string.Format("EasyForex BackOffice retriever run for, account {0}, for date {1}", Instance.Configuration.Options["User"], _requiredDay.ToShortDateString()), LogMessageType.Information);
			// Get data from EasyForex BackOffice.						
			Retrieve(GetConfigurationOptionsField("User"), Encryptor.Decrypt(GetConfigurationOptionsField("Password"), GetConfigurationOptionsField("Password")), dataFromBO);

			string fileName = WriteResultToFile(dataFromBO, _requiredDay);
			if (!String.IsNullOrEmpty(fileName))
				return SaveFilePathToDB(BackOfficeServiceType, fileName, _requiredDay, true);
			else
				throw new Exception("FileName is empty.");
		}

		/*=========================*/
		#endregion   
	}
}


