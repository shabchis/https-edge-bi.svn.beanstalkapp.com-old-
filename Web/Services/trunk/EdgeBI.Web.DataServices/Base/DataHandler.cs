﻿using System;
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

namespace EdgeBI.Web.DataServices
{
    public class DataHandler
    {
        Dictionary<int, Measure> measuresByID;
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
            Dictionary<int, Measure> measuresList,
            Mode mode
            )
        {
            Measures = new List<Measure>(measuresList.Values);
            List<ObjData> Data = new List<ObjData>();
            int measureIndex = 0 ,RangeIndex = 0;
            string Sql = null, IdsList = null;
            foreach (DayCodeRange dc in ranges)
            {
                RangeIndex = 0;
                string AggregateFunction = null , HavingString = null;
                foreach(Measure m in measuresList.Values)
                {
                    string lAggregateFunction = null , lHavingString = null;
                    GetFieldsSQL(m,measureIndex,RangeIndex,out lAggregateFunction,out lHavingString);
                    AggregateFunction += "," + lAggregateFunction;
                    HavingString += "," + lHavingString;
                    measureIndex++; 
                }
                Sql = GetSQL(AggregateFunction, HavingString, accountID, dc, grouping, dataSort, top, IdsList,mode);
                if(IdsList == null)
                    GetData(out IdsList, Sql,out Data);
                else
                    GetData(Sql,out Data);
                RangeIndex++;
            }
            if (diff.Length > 0) Data = DiffCalculation(Data, diff);
            return null;
        }
        private void GetData(string sql, out List<ObjData> data)
        {
            string ids = null;
            GetData(out ids, sql,out data);
        }
        private void GetData(out string idslist, string sql, out List<ObjData> data)
        {
            string ids = null;
            using (DataManager.Current.OpenConnection())
            {
                SqlCommand searchEngineCmd = DataManager.CreateCommand(AppSettings.Get(this, "StoredProcedures.GetData"), CommandType.StoredProcedure);
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
                    if (!reader["FieldName"].Equals(System.DBNull.Value)) _value.FieldName = (string)reader["FieldName"];
                    if (!reader["VALUE"].Equals(System.DBNull.Value)) _value.ValueData = Convert.ToString(reader["VALUE"]);
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
                idslist = ids;
                reader.Close();
            }
            
        }

        private void GetFieldsSQL(Measure measure, int measureIndex, int RangeIndex,out string  AggregateFunction,out string  HavingString)
        {
            AggregateFunction = BuildAggregateFunction(measure) + " AS M" + measureIndex + ".R" + RangeIndex + ".Value";
            HavingString = AggregateFunction + " IS NOT NULL AND";
            AggregateFunction = AggregateFunction + " AS M" + measureIndex + ".R" + RangeIndex + ".Value";

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
            while (index < DataSort.Length)
            {
                OrderBy += "," + DataSort[index].MeasureIndex.ToString() + DataSort[index].RangeIndex.ToString() + ".Value" + DataSort[index].SortDir;
                index++;
            }
            OrderBy = " ORDER BY " + OrderBy.Substring(1); 
            return OrderBy;
        }

