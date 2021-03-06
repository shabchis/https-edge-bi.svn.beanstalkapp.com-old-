﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using System.ServiceModel;
using myFacebook = Facebook;
using Easynet.Edge.Services.DataRetrieval.Retriever;
//using Excel =  Microsoft.Office.Interop.Excel;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core;
using Easynet.Edge.Core.Configuration;
using System.IO;
using System.Net;

namespace Easynet.Edge.Services.Facebook
{
	public class FacebookBaseRetriever : BaseRetriever
	{
		protected myFacebook.Session.ConnectSession connSession;
		protected string _targetDirectory;
		protected System.IO.StreamWriter wrtTxtFile;
		protected string _ApplicationID = string.Empty;
		protected string _FBaccountID = string.Empty;
		protected string _apiKey = string.Empty;
		protected string _ap_secret = string.Empty;
		protected string _session = string.Empty;
		protected string _sessionSecret = string.Empty;
		protected string _UserId = string.Empty;
		protected string _accoutnName = string.Empty;
		protected string _createExcelDirectoryPath = string.Empty;
		protected string _pipe = string.Empty;
		protected string _accessToken;
		protected string _urlAuth;
		protected string _redirectUrl;
		protected Uri _baseAddress;

		protected override void InitalzieReportData()
		{


			rawDataFields = (Easynet.Edge.Services.DataRetrieval.Configuration.FieldElementSection)System.Configuration.ConfigurationManager.GetSection
		 (GetConfigurationOptionsField("FieldsMapping"));

			if (Instance.Configuration.Options["Facebook.Auth.ApiKey"] == null)
				_apiKey = Instance.ParentInstance.Configuration.Options["Facebook.Auth.ApiKey"].ToString();
			else
				_apiKey = Instance.Configuration.Options["Facebook.Auth.ApiKey"].ToString();


			if (Instance.Configuration.Options["Facebook.Auth.AppSecret"] == null)
				_ap_secret = Instance.ParentInstance.Configuration.Options["Facebook.Auth.AppSecret"].ToString();
			else
				_ap_secret = Instance.Configuration.Options["Facebook.Auth.AppSecret"].ToString();

			if (Instance.Configuration.Options["Facebook.Auth.SessionSecret"] == null)
				_sessionSecret = Instance.ParentInstance.Configuration.Options["Facebook.Auth.SessionSecret"].ToString();
			else
				_sessionSecret = Instance.Configuration.Options["Facebook.Auth.SessionSecret"].ToString();


			if (Instance.Configuration.Options["FBaccountID"] == null)
				_FBaccountID = Instance.ParentInstance.Configuration.Options["FBaccountID"].ToString();
			else
				_FBaccountID = Instance.Configuration.Options["FBaccountID"].ToString();

			if (Instance.Configuration.Options["Facebook.AuthenticationUrl"] == null)
				_urlAuth = Instance.ParentInstance.Configuration.Options["Facebook.AuthenticationUrl"].ToString();
			else
				_urlAuth = Instance.Configuration.Options["Facebook.AuthenticationUrl"].ToString();

			if (Instance.Configuration.Options["Facebook.Auth.RedirectUri"] == null)
				_redirectUrl = Instance.ParentInstance.Configuration.Options["Facebook.Auth.RedirectUri"].ToString();
			else
				_redirectUrl = Instance.Configuration.Options["Facebook.Auth.RedirectUri"].ToString();

			if (Instance.Configuration.Options["Facebook.BaseServiceAdress"] == null)
				_baseAddress = new Uri(Instance.ParentInstance.Configuration.Options["Facebook.BaseServiceAdress"].ToString());
			else
				_baseAddress = new Uri(Instance.Configuration.Options["Facebook.BaseServiceAdress"].ToString());


			if (Instance.Configuration.Options["accountName"] == null)
				_accoutnName = Instance.ParentInstance.Configuration.Options["accountName"].ToString();
			else
				_accoutnName = Instance.Configuration.Options["accountName"].ToString();

			if (Instance.Configuration.Options["TargetDirectory"] == null)
				_targetDirectory = Instance.ParentInstance.Configuration.Options["TargetDirectory"].ToString();
			else
				_targetDirectory = Instance.Configuration.Options["TargetDirectory"].ToString();






			if (Instance.Configuration.Options["serviceType"] == null)
				serviceType = Instance.ParentInstance.Configuration.Options["serviceType"].ToString();
			else
				serviceType = Instance.Configuration.Options["serviceType"].ToString();

			try
			{
				if (Instance.Configuration.Options["Pipe"] == null)
					_pipe = Instance.ParentInstance.Configuration.Options["Pipe"].ToString();
				else
					_pipe = Instance.Configuration.Options["Pipe"].ToString();

			}
			catch (Exception e) { }


			//-------new connection to facebook-------------------------///
			_urlAuth = string.Format(string.Format(_urlAuth, _apiKey, _redirectUrl, _ap_secret, _sessionSecret));



			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(_urlAuth);
			WebResponse response = request.GetResponse();

			using (StreamReader stream = new StreamReader(response.GetResponseStream()))
			{
				_accessToken = stream.ReadToEnd();
			}




		}

