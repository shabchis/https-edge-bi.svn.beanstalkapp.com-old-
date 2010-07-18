using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easynet.Edge.Services.DataRetrieval.Retriever
{
	class BackOfficeGenericRetriever :BaseRetriever
	{
		#region Consts
		/*=========================*/

		private const string BackOfficeServiceType = "BackOffice";
		//private const string BackOfficeTable = "BackOffice_Client_Gateway";
		private const string DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm'Z'";

		/*=========================*/
		#endregion

		#region Fields
		/*=========================*/

		string _webServiceUrl = string.Empty;
		DateTime _startDate;
		DateTime _endDate;
		bool _bUseDataSet;

		/*=========================*/
		#endregion

		#region Constructor
		/*=========================*/

		/// <summary>
		/// Initalize the parent fields with const values of this class.
		/// </summary>
		public BackOfficeGenericRetriever()
		{
			serviceType = BackOfficeServiceType;
		}

		/*=========================*/
		#endregion   

		#region Override Protected Methods
		/*=========================*/

		protected override string WriteResultToFile()
		{
			if (!_bUseDataSet)
				return WriteResultToFile(_webServiceUrl, _requiredDay, false, false);
			else
				return WriteDataSetToFile(_webServiceUrl, _startDate, _endDate);
		}


		protected virtual string WriteDataSetToFile(string url, DateTime startDate, DateTime endDate)
		{
			return string.Empty;
		}

		protected override void InitalzieReportData()
		{
			// If UTCOffest exist to the account in the config it means that we want to
			// fetch data also by time and not only by date (for accounts that actucally use 
			// the time parmeter).
			// Also some accounts might have Offest from the UTC, therefore to fix this Offest we  
			// add hours to requiredDay acroding to the UTCOffest value.
			if (Instance.ParentInstance.Configuration.Options["UTCOffest"] != null)
			{
				int UTCOffest = 0;
								// Add UTCOffest hours for requiredDay to get UniversalTime of yestorday
				if (int.TryParse(Instance.ParentInstance.Configuration.Options["UTCOffest"], out UTCOffest))
					_requiredDay = _requiredDay.AddHours(UTCOffest);
			}
		}

		protected override void GetReportData()
		{
			_startDate = _requiredDay.AddMinutes(1);
			_endDate = _requiredDay.AddDays(1).AddMinutes(-1);

			// Initalize the url.
			_webServiceUrl = GetConfigurationOptionsField("URL");

			if (string.IsNullOrEmpty(_webServiceUrl))
				throw new Exception("There isn't url for the backoffice service.");

			string urlParameters = GetConfigurationOptionsField("UrlParameters");

			// Fetch FromParameterName & ToParameterName from configuration if exist
			string fromDate = GetConfigurationOptionsField("FromParameterName") == string.Empty ? "from" : GetConfigurationOptionsField("FromParameterName");
			string toDate = GetConfigurationOptionsField("ToParameterName") == string.Empty ? "to" : GetConfigurationOptionsField("ToParameterName");

			//See if we're using a data set or not.
			if (!Boolean.TryParse(GetConfigurationOptionsField("UseDataSet"), out _bUseDataSet))
				_bUseDataSet = false;
			
			if (!_bUseDataSet)
			{
				// Create the xml file from the web service.
				//Build the parameters string.
				string paramString = fromDate + "=" + _startDate.ToUniversalTime().ToString(DateTimeFormat) + 
					"&" + toDate + "=" + _endDate.ToUniversalTime().ToString(DateTimeFormat) + urlParameters;

				_webServiceUrl += "?" + paramString;
			}
		}

		/*=========================*/
		#endregion   
	}
}
