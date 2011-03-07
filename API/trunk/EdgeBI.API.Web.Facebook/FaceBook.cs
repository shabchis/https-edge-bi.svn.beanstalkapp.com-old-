using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Activation;
using EdgeBI.Objects;
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;
using System.ServiceModel.Web;

namespace EdgeBI.API.Web
{
	[ServiceContract]
	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class Facebook
	{
		[WebGet(UriTemplate = "accounts/{accountID}/campaigns/channels/{channelID}")]
		public List<Campaign> GetCampaignsByAccountIdAndChannel(string accountID, string channelID)
		{
			List<Campaign> campaigns = new List<Campaign>();
			try
			{
			campaigns= Campaign.GetCampaignsByAccountIdAndChannel( int.Parse(accountID),int.Parse(channelID));
			}
			catch (Exception ex)
			{
				ErrorMessageInterceptor.ThrowError(System.Net.HttpStatusCode.Forbidden, ex);
			}
			return campaigns;
		}
		[WebInvoke(Method = "POST", UriTemplate = "CampaignStatusSchedule")]
		public void ScheduleCampaigns(List<CampaignStatusSchedule> campaignStatusSchedules)
		{
			try
			{
				CampaignStatusSchedule.Update(campaignStatusSchedules);
			}
			catch (Exception ex)
			{
				ErrorMessageInterceptor.ThrowError(System.Net.HttpStatusCode.Forbidden, ex);
			}
		}
		[WebGet(UriTemplate = "CampaignStatusSchedule/{campaignGK}")]
		public List<CampaignStatusSchedule> GetCampaignStatusSchedulesBYcampaignGK(string campaignGK)
		{
			List<CampaignStatusSchedule> campaignStatusSchedules = new List<CampaignStatusSchedule>();
			try
			{
			return CampaignStatusSchedule.GetCampaignStatusSchedules(int.Parse(campaignGK));
			}
			catch (Exception ex)
			{
				ErrorMessageInterceptor.ThrowError(System.Net.HttpStatusCode.Forbidden, ex);
			}
			return campaignStatusSchedules;
		}


	}
}
