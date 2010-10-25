using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Reflection;

namespace EdgeBI.Web.DataServices
{
	public class DataObj
	{
		public DataObj(IDataRecord record)
		{
			Dictionary<string,int> indexes = new Dictionary<string,int>();
			for (int i = 0; i < record.FieldCount; i++)
				indexes.Add(record.GetName(i), i);

			Type t = this.GetType();
			foreach (FieldInfo field in t.GetFields())
			{
				int column;
				if (!indexes.TryGetValue(field.Name, out column))
					continue;
				if (!record.IsDBNull(column))
					field.SetValue(this, record.GetValue(column));
			}
		}
	}

	public class Measure:DataObj
	{
		public Measure(IDataRecord record) : base(record) { }

		public int MeasureID;
		public int AccountID;
		public int TargetMeasureID;
		public string DWH_ProcessedTable;
		public string OLTP_Table;
		public string DWH_Table;
		public string DWH_Name;
		public string FieldName;
		public string DWH_AggregateFunction;
		public string StringFormat;
		public string DisplayName;
		public bool IsBo;
		public bool IsAbsolute;
		public int IsAdTest;
		public bool IsTarget;
		public bool IsDashboard;
        public List<Measure> FunctionMeasures;
	}

	public class Channel:DataObj
	{
		public Channel(IDataRecord record) : base(record) { }
		public int ChannelID;
		public string ChannelName;
	}

	public class ObjData
	{
        public Int64 ID;
        public string Name;
        public List<ObjValue> Values;
	}

    public class ObjValue
    {
        public string FieldName;
        public string ValueData;
    }



}
