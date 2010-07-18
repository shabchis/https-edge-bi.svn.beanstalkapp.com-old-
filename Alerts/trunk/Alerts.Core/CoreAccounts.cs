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
    /// This class contains the relevant filters for each account.
    /// </summary>
    public class AccountAlertFilters : Hashtable
    {

        #region Constructors
        public AccountAlertFilters()
        {
            Fill(-1);
        }

        public AccountAlertFilters(int accountID)
        {
            Fill(accountID);
        }
        #endregion

        #region Private Methods
        private void Fill(int accountID)
        {
            DataManager.ConnectionString = AppSettings.GetAbsolute("Easynet.Edge.Core.Workflow.AlertConnectionString");

            //Create the command.
            using (DataManager.Current.OpenConnection())
            {
                string sql = "SELECT * FROM AccountMeasureFilters";
                if (accountID > 0)
                    sql += " WHERE AccountID = " + accountID.ToString();

                SqlCommand filters = DataManager.CreateCommand(sql);

                SqlDataReader dr = filters.ExecuteReader();
                while (dr.Read())
                {
                    AccountAlertFilter aaf = new AccountAlertFilter(dr);
                    Add(aaf.RowID, aaf);
                }

                dr.Close();
                dr.Dispose();
            }
        }
        #endregion

        #region Public Methods
        public bool IncludeMeasure(AlertMeasure am, MeasuredParameter mp)
        {
            if (am == null)
                throw new ArgumentNullException("Invalid alert measure parameter. Cannot be null.");

            if (mp == null)
                throw new ArgumentNullException("Invalid measured parameter argument. Cannot be null.");

            IDictionaryEnumerator ide = GetEnumerator();
            while (ide.MoveNext())
            {
                AccountAlertFilter aaf = (AccountAlertFilter)ide.Value;
                if (aaf.MeasureID == am.AlertMeasureID)
                {
                    //This is the measure we want to check the filter of.
                    if (aaf.MatchesFilter(am, mp))
                        return true;
                }
            }

            return false;
        }

        public bool ContainsMeasure(AlertMeasure am)
        {
            if (am == null)
                throw new ArgumentNullException("Invalid alert measure parameter. Cannot be null.");

            IDictionaryEnumerator ide = GetEnumerator();
            while (ide.MoveNext())
            {
                AccountAlertFilter aaf = (AccountAlertFilter)ide.Value;
                if (aaf.MeasureID == am.AlertMeasureID)
                    return true;
            }

            return false;
        }

        public AccountAlertFilter GetFilter(AlertMeasure am)
        {
            IDictionaryEnumerator ide = GetEnumerator();
            while (ide.MoveNext())
            {
                AccountAlertFilter aaf = (AccountAlertFilter)ide.Value;
                if (aaf.MeasureID == am.AlertMeasureID)
                    return aaf;
            }

            return null;
        }
        #endregion

    }

    /// <summary>
    /// Represents a filter for a specific measure for a specific account.
    /// </summary>
    public class AccountAlertFilter
    {

        #region Members
        private int _rowID = -1;
        private int _accountID = -1;
        private int _measureID = -1;
        private double _minValue = -1;
        private double _maxValue = -1;
        private double _goodThreshold = -1;
        private double _badThreshold = -1;
        private double _criticalThreshold = -1;
        #endregion

        #region Constructors
        public AccountAlertFilter()
        {
        }

        public AccountAlertFilter(SqlDataReader dr)
        {
            if (dr == null ||
                !dr.HasRows)
                throw new ArgumentException("Invalid data row. Cannot be null or empty");

            if (dr.IsDBNull(dr.GetOrdinal("RowID")))
                throw new Exception("Invalid data row. Row ID cannot be null.");

            if (dr.IsDBNull(dr.GetOrdinal("AccountID")))
                throw new Exception("Invalid data row. Account ID cannot be null.");

            if (dr.IsDBNull(dr.GetOrdinal("MeasureID")))
                throw new Exception("Invalid data row. Measure ID cannot be null.");

            if (dr.IsDBNull(dr.GetOrdinal("MinValue")))
                throw new Exception("Invalid data row. Min Value cannot be null.");

            if (dr.IsDBNull(dr.GetOrdinal("MaxValue")))
                throw new Exception("Invalid data row. Max Value cannot be null.");

            if (dr.IsDBNull(dr.GetOrdinal("GoodThreshold")))
                throw new Exception("Invalid data row. Good threshold cannot be null.");

            if (dr.IsDBNull(dr.GetOrdinal("BadThreshold")))
                throw new Exception("Invalid data row. Bad Threshold cannot be null.");

            if (dr.IsDBNull(dr.GetOrdinal("CriticalThreshold")))
                throw new Exception("Invalid data row. Critical threshold cannot be null.");

            _rowID = Convert.ToInt32(dr["RowID"]);
            _accountID = Convert.ToInt32(dr["AccountID"]);
            _measureID = Convert.ToInt32(dr["MeasureID"]);
            _minValue = Convert.ToDouble(dr["MinValue"]);
            _maxValue = Convert.ToDouble(dr["MaxValue"]);
            _goodThreshold = Convert.ToDouble(dr["GoodThreshold"]);
            _badThreshold = Convert.ToDouble(dr["BadThreshold"]);
            _criticalThreshold = Convert.ToDouble(dr["CriticalThreshold"]);
        }
        #endregion

        #region Properties
        public int RowID
        {
            get
            {
                return _rowID;
            }
        }

        public int AccountID
        {
            get
            {
                return _accountID;
            }
        }

        public int MeasureID
        {
            get
            {
                return _measureID;
            }
        }

        public double MinValue
        {
            get
            {
                return _minValue;
            }
        }

        public double MaxValue
        {
            get
            {
                return _maxValue;
            }
        }

        public double GoodThreshold
        {
            get
            {
                return _goodThreshold;
            }
        }

        public double BadThreshold
        {
            get
            {
                return _badThreshold;
            }
        }

        public double CriticalThreshold
        {
            get
            {
                return _criticalThreshold;
            }
        }
        #endregion

        #region Public Methods
        public bool MatchesFilter(AlertMeasure am, MeasuredParameter mp)
        {
            if (am == null)
                throw new ArgumentNullException("Invalid alert measure parameter. Cannot be null.");

            if (mp == null)
                throw new ArgumentNullException("Invalid measured parameter argument. Cannot be null.");


            if (_minValue > 0)
            {
                if (Convert.ToDouble(mp.CurrentValueFromMeasure(am.AlertMeasureName)) < _minValue ||
                    Convert.ToDouble(mp.CompareValueFromMeasure(am.AlertMeasureName)) < _minValue)
                    return false;
            }

            if (_maxValue > 0)
            {
                if (Convert.ToDouble(mp.CompareValueFromMeasure(am.AlertMeasureName)) > _maxValue ||
                    Convert.ToDouble(mp.CurrentValueFromMeasure(am.AlertMeasureName)) > _maxValue)
                    return false;
            }

            return true;
        }
        #endregion

    }
}