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
    /// <summary>
    /// Represents a parameter which is measured as part of the workflow process.
    /// </summary>
    public class MeasuredParameter
    {

        #region Members
        protected object _measuredValue = null;

        protected AccountAlertFilters _filters = null;
        protected AlertMeasures _measures = null;
        protected bool _include = false;
        protected TimeMeasurement _timeMeasurement = TimeMeasurement.Unknown;
        protected AlertType _alertType = AlertType.Unknown;
        protected MeasurementType _measurementType = MeasurementType.Unknown;

        protected DateTime _baseDate = DateTime.MinValue;
        protected DateTime _currentDate = DateTime.MinValue;

        protected DateTime _currentStartDate = DateTime.MinValue;
        protected DateTime _currentEndDate = DateTime.MinValue;
        protected DateTime _compareStartDate = DateTime.MinValue;
        protected DateTime _compareEndDate = DateTime.MinValue;

        protected int _accountID = -1;
        protected int _channelID = -1;
        protected string _accountName = String.Empty;
        protected int _clicksCurrent = 0;
        protected int _clicksCompare = 0;
        protected float _clicksChangeRatio = 0;
        protected int _impsCurrent = 0;
        protected int _impsCompare = 0;
        protected int _costCurrent = 0;
        protected int _costCompare = 0;
        protected int _boNewUsersCurrent = 0;
        protected int _boNewUsersCompare = 0;
        protected int _boNewActivationsCurrent = 0;
        protected int _boNewActivationsCompare = 0;
        protected int _purchasesCurrent = 0;
        protected int _purchasesCompare = 0;
        protected int _signupsCurrent = 0;
        protected int _signupsCompare = 0;
        protected int _leadsCurrent = 0;
        protected int _leadsCompare = 0;
        protected int _convsCurrent = 0;
        protected int _convsCompare = 0;
        protected double _convRateCurrent = 0; //Conversions / Clicks
        protected double _convRateCompare = 0;
        protected double _cpcCurrent = 0; //Cost / Clicks
        protected double _cpcCompare = 0;
        protected double _ctrCurrent = 0; //Clicks / Impressions
        protected double _ctrCompare = 0;
        protected double _costPerConvCurrent = 0; //Cost / Conversions.
        protected double _costPerConvCompare = 0;
        protected double _costPerBONewUsersCurrent = 0; //Cost / BO New Users
        protected double _costPerBONewUsersCompare = 0;
        protected double _avgPosition = 0;

        private bool _criticalFound = false;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public MeasuredParameter()
        {
        }
        #endregion

        #region Properties
        public virtual int GK
        {
            get
            {
                return -1;
            }
        }

        public object Value
        {
            get
            {
                return _measuredValue;
            }
        }

        public int ClicksCurrent
        {
            get
            {
                return _clicksCurrent;
            }
            set
            {
                _clicksCurrent = value;
            }
        }

        public int ClicksCompare
        {
            get
            {
                return _clicksCompare;
            }
            set
            {
                _clicksCompare = value;
            }
        }

        public float ClicksChangeRatio
        {
            get
            {
                _clicksChangeRatio = CalculateChangeRatio(Convert.ToDouble(_clicksCurrent), Convert.ToDouble(_clicksCompare));
                return _clicksChangeRatio;
            }
            set
            {
                _clicksChangeRatio = value;
            }
        }

        public int ImpsCurrent
        {
            get
            {
                return _impsCurrent;
            }
            set
            {
                _impsCurrent = value;
            }
        }

        public int ImpsCompare
        {
            get
            {
                return _impsCompare;
            }
            set
            {
                _impsCompare = value;
            }
        }

        public float ImpsChangeRatio
        {
            get
            {
                return CalculateChangeRatio(Convert.ToDouble(_impsCurrent), Convert.ToDouble(_impsCompare));
            }
        }

        public int CostCurrent
        {
            get
            {
                return _costCurrent;
            }
            set
            {
                _costCurrent = value;
            }
        }

        public int CostCompare
        {
            get
            {
                return _costCompare;
            }
            set
            {
                _costCompare = value;
            }
        }

        public float CostChangeRatio
        {
            get
            {
                return CalculateChangeRatio(Convert.ToDouble(_costCurrent), Convert.ToDouble(_costCompare));
            }
        }

        public int BONewUsersCurrent
        {
            get
            {
                return _boNewUsersCurrent;
            }
            set
            {
                _boNewUsersCurrent = value;
            }
        }

        public int BONewUsersCompare
        {
            get
            {
                return _boNewUsersCompare;
            }
            set
            {
                _boNewUsersCompare = value;
            }
        }

        public float BONewUsersChangeRatio
        {
            get
            {
                return CalculateChangeRatio(Convert.ToDouble(_boNewUsersCurrent), Convert.ToDouble(_boNewUsersCompare));
            }
        }

        public int BONewActivationsCurrent
        {
            get
            {
                return _boNewActivationsCurrent;
            }
            set
            {
                _boNewActivationsCurrent = value;
            }
        }

        public int BONewActivationsCompare
        {
            get
            {
                return _boNewActivationsCompare;
            }
            set
            {
                _boNewActivationsCompare = value;
            }
        }

        public float BONewActivationsChangeRatio
        {
            get
            {
                return CalculateChangeRatio(Convert.ToDouble(_boNewActivationsCurrent), Convert.ToDouble(_boNewActivationsCompare));
            }
        }

        public int PurchasesCurrent
        {
            get
            {
                return _purchasesCurrent;
            }
            set
            {
                _purchasesCurrent = value;
            }
        }

        public int PurcasesCompare
        {
            get
            {
                return _purchasesCompare;
            }
            set
            {
                _purchasesCompare = value;
            }
        }

        public float PurchasesChangeRatio
        {
            get
            {
                return CalculateChangeRatio(Convert.ToDouble(_purchasesCurrent), Convert.ToDouble(_purchasesCompare));
            }
        }

        public int SignupsCurrent
        {
            get
            {
                return _signupsCurrent;
            }
            set
            {
                _signupsCurrent = value;
            }
        }

        public int SignupsCompare
        {
            get
            {
                return _signupsCompare;
            }
            set
            {
                _signupsCompare = value;
            }
        }

        public float SignupsChangeRatio
        {
            get
            {
                return CalculateChangeRatio(Convert.ToDouble(_signupsCurrent), Convert.ToDouble(_signupsCompare));
            }
        }

        public int ConversionsCurrent
        {
            get
            {
                return _convsCurrent;
            }
            set
            {
                _convsCurrent = value;
            }
        }

        public int ConversionsCompare
        {
            get
            {
                return _convsCompare;
            }
            set
            {
                _convsCompare = value;
            }
        }

        public float ConversionsChangeRatio
        {
            get
            {
                return CalculateChangeRatio(Convert.ToDouble(_convsCurrent), Convert.ToDouble(_convsCompare));
            }
        }

        public int LeadsCurrent
        {
            get
            {
                return _leadsCurrent;
            }
            set
            {
                _leadsCurrent = value;
            }
        }

        public int LeadsCompare
        {
            get
            {
                return _leadsCompare;
            }
            set
            {
                _leadsCompare = value;
            }
        }

        public float LeadsChangeRatio
        {
            get
            {
                return CalculateChangeRatio(Convert.ToDouble(_leadsCurrent), Convert.ToDouble(_leadsCompare));
            }
        }

        public double ConversionRateCurrent
        {
            get
            {
                return _convRateCurrent;
            }
            set
            {
                _convRateCurrent = value;
            }
        }

        public double ConversionRateCompare
        {
            get
            {
                return _convRateCompare;
            }
            set
            {
                _convRateCompare = value;
            }
        }

        public float ConversionRateChangeRatio
        {
            get
            {
                return CalculateChangeRatio(Convert.ToDouble(_convRateCurrent), Convert.ToDouble(_convRateCompare));
            }
        }

        public double CPCCurrent
        {
            get
            {
                return _cpcCurrent;
            }
            set
            {
                _cpcCurrent = value;
            }
        }

        public double CPCCompare
        {
            get
            {
                return _cpcCompare;
            }
            set
            {
                _cpcCompare = value;
            }
        }

        public float CPCChangeRatio
        {
            get
            {
                return CalculateChangeRatio(Convert.ToDouble(_cpcCurrent), Convert.ToDouble(_cpcCompare));
            }
        }

        public double CTRCurrent
        {
            get
            {
                return _ctrCurrent;
            }
            set
            {
                _ctrCurrent = value;
            }
        }

        public double CTRCompare
        {
            get
            {
                return _ctrCompare;
            }
            set
            {
                _ctrCompare = value;
            }
        }

        public float CTRChangeRatio
        {
            get
            {
                return CalculateChangeRatio(Convert.ToDouble(_ctrCurrent), Convert.ToDouble(_ctrCompare));
            }
        }

        public bool Include
        {
            get
            {
                return _include;
            }
            set
            {
                _include = value;
            }
        }

        public double CostPerConversionsCurrent
        {
            get
            {
                return _costPerConvCurrent;
            }
            set
            {
                _costPerConvCurrent = value;
            }
        }

        public double CostPerConversionsCompare
        {
            get
            {
                return _costPerConvCompare;
            }
            set
            {
                _costPerConvCompare = value;
            }
        }

        public float CostPerConversionsChangeRatio
        {
            get
            {
                return CalculateChangeRatio(Convert.ToDouble(_costPerConvCurrent), Convert.ToDouble(_costPerConvCompare));
            }
        }

        public double CostPerBONewUsersCurrent
        {
            get
            {
                return _costPerBONewUsersCurrent;
            }
            set
            {
                _costPerBONewUsersCurrent = value;
            }
        }

        public double CostPerBONewUsersCompare
        {
            get
            {
                return _costPerBONewUsersCompare;
            }
            set
            {
                _costPerBONewUsersCompare = value;
            }
        }

        public float CostPerBONewUsersChangeRatio
        {
            get
            {
                return CalculateChangeRatio(Convert.ToDouble(_costPerBONewUsersCurrent), Convert.ToDouble(_costPerBONewUsersCompare));
            }
        }

        public int AccountID
        {
            get
            {
                return _accountID;
            }
            set
            {
                _accountID = value;
            }
        }

        public int ChannelID
        {
            get
            {
                return _channelID;
            }
            set
            {
                _channelID = value;
            }
        }

        public string AccountName
        {
            get
            {
                return _accountName;
            }
            set
            {
                _accountName = value;
            }
        }

        public TimeMeasurement TimeMeasurement
        {
            get
            {
                return _timeMeasurement;
            }
            set
            {
                _timeMeasurement = value;
            }
        }

        public DateTime CurrentDay
        {
            set
            {
                if (value > DateTime.MinValue &&
                    value < DateTime.MaxValue)
                    _baseDate = value;
            }
            get
            {
                return _baseDate;
            }
        }

        public DateTime CompareDate
        {
            get
            {
                return _currentDate;
            }
            set
            {
                if (value > DateTime.MinValue &&
                    value < DateTime.MaxValue)
                    _currentDate = value;
            }
        }

        public DateTime CurrentStartDate
        {
            get
            {
                return _currentStartDate;
            }
        }

        public DateTime CurrentEndDate
        {
            get
            {
                return _currentEndDate;
            }
        }

        public DateTime CompareStartDate
        {
            get
            {
                return _compareStartDate;
            }
        }

        public DateTime CompareEndDate
        {
            get
            {
                return _compareEndDate;
            }
        }

        public double AveragePosition
        {
            get
            {
                return _avgPosition;
            }
            set
            {
                _avgPosition = value;
            }
        }

        public bool CriticalThreshold
        {
            get
            {
                return _criticalFound;
            }
        }

        public MeasurementType MeasurementType
        {
            get
            {
                return _measurementType;
            }
            set
            {
                _measurementType = value;
            }
        }
        #endregion

        #region Public Methods
        public float ValueFromMeasure(string measure)
        {
            //Only for non-composite measures!
            if (measure == null ||
                measure == String.Empty)
                return 0;

            float ret = 0;
            switch (measure.ToLower())
            {
                case "clicks":
                    {
                        ret = ClicksChangeRatio;
                        break;
                    }

                case "imps":
                case "impressions":
                    {
                        ret = ImpsChangeRatio;
                        break;
                    }

                case "cost":
                    {
                        ret = CostChangeRatio;
                        break;
                    }

                case "bo_new_users":
                case "bonewusers":
                case "bo_newusers":
                case "bonew_users":
                    {
                        ret = BONewUsersChangeRatio;
                        break;
                    }

                case "bo_new_activations":
                case "bo_new_activation":
                case "bonewactivation":
                case "bonewactivations":
                case "bo_newactivations":
                case "bo_newactivation":
                case "bonew_activation":
                case "bonew_activations":
                    {
                        ret = BONewActivationsChangeRatio;
                        break;
                    }

                case "purchases":
                case "purchase":
                    {
                        ret = PurchasesChangeRatio;
                        break;
                    }

                case "signups":
                case "signup":
                    {
                        ret = SignupsChangeRatio;
                        break;
                    }

                case "leads":
                case "lead":
                    {
                        ret = LeadsChangeRatio;
                        break;
                    }

                case "convs":
                case "conversions":
                case "conversion":
                    {
                        ret = ConversionsChangeRatio;
                        break;
                    }

                case "convrate":
                case "conv_rate":
                case "conversion_rate":
                case "conversionrate":
                    {
                        ret = ConversionRateChangeRatio;
                        break;
                    }

                case "cpc":
                    {
                        ret = CPCChangeRatio;
                        break;
                    }

                case "ctr":
                    {
                        ret = CTRChangeRatio;
                        break;
                    }

                case "cost_per_conv":
                case "costperconv":
                    {
                        ret = CostPerConversionsChangeRatio;
                        break;
                    }

                case "cost_per_bo_new_users":
                case "costperbonewusers":
                case "cost_perbonewusers":
                case "cost_per_bonewusers":
                    {
                        ret = CostPerBONewUsersChangeRatio;
                        break;
                    }
            }

            return ret;
        }

        public float CurrentValueFromMeasure(string measure)
        {
            //Only for non-composite measures!
            if (measure == null ||
                measure == String.Empty)
                return 0;

            float ret = 0;
            switch (measure.ToLower())
            {
                case "clicks":
                    {
                        ret = ClicksCurrent;
                        break;
                    }

                case "imps":
                case "impressions":
                    {
                        ret = ImpsCurrent;
                        break;
                    }

                case "cost":
                    {
                        ret = CostCurrent;
                        break;
                    }

                case "bo_new_users":
                case "bonewusers":
                case "bo_newusers":
                case "bonew_users":
                    {
                        ret = BONewUsersCurrent;
                        break;
                    }

                case "bo_new_activations":
                case "bo_new_activation":
                case "bonewactivation":
                case "bonewactivations":
                case "bo_newactivations":
                case "bo_newactivation":
                case "bonew_activation":
                case "bonew_activations":
                    {
                        ret = BONewActivationsCurrent;
                        break;
                    }

                case "purchases":
                case "purchase":
                    {
                        ret = PurchasesCurrent;
                        break;
                    }

                case "signups":
                case "signup":
                    {
                        ret = SignupsCurrent;
                        break;
                    }

                case "leads":
                case "lead":
                    {
                        ret = LeadsCurrent;
                        break;
                    }

                case "convs":
                case "conversions":
                case "conversion":
                    {
                        ret = ConversionsCurrent;
                        break;
                    }

                case "convrate":
                case "conv_rate":
                case "conversion_rate":
                case "conversionrate":
                    {
                        ret = (float)ConversionRateCurrent;
                        break;
                    }

                case "cpc":
                    {
                        ret = (float)CPCCurrent;
                        break;
                    }

                case "ctr":
                    {
                        ret = (float)CTRCurrent;
                        break;
                    }

                case "cost_per_conv":
                case "costperconv":
                    {
                        ret = (float)CostPerConversionsCurrent;
                        break;
                    }

                case "cost_per_bo_new_users":
                case "costperbonewusers":
                case "cost_perbonewusers":
                case "cost_per_bonewusers":
                    {
                        ret = (float)CostPerBONewUsersCurrent;
                        break;
                    }
            }

            return ret;
        }

        public float CompareValueFromMeasure(string measure)
        {
            //Only for non-composite measures!
            if (measure == null ||
                measure == String.Empty)
                return 0;

            float ret = 0;
            switch (measure.ToLower())
            {
                case "clicks":
                    {
                        ret = ClicksCompare;
                        break;
                    }

                case "imps":
                case "impressions":
                    {
                        ret = ImpsCompare;
                        break;
                    }

                case "cost":
                    {
                        ret = CostCompare;
                        break;
                    }

                case "bo_new_users":
                case "bonewusers":
                case "bo_newusers":
                case "bonew_users":
                    {
                        ret = BONewUsersCompare;
                        break;
                    }

                case "bo_new_activations":
                case "bo_new_activation":
                case "bonewactivation":
                case "bonewactivations":
                case "bo_newactivations":
                case "bo_newactivation":
                case "bonew_activation":
                case "bonew_activations":
                    {
                        ret = BONewActivationsCompare;
                        break;
                    }

                case "purchases":
                case "purchase":
                    {
                        ret = PurcasesCompare;
                        break;
                    }

                case "signups":
                case "signup":
                    {
                        ret = SignupsCompare;
                        break;
                    }

                case "leads":
                case "lead":
                    {
                        ret = LeadsCompare;
                        break;
                    }

                case "convs":
                case "conversions":
                case "conversion":
                    {
                        ret = ConversionsCompare;
                        break;
                    }

                case "convrate":
                case "conv_rate":
                case "conversion_rate":
                case "conversionrate":
                    {
                        ret = (float)ConversionRateCompare;
                        break;
                    }

                case "cpc":
                    {
                        ret = (float)CPCCompare;
                        break;
                    }

                case "ctr":
                    {
                        ret = (float)CTRCompare;
                        break;
                    }

                case "cost_per_conv":
                case "costperconv":
                    {
                        ret = (float)CostPerConversionsCompare;
                        break;
                    }

                case "cost_per_bo_new_users":
                case "costperbonewusers":
                case "cost_perbonewusers":
                case "cost_per_bonewusers":
                    {
                        ret = (float)CostPerBONewUsersCompare;
                        break;
                    }
            }

            return ret;
        }
        #endregion

        #region Virtual Methods
        /// <summary>
        /// Converts the object to CSV format.
        /// </summary>
        /// <returns>String (CSV)</returns>
        public virtual string ToCSV()
        {
            throw new NotImplementedException("Not implemented!");
        }

        /// <summary>
        /// Converts the object to XML.
        /// </summary>
        /// <returns>String (XML)</returns>
        public virtual string ToXML()
        {
            throw new NotImplementedException("Not implemented!");
        }

        /// <summary>
        /// Converts the object to HTML.
        /// </summary>
        /// <returns>String (HTML)</returns>
        public virtual string ToHTML()
        {
            throw new NotImplementedException("Not implemented!");
        }

        /// <summary>
        /// Converts the relevant measured parameter to an Excel. Not implemeted in this base class
        /// </summary>
        /// <param name="ew">ExcelWorksheet to use.</param>
        /// <param name="row">Row index</param>
        public virtual void ToExcel(ref ExcelWorksheet ew, int row, AlertMeasure main, string additionalMeasures)
        {
            throw new NotImplementedException("Not implemented in this class.");
        }

        /// <summary>
        /// Fills the measured parameter class from the database.
        /// </summary>
        /// <param name="dr">Data Reader</param>
        protected virtual void Fill(SqlDataReader dr)
        {
            if (dr == null ||
                !dr.HasRows)
                throw new ArgumentException("Invalid data row. Cannot be null or empty");

            if (!dr.IsDBNull(dr.GetOrdinal("Account_id")))
                _accountID = Convert.ToInt32(dr["Account_id"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Channel_id")))
                _channelID = Convert.ToInt32(dr["Channel_id"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Account_name")))
                _accountName = dr["Account_name"].ToString();

            LoadDateFields(dr);
        }

        protected virtual void Fill(string row)
        {
            throw new NotImplementedException("Not implemented at this class.");
        }

        public virtual string GetMeasureParameterSQL(AlertMeasure am, AlertMeasures measures)
        {
            string ret = String.Empty;
            if (am.CompositeMeasure)
                ret = am.GetCompositeMeasureSQL(this, measures);
            else
                ret = am.AlertMeasureID.ToString() + ",'" + CurrentValueFromMeasure(am.AlertMeasureName) + "','" + CompareValueFromMeasure(am.AlertMeasureName) + "','" + ValueFromMeasure(am.AlertMeasureName) + "'";

            return ret;
        }

        public virtual void Filter(AlertMeasures measures, AccountAlertFilters filters)
        {
            if (measures == null ||
                measures.Count <= 0)
                throw new ArgumentException("Invalid measures collection. Cannot be null or empty");

            if (filters == null)
                throw new ArgumentNullException("Invalid filters collection. Cannot be null.");

            //If there are no filters assigned to this account, just include it.
            if (filters.Count <= 0)
            {
                _include = true;
                return;
            }

            _filters = filters;
            _measures = measures;
            _include = true; //Default = include.

            IDictionaryEnumerator ide = measures.GetEnumerator();
            while (ide.MoveNext())
            {
                AlertMeasure am = (AlertMeasure)ide.Value;
                if (filters.ContainsMeasure(am))
                {
                    if (!filters.IncludeMeasure(am, this))
                    {
                        //It's enough that one measure is not within the min/max values, and therefore
                        //we don't include this parameter.
                        _include = false;
                        break;
                    }
                }
            }
        }

        public virtual void LoadDateFields(SqlDataReader dr)
        {
            switch (_alertType)
            {
                case AlertType.Daily:
                    {
                        try
                        {
                            if (!dr.IsDBNull(dr.GetOrdinal("Current_Day")))
                                _currentDate = DayCode.GenerateDateTime(dr["Current_Day"]);

                            if (!dr.IsDBNull(dr.GetOrdinal("Compare_Day")))
                                _baseDate = DayCode.GenerateDateTime(dr["Compare_Day"]);
                        }
                        catch (Exception ex)
                        {
                            Log.Write("Alert Type is Daily, but couldn't find correct fields.", ex);
                        }

                        break;
                    }

                case AlertType.Period:
                    {
                        try
                        {
                            if (!dr.IsDBNull(dr.GetOrdinal("Current_Start_Day")))
                                _currentStartDate = DayCode.GenerateDateTime(dr["Current_Start_Day"]);

                            if (!dr.IsDBNull(dr.GetOrdinal("Current_End_Day")))
                                _currentEndDate = DayCode.GenerateDateTime(dr["Current_End_Day"]);

                            if (!dr.IsDBNull(dr.GetOrdinal("Compare_Start_Day")))
                                _compareStartDate = DayCode.GenerateDateTime(dr["Compare_Start_Day"]);

                            if (!dr.IsDBNull(dr.GetOrdinal("Compare_End_Day")))
                                _compareEndDate = DayCode.GenerateDateTime(dr["Compare_End_Day"]);
                        }
                        catch (Exception ex)
                        {
                            Log.Write("Alert Type is Period, but couldn't find additional fields.", ex);
                        }

                        break;
                    }
            }
        }
        #endregion

        #region Public Static Methods
        public static Measures FromString(string measure)
        {
            switch (measure.ToLower())
            {
                case "clicks":
                    return Measures.Clicks;

                case "imps":
                    return Measures.Impressions;

                default:
                    return Measures.Unknown;
            }
        }

        public static string FromMeasure(Measures m)
        {
            return m.ToString();
        }
        #endregion

        #region Protected Methods
        protected void GenerateExcel(ref ExcelWorksheet ew, int row, int col, AlertMeasure main, string additionalMeasures)
        {
            //Main Measure Compare
            ew.Cell(row, col).Value = CompareValueFromMeasure(main.AlertMeasureName).ToString();
            //Main Measure Current
            ew.Cell(row, col + 1).Value = CurrentValueFromMeasure(main.AlertMeasureName).ToString();
            //Main Measure Change Ratio
            ew.Cell(row, col + 2).Value = ValueFromMeasure(main.AlertMeasureName).ToString("N2");
            //Cell Style
            SetCellStyle(main, ref ew, row, col + 2);

            //Loop on the additional measures.
            if (additionalMeasures == String.Empty)
                return;

            int next = col + 3;
            string[] measures = additionalMeasures.Split(',');
            for (int i = 0; i < measures.Length; i++)
            {
                //Compare
                ew.Cell(row, next).Value = CompareValueFromMeasure(measures[i]).ToString();
                //Current
                ew.Cell(row, next + 1).Value = CurrentValueFromMeasure(measures[i]).ToString();
                //Change Ratio
                ew.Cell(row, next + 2).Value = ValueFromMeasure(measures[i]).ToString("N2");
                //Cell Style
                if (_measures != null)
                {
                    AlertMeasure am = (AlertMeasure)_measures.Get(measures[i]);
                    if (am != null)
                        SetCellStyle(am, ref ew, row, next + 2);
                }
                //Advance the column counter.
                next = next + 3;
            }
        }
        #endregion

        #region Private Methods
        private void SetCellStyle(AlertMeasure measure, ref ExcelWorksheet ew, int row, int col)
        {
            //First get the filter.
            if (_filters == null)
                return;

            if (!_filters.ContainsMeasure(measure))
                return;

            AccountAlertFilter aaf = _filters.GetFilter(measure);

            //If the measure is negative
            double value = Convert.ToDouble(ValueFromMeasure(measure.AlertMeasureName));
            if (value < 0)
            {
                //Critical first.
                if (aaf.CriticalThreshold > 0)
                {
                    double critical = 0 - aaf.CriticalThreshold;
                    if (critical > value)
                    {
                        _criticalFound = true;
                        ew.Cell(row, col).Style = "Bad";
                        return;
                    }
                }

                if (aaf.BadThreshold > 0)
                {
                    double bad = 0 - aaf.BadThreshold;
                    if (bad > value)
                    {
                        ew.Cell(row, col).Style = "Neutral";
                    }
                }
            }
            else
            {
                //We're positive, check for the good threshold.
                if (value > aaf.GoodThreshold)
                    ew.Cell(row, col).Style = "Good";
            }
        }

        private float CalculateChangeRatio(double current, double compare)
        {
            if (_timeMeasurement == TimeMeasurement.Relative ||
                _timeMeasurement == TimeMeasurement.Unknown)
            {
                if (_measurementType == MeasurementType.Relative ||
                    _measurementType == MeasurementType.Unknown)
                {
                    double d = current - compare;
                    if (compare != 0)
                        d /= compare;

                    d *= 100;

                    return (float)d;
                }
                else
                {
                    return (float)(current - compare);
                }
            }
            else
            {
                return (float)current;
            }
        }
        #endregion

    }

    /// <summary>
    /// Represents a collection of MeasuredParameter objects.
    /// </summary>
    public class MeasuredParameters : Hashtable
    {

        #region Members
        private bool _criticalFound = false;
        #endregion

        #region Constructor
        /// <summary>
        /// Clears the internal collection
        /// </summary>
        public MeasuredParameters()
        {
            Clear();
        }
        #endregion

        #region Properties
        public bool CriticalThreshold
        {
            get
            {
                return _criticalFound;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Appends an external list into our internal list. Please note that if any overlapping
        /// elements are found, the elements from the external list will be taken.
        /// </summary>
        /// <param name="mps">External list</param>
        public void Append(MeasuredParameters mps)
        {
            if (mps == null)
                throw new ArgumentException("Invalid MeasuredParameters argument. Cannot be null");

            IDictionaryEnumerator ide = mps.GetEnumerator();
            ide.Reset();

            while (ide.MoveNext())
            {
                if (!ContainsKey(ide.Key))
                    Add(ide.Key, ide.Value);
                else
                    this[ide.Key] = ide.Value;
            }
        }

        /// <summary>
        /// Creates a CSV file representing the collection.
        /// </summary>
        /// <param name="path">Where to save the CSV file</param>
        /// <returns>The file name which was created</returns>
        public string ToCSV(string path, string headings, bool outputToFile)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fileName = DateTime.Now.ToString("yyyyMMdd") + ".csv";
            string thePath = path;
            if (!thePath.EndsWith(@"\"))
                thePath += @"\";

            thePath += fileName;

            IDictionaryEnumerator ide = GetEnumerator();
            ide.Reset();
            StringBuilder sb = new StringBuilder();

            //First generate the headings.
            if (headings != null &&
                headings != String.Empty)
            {
                sb.AppendLine(headings);
            }

            while (ide.MoveNext())
            {
                MeasuredParameter mp = (MeasuredParameter)ide.Value;
                sb.AppendLine(mp.ToCSV());
            }

            if (outputToFile)
            {
                File.WriteAllText(thePath, sb.ToString());
                return thePath;
            }
            else
            {
                return sb.ToString();
            }
        }

        public string ToCSV(string headings)
        {
            IDictionaryEnumerator ide = GetEnumerator();
            ide.Reset();
            StringBuilder sb = new StringBuilder();

            //First generate the headings.
            if (headings != null &&
                headings != String.Empty)
            {
                sb.AppendLine(headings);
            }

            //Sort the measured parameters first.
            MeasureSorter sorter = new MeasureSorter();
            ArrayList vals = new ArrayList(Values);
            vals.Sort(sorter);

            for (int i = 0; i < vals.Count; i++)
            {
                MeasuredParameter mp = (MeasuredParameter)vals[i];
                sb.AppendLine(mp.ToCSV());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates an XML file representing the collection.
        /// </summary>
        /// <param name="path">Where to save the XML file</param>
        /// <returns>The file name which was created</returns>
        public string ToXML(string path)
        {
            throw new NotImplementedException("Not implemented!");
        }

        /// <summary>
        /// Creates an HTML file representing the collection.
        /// </summary>
        /// <param name="path">Where to save the HTML file</param>
        /// <returns>The file which was created</returns>
        public string ToHTML(string path)
        {
            throw new NotImplementedException("Not implemented!");
        }

        /// <summary>
        /// Fills the Excel Worksheet parameter based on the internal measured parameters
        /// collection.
        /// </summary>
        /// <param name="ew">The excel worksheet to fill</param>
        /// <param name="row">The row to insert from.</param>
        public void ToExcel(ref ExcelWorksheet ew, ref int row, AlertMeasure main, string additionalMeasures)
        {
            //Sort the measured parameters first.
            MeasureSorter sorter = new MeasureSorter(main);
            ArrayList vals = new ArrayList(Values);
            vals.Sort(sorter);

            for (int i = 0; i < vals.Count; i++)
            {
                MeasuredParameter mp = (MeasuredParameter)vals[i];
                row++;
                mp.ToExcel(ref ew, row, main, additionalMeasures);
                if (mp.CriticalThreshold)
                    _criticalFound = mp.CriticalThreshold;
            }
        }
        #endregion

    }

    /// <summary>
    /// Sorts the measured parameters based on the relevant main measure.
    /// </summary>
    public class MeasureSorter : IComparer
    {

        #region Members
        AlertMeasure _selectedMeasure = null;
        #endregion

        #region Constructors
        public MeasureSorter()
        {
        }

        public MeasureSorter(AlertMeasure measure)
        {
            _selectedMeasure = measure;
        }
        #endregion

        #region IComparer Members

        public int Compare(object x, object y)
        {
            MeasuredParameter fx = (MeasuredParameter)x;
            MeasuredParameter fy = (MeasuredParameter)y;

            int ret = _selectedMeasure.Compare(fx, fy);
            return ret;
        }

        #endregion

    }

    /// <summary>
    /// A collection of measure objects.
    /// </summary>
    public class AlertMeasures : Hashtable
    {

        #region Constructors
        public AlertMeasures()
        {
            DataManager.ConnectionString = AppSettings.GetAbsolute("Easynet.Edge.Core.Workflow.AlertConnectionString");

            //Create the command.
            using (DataManager.Current.OpenConnection())
            {
                string sql = "SELECT * FROM AlertMeasures";

                SqlCommand filters = DataManager.CreateCommand(sql);

                SqlDataReader dr = filters.ExecuteReader();
                while (dr.Read())
                {
                    AlertMeasure am = new AlertMeasure(dr);
                    Add(am.AlertMeasureID, am);
                }

                dr.Close();
                dr.Dispose();
            }
        }

        public AlertMeasures(string composite)
        {
        }
        #endregion

        #region Public Methods
        public AlertMeasure Get(string measureName)
        {
            if (measureName == String.Empty ||
                measureName == null)
                return null;

            IDictionaryEnumerator ide = GetEnumerator();
            while (ide.MoveNext())
            {
                AlertMeasure am = (AlertMeasure)ide.Value;
                if (am.AlertMeasureName == measureName)
                    return am;
            }

            return null;
        }
        #endregion

    }

    /// <summary>
    /// Represents a single alert measure class. An alert measure is a measure object which represents
    /// a specific piece of information regarding a statistical measure that the system measures.
    /// There are two types of measures:
    /// 1) Simple measures - these are standard basic measures (clicks, impressions etc)
    /// 2) Composite measures - these are measures which are comprised of other measures with a specific
    /// //                      relation between them, such as: costs AND impressions.
    /// </summary>
    public class AlertMeasure
    {

        #region Members
        private int _alertMeasureID = -1;
        private string _alertMeasureName = String.Empty;
        private string _alertMeasureDescription = String.Empty;
        protected bool _compositeMeasure = false;
        protected string _compositeMeasureValue = String.Empty;
        protected string _measureColumnNames = String.Empty;
        #endregion

        #region Constructors
        public AlertMeasure()
        {
        }

        public AlertMeasure(SqlDataReader dr)
        {
            if (dr == null ||
                !dr.HasRows)
                throw new ArgumentException("Invalid data row. Cannot be null or empty");

            if (dr.IsDBNull(dr.GetOrdinal("AlertMeasureID")))
                throw new Exception("Invalid data row. Alert measure ID cannot be null.");

            if (dr.IsDBNull(dr.GetOrdinal("AlertMeasureName")))
                throw new Exception("Invalid data row. Alert measure name cannot be null.");

            if (dr.IsDBNull(dr.GetOrdinal("CompositeMeasure")))
                throw new Exception("Invalid data row. Composite measure cannot be null.");

            _alertMeasureID = Convert.ToInt32(dr["AlertMeasureID"]);
            _alertMeasureName = dr["AlertMeasureName"].ToString();

            if (!dr.IsDBNull(dr.GetOrdinal("AlertMeasureDescription")))
                _alertMeasureDescription = dr["AlertMeasureDescription"].ToString();

            _compositeMeasure = Convert.ToBoolean(dr["CompositeMeasure"]);

            if (!dr.IsDBNull(dr.GetOrdinal("CompositeValue")))
                _compositeMeasureValue = dr["CompositeValue"].ToString();

            if (!dr.IsDBNull(dr.GetOrdinal("MeasureColumnNames")))
                _measureColumnNames = dr["MeasureColumnNames"].ToString();
        }
        #endregion

        #region Properties
        public int AlertMeasureID
        {
            get
            {
                return _alertMeasureID;
            }
            set
            {
                if (value > 0)
                    _alertMeasureID = value;
            }
        }

        public string AlertMeasureName
        {
            get
            {
                return _alertMeasureName;
            }
            set
            {
                if (value != String.Empty &&
                    value != null)
                    _alertMeasureName = value;
            }
        }

        public string AlertMeasureDescription
        {
            get
            {
                return _alertMeasureDescription;
            }
            set
            {
                _alertMeasureDescription = value;
            }
        }

        public bool CompositeMeasure
        {
            get
            {
                return _compositeMeasure;
            }
            set
            {
                _compositeMeasure = value;
            }
        }

        public string CompositeValue
        {
            get
            {
                return _compositeMeasureValue;
            }
            set
            {
                if (_compositeMeasure)
                {
                    if (value != String.Empty &&
                        value != null)
                        _compositeMeasureValue = value;
                }
            }
        }

        public string AlertMeasureColumns
        {
            get
            {
                return _measureColumnNames;
            }
        }
        #endregion

        #region Public Methods
        public string GetCompositeMeasureSQL(MeasuredParameter mp, AlertMeasures measures)
        {
            if (mp == null)
                throw new ArgumentNullException("Measured Parameter argument cannot be null.");

            if (measures == null ||
                measures.Count <= 0)
                throw new ArgumentException("Invalid measures argument. Cannot be null or empty");

            if (!_compositeMeasure)
                throw new Exception("GetCompositeMeasureSQL does not support non-composite measures. Use the BaseAlertActivity for that evaluation");

            //Get the composite measures.
            //9_AND_14_OR_7
            string[] composite = _compositeMeasureValue.Split('_');
            if (composite.Length <= 0)
                return String.Empty;

            string currents = String.Empty;
            string compares = String.Empty;
            string ratios = String.Empty;
            string ids = String.Empty;
            int id = -1;
            for (int i = 0; i < composite.Length; i++)
            {
                if (Int32.TryParse(composite[i], out id))
                {
                    //This is an ID
                    AlertMeasure dependant = (AlertMeasure)measures[id];
                    float current = mp.CurrentValueFromMeasure(dependant.AlertMeasureName);
                    float compare = mp.CompareValueFromMeasure(dependant.AlertMeasureName);
                    float ratio = mp.ValueFromMeasure(dependant.AlertMeasureName);

                    if (ids == String.Empty)
                        ids += id.ToString();
                    else
                        ids += "," + id.ToString();

                    if (currents == String.Empty)
                        currents += current.ToString();
                    else
                        currents += "," + current.ToString();

                    if (compares == String.Empty)
                        compares += compare.ToString();
                    else
                        compares += "," + compare.ToString();

                    if (ratios == String.Empty)
                        ratios += ratio.ToString();
                    else
                        ratios += "," + ratio.ToString();
                }
            }

            string ret = String.Empty;
            ret = "'" + ids + "','" + currents + "','" + compares + "','" + ratios + "'";
            return ret;
        }

        public bool Evaluate(MeasuredParameter param, float value, MeasureDiff diff, AlertMeasures measures)
        {
            if (param == null)
                throw new ArgumentNullException("Measured Parameter argument cannot be null.");

            if (diff == MeasureDiff.Unknown)
                return false;

            if (measures == null ||
                measures.Count <= 0)
                throw new ArgumentException("Invalid measures argument. Cannot be null or empty");

            if (!_compositeMeasure)
                throw new Exception("Evaluate does not support evaluating non-composite measures. Use the BaseAlertActivity for that evaluation");

            //Get the composite measures.
            //9_AND_14_OR_7
            string[] composite = _compositeMeasureValue.Split('_');
            if (composite.Length <= 0)
                return false;

            //Evaluation algorithm.
            //We always check pairs of values with an operator.
            //If the operator is AND, if the first pair is false, then we assume false.
            //If the operator is OR, it's enough that the first pair is true.
            int id = -1;
            Array measuredValues = new float[2];
            Operators op = Operators.Unknown;
            bool bRet = false;
            int idx = 0;
            for (int i = 0; i < composite.Length; i++)
            {
                if (Int32.TryParse(composite[i], out id))
                {
                    //This is an ID
                    AlertMeasure dependant = (AlertMeasure)measures[id];
                    float f = param.ValueFromMeasure(dependant.AlertMeasureName);
                    measuredValues.SetValue(f, idx);
                    idx++;
                }
                else
                {
                    //This is an operator.
                    op = GetOperator(composite[i]);
                }

                if (((i + 1) % 3 == 0) &&
                    (i != 0))
                {
                    //We did a pair + operator.
                    idx = 0;
                    bRet = InternalEvaluate(measuredValues, op, value, diff);

                    //Failed evaluation - exit.
                    if (!bRet)
                        break;
                }
            }

            return bRet;
        }

        public int Compare(MeasuredParameter x, MeasuredParameter y)
        {
            float fx = (x.CurrentValueFromMeasure(this.AlertMeasureName) -
                        x.CompareValueFromMeasure(this.AlertMeasureName)) * x.ValueFromMeasure(this.AlertMeasureName);

            float fy = (y.CurrentValueFromMeasure(this.AlertMeasureName) -
                        y.CompareValueFromMeasure(this.AlertMeasureName)) * y.ValueFromMeasure(this.AlertMeasureName);

            if (fx > fy)
                return -1;
            else if (fx == fy)
                return 0;
            else
                return 1;
        }
        #endregion

        #region Private Methods
        private Operators GetOperator(string op)
        {
            Operators ret = Operators.Unknown;
            switch (op.ToLower())
            {
                case "and":
                    {
                        ret = Operators.AND;
                        break;
                    }

                case "or":
                    {
                        ret = Operators.OR;
                        break;
                    }
            }

            return ret;
        }

        private bool InternalEvaluate(Array a, Operators op, float value, MeasureDiff diff)
        {
            if (a.Length <= 0)
                return false;

            bool bRet = false;

            float v1 = 0;
            v1 = (float)Convert.ToDouble(a.GetValue(0));

            float v2 = 0;
            if (a.Length > 1)
                v2 = (float)Convert.ToDouble(a.GetValue(1));

            switch (diff)
            {
                case MeasureDiff.Equal:
                    {
                        #region Equal
                        switch (op)
                        {
                            case Operators.AND:
                                {
                                    if (v1 == value && v2 == value)
                                        bRet = true;
                                    else
                                        bRet = false;

                                    break;
                                }

                            case Operators.OR:
                                {
                                    if (v1 == value || v2 == value)
                                        bRet = true;
                                    else
                                        bRet = false;

                                    break;
                                }
                        }
                        #endregion

                        break;
                    }

                case MeasureDiff.Large:
                    {
                        #region Larger
                        switch (op)
                        {
                            case Operators.AND:
                                {
                                    if (v1 > value && v2 > value)
                                        bRet = true;
                                    else
                                        bRet = false;

                                    break;
                                }

                            case Operators.OR:
                                {
                                    if (v1 > value || v2 > value)
                                        bRet = true;
                                    else
                                        bRet = false;

                                    break;
                                }
                        }
                        #endregion

                        break;
                    }

                case MeasureDiff.Smaller:
                    {
                        #region Smaller
                        switch (op)
                        {
                            case Operators.AND:
                                {
                                    if (v1 < value && v2 < value)
                                        bRet = true;
                                    else
                                        bRet = false;

                                    break;
                                }

                            case Operators.OR:
                                {
                                    if (v1 < value || v2 < value)
                                        bRet = true;
                                    else
                                        bRet = false;

                                    break;
                                }
                        }
                        #endregion

                        break;
                    }
            }

            return bRet;
        }
        #endregion

    }

}