using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data.SqlClient;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Services.DataRetrieval.GAdWordsReportServiceV13;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Services.DataRetrieval.Configuration;
using System.Configuration;
using System.IO;
using System.Net;
using Easynet.Edge.Core.Configuration;

namespace Easynet.Edge.Services.DataRetrieval.Retriever
{
	public class BaseRetriever : RetrieverService
	{

		#region Consts
		/*=========================*/

		protected const int DivideFieldsValue = 1000000;
		protected string xmlRowName = string.Empty;
		protected string tableName = string.Empty;
		protected string serviceType = string.Empty;
		protected string defaultErrorSubDirPath = string.Empty;
		protected int defaultMinusRequiredDate = 0;

		/*=========================*/
		#endregion  

		#region Fields
		/*=========================*/

        protected Easynet.Edge.Services.DataRetrieval.Configuration.FieldElementSection rawDataFields;
		protected int _channelID;	
		protected Dictionary<string, string> _tableTypeMappings;
		private ArrayList _errorDates = new ArrayList();

		/*=========================*/
		#endregion   
	 
		#region Protected Methods
		/*=========================*/

		protected virtual bool SaveReport()
		{
			GetReportData();

			string fileName = WriteResultToFile(); 

			//string fileName = WriteResultToFile(dataFromBO, _requiredDay);
			if (!String.IsNullOrEmpty(fileName))
				return SaveFilePathToDB(serviceType, fileName, _requiredDay, _adwordsFile);
			else
			{
				Log.Write("Saved report file name is empty.", LogMessageType.Error);
				return false;
			}
		}


    


		protected virtual bool GetReport()
		{
			InitalzieReportData();

			Log.Write(string.Format("Run retriever for service {2}, for account {0}, for date {1}.", _accountID.ToString(), _requiredDay.ToShortDateString(), Instance.ParentInstance.Configuration.Name), LogMessageType.Information);

			int numOfRetries = 0;
			bool reportSaved = false;

			while (!reportSaved)
			{
			//	try
				{
					reportSaved = SaveReport();

					if (numOfRetries >= _maxRetries)	// too many retries, bail out					
						return false;

					++numOfRetries;
				}
				//catch (Exception ex)
				{
					if (numOfRetries >= _maxRetries)	// too many retries, bail out					
					{
					//	Log.Write("Can't get report.", ex);
						return false;
					}

					++numOfRetries;
				}
			}
			return true;

			// Get data from EasyForex BackOffice.						
			//Retrieve(GetConfigurationOptionsField("User"), Encryptor.Decrypt(GetConfigurationOptionsField("Password")), dataFromBO);	
		}

		protected virtual void InitalizeReportDate()
		{
			_requiredDay = DateTime.Today;
		}

		/*=========================*/
		#endregion

		#region Empry Protected Methods
		/*=========================*/

		protected virtual void InitalzieReportData()
		{
		}

		protected virtual bool InitalizeServiceData()
		{
			return InitalizeAccountID();
		}

		protected virtual void GetReportData()
		{
		}

		protected virtual string WriteResultToFile()
		{
			return string.Empty;
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

			if (!string.IsNullOrEmpty(GetConfigurationOptionsField("FilePath")) && File.Exists(GetConfigurationOptionsField("FilePath")))
				return ServiceOutcome.Success;

			if (!InitalizeServiceData())
				return ServiceOutcome.Failure;

			ArrayList dates = new ArrayList();

			if (CheckRangeDate(ref dates))
			{
				int i;

				if (dates == null || dates.Count == 0)
					return ServiceOutcome.Failure;

				for (i = 0; i < dates.Count && i < _maxInstancesReRuns; ++i)
				{
					_requiredDay = (DateTime)dates[i];
                    StringToDate(GetDayCode(_requiredDay).ToString(),out _requiredDay);
					GetReport();
				}

				// Write to the log all the dates that din't run because max instances ReRuns.
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
						GetReport();
				}
			}
			else
			{
				InitalizeReportDate();

				// Check if we need to get manual date.
				CheckManualDate();
				if (!GetReport())
					return ServiceOutcome.Failure;
			}

			return ServiceOutcome.Success;						
		}

		

		/*=========================*/
		#endregion
	}
		
}

