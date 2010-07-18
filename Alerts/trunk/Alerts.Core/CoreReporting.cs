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
    /// Represents a collection of report objects.
    /// </summary>
    public class Reports : Hashtable
    {

        #region Constants
        public const int START_ROW = 6;
        #endregion

        #region Members
        private string _accountName = String.Empty;
        private bool _criticalFound = false;
        #endregion

        #region Properties
        public string AccountName
        {
            set
            {
                if (value != null &&
                    value != String.Empty)
                    _accountName = value;
            }
        }

        public bool CriticalThreshold
        {
            get
            {
                return _criticalFound;
            }
        }
        #endregion

        #region Public Methods
        public Hashtable Get(MeasureDiff diff)
        {
            Hashtable ret = new Hashtable();
            if (diff == MeasureDiff.Unknown)
                return ret;

            IDictionaryEnumerator ide = GetEnumerator();
            ide.Reset();

            while (ide.MoveNext())
            {
                Report r = (Report)ide.Value;
                if (r.DifferenceType == diff)
                    ret.Add(ide.Key, ide.Value);
            }

            return ret;
        }

        public Hashtable Get(Measures measure)
        {
            Hashtable ret = new Hashtable();
            if (measure == Measures.Unknown)
                return ret;

            IDictionaryEnumerator ide = GetEnumerator();
            ide.Reset();

            while (ide.MoveNext())
            {
                Report r = (Report)ide.Value;
                if (r.Measures.Contains(measure))
                    ret.Add(ide.Key, ide.Value);
            }

            return ret;
        }

        public Hashtable Get(EntityTypes entityType)
        {
            if (entityType == EntityTypes.Unknown)
                return null;

            IDictionaryEnumerator ide = GetEnumerator();
            ide.Reset();

            while (ide.MoveNext())
            {
                EntityTypes cur = (EntityTypes)ide.Key;
                if (cur == entityType)
                    return (Hashtable)ide.Value;
            }

            return null;
        }

        public bool ContainsDifference(MeasureDiff diff)
        {
            return (Get(diff).Count > 0);
        }

        public bool ContainsMeasure(Measures measure)
        {
            return (Get(measure).Count > 0);
        }

        public bool ContainsEntity(EntityTypes entityType)
        {
            if (entityType == EntityTypes.Unknown)
                return false;

            IDictionaryEnumerator ide = GetEnumerator();
            ide.Reset();

            while (ide.MoveNext())
            {
                EntityTypes cur = (EntityTypes)ide.Key;
                if (cur == entityType)
                    return true;
            }

            return false;
        }

        public void Generate(ref ExcelWorkbook wb, EntityTypes type, AlertMeasure main, string additionalMeasures)
        {
            //Loop on the reports we have, get the worksheet based on the entity type.
            ExcelWorksheet ew = null;
            switch (type)
            {
                case EntityTypes.Account:
                    {
                        ew = wb.Worksheets["Accounts"];
                        break;
                    }

                case EntityTypes.Campaign:
                    {
                        ew = wb.Worksheets["Campaigns"];
                        break;
                    }

                case EntityTypes.Adgroup:
                    {
                        ew = wb.Worksheets["AdGroups"];
                        break;
                    }

                case EntityTypes.Keyword:
                    {
                        ew = wb.Worksheets["Keywords"];
                        break;
                    }

                case EntityTypes.Gateway:
                    {
                        ew = wb.Worksheets["Gateways"];
                        break;
                    }

                case EntityTypes.Adtext:
                    {
                        ew = wb.Worksheets["AdTexts"];
                        break;
                    }
            }

            int row = START_ROW;
            IDictionaryEnumerator ide = GetEnumerator();
            while (ide.MoveNext())
            {
                Report r = (Report)ide.Value;
                r.AccountName = _accountName;
                r.GenerateExcel(ref ew, ref row, main, additionalMeasures);
                if (r.CriticalThreshold)
                    _criticalFound = r.CriticalThreshold;

                row++;
                ew.InsertRow(row);
                row++;
                ew.InsertRow(row);
                row++;
            }
        }

        public void Finalize(ref ExcelWorkbook ew)
        {
            //Dirty fix - if you don't update at least one cell in EVERY
            //worksheet, there's an exception when saving the excel file.
            ExcelWorksheet accounts = ew.Worksheets["Accounts"];
            ExcelWorksheet campaigns = ew.Worksheets["Campaigns"];
            ExcelWorksheet adgroups = ew.Worksheets["AdGroups"];
            ExcelWorksheet keywords = ew.Worksheets["Keywords"];
            ExcelWorksheet gateways = ew.Worksheets["Gateways"];
            ExcelWorksheet adtexts = ew.Worksheets["AdTexts"];

            if (accounts != null)
                accounts.Cell(3, 24).Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            if (campaigns != null)
                campaigns.Cell(3, 24).Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            if (adgroups != null)
                adgroups.Cell(3, 24).Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            if (keywords != null)
                keywords.Cell(3, 24).Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            if (gateways != null)
                gateways.Cell(3, 24).Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            if (adtexts != null)
                adtexts.Cell(3, 24).Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
        }
        #endregion

    }

    /// <summary>
    /// Represents a single report object. A report is a set of ad-units and measures
    /// which fit a specific threshold, a report is per ad-unit, per measure difference.
    /// </summary>
    public class Report
    {

        #region Members
        private MeasuredParameters _measuredParameters = new MeasuredParameters();
        private bool _processed = false;
        private string _contents = String.Empty;
        private ArrayList _measures = new ArrayList();
        private string _name = String.Empty;
        private MeasureDiff _diff = MeasureDiff.Unknown;
        private string _heading = String.Empty;
        private string _columns = String.Empty;
        private static AccountAlertFilters _accountFilters = null;
        private static int _accountID = -1;
        private static string _accountName = String.Empty;
        private bool _criticalFound = false;
        #endregion

        #region Properties
        public MeasuredParameters MeasuredParameters
        {
            get
            {
                return _measuredParameters;
            }
        }

        public bool Processed
        {
            get
            {
                return _processed;
            }
            set
            {
                _processed = value;
            }
        }

        public string Contents
        {
            get
            {
                return _contents;
            }
        }

        public ArrayList Measures
        {
            get
            {
                return _measures;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != String.Empty &&
                    value != null)
                    _name = value;
            }
        }

        public MeasureDiff DifferenceType
        {
            get
            {
                return _diff;
            }
            set
            {
                _diff = value;
            }
        }

        public string Heading
        {
            get
            {
                return _heading;
            }
            set
            {
                if (value != String.Empty &&
                    value != null)
                    _heading = value;
            }
        }

        public string Columns
        {
            set
            {
                if (value != null &&
                    value != String.Empty)
                    _columns = value;
            }
        }

        public AccountAlertFilters Filters
        {
            set
            {
                _accountFilters = value;
            }
        }

        public int Account
        {
            set
            {
                _accountID = value;
            }
        }

        public string AccountName
        {
            set
            {
                if (value != null &&
                    value != String.Empty)
                    _accountName = value;
            }
        }

        public bool CriticalThreshold
        {
            get
            {
                return _criticalFound;
            }
        }
        #endregion

        #region Static Properties
        public static AccountAlertFilters AccountFilters
        {
            get
            {
                return _accountFilters;
            }
        }

        public static int AccountID
        {
            get
            {
                return _accountID;
            }
        }
        #endregion

        #region Public Methods
        public bool Generate(ReportType type, string headings)
        {
            if (type == ReportType.Unknown)
                return false;

            if (_measuredParameters.Count <= 0 ||
                _measuredParameters == null)
                return false;

            if (type == ReportType.CSV)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(_heading);
                sb.AppendLine();
                sb.AppendLine(_measuredParameters.ToCSV(headings));
                _contents = sb.ToString();
            }

            _processed = true;

            return true;
        }

        public bool GenerateExcel(ref ExcelWorksheet ew, ref int row, AlertMeasure main, string additionalMeasures)
        {
            //Add rows into the excel worksheet.
            if (_measuredParameters.Count <= 0)
                return false;

            ew.Cell(row, 3).Value = _accountName;
            //First put the direction (falling/rising).
            row++;
            ew.InsertRow(row);
            ew.Cell(row, 1).Value = _heading;
            row++;
            ew.InsertRow(row);
            row++;

            //Now the headers (columns)
            ew.InsertRow(row);
            GenerateHeaders(ref ew, row, main);

            row++;
            _measuredParameters.ToExcel(ref ew, ref row, main, additionalMeasures);

            _criticalFound = _measuredParameters.CriticalThreshold;

            _processed = true;

            return true;
        }
        #endregion

        #region Private Methods
        private void GenerateHeaders(ref ExcelWorksheet ew, int row, AlertMeasure main)
        {
            string[] arr = _columns.Split(',');
            ArrayList a = new ArrayList(arr);

            int count = 0;
            for (int i = 0; i < a.Count; i++)
            {
                ew.Cell(row, i + 1).Value = a[i].ToString();
                if (a[i].ToString().IndexOf(main.AlertMeasureName) != -1)
                {
                    ew.Cell(row, i + 1).Style = "Heading 4";
                    count++;
                }
                else
                {
                    //If we're here, and we already "colored" two cells, it means that we're in the
                    //third cell - the "Difference (%)" which we also need to color.
                    if (count == 2)
                    {
                        ew.Cell(row, i + 1).Style = "Heading 4";
                        count = 0;
                    }
                }
            }
        }
        #endregion

    }

    public class AdminReport
    {

        #region Constants
        public const int ADMIN_START_ROW = 2;
        #endregion

        #region Members
        private Hashtable _source = null; //Source data (Google for example)
        private Hashtable _compare = null; //Compare data (internal easynet data).
        private Hashtable _bi = null; //BI data (Panorama).
        private Hashtable _bo = null; //BO Data.
        private string _generatedFileName = String.Empty;
        #endregion

        #region Constructors
        public AdminReport(Hashtable source, Hashtable compare)
        {
            _source = source;
            _compare = compare;
        }

        public AdminReport(Hashtable source, Hashtable compare, Hashtable bi)
        {
            _source = source;
            _compare = compare;
            _bi = bi;
        }
        #endregion

        #region Properties
        public string GeneratedReportFile
        {
            get
            {
                return _generatedFileName;
            }
        }

        public Hashtable BOResults
        {
            set
            {
                if (value != null)
                    _bo = value;
            }
        }
        #endregion

        #region Public Methods
        public void Generate(string templateFileName, DateTime date)
        {
            if (templateFileName == String.Empty ||
                templateFileName == null)
                throw new ArgumentException("Invalid template file name. Cannot be null or empty");

            if (!File.Exists(templateFileName))
                throw new FileNotFoundException("Could not find templat efile: " + templateFileName);

            if (_source == null ||
                _compare == null)
                throw new Exception("Invalid source or compare data. Cannot be null.");

            string reportFilePath = Path.GetDirectoryName(templateFileName);
            if (!reportFilePath.EndsWith("\\"))
                reportFilePath += "\\";

            string reportFileName = "AdminAlert_" + DayCode.ToDayCode(date).ToString() + ".xlsx";
            reportFileName = reportFilePath + reportFileName;

            FileInfo newFile = new FileInfo(reportFileName);
            FileInfo template = new FileInfo(templateFileName);

            using (ExcelPackage ep = new ExcelPackage(newFile, template))
            {
                ExcelWorkbook ew = ep.Workbook;

                ExcelWorksheet worksheet = ew.Worksheets["AdWords"];

                //Start inserting rows from the begining.
                int row = ADMIN_START_ROW;
                IDictionaryEnumerator ide = _source.GetEnumerator();
                while (ide.MoveNext())
                {
                    AccountAllMeasures csv = (AccountAllMeasures)ide.Value;

                    //First the CSV Row.
                    worksheet.Cell(row, 1).Value = "Adwords - " + csv.AccountName;
                    worksheet.Cell(row, 2).Value = csv.ImpsCurrent.ToString();
                    worksheet.Cell(row, 3).Value = csv.ClicksCurrent.ToString();
                    worksheet.Cell(row, 4).Value = csv.CPCCurrent.ToString();
                    worksheet.Cell(row, 5).Value = csv.CostCurrent.ToString();
                    worksheet.Cell(row, 6).Value = csv.AveragePosition.ToString();
                    worksheet.Cell(row, 7).Value = csv.ConversionsCurrent.ToString();
                    worksheet.Cell(row, 8).Value = csv.PurchasesCurrent.ToString();
                    worksheet.Cell(row, 9).Value = csv.LeadsCurrent.ToString();
                    worksheet.Cell(row, 10).Value = csv.SignupsCurrent.ToString();

                    //Now - OLTP
                    int id = -1;
                    try
                    {
                        id = AccountAllMeasures.FromAccountName(csv.AccountName);
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }

                    if (_compare.ContainsKey(id))
                    {
                        AccountAllMeasures oltp = (AccountAllMeasures)_compare[id];
                        worksheet.Cell(row + 1, 1).Value = "OLTP - " + oltp.AccountName;
                        worksheet.Cell(row + 1, 2).Value = oltp.ImpsCurrent.ToString();
                        worksheet.Cell(row + 1, 3).Value = oltp.ClicksCurrent.ToString();
                        worksheet.Cell(row + 1, 4).Value = oltp.CPCCurrent.ToString();
                        worksheet.Cell(row + 1, 5).Value = oltp.CostCurrent.ToString();
                        worksheet.Cell(row + 1, 6).Value = oltp.AveragePosition.ToString();
                        worksheet.Cell(row + 1, 7).Value = oltp.ConversionsCurrent.ToString();
                        worksheet.Cell(row + 1, 8).Value = oltp.PurchasesCurrent.ToString();
                        worksheet.Cell(row + 1, 9).Value = oltp.LeadsCurrent.ToString();
                        worksheet.Cell(row + 1, 10).Value = oltp.SignupsCurrent.ToString();

                        if (csv.ImpsCurrent != oltp.ImpsCurrent)
                            worksheet.Cell(row + 1, 2).Style = "Bad";
                        
                        if (csv.ClicksCurrent != oltp.ClicksCurrent)
                            worksheet.Cell(row + 1, 3).Style = "Bad";

                        if (csv.CPCCurrent != oltp.CPCCurrent)
                            worksheet.Cell(row + 1, 4).Style = "Bad";

                        if (csv.CostCurrent != oltp.CostCurrent)
                            worksheet.Cell(row + 1, 5).Style = "Bad";

                        if (csv.AveragePosition != oltp.AveragePosition)
                            worksheet.Cell(row + 1, 6).Style = "Bad";

                        if (csv.ConversionsCurrent != oltp.ConversionsCurrent)
                            worksheet.Cell(row + 1, 7).Style = "Bad";

                        if (csv.PurchasesCurrent != oltp.PurchasesCurrent)
                            worksheet.Cell(row + 1, 8).Style = "Bad";

                        if (csv.LeadsCurrent != oltp.LeadsCurrent)
                            worksheet.Cell(row + 1, 9).Style = "Bad";

                        if (csv.SignupsCurrent != oltp.SignupsCurrent)
                            worksheet.Cell(row + 1, 10).Style = "Bad";
                    }
                    else
                    {
                        //Couldn't find it. Put 0 in everyone, and mark them all as "Bad".
                        worksheet.Cell(row + 1, 1).Value = "OLTP - " + csv.AccountName;
                        worksheet.Cell(row + 1, 2).Value = "0";
                        worksheet.Cell(row + 1, 2).Style = "Bad";
                        worksheet.Cell(row + 1, 3).Value = "0";
                        worksheet.Cell(row + 1, 3).Style = "Bad";
                        worksheet.Cell(row + 1, 4).Value = "0";
                        worksheet.Cell(row + 1, 4).Style = "Bad";
                        worksheet.Cell(row + 1, 5).Value = "0";
                        worksheet.Cell(row + 1, 5).Style = "Bad";
                        worksheet.Cell(row + 1, 6).Value = "0";
                        worksheet.Cell(row + 1, 6).Style = "Bad";
                        worksheet.Cell(row + 1, 7).Value = "0";
                        worksheet.Cell(row + 1, 7).Style = "Bad";
                        worksheet.Cell(row + 1, 8).Value = "0";
                        worksheet.Cell(row + 1, 8).Style = "Bad";
                        worksheet.Cell(row + 1, 9).Value = "0";
                        worksheet.Cell(row + 1, 9).Style = "Bad";
                        worksheet.Cell(row + 1, 10).Value = "0";
                        worksheet.Cell(row + 1, 10).Style = "Bad";
                    }

                    //Now panorama
                    string accountName = AccountAllMeasures.FromGoogleAccountName(csv.AccountName);
                    if (_bi != null && _bi.ContainsKey(accountName))
                    {
                        AccountAllMeasures panorama = (AccountAllMeasures)_bi[accountName];
                        worksheet.Cell(row + 2, 1).Value = "PANORAMA - " + panorama.AccountName;
                        worksheet.Cell(row + 2, 2).Value = panorama.ImpsCurrent.ToString();
                        worksheet.Cell(row + 2, 3).Value = panorama.ClicksCurrent.ToString();
                        worksheet.Cell(row + 2, 4).Value = panorama.CPCCurrent.ToString();
                        worksheet.Cell(row + 2, 5).Value = panorama.CostCurrent.ToString();
                        worksheet.Cell(row + 2, 6).Value = panorama.AveragePosition.ToString();
                        worksheet.Cell(row + 2, 7).Value = panorama.ConversionsCurrent.ToString();
                        worksheet.Cell(row + 2, 8).Value = panorama.PurchasesCurrent.ToString();
                        worksheet.Cell(row + 2, 9).Value = panorama.LeadsCurrent.ToString();
                        worksheet.Cell(row + 2, 10).Value = panorama.SignupsCurrent.ToString();

                        if (csv.ImpsCurrent != panorama.ImpsCurrent)
                            worksheet.Cell(row + 1, 2).Style = "Bad";

                        if (csv.ClicksCurrent != panorama.ClicksCurrent)
                            worksheet.Cell(row + 1, 3).Style = "Bad";

                        if (csv.CPCCurrent != panorama.CPCCurrent)
                            worksheet.Cell(row + 1, 4).Style = "Bad";

                        if (csv.CostCurrent != panorama.CostCurrent)
                            worksheet.Cell(row + 1, 5).Style = "Bad";

                        if (csv.AveragePosition != panorama.AveragePosition)
                            worksheet.Cell(row + 1, 6).Style = "Bad";

                        if (csv.ConversionsCurrent != panorama.ConversionsCurrent)
                            worksheet.Cell(row + 1, 7).Style = "Bad";

                        if (csv.PurchasesCurrent != panorama.PurchasesCurrent)
                            worksheet.Cell(row + 1, 8).Style = "Bad";

                        if (csv.LeadsCurrent != panorama.LeadsCurrent)
                            worksheet.Cell(row + 1, 9).Style = "Bad";

                        if (csv.SignupsCurrent != panorama.SignupsCurrent)
                            worksheet.Cell(row + 1, 10).Style = "Bad";
                    }
                    else
                    {
                        //Couldn't find it. Put 0 in everyone, and mark them all as "Bad".
                        worksheet.Cell(row + 2, 1).Value = "PANORAMA - " + csv.AccountName;
                        worksheet.Cell(row + 2, 2).Value = "0";
                        worksheet.Cell(row + 2, 2).Style = "Bad";
                        worksheet.Cell(row + 2, 3).Value = "0";
                        worksheet.Cell(row + 2, 3).Style = "Bad";
                        worksheet.Cell(row + 2, 4).Value = "0";
                        worksheet.Cell(row + 2, 4).Style = "Bad";
                        worksheet.Cell(row + 2, 5).Value = "0";
                        worksheet.Cell(row + 2, 5).Style = "Bad";
                        worksheet.Cell(row + 2, 6).Value = "0";
                        worksheet.Cell(row + 2, 6).Style = "Bad";
                        worksheet.Cell(row + 2, 7).Value = "0";
                        worksheet.Cell(row + 2, 7).Style = "Bad";
                        worksheet.Cell(row + 2, 8).Value = "0";
                        worksheet.Cell(row + 2, 8).Style = "Bad";
                        worksheet.Cell(row + 2, 9).Value = "0";
                        worksheet.Cell(row + 2, 9).Style = "Bad";
                        worksheet.Cell(row + 2, 10).Value = "0";
                        worksheet.Cell(row + 2, 10).Style = "Bad";
                    }

                    //Advance 3 rows.
                    
                    row+= 3;
                }

                worksheet.Cell(ADMIN_START_ROW + 1, 16).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                worksheet.Cell(ADMIN_START_ROW + 2, 16).Value = date.ToString("dd/MM/yyyy HH:mm:ss");

                ExcelWorksheet boHistory = ew.Worksheets["BOHistory"];
                ExcelWorksheet bo_ef = ew.Worksheets["BO_EF"];

                if (_bo != null)
                {
                    //Add the BO sheet.
                    int boRow = ADMIN_START_ROW;
                    IDictionaryEnumerator boe = _bo.GetEnumerator();

                    if (boHistory != null)
                    {
                        while (boe.MoveNext())
                        {
                            AccountAllMeasures boData = (AccountAllMeasures)boe.Value;

                            boHistory.Cell(boRow, 1).Value = boData.AccountName;
                            boHistory.Cell(boRow, 2).Value = boData.ClicksCurrent.ToString();
                            boHistory.Cell(boRow, 3).Value = boData.LeadsCurrent.ToString();
                            boHistory.Cell(boRow, 4).Value = boData.BONewUsersCurrent.ToString();
                            boHistory.Cell(boRow, 5).Value = boData.BONewActivationsCurrent.ToString();
                            boHistory.Cell(boRow, 6).Value = boData.SumOfActiveUsers.ToString();
                            boHistory.Cell(boRow, 7).Value = boData.SumOfNewNetDeposits.ToString();
                            boHistory.Cell(boRow, 8).Value = boData.SumOfTotalNetDeposits.ToString();
                            boHistory.Cell(boRow, 9).Value = boData.SumOfClientSpecific1.ToString();
                            boHistory.Cell(boRow, 10).Value = boData.SumOfClientSpecific2.ToString();
                            boHistory.Cell(boRow, 11).Value = boData.SumOfClientSpecific3.ToString();
                            boHistory.Cell(boRow, 12).Value = boData.SumOfClientSpecific4.ToString();
                            boHistory.Cell(boRow, 13).Value = boData.SumOfClientSpecific5.ToString();

                            boRow++;
                        }
                    }

                    boe.Reset();

                    boRow = ADMIN_START_ROW;

                    if (bo_ef != null)
                    {
                        while (boe.MoveNext())
                        {
                            AccountAllMeasures boef = (AccountAllMeasures)boe.Value;

                            //Hack for easy forex only!
                            if (boef.AccountID == 7)
                            {
                                bo_ef.Cell(boRow, 2).Value = boef.LeadsCurrent.ToString();
                                bo_ef.Cell(boRow, 3).Value = boef.BONewUsersCurrent.ToString();
                                bo_ef.Cell(boRow, 4).Value = boef.BONewActivationsCurrent.ToString();
                                bo_ef.Cell(boRow, 5).Value = boef.SumOfActiveUsers.ToString();
                                bo_ef.Cell(boRow, 6).Value = boef.SumOfNewNetDeposits.ToString();
                                bo_ef.Cell(boRow, 7).Value = boef.SumOfTotalNetDeposits.ToString();
                            }
                        }
                    }
                }
                else
                {
                    boHistory.Cell(100, 1).Value = "1";
                    bo_ef.Cell(100, 1).Value = "1";
                }

                ep.Save();

                _generatedFileName = reportFileName;
            }
        }
        #endregion

    }

}