using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Reflection;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;
using System.Data;
using Easynet.Edge.Core.Configuration;
using System.Collections;
using System.Configuration;

namespace EdgeBI.Web.DataServices
{
    public class DataHandler
    {
        Dictionary<string, string> DicMeasuresFormat = new Dictionary<string, string>();
        List<Measure> Measures = new List<Measure>();
        public List<ObjData> GetData(
            int accountID,
            DataGrouping grouping,
            int top,
            MeasureRef[] measures,
            DayCodeRange[] ranges,
            MeasureDiff[] diff,
            MeasureSort[] dataSort,
            MeasureSort[] viewSort,
            MeasureFormat[] format,
            Dictionary<int, Measure> measuresList
            )
        {
			Mode mode = GetMode(dataSort);

            Measures = new List<Measure>(measuresList.Values);
            List<ObjData> Data = new List<ObjData>();
            int measureIndex = 1 ,RangeIndex = 1;
            string Sql = null, IdsList = null;
            foreach (DayCodeRange dc in ranges)
            {
                RangeIndex = 1;
                string AggregateFunction = null , HavingString = null;
                foreach (MeasureRef mf in measures)
                {
                    string lAggregateFunction = null , lHavingString = null;
                    Measure m = measuresList[mf.MeasureID];
                    if (mf.IsTargetRef)
                        m = measuresList[m.TargetMeasureID];
                    GetFieldsSQL(m, measureIndex, RangeIndex, out lAggregateFunction, out lHavingString, measuresList);
                    AggregateFunction += "," + lAggregateFunction;
                    HavingString += HavingString != null ? " AND " + lHavingString : lHavingString;
                    DicMeasuresFormat.Add("M" + measureIndex + ".R" + RangeIndex + ".Value", m.StringFormat);
                    measureIndex++;
                }
                Sql = GetSQL(AggregateFunction.Substring(1), HavingString, accountID, dc, grouping, dataSort, top, IdsList,mode);
                if(IdsList == null)
                    GetData(out IdsList, Sql,out Data);
                else
                    GetData(Sql,out Data);
                RangeIndex++;
            }
            if (diff != null && diff.Length > 0) GetDiffCalculation(out Data, Data, diff);
            if (viewSort != null && viewSort.Length > 0 && mode == Mode.Simple) GetSortData(out Data, Data, viewSort);
            if (mode == Mode.Advanced) GetDataByDataSort(out Data, Data ,top, dataSort);
            if (Data.Count>0) GetFormateData(out Data, Data);
            return Data;
        }

        private void GetDataByDataSort(out List<ObjData> dataout, List<ObjData> datain, int top, MeasureSort[] dataSort)
        {
            GetSortData(out dataout,datain, dataSort);
            List<ObjData> RetData = new List<ObjData>();
            int i = 1;
            foreach (ObjData rd in datain)
            {
                if (top >= i)
                    RetData.Add(rd);
                else
                    break;
                i++;
            }
            dataout = RetData;
        }

        private void GetData(string sql, out List<ObjData> data)
        {
            string ids = null;
            GetData(out ids, sql,out data);
        }

        private Mode GetMode(MeasureSort[] dataSort)
        {
            Mode mode = Mode.None;
            if (dataSort != null)
            {
                foreach (MeasureSort ms in dataSort)
                    if (ms.DiffType != DiffType.None)
                        if (mode == Mode.None || mode == Mode.Simple)
                            mode = Mode.Simple;
                        else
                            throw new System.ArgumentException("Parameter not legal", "dataSort parameter");
                    else
                    {
                        if (mode == Mode.None)
                            mode = Mode.Advanced;
                        else
                            throw new System.ArgumentException("Parameter not legal", "dataSort parameter");
                    }
            }
            return mode;
        }

