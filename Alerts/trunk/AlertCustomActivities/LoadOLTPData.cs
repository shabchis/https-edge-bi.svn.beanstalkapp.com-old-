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
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;

namespace Easynet.Edge.Services.Alerts.AlertCustomActivities
{
	public partial class LoadOLTPData: BaseAlertActivity
	{
		public LoadOLTPData()
		{
			InitializeComponent();
		}


        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            DateTime reportDate = DateTime.Now.AddDays(-1);
            if (ParentWorkflow.InternalParameters.ContainsKey("ReportDate"))
                reportDate = Convert.ToDateTime(ParentWorkflow.InternalParameters["ReportDate"]);


            //Execute the query to get the measured parameters for the OLTP for all accounts.
            string sql = @"SELECT Account_Name AccountName,Paid_API_AllColumns.account_id,
                            SUM(imps) as SumOfImps,SUM(clicks) as SumOfClicks,Avg(cpc) AVGCPC,
                            SUM(cost) SumOfCost,Avg(pos) AvgPos,SUM(conv) SumOfConv,SUM(purchases) SumOfPurchase,
                            SUM(leads) SumOfleads,SUM(signups) SumOfSignups
                           FROM easynet_OLTP.dbo.Paid_API_AllColumns,easynet_OLTP.dbo.User_GUI_Account ";

            if (Convert.ToBoolean(ParentWorkflow.InternalParameters["Monthly"]))
            {
                string time = reportDate.Year.ToString();
                if (reportDate.Month.ToString().Length < 2)
                    time += "0" + reportDate.Month.ToString();
                else
                    time += reportDate.Month.ToString();

                sql += "WHERE left(day_code,6) = '" + time +"' ";
            }
            else
            {
                sql += "WHERE day_code =  '" + DayCode.ToDayCode(reportDate) + "' ";
            }

            sql += "AND Paid_API_AllColumns.account_id= User_GUI_Account.Account_ID " +
                    "AND channel_id = 1 " +
                   "GROUP BY Account_Name,Paid_API_AllColumns.account_id " + 
                   "ORDER BY 1";

            DataManager.ConnectionString = ParentWorkflow.Parameters["AdminConnectionString"].ToString();
            DataManager.CommandTimeout = 0;

            using (DataManager.Current.OpenConnection())
            {
                SqlCommand cmd = DataManager.CreateCommand(sql);
                SqlDataReader dr = cmd.ExecuteReader();

                //Loop on the results, and build a hash-table per account. We assume that each
                //account only appears ONCE!.
                Hashtable ht = new Hashtable();
                while (dr.Read())
                {
                    AccountAllMeasures aam = new AccountAllMeasures(dr);
                    ht.Add(aam.AccountID, aam);
                }

                dr.Close();
                dr.Dispose();

                if (!ParentWorkflow.InternalParameters.ContainsKey("OLTPResults"))
                    ParentWorkflow.InternalParameters.Add("OLTPResults", ht);
            }

            return ActivityExecutionStatus.Closed;
        }
	}
}
