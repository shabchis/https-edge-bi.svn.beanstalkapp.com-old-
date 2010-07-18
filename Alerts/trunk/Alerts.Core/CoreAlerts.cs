using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Collections.Specialized;

using OfficeOpenXml; //For excel.

using Easynet.Edge.Core.Workflow;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Configuration;

namespace Easynet.Edge.Alerts.Core
{

    #region Enumerations
    public enum MeasureDiff
    {
        Unknown = -1,
        Large = 0,
        Smaller = 1,
        Equal = 2,
    }

    public enum ReportType
    {
        Unknown = -1,
        CSV = 0,
        XML = 1,
        HTML = 2,
        Excel = 3,
    }

    public enum Measures
    {
        Unknown = -1,
        Clicks = 0,
        Impressions = 1,
        Cost = 2,
        BONewUsers = 3,
        BONewActivations = 4,
    }

    public enum EntityTypes
    {
        Unknown = -1,
        Account = 0,
        Campaign = 1,
        Adgroup = 2,
        Adtext = 3,
        Keyword = 4,
        Gateway = 5,
    }

    public enum TimeDeltaType
    {
        General = 0,
        Daily = 1,
        Weekly = 2,
        Monthly = 3,
    }

    public enum Operators
    {
        Unknown = -1,
        AND = 0,
        OR = 1,
    }

    public enum TimeMeasurement
    {
        Unknown = -1,
        Relative = 0,
        Absolute = 1,
    }

    public enum AlertType
    {
        Unknown = -1,
        Daily = 0,
        Period = 1,
    }

    public enum MeasurementType
    {
        Unknown = -1,
        Relative = 0,
        Absolute = 1,
    }
    #endregion

    #region Structures
    public struct AdgroupGK
    {
        public int _campaignGK;
        public int _adgroupGK;
    }
    #endregion


    /// <summary>
    /// The base class for all alert custom activities.
    /// </summary>
    public class BaseAlertActivity : BaseActivity
    {

        #region Members
        protected EntityTypes _entityType = EntityTypes.Unknown;
        protected ArrayList _results = new ArrayList();
        protected AlertMeasures _measures = null;
        protected AccountAlertFilters _filters = null;
        protected TimeMeasurement _timeMeasurement = TimeMeasurement.Unknown;
        protected AlertType _alertType = AlertType.Unknown;
        protected MeasurementType _measurementType = MeasurementType.Unknown;
        #endregion

        #region Properties
        public EntityTypes EntityType
        {
            get
            {
                return _entityType;
            }
        }

        public AlertMeasures AlertMeasures
        {
            get
            {
                if (!ParentWorkflow.InternalParameters.ContainsKey("AlertMeasures"))
                {
                    _measures = new AlertMeasures();
                    ParentWorkflow.InternalParameters.Add("AlertMeasures", _measures);
                }
                else
                {
                    _measures = (Easynet.Edge.Alerts.Core.AlertMeasures)ParentWorkflow.InternalParameters["AlertMeasures"];
                }

                return _measures;
            }
        }

        public AccountAlertFilters Filters
        {
            get
            {
                if (!ParentWorkflow.InternalParameters.ContainsKey("AccountFilters"))
                {
                    int accountID = -1;
                    if (ParentWorkflow.Parameters.ContainsKey("AccountID"))
                        accountID = Convert.ToInt32(ParentWorkflow.Parameters["AccountID"]);
                    _filters = new AccountAlertFilters(accountID);
                    ParentWorkflow.InternalParameters.Add("AccountFilters", _filters);
                }
                else
                {
                    _filters = (AccountAlertFilters)ParentWorkflow.InternalParameters["AccountFilters"];
                }

                return _filters;
            }
        }

        public TimeMeasurement TimeMeasurement
        {
            get
            {
                return _timeMeasurement;
            }
        }

        public MeasurementType MeasurementType
        {
            get
            {
                return _measurementType;
            }
        }
        #endregion

