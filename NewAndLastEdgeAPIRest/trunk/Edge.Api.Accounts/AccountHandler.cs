using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Edge.Api.Handlers.Template;
using Edge.Objects;
using System.Net;

namespace Edge.Api.Accounts.Handlers
{
	public class AccountHandler : TemplateHandler
	{
		#region Accounts

		[UriMapping(Method="GET",Template = "accounts/{accountID}")]
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

		[UriMapping(Method="GET",Template = "accounts")]
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
		[UriMapping(Method="GET",Template = "accounts/{accountID}/campaigns/channels/{channelID}")]
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
		[UriMapping(Method="GET",Template = "accounts/{accountID}/campaigns/{campaignGK}")]
		public List<CampaignStatusSchedule> GetCampaignStatusSchedulesBYcampaignGK(string accountID, string campaignGK)
		{
			List<CampaignStatusSchedule> campaignStatusSchedules = new List<CampaignStatusSchedule>();
			
				return CampaignStatusSchedule.GetCampaignStatusSchedules(int.Parse(campaignGK));
			
			return campaignStatusSchedules;
		}


		#endregion



		public override bool ShouldValidateSession
		{
			get { return true; }
		}
	}
}