		static string ResultOutputPath = AppSettings.Get(typeof(FacebookBaseRetriever), "ResultOutputPath");
		protected string SendFacebookRequest(string methodUrl, string methodName)
		{
			return SendFacebookRequest(methodUrl, methodName, string.Empty);
		}
		protected string SendFacebookRequest(string methodUrl, string methodName, string fileSuffix)
		{



			Log.Write("Facebook API request.\n\n" + methodUrl, LogMessageType.Information);

			StringBuilder result=new StringBuilder();
			Uri finalUrl = new Uri(_baseAddress, methodUrl);
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(string.Format("{0}&{1}", finalUrl.ToString(), _accessToken));

			//result = facebookAPI.Application.SendRequest(parameterList);

			string outputDir = Path.Combine(ResultOutputPath, String.Format("{0:yyyy/MM/dd}", DateTime.Now));
			if (!Directory.Exists(outputDir))
				Directory.CreateDirectory(outputDir);

			string outputPath = Path.Combine(outputDir, String.Format("{1:00000}@{0:hhmm} {2}{3}.xml",
				DateTime.Now,
				Instance.AccountID,
				methodName,
				fileSuffix
			));

			try
			{
				WebResponse response = request.GetResponse();


				using (Stream reader = response.GetResponseStream())
				{
					using (FileStream outputstream = File.Create(outputPath))
					{
						int bufferSize = 2 << 10;
						byte[] buffer = new byte[bufferSize];
						int bytesRead = 0;
						while ((bytesRead=reader.Read(buffer, 0, bufferSize)) != 0)
						{
							result.Append(Encoding.UTF8.GetString(buffer,0,bytesRead));
							outputstream.Write(buffer, 0, bytesRead);
						}
						
						//File.WriteAllText(, result, Encoding.UTF8);
						Log.Write("Facebook API results saved to " + outputPath, LogMessageType.Information);

					}

				}
			}
			catch (Exception ex)
			{
				Log.Write("Facebook API results could not be saved to " + outputPath, ex);
			}
			return result.ToString();
		}


		protected virtual bool UpdateDB()
		{
			return true;
		}

		//protected override string WriteResultToFile()
		//{
		//    try
		//    {
		//        if(null==listOfFaceBookRows)
		//            return null;

		//        //Core.Utilities.Log.Write("---------------listOfFaceBookRows.Count, " + listOfFaceBookRows.Count, Core.Utilities.LogMessageType.Error);
		//        if (listOfFaceBookRows.Count > 0)
		//            return CreateXLSFile(listOfFaceBookRows);
		//        else
		//            return null;
		//    }
		//    catch (Exception ex)
		//    {
		//        Core.Utilities.Log.Write("Error in WriteResultToFile(), " + ex.Message, Core.Utilities.LogMessageType.Error);
		//        return null;
		//    }
		//}

