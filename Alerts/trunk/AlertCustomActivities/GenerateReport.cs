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

using Easynet.Edge.Core.Workflow;
using System.IO;
using System.Text;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Alerts.Core;

namespace Easynet.Edge.Services.Alerts.AlertCustomActivities
{
	public partial class GenerateReport: BaseAlertActivity
	{

        private string _reportContents = String.Empty;



		public GenerateReport()
		{
			InitializeComponent();
		}


        public static DependencyProperty ReportHeadingProperty = DependencyProperty.Register("ReportHeading", typeof(string), typeof(GenerateReport));

        public string ReportHeading
        {
            get
            {
                return ((string)base.GetValue(GenerateReport.ReportHeadingProperty));
            }
            set
            {
                base.SetValue(GenerateReport.ReportHeadingProperty, value);
            }
        }


        public static DependencyProperty ReportMeasureDiffProperty = DependencyProperty.Register("ReportMeasureDiff", typeof(MeasureDiff), typeof(GenerateReport));

        public MeasureDiff ReportMeasureDiff
        {
            get
            {
                return ((MeasureDiff)base.GetValue(GenerateReport.ReportMeasureDiffProperty));
            }
            set
            {
                base.SetValue(GenerateReport.ReportMeasureDiffProperty, value);
            }
        }


        public static DependencyProperty ReportHeadingsProperty = DependencyProperty.Register("ReportHeadings", typeof(string), typeof(GenerateReport));

        [Description("Reports Headings (Delimited String)")]
        public string ReportHeadings
        {
            get
            {
                return ((string)base.GetValue(GenerateReport.ReportHeadingsProperty));
            }
            set
            {
                base.SetValue(GenerateReport.ReportHeadingsProperty, value);
            }
        }


        public static DependencyProperty ReportEntityTypeProperty = DependencyProperty.Register("ReportEntityType", typeof(EntityTypes), typeof(GenerateReport));

        [Description("The entity type that this report represents")]
        public EntityTypes ReportEntityType
        {
            get
            {
                return ((EntityTypes)base.GetValue(GenerateReport.ReportEntityTypeProperty));
            }
            set
            {
                base.SetValue(GenerateReport.ReportEntityTypeProperty, value);
            }
        }


        public string ReportContents
        {
            get
            {
                return _reportContents;
            }
        }


        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            try
            {
                //Generate CSV report.
                if (ParentWorkflow.InternalParameters.ContainsKey("Reports"))
                {
                    //Get the headings.
                    string headings = ReportHeadings;
                    string additional = GetAdditionalColumns();
                    if (additional != String.Empty)
                    {
                        if (!headings.EndsWith(","))
                            headings += ",";

                        headings += additional;
                    }

                    //Default.
                    ReportType type = ReportType.CSV;
                    if (ParentWorkflow.Parameters.ContainsKey("ReportType"))
                        type = GetReportType(ParentWorkflow.Parameters["ReportType"].ToString());

                    int critical = -1;
                    if (ParentWorkflow.Parameters.ContainsKey("BadThreshold"))
                        critical = Convert.ToInt32(ParentWorkflow.Parameters["BadThreshold"]);

                    //First get the report for the entity type.
                    Hashtable ht = (Hashtable)ParentWorkflow.InternalParameters["Reports"];
                    if (ht.ContainsKey(ReportEntityType))
                    {
                        Reports rs = (Reports)ht[ReportEntityType];
                        if (ReportMeasureDiff != MeasureDiff.Unknown)
                        {
                            Hashtable reports = rs.Get(ReportMeasureDiff);
                            if (reports.Count > 0)
                            {
                                IDictionaryEnumerator ide = reports.GetEnumerator();
                                while (ide.MoveNext())
                                {
                                    Report r = (Report)ide.Value;
                                    r.Heading = ReportHeading;
                                    r.Columns = headings;
                                    rs[ide.Key] = r;
                                }
                            }
                        }

                        ht[ReportEntityType] = rs;
                        ParentWorkflow.InternalParameters["Reports"] = ht;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception at Generate Report: " + ex.ToString());
            }

            return ActivityExecutionStatus.Closed;
        }



        private ReportType GetReportType(string reportType)
        {
            switch (reportType.ToLower())
            {
                case "csv":
                    return ReportType.CSV;

                case "xml":
                    return ReportType.XML;

                case "html":
                    return ReportType.HTML;

                default:
                    return ReportType.Unknown;
            }
        }

        private string GetAdditionalColumns()
        {
            //Add the date columns
            string dates = String.Empty;
            if (_alertType == AlertType.Period)
                dates = "Base_Start_Date,Base_End_Date,Current_Start_Date,Current_End_Date";
            else
                dates = "Base_Date,Current_Date";

            //Get the columns based on the measures we have.
            AlertMeasure main = GetMainMeasure();
            string mainColumns = main.AlertMeasureColumns;
            if (mainColumns == String.Empty ||
                mainColumns == null)
                return dates;

            mainColumns = dates + "," + mainColumns;

            string additionalMeasures = String.Empty;
            if (ParentWorkflow.Parameters.ContainsKey("AdditionalMeasures"))
                additionalMeasures = ParentWorkflow.Parameters["AdditionalMeasures"].ToString();

            if (additionalMeasures == String.Empty ||
                additionalMeasures == null)
                return mainColumns;

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

            if (measures == null)
            {
                Log.Write("Invalid Measures collection - could not find in parameters, and could not generate.", LogMessageType.Error);
                return mainColumns;
            }

            string ret = String.Empty;
            string[] arr = additionalMeasures.Split(',');
            for (int i = 0; i<arr.Length; i++)
            {
                string name = arr[i];
                AlertMeasure am = measures.Get(name);
                if (am != null)
                {
                    //FUTURE - Remove double entries.
                    if (am.CompositeMeasure)
                    {
                        //If we're a composite measure, check if one of our measures was
                        //already displayed.
                    }

                    if (ret == String.Empty)
                        ret += am.AlertMeasureColumns;
                    else
                    {
                        ret += "," + am.AlertMeasureColumns;
                    }
                }
                else
                {
                    Log.Write("Could not find measure with name: " + name + ". In the measures collection.", LogMessageType.Warning);
                }
            }

            mainColumns += "," + ret;
            return mainColumns;
        }

    }
}