        private void GetData(out string idslist, string sql, out List<ObjData> data)
        {
            string ids = null;
            using (DataManager.Current.OpenConnection())
            {
                SqlCommand searchEngineCmd = DataManager.CreateCommand(ConfigurationSettings.AppSettings["EdgeBI.Web.DataServices.MeasureDataService.Commands.GetData"].ToString(), CommandType.StoredProcedure);
                searchEngineCmd.CommandTimeout = (60 * 1000) * 5;
                searchEngineCmd.Parameters["@Sql"].Value = sql;
                SqlDataReader reader = searchEngineCmd.ExecuteReader();
                Dictionary<string, ObjData> arrAllReturnData = new Dictionary<string, ObjData>();
                while (reader.Read())
                {
                    ObjData _Data = new ObjData();
                    if (arrAllReturnData.ContainsKey(reader["ID"].ToString()))
                        _Data = arrAllReturnData[reader["ID"].ToString()];
                    if (!reader["ID"].Equals(System.DBNull.Value)) _Data.ID = Convert.ToInt64(reader["ID"]);
                    if (!reader["NAME"].Equals(System.DBNull.Value)) _Data.Name = (string)reader["Name"];
                    ObjValue _value = new ObjValue();
                    _value.FieldName = reader.GetName(2).ToString();
                    if (!reader[_value.FieldName].Equals(System.DBNull.Value)) _value.ValueData = Convert.ToString(reader[_value.FieldName]);
                    if (_Data.Values == null)
                        _Data.Values = new List<ObjValue>();
                    _Data.Values.Add(_value);
                    if (arrAllReturnData.ContainsKey(reader["ID"].ToString()))
                        arrAllReturnData[reader["ID"].ToString()] = _Data;
                    else
                        arrAllReturnData.Add(reader["ID"].ToString(), _Data);
                }
                data = new List<ObjData>(arrAllReturnData.Values);
                foreach (string id in arrAllReturnData.Keys)
                    ids += "," + id;
                idslist = ids != null ? ids.Substring(1): null;
                reader.Close();
            }
            
        }

        private void GetSortData(out List<ObjData> Dataout, List<ObjData> Datain, MeasureSort[] viewSort)
        {
            List<ObjData> SortedReturnData = new List<ObjData>();
            foreach (MeasureSort msort in viewSort)
            {
                string sortBy = "M." + msort.MeasureIndex +  (msort.RangeIndex == 0 || msort.DiffType != DiffType.None  ? "Diff" + msort.DiffType.ToString(): "R." + msort.RangeIndex);
                Dictionary<Int64, double> dic = new Dictionary<Int64, double>();
                foreach (ObjData Entity in Datain)
                {
                    foreach (ObjValue lvalue in Entity.Values)
                    {
                        if (lvalue.FieldName.ToLower().Contains(sortBy))
                        {
                            dic.Add(Entity.ID, Convert.ToDouble(lvalue.ValueData));
                        }
                    }
                    List<KeyValuePair<Int64, double>> myList = new List<KeyValuePair<Int64, double>>(dic);
                                myList.Sort(
                                    delegate(KeyValuePair<Int64, double> firstPair,
                                    KeyValuePair<Int64, double> nextPair)
                                    {
                                        return firstPair.Value.CompareTo(nextPair.Value);
                                    }
                                 );
                    switch (msort.SortDir)
                    {
                        case SortDir.Asc:
                            for (int i = 0; i < myList.Count; i++)
                            {
                                SortedReturnData.Add(GetDataObject(Datain, myList[i].Key));
                            }
                            break;
                        case SortDir.Desc:
                            for (int i = myList.Count - 1; i >= 0; i--)
                            {
                                SortedReturnData.Add(GetDataObject(Datain, myList[i].Key));
                            }
                            break;
                    }
                }
            }
            Dataout = SortedReturnData;
        }

        private ObjData GetDataObject(List<ObjData> AllReturnData, Int64 Key)
        {
            ObjData RetEntity = new ObjData();
            foreach (ObjData Entity in AllReturnData)
            {
                if (Entity.ID == Key)
                {
                    RetEntity = Entity;
                    break;
                }

            }
            return RetEntity;
        }