        private void GetSqlOptions(out string Selectfields, out string Join, out string GroupBy, out string AdditionalWhere, out string AdditionalWhere2,  DataGrouping DataGrouping,  string IdsList)
        {
            switch (DataGrouping)
            {
                case DataGrouping.Account:
                    Selectfields = " a.Account_ID, b.Account_Name,";
                    Join = "Dwh_Dim_Account_Domains b with (nolock) ON a.Account_ID = b.Account_ID";
                    GroupBy = "a.Account_ID,b.Account_Name";
                    AdditionalWhere = " a.Account_ID in (" + IdsList + ") ";
                    AdditionalWhere2 = "";
                    break;
                case DataGrouping.Channel:
                    Selectfields = " a.Channel_ID , b.Channel_Name,";
                    Join = "Dwh_Dim_Channels b with (nolock) ON a.Channel_ID = b.Channel_ID";
                    GroupBy = "a.Channel_ID,b.Channel_Name";
                    AdditionalWhere = " a.Channel_ID in (" + IdsList + ") ";
                    AdditionalWhere2 = " AND a.Channel_ID > 0 ";

                    break;
                case DataGrouping.Campaign:
                    Selectfields = " a.Campaign_GK , b.Campaign,";
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

        private string BuildAggregateFunction(Measure measure)
        {
            string AggregateFunction =null;
            if (measure.FunctionMeasures == null)
            {
                string FieldName = null;
                int Pos1 = measure.DWH_AggregateFunction.IndexOf("<");
                int Pos2 = measure.DWH_AggregateFunction.IndexOf(">");
                FieldName = measure.DWH_AggregateFunction.Substring(Pos1 + 1, (Pos2 - 1) - Pos1);
                Type MyType = measure.GetType();
                PropertyInfo prop = MyType.GetProperty(FieldName);
                object o = prop.GetValue(measure, null);
                AggregateFunction = measure.DWH_AggregateFunction.Replace("<" + FieldName + ">", "a." + o.ToString());
            }
            else
            {
                Measure _measure ;
                char[] Delimiter = { ',' };
                int MeasureID = 0;
                MatchCollection matches = Regex.Matches(measure.DWH_AggregateFunction, @"\<[^\>]+\>");
                foreach (Match m in matches)
                {
                    if (m.Value.ToString().ToLower().Contains("id:"))
                    {
                        int Pos = m.Value.ToString().IndexOf(">");
                        MeasureID =  Convert.ToInt32(m.Value.ToString().Substring(m.Value.ToString().IndexOf(":") + 1, (Pos - 1) - m.Value.ToString().IndexOf(":")));
                    }
                    else if (m.Value.ToString().ToLower().Contains("param:") && m.Value.ToString().ToLower().Contains("/"))
                    {
                        int Pos1, Pos2;
                        Pos1 = m.Value.ToString().IndexOf(":");
                        Pos2 = m.Value.ToString().IndexOf("/");
                        MeasureID = Convert.ToInt32(m.Value.ToString().Substring(Pos1 + 1, Pos2 - Pos1 - 1)) - 1;
                    }
                    else if (m.Value.ToString().ToLower().Contains("param:"))
                    {
                        int Pos1, Pos2;
                        Pos1 = m.Value.ToString().IndexOf(":");
                        Pos2 = m.Value.ToString().IndexOf(">");
                        MeasureID = Convert.ToInt32(m.Value.ToString().Substring(Pos1 + 1, Pos2 - Pos1 - 1)) - 1;
                    }
                    _measure = measure.FunctionMeasures[MeasureID]; 
                    AggregateFunction = AggregateFunction.Replace(m.Value, "a." + _measure.DWH_Name);
                }
            }
            
            return AggregateFunction;

        }

        //internal List<objData> GetData(int accountID, DataGrouping grouping, int top, MeasureRef[] measures, DayCodeRange[] ranges, MeasureDiff[] diff, MeasureSort[] dataSort, MeasureSort[] viewSort, MeasureFormat[] format, Dictionary<int, Measure> measuresByID, bool p)
        //{
        //    throw new NotImplementedException();
        //}
        private int GetMeasureID(string MeasureID)
        {
            int TargetMeasureID;
            if (MeasureID.Contains("T"))
            {
                int BaseMeasureID = Convert.ToInt32(MeasureID.Replace("T", ""));
                Measure _measure = Measures.Find(delegate(Measure m1) { return m1.MeasureID == BaseMeasureID; });
                TargetMeasureID = _measure.TargetMeasureID;
            }
            else
                TargetMeasureID = Convert.ToInt32(MeasureID);
            return TargetMeasureID;
        }

        private List<ObjData> DiffCalculation(List<ObjData> data, MeasureDiff[] diff)
        {
            List<ObjData> returndata = new List<ObjData>();

            foreach (ObjData Entity in data)
            {
                ArrayList EntityValues = new ArrayList();
                int i = 0, j = 0;
                if (Entity.Values.Count > 1)
                {
                    foreach (ObjValue lvalue in Entity.Values)
                    {
                        if (lvalue.FieldName.Contains("VALUE"))
                        {
                            EntityValues.Add(lvalue.ValueData);
                            i++;
                        }
                    }
                    while (j + 1 < i)
                    {
                        clsvalue _retvalue = new clsvalue();
                        _retvalue.FieldName = "Diff" + Convert.ToString(j + 2);
                        double value1, value2;
                        value1 = Convert.ToDouble(EntityValues[j]);
                        value2 = Convert.ToDouble(EntityValues[j + 1]);
                        if (value2 != 0)
                        {
                            switch (ediffType)
                            {
                                case diffType.abs:
                                    _retvalue.ValueData = Convert.ToString(value1 - value2);
                                    break;
                                case diffType.rel:
                                    _retvalue.ValueData = Convert.ToString(((value1 - value2) / Math.Abs(value2)) * 100);
                                    break;
                                default:
                                    _retvalue.ValueData = Convert.ToString(value1 - value2);
                                    break;

                            }
                        }
                        else
                            _retvalue.ValueData = null;

                        Entity.Value.Add(_retvalue);
                        j++;
                    }
                }
            }
            return AllReturnData;
        }

    }

}
