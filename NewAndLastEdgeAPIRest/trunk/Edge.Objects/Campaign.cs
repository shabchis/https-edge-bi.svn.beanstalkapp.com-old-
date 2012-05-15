using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Edge.Core.Data;
using System.Data.SqlClient;

namespace Edge.Objects
{
	[DataContract]
	[TableMap("UserProcess_GUI_PaidCampaign")]
	public class Campaign
	{
		[DataMember(Order = 0)]
		[FieldMap("Campaign_GK")]
		public int Campaign_GK;

		[DataMember(Order =1)]
		[FieldMap("campaignid")]
		public long Campaign_ID;

		[DataMember(Order = 2)]
		[FieldMap("campaign")]
		public string Campaign_Name;

		[DataMember(Order = 3)]
		[FieldMap("Account_ID")]
		public int Account_ID;

		[DataMember(Order = 4)]
		[FieldMap("Channel_ID")]
		public int Channel_ID;

		[DataMember(Order = 5)]
		[FieldMap("campStatus")]
		public int Campaign_Status;

		[DataMember(Order = 6)]
		[FieldMap("ScheduleEnabled")]
		public bool ScheduleEnabled;






		public static List<Campaign> GetCampaignsByAccountIdAndChannel(int accountID, int channelID)
		{
			List<Campaign> campaigns = new List<Campaign>();
			ThingReader<Campaign> thingReader;
			//Func<FieldInfo, IDataRecord, object> customApply = CustomApply;
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand("CampaignByAccountAndChannel(@Account_ID:Int,@Channel_ID:Int",System.Data.CommandType.StoredProcedure);

				sqlCommand.Parameters["@Account_ID"].Value = accountID;
				sqlCommand.Parameters["@Channel_ID"].Value = channelID;

				thingReader = new ThingReader<Campaign>(sqlCommand.ExecuteReader(), null);
				while (thingReader.Read())
				{
					campaigns.Add((Campaign)thingReader.Current);
				}
			}
			
			return campaigns;
		}
	}
}