        private void GetFieldsSQL(Measure measure, int measureIndex, int RangeIndex, out string AggregateFunction, out string HavingString, Dictionary<int, Measure> measuresList)
        {
            AggregateFunction = BuildAggregateFunction(measure, measuresList);
            HavingString = AggregateFunction + " IS NOT NULL ";
            AggregateFunction = AggregateFunction + " AS 'M" + measureIndex + ".R" + RangeIndex + ".Value'";

        }

        private string GetSQL(string Sql, string HavingString, int AccountID, DayCodeRange DaysCode, DataGrouping DataGrouping, MeasureSort[] DataSort, int Top, string IdsList,Mode mode)
        {
            string OrderBy = null, TableName = null, AggregateFunctions = null, ltop = null, Selectfields = null, Join = null, GroupBy = null, AdditionalWhere = null, BetweenDatesSql = null, AdditionalWhere2 = null;
            
            TableName = "Dwh_Fact_PPC_Campaigns_ProcessedMeasures";
            OrderBy = GetOrderBy(DataSort);
            BetweenDatesSql = " AND a.Day_ID BETWEEN " + DaysCode.From + " AND " + DaysCode.To;
            if (mode == Mode.Simple) ltop = Top <= 0 ? "" : "TOP " + Top.ToString() + " ";
            AggregateFunctions = Sql;
            GetSqlOptions(out Selectfields, out  Join, out  GroupBy, out  AdditionalWhere, out  AdditionalWhere2, DataGrouping, IdsList);
            Sql = "SELECT " + ltop + Selectfields + AggregateFunctions + " FROM " + TableName + " a inner JOIN " + Join + " WHERE a.Account_ID = " + AccountID + AdditionalWhere2 + BetweenDatesSql + " GROUP BY " + GroupBy + " HAVING " + HavingString + OrderBy;
            return Sql;
        }

        private string GetOrderBy(MeasureSort[] DataSort)
        {
            int index=0;
            string OrderBy = null;
            if (DataSort != null)
            {
                while (index < DataSort.Length)
                {
                    OrderBy += ",'M" + DataSort[index].MeasureIndex.ToString() + ".R" + DataSort[index].RangeIndex.ToString() + ".Value' " + DataSort[index].SortDir;
                    index++;
                }
                OrderBy = " ORDER BY " + OrderBy.Substring(1);
            }
            return OrderBy;
        }

        private void GetSqlOptions(out string Selectfields, out string Join, out string GroupBy, out string AdditionalWhere, out string AdditionalWhere2,  DataGrouping DataGrouping,  string IdsList)
        {
            switch (DataGrouping)
            {
                case DataGrouping.Account:
                    Selectfields = " a.Account_ID AS ID, b.Account AS NAME,";
                    Join = "Dwh_Dim_Accounts b with (nolock) ON a.Account_ID = b.Account_ID";
                    GroupBy = "a.Account_ID,b.Account";
                    AdditionalWhere = " a.Account_ID in (" + IdsList + ") ";
                    AdditionalWhere2 = "";
                    break;
                case DataGrouping.Channel:
                    Selectfields = " a.Channel_ID AS ID, b.Channel_Name AS NAME,";
                    Join = "Dwh_Dim_Channels b with (nolock) ON a.Channel_ID = b.Channel_ID";
                    GroupBy = "a.Channel_ID,b.Channel_Name";
                    AdditionalWhere = " a.Channel_ID in (" + IdsList + ") ";
                    AdditionalWhere2 = " AND a.Channel_ID > 0 ";

                    break;
                case DataGrouping.Campaign:
                    Selectfields = " a.Campaign_GK AS ID, b.Campaign AS NAME,";
                    Join = "dwh_Dim_Campaigns b with (nolock) ON a.Campaign_Gk = b.Campaign_Gk";
                    GroupBy = "a.Campaign_GK,b.Campaign";
                    AdditionalWhere = " a.Campaign_GK in (" + IdsList + ") ";
                    AdditionalWhere2 =  " AND a.Campaign_GK > 0 ";
                    break;
                default:
                    Selectfields = "";
                    Join = "";
                    GroupBy = "";
                    AdditionalWhere = "";
                    AdditionalWhere2 = "";
                    break;
            }
        }