        #region Private Methods
        private void SetTimeDeltaType(TimeDeltaType type)
        {
            if (!ParentWorkflow.InternalParameters.ContainsKey("TimeDeltaType"))
            {
                ParentWorkflow.InternalParameters.Add("TimeDeltaType", type);
            }
        }

        private void SetDateRanges(DateTime cur, DateTime comp)
        {
            if (!ParentWorkflow.InternalParameters.ContainsKey("CurrentDate"))
            {
                ParentWorkflow.InternalParameters.Add("CurrentDate", cur);
            }

            if (!ParentWorkflow.InternalParameters.ContainsKey("CompareDate"))
            {
                ParentWorkflow.InternalParameters.Add("CompareDate", comp);
            }
        }
        #endregion

        #region Protected Methods
        protected void AllocateReport(MeasuredParameters mps, string measure, MeasureDiff diff)
        {
            if (ParentWorkflow.InternalParameters.ContainsKey("Reports"))
            {
                Hashtable currentReports = (Hashtable)ParentWorkflow.InternalParameters["Reports"];
                if (currentReports.ContainsKey(_entityType))
                {
                    Reports entReports = (Reports)currentReports[_entityType];
                    if (entReports.ContainsDifference(diff))
                    {
                        //We have a report which contains this type of difference.
                        Report curReport = (Report)entReports[diff];
                        curReport.MeasuredParameters.Append(mps);
                        curReport.Processed = false;
                        curReport.DifferenceType = diff;

                        if (!curReport.Measures.Contains(MeasuredParameter.FromString(measure)))
                            curReport.Measures.Add(MeasuredParameter.FromString(measure));

                        entReports[diff] = curReport;
                    }
                    else
                    {
                        //We do not have a report which contains this type of difference.
                        Report r = new Report();
                        r.MeasuredParameters.Append(mps);
                        r.Processed = false;
                        r.Name = this.Name;
                        r.DifferenceType = diff;
                        r.Measures.Add(MeasuredParameter.FromString(measure));
                        entReports.Add(diff, r);
                    }

                    currentReports[_entityType] = entReports;
                }
                else
                {
                    //We do not have a report for this entity type.
                    Report r = new Report();
                    r.MeasuredParameters.Append(mps);
                    r.Processed = false;
                    r.Name = this.Name;
                    r.DifferenceType = diff;
                    r.Measures.Add(MeasuredParameter.FromString(measure));

                    if (ParentWorkflow.InternalParameters.ContainsKey("AccountFilters"))
                        r.Filters = (AccountAlertFilters)ParentWorkflow.InternalParameters["AccountFilters"];

                    r.Account = Convert.ToInt32(ParentWorkflow.Parameters["AccountID"]);

                    Reports ht = new Reports();
                    ht.Add(diff, r);

                    currentReports.Add(_entityType, ht);
                    ParentWorkflow.InternalParameters["Reports"] = currentReports;
                }
            }
            else
            {
                Report r = new Report();
                r.MeasuredParameters.Append(mps);
                r.Processed = false;
                r.Name = this.Name;
                r.DifferenceType = diff;
                r.Measures.Add(MeasuredParameter.FromString(measure));

                if (ParentWorkflow.InternalParameters.ContainsKey("AccountFilters"))
                    r.Filters = (AccountAlertFilters)ParentWorkflow.InternalParameters["AccountFilters"];

                r.Account = Convert.ToInt32(ParentWorkflow.Parameters["AccountID"]);

                Reports ht = new Reports();
                ht.Add(diff, r);

                Hashtable reports = new Hashtable();
                reports.Add(_entityType, ht);
                ParentWorkflow.InternalParameters.Add("Reports", reports);
            }
        }

