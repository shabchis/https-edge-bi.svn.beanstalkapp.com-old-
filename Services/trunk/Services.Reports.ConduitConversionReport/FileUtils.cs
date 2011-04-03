using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Easynet.Edge.Services.Reports
{
	public class FileUtils
	{
		public string CreateUnicode(List<ReportRowEntity> report, string _path)
		{
			if (!string.IsNullOrEmpty(_path))
			{

				var _headers = CreateHeaders();
				var fi = new FileInfo(_path + "filename" + ".xls");
				using (var stream = fi.Open(FileMode.Create, FileAccess.Write))
				{
					using (StreamWriter sw = new StreamWriter(stream, Encoding.Unicode))
					{
						sw.WriteLine(_headers);
						//Create Data Row
						foreach (ReportRowEntity row in report)
						{
							
							StringBuilder _row = new StringBuilder();
							//_row.Append(); // AccountName
							//_row.Append("\t" + _curDate.ToString());
							//_row.Append("\t" + gw.ToString()); 
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
				"AccountName","Day_Code","Gateway_id"
			};
			foreach (var h in Headers)
			{
				sb.Append(h + "\t");
			}
			return sb.ToString();
		}
	}
}
