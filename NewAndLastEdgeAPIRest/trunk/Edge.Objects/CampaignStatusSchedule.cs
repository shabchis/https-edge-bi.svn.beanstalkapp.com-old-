using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;

namespace Edge.Objects
{
	[DataContract]
	[TableMap("Facebook_Campaign_StatusByTime")]
	public class CampaignStatusSchedule
	{
		[DataMember(Order = 0)]
		[FieldMap("Account_ID")]
		public int AccountID;

		[DataMember(Order = 1)]
		[FieldMap("Campaign_GK")]
		public int Campaign_GK;

		[DataMember(Order = 2)]
		[FieldMap("Day")]
		public int Day;

		[DataMember(Order = 3)]
		[FieldMap("Channel_ID")]
		public int Channel_ID;

		[DataMember(Order = 4)]
		[FieldMap("ScheduleEnabled")]
		public bool ScheduleEnabled;

		[DataMember(Order = 5)]
		[FieldMap("Hour00")]
		public CampaignStatus Hour00 = 0;
		[DataMember(Order = 6)]
		[FieldMap("Hour01")]
		public CampaignStatus Hour01 = 0;

		[DataMember(Order = 7)]
		[FieldMap("Hour02")]
		public CampaignStatus Hour02 = 0;

		[DataMember(Order = 8)]
		[FieldMap("Hour03")]
		public CampaignStatus Hour03 = 0;

		[DataMember(Order = 9)]
		[FieldMap("Hour04")]
		public CampaignStatus Hour04 = 0;

		[DataMember(Order = 10)]
		[FieldMap("Hour05")]
		public CampaignStatus Hour05 = 0;

		[DataMember(Order = 11)]
		[FieldMap("Hour06")]
		public CampaignStatus Hour06 = 0;

		[DataMember(Order = 12)]
		[FieldMap("Hour07")]
		public CampaignStatus Hour07 = 0;

		[DataMember(Order = 13)]
		[FieldMap("Hour08")]
		public CampaignStatus Hour08 = 0;

		[DataMember(Order = 14)]
		[FieldMap("Hour09")]
		public CampaignStatus Hour09 = 0;

		[DataMember(Order = 15)]
		[FieldMap("Hour10")]
		public CampaignStatus Hour10 = 0;

		[DataMember(Order = 16)]
		[FieldMap("Hour11")]
		public CampaignStatus Hour11 = 0;

		[DataMember(Order = 17)]
		[FieldMap("Hour12")]
		public CampaignStatus Hour12 = 0;

		[DataMember(Order = 18)]
		[FieldMap("Hour13")]
		public CampaignStatus Hour13 = 0;

		[DataMember(Order = 19)]
		[FieldMap("Hour14")]
		public CampaignStatus Hour14 = 0;

		[DataMember(Order = 20)]
		[FieldMap("Hour15")]
		public CampaignStatus Hour15 = 0;

		[DataMember(Order = 21)]
		[FieldMap("Hour16")]
		public CampaignStatus Hour16 = 0;

		[DataMember(Order = 22)]
		[FieldMap("Hour17")]
		public CampaignStatus Hour17 = 0;

		[DataMember(Order = 23)]
		[FieldMap("Hour18")]
		public CampaignStatus Hour18 = 0;

		[DataMember(Order = 24)]
		[FieldMap("Hour19")]
		public CampaignStatus Hour19 = 0;

		[DataMember(Order = 25)]
		[FieldMap("Hour20")]
		public CampaignStatus Hour20 = 0;

		[DataMember(Order = 26)]
		[FieldMap("Hour21")]
		public CampaignStatus Hour21 = 0;

		[DataMember(Order = 27)]
		[FieldMap("Hour22")]
		public CampaignStatus Hour22 = 0;

		[DataMember(Order = 28)]
		[FieldMap("Hour23")]
		public CampaignStatus Hour23 = 0;



		public static void Update(List<CampaignStatusSchedule> campaignStatusSchedules)
		{
			string command;
			SqlTransaction sqlTransaction = null;
			SqlConnection sqlConnection = new SqlConnection(DataManager.ConnectionString);
			sqlConnection.Open();
			sqlTransaction = sqlConnection.BeginTransaction("Schedule");
			command = "DELETE FROM Facebook_Campaign_StatusByTime WHERE Campaign_GK=@Campaign_GK:int";
			MapperUtility.SaveOrRemoveSimpleObject<CampaignStatusSchedule>(command, System.Data.CommandType.Text, SqlOperation.Delete, campaignStatusSchedules[0], sqlConnection, sqlTransaction);
			command = @"CampaignStatusSchedule_Insert(@Action:Int,@Account_ID:int,
           @Campaign_GK:int,
           @Channel_ID:int,
           @Day:int,
           @Hour00:int,
           @Hour01:int,
           @Hour02:int,
           @Hour03:int,
           @Hour04:int,
           @Hour05:int,
           @Hour06:int,
           @Hour07:int,
           @Hour08:int,
           @Hour09:int,
           @Hour10:int,
           @Hour11:int,
           @Hour12:int,
           @Hour13:int,
           @Hour14:int,
           @Hour15:int,
           @Hour16:int,
           @Hour17:int,
           @Hour18:int,
           @Hour19:int,
           @Hour20:int,
           @Hour21:int,
           @Hour22:int,
           @Hour23:int)";
			foreach (CampaignStatusSchedule campaignStatusSchedule in campaignStatusSchedules)
			{
				MapperUtility.SaveOrRemoveSimpleObject<CampaignStatusSchedule>(command, System.Data.CommandType.StoredProcedure, SqlOperation.Update, campaignStatusSchedule, sqlConnection, sqlTransaction);
			}
			command = @"UPDATE UserProcess_GUI_PaidCampaign
						SET ScheduleEnabled=@ScheduleEnabled:bit
						WHERE Campaign_GK= @Campaign_GK:int";
				;
				MapperUtility.SaveOrRemoveSimpleObject<CampaignStatusSchedule>(command, System.Data.CommandType.Text, SqlOperation.Update, campaignStatusSchedules[0], sqlConnection, sqlTransaction);
			sqlTransaction.Commit();
		}


		public static List<CampaignStatusSchedule> GetCampaignStatusSchedules(long campaignID)
		{
			List<CampaignStatusSchedule> campaignStatusSchedules = new List<CampaignStatusSchedule>();
			ThingReader<CampaignStatusSchedule> thingReader;
			//Func<FieldInfo, IDataRecord, object> customApply = CustomApply;
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand("CampaignStatusSchedules_GetByCampaignGK(@Campaign_GK:Int", System.Data.CommandType.StoredProcedure);
				sqlCommand.Parameters["@Campaign_GK"].Value = campaignID;


				thingReader = new ThingReader<CampaignStatusSchedule>(sqlCommand.ExecuteReader(), null);
				while (thingReader.Read())
				{
					campaignStatusSchedules.Add((CampaignStatusSchedule)thingReader.Current);
				}
			}

			return campaignStatusSchedules;
		}
	}
	public enum CampaignStatus
	{
		IGNORE = 0,
		ACTIVE = 1,
		PAUSED = 2,
		DELETED = 3 //NOT USED!


	}
}
