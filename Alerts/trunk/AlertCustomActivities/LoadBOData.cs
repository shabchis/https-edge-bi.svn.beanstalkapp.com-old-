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
	public partial class LoadBOData: BaseAlertActivity
	{
		public LoadBOData()
		{
			InitializeComponent();
		}


        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (Convert.ToBoolean(ParentWorkflow.InternalParameters["Monthly"]))
            {
                return ActivityExecutionStatus.Closed;
            }

            DateTime reportDate = DateTime.Now.AddDays(-1);
            if (ParentWorkflow.InternalParameters.ContainsKey("ReportDate"))
                reportDate = Convert.ToDateTime(ParentWorkflow.InternalParameters["ReportDate"]);


            //Execute the query to get the measured parameters for the OLTP for all accounts.
            string sql = @"SELECT BO.Account_ID, Account_Name AccountName, sum(BOClicks) AS SumOfClicks,
                            SUM(new_leads) AS SumOfLeads, SUM(new_users) AS SumOfNewUsers,
                            SUM(new_active_users) AS SumOfNewActiveUsers, SUM(active_users) AS SumOfActiveUsers,
                            SUM(new_net_deposits_in_dollars) SumOfNewNetDeposits, SUM(total_net_deposits_in_dollars) SumOfTotalNetDeposits,
                            SUM(clientspecific1) AS SumOfClientSpecific1, SUM(clientspecific2) AS SumOfClientSpecific2,
                            SUM(clientspecific3) AS SumOfClientSpecific3, SUM(clientspecific4) AS SumOfClientSpecific4,
                            SUM(clientspecific5) AS SumOfClientSpecific5
                            FROM easynet_oltp.dbo.BackOffice_Client_Gateway BO,easynet_oltp.dbo.User_GUI_Account ";

            sql += "WHERE day_code =  '" + DayCode.ToDayCode(reportDate) + "' ";
            sql += "AND BO.account_id = User_GUI_Account.Account_ID GROUP BY Account_Name,BO.Account_id,day_code ORDER BY 2";

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
                    AccountAllMeasures aam = new AccountAllMeasures(dr,true);
                    ht.Add(aam.AccountID, aam);
                }

                dr.Close();
                dr.Dispose();

                if (!ParentWorkflow.InternalParameters.ContainsKey("BOResults"))
                    ParentWorkflow.InternalParameters.Add("BOResults", ht);
            }

            return ActivityExecutionStatus.Closed;
        }
	}
}
