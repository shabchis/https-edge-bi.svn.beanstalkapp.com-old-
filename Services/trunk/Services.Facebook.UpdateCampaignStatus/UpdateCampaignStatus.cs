﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using myFacebook = Facebook;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;
using System.Xml;

namespace Services.UpdateCampaignStatus
{
	public class UpdateCampaignStatus : Service
	{
		protected myFacebook.Session.ConnectSession connSession;
		private Dictionary<string, string> _parameterList;
		private myFacebook.Rest.Api _facebookAPI;
		private string _ApplicationID = string.Empty;
		private string _FBaccountID = string.Empty;
		private string _apiKey = string.Empty;
		private string _ap_secret = string.Empty;
		private string _session = string.Empty;
		private string _sessionSecret = string.Empty;
		private string _UserId = string.Empty;
		private string _accoutnName = string.Empty;
		private string _createExcelDirectoryPath = string.Empty;
		private string _pipe = string.Empty;
		private string _serviceType = string.Empty;
		private int _channelID;
		protected override void OnInit()
		{

			_parameterList = new Dictionary<string, string>();

			try
			{

				if (Instance.Configuration.Options["APIKey"] == null)
					_apiKey = Instance.ParentInstance.Configuration.Options["APIKey"].ToString();
				else
					_apiKey = Instance.Configuration.Options["APIKey"].ToString();


				if (Instance.Configuration.Options["sessionKey"] == null)
					_session = Instance.ParentInstance.Configuration.Options["sessionKey"].ToString();
				else
					_session = Instance.Configuration.Options["sessionKey"].ToString();

				if (Instance.Configuration.Options["applicationSecret"] == null)
					_ap_secret = Instance.ParentInstance.Configuration.Options["applicationSecret"].ToString();
				else
					_ap_secret = Instance.Configuration.Options["applicationSecret"].ToString();

				if (Instance.Configuration.Options["FBaccountID"] == null)
					_FBaccountID = Instance.ParentInstance.Configuration.Options["FBaccountID"].ToString();
				else
					_FBaccountID = Instance.Configuration.Options["FBaccountID"].ToString();

				//if (Instance.Configuration.Options["applicationID"] == null)
				//    _ApplicationID = Instance.ParentInstance.Configuration.Options["applicationID"].ToString();
				//else
				//    _ApplicationID = Instance.Configuration.Options["applicationID"].ToString();



				if (Instance.Configuration.Options["accountName"] == null)
					_accoutnName = Instance.ParentInstance.Configuration.Options["accountName"].ToString();
				else
					_accoutnName = Instance.Configuration.Options["accountName"].ToString();

				if (Instance.Configuration.Options["sessionSecret"] == null)
					_sessionSecret = Instance.ParentInstance.Configuration.Options["sessionSecret"].ToString();
				else
					_sessionSecret = Instance.Configuration.Options["sessionSecret"].ToString();


				//if (Instance.Configuration.Options["serviceType"] == null)
				//    _serviceType = Instance.ParentInstance.Configuration.Options["serviceType"].ToString();
				//else
				//    _serviceType = Instance.Configuration.Options["serviceType"].ToString();
				_channelID =System.Convert.ToInt32( Instance.Configuration.Options["ChannelID"]);
				connSession = new myFacebook.Session.ConnectSession(_apiKey, _ap_secret);
				connSession.SessionKey = _session;
				connSession.SessionSecret = _sessionSecret;


				_facebookAPI = new myFacebook.Rest.Api(connSession);

				_facebookAPI.Auth.Session.SessionExpires = false;


			}
			catch (Exception ex)
			{
				Easynet.Edge.Core.Utilities.Log.Write("Error on Init FacebookUpdateCampaignStatus: " + ex.Message, Easynet.Edge.Core.Utilities.LogMessageType.Error);

			}


		}
		protected override ServiceOutcome DoWork()
		{
			//Get the hour of the Day
			ServiceOutcome outcome = ServiceOutcome.Success;
			int hourOfDay = DateTime.Now.Hour;
			int today = Convert.ToInt32(DateTime.Now.DayOfWeek);
			if (today == 0)
				today = 7;
			

			StringBuilder campaign_specs = new StringBuilder();



			//prepere sqlstatment by time of day get all campaings by time and status != null and 1

			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand(string.Format(@"SELECT T2.campaignid,T0.Hour{0} 
																				FROM Campaigns_Scheduling T0
																				INNER JOIN User_GUI_Account T1 ON T0.Account_ID=T1.Account_ID
																				INNER JOIN UserProcess_GUI_PaidCampaign T2 ON T0.Campaign_GK=T2.Campaign_GK
																				WHERE T0.Day=@Day:Int AND T0.Account_ID=@Account_ID:Int 
																				AND (T0.Hour{0} =1 OR T0.Hour{0}=2) AND
																				T2.Channel_ID=@Channel_ID:Int AND T1.Status!=0 AND T2.campStatus<>3 AND T2.ScheduleEnabled=1", hourOfDay.ToString().PadLeft(2, '0')));
				sqlCommand.Parameters["@Day"].Value = today;
				sqlCommand.Parameters["@Account_ID"].Value = this.Instance.AccountID;
				sqlCommand.Parameters["@Channel_ID"].Value = _channelID;


				using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
				{
					while (sqlDataReader.Read())
					{
						long campaign_ID = Convert.ToInt64(sqlDataReader[0]);
						int campaign_status = Convert.ToInt32(sqlDataReader[1]);
						campaign_specs.Append("{\"campaign_id\":" + campaign_ID.ToString() + ",\"campaign_status\":" + campaign_status.ToString() + "},");



					}
				}
			}

		
			/* For test only - capaign with deleted status
			campaign_specs.Append("{\"campaign_id\":" + "60028003668686853" + ",\"campaign_status\":" + "2" + "},");
			List<string> campaigns = GetCampaigns();*/
			if (!string.IsNullOrEmpty(campaign_specs.ToString()))
			{

				//Remove the last ","
				campaign_specs = campaign_specs.Remove(campaign_specs.Length - 1, 1);
				//add '[' ']' for json array
				campaign_specs = campaign_specs.Insert(0, '[');
				campaign_specs = campaign_specs.Insert(campaign_specs.Length, ']');

				string Errors = UpdateStatus(campaign_specs.ToString());
				if (!string.IsNullOrEmpty(Errors))
				{
					
					Easynet.Edge.Core.Utilities.Log.Write(Errors, Easynet.Edge.Core.Utilities.LogMessageType.Error);
					outcome = ServiceOutcome.Failure;


				}

			}
			return outcome;


		}

		private List<string> GetCampaigns()
		{
			List<string> errors = new List<string>();


			System.Xml.XmlDocument retXml = new System.Xml.XmlDocument();
			string result;
			_parameterList.Clear();

			_parameterList.Add("account_id", _FBaccountID);

			_parameterList.Add("method", "facebook.Ads.getCampaigns");
			_parameterList.Add("include_deleted", "true");
			try
			{
				result = _facebookAPI.Application.SendRequest(_parameterList);


				//get the ones who failed and return them on a list ;
				retXml.LoadXml(result);
			}
			catch (Exception ex)
			{
				Easynet.Edge.Core.Utilities.Log.Write("Error on respond from facebook", ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
				throw new Exception("Error on respond from facebook", ex);
			}
			foreach (XmlNode errorCampaign in retXml.ChildNodes[1].ChildNodes[1].ChildNodes)
			{
				errors.Add(errorCampaign.InnerText);
			}

			return errors;


		}
		private string UpdateStatus(string campaign_specs)
		{
			StringBuilder errors = new StringBuilder();
			System.Xml.XmlDocument retXml = new System.Xml.XmlDocument();
			string result;
			_parameterList.Clear();

			_parameterList.Add("account_id", _FBaccountID);
			_parameterList.Add("campaign_specs", campaign_specs);
			_parameterList.Add("method", "facebook.Ads.updateCampaigns");
			try
			{
				result = _facebookAPI.Application.SendRequest(_parameterList);
				retXml.LoadXml(result);

				//get the ones who failed and return them on a list ;
			}
			catch (Exception ex)
			{
				Easynet.Edge.Core.Utilities.Log.Write("Error on respond from facebook", ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
				throw new Exception("Error on respond from facebook", ex);
			}
			foreach (XmlNode errorCampaign in retXml.DocumentElement["failed_campaigns"].ChildNodes)
			{
				errors.AppendFormat("{0},", errorCampaign.Attributes[0].Value);
			}
			if (!retXml.DocumentElement["updated_campaigns"].HasChildNodes)
			{
				errors.Append("All campaigns status were not updated from unknown reason");
			}
			if (!string.IsNullOrEmpty(errors.ToString()))
				errors.Insert(0, "Failed Campaigns: ");

			return errors.ToString();

		}

	}

}