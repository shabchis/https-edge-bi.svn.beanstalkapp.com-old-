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

using Easynet.Edge.Alerts.Core;

namespace Easynet.Edge.Services.Alerts.AlertCustomActivities
{
	public partial class AdministrativeReport: BaseAlertActivity
	{

		public AdministrativeReport()
		{
			InitializeComponent();
		}

        
        public static DependencyProperty DifferenceFoundProperty = DependencyProperty.Register("DifferenceFound", typeof(bool), typeof(AdministrativeReport));

        public bool DifferenceFound
        {
            get
            {
                return ((bool)base.GetValue(AdministrativeReport.DifferenceFoundProperty));
            }
            set
            {
                base.SetValue(AdministrativeReport.DifferenceFoundProperty, value);
            }
        }


        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            //Compare the results per account, and see if there is a difference
            if (!ParentWorkflow.InternalParameters.ContainsKey("CSVResults"))
                throw new Exception("Could not find the results from the CSV file.");

            if (!ParentWorkflow.InternalParameters.ContainsKey("OLTPResults"))
                throw new Exception("Could not find the results from the OLTP database.");

            Hashtable csv = (Hashtable)ParentWorkflow.InternalParameters["CSVResults"];
            Hashtable oltp = (Hashtable)ParentWorkflow.InternalParameters["OLTPResults"];
            Hashtable panorama = null;
            Hashtable boResults = null;
            if (ParentWorkflow.InternalParameters.ContainsKey("PanoramaCSVResults"))
                panorama = (Hashtable)ParentWorkflow.InternalParameters["PanoramaCSVResults"];

            if (ParentWorkflow.InternalParameters.ContainsKey("BOResults"))
                boResults = (Hashtable)ParentWorkflow.InternalParameters["BOResults"];

            IDictionaryEnumerator ide = csv.GetEnumerator();
            while (ide.MoveNext())
            {
                AccountAllMeasures csvRecord = (AccountAllMeasures)ide.Value;

                //Get the account ID based on the name of the account from the CSV.
                int accountID = -1;
                try
                {
                    accountID = AccountAllMeasures.FromAccountName(ide.Key.ToString());
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }

                if (accountID <= 0)
                    continue;
                
                if (!oltp.ContainsKey(accountID))
                {
                    //If we couldn't find this account in the OLTP, this means
                    //we have no data about it.
                    DifferenceFound = true;
                }
                else
                {
                    AccountAllMeasures oltpRecord = (AccountAllMeasures)oltp[accountID];

                    //Check if we have a difference. Since we generate the report anyway,
                    //it means that this repoert will be sent to more people.
                    try
                    {
                        if (csvRecord.Different(oltpRecord))
                            DifferenceFound = true;
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                        continue;
                    }
                }
            }

            if (DifferenceFound)
            {
                string rndEmail = "rnd@easynet.co.il";
                if (ParentWorkflow.Parameters.ContainsKey("RNDEmail"))
                    rndEmail = ParentWorkflow.Parameters["RNDEmail"].ToString();
                ParentWorkflow.InternalParameters.Add("Emails", rndEmail);
            }
            else
            {
                string itEmail = "lior@easynet.co.il";
                if (ParentWorkflow.Parameters.ContainsKey("ITEmail"))
                    itEmail = ParentWorkflow.Parameters["ITEmail"].ToString();
                ParentWorkflow.InternalParameters.Add("Emails", itEmail);
            }

            if (!ParentWorkflow.Parameters.ContainsKey("AdminAlertTemplate"))
                throw new Exception("Could not find alert template file.");

            string templateFile = ParentWorkflow.Parameters["AdminAlertTemplate"].ToString();

            try
            {
                DateTime reportDate = DateTime.Now.AddDays(-1);
                if (ParentWorkflow.InternalParameters.ContainsKey("ReportDate"))
                    reportDate = Convert.ToDateTime(ParentWorkflow.InternalParameters["ReportDate"]);

                //Generate the excel report.
                AdminReport ar = new AdminReport(csv, oltp, panorama);
                if (boResults != null)
                    ar.BOResults = boResults;

                ar.Generate(templateFile,reportDate);

                ParentWorkflow.InternalParameters.Add("AccountName", "Edge Administration");
                ParentWorkflow.InternalParameters.Add("MessageFileName", ar.GeneratedReportFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception while trying to generate report. Exception: " + ex.ToString());
            }

            return ActivityExecutionStatus.Closed;
        }
	}
}
