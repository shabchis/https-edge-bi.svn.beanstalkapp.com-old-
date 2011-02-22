using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using myFacebook = Facebook;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;

namespace Services.Facebook.UpdateCampaignStatus
{
	public class FacebookUpdateCampaignStatus : Service
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
			ServiceOutcome outcome=ServiceOutcome.Success;
			int hourOfDay = DateTime.Now.Hour;
			int today=DateTime.Now.Day;
			StringBuilder campaign_specs=null;



			//prepere sqlstatment by time of day get all campaings by time and status != null and 1

			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand(string.Format(@"SELECT T0.Campaign_GK,T0.Hour{0} 
																				FROM Facebook_Campaign_StatusByTime T0
																				INNER JOIN User_GUI_Account T1 ON T0.Account_ID=T1.Account_ID
																				INNER JOIN UserProcess_GUI_PaidCampaign T2 ON T0.Campaign_GK=T2.Campaign_GK
																				WHERE T0.Day=@Day:Int AND T0.Account_ID=@Account_ID:Int AND
																				T0.Channel_ID=6 AND T1.Status!=0", hourOfDay));
				using (SqlDataReader sqlDataReader=sqlCommand.ExecuteReader())
				{
					while (sqlDataReader.Read())
					{
						int campaign_ID=Convert.ToInt32(sqlDataReader[0]);
						int campaign_status=Convert.ToInt32(sqlDataReader[1]);
						campaign_specs.AppendLine(string.Format("{\"campaign_id\":\"{0}\",\"campaign_status\":\"{1}\"},",campaign_ID ,campaign_status ));
						
						
					}					
				}				
			}
			if (!string.IsNullOrEmpty(campaign_specs.ToString()))
			{
				//Remove the last ","
				campaign_specs.Remove(campaign_specs.Length - 1, 1);
				//add '[' ']' for json array
				campaign_specs.Insert(0, '[');
				campaign_specs.Insert(campaign_specs.Length - 1, ']');
				string result = UpdateCampaignStatus(campaign_specs.ToString());
			}
			return outcome;

			
		}
		private string UpdateCampaignStatus(string campaign_specs)
		{
			
			_parameterList.Clear();
			
			_parameterList.Add("account_id", _FBaccountID);

			_parameterList.Add("method", "facebook.Ads.updateCampaigns");			

			_parameterList.Add("campaign_specs", campaign_specs);
			string res = _facebookAPI.Application.SendRequest(_parameterList);
			System.Xml.XmlDocument retXml = new System.Xml.XmlDocument();
			retXml.LoadXml(res);
			return retXml.ChildNodes[1].ChildNodes[1].Attributes[0].Value;
		}
		
	}
	
}
