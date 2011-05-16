using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Easynet.Edge.Services.Reports
{
	public class ReportRowEntity
	{
		public ReportRowEntity()
		{

		}
		public ReportRowEntity(SqlDataReader reader)
		{
			DayCode = Convert.ToString(reader[0]);
			Campaign = Convert.ToString(reader[1]);
			AdGroup = Convert.ToString(reader[2]);
			DestUrl = Convert.ToString(reader[3]);
			Imps = Convert.ToUInt64(reader[4]);
			Clicks = Convert.ToUInt64(reader[5]);
			CTR = Convert.ToUInt64(reader[6]);
			AvgCPC = Convert.ToUInt64(reader[7]);
			Cost = Convert.ToUInt64(reader[8]);
			AvgPosition = Convert.ToUInt64(reader[9]);
			SignUpConv = Convert.ToUInt64(reader[10]);
		}

		public String DayCode { get; set; }
		public String Campaign { get; set; }
		public String AdGroup { get; set; }
		public String DestUrl { get; set; }
		public UInt64 Imps { get; set; }
		public UInt64 Clicks { get; set; }
		public UInt64 CTR { get; set; }
		public UInt64 Cost { get; set; }
		public UInt64 AvgCPC { get; set; }
		public UInt64 AvgPosition { get; set; }
		public UInt64 SignUpConv { get; set; }
		 
			

	}
}