        private string BuildAggregateFunction(Measure measure, Dictionary<int, Measure> measuresList)
        {
            string AggregateFunction =null;
            if (measure.FunctionMeasures == null)
               AggregateFunction = measure.DWH_AggregateFunction.Replace("<DWH_Name>", "a." + measure.DWH_Name);
            else
            {
                int MeasureID;
                Measure _measure = null;
                MatchCollection matches = Regex.Matches(measure.DWH_AggregateFunction, @"\<[^\>]+\>");
                foreach (Match m in matches)
                {

                    if (m.Value.ToString().ToLower().Contains("id:"))
                    {
                        int Pos = m.Value.ToString().IndexOf(">");
                        MeasureID = Convert.ToInt32(m.Value.ToString().Substring(m.Value.ToString().IndexOf(":") + 1, (Pos - 1) - m.Value.ToString().IndexOf(":")));
                        _measure = measuresList[MeasureID];
                    }
                    else if (m.Value.ToString().ToLower().Contains("param:") && m.Value.ToString().ToLower().Contains("/"))
                    {
                        int Pos1, Pos2;
                        Pos1 = m.Value.ToString().IndexOf(":");
                        Pos2 = m.Value.ToString().IndexOf("/");
                        _measure = measure.FunctionMeasures[Convert.ToInt32(m.Value.ToString().Substring(Pos1 + 1, Pos2 - Pos1 - 1)) - 1];
                        measure = measuresList[_measure.TargetMeasureID];
                    }
                    else if (m.Value.ToString().ToLower().Contains("param:"))
                    {
                        int Pos1, Pos2;
                        Pos1 = m.Value.ToString().IndexOf(":");
                        Pos2 = m.Value.ToString().IndexOf(">");
                        _measure = measure.FunctionMeasures[Convert.ToInt32(m.Value.ToString().Substring(Pos1 + 1, Pos2 - Pos1 - 1)) - 1];
                    }
                    AggregateFunction = AggregateFunction.Replace(m.Value, "a." + _measure.DWH_Name);
                }
            }
            
            return AggregateFunction;

        }

        //internal List<objData> GetData(int accountID, DataGrouping grouping, int top, MeasureRef[] measures, DayCodeRange[] ranges, MeasureDiff[] diff, MeasureSort[] dataSort, MeasureSort[] viewSort, MeasureFormat[] format, Dictionary<int, Measure> measuresByID, bool p)
        //{
        //    throw new NotImplementedException();
        //}
        
        private MeasureDiff GetMeasureDiff(string key, MeasureDiff[] diff)
        {
            foreach (MeasureDiff mdiff in diff)
            {
                if (key == Convert.ToString("M." + mdiff.MeasureIndex.ToString()) || mdiff.MeasureIndex == 0)
                    return mdiff;
            }
            return new MeasureDiff();
        }

