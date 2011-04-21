using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Easynet.Edge.Services.Reports
{
	public class FileUtils
	{
		string _fileName = "ConduitReport" + DateTime.Now.ToString("yyyyMMddhhmmssffffff");
		public string CreateUnicode(List<ReportRowEntity> report, string _path)
		{
			if (!string.IsNullOrEmpty(_path))
			{
				var _headers = CreateHeaders();
				var fi = new FileInfo(_path + _fileName + ".csv");
				using (var stream = fi.Open(FileMode.Create, FileAccess.Write))
				{
					using (StreamWriter sw = new StreamWriter(stream, Encoding.Unicode))
					{
						//Write Headers to file
						sw.WriteLine(_headers);

						//Write Rows to file
						foreach (ReportRowEntity row in report)
						{
							StringBuilder _row = new StringBuilder();
							_row.Append(row.DayCode);
							_row.Append("\t"+row.Campaign);
							_row.Append("\t" + row.AdGroup);
							_row.Append("\t" + row.DestUrl);
							_row.Append("\t" + row.Imps.ToString());
							_row.Append("\t" + row.Clicks.ToString());
							_row.Append("\t" + row.CTR.ToString());
							_row.Append("\t" + row.Cost.ToString());
							_row.Append("\t" + row.SignUpConv.ToString());
							sw.WriteLine(_row);
						}
						sw.Close();
					}
				}
				return _path + _fileName + ".csv";
			}
			return "";
		}

		private string CreateHeaders()
		{
			StringBuilder sb = new StringBuilder();
			List<string> Row = new List<string>();

			//TO DO:  Get Headers from config file section conduit report email
	
			List<string> Headers = new List<string>(){
				"Date","Campaign","Ad Group","Destination URL","Impressions","Clicks","CTR","Cost","Sign-up Conv. (many-per-click)"
			};
			foreach (var h in Headers)
			{
				sb.Append(h + "\t");
			}
			return sb.ToString();
		}
	}
}
