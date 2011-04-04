using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Easynet.Edge.Services.Reports
{
	public class FileUtils
	{
		string _fileName = "ConduitReport" + DateTime.Now.ToLongDateString();
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
							_row.Append(row.Campaign);
							_row.Append(row.AdGroup);
							_row.Append(row.DestUrl);
							_row.Append(row.Imps);
							_row.Append(row.Clicks);
							_row.Append(row.CTR);
							_row.Append(row.Cost);
							_row.Append(row.SignUpConv);
							sw.WriteLine(_row);
						}
						sw.Close();
					}
				}

			}
			return "";

		}

		private string CreateHeaders()
		{
			StringBuilder sb = new StringBuilder();
			List<string> Row = new List<string>();

			//TO DO:  Get Headers from config file section conduit report email
	
			List<string> Headers = new List<string>(){
				"DayCode","Campaign","AdGroup","DestUrl","Imps","Clicks","CTR","Cost","SignUpConv"
			};
			foreach (var h in Headers)
			{
				sb.Append(h + "\t");
			}
			return sb.ToString();
		}
	}
}
