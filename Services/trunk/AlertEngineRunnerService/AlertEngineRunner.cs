using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Data.SqlClient;
using System.Threading;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Xml;

using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Data.Proxy;
using Easynet.Edge.Core.Workflow;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core;
using Easynet.Edge.Alerts.Core;


namespace Easynet.Edge.Services.Alerts
{

    /// <summary>
    /// This is the service that would be used by the Scheduling Manager to run specific workflows at specific
    /// times.
    /// </summary>
    public class AlertEngineRunner : Service
    {

        #region Protected Overrides
        /// <summary>
        /// Performs the operation of actually running the desired workflow. The desired workflow is selected by using
        /// an ID parameter that is passed to the service through the options settings collection.
        /// </summary>
        /// <returns></returns>
        protected override ServiceOutcome DoWork()
        {
            using (ServiceClient<IAlertEngine> alertEngine = new ServiceClient<IAlertEngine>())
            {
                //Connection string.
                string connectionString = AppSettings.GetAbsolute("Easynet.Edge.Core.Workflow.AlertConnectionString");

                //Get the workflow ID.
                int id = -1;
                if (this.Instance.Configuration.Options.ContainsKey("WorkflowID"))
                {
                    string sid = this.Instance.Configuration.Options["WorkflowID"];
                    try
                    {
                        id = Convert.ToInt32(sid);
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                }
                else
                {
                    //Couldn't find a workflow ID. This service knows only to run workflows based on their ID's.
                    throw new Exception("Invalid ID parameter used to run this workflow, cannot be null.");
                }

                if (id <= 0)
                {
                    //No ID - can't have that.
                    throw new Exception("Invalid ID parameter used to run this workflow: " + this.Instance.Configuration.Options["WorkflowID"]);
                }

                SettingsCollection scp = null;
                if (this.Instance.Configuration.Options.ContainsKey("WorkflowParameters"))
                    scp = new SettingsCollection(this.Instance.Configuration.Options["WorkflowParameters"]);

                AlertType alertType = AlertType.Unknown;
                if (scp.ContainsKey("AlertType"))
                {
                    if (scp["AlertType"].ToLower() == "period")
                        alertType = AlertType.Period;
                    else
                        alertType = AlertType.Daily;
                }
                else
                {
                    alertType = AlertType.Daily;
                }

                if (alertType == AlertType.Daily)
                {
                    //Add the current date. - do this if we're doing a "daily" alert.
                    if (scp.ContainsKey("CurrentDayCode"))
                        scp["CurrentDayCode"] = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                    else
                        scp.Add("CurrentDayCode", DateTime.Now.AddDays(-1).ToString("yyyyMMdd"));

                    if (scp.ContainsKey("CompareDayCode"))
                        scp["CompareDayCode"] = DateTime.Now.AddDays(-2).ToString("yyyyMMdd");
                    else
                        scp.Add("CompareDayCode", DateTime.Now.AddDays(-2).ToString("yyyyMMdd"));
                }
                else
                {
                    //The delta between the base start and base end (i.e. from yesterday till when?)
                    //As well as the delta between the compare dates.
                    int baseDelta = 0;
                    if (scp.ContainsKey("BasePeriodDelta"))
                        baseDelta = Convert.ToInt32(scp["BasePeriodDelta"]);

                    //The delta between the base and compare dates. (i.e. what's the
                    //duration of time between the two date groups).
                    int compareDelta = 0;
                    if (scp.ContainsKey("ComparePeriodDelta"))
                        compareDelta = Convert.ToInt32(scp["ComparePeriodDelta"]);

                    //If specified, what is the time difference between the two
                    //compare dates.
                    int timeDelta = 0;
                    if (scp.ContainsKey("TimePeriodDelta"))
                        timeDelta = Convert.ToInt32(scp["TimePeriodDelta"]);

                    DateTime baseEndTime = DateTime.Now.AddDays(-1);
                    DateTime baseStartTime = baseEndTime.AddDays(baseDelta);
                    DateTime compareEndTime = baseEndTime.AddDays(compareDelta);

                    DateTime compareStartTime = DateTime.MinValue;
                    if (timeDelta == 0)
                        compareStartTime = compareEndTime.AddDays(baseDelta);
                    else
                        compareStartTime = compareEndTime.AddDays(timeDelta);

                    scp.Add("CurrentStartDayCode", baseStartTime.ToString("yyyyMMdd"));
                    scp.Add("CurrentEndDayCode", baseEndTime.ToString("yyyyMMdd"));
                    scp.Add("CompareStartDayCode", compareStartTime.ToString("yyyyMMdd"));
                    scp.Add("CompareEndDayCode", compareEndTime.ToString("yyyyMMdd"));
                }

                SettingsCollection sccv = null;
                if (this.Instance.Configuration.Options.ContainsKey("WorkflowConditionValues"))
                    sccv = new SettingsCollection(this.Instance.Configuration.Options["WorkflowConditionValues"]);

                //Now run it!
                Hashtable wfParams = new Hashtable();
                if (this.Instance.Configuration.Options.ContainsKey("SMTPPort"))
                    wfParams.Add("SMTPPort", this.Instance.Configuration.Options["SMTPPort"]);

                if (this.Instance.Configuration.Options.ContainsKey("SMTPHost"))
                    wfParams.Add("SMTPHost", this.Instance.Configuration.Options["SMTPHost"]);

                wfParams.Add("ConnectionString", connectionString);
                
                //HACK - For Admin Alert only!
                if (this.Instance.Configuration.Options.ContainsKey("SourceFilePath"))
                {
                    if (scp == null)
                        scp = new SettingsCollection();

                    scp.Add("SourceFilePath", this.Instance.Configuration.Options["SourceFilePath"]);
                }
                
                if (scp != null)
                {
                    IDictionaryEnumerator ide = scp.GetEnumerator();
                    ide.Reset();
                    while (ide.MoveNext())
                    {
                        wfParams.Add(ide.Key, ide.Value);
                    }
                }

                Hashtable wfConditionValues = new Hashtable();
                if (sccv != null)
                {
                    IDictionaryEnumerator ide = sccv.GetEnumerator();
                    ide.Reset();
                    while (ide.MoveNext())
                    {
                        wfConditionValues.Add(ide.Key, ide.Value);
                    }
                }

                alertEngine.Service.RunExistingFlow(id, wfParams, wfConditionValues);
            }

            return ServiceOutcome.Success;
        }
        #endregion

    }
}
