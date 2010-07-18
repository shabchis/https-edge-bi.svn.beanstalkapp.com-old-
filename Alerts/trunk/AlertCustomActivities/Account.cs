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
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;
using System.Collections.Generic;

namespace Easynet.Edge.Services.Alerts.AlertCustomActivities
{
	public partial class Account: BaseAlertActivity
	{
		public Account()
		{
			InitializeComponent();

            _entityType = EntityTypes.Account;
		}


        protected override SqlCommand BuildCommand()
        {
            string sql = String.Empty;
            SqlCommand ret = null;

            switch (_alertType)
            {
                case AlertType.Daily:
                    {
                        sql = "SP_Alerts_AccountAllMeasuresDayDelta";
                        break;
                    }
            }

            ret = DataManager.CreateCommand(sql, System.Data.CommandType.StoredProcedure);

            BuildCommandParameters(ref ret);

            SetCommandParameterValues(ref ret);

            return ret;
        }

        protected override void BuildCommandParameters(ref SqlCommand cmd)
        {
            cmd.Parameters.Add("@channel_id", System.Data.SqlDbType.NVarChar);
            
            switch (_alertType)
            {
                case AlertType.Daily:
                    {
                        cmd.Parameters.Add("@CurrentDayCode", System.Data.SqlDbType.NVarChar);
                        cmd.Parameters.Add("@CompareDayCode", System.Data.SqlDbType.NVarChar);
                        break;
                    }
            }
        }

        protected override void SetCommandParameterValues(ref SqlCommand cmd)
        {
            if (!ParentWorkflow.Parameters.ContainsKey("ChannelID"))
                throw new Exception("Invalid workflow parameters. Could not find Channel ID.");

            string channelID = ParentWorkflow.Parameters["ChannelID"].ToString();
            cmd.Parameters["@channel_id"].Value = channelID;

            InitializeTimes(ref cmd);
        }

        protected override SqlDataReader GetMeasuredData(Hashtable parameters)
        {
            string channelID = parameters["ChannelID"].ToString();
            string sql = String.Empty;

            switch (_alertType)
            {
                case AlertType.Daily:
                    {
                        sql = "SELECT * FROM AccountAllMeasuresDayDelta WHERE Channel_ID = " + channelID.ToString();
                        break;
                    }
            }

            SqlCommand measureTable = DataManager.CreateCommand(sql);
            SqlDataReader sdr = measureTable.ExecuteReader();

            return sdr;
        }


        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            DataManager.ConnectionString = ParentWorkflow.Parameters["ConnectionString"].ToString();
            DataManager.CommandTimeout = 0;

            //Run the stored procedure, based on the params we have.
            try
            {
                string channelID = ParentWorkflow.Parameters["ChannelID"].ToString();

                //Create the command.
                using (DataManager.Current.OpenConnection())
                {
                    SqlCommand accounts = BuildCommand();
                    accounts.ExecuteNonQuery();

                    Easynet.Edge.Alerts.Core.AlertMeasures measures = AlertMeasures;

                    //Now we need to loop on the table and build the list of campaigns and deltas.
                    SqlDataReader sdr = GetMeasuredData(ParentWorkflow.Parameters);
                    string accountName = String.Empty;
                    List<AccountAllMeasures> accountList = new List<AccountAllMeasures>();
                    while (sdr.Read())
                    {
                        AccountAllMeasures aam = new AccountAllMeasures(sdr, measures, _alertType);
                        accountList.Add(aam);
                    }

                    sdr.Close();
                    sdr.Dispose();

                    foreach (AccountAllMeasures aams in accountList)
                    {
                        AccountAlertFilters filters = new AccountAlertFilters(aams.AccountID);
                        aams.Filter(measures, filters);
                        accountName = aams.AccountName;
                        aams.TimeMeasurement = this.TimeMeasurement;
                        aams.MeasurementType = this.MeasurementType;

                        //Only add if we're going to include it in the list (i.e. passed the min)
                        if (aams.Include)
                            _results.Add(aams);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception at Account Campaigns: " + ex.ToString());
                throw ex;
            }

            return ActivityExecutionStatus.Closed;
        }
	}
}
