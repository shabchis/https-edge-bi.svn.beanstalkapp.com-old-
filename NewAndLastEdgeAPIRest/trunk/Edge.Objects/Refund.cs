using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Edge.Core.Configuration;
using System.Data.SqlClient;

namespace Edge.Objects
{
	public class Refund
	{
		[FieldMap("AccountID")]
		public int AccountID;

		[FieldMap("ChannelID")]
		public int ChannelID;

		[FieldMap("Month")]
		public DateTime Month;

		[FieldMap("RefundAmount")]
		public decimal RefundAmount;

		public void AddRefund()
		{
			string command = "SP_Add_Refund_per_Account(@AccountID:Int,@ChannelID:Int,@Month:datetime, @RefundAmount:decimal)";
			SqlConnection sqlConnection=new SqlConnection(AppSettings.Get(string.Empty, "DWH.ConnectionString").ToString());
			sqlConnection.Open();
			MapperUtility.SaveOrRemoveSimpleObject<Refund>(command, System.Data.CommandType.StoredProcedure, SqlOperation.Insert,this, sqlConnection,null);
		}

		public void DeleteRefund()
		{
			string command = "SP_Delete_Refund_per_Account(@AccountID:Int,@ChannelID:Int,@Month:datetime)";
			SqlConnection sqlConnection = new SqlConnection(AppSettings.Get(string.Empty, "DWH.ConnectionString").ToString());
			sqlConnection.Open();
			MapperUtility.SaveOrRemoveSimpleObject<Refund>(command, System.Data.CommandType.StoredProcedure, SqlOperation.Insert, this, sqlConnection,null);
		}
	}
}
