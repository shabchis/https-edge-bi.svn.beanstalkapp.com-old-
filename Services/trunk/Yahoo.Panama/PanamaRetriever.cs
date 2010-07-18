using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Threading;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Services.Yahoo.Panama.PanamaReportServiceV4;
using Easynet.Edge.Services.Yahoo.Panama.PanamaLocationServiceV4;
using Easynet.Edge.Services.DataRetrieval;

namespace Easynet.Edge.Services.Yahoo.Panama
{
	
	public class PanamaRetrieverService : RetrieverService
	{
		#region Consts
		/*=========================*/
		//private const string RetrieverTable = "Adwords_API_Retriever";
		private const string PanamaServiceType = "Yahoo.Panama";
		private const string Username = "89adk39d3";
		private const string Password = "0ad83k23";

		//private const string Username = "diascience";
		//private const string Password = "yeh06457";
												
		private const string PanamaAccountID =  "22467298744"; //"4074450470";
		private const String PanamaMasterAccountID = "1050674"; // 312233
		private const String License =  "358568CC5A509EC9" ;
		private const String ReportPending = "PENDING";
		//private static string Market = "US";
		//private static string Locale = "en_US";
		private static Encoding Charset = Encoding.UTF8;

		// Yaniv: stantrad it.
		private static string EWS_ACCESS_HTTP_PROTOCOL = "https";
		private static string EWS_LOCATION_SERVICE_ENDPOINT = "sandbox.marketing.ews.yahooapis.com";
		//enumerations for use in printing rejection reasons
		private const int AD_TYPE = 1;
		private const int KEYWORD_TYPE = 2;
		

		/*=========================*/
		#endregion

		#region Members
		/*=========================*/
		private string _panamaDirectory = string.Empty;
		private string _endPointLocation = null;
		private DateTime _retrievelDate;
		
		

		//private static PanamaReportServiceV4.masterAccountID _masterAccountID;
		//private static accountID _AccountID;
		//private static string _accountID;
		//private static PanamaReportServiceV4.Security _securityValue;
		//private static PanamaReportServiceV4.license _license;
		private static Hashtable _locationCache = new Hashtable();
		private static Hashtable _localizedHash = new Hashtable();
		//private static string _locationCacheFilename;
		//private static DateTime _starttime;	
		private static LocationServiceService _locationService = null;

		/*=========================*/
		#endregion

		#region Constructor
		/*=========================*/
		public PanamaRetrieverService()
		{
			System.Net.ServicePointManager.Expect100Continue = false;
		}
		/*=========================*/
		#endregion

		#region

		private void InitalizePanamaService(BasicReportServiceService panamaService)
		{
			panamaService.masterAccountIDValue = new Easynet.Edge.Services.Yahoo.Panama.PanamaReportServiceV4.masterAccountID();
			panamaService.masterAccountIDValue.Text = new String[] { PanamaMasterAccountID };
			panamaService.licenseValue = new Easynet.Edge.Services.Yahoo.Panama.PanamaReportServiceV4.license();
			panamaService.licenseValue.Text = new String[] { License };
			panamaService.SecurityValue = new Easynet.Edge.Services.Yahoo.Panama.PanamaReportServiceV4.Security();
			panamaService.SecurityValue.UsernameToken = new Easynet.Edge.Services.Yahoo.Panama.PanamaReportServiceV4.UsernameToken();
			panamaService.SecurityValue.UsernameToken.Username = Username;
			panamaService.SecurityValue.UsernameToken.Password = Password;

			try
			{
				if (_endPointLocation == null)
				{
					_locationService = new LocationServiceService();
					_locationService.Url = (string)(EWS_ACCESS_HTTP_PROTOCOL + "://" + EWS_LOCATION_SERVICE_ENDPOINT + "/services/V4/LocationService");

					_locationService.masterAccountIDValue = new PanamaLocationServiceV4.masterAccountID();
					_locationService.masterAccountIDValue.Text = new String[] { PanamaMasterAccountID };

					_locationService.SecurityValue = new PanamaLocationServiceV4.Security();
					_locationService.SecurityValue.UsernameToken = new PanamaLocationServiceV4.UsernameToken();
					_locationService.SecurityValue.UsernameToken.Username = Username;
					_locationService.SecurityValue.UsernameToken.Password = Password;

					_locationService.licenseValue = new PanamaLocationServiceV4.license();
					_locationService.licenseValue.Text = new String[] { License };

					//use LocationService to get the address prefix for the rest of the services
					_endPointLocation = _locationService.getMasterAccountLocation();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}


		/// <summary>
		/// Get the data from Panama to and save the data into xml file. 
		/// </summary>
		/// <param name="panamaService"></param>
		/// <param name="panamaReport"></param>
		/// <param name="reportFileFormat"></param>
		private void Retrieve(BasicReportServiceService panamaService, BasicReportRequest panamaReport, FileOutputFormat reportFileFormat)
		{
			// Get report ID from Panama.
			long reportID = panamaService.addReportRequestForAccountID(PanamaAccountID, panamaReport);

			if (reportID == 0)
			{
				throw new Exception("Can't get data from Panama, report id = 0.");
			}

			// Get report url.
			string reportStatus = panamaService.getReportOutputUrl(reportID, reportFileFormat);
			try
			{
				while (reportStatus == ReportPending)
				{
					reportStatus = panamaService.getReportOutputUrl(reportID, reportFileFormat);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}

			string reportUrl = reportStatus;

			//if (url == "failed")
			//{
			//    throw new Exception("Panama return invaild url, report creation failed.");
			//}

			string fileName = WriteResultToFile(reportUrl, _retrievelDate, true);

			// Insert the data to DB.
			SaveFilePathToDB(PanamaServiceType, fileName);
		}

	



		#endregion

		#region Service Override Methods
		/*=========================*/

		/// <summary>
		/// Handle abort event
		/// </summary>
		protected override void OnAbort()
		{
			ServiceAbort("Panama Retriever");
		}

		/// <summary>
		/// Main entry point of the service.
		/// </summary>
		/// <returns></returns>
		protected override ServiceOutcome DoWork()
		{
			BasicReportServiceService panamaService = new BasicReportServiceService();
			BasicReportRequest panamaReport = new BasicReportRequest();

			// Initalize Panama sevice parameters.
			InitalizePanamaService(panamaService);

			if (_endPointLocation != null && _endPointLocation != "")
				panamaService.Url = _endPointLocation + "/V4/BasicReportService";

			_retrievelDate = DateTime.Now.AddDays(-1);
			// Initalize date parameters.
			panamaReport.startDate = _retrievelDate;
			panamaReport.endDate = _retrievelDate;
			panamaReport.dateRangeSpecified = false;
			panamaReport.endDateSpecified = true;
			panamaReport.startDateSpecified = true;


			// http://searchmarketing.yahoo.com/developer/docs/V4/reference/reportDetails.php
			panamaReport.reportType = BasicReportType.AdvancedAdKeywordPerformanceByDay;
			
			panamaReport.reportTypeSpecified = true;
			panamaReport.reportName = "Test";

 
			// Initalzie report FileOutputFormat to XML.
			FileOutputFormat reportFileFormat = new FileOutputFormat();
			reportFileFormat.fileOutputType = FileOutputType.XML;
			reportFileFormat.fileOutputTypeSpecified = true;

			Retrieve(panamaService, panamaReport, reportFileFormat);
	
			return ServiceOutcome.Success;
		}
		
		/*=========================*/
		#endregion
	}
}