        protected string GetCompareDate(int timeDiff, DateTime curDate, DateTime originalCompDate)
        {
            //if time diff is 0, then just return the original compare date.
            DateTime ret = DateTime.MinValue;
            if (timeDiff == 0)
            {
                //General Report Type.
                SetTimeDeltaType(TimeDeltaType.General);
                SetDateRanges(curDate, originalCompDate);
                return originalCompDate.ToString("yyyyMMdd");
            }
            else if (timeDiff == -1)
            {
                //Check if today is a weekend (i.e. Monday) so, we need
                //the previous Friday.
                if (curDate.DayOfWeek == DayOfWeek.Monday)
                {
                    //Need to subtract 3.
                    ret = curDate.AddDays(-3);
                }
                else
                {
                    ret = curDate.AddDays(Convert.ToDouble(timeDiff));
                }

                //This means that we're in the DAILY report type.
                SetTimeDeltaType(TimeDeltaType.Daily);
            }
            else
            {
                ret = curDate.AddDays(Convert.ToDouble(timeDiff));

                if (timeDiff == -7)
                {
                    //Weekly report type.
                    SetTimeDeltaType(TimeDeltaType.Weekly);
                }
                else if (timeDiff == -30)
                {
                    //Monthly report type.
                    SetTimeDeltaType(TimeDeltaType.Monthly);
                }
                else
                {
                    //General report type.
                    SetTimeDeltaType(TimeDeltaType.General);
                }
            }

            SetDateRanges(curDate, ret);
            return ret.ToString("yyyyMMdd");
        }

        protected bool Evaluate(float value, float compare, MeasureDiff diff)
        {
            bool bRet = false;
            switch (diff)
            {
                case MeasureDiff.Equal:
                    {
                        if (value == compare)
                            bRet = true;
                        else
                            bRet = false;

                        break;
                    }

                case MeasureDiff.Large:
                    {
                        if (value > compare)
                            bRet = true;
                        else
                            bRet = false;

                        break;
                    }

                case MeasureDiff.Smaller:
                    {
                        //AP - what we want here is actually if the delta is smaller than -VALUE
                        float neg = 0 - compare;
                        if (value < neg)
                            bRet = true;
                        else
                            bRet = false;

                        break;
                    }
            }

            return bRet;
        }
        #endregion

