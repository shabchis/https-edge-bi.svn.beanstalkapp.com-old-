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
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;

using Easynet.Edge.Core.Workflow;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Data;
using System.Configuration;
using System.Collections.Specialized;
using Easynet.Edge.Alerts.Core;

namespace Easynet.Edge.Services.Alerts.AlertCustomActivities
{
    public partial class AccountCampaigns : BaseAlertActivity
    {

        #region Constructor
        public AccountCampaigns()
		{
			InitializeComponent();

            _entityType = EntityTypes.Campaign;
        }
        #endregion

        #region Private Methodss
        private void BuildCampaignGKList()
        {
            ArrayList gks = new ArrayList();
            for (int i = 0; i < _results.Count; i++)
            {
                CampaignAllMeasures cam = (CampaignAllMeasures)_results[i];
                gks.Add(cam.CampaignGK);
            }

            if (ParentWorkflow.InternalParameters.Contains("CampaignGKList"))
            {
                ArrayList gkList = (ArrayList)ParentWorkflow.InternalParameters["CampaignGKList"];
                for (int i = 0; i < gkList.Count; i++)
                {
                    if (!gks.Contains(gkList[i]))
                        gks.Add(gkList[i]);
                }

                ParentWorkflow.InternalParameters["CampaignGKList"] = gks;
            }
            else
            {
                ParentWorkflow.InternalParameters.Add("CampaignGKList", gks);
            }
        }
        #endregion

        #region Alert Core Overrides
        protected override SqlCommand BuildCommand()
        {
            string sql = String.Empty;
            SqlCommand ret = null;

            switch (_alertType)
            {
                case AlertType.Daily:
                    {
                        sql = "SP_Alerts_CampaignPerAccountAllMeasuresDayDelta";
                        break;
                    }

                case AlertType.Period:
                    {
                        sql = "SP_Alerts_CampaignPerAccountAllMeasuresPeriod";
                        break;
                    }
            }

            ret = DataManager.CreateCommand(sql, System.Data.CommandType.StoredProcedure);

            BuildCommandParameters(ref ret);

            SetCommandParameterValues(ref ret);

            return ret;
        }

        protected override SqlDataReader GetMeasuredData(Hashtable parameters)
        {
            int accountID = Convert.ToInt32(parameters["AccountID"]);
            int channelID = Convert.ToInt32(parameters["ChannelID"]);
            string sql = String.Empty;

            switch (_alertType)
            {
                case AlertType.Daily:
                    {
                        sql = "SELECT * FROM CampaignPerAccountAllMeasuresDayDelta WHERE Account_ID = " + accountID.ToString() + " AND Channel_ID = " + channelID.ToString();
                        break;
                    }

                case AlertType.Period:
                    {
                        sql = "SELECT * FROM CampaignPerAccountAllMeasuresPeriod WHERE Account_ID = " + accountID.ToString() + " AND Channel_ID = " + channelID.ToString();
                        break;
                    }
            }

            SqlCommand measureTable = DataManager.CreateCommand(sql);
            SqlDataReader sdr = measureTable.ExecuteReader();

            return sdr;
        }
        #endregion

        #region Execute
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            DataManager.ConnectionString = ParentWorkflow.Parameters["ConnectionString"].ToString();
            DataManager.CommandTimeout = 0;

            //Run the stored procedure, based on the params we have.
            try
            {
                int accountID = Convert.ToInt32(ParentWorkflow.Parameters["AccountID"]);
                int channelID = Convert.ToInt32(ParentWorkflow.Parameters["ChannelID"]);

                //Create the command.
                using (DataManager.Current.OpenConnection())
                {
                    SqlCommand accountCampaigns = BuildCommand();
                    accountCampaigns.ExecuteNonQuery();

                    AccountAlertFilters filters = Filters;
                    Easynet.Edge.Alerts.Core.AlertMeasures measures = AlertMeasures;

                    //Now we need to loop on the table and build the list of campaigns and deltas.
                    SqlDataReader sdr = GetMeasuredData(ParentWorkflow.Parameters);
                    string accountName = String.Empty;
                    while (sdr.Read())
                    {
                        CampaignAllMeasures cam = new CampaignAllMeasures(sdr, filters, measures, _alertType);
                        accountName = cam.AccountName;
                        cam.TimeMeasurement = this.TimeMeasurement;
                        cam.MeasurementType = this.MeasurementType;

                        //Only add if we're going to include it in the list (i.e. passed the min)
                        if (cam.Include)
                            _results.Add(cam);
                    }

                    ParentWorkflow.InternalParameters.Add("AccountName", accountName);

                    //Add a list of all campaign GK's we have, so in case someone wants to use
                    //ad groups after us without giving a specific campaign GK. They'll have them.
                    BuildCampaignGKList();

                    sdr.Close();
                    sdr.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception at Account Campaigns: " + ex.ToString());
                throw ex;
            }

            return ActivityExecutionStatus.Closed;
        }
        #endregion

    }
}

