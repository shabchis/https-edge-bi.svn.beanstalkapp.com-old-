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
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;
using Easynet.Edge.Core.Utilities;

namespace Easynet.Edge.Services.Alerts.AlertCustomActivities
{
	public partial class LoadDWHData: BaseAlertActivity
	{
		public LoadDWHData()
		{
			InitializeComponent();
		}


        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            DateTime reportDate = DateTime.Now.AddDays(-1);
            if (ParentWorkflow.InternalParameters.ContainsKey("ReportDate"))
                reportDate = Convert.ToDateTime(ParentWorkflow.InternalParameters["ReportDate"]);


            //Execute the query to get the measured parameters for the OLTP for all accounts.
            string sql = @"SELECT Account_Name AccountName,User_GUI_Account.account_id,
                            SUM(impressions) as SumOfImps,SUM(clicks) as SumOfClicks,
                            SUM(cost) SumOfCost,SUM(conv) SumOfConv,SUM(purchases) SumOfPurchase,
                            SUM(leads) SumOfleads,SUM(signups) SumOfSignups
                           FROM easynet_dwh.dbo.Dwh_Fact_PPC_Campaigns, easynet_OLTP.dbo.User_GUI_Account ";

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

            sql += "AND Dwh_Fact_PPC_Campaigns.account_id = User_GUI_Account.account_id " +
                    "AND channel_id = 1 " +
                   "GROUP BY Account_Name,User_GUI_Account.account_id,channel_id " + 
                   "ORDER BY 1,2";

            DataManager.ConnectionString = ParentWorkflow.Parameters["AdminConnectionString"].ToString();
            DataManager.CommandTimeout = 0;

            using (DataManager.Current.OpenConnection())
            {
                SqlCommand cmd = DataManager.CreateCommand(sql);
                SqlDataReader dr = cmd.ExecuteReader();

                dr.Close();
                dr.Dispose();
            }
            
            return ActivityExecutionStatus.Closed;
        }


	}
}
