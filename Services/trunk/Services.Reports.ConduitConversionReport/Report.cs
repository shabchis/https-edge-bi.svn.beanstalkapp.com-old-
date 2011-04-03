using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Easynet.Edge.Services.Reports
{
	public class Report
	{
		public Report()
		{
			rows = new List<ReportRowEntity>();
		}
		public void AddRow(SqlDataReader reader)
		{
			ReportRowEntity row = new ReportRowEntity(reader);
			rows.Add(row);
		}

		List<ReportRowEntity> rows;
	}
	
}
