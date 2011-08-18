using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Services.DataRetrieval.Configuration;
using System.Net;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Utilities;
using System.IO;
using System.Configuration;

namespace Easynet.Edge.Services.DataRetrieval.Retriever
{
	class GAnalyticsRetriever :BaseRetriever
	{
		#region consts
		/*=========================*/

		private const string AnalyticsServiceType = "Analytics";

		/*=========================*/
		#endregion

		#region Fields
		/*=========================*/
	
		string _reportUrl = string.Empty;
		FieldElementSection _fieldsMapping;
		Dictionary<string, string> _headers = new Dictionary<string, string>();

		/*=========================*/
		#endregion

		#region Constructor
		/*=========================*/

		/// <summary>
		/// Initalize the parent fields with const values of this class.
		/// </summary>
		public GAnalyticsRetriever()
		{
			serviceType = AnalyticsServiceType;
		}

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
		private string CreateGAnalyticsUrl()
		{
			try
			{
				string profileID = GetConfigurationOptionsField("profileID");

				char[] charsToTrim = { ',' };
				string url = @"https://www.google.com/analytics/feeds/data?ids=ga:" + profileID + @"&dimensions=";

				// Add dimensions' fields
				foreach (FieldElement fe in _fieldsMapping.Fields)
				{
					if (fe.Enabled && fe.IsDimension)
						url += fe.Key + ",";
				}

				// Remove last ,
				url = url.TrimEnd(charsToTrim);
				url += @"&metrics=";

				// Add metrics fields
				foreach (FieldElement fe in _fieldsMapping.Fields)
				{
					if (fe.Enabled && !fe.IsDimension)
						url += fe.Key + ",";
				}

				url = url.TrimEnd(charsToTrim);
				url += @"&sort=ga:date&start-date=" + ConvetDateTimeToString(_requiredDay) + "&end-date=" + ConvetDateTimeToString(_requiredDay);
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

		protected override bool InitalizeServiceData()
		{
			_fieldsMapping = (FieldElementSection)ConfigurationManager.GetSection
				(GetConfigurationOptionsField("FieldsMapping"));

			// Create Authentication Token 
			string authToken = FetchAutoToken();
			_headers.Add("Authorization", "GoogleLogin auth=" + authToken);

			return base.InitalizeServiceData();
		}

		protected override string WriteResultToFile()
		{
			return WriteResultToFile(_reportUrl, _requiredDay, string.Empty, false, _headers);
		}

		protected override void GetReportData()
		{
			_reportUrl = CreateGAnalyticsUrl();
		}

		/*=========================*/
		#endregion
	}
}
