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
using System.Diagnostics;

namespace Easynet.Edge.Services.Alerts.AlertCustomActivities
{
	public partial class KeepAlive: BaseAlertActivity
	{

		public KeepAlive()
		{
			InitializeComponent();
		}


        public static DependencyProperty AliveProperty = DependencyProperty.Register("Alive", typeof(bool), typeof(KeepAlive));

        [Category("Properties")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool Alive
        {
            get
            {
                return ((bool)base.GetValue(KeepAlive.AliveProperty));
            }
            set
            {
                base.SetValue(KeepAlive.AliveProperty, value);
            }
        }



        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            //Check the delta from the keep alive table. See that it didn't pass the 
            //threshold (in minutes).
            if (!ParentWorkflow.Parameters.ContainsKey("KeepAliveConnectionString"))
                throw new Exception("Invalid connecting string. Could not find it within the parameters collection.");

            if (!ParentWorkflow.Parameters.ContainsKey("MinutesThreshold"))
                throw new Exception("Did not find the minutes threshold parameter within the parameters collection.");

            string connectionString = ParentWorkflow.Parameters["KeepAliveConnectionString"].ToString();
            int threshold = Convert.ToInt32(ParentWorkflow.Parameters["MinutesThreshold"]);

            string sql = @"SELECT KeepAlive_Time AS TouchDate FROM Source.dbo.KeepAlive";

            DataManager.ConnectionString = connectionString;
            DataManager.CommandTimeout = 0;

            using (DataManager.Current.OpenConnection())
            {
                SqlCommand cmd = DataManager.CreateCommand(sql);
                SqlDataReader dr = cmd.ExecuteReader();

                DateTime date = DateTime.MinValue;
                DateTime now = DateTime.Now;

                if (!dr.HasRows)
                {
                    Alive = false;
                }
                else
                {
                    while (dr.Read())
                    {
                        if (!dr.IsDBNull(dr.GetOrdinal("TouchDate")))
                            date = Convert.ToDateTime(dr["TouchDate"]);
                    }

                    if (date.AddMinutes(Convert.ToDouble(threshold)) < now)
                        Alive = false;
                    else
                        Alive = true;
                }

                dr.Close();
                dr.Dispose();
            }

            //If not alive. Kill.
            if (ParentWorkflow.Parameters.ContainsKey("KillerApp"))
            {
                string app = ParentWorkflow.Parameters["KillerApp"].ToString();
                Process p = new Process();
                p.StartInfo.FileName = app;
                p.Start();
            }

            return ActivityExecutionStatus.Closed;
        }

	}
}