		public string CreateXLSFile(List<FacebookRow> listOfFaceBookRows)
		{


			string _createCopyFilePath;
			if (listOfFaceBookRows.Count == 0)
				return "empty";
			string rowString = "";

			//_createExcelDirectoryPath = _targetDirectory + "xxxxxxxxx.txt";
			//_createExcelDirectoryPath=@"C:\"+ "TestFacebookFile222.txt";
			//_createExcelDirectoryPath = @"D:\Edge\IMPORT\PPC\Creative\" + "TestFacebookFile222.txt";              
			string dir = FormatPath(Easynet.Edge.Core.Configuration.AppSettings.Get(this, "ResultsRoot"));
			_createExcelDirectoryPath = dir + _targetDirectory + DateTime.Now.Millisecond.ToString() + "Facebook.txt";



			string targetSubDir = Instance.ParentInstance.Configuration.Options["TargetSubDirectory"].ToString();



			_createCopyFilePath = InitalizeFileName(_createExcelDirectoryPath, DateTime.Now, string.Empty, targetSubDir);


			using (wrtTxtFile = new System.IO.StreamWriter(_createExcelDirectoryPath,
				  false, Encoding.Unicode))
			{



				string accountName = _accoutnName; //"EasyForex";
				string month = _requiredDay.Month.ToString();
				if (month.Length == 1)
					month = "0" + month;

				string day = _requiredDay.Day.ToString();
				if (day.Length == 1)
					day = "0" + day;

				string date = _requiredDay.Year.ToString() + month + day;



				//Create the headers
				rowString = "Channel\tAccountName\tDay_Code";
				foreach (var item in listOfFaceBookRows[0]._Values.Keys)
				{

					rowString += "\t" + item;
				}
				wrtTxtFile.WriteLine(rowString);
				//~Create the headers


				foreach (var row in listOfFaceBookRows)
				{
					rowString = "Facebook\t" + accountName + "\t" + date + "\t";
					foreach (var value in row._Values)//.Values)
					{
						if (value.Key == "Cost")///100 the cost
						{
							double cost = Convert.ToDouble(value.Value);
							cost = cost / 100;
							string strCost = String.Format("{0:0.00}", cost);
							rowString += strCost + "\t";
						}
						else
							rowString += value.Value + "\t";
					}
					wrtTxtFile.WriteLine(rowString);
				}


			}
			if (_createCopyFilePath != null)
				System.IO.File.Copy(_createExcelDirectoryPath, _createCopyFilePath);
			return _createExcelDirectoryPath;
		}

		//public string CreateXLSFile(List<AdGroupClass> list)
		// {
		//     try
		//     {            
		//          string rowString = "";

		//          try
		//          {
		//              _createExcelDirectoryPath = _targetDirectory + @"\TTTTTT.txt";
		//              // wrtTxtFile = new System.IO.StreamWriter( _createExcelDirectoryPath ,
		//              wrtTxtFile = new System.IO.StreamWriter(_createExcelDirectoryPath,
		//                  false, Encoding.Unicode);
		//          }
		//          catch (Exception ex)
		//          {
		//              Core.Utilities.Log.Write("Error in CreateXLSFile() cannot create file, " + ex.Message, Core.Utilities.LogMessageType.Error);
		//              return "";

		//          }     
		//         string accountName = _accoutnName; //"EasyForex";
		//         string month = _requiredDay.Month.ToString();
		//         if (month.Length == 1)
		//             month = "0" + month;

		//         string day = _requiredDay.Day.ToString();
		//         if (day.Length == 1)
		//             day = "0" + day;

		//         string date = _requiredDay.Year.ToString() + month + day;


		//         

		//     rowString = "Channel\tAccountName\tDay_Code\tdestUrl\tCampaign\tImps\tClicks\tCost";
		//         wrtTxtFile.WriteLine(rowString);

		//         foreach (AdGroupClass item in list)
		//         {
		//             rowString = "";
		//             rowString += rowString + "Facebook\t" + accountName + "\t" + date + "\t" + item._name + "\t"
		//                 + item._campainName + "\t" + item._impressions  + "\t" + item._clicks
		//                   + "\t" + (Convert.ToDouble(item._spent) / 100).ToString();

		//           //  rowCount++;
		//             wrtTxtFile.WriteLine(rowString);
		//         }
		//         wrtTxtFile.Close();
		//         wrtTxtFile.Dispose();

		//         return _createExcelDirectoryPath;
		//     }
		//     catch (Exception ex)
		//     {
		//         Edge.Core.Utilities.Log.Write("Error in FacebookRetriever.CreateXLSFile(): "+ex.Message, Edge.Core.Utilities.LogMessageType.Error);
		//         return null;
		//     }
		// }
	}
}
