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
    /// Represents the account data.
    /// </summary>
    public class AccountAllMeasures : MeasuredParameter
    {

        #region Members
        protected int _sumOfActiveUsers = -1;
        protected double _sumOfNewNetDesposits = -1;
        protected double _sumOfTotalNetDeposits = -1;
        protected double _sumOfClientSpecific1 = -1;
        protected double _sumOfClientSpecific2 = -1;
        protected double _sumOfClientSpecific3 = -1;
        protected double _sumOfClientSpecific4 = -1;
        protected double _sumOfClientSpecific5 = -1;
        #endregion

        #region Public Constants
        public const int ADWORDS_CSV_START_ROW = 7;
        public const int DATE_RANGE_ROW = 3;
        #endregion

        #region Constructor
        public AccountAllMeasures(string row)
        {
            Fill(row);
        }

        public AccountAllMeasures(string row, bool panorama)
        {
            Fill(row, panorama);
        }

        public AccountAllMeasures(SqlDataReader dr)
        {
            FillInternal(dr);
        }

        public AccountAllMeasures(SqlDataReader dr, bool bo)
        {
            Fill(dr, bo);
        }

        /// <summary>
        /// Additional constructor, this one receives the data reader containing the details of the relevant measured
        /// parameter (account in this case), the filters for the account and the measures list.
        /// </summary>
        /// <param name="dr">The SqlDataReader containing the data read from the database.</param>
        /// <param name="measures">The collection of available measures</param>
        /// <param name="alertType">The type of alert</param>
        public AccountAllMeasures(SqlDataReader dr, AlertMeasures measures, AlertType alertType)
        {
            _alertType = alertType;

            Fill(dr);
        }
        #endregion

        #region Properties
        public override int GK
        {
            get
            {
                return _accountID;
            }
        }

        public int SumOfActiveUsers
        {
            get
            {
                return _sumOfActiveUsers;
            }
        }

        public double SumOfNewNetDeposits
        {
            get
            {
                return _sumOfNewNetDesposits;
            }
        }

        public double SumOfTotalNetDeposits
        {
            get
            {
                return _sumOfTotalNetDeposits;
            }
        }

        public double SumOfClientSpecific1
        {
            get
            {
                return _sumOfClientSpecific1;
            }
        }

        public double SumOfClientSpecific2
        {
            get
            {
                return _sumOfClientSpecific2;
            }
        }

        public double SumOfClientSpecific3
        {
            get
            {
                return _sumOfClientSpecific3;
            }
        }

        public double SumOfClientSpecific4
        {
            get
            {
                return _sumOfClientSpecific4;
            }
        }

        public double SumOfClientSpecific5
        {
            get
            {
                return _sumOfClientSpecific5;
            }
        }
        #endregion

        #region Private Methods
        private void Fill(string row, bool panorama)
        {
            if (!panorama)
            {
                Fill(row);
            }
            else
            {
                if (row == null ||
                    row == String.Empty)
                    throw new ArgumentException("Invalid row, cannot be empty or null.");

                string[] rowData = row.Split('\t');
                if (rowData.Length <= 1)
                    rowData = row.Split(',');

                _accountName = rowData[0];

                if (rowData[1] == String.Empty)
                    _clicksCurrent = 0;
                else
                {
                    string clicks = rowData[1];
                    clicks = clicks.Trim();
                    clicks = clicks.Replace("\"", "");
                    clicks = clicks.Replace(",", "");
                    clicks = clicks.Replace("%", "");
                    clicks = clicks.Trim();

                    if (clicks == String.Empty)
                        clicks = "0";

                    if (clicks.Contains("."))
                    {
                        double d = Convert.ToDouble(clicks);
                        _clicksCurrent = Convert.ToInt32(d);
                    }
                    else
                    {
                        _clicksCurrent = Convert.ToInt32(clicks);
                    }
                }

                double cost = 0;
                if (rowData[2] != String.Empty)
                {
                    string cst = rowData[2];
                    cst = cst.Trim();
                    cst = cst.Replace("\"", "");
                    cst = cst.Replace(",", "");
                    cst = cst.Replace("%", "");
                    cst = cst.Trim();

                    if (cst == String.Empty)
                        cst = "0";

                    cost = Convert.ToDouble(cst);
                }

                _costCurrent = Convert.ToInt32(cost);

                if (rowData[3] == String.Empty)
                    _convsCurrent = 0;
                else
                {
                    string convs = rowData[3];
                    convs = convs.Trim();
                    convs = convs.Replace("\"", "");
                    convs = convs.Replace(",", "");
                    convs = convs.Replace("%", "");
                    convs = convs.Trim();

                    if (convs == String.Empty)
                        convs = "0";

                    if (convs.Contains("."))
                    {
                        double d = Convert.ToDouble(convs);
                        _convsCurrent = Convert.ToInt32(d);
                    }
                    else
                    {
                        _convsCurrent = Convert.ToInt32(convs);
                    }
                }

                if (rowData[4] == String.Empty)
                    _impsCurrent = 0;
                else
                {
                    string imps = rowData[4];
                    imps = imps.Trim();
                    imps = imps.Replace("\"", "");
                    imps = imps.Replace(",", "");
                    imps = imps.Replace("%", "");
                    imps = imps.Trim();

                    if (imps == String.Empty)
                        imps = "0";

                    if (imps.Contains("."))
                    {
                        double d = Convert.ToDouble(imps);
                        _impsCurrent = Convert.ToInt32(d);
                    }
                    else
                    {
                        _impsCurrent = Convert.ToInt32(imps);
                    }
                }

                _avgPosition = 0;

                if (rowData[5] == String.Empty)
                    _purchasesCurrent = 0;
                else
                {
                    string purchases = rowData[5];
                    purchases = purchases.Trim();
                    purchases = purchases.Replace("\"", "");
                    purchases = purchases.Replace(",", "");
                    purchases = purchases.Replace("%", "");
                    purchases = purchases.Trim();

                    if (purchases == String.Empty)
                        purchases = "0";

                    if (purchases.Contains("."))
                    {
                        double d = Convert.ToDouble(purchases);
                        _purchasesCurrent = Convert.ToInt32(d);
                    }
                    else
                    {
                        _purchasesCurrent = Convert.ToInt32(purchases);
                    }
                }

                if (rowData[6] == String.Empty)
                    _leadsCurrent = 0;
                else
                {
                    string leads = rowData[6];
                    leads = leads.Trim();
                    leads = leads.Replace("\"", "");
                    leads = leads.Replace(",", "");
                    leads = leads.Replace("%", "");
                    leads = leads.Trim();

                    if (leads == String.Empty)
                        leads = "0";

                    if (leads.Contains("."))
                    {
                        double d = Convert.ToDouble(leads);
                        _leadsCurrent = Convert.ToInt32(d);
                    }
                    else
                    {
                        _leadsCurrent = Convert.ToInt32(leads);
                    }
                }

                if (rowData[7] == String.Empty)
                    _signupsCurrent = 0;
                else
                {
                    string singups = rowData[7];
                    singups = singups.Trim();
                    singups = singups.Replace("\"", "");
                    singups = singups.Replace(",", "");
                    singups = singups.Replace("%", "");
                    singups = singups.Trim();

                    if (singups == String.Empty)
                        singups = "0";

                    if (singups.Contains("."))
                    {
                        double d = Convert.ToDouble(singups);
                        _signupsCurrent = Convert.ToInt32(d);
                    }
                    else
                    {
                        _signupsCurrent = Convert.ToInt32(singups);
                    }
                }
            }
        }

        private void Fill(SqlDataReader dr, bool bo)
        {
            if (dr == null ||
                !dr.HasRows)
                throw new ArgumentException("Invalid data row. Cannot be null or empty");

            if (!bo)
            {
                Fill(dr);
                return;
            }

            if (!dr.IsDBNull(dr.GetOrdinal("AccountName")))
                _accountName = dr["AccountName"].ToString();

            if (!dr.IsDBNull(dr.GetOrdinal("Account_ID")))
                _accountID = Convert.ToInt32(dr["account_id"]);

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfClicks")))
                _clicksCurrent = Convert.ToInt32(dr["SumOfClicks"]);

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfLeads")))
                _leadsCurrent = Convert.ToInt32(dr["SumOfLeads"]);

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfNewUsers")))
                _boNewUsersCurrent = Convert.ToInt32(dr["SumOfNewUsers"]);

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfNewActiveUsers")))
                _boNewActivationsCurrent = Convert.ToInt32(dr["SumOfNewActiveUsers"]);

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfActiveUsers")))
                _sumOfActiveUsers = Convert.ToInt32(dr["SumOfActiveUsers"]);

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfNewNetDeposits")))
                _sumOfNewNetDesposits = Convert.ToDouble(dr["SumOfNewNetDeposits"]);

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfTotalNetDeposits")))
                _sumOfTotalNetDeposits = Convert.ToDouble(dr["SumOfTotalNetDeposits"]);

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfClientSpecific1")))
                _sumOfClientSpecific1 = Convert.ToDouble(dr["SumOfClientSpecific1"]);

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfClientSpecific2")))
                _sumOfClientSpecific2 = Convert.ToDouble(dr["SumOfClientSpecific2"]);

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfClientSpecific3")))
                _sumOfClientSpecific3 = Convert.ToDouble(dr["SumOfClientSpecific3"]);

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfClientSpecific4")))
                _sumOfClientSpecific4 = Convert.ToDouble(dr["SumOfClientSpecific4"]);

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfClientSpecific5")))
                _sumOfClientSpecific5 = Convert.ToDouble(dr["SumOfClientSpecific5"]);
        }

        private void FillInternal(SqlDataReader dr)
        {
            //Get the additional data.
            if (dr == null ||
                !dr.HasRows)
                throw new ArgumentException("Invalid data row. Cannot be null or empty");

            if (!dr.IsDBNull(dr.GetOrdinal("account_id")))
                _accountID = Convert.ToInt32(dr["account_id"]);

            if (!dr.IsDBNull(dr.GetOrdinal("AccountName")))
                _accountName = dr["AccountName"].ToString();

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfImps")))
                _impsCurrent = Convert.ToInt32(dr["SumOfImps"]);

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfClicks")))
                _clicksCurrent = Convert.ToInt32(dr["SumOfClicks"]);

            _cpcCurrent = 0;
            //if (!dr.IsDBNull(dr.GetOrdinal("AVGCPC")))
            //    _cpcCurrent = Convert.ToDouble(dr["AVGCPC"]);

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfCost")))
                _costCurrent = Convert.ToInt32(dr["SumOfCost"]);

            _avgPosition = 0;
            //if (!dr.IsDBNull(dr.GetOrdinal("AvgPos")))
            //    _avgPosition = Convert.ToDouble(dr["AvgPos"]);

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfConv")))
                _convsCurrent = Convert.ToInt32(dr["SumOfConv"]);

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfPurchase")))
                _purchasesCurrent = Convert.ToInt32(dr["SumOfPurchase"]);

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfleads")))
                _leadsCurrent = Convert.ToInt32(dr["SumOfleads"]);

            if (!dr.IsDBNull(dr.GetOrdinal("SumOfSignups")))
                _signupsCurrent = Convert.ToInt32(dr["SumOfSignups"]);
        }
        #endregion

        #region Protected Overrides
        protected override void Fill(string row)
        {
            if (row == null ||
                row == String.Empty)
                throw new ArgumentException("Invalid row, cannot be empty or null.");

            string[] rowData = row.Split('\t');
            if (rowData.Length <= 1)
                rowData = row.Split(',');

            if (rowData[0] != String.Empty)
            {
                string[] dateString = rowData[0].Split('/');
                if (dateString.Length == 3)
                {
                    int month = Convert.ToInt32(dateString[dateString.Length - dateString.Length]);
                    int day = Convert.ToInt32(dateString[dateString.Length - 2]);
                    int year = Convert.ToInt32(dateString[dateString.Length - 1]);
                    if (dateString[dateString.Length - 1].Length <= 2)
                        year += 2000;

                    _currentDate = new DateTime(year,
                                                month,
                                                day);
                }
                else
                {
                    //If this is a monthly report, we should have the year and the month name
                    //try converting it into date time.
                    DateTime time = DateTime.MinValue;
                    if (DateTime.TryParse(rowData[0], out time))
                    {
                        _currentDate = time;
                    }
                }
            }

            _accountName = rowData[1];
            
            if (rowData[2] == String.Empty)
                _impsCurrent = 0;
            else
                _impsCurrent = Convert.ToInt32(rowData[2]);

            if (rowData[3] == String.Empty)
                _clicksCurrent = 0;
            else
                _clicksCurrent = Convert.ToInt32(rowData[3]);

            double cost = 0;
            if (rowData[5] != String.Empty)
                cost = Convert.ToDouble(rowData[5]);
            
            _costCurrent = Convert.ToInt32(cost);

            if (rowData[7] == String.Empty)
                _convsCurrent = 0;
            else
                _convsCurrent = Convert.ToInt32(rowData[7]);

            if (rowData[8] == String.Empty)
                _purchasesCurrent = 0;
            else
                _purchasesCurrent = Convert.ToInt32(rowData[8]);

            if (rowData[9] == String.Empty)
                _leadsCurrent = 0;
            else
                _leadsCurrent = Convert.ToInt32(rowData[9]);

            if (rowData[10] == String.Empty)
                _signupsCurrent = 0;
            else
                _signupsCurrent = Convert.ToInt32(rowData[10]);
        }

        protected override void Fill(SqlDataReader dr)
        {
            base.Fill(dr);

            if (!dr.IsDBNull(dr.GetOrdinal("Clicks_Current")))
                _clicksCurrent = Convert.ToInt32(dr["Clicks_Current"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Clicks_Compare")))
                _clicksCompare = Convert.ToInt32(dr["Clicks_Compare"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Imps_Current")))
                _impsCurrent = Convert.ToInt32(dr["Imps_Current"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Imps_Compare")))
                _impsCompare = Convert.ToInt32(dr["Imps_Compare"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Cost_Current")))
                _costCurrent = Convert.ToInt32(dr["Cost_Current"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Cost_Compare")))
                _costCompare = Convert.ToInt32(dr["Cost_Compare"]);

            if (!dr.IsDBNull(dr.GetOrdinal("BO_New_Users_Current")))
                _boNewUsersCurrent = Convert.ToInt32(dr["BO_New_Users_Current"]);

            if (!dr.IsDBNull(dr.GetOrdinal("BO_New_Users_Compare")))
                _boNewUsersCompare = Convert.ToInt32(dr["BO_New_Users_Compare"]);

            if (!dr.IsDBNull(dr.GetOrdinal("BO_New_Activations_Current")))
                _boNewActivationsCurrent = Convert.ToInt32(dr["BO_New_Activations_Current"]);

            if (!dr.IsDBNull(dr.GetOrdinal("BO_New_Activations_Compare")))
                _boNewActivationsCompare = Convert.ToInt32(dr["BO_New_Activations_Compare"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Purchases_Current")))
                _purchasesCurrent = Convert.ToInt32(dr["Purchases_Current"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Purchases_Compare")))
                _purchasesCompare = Convert.ToInt32(dr["Purchases_Compare"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Signups_Current")))
                _signupsCurrent = Convert.ToInt32(dr["Signups_Current"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Signups_Compare")))
                _signupsCompare = Convert.ToInt32(dr["Signups_Compare"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Leads_Current")))
                _leadsCurrent = Convert.ToInt32(dr["Leads_Current"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Leads_Compare")))
                _leadsCompare = Convert.ToInt32(dr["Leads_Compare"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Convs_Current")))
                _convsCurrent = Convert.ToInt32(dr["Convs_Current"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Convs_Compare")))
                _convsCompare = Convert.ToInt32(dr["Convs_Compare"]);

            if (_clicksCompare != 0)
            {
                float f1 = (float)_convsCompare;
                float f2 = (float)_clicksCompare;
                float f3 = f1 / f2;
                _convRateCompare = Convert.ToDouble(f3);

                f1 = (float)_costCompare;
                f2 = (float)_clicksCompare;
                f3 = f1 / f2;
                _cpcCompare = Convert.ToDouble(f3);
            }

            if (_clicksCurrent != 0)
            {
                float f1 = (float)_convsCurrent;
                float f2 = (float)_clicksCurrent;
                float f3 = f1 / f2;
                _convRateCurrent = Convert.ToDouble(f3);

                f1 = (float)_costCurrent;
                f2 = (float)_clicksCurrent;
                f3 = f1 / f2;
                _cpcCurrent = Convert.ToDouble(f3);
            }

            if (_impsCompare != 0)
            {
                float f1 = (float)_clicksCompare;
                float f2 = (float)_impsCompare;
                float f3 = f1 / f2;
                _ctrCompare = Convert.ToDouble(f3);
            }

            if (_impsCurrent != 0)
            {
                float f1 = (float)_clicksCurrent;
                float f2 = (float)_impsCurrent;
                float f3 = f1 / f2;
                _ctrCurrent = Convert.ToDouble(f3);
            }

            if (_convsCompare != 0)
            {
                float f1 = (float)_costCompare;
                float f2 = (float)_convsCompare;
                float f3 = f1 / f2;
                _costPerConvCompare = Convert.ToDouble(f3);
            }

            if (_convsCurrent != 0)
            {
                float f1 = (float)_costCurrent;
                float f2 = (float)_convsCurrent;
                float f3 = f1 / f2;
                _costPerConvCurrent = Convert.ToDouble(f3);
            }

            if (_boNewUsersCompare != 0)
            {
                float f1 = (float)_costCompare;
                float f2 = (float)_boNewUsersCompare;
                float f3 = f1 / f2;
                _costPerBONewUsersCompare = Convert.ToDouble(f3);
            }

            if (_boNewUsersCurrent != 0)
            {
                float f1 = (float)_costCurrent;
                float f2 = (float)_boNewUsersCurrent;
                float f3 = f1 / f2;
                _costPerBONewUsersCurrent = Convert.ToDouble(f3);
            }
        }
        #endregion

        #region Public Methods
        public bool Different(AccountAllMeasures other)
        {
            if (other.AccountName != "9000000")
            {
                if (other.AccountName.ToLower() != _accountName.ToLower())
                    throw new ArgumentException("Invalid AccountAllMeasures object. Account name [" + other.AccountName + "] does not match internal account [" + _accountName + "].");
            }
            else
            {
                //HACK!
                if ("9million" != _accountName.ToLower())
                    throw new ArgumentException("Invalid AccountAllMeasures object. Account name [9million] does not match internal account [" + _accountName + "].");
            }

            bool bRet = false;

            if (other.ImpsCurrent !=
                _impsCurrent)
                bRet = true;

            if (other.ClicksCurrent !=
                _clicksCurrent)
                bRet = true;

            if (other.CostCurrent !=
                _costCurrent)
                bRet = true;

            if (other.ConversionsCurrent !=
                _convsCurrent)
                bRet = true;

            if (other.PurchasesCurrent !=
                _purchasesCurrent)
                bRet = true;

            if (other.LeadsCurrent !=
                _leadsCurrent)
                bRet = true;

            if (other.SignupsCurrent !=
                _signupsCurrent)
                bRet = true;

            return bRet;
        }
        #endregion

        #region Public Static Methods
        public static int FromAccountName(string name)
        {
            if (name == String.Empty ||
                name == null)
                throw new ArgumentException("Invalid account name. Cannot be empty or null.");

            int ret = -1;

            DataManager.ConnectionString = AppSettings.GetAbsolute("Easynet.Edge.Core.Workflow.AlertConnectionString");
            DataManager.CommandTimeout = 0;

            string sql = @"SELECT InternalAccountID FROM GoogleAccountNames WHERE GoogleAccountName = '" + name + "'";

            using (DataManager.Current.OpenConnection())
            {
                SqlCommand cmd = DataManager.CreateCommand(sql);
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    if (!dr.IsDBNull(dr.GetOrdinal("InternalAccountID")))
                        ret = Convert.ToInt32(dr["InternalAccountID"]);
                }

                dr.Close();
                dr.Dispose();
            }

            return ret;
        }

        public static string FromGoogleAccountName(string name)
        {
            if (name == String.Empty ||
                name == null)
                throw new ArgumentException("Invalid account name. Cannot be empty or null.");

            string ret = String.Empty;

            DataManager.ConnectionString = AppSettings.GetAbsolute("Easynet.Edge.Core.Workflow.AlertConnectionString");
            DataManager.CommandTimeout = 0;

            string sql = @"SELECT 
                            uga.Account_Name,
                            gan.InternalAccountID 
                            FROM 
                            Edge2Alerts.dbo.GoogleAccountNames AS gan,easynet_OLTP.dbo.User_Gui_Account AS uga
                            WHERE GoogleAccountName = '" + name + "' AND gan.InternalAccountID = uga.Account_ID";

            using (DataManager.Current.OpenConnection())
            {
                SqlCommand cmd = DataManager.CreateCommand(sql);
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    if (!dr.IsDBNull(dr.GetOrdinal("Account_Name")))
                        ret = dr["Account_Name"].ToString();
                }

                dr.Close();
                dr.Dispose();
            }

            return ret;
        }
        #endregion

        #region Public Overrides
        public override void ToExcel(ref ExcelWorksheet ew, int row, AlertMeasure main, string additionalMeasures)
        {
            ew.InsertRow(row);

            //Campaign Name
            string name = _accountName;
            if (name.Contains("'"))
                name = name.Replace("'", "");
            ew.Cell(row, 1).Value = name;

            int firstCol = -1;
            if (_alertType == AlertType.Daily)
            {
                //Base Date
                ew.Cell(row, 2).Value = _baseDate.ToString("dd/MM/yyyy");

                //Compare Date
                ew.Cell(row, 3).Value = _currentDate.ToString("dd/MM/yyyy");
                firstCol = 4;
            }
            else
            {
                //Base Start Date
                ew.Cell(row, 2).Value = _compareStartDate.ToString("dd/MM/yyyy");
                ew.Cell(row, 3).Value = _compareEndDate.ToString("dd/MM/yyyy");

                //Compare Date
                ew.Cell(row, 4).Value = _currentStartDate.ToString("dd/MM/yyyy");
                ew.Cell(row, 5).Value = _currentEndDate.ToString("dd/MM/yyyy");
                firstCol = 6;
            }

            GenerateExcel(ref ew, row, firstCol, main, additionalMeasures);

        }
        #endregion


    }

    /// <summary>
    /// Represents the CampaignAllMeasures table. Inheris the MeasuredParameter class.
    /// </summary>
    public class CampaignAllMeasures : MeasuredParameter
    {

        #region Members
        protected int _campaignGK = -1;
        protected string _campaignName = String.Empty;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public CampaignAllMeasures()
        {
        }

        /// <summary>
        /// A constructor which fills the internal members from the SqlDataReader (db)
        /// </summary>
        /// <param name="dr">The SqlDataReader containing the data read from the database.</param>
        public CampaignAllMeasures(SqlDataReader dr)
        {
            Fill(dr);
        }

        /// <summary>
        /// Additional constructor, this one receives the data reader containing the details of the relevant measured
        /// parameter (campaign in this case), the filters for the account and the measures list.
        /// </summary>
        /// <param name="dr">The SqlDataReader containing the data read from the database.</param>
        /// <param name="filters">The filters to use</param>
        /// <param name="measures">The collection of available measures</param>
        public CampaignAllMeasures(SqlDataReader dr, AccountAlertFilters filters, AlertMeasures measures)
        {
            //Fill our internal data structures.
            Fill(dr);

            //Now check the filters based on the alert measures.
            Filter(measures, filters);
        }

        /// <summary>
        /// Additional constructor, this one receives the data reader containing the details of the relevant measured
        /// parameter (campaign in this case), the filters for the account and the measures list.
        /// </summary>
        /// <param name="dr">The SqlDataReader containing the data read from the database.</param>
        /// <param name="filters">The filters to use</param>
        /// <param name="measures">The collection of available measures</param>
        /// <param name="alertType">The type of alert</param>
        public CampaignAllMeasures(SqlDataReader dr, AccountAlertFilters filters, AlertMeasures measures, AlertType alertType)
        {
            _alertType = alertType;

            Fill(dr);

            Filter(measures, filters);
        }
        #endregion

        #region Properties
        public override int GK
        {
            get
            {
                return _campaignGK;
            }
        }

        public int CampaignGK
        {
            get
            {
                return _campaignGK;
            }
            set
            {
                _campaignGK = value;
            }
        }

        public string CampaignName
        {
            get
            {
                return _campaignName;
            }
            set
            {
                _campaignName = value;
            }
        }
        #endregion

        #region Public Overrides
        /// <summary>
        /// Returns the object as a CSV string.
        /// </summary>
        /// <returns>String (CSV)</returns>
        public override string ToCSV()
        {
            string ret = String.Empty;
            ret = _campaignName + "," + _baseDate.ToString("dd/MM/yyyy") + "," + _currentDate.ToString("dd/MM/yyyy") + "," + _clicksCurrent.ToString() + "," + _clicksCompare.ToString() + "," + _clicksChangeRatio.ToString("N2");
            return ret;
        }

        public override void ToExcel(ref ExcelWorksheet ew, int row, AlertMeasure main, string additionalMeasures)
        {
            ew.InsertRow(row);

            //Campaign Name
            string name = _campaignName;
            if (name.Contains("'"))
                name = name.Replace("'", "");
            ew.Cell(row, 1).Value = name;

            int firstCol = -1;
            if (_alertType == AlertType.Daily)
            {
                //Base Date
                ew.Cell(row, 2).Value = _baseDate.ToString("dd/MM/yyyy");

                //Compare Date
                ew.Cell(row, 3).Value = _currentDate.ToString("dd/MM/yyyy");
                firstCol = 4;
            }
            else
            {
                //Base Start Date
                ew.Cell(row, 2).Value = _compareStartDate.ToString("dd/MM/yyyy");
                ew.Cell(row, 3).Value = _compareEndDate.ToString("dd/MM/yyyy");

                //Compare Date
                ew.Cell(row, 4).Value = _currentStartDate.ToString("dd/MM/yyyy");
                ew.Cell(row, 5).Value = _currentEndDate.ToString("dd/MM/yyyy");
                firstCol = 6;
            }
            
            GenerateExcel(ref ew, row, firstCol, main, additionalMeasures);
        }

        public override string GetMeasureParameterSQL(AlertMeasure am, AlertMeasures measures)
        {
            string ret = base.GetMeasureParameterSQL(am, measures) + "," + CampaignGK.ToString() + ",'" + CampaignName.Replace("'", "") + "'";
            return ret;
        }
        #endregion

        #region Protected Overrides
        protected override void Fill(SqlDataReader dr)
        {
            base.Fill(dr);

            if (!dr.IsDBNull(dr.GetOrdinal("Campaign_gk")))
                _campaignGK = Convert.ToInt32(dr["Campaign_gk"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Campaign_name")))
                _campaignName = dr["Campaign_name"].ToString();

            if (!dr.IsDBNull(dr.GetOrdinal("Clicks_Current")))
                _clicksCurrent = Convert.ToInt32(dr["Clicks_Current"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Clicks_Compare")))
                _clicksCompare = Convert.ToInt32(dr["Clicks_Compare"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Imps_Current")))
                _impsCurrent = Convert.ToInt32(dr["Imps_Current"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Imps_Compare")))
                _impsCompare = Convert.ToInt32(dr["Imps_Compare"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Cost_Current")))
                _costCurrent = Convert.ToInt32(dr["Cost_Current"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Cost_Compare")))
                _costCompare = Convert.ToInt32(dr["Cost_Compare"]);

            if (!dr.IsDBNull(dr.GetOrdinal("BO_New_Users_Current")))
                _boNewUsersCurrent = Convert.ToInt32(dr["BO_New_Users_Current"]);

            if (!dr.IsDBNull(dr.GetOrdinal("BO_New_Users_Compare")))
                _boNewUsersCompare = Convert.ToInt32(dr["BO_New_Users_Compare"]);

            if (!dr.IsDBNull(dr.GetOrdinal("BO_New_Activations_Current")))
                _boNewActivationsCurrent = Convert.ToInt32(dr["BO_New_Activations_Current"]);

            if (!dr.IsDBNull(dr.GetOrdinal("BO_New_Activations_Compare")))
                _boNewActivationsCompare = Convert.ToInt32(dr["BO_New_Activations_Compare"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Purchases_Current")))
                _purchasesCurrent = Convert.ToInt32(dr["Purchases_Current"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Purchases_Compare")))
                _purchasesCompare = Convert.ToInt32(dr["Purchases_Compare"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Signups_Current")))
                _signupsCurrent = Convert.ToInt32(dr["Signups_Current"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Signups_Compare")))
                _signupsCompare = Convert.ToInt32(dr["Signups_Compare"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Leads_Current")))
                _leadsCurrent = Convert.ToInt32(dr["Leads_Current"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Leads_Compare")))
                _leadsCompare = Convert.ToInt32(dr["Leads_Compare"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Convs_Current")))
                _convsCurrent = Convert.ToInt32(dr["Convs_Current"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Convs_Compare")))
                _convsCompare = Convert.ToInt32(dr["Convs_Compare"]);

            if (_clicksCompare != 0)
            {
                float f1 = (float)_convsCompare;
                float f2 = (float)_clicksCompare;
                float f3 = f1 / f2;
                _convRateCompare = Convert.ToDouble(f3);

                f1 = (float)_costCompare;
                f2 = (float)_clicksCompare;
                f3 = f1 / f2;
                _cpcCompare = Convert.ToDouble(f3);
            }

            if (_clicksCurrent != 0)
            {
                float f1 = (float)_convsCurrent;
                float f2 = (float)_clicksCurrent;
                float f3 = f1 / f2;
                _convRateCurrent = Convert.ToDouble(f3);

                f1 = (float)_costCurrent;
                f2 = (float)_clicksCurrent;
                f3 = f1 / f2;
                _cpcCurrent = Convert.ToDouble(f3);
            }

            if (_impsCompare != 0)
            {
                float f1 = (float)_clicksCompare;
                float f2 = (float)_impsCompare;
                float f3 = f1 / f2;
                _ctrCompare = Convert.ToDouble(f3);
            }

            if (_impsCurrent != 0)
            {
                float f1 = (float)_clicksCurrent;
                float f2 = (float)_impsCurrent;
                float f3 = f1 / f2;
                _ctrCurrent = Convert.ToDouble(f3);
            }

            if (_convsCompare != 0)
            {
                float f1 = (float)_costCompare;
                float f2 = (float)_convsCompare;
                float f3 = f1 / f2;
                _costPerConvCompare = Convert.ToDouble(f3);
            }

            if (_convsCurrent != 0)
            {
                float f1 = (float)_costCurrent;
                float f2 = (float)_convsCurrent;
                float f3 = f1 / f2;
                _costPerConvCurrent = Convert.ToDouble(f3);
            }

            if (_boNewUsersCompare != 0)
            {
                float f1 = (float)_costCompare;
                float f2 = (float)_boNewUsersCompare;
                float f3 = f1 / f2;
                _costPerBONewUsersCompare = Convert.ToDouble(f3);
            }

            if (_boNewUsersCurrent != 0)
            {
                float f1 = (float)_costCurrent;
                float f2 = (float)_boNewUsersCurrent;
                float f3 = f1 / f2;
                _costPerBONewUsersCurrent = Convert.ToDouble(f3);
            }
        }
        #endregion

    }

    /// <summary>
    /// All measures related to ad-group
    /// </summary>
    public class AdgroupAllMeasures : CampaignAllMeasures
    {

        #region Members
        protected int _adgroupGK = 0;
        protected string _adgroupName = String.Empty;
        #endregion

        #region Constructors
        public AdgroupAllMeasures()
        {
        }

        public AdgroupAllMeasures(SqlDataReader dr)
        {
            Fill(dr);
        }

        /// <summary>
        /// Additional constructor, this one receives the data reader containing the details of the relevant measured
        /// parameter (adgroups in this case), the filters for the account and the measures list.
        /// </summary>
        /// <param name="dr">The SqlDataReader containing the data read from the database.</param>
        /// <param name="filters">The filters to use</param>
        /// <param name="measures">The collection of available measures</param>
        public AdgroupAllMeasures(SqlDataReader dr, AccountAlertFilters filters, AlertMeasures measures)
        {
            //Fill our internal data structures.
            Fill(dr);

            //Now check the filters based on the alert measures.
            Filter(measures, filters);
        }

        /// <summary>
        /// Additional constructor, this one receives the data reader containing the details of the relevant measured
        /// parameter (adgroups in this case), the filters for the account and the measures list.
        /// </summary>
        /// <param name="dr">The SqlDataReader containing the data read from the database.</param>
        /// <param name="filters">The filters to use</param>
        /// <param name="measures">The collection of available measures</param>
        /// <param name="alertType">The type of alert</param>
        public AdgroupAllMeasures(SqlDataReader dr, AccountAlertFilters filters, AlertMeasures measures, AlertType alertType)
        {
            _alertType = alertType;

            Fill(dr);

            Filter(measures, filters);
        }
        #endregion

        #region Properties
        public override int GK
        {
            get
            {
                return _adgroupGK;
            }
        }

        public int AdgroupGK
        {
            get
            {
                return _adgroupGK;
            }
        }

        public string AdgroupName
        {
            get
            {
                return _adgroupName;
            }
        }
        #endregion

        #region Public Overrides
        public override string ToCSV()
        {
            string ret = String.Empty;
            ret = _adgroupName + "," + _baseDate.ToString("dd/MM/yyyy") + "," + _currentDate.ToString("dd/MM/yyyy") + "," + _clicksCurrent.ToString() + "," + _clicksCompare.ToString() + "," + _clicksChangeRatio.ToString("N2");
            return ret;
        }

        public override void ToExcel(ref ExcelWorksheet ew, int row, AlertMeasure main, string additionalMeasures)
        {
            ew.InsertRow(row);

            //Campaign Name
            string cmpgn = _campaignName;
            if (cmpgn.Contains("'"))
                cmpgn = cmpgn.Replace("'", "");
            ew.Cell(row, 1).Value = cmpgn;

            string name = _adgroupName;
            if (name.Contains("'"))
                name = name.Replace("'", "");
            ew.Cell(row, 2).Value = name;

            //Base = Current.
            int firstCol = -1;
            if (_alertType == AlertType.Daily)
            {
                //Base Date
                ew.Cell(row, 3).Value = _baseDate.ToString("dd/MM/yyyy");

                //Compare Date
                ew.Cell(row, 4).Value = _currentDate.ToString("dd/MM/yyyy");
                firstCol = 5;
            }
            else
            {
                //Base Start Date
                ew.Cell(row, 3).Value = _compareStartDate.ToString("dd/MM/yyyy");
                ew.Cell(row, 4).Value = _compareEndDate.ToString("dd/MM/yyyy");

                //Compare Date
                ew.Cell(row, 5).Value = _currentStartDate.ToString("dd/MM/yyyy");
                ew.Cell(row, 6).Value = _currentEndDate.ToString("dd/MM/yyyy");
                firstCol = 7;
            }

            GenerateExcel(ref ew, row, firstCol, main, additionalMeasures);
        }

        public override string GetMeasureParameterSQL(AlertMeasure am, AlertMeasures measures)
        {
            string ret = base.GetMeasureParameterSQL(am, measures) + "," + AdgroupGK.ToString() + ",'" + AdgroupName.Replace("'", "") + "'";
            return ret;
        }
        #endregion

        #region Protected Overrides
        protected override void Fill(SqlDataReader dr)
        {
            base.Fill(dr);

            if (!dr.IsDBNull(dr.GetOrdinal("Adgroup_gk")))
                _adgroupGK = Convert.ToInt32(dr["Adgroup_gk"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Adgroup_name")))
            {
                _adgroupName = dr["Adgroup_name"].ToString();
                if (_adgroupName.Contains(","))
                    _adgroupName = _adgroupName.Replace(",", "-");
            }
        }
        #endregion

    }

    /// <summary>
    /// All measures related to ad-text
    /// </summary>
    public class AdtextAllMeasures : AdgroupAllMeasures
    {

        #region Members
        protected int _adTextGK = -1;
        protected string _adTextName = String.Empty;
        protected string _adTextDescription = String.Empty;
        #endregion

        #region Constructors
        public AdtextAllMeasures()
        {
        }

        public AdtextAllMeasures(SqlDataReader dr)
        {
            Fill(dr);
        }

        /// <summary>
        /// Additional constructor, this one receives the data reader containing the details of the relevant measured
        /// parameter (adtext in this case), the filters for the account and the measures list.
        /// </summary>
        /// <param name="dr">The SqlDataReader containing the data read from the database.</param>
        /// <param name="filters">The filters to use</param>
        /// <param name="measures">The collection of available measures</param>
        public AdtextAllMeasures(SqlDataReader dr, AccountAlertFilters filters, AlertMeasures measures)
        {
            //Fill our internal data structures.
            Fill(dr);

            //Now check the filters based on the alert measures.
            Filter(measures, filters);
        }

        /// <summary>
        /// Additional constructor, this one receives the data reader containing the details of the relevant measured
        /// parameter (adtext in this case), the filters for the account and the measures list.
        /// </summary>
        /// <param name="dr">The SqlDataReader containing the data read from the database.</param>
        /// <param name="filters">The filters to use</param>
        /// <param name="measures">The collection of available measures</param>
        /// <param name="alertType">The type of alert</param>
        public AdtextAllMeasures(SqlDataReader dr, AccountAlertFilters filters, AlertMeasures measures, AlertType alertType)
        {
            _alertType = alertType;

            Fill(dr);

            Filter(measures, filters);
        }
        #endregion

        #region Properties
        public override int GK
        {
            get
            {
                return _adTextGK;
            }
        }

        public int AdTextGK
        {
            get
            {
                return _adTextGK;
            }
        }

        public string AdTextName
        {
            get
            {
                return _adTextName;
            }
        }

        public string AdTextDescription
        {
            get
            {
                return _adTextDescription;
            }
        }
        #endregion

        #region Public Overrides
        public override string ToCSV()
        {
            return String.Empty;
        }

        public override void ToExcel(ref ExcelWorksheet ew, int row, AlertMeasure main, string additionalMeasures)
        {
            ew.InsertRow(row);

            //Campaign Name
            string cmpgn = _campaignName;
            if (cmpgn.Contains("'"))
                cmpgn = cmpgn.Replace("'", "");
            ew.Cell(row, 1).Value = cmpgn;

            string name = _adgroupName;
            if (name.Contains("'"))
                name = name.Replace("'", "");
            ew.Cell(row, 2).Value = name;

            string adtName = _adgroupName;
            if (adtName.Contains("'"))
                adtName = adtName.Replace("'", "");
            ew.Cell(row, 3).Value = adtName;

            string adtDesc = _adTextDescription;
            if (adtDesc.Contains("'"))
                adtDesc = adtDesc.Replace("'", "");
            ew.Cell(row, 4).Value = adtDesc;

            //Base = Current.
            int firstCol = -1;
            if (_alertType == AlertType.Daily)
            {
                //Base Date
                ew.Cell(row, 5).Value = _baseDate.ToString("dd/MM/yyyy");

                //Compare Date
                ew.Cell(row, 6).Value = _currentDate.ToString("dd/MM/yyyy");
                firstCol = 7;
            }
            else
            {
                //Base Start Date
                ew.Cell(row, 5).Value = _compareStartDate.ToString("dd/MM/yyyy");
                ew.Cell(row, 6).Value = _compareEndDate.ToString("dd/MM/yyyy");

                //Compare Date
                ew.Cell(row, 7).Value = _currentStartDate.ToString("dd/MM/yyyy");
                ew.Cell(row, 8).Value = _currentEndDate.ToString("dd/MM/yyyy");
                firstCol = 9;
            }

            GenerateExcel(ref ew, row, firstCol, main, additionalMeasures);
        }

        public override string GetMeasureParameterSQL(AlertMeasure am, AlertMeasures measures)
        {
            string ret = base.GetMeasureParameterSQL(am, measures) + "," + AdTextGK.ToString() + ",'" + AdTextName.Replace("'", "") + "','" + AdTextDescription.Replace("'", "") + "'";
            return ret;
        }
        #endregion

        #region Protected Overrides
        protected override void Fill(SqlDataReader dr)
        {
            base.Fill(dr);

            if (!dr.IsDBNull(dr.GetOrdinal("Adtext_gk")))
                _adTextGK = Convert.ToInt32(dr["Adtext_gk"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Adtext_name")))
            {
                _adTextName = dr["Adtext_name"].ToString();
                if (_adTextName.Contains(","))
                    _adTextName = _adTextName.Replace(",", "-");
            }

            if (!dr.IsDBNull(dr.GetOrdinal("Adtext_Description")))
            {
                _adTextDescription = dr["Adtext_Description"].ToString();
                if (_adTextDescription.Contains(","))
                    _adTextDescription = _adTextDescription.Replace(",", "-");
            }
        }
        #endregion

    }

    /// <summary>
    /// All measures related to gateways.
    /// </summary>
    public class GatewayAllMeasures : AdgroupAllMeasures
    {

        #region Members
        protected int _gatewayID = -1;
        protected int _gatewayGK = -1;
        protected string _gatewayName = String.Empty;
        #endregion

        #region Constructors
        public GatewayAllMeasures()
        {
        }

        public GatewayAllMeasures(SqlDataReader dr)
        {
            Fill(dr);
        }

        /// <summary>
        /// Additional constructor, this one receives the data reader containing the details of the relevant measured
        /// parameter (gateways in this case), the filters for the account and the measures list.
        /// </summary>
        /// <param name="dr">The SqlDataReader containing the data read from the database.</param>
        /// <param name="filters">The filters to use</param>
        /// <param name="measures">The collection of available measures</param>
        public GatewayAllMeasures(SqlDataReader dr, AccountAlertFilters filters, AlertMeasures measures)
        {
            //Fill our internal data structures.
            Fill(dr);

            //Now check the filters based on the alert measures.
            Filter(measures, filters);
        }

        /// <summary>
        /// Additional constructor, this one receives the data reader containing the details of the relevant measured
        /// parameter (gateways in this case), the filters for the account and the measures list.
        /// </summary>
        /// <param name="dr">The SqlDataReader containing the data read from the database.</param>
        /// <param name="filters">The filters to use</param>
        /// <param name="measures">The collection of available measures</param>
        /// <param name="alertType">The type of alert</param>
        public GatewayAllMeasures(SqlDataReader dr, AccountAlertFilters filters, AlertMeasures measures, AlertType alertType)
        {
            _alertType = alertType;

            Fill(dr);

            Filter(measures, filters);
        }
        #endregion

        #region Properties
        public override int GK
        {
            get
            {
                return _gatewayGK;
            }
        }

        public int GatewayGK
        {
            get
            {
                return _gatewayGK;
            }
        }

        public string GatewayName
        {
            get
            {
                return _gatewayName;
            }
        }

        public int GatewayID
        {
            get
            {
                return _gatewayID;
            }
        }
        #endregion

        #region Public Overrides
        public override string ToCSV()
        {
            return String.Empty;
        }

        public override void ToExcel(ref ExcelWorksheet ew, int row, AlertMeasure main, string additionalMeasures)
        {
            ew.InsertRow(row);

            //Campaign Name
            string cmpgn = _campaignName;
            if (cmpgn.Contains("'"))
                cmpgn = cmpgn.Replace("'", "");
            ew.Cell(row, 1).Value = cmpgn;

            string name = _adgroupName;
            if (name.Contains("'"))
                name = name.Replace("'", "");
            ew.Cell(row, 2).Value = name;

            string gatewayName = _gatewayName;
            if (gatewayName.Contains("'"))
                gatewayName = gatewayName.Replace("'", "");
            ew.Cell(row, 3).Value = gatewayName;

            ew.Cell(row, 4).Value = _gatewayID.ToString();

            //Base = Current.
            //Base = Current.
            int firstCol = -1;
            if (_alertType == AlertType.Daily)
            {
                //Base Date
                ew.Cell(row, 5).Value = _baseDate.ToString("dd/MM/yyyy");

                //Compare Date
                ew.Cell(row, 6).Value = _currentDate.ToString("dd/MM/yyyy");
                firstCol = 7;
            }
            else
            {
                //Base Start Date
                ew.Cell(row, 5).Value = _compareStartDate.ToString("dd/MM/yyyy");
                ew.Cell(row, 6).Value = _compareEndDate.ToString("dd/MM/yyyy");

                //Compare Date
                ew.Cell(row, 7).Value = _currentStartDate.ToString("dd/MM/yyyy");
                ew.Cell(row, 8).Value = _currentEndDate.ToString("dd/MM/yyyy");
                firstCol = 9;
            }

            GenerateExcel(ref ew, row, firstCol, main, additionalMeasures);
        }

        public override string GetMeasureParameterSQL(AlertMeasure am, AlertMeasures measures)
        {
            string ret = base.GetMeasureParameterSQL(am, measures) + "," + GatewayGK.ToString() + ",'" + GatewayName.Replace("'", "") + "'," + GatewayID.ToString();
            return ret;
        }
        #endregion

        #region Protected Overrides
        protected override void Fill(SqlDataReader dr)
        {
            base.Fill(dr);

            if (!dr.IsDBNull(dr.GetOrdinal("Gateway_gk")))
                _gatewayGK = Convert.ToInt32(dr["Gateway_gk"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Gateway_name")))
            {
                _gatewayName = dr["Gateway_name"].ToString();
                if (_gatewayName.Contains(","))
                    _gatewayName = _gatewayName.Replace(",", "-");
            }

            if (!dr.IsDBNull(dr.GetOrdinal("Gateway_id")))
                _gatewayID = Convert.ToInt32(dr["Gateway_id"]);
        }
        #endregion

    }

    /// <summary>
    /// All measures related to keywords.
    /// </summary>
    public class KeywordAllMeasures : AdgroupAllMeasures
    {

        #region Members
        protected int _keywordGK = -1;
        protected string _keywordName = String.Empty;
        #endregion

        #region Constructors
        public KeywordAllMeasures()
        {
        }

        public KeywordAllMeasures(SqlDataReader dr)
        {
            Fill(dr);
        }

        /// <summary>
        /// Additional constructor, this one receives the data reader containing the details of the relevant measured
        /// parameter (keywords in this case), the filters for the account and the measures list.
        /// </summary>
        /// <param name="dr">The SqlDataReader containing the data read from the database.</param>
        /// <param name="filters">The filters to use</param>
        /// <param name="measures">The collection of available measures</param>
        public KeywordAllMeasures(SqlDataReader dr, AccountAlertFilters filters, AlertMeasures measures)
        {
            //Fill our internal data structures.
            Fill(dr);

            //Now check the filters based on the alert measures.
            Filter(measures, filters);
        }

        /// <summary>
        /// Additional constructor, this one receives the data reader containing the details of the relevant measured
        /// parameter (keywords in this case), the filters for the account and the measures list.
        /// </summary>
        /// <param name="dr">The SqlDataReader containing the data read from the database.</param>
        /// <param name="filters">The filters to use</param>
        /// <param name="measures">The collection of available measures</param>
        /// <param name="alertType">The type of alert</param>
        public KeywordAllMeasures(SqlDataReader dr, AccountAlertFilters filters, AlertMeasures measures, AlertType alertType)
        {
            _alertType = alertType;

            Fill(dr);

            Filter(measures, filters);
        }
        #endregion

        #region Properties
        public override int GK
        {
            get
            {
                return _keywordGK;
            }
        }

        public int KeywordGK
        {
            get
            {
                return _keywordGK;
            }
        }

        public string KeywordName
        {
            get
            {
                return _keywordName;
            }
        }
        #endregion

        #region Public Overrides
        public override string ToCSV()
        {
            return String.Empty;
        }

        public override void ToExcel(ref ExcelWorksheet ew, int row, AlertMeasure main, string additionalMeasures)
        {
            ew.InsertRow(row);

            //Campaign Name
            string cmpgn = _campaignName;
            if (cmpgn.Contains("'"))
                cmpgn = cmpgn.Replace("'", "");
            ew.Cell(row, 1).Value = cmpgn;

            string name = _adgroupName;
            if (name.Contains("'"))
                name = name.Replace("'", "");
            ew.Cell(row, 2).Value = name;

            string keywordName = _keywordName;
            if (keywordName.Contains("'"))
                keywordName = keywordName.Replace("'", "");
            ew.Cell(row, 3).Value = keywordName;

            //Base = Current.
            int firstCol = -1;
            if (_alertType == AlertType.Daily)
            {
                //Base Date
                ew.Cell(row, 4).Value = _baseDate.ToString("dd/MM/yyyy");

                //Compare Date
                ew.Cell(row, 5).Value = _currentDate.ToString("dd/MM/yyyy");
                firstCol = 6;
            }
            else
            {
                //Base Start Date
                ew.Cell(row, 4).Value = _compareStartDate.ToString("dd/MM/yyyy");
                ew.Cell(row, 5).Value = _compareEndDate.ToString("dd/MM/yyyy");

                //Compare Date
                ew.Cell(row, 6).Value = _currentStartDate.ToString("dd/MM/yyyy");
                ew.Cell(row, 7).Value = _currentEndDate.ToString("dd/MM/yyyy");
                firstCol = 8;
            }

            GenerateExcel(ref ew, row, firstCol, main, additionalMeasures);
        }

        public override string GetMeasureParameterSQL(AlertMeasure am, AlertMeasures measures)
        {
            string ret = base.GetMeasureParameterSQL(am, measures) + "," + KeywordGK.ToString() + ",'" + KeywordName.Replace("'", "") + "'";
            return ret;
        }
        #endregion

        #region Protected Overrides
        protected override void Fill(SqlDataReader dr)
        {
            base.Fill(dr);

            if (!dr.IsDBNull(dr.GetOrdinal("Keyword_gk")))
                _keywordGK = Convert.ToInt32(dr["Keyword_gk"]);

            if (!dr.IsDBNull(dr.GetOrdinal("Keyword_name")))
            {
                _keywordName = dr["Keyword_name"].ToString();
                if (_keywordName.Contains(","))
                    _keywordName = _keywordName.Replace(",", "-");
            }
        }
        #endregion

    }
}
