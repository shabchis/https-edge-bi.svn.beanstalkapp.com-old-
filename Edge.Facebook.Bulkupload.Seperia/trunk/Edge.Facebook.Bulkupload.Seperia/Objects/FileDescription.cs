using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Edge.Facebook.Bulkupload.Objects
{
	public class FileDescription
	{
		public Dictionary<int, ColumnDescriptionAndValues> Settings = new Dictionary<int, ColumnDescriptionAndValues>();
	}
	public class ColumnDescriptionAndValues
	{
		public string ColumnName;
		public string SettingName;
		public List<string> values = new List<string>();
		public int? from;
		public int? PadLeftLength = null;

	}
}