        private void GetDiffCalculation(out List<ObjData> dataout, List<ObjData> datain, MeasureDiff[] diff)
        {
            List<ObjData> retdata = new List<ObjData>();

            foreach (ObjData Entity in datain)
            {
                Dictionary<string, ArrayList> EntityValues = new Dictionary<string, ArrayList>(); ;
                if (Entity.Values.Count > 1)
                {
                    string mIdnew = null,mIdprev = null;
                    ArrayList mValues = new ArrayList();
                    foreach (ObjValue lvalue in Entity.Values)
                    {
                        mIdprev = mIdnew;
                        mIdnew = lvalue.FieldName.Substring(0,lvalue.FieldName.IndexOf("."));
                        if(mIdnew == mIdprev || mValues.Count==0)
                            mValues.Add(lvalue.ValueData);
                        else
                        {
                            EntityValues.Add(mIdnew,mValues);
                            mValues.Clear();
                        }
                      }
                    foreach(string key in EntityValues.Keys)
                    {
                        mValues = EntityValues[key];
                        for(int i = 0; i<EntityValues.Count;i++)
                        {
                            double value1, value2;
                            value1 = Convert.ToDouble(mValues[i]);
                            value2 = Convert.ToDouble(mValues[i + 1]);
                            ObjValue value = new ObjValue();
                            if (value2 != 0)
                            {
                               MeasureDiff  mDiff = GetMeasureDiff(key,diff);
                               DiffType difftype = mDiff.DiffType ;
                               if (mDiff.MeasureIndex > -1)
                               {
                                   switch(difftype)
                                   {
                                       case DiffType.Both:
                                           value.FieldName = key + "DiffAbs" + i;
                                           value.ValueData = Convert.ToString(value1 - value2); 
                                           Entity.Values.Add(value);
                                           value.FieldName = key + "DiffRel" + i;
                                           value.ValueData = Convert.ToString(((value1 - value2) / Math.Abs(value2)) * 100); 
                                           break;
                                       case DiffType.DiffAbs:
                                           value.FieldName = key + "DiffAbs" + i;
                                           value.ValueData = Convert.ToString(value1 - value2); 
                                           break;
                                       case DiffType.DiffRel:
                                           value.FieldName = key + "DiffRel" + i;
                                           value.ValueData = Convert.ToString(((value1 - value2) / Math.Abs(value2)) * 100); 
                                           break;
                                   }
                               }
                            }
                            else
                                value.ValueData = null;
                            Entity.Values.Add(value);
                        }
                    }
                    retdata.Add(Entity);
                }
            }
            dataout = retdata;
        }

        private void GetFormateData(out List<ObjData> dataout, List<ObjData> datain)
        {
            List<ObjData> data = new List<ObjData>();
            foreach (ObjData d in datain)
            {
                ObjData rd = new ObjData();
                foreach (ObjValue value in d.Values)
                {
                    rd.Values = new List<ObjValue>();
                    string StringFormat = DicMeasuresFormat[value.FieldName];
                    if (!value.FieldName.Contains(".Diff") && value.ValueData != null)
                    {
                        if (StringFormat.Contains("C"))
                        {
                            string MinusSign = "";
                            if (Convert.ToDouble(value.ValueData) < 0) MinusSign = "-";
                            int NativeValue = decimal.ToInt32(Convert.ToDecimal(value.ValueData));
                            if (NativeValue < 0) NativeValue = NativeValue * -1;
                            value.ValueData = string.Format("{0:" + StringFormat + "}", Convert.ToDouble(value.ValueData));
                            value.ValueData = value.ValueData.Replace(Convert.ToString(NativeValue), MinusSign + Convert.ToString(NativeValue));
                        }
                        else
                            value.ValueData = string.Format("{0:" + StringFormat + ";" + StringFormat + ";0}", Convert.ToDouble(value.ValueData));
                    }
                    else if (value.FieldName.Contains(".Diff") && value.ValueData != null)
                    {
                        if (value.FieldName.Contains(".DiffAbs"))
                            value.ValueData = string.Format("{0:" + ConfigurationSettings.AppSettings["Dwh.Data.Service.ConsoleDataServices.Formating.Diff.Abs"].ToString() + "}", Convert.ToDouble(value.ValueData));//"{0:0.##}"
                        else if (value.FieldName.Contains(".DiffRel"))
                            value.ValueData = string.Format("{0:" + ConfigurationSettings.AppSettings["Dwh.Data.Service.ConsoleDataServices.Formating.Diff.Rel"].ToString() + "}", Convert.ToDouble(value.ValueData));//"{0:0.##}"
                   }
                    rd.ID = d.ID ;
                    rd.Name=d.Name ;
                    rd.Values.Add(value);
                }
                data.Add(rd);
            }
            dataout = data;
        }
    }
}
