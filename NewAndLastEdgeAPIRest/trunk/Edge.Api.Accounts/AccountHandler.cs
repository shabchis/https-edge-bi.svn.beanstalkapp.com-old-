using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Edge.Api.Handlers.Template;
using Edge.Objects;
using System.Net;
using System.ServiceModel;
using Edge.Core.Scheduling.Objects;
using Legacy = Edge.Core.Services;

namespace Edge.Api.Accounts.Handlers
{
	public class AccountHandler : TemplateHandler
	{
		#region Accounts

		[UriMapping(Method = "GET", Template = "accounts/{accountID}")]
		public Account GetAccountByID(string accountID)
		{
			List<Account> acc = null;

			int currentUser;
			currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
			int? accId = int.Parse(accountID);
			acc = Account.GetAccount(accId, true, currentUser);
			if (acc.Count == 0)
				throw new HttpStatusException(String.Format("No account with permission found for user {0}", currentUser), HttpStatusCode.NotFound);





			return acc[0];
		}

		[UriMapping(Method = "GET", Template = "accounts")]
		public List<Account> GetAccountS()
		{
			List<Account> acc = null;

			int currentUser;
			currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
			acc = Account.GetAccount(null, true, currentUser);
			if (acc.Count == 0)
				throw new HttpStatusException(String.Format("No account with permission found for user {0}", currentUser), HttpStatusCode.NotFound);

			return acc;
		}
		#endregion

		#region Campaigns
		[UriMapping(Method = "GET", Template = "accounts/{accountID}/campaigns?channel={channelID}")]
		public List<Campaign> GetCampaignsByAccountIdAndChannel(string accountID, string channelID)
		{
			List<Campaign> campaigns = new List<Campaign>();

			campaigns = Campaign.GetCampaignsByAccountIdAndChannel(int.Parse(accountID), int.Parse(channelID));


			return campaigns;
		}
		[UriMapping(Method = "POST", Template = "accounts/{accountID}/campaigns/{campaignGK}", BodyParameter = "campaignStatusSchedules")]
		public void ScheduleCampaigns(List<CampaignStatusSchedule> campaignStatusSchedules, string accountID, string campaignGK)
		{

			CampaignStatusSchedule.Update(campaignStatusSchedules);

		}
		[UriMapping(Method = "GET", Template = "accounts/{accountID}/campaigns/{campaignGK}")]
		public List<CampaignStatusSchedule> GetCampaignStatusSchedulesBYcampaignGK(string accountID, string campaignGK)
		{
			List<CampaignStatusSchedule> campaignStatusSchedules = new List<CampaignStatusSchedule>();

			return CampaignStatusSchedule.GetCampaignStatusSchedules(int.Parse(campaignGK));

			return campaignStatusSchedules;
		}


		#endregion

		#region Services
		[UriMapping(Method = "POST", Template = "accounts/{accountid}/Services/{servicename}?options={options}")] 
		public Guid RunService(string accountid, string servicename, string options)
		{
			Guid guid;
			ChannelFactory<ISchedulingCommunication> channel = new ChannelFactory<ISchedulingCommunication>("SeperiaSchedulerCommunication");
			ISchedulingCommunication schedulingCommunication = channel.CreateChannel();
			Dictionary<string, string> settings = new Dictionary<string, string>();
			string[] settingsArray = options.Split(':');
			foreach (var setting in settingsArray)
			{
				string[] keyValue = setting.Split('=');
				settings.Add(keyValue[0], keyValue[1]);


			}
			guid = schedulingCommunication.AddUnplanedService(int.Parse(accountid), servicename, settings, DateTime.Now);
			return guid;
		}
		[UriMapping(Method = "GET", Template = "accounts/{accountid}/Services?guid={guid}")] 
		public Legacy.IsAlive GetStatus(string accountid,string guid)
		{
			Guid unplannedGuid=Guid.Parse(guid);
			Legacy.IsAlive status;
			ChannelFactory<ISchedulingCommunication> channel = new ChannelFactory<ISchedulingCommunication>("SeperiaSchedulerCommunication");
			ISchedulingCommunication schedulingCommunication = channel.CreateChannel();
			status = schedulingCommunication.IsAlive(unplannedGuid);



			return status;
		}

		#endregion



		public override bool ShouldValidateSession
		{
			get { return true; }
		}
	}
	[ServiceContract(SessionMode = SessionMode.Required)]
	public interface ISchedulingCommunication
	{
		[OperationContract]
		void Subscribe();
		[OperationContract]
		Legacy.IsAlive IsAlive(Guid guid);	
		[OperationContract]
		void Abort(Guid guid);
		[OperationContract]
		void ResetUnEnded();
		[OperationContract]
		List<AccounServiceInformation> GetServicesConfigurations();
		[OperationContract]
		Guid AddUnplanedService(int accountID, string serviceName, Dictionary<string, string> options, DateTime targetDateTime);
	}
}
