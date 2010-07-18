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
using System.Configuration;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;

using Easynet.Edge.Core.Workflow;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Alerts.Core;

namespace Easynet.Edge.Services.Alerts.AlertCustomActivities
{
	public partial class Campaign: BaseActivity
	{
		public Campaign()
		{
			InitializeComponent();
		}

        public static DependencyProperty ClicksProperty = DependencyProperty.Register("Clicks", typeof(int), typeof(Campaign));

        public int Clicks
        {
            get
            {
                return Convert.ToInt32(base.GetValue(Campaign.ClicksProperty));
            }
            set
            {
                base.SetValue(Campaign.ClicksProperty, value);
            }
        }


        public static DependencyProperty ClicksDeltaProperty = DependencyProperty.Register("ClicksDelta", typeof(float), typeof(Campaign));

        public float ClicksDelta
        {
            get
            {
                return ((float)base.GetValue(Campaign.ClicksDeltaProperty));
            }
            set
            {
                base.SetValue(Campaign.ClicksDeltaProperty, value);
            }
        }



        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            DataManager.ConnectionString = ParentWorkflow.Parameters["ConnectionString"].ToString();

            //Run the stored procedure, based on the params we have.
            try
            {
                int accountID = Convert.ToInt32(ParentWorkflow.Parameters["AccountID"]);
                DateTime cur = DayCode.GenerateDateTime(ParentWorkflow.Parameters["CurrentDayCode"]);
                DateTime comp = DayCode.GenerateDateTime(ParentWorkflow.Parameters["CompareDayCode"]);
                int channelID = Convert.ToInt32(ParentWorkflow.Parameters["ChannelID"]);
                int campaignGK = Convert.ToInt32(ParentWorkflow.Parameters["CampaignGK"]);

                //Create the command.
                using (DataManager.Current.OpenConnection())
                {
                    SqlCommand clicks = DataManager.CreateCommand("SP_Alerts_SumOfClicksPerAccount", System.Data.CommandType.StoredProcedure);
                    clicks.Parameters.Add("@Account_id", System.Data.SqlDbType.NVarChar);
                    clicks.Parameters.Add("@CurrentDayCode", System.Data.SqlDbType.NVarChar);
                    clicks.Parameters.Add("@CompareDayCode", System.Data.SqlDbType.NVarChar);
                    clicks.Parameters.Add("@channel_id", System.Data.SqlDbType.NVarChar);
                    clicks.Parameters.Add("@Campaign_GK", System.Data.SqlDbType.NVarChar);

                    clicks.Parameters.Add("@RC", System.Data.SqlDbType.Int);
                    clicks.Parameters["@RC"].Direction = System.Data.ParameterDirection.ReturnValue;

                    clicks.Parameters["@Account_id"].Value = accountID.ToString();
                    clicks.Parameters["@channel_id"].Value = channelID.ToString();
                    clicks.Parameters["@Campaign_GK"].Value = campaignGK.ToString();

                    //format the current and compare dates.
                    string curDate = cur.ToString("yyyyMMdd");
                    string compDate = comp.ToString("yyyyMMdd");

                    clicks.Parameters["@CurrentDayCode"].Value = curDate;
                    clicks.Parameters["@CompareDayCode"].Value = compDate;

                    clicks.ExecuteNonQuery();   
                    int d = Convert.ToInt32(clicks.Parameters["@RC"].Value);
                    ClicksDelta = (float)d;

                    CampaignAllMeasures cam = new CampaignAllMeasures();
                    cam.CampaignGK = campaignGK;
                    cam.ClicksChangeRatio = ClicksDelta;
                    cam.ChannelID = channelID;
                    cam.AccountID = accountID;

                    if (ParentWorkflow.InternalParameters.ContainsKey("MeasuredParams"))
                    {
                        MeasuredParameters mps = (MeasuredParameters)ParentWorkflow.InternalParameters["MeasuredParams"];
                        if (!mps.ContainsKey(cam.CampaignGK))
                            mps.Add(cam.CampaignGK, cam);
                        
                        ParentWorkflow.InternalParameters["MeasuredParams"] = mps;
                    }
                    else
                    {
                        MeasuredParameters mps = new MeasuredParameters();
                        mps.Add(cam.CampaignGK, cam);
                        ParentWorkflow.InternalParameters.Add("MeasuredParams", mps);
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            //Put the result in the clicks property.

            return ActivityExecutionStatus.Closed;
        }
	}
}
