using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Services.Reports.ConduitConversionReport
{
	public class ReportRowEntity
	{
		public ReportRowEntity()
		{

		}
		public ReportRowEntity(SqlDataReader reader)
		{
			DayCode = Convert.ToUInt64(reader[0]);
			Campaign = Convert.ToString(reader[1]);
			AdGroup = Convert.ToString(reader[2]);
			DestUrl = Convert.ToString(reader[3]);
			Imps = Convert.ToUInt64(reader[4]);
			Clicks = Convert.ToUInt64(reader[5]);
			CTR = Convert.ToUInt64(reader[6]);
			Cost = Convert.ToUInt64(reader[7]);
			SignUpConv = Convert.ToUInt64(reader[8]);
		}

		UInt64 DayCode { get; set; }
		String Campaign { get; set; }
		String AdGroup { get; set; }
		String DestUrl { get; set; }
		UInt64 Imps { get; set; }
		UInt64 Clicks { get; set; }
		UInt64 CTR { get; set; }
		UInt64 Cost { get; set; }
		UInt64 SignUpConv { get; set; }

	}
}
