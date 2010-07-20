using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Services.DataRetrieval;
using Easynet.Edge.Services.DataRetrieval.Configuration;
using System.Collections;

namespace Easynet.Edge.Services.Google.Analytics
{
	public class GAnalyticsRetrieverService : RetrieverService
	{
		#region consts
		/*=========================*/

		private const string AnalyticsServiceType = "Analytics";

		/*=========================*/
		#endregion

		#region Fields
		/*=========================*/

		private ArrayList _errorDates = new ArrayList();

		/*=========================*/
		#endregion

		#region Private Methods
		/*=========================*/

		/// <summary>
		/// Get the authentication token according to our user and password. 
		/// </summary>
		/// <remarks>We get authentication token from the url:
		/// https://www.google.com/accounts/ClientLogin
		/// The user and password are provided using post in a regular HttpWebRequest.
		/// The return authentication from the web request is saved to a temporary file and 
		/// fetched from the file and used for the Ganalytics url request.
		///</remarks>
		/// <returns>The authentication token</returns>
		private string FetchAutoToken()
		{
			string loginUrl = @"https://www.google.com/accounts/ClientLogin";

			// Initalize HttpWebRequest
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(loginUrl);
			request.UserAgent = AppSettings.Get(this, "UserAgent");
			request.Timeout = (int)TimeSpan.Parse(AppSettings.Get(this, "RequestTimeout")).TotalMilliseconds;
			request.ContentType = "text/plain";

			// Initalize post Data
			request.ContentType = "application/x-www-form-urlencoded ";
			request.Method = "POST";
			ASCIIEncoding encoding = new ASCIIEncoding();
			string postData = "accountType=GOOGLE&Email=" + GetConfigurationOptionsField("User") + "&Passwd=" + Encryptor.Decrypt(GetConfigurationOptionsField("Password")) + "&source=curl-tester-1.0&service=analytics";
			byte[] data = encoding.GetBytes(postData);

			// Prepare web request...
			request.ContentLength = data.Length;
			Stream newStream = request.GetRequestStream();
			// Send the data.
			newStream.Write(data, 0, data.Length);
			newStream.Close();

			// Fetch auth token from HttpWebResponse.  
			string fileName = String.Empty;
			string contents;
			int authIndex;
			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			{
				// Initalize urlStream.
				Stream urlStream = response.GetResponseStream();
				urlStream.ReadTimeout = 30000;

				StreamReader reader = new StreamReader(urlStream);
				contents = reader.ReadToEnd();
				authIndex = contents.IndexOf("Auth=");
			}

			return contents.Substring(authIndex + 5);
		}


		/// <summary>
		/// Create GAnalytics url using the dimesnions (the nodes with attribute 
		/// IsDimension="True") and metrics in the fieldsMapping and the date retrievedDay.
		/// </summary>
		/// <param name="profileID">The required profile id to get his data from Ganalytics.</param>
		/// <param name="fieldsMapping">Contain a section with the metrics and dimesions to insert to the url.</param>
		/// <param name="retrievedDay">The date to fetch the data added to the end of the url.</param>
		/// <returns></returns>
		private string CreateGAnalyticsUrl(string profileID, FieldElementSection fieldsMapping, DateTime retrievedDay)
		{
			try
			{
				char[] charsToTrim = { ',' };
				string url = @"https://www.google.com/analytics/feeds/data?ids=ga:" + profileID + @"&dimensions=";

				// Add dimensions' fields
				foreach (FieldElement fe in fieldsMapping.Fields)
				{
					if (fe.Enabled && fe.IsDimension)
						url += fe.Key + ",";
				}

				// Remove last ,
				url = url.TrimEnd(charsToTrim);
				url += @"&metrics=";

				// Add metrics fields
				foreach (FieldElement fe in fieldsMapping.Fields)
				{
					if (fe.Enabled && !fe.IsDimension)
						url += fe.Key + ",";
				}

				url = url.TrimEnd(charsToTrim);
				url += @"&sort=ga:date&start-date=" + ConvetDateTimeToString(retrievedDay) + "&end-date=" + ConvetDateTimeToString(retrievedDay);
				return url;
			}
			catch (Exception ex)
			{
				throw new Exception("Error fetch data from the configuration.", ex);
			}
		}

		/// <summary>
		/// Convert datetime to the format YYYY-MM-DD.
		/// </summary>
		/// <param name="retrievedDay"></param>
		/// <returns></returns>
		private string ConvetDateTimeToString(DateTime retrievedDay)
		{
			return retrievedDay.Year + "-" + retrievedDay.Month.ToString("00") + "-" + retrievedDay.Day.ToString("00");
		}

		/*=========================*/
		#endregion

		#region protected override Methods
		/*================================*/

		/// <summary>
		/// Main entry point of the service.
		/// </summary>
		/// <returns></returns>
		protected override ServiceOutcome DoWork()
		{
			FieldElementSection fieldsMapping = (FieldElementSection)ConfigurationManager.GetSection
				(GetConfigurationOptionsField("FieldsMapping"));

			string profileID = GetConfigurationOptionsField("profileID");

			// Create Authentication Token 
			string authToken = FetchAutoToken();
			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add("Authorization", "GoogleLogin auth=" + authToken);
			
			ArrayList dates = new ArrayList();

			if (CheckRangeDate(ref dates))
			{
				int i;
				if (dates == null || dates.Count == 0)
					return ServiceOutcome.Failure;

				for (i = 0; i < dates.Count && i < _maxInstancesReRuns; ++i)
					GetReport(fieldsMapping, (DateTime)dates[i], profileID, headers);

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
						GetReport(fieldsMapping, (DateTime)dates[i], profileID, headers);
				}
			}
			else
			{
				DateTime retrievedDay = DateTime.Now.AddDays(-1);
				// Check if we need to get manual date.
				CheckManualDate(ref retrievedDay);
				if (!GetReport(fieldsMapping, retrievedDay, profileID, headers))
					return ServiceOutcome.Failure;
			}
			
			return ServiceOutcome.Success;
		}

		private bool GetReport(FieldElementSection fieldsMapping, DateTime retrievedDay, string profileID, Dictionary<string, string> headers)
		{
			Log.Write(string.Format("run GAnalytics Retriever for account {0}, for date {1}.", Instance.AccountID.ToString(), retrievedDay.ToShortDateString()), LogMessageType.Information);

			string url = CreateGAnalyticsUrl(profileID, fieldsMapping, retrievedDay);
			string fileName = WriteResultToFile(url, retrievedDay, string.Empty, false, headers);
			return SaveFilePathToDB(AnalyticsServiceType, fileName, retrievedDay, _adwordsFile);
		}

		/*=========================*/
		#endregion
	}
}
