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
using Easynet.Edge.Services.DataRetrieval.Retriever;
using System.Net;
using System.Xml;
using System.Collections;

namespace Easynet.Edge.Services.BackOffice.Generic
{

	public class BackOfficeGenericRetrieverService : RetrieverService
	{
		#region Consts
		/*=========================*/

		private const string BackOfficeServiceType = "BackOffice";
		private const string BackOfficeTable = "BackOffice_Client_Gateway";
		private const string DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm'Z'";

		/*=========================*/
		#endregion

		#region Fields
		/*=========================*/

		//private DateTime _requiredDay = DateTime.Today;		
		private ArrayList _errorDates = new ArrayList();

		/*=========================*/
		#endregion

		#region Private Methods
		/*=========================*/
		/// <summary>
		/// Get the data from EasyForex BackOffice using account info and 
		/// return the data in dataFromBO data set.
		/// </summary>
		/// <param name="userName">Account name (for example "Amiry")</param>
		/// <param name="password">Password (for example "wretg2gad")</param>
		/// <param name="dataFromBO">output data set variable that contain 
		/// the data we got from EasyForex BackOffice</param>
		/// <returns>Bool value that indicates if we able get data from EasyForex BackOffice.</returns>
		private bool Retrieve(DateTime startDate, DateTime endDate)
		{
			Log.Write(string.Format("Run BackOffice Generic Retriever for account {0}, for date {1}.", Instance.AccountID.ToString(), startDate.ToShortDateString()), LogMessageType.Information);

			// Initalize the url.
			if (Instance.ParentInstance == null || (Instance.ParentInstance != null && 
				Instance.ParentInstance.Configuration.Options["URL"] == null))
			{
				throw new Exception("There isn't url for the backoffice service.");
			}

			string urlParameters = string.Empty;
			string fromDate = "from";
			string toDate = "to";

			// Fetch UrlParameters from configuration if exist
			if (Instance.ParentInstance.Configuration.Options["UrlParameters"] != null)
			{
				urlParameters = Instance.ParentInstance.Configuration.Options["UrlParameters"].ToString();
			}

			// Fetch FromParameterName from configuration if exist
			if (Instance.ParentInstance.Configuration.Options["FromParameterName"] != null)
			{
				fromDate = Instance.ParentInstance.Configuration.Options["FromParameterName"].ToString();
			}

			// Fetch ToParameterName from configuration if exist
			if (Instance.ParentInstance.Configuration.Options["ToParameterName"] != null)
			{
				toDate = Instance.ParentInstance.Configuration.Options["ToParameterName"].ToString();
			}

            bool bUseDataSet = false;
            //See if we're using a data set or not.
            if (Instance.ParentInstance.Configuration.Options["UseDataSet"] != null)
            {
                bUseDataSet = Convert.ToBoolean(Instance.ParentInstance.Configuration.Options["UseDataSet"]);
            }

			// Build webServiceUrl url and convert our local time to UniversalTime
			// (decrese 2 hours (or 3 hours in summer clock) from startDate & toDate).
            string webServiceUrl = Instance.ParentInstance.Configuration.Options["URL"].ToString();

            string fileName = String.Empty;
            if (!bUseDataSet)
            {
                // Create the xml file from the web service.
                //Build the parameters string.
                string paramString = fromDate + "=" + startDate.ToUniversalTime().ToString(DateTimeFormat) + "&" + toDate + "=" + endDate.ToUniversalTime().ToString(DateTimeFormat)
                    + urlParameters;
				
                webServiceUrl += "?" + paramString;
                fileName = WriteResultToFile(webServiceUrl, startDate, false, false);
            }
            else
            {
                //We just need the basic URL. We build the post parameters internally.
                fileName = WriteDataSetToFile(webServiceUrl, startDate, endDate);
            }

			// Insert the data to DB.
			return SaveFilePathToDB(BackOfficeServiceType, fileName, startDate, _adwordsFile);		
		}

		protected string PharseDate(DateTime date)
		{
			return date.Month.ToString("00") + date.Day.ToString("00") + date.Year.ToString();
		}

        protected virtual string WriteDataSetToFile(string url, DateTime startDate, DateTime endDate)
        {
            return String.Empty;
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
				
			}
		}

		/// <summary>
		/// Main entry point of the service.
		/// </summary>
		/// <returns>True for success. False for failure</returns>
		protected override ServiceOutcome DoWork()
		{
			ArrayList dates = new ArrayList();

			if (CheckRangeDate(ref dates))
			{
				int i;
				if (dates == null || dates.Count == 0)
					return ServiceOutcome.Failure;
				for (i = 0; i < dates.Count && i < _maxInstancesReRuns; ++i)
					GetReport((DateTime)dates[i]);

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
						GetReport((DateTime)dates[i]);
				}
			}
			else
			{
				DateTime rawRequiredDay = DateTime.Today;
				// Check if we need to get manual date.
				//CheckManualDate(ref rawRequiredDay);
				if (!GetReport(rawRequiredDay))
					return ServiceOutcome.Failure;
			}

			return ServiceOutcome.Success;
		}

		private bool GetReport(DateTime rawRequiredDay)
		{
			// If UTCOffest exist to the account in the config it means that we want to
			// fetch data also by time and not only by date (for accounts that actucally use 
			// the time parmeter).
			// Also some accounts might have Offest from the UTC, therefore to fix this Offest we  
			// add hours to requiredDay acroding to the UTCOffest value.
			if (Instance.ParentInstance.Configuration.Options["UTCOffest"] != null)
			{
				int UTCOffest = 0;
				int.TryParse(Instance.ParentInstance.Configuration.Options["UTCOffest"], out UTCOffest);

				DateTime requiredDay = new DateTime(rawRequiredDay.Year, rawRequiredDay.Month, rawRequiredDay.Day);

				// Add UTCOffest hours for requiredDay to get UniversalTime of yestorday
				requiredDay = requiredDay.AddHours(UTCOffest);

				// Get data from Generic BackOffice.
				return Retrieve(requiredDay.AddMinutes(1), requiredDay.AddDays(1).AddMinutes(-1));
			}
			else // Fetch data only by date (time is not relevent)
			{
				// Get data from Generic BackOffice.
				DateTime requiredDay = new DateTime(rawRequiredDay.Year, rawRequiredDay.Month, rawRequiredDay.Day);
				return Retrieve(requiredDay.AddMinutes(1), requiredDay.AddDays(1).AddMinutes(-1));
			}
		}
		/*=========================*/
		#endregion
	}
}
