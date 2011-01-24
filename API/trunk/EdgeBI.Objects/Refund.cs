using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Configuration;

namespace EdgeBI.Objects
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
			MapperUtility.SaveOrRemoveSimpleObject<Refund>(command, System.Data.CommandType.StoredProcedure, SqlOperation.Insert, this,AppSettings.GetAbsolute("DWH.ConnectionString").ToString());
		}

		public void DeleteRefund()
		{
			string command = "SP_Delete_Refund_per_Account(@AccountID:Int,@ChannelID:Int,@Month:datetime)";
			MapperUtility.SaveOrRemoveSimpleObject<Refund>(command, System.Data.CommandType.StoredProcedure, SqlOperation.Insert, this, AppSettings.GetAbsolute("DWH.ConnectionString").ToString());
		}
	}
}
