using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;
using System.IO;
using System.Text;

using OfficeOpenXml;
using Easynet.Edge.Core.Workflow;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Alerts.Core;
using Easynet.Edge.Core.Messaging;
using Easynet.Edge.BusinessObjects;
using System.Collections.Generic;

namespace Easynet.Edge.Services.Alerts.AlertCustomActivities
{
	public partial class MasterReport: BaseAlertActivity
	{

		public MasterReport()
		{
			InitializeComponent();
		}


        public static DependencyProperty HasReportProperty = DependencyProperty.Register("HasReport", typeof(bool), typeof(MasterReport));

        public bool HasReport
        {
            get
            {
                return ((bool)base.GetValue(MasterReport.HasReportProperty));
            }
            set
            {
                base.SetValue(MasterReport.HasReportProperty, value);
            }
        }


        private string GetReportTypeString(TimeDeltaType type)
        {
            string ret = String.Empty;
            switch (type)
            {
                case TimeDeltaType.Daily:
                    {
                        ret = "Last_Day";
                        break;
                    }

                case TimeDeltaType.General:
                    {
                        ret = "General";
                        break;
                    }

                case TimeDeltaType.Weekly:
                    {
                        ret = "7_Days_Ago";
                        break;
                    }

                case TimeDeltaType.Monthly:
                    {
                        ret = "30_Days_Ago";
                        break;
                    }
            }

            return ret;
        }

        private string GetAlertTypeString()
        {
            string ret = String.Empty;
            switch (_alertType)
            {
                case AlertType.Daily:
                    {
                        ret = "Daily";
                        break;
                    }

                case AlertType.Period:
                    {
                        ret = "Period";
                        break;
                    }

                default:
                    {
                        ret = "Unknown";
                        break;
                    }
            }

            return ret;
        }

