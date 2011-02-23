using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Activation;
using EdgeBI.Objects;
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;

namespace EdgeBI.API.Web.Facebook
{
	[ServiceContract]
	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class Facebook
	{

		public List<Campaign> GetCampaignsByAccountIdAndChannel(int accountID,int channelID)
		{
			List<Campaign> campaigns = new List<Campaign>();
			try
			{
			campaigns= Campaign.GetCampaignsByAccountIdAndChannel(accountID);
			}
			catch (Exception ex)
			{
				ErrorMessageInterceptor.ThrowError(System.Net.HttpStatusCode.Forbidden, ex);
			}
			return campaigns;
		}
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
		public List<CampaignStatusSchedule> GetCampaignStatusSchedules(long campaignID)
		{
			List<CampaignStatusSchedule> campaignStatusSchedules = new List<CampaignStatusSchedule>();
			try
			{
			return CampaignStatusSchedule.GetCampaignStatusSchedules(campaignID);
			}
			catch (Exception ex)
			{
				ErrorMessageInterceptor.ThrowError(System.Net.HttpStatusCode.Forbidden, ex);
			}
			return campaignStatusSchedules;

		}


	}
}
