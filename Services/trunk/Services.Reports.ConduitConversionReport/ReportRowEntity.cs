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
			Imps = Convert.ToString(reader[4]);
			Clicks = Convert.ToString(reader[5]);
			CTR = Convert.ToString(reader[6]);
			AvgCPC = Convert.ToString(reader[7]);
			Cost = Convert.ToString(reader[8]);
			AvgPosition = Convert.ToString(reader[9]);
			SignUpConv = Convert.ToString(reader[10]);
		}

		public String DayCode { get; set; }
		public String Campaign { get; set; }
		public String AdGroup { get; set; }
		public String DestUrl { get; set; }
		public String Imps { get; set; }
		public String Clicks { get; set; }
		public String CTR { get; set; }
		public String Cost { get; set; }
		public String AvgCPC { get; set; }
		public String AvgPosition { get; set; }
		public String SignUpConv { get; set; }
		 
			

	}
}