        private void PrepareMessage(bool critical)
        {
            //First urgency.
            MessageUrgency urgency = MessageUrgency.Unknown;
            if (critical)
                urgency = MessageUrgency.High;
            else
                urgency = MessageUrgency.Low;

            if (ParentWorkflow.InternalParameters.ContainsKey("MessageUrgency"))
                ParentWorkflow.InternalParameters["MessageUrgency"] = urgency;
            else
                ParentWorkflow.InternalParameters.Add("MessageUrgency", urgency);

            //Now recipients.
            List<string> recipients = new List<string>();
            string emails = String.Empty;
            if (ParentWorkflow.Parameters.ContainsKey("Emails"))
                emails = ParentWorkflow.Parameters["Emails"].ToString();

            if (emails == String.Empty)
            {
                //See if we have internal emails.
                if (ParentWorkflow.InternalParameters.ContainsKey("Emails"))
                    emails = ParentWorkflow.InternalParameters["Emails"].ToString();
            }

            //Get the users and get their email addresses.
            SystemUsers users = new SystemUsers(AppSettings.GetAbsolute("Easynet.Edge.Core.Domain"));
            foreach (SystemUser su in users)
            {
                if (su.Email != null &&
                    su.Email != String.Empty)
                {
                    if (emails.Contains(su.Email) &&
                        !recipients.Contains(su.Name))
                        recipients.Add(su.Name);
                }
            }

            if (ParentWorkflow.InternalParameters.ContainsKey("MessageRecipients"))
            {
                List<string> cur = (List<string>)ParentWorkflow.InternalParameters["MessageRecipients"];
                cur.AddRange(recipients);
                ParentWorkflow.InternalParameters["MessageRecipients"] = cur;
            }
            else
                ParentWorkflow.InternalParameters.Add("MessageRecipients", recipients);

            //Attachments
            bool attachmentsFound = false;
            if (ParentWorkflow.InternalParameters.ContainsKey("MessageFileName"))
            {
                List<string> attachments = new List<string>();
                attachments.Add(ParentWorkflow.InternalParameters["MessageFileName"].ToString());

                if (ParentWorkflow.InternalParameters.ContainsKey("MessageAttachments"))
                {
                    List<string> current = (List<string>)ParentWorkflow.InternalParameters["MessageAttachments"];
                    current.AddRange(attachments);
                    ParentWorkflow.InternalParameters["MessageAttachments"] = current;
                }
                else
                    ParentWorkflow.InternalParameters.Add("MessageAttachments", attachments);

                attachmentsFound = true;
            }

            //Text and Title
            string curdate = String.Empty;
            if (ParentWorkflow.InternalParameters.ContainsKey("CurrentDate"))
            {
                DateTime t = Convert.ToDateTime(ParentWorkflow.InternalParameters["CurrentDate"]);
                curdate = t.ToString("yyyyMMdd");
            }

            string comp = String.Empty;
            if (ParentWorkflow.InternalParameters.ContainsKey("CompareDate"))
            {
                DateTime t = Convert.ToDateTime(ParentWorkflow.InternalParameters["CompareDate"]);
                comp = t.ToString("yyyyMMdd");
            }

            string date = String.Empty;
            if (ParentWorkflow.InternalParameters.ContainsKey("ReportDate"))
            {
                DateTime t = Convert.ToDateTime(ParentWorkflow.InternalParameters["ReportDate"]);
                date = t.ToString("yyyyMMdd");
            }

            string accountName = "Unknown";
            if (ParentWorkflow.InternalParameters.ContainsKey("AccountName"))
                accountName = ParentWorkflow.InternalParameters["AccountName"].ToString();

            string subject = "Message from easynet Alert System (Account: " + accountName + ")";
            if (curdate != String.Empty)
                subject += " Current Date: " + curdate;

            if (comp != String.Empty)
                subject += " Base Date: " + comp;

            if (date != String.Empty)
                subject += " Report Date: " + date;

            if (ParentWorkflow.InternalParameters.ContainsKey("MessageTitle"))
                ParentWorkflow.InternalParameters["MessageTitle"] = subject;
            else
                ParentWorkflow.InternalParameters.Add("MessageTitle", subject);

            string body = "The attached report contains the information needed.";
            if (!attachmentsFound)
                body = accountName + ": Has Nothing to Report.";

            if (ParentWorkflow.InternalParameters.ContainsKey("MessageText"))
                ParentWorkflow.InternalParameters["MessageText"] = body;
            else
                ParentWorkflow.InternalParameters.Add("MessageText", body);
        }


        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            try
            {
                HasReport = false;
                bool critical = false;
                if (ParentWorkflow.InternalParameters.ContainsKey("Reports"))
                {
                    //Get the reports and generate the file.
                    string accountName = "Unknown";
                    if (ParentWorkflow.InternalParameters.ContainsKey("AccountName"))
                        accountName = ParentWorkflow.InternalParameters["AccountName"].ToString();

                    string outputDir = String.Empty;
                    if (!ParentWorkflow.Parameters.ContainsKey("DefaultReportDirectory"))
                        outputDir = @"c:\temp";
                    else
                        outputDir = ParentWorkflow.Parameters["DefaultReportDirectory"].ToString();

                    TimeDeltaType type = TimeDeltaType.General;
                    if (ParentWorkflow.InternalParameters.ContainsKey("TimeDeltaType"))
                        type = (TimeDeltaType)ParentWorkflow.InternalParameters["TimeDeltaType"];

                    string repType = GetReportTypeString(type);

                    string alertType = GetAlertTypeString();

                    string mainMeasure = String.Empty;
                    if (ParentWorkflow.Parameters.Contains("MainMeasure"))
                        mainMeasure = ParentWorkflow.Parameters["MainMeasure"].ToString();

                    string fileName = alertType + "_" + repType + "_" + accountName + "_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
                    string dir = String.Empty;
                    if (mainMeasure != String.Empty)
                        fileName = mainMeasure + "_" + fileName;

                    if (!outputDir.EndsWith(@"\"))
                        outputDir += @"\";

                    dir = outputDir;
                    outputDir += fileName;

                    string additionalMeasures = String.Empty;
                    if (ParentWorkflow.Parameters.ContainsKey("AdditionalMeasures"))
                        additionalMeasures = ParentWorkflow.Parameters["AdditionalMeasures"].ToString();

                    //First create the excel workbook
                    string wbFileName = @"c:\temp\ReportTemplate.xlsx";
                    wbFileName = AppSettings.GetAbsolute("Easynet.Edge.Alerts.TemplateFileName");

                    if (ParentWorkflow.Parameters.ContainsKey("TemplateFileName"))
                        wbFileName = dir + ParentWorkflow.Parameters["TemplateFileName"].ToString();

                    FileInfo newFile = new FileInfo(outputDir);
                    FileInfo template = new FileInfo(wbFileName);

                    using (ExcelPackage ep = new ExcelPackage(newFile, template))
                    {
                        ExcelWorkbook ew = ep.Workbook;

                        Hashtable reps = (Hashtable)ParentWorkflow.InternalParameters["Reports"];
                        IDictionaryEnumerator ereps = reps.GetEnumerator();

                        AlertMeasure main = GetMainMeasure();
                        StringBuilder sb = new StringBuilder();
                        while (ereps.MoveNext())
                        {
                            HasReport = true;
                            Reports reports = (Reports)ereps.Value;
                            reports.AccountName = accountName;
                            reports.Generate(ref ew, (EntityTypes)ereps.Key, main, additionalMeasures);
                        }

                        //Do another loop - to update one cell in each tab.
                        ereps.Reset();
                        while (ereps.MoveNext())
                        {
                            Reports reports = (Reports)ereps.Value;
                            reports.Finalize(ref ew);

                            if (reports.CriticalThreshold)
                                critical = reports.CriticalThreshold;
                        }


                        ep.Save();

                        ParentWorkflow.InternalParameters.Add("MessageFileName", outputDir);
                    }
                }

                PrepareMessage(critical);

                if (!HasReport)
                    Console.WriteLine("AlertEngine Says: Nothing to report.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception at Master Report: " + ex.ToString());
            }

            return ActivityExecutionStatus.Closed;
        }
	}
}