        #region Public Methods
        public bool CheckEntity(float value, AlertMeasure measure, MeasureDiff diff)
        {
            if (value < 0)
                return false;

            if (measure == null)
                return false;

            try
            {
                MeasuredParameters ret = new MeasuredParameters();
                bool bRet = false;
                ret = CheckMeasure(measure, value, diff);

                if (ret != null && ret.Count > 0)
                {
                    AllocateReport(ret, measure.AlertMeasureName, diff);

                    //If we have something to report about - also flush it into the database.
                    Flush(ret);
                    bRet = true;
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Log.Write("Exception in CheckEntity", ex);
                return false;
            }
        }

        public AlertMeasure GetMainMeasure()
        {
            AlertMeasures measures = null;
            AlertMeasure ret = null;
            if (!ParentWorkflow.InternalParameters.Contains("AlertMeasures"))
            {
                measures = new AlertMeasures();
                ParentWorkflow.InternalParameters.Add("AlertMeasures", measures);
            }
            else
            {
                measures = (AlertMeasures)ParentWorkflow.InternalParameters["AlertMeasures"];
            }

            if (ParentWorkflow.Parameters.Contains("MainMeasure"))
            {
                //Return the main measure.
                string main = ParentWorkflow.Parameters["MainMeasure"].ToString();
                ret = measures.Get(main);
            }
            else
            {
                //Default to clicks.
                ret = measures.Get("Clicks");
            }

            return ret;
        }

        public AlertMeasure GetMeasure(string name)
        {
            if (name == null ||
                name == String.Empty)
                throw new ArgumentException("Invalid measure name. Cannot be null or empty!");

            AlertMeasures measures = null;
            AlertMeasure ret = null;
            if (!ParentWorkflow.InternalParameters.Contains("AlertMeasures"))
            {
                measures = new AlertMeasures();
                ParentWorkflow.InternalParameters.Add("AlertMeasures", measures);
            }
            else
            {
                measures = (AlertMeasures)ParentWorkflow.InternalParameters["AlertMeasures"];
            }

            ret = measures.Get(name);

            return ret;
        }

        public void Flush(MeasuredParameters mps)
        {
            IDictionaryEnumerator ide = mps.GetEnumerator();
            while (ide.MoveNext())
            {
                MeasuredParameter mp = (MeasuredParameter)ide.Value;
                WriteResult(mp);
            }
        }
        #endregion

        #region Virtual Methods
        /* Writing the results to the database */
        protected virtual void WriteResult(MeasuredParameter mp)
        {
            AlertMeasures measures = (AlertMeasures)ParentWorkflow.InternalParameters["AlertMeasures"];

            //Loop on each of the measures we have - and for each, write the results.
            IDictionaryEnumerator ide = measures.GetEnumerator();
            while (ide.MoveNext())
            {
                AlertMeasure am = (AlertMeasure)ide.Value;
                WriteResult(mp, am);
            }
        }

        protected virtual void WriteResult(MeasuredParameter mp, AlertMeasure am)
        {
            DataManager.ConnectionString = AppSettings.GetAbsolute("Easynet.Edge.Core.Workflow.AlertConnectionString");

            //Create the command.
            using (DataManager.Current.OpenConnection())
            {
                AlertMeasures measures = (AlertMeasures)ParentWorkflow.InternalParameters["AlertMeasures"];

                string sql = "INSERT INTO AlertResults ";
                string columns = "(wf_id,wf_date,wf_parameters,wf_conditionValues,entity_type,Account_id,Channel_id,Account_name,Current_Day,Compare_Day,measure_id,measure_current_value,measure_compare_value,measure_change_ratio";

                columns += GetColumns(mp);
                sql += columns + " VALUES(";

                int wfID = ParentWorkflow.WorkflowID;
                if (wfID <= 0)
                {
                    if (ParentWorkflow.Parameters.ContainsKey("WorkflowID"))
                        wfID = Convert.ToInt32(ParentWorkflow.Parameters["WorkflowID"]);
                }

                string values = String.Empty;
                values = wfID.ToString() + ",'" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss") + "','" + ParentWorkflow.GetParametersAsString() + "','" + ParentWorkflow.GetConditionValuesAsString() + "'," + Convert.ToInt32(_entityType).ToString() + ",";
                values += mp.AccountID.ToString() + "," + mp.ChannelID.ToString() + ",'" + mp.AccountName + "'," + DayCode.ToDayCode(mp.CurrentDay).ToString() + "," + DayCode.ToDayCode(mp.CompareDate).ToString() + ",";
                values += mp.GetMeasureParameterSQL(am, measures) + ")";

                sql += values;
                SqlCommand alertResults = DataManager.CreateCommand(sql);
                try
                {
                    alertResults.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Log.Write("Failed to write alert result to database.", ex);
                    throw ex;
                }
            }
        }

        /* Rules */
        protected virtual MeasuredParameters CheckMeasure(AlertMeasure measure, float value, MeasureDiff diff)
        {
            AlertMeasures measures = null;
            if (!ParentWorkflow.InternalParameters.Contains("AlertMeasures"))
            {
                measures = new AlertMeasures();
                ParentWorkflow.InternalParameters.Add("AlertMeasures", measures);
            }
            else
            {
                measures = (AlertMeasures)ParentWorkflow.InternalParameters["AlertMeasures"];
            }

            MeasuredParameters ret = new MeasuredParameters();
            for (int i = 0; i < _results.Count; i++)
            {
                try
                {
                    MeasuredParameter mp = (MeasuredParameter)_results[i];
                    if (measure.CompositeMeasure)
                    {
                        if (measure.Evaluate(mp, value, diff, measures))
                        {
                            ret.Add(mp.GK, mp);
                        }
                    }
                    else
                    {
                        if (Evaluate(mp.ValueFromMeasure(measure.AlertMeasureName),
                            value,
                            diff))
                        {
                            ret.Add(mp.GK, mp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write("Exception occured while adding measured parameters", ex);
                    continue; ;
                }
            }

            return ret;
        }

        /* Building the commands */
        protected virtual SqlCommand BuildCommand()
        {
            throw new NotImplementedException("Not Implemented!");
        }

        protected virtual void BuildCommandParameters(ref SqlCommand cmd)
        {
            cmd.Parameters.Add("@Account_id", System.Data.SqlDbType.NVarChar);
            cmd.Parameters.Add("@channel_id", System.Data.SqlDbType.NVarChar);
            cmd.Parameters.Add("@RC", System.Data.SqlDbType.Int);
            cmd.Parameters["@RC"].Direction = System.Data.ParameterDirection.ReturnValue;

            switch (_alertType)
            {
                case AlertType.Daily:
                    {
                        cmd.Parameters.Add("@CurrentDayCode", System.Data.SqlDbType.NVarChar);
                        cmd.Parameters.Add("@CompareDayCode", System.Data.SqlDbType.NVarChar);
                        break;
                    }

                case AlertType.Period:
                    {
                        cmd.Parameters.Add("@CurrentStartDayCode", System.Data.SqlDbType.NVarChar);
                        cmd.Parameters.Add("@CurrentEndDayCode", System.Data.SqlDbType.NVarChar);
                        cmd.Parameters.Add("@CompareStartDayCode", System.Data.SqlDbType.NVarChar);
                        cmd.Parameters.Add("@CompareEndDayCode", System.Data.SqlDbType.NVarChar);
                        break;
                    }
            }
        }

        protected virtual void SetCommandParameterValues(ref SqlCommand cmd)
        {
            if (!ParentWorkflow.Parameters.ContainsKey("AccountID"))
                throw new Exception("Invalid workflow parameters. Could not find Account ID.");

            if (!ParentWorkflow.Parameters.ContainsKey("ChannelID"))
                throw new Exception("Invalid workflow parameters. Could not find Channel ID.");

            int accountID = Convert.ToInt32(ParentWorkflow.Parameters["AccountID"]);
            string channelID = ParentWorkflow.Parameters["ChannelID"].ToString();

            cmd.Parameters["@Account_id"].Value = accountID.ToString();
            cmd.Parameters["@channel_id"].Value = channelID;

            InitializeTimes(ref cmd);
        }

        protected virtual void InitializeTimes(ref SqlCommand command)
        {
            switch (_alertType)
            {
                case AlertType.Daily:
                    {
                        DateTime cur = DayCode.GenerateDateTime(ParentWorkflow.Parameters["CurrentDayCode"]);
                        DateTime comp = DayCode.GenerateDateTime(ParentWorkflow.Parameters["CompareDayCode"]);

                        int timeDiff = 0;
                        if (ParentWorkflow.Parameters.ContainsKey("TimeDifference"))
                        {
                            timeDiff = Convert.ToInt32(ParentWorkflow.Parameters["TimeDifference"]);
                        }

                        //format the current and compare dates.
                        string curDate = cur.ToString("yyyyMMdd");
                        string compDate = GetCompareDate(timeDiff, cur, comp);

                        command.Parameters["@CurrentDayCode"].Value = curDate;

                        if (_timeMeasurement == TimeMeasurement.Relative)
                            command.Parameters["@CompareDayCode"].Value = compDate;
                        else
                            command.Parameters["@CompareDayCode"].Value = curDate;

                        break;
                    }

                case AlertType.Period:
                    {
                        DateTime curStart = DayCode.GenerateDateTime(ParentWorkflow.Parameters["CurrentStartDayCode"]);
                        DateTime curEnd = DayCode.GenerateDateTime(ParentWorkflow.Parameters["CurrentEndDayCode"]);
                        DateTime compareStart = DayCode.GenerateDateTime(ParentWorkflow.Parameters["CompareStartDayCode"]);
                        DateTime compareEnd = DayCode.GenerateDateTime(ParentWorkflow.Parameters["CompareEndDayCode"]);

                        command.Parameters["@CurrentStartDayCode"].Value = curStart.ToString("yyyyMMdd");
                        command.Parameters["@CurrentEndDayCode"].Value = curEnd.ToString("yyyyMMdd");
                        command.Parameters["@CompareStartDayCode"].Value = compareStart.ToString("yyyyMMdd");
                        command.Parameters["@CompareEndDayCode"].Value = compareEnd.ToString("yyyyMMdd");
                        break;
                    }
            }
        }

        /* Obtaining data from the temporary tables. */
        protected virtual SqlDataReader GetMeasuredData(Hashtable parameters)
        {
            throw new NotImplementedException("Not implemented.");
        }
        #endregion

        #region Private Methods
        private string GetColumns(MeasuredParameter mp)
        {
            if (mp == null)
                throw new ArgumentNullException("Invalid measure parameter. Cannot be null.");

            string ret = String.Empty;
            if (mp.GetType() == typeof(CampaignAllMeasures))
            {
                ret = ",Campaign_gk,Campaign_name)";
            }
            else if (mp.GetType() == typeof(AdgroupAllMeasures))
            {
                ret = ",Campaign_gk,Campaign_name,Adgroup_gk,Adgroup_name)";
            }
            else if (mp.GetType() == typeof(AdtextAllMeasures))
            {
                ret = ",Campaign_gk,Campaign_name,Adgroup_gk,Adgroup_name,Adtext_gk,Adtext_name,Adtext_Description)";
            }
            else if (mp.GetType() == typeof(KeywordAllMeasures))
            {
                ret = ",Campaign_gk,Campaign_name,Adgroup_gk,Adgroup_name,Keyword_gk,Keyword_name)";
            }
            else if (mp.GetType() == typeof(GatewayAllMeasures))
            {
                ret = ",Campaign_gk,Campaign_name,Adgroup_gk,Adgroup_name,Gateway_gk,Gateway_name,Gateway_id)";
            }
            else
            {
                ret = ")";
            }

            return ret;
        }

        private TimeMeasurement FromTimeParameter(object param)
        {
            if (param == null)
                return TimeMeasurement.Unknown;

            if (param.ToString().ToLower() == "relative")
                return TimeMeasurement.Relative;
            else if (param.ToString().ToLower() == "absolute")
                return TimeMeasurement.Absolute;
            else
                return TimeMeasurement.Unknown;
        }

        private MeasurementType FromMeasurementParameter(object param)
        {
            if (param == null)
                return MeasurementType.Unknown;

            if (param.ToString().ToLower() == "relative")
                return MeasurementType.Relative;
            else if (param.ToString().ToLower() == "absolute")
                return MeasurementType.Absolute;
            else
                return MeasurementType.Unknown;
        }
        #endregion

        #region Protected Overrides
        protected override void Initialize(IServiceProvider provider)
        {
            if (ParentWorkflow.Parameters.ContainsKey("TimeMeasurement"))
                _timeMeasurement = FromTimeParameter(ParentWorkflow.Parameters["TimeMeasurement"]);
            else
                _timeMeasurement = TimeMeasurement.Relative; //Assume relative.

            if (ParentWorkflow.Parameters.ContainsKey("MeasurementType"))
                _measurementType = FromMeasurementParameter(ParentWorkflow.Parameters["MeasurementType"]);
            else
                _measurementType = MeasurementType.Relative; //Assume relative.

            if (ParentWorkflow.Parameters.ContainsKey("AlertType"))
            {
                if (ParentWorkflow.Parameters["AlertType"].ToString().ToLower() == "period")
                    _alertType = AlertType.Period;
                else
                    _alertType = AlertType.Daily;
            }
            else
                _alertType = AlertType.Daily; //Assume daily.
        }
        #endregion

    }

    /// <summary>
    /// The base alert workflow class, contains standard functionality for all workflows
    /// related to alerts.
    /// </summary>
    public class BaseAlertWorkflow : BaseSequentialWorkflow
    {
    }










}
