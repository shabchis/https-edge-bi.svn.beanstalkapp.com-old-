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
using System.Collections.Specialized;
using System.Configuration;

using Easynet.Edge.Core.Workflow;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Alerts.Core;

namespace Easynet.Edge.Services.Alerts.AlertCustomActivities
{
	public partial class AdgroupAdtexts: BaseAlertActivity
	{
		public AdgroupAdtexts()
		{
			InitializeComponent();

            _entityType = EntityTypes.Adtext;
		}


        private void LoadAdtextsForAdgroup(int accountID, int channelID, int campaignGK, int adgroupGK)
        {
            using (DataManager.Current.OpenConnection())
            {
                SqlCommand adgroupKeywords = BuildCommand();

                adgroupKeywords.Parameters["@Campaign_GK"].Value = campaignGK.ToString();
                adgroupKeywords.Parameters["@adgroup_GK"].Value = adgroupGK.ToString();

                adgroupKeywords.ExecuteNonQuery();

                Hashtable parameters = new Hashtable();
                parameters.Add("AccountID", accountID);
                parameters.Add("ChannelID", channelID);
                parameters.Add("CampaignGK", campaignGK);
                parameters.Add("AdgroupGK", adgroupGK);

                //Now we need to loop on the table and build the list of campaigns and deltas.
                SqlDataReader sdr = GetMeasuredData(parameters);
                string accountName = String.Empty;
                while (sdr.Read())
                {
                    AdtextAllMeasures aam = new AdtextAllMeasures(sdr, Filters,AlertMeasures,_alertType);
                    accountName = aam.AccountName;
                    aam.TimeMeasurement = this.TimeMeasurement;
                    aam.MeasurementType = this.MeasurementType;

                    if (aam.Include)
                        _results.Add(aam);
                }

                if (!ParentWorkflow.InternalParameters.Contains("AccountName"))
                    ParentWorkflow.InternalParameters.Add("AccountName", accountName);

                sdr.Close();
                sdr.Dispose();

            }
        }


        protected override SqlCommand BuildCommand()
        {
            string sql = String.Empty;
            SqlCommand ret = null;

            switch (_alertType)
            {
                case AlertType.Daily:
                    {
                        sql = "SP_Alerts_AdtextPerAdgroupAllMeasuresDayDelta";
                        break;
                    }

                case AlertType.Period:
                    {
                        sql = "SP_Alerts_AdtextPerAdgroupAllMeasuresPeriod";
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
            base.BuildCommandParameters(ref cmd);

            cmd.Parameters.Add("@Campaign_GK", System.Data.SqlDbType.NVarChar);
            cmd.Parameters.Add("@adgroup_GK", System.Data.SqlDbType.NVarChar);
        }

        protected override SqlDataReader GetMeasuredData(Hashtable parameters)
        {
            int accountID = Convert.ToInt32(parameters["AccountID"]);
            int channelID = Convert.ToInt32(parameters["ChannelID"]);
            int campaignGK = Convert.ToInt32(parameters["CampaignGK"]);
            int adgroupGK = Convert.ToInt32(parameters["AdgroupGK"]);

            string sql = String.Empty;

            switch (_alertType)
            {
                case AlertType.Daily:
                    {
                        sql = "SELECT * FROM AdtextPerAdgroupAllMeasuresDayDelta WHERE Account_ID = " + accountID.ToString() + " AND Channel_ID = " + channelID.ToString() + " AND Campaign_gk = " + campaignGK.ToString() + " AND Adgroup_gk = " + adgroupGK.ToString();
                        break;
                    }

                case AlertType.Period:
                    {
                        sql = "SELECT * FROM AdtextPerAdgroupAllMeasuresPeriod WHERE Account_ID = " + accountID.ToString() + " AND Channel_ID = " + channelID.ToString() + " AND Campaign_gk = " + campaignGK.ToString() + " AND Adgroup_gk = " + adgroupGK.ToString();
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

            //Run the stored procedure, based on the params we have.
            try
            {
                int accountID = Convert.ToInt32(ParentWorkflow.Parameters["AccountID"]);
                int channelID = Convert.ToInt32(ParentWorkflow.Parameters["ChannelID"]);

                //If we got the CampaignGK and AdgroupGK parameters, this means that someone wants to run us
                //with a specific campaign/adgroup.
                if (ParentWorkflow.Parameters.Contains("AdgroupGK") && ParentWorkflow.Parameters.Contains("CampaignGK"))
                {
                    int adgroupGK = Convert.ToInt32(ParentWorkflow.Parameters["AdgroupGK"]);
                    int campaignGK = Convert.ToInt32(ParentWorkflow.Parameters["CampaignGK"]);
                    LoadAdtextsForAdgroup(accountID, channelID, campaignGK, adgroupGK);
                }
                else
                {
                    //We didn't get a campaign GK parameter, this means that we need to check if
                    //we have a list of campaign GK's from somewhere (i.e. AccountCampaigns).
                    if (ParentWorkflow.InternalParameters.Contains("AdgroupGKList"))
                    {
                        ArrayList gks = (ArrayList)ParentWorkflow.InternalParameters["AdgroupGKList"];
                        for (int i = 0; i < gks.Count; i++)
                        {
                            AdgroupGK adg = (AdgroupGK)gks[i];
                            LoadAdtextsForAdgroup(accountID, channelID, adg._campaignGK, adg._adgroupGK);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception at Adgroup Adtext: " + ex.ToString());
                throw ex;
            }

            return ActivityExecutionStatus.Closed;
        }
	}
}
