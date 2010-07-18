using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Data.SqlClient;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using System.Workflow.ComponentModel.Compiler;
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

using AlertWorkflows;


namespace Easynet.Edge.Services.Alerts
{
    /// <summary>
    /// The alert engine service is responsible for executing flows that result in alerts.
    /// </summary>
    public class AlertEngineService : Service, IAlertEngine
    {

        #region Members
        private static string _connectionString = String.Empty;
        #endregion

        #region IAlertEngine Implementation
        /// <summary>
        /// Runs a new workflow. Used mainly to save the workflow as a template.
        /// </summary>
        /// <param name="wfType">The type of the workflow</param>
        /// <param name="parameters">The parameters to use in the workflow</param>
        /// <param name="conditionValues">The condition values to use in the workflow</param>
        public void RunNewFlow(Type wfType, Hashtable parameters, Hashtable conditionValues)
        {
            Console.WriteLine("Running New Flow. Type: " + wfType.ToString());
            using (WorkflowRuntime runtime = new WorkflowRuntime())
            {
                AutoResetEvent waitHandle = new AutoResetEvent(false);
                runtime.WorkflowCompleted += delegate(object sender, WorkflowCompletedEventArgs e) { waitHandle.Set(); };
                runtime.WorkflowTerminated += delegate(object sender, WorkflowTerminatedEventArgs e)
                {
                    Console.WriteLine(e.Exception.Message);
                    waitHandle.Set();
                };

                SqlWorkflowPersistenceService persistanceService =
                    new SqlWorkflowPersistenceService(_connectionString);

                runtime.AddService(persistanceService);
                runtime.StartRuntime();

                //Fill the parameters .
                Dictionary<string, object> paramsD = new Dictionary<string, object>();
                paramsD.Add("Parameters", parameters);
                paramsD.Add("ConditionValues", conditionValues);

                WorkflowInstance instance = runtime.CreateWorkflow(wfType, paramsD);

                //This is a new workflow. Save it into the database.
                BaseSequentialWorkflow esw = (BaseSequentialWorkflow)Activator.CreateInstance(wfType);
                esw.WorkflowGUID = instance.InstanceId;
                esw.ConditionValues = conditionValues;
                esw.Parameters = parameters;
                esw.ConnectionString = _connectionString;
                esw.Template = true;
                esw.WorkflowType = wfType;
                esw.Serialize();

                instance.Start();

                waitHandle.WaitOne();
            }
        }

        /// <summary>
        /// Runs an existing workflow.
        /// </summary>
        /// <param name="id">The ID of the workflow to run</param>
        /// <param name="parameters">The parameters to pass to the workflow</param>
        /// <param name="conditionValues">The condition values to use in the workflow</param>
        public void RunExistingFlow(int id, Hashtable parameters, Hashtable conditionValues)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid workflow ID parameter. Cannot be 0 or less.");

            Console.WriteLine("Running Existing Flow. ID: " + id.ToString());
            using (WorkflowRuntime runtime = new WorkflowRuntime())
            {
                WorkflowStatusManager wsm = new WorkflowStatusManager();
                AutoResetEvent waitHandle = new AutoResetEvent(false);
                DateTime startTime = DateTime.Now;

                /*
                SqlWorkflowPersistenceService persistanceService =
                    new SqlWorkflowPersistenceService(_connectionString);

                runtime.AddService(persistanceService);
                */
                runtime.StartRuntime();

                parameters.Add("WorkflowID", id);

                //Get additional parameters from the database. Based on the ID we got.
                BaseAlertWorkflow esw = new BaseAlertWorkflow();
                esw.WorkflowID = id;
                esw.ConnectionString = _connectionString;
                esw.Deserialize();
                esw.AppendParameters(parameters);
                esw.AppendConditionValues(conditionValues);

                
                runtime.WorkflowCompleted += delegate(object sender, WorkflowCompletedEventArgs e) 
                {
                    wsm.UpdateStatus(WorkflowStatusCodes.Success, esw, startTime, "Success");
                    waitHandle.Set(); 
                };
                runtime.WorkflowTerminated += delegate(object sender, WorkflowTerminatedEventArgs e)
                {
                    wsm.UpdateStatus(WorkflowStatusCodes.Failed, esw, startTime, e.Exception.Message);
                    Console.WriteLine(e.Exception.Message);
                    waitHandle.Set();
                };

                //Fill the parameters .
                Dictionary<string, object> paramsD = new Dictionary<string, object>();
                paramsD.Add("Parameters", esw.Parameters);
                paramsD.Add("ConditionValues", esw.ConditionValues);

                //WorkflowInstance instance = runtime.GetWorkflow(guid);
                WorkflowInstance instance = runtime.CreateWorkflow(esw.WorkflowType, paramsD);

                instance.Start();

                waitHandle.WaitOne();
            }
        }

        /// <summary>
        /// Runs a flow based on a xoml file.
        /// </summary>
        /// <param name="xomlFile">Xoml file name containing the flow. Please note that the .rules file containing the rules
        /// must be located at the same folder as the xoml file.</param>
        /// <param name="parameters">The parameters to pass to the workflow</param>
        /// <param name="conditionValues">The condition values to use in the workflow</param>
        public void RunXomlFlow(string xomlFile, Hashtable parameters, Hashtable conditionValues)
        {
            if (!File.Exists(xomlFile) ||
                xomlFile == null ||
                xomlFile == String.Empty)
                throw new ArgumentException("Invalid workflow parameter. Cannot be null/empty, or xoml file does not exist.");

            Console.WriteLine("Running XOML Flow (" + xomlFile + ")");
            using (WorkflowRuntime runtime = new WorkflowRuntime())
            {
                AutoResetEvent waitHandle = new AutoResetEvent(false);
                runtime.WorkflowCompleted += delegate(object sender, WorkflowCompletedEventArgs e) { waitHandle.Set(); };
                runtime.WorkflowTerminated += delegate(object sender, WorkflowTerminatedEventArgs e)
                {
                    Console.WriteLine(e.Exception.Message);
                    waitHandle.Set();
                };


                SqlWorkflowPersistenceService persistanceService =
                    new SqlWorkflowPersistenceService(_connectionString);

                runtime.AddService(persistanceService);
                runtime.StartRuntime();

                //Fill the parameters .
                Dictionary<string, object> paramsD = new Dictionary<string, object>();
                paramsD.Add("Parameters", parameters);
                paramsD.Add("ConditionValues", conditionValues);

                WorkflowInstance instance = null;
                using (XmlTextReader reader = new XmlTextReader(xomlFile))
                {
                    string rulesFileName = xomlFile.Replace(".xoml", ".rules");
                    if (File.Exists(rulesFileName))
                    {
                        XmlTextReader rulesFileReader = new XmlTextReader(rulesFileName);
                        instance = runtime.CreateWorkflow(reader, rulesFileReader, paramsD);
                    }
                    else
                    {
                        instance = runtime.CreateWorkflow(reader, null, paramsD);
                    }
                }

                instance.Start();

                waitHandle.WaitOne();
            }
        }

        /// <summary>
        /// Runs a workflow that has been compiled either form .xoml or from any other type (i.e. a DLL)
        /// </summary>
        /// <param name="dllFile">The DLL file containing the compiled workflow</param>
        /// <param name="parameters">The parameters to pass to the workflow</param>
        /// <param name="conditionValues">The condition values to use in the workflow</param>
        public void RunCompiledFlow(string dllFile, Hashtable parameters, Hashtable conditionValues)
        {
            throw new NotImplementedException("Currently not implemented!");
        }
        #endregion

        #region Protected Overrides
        /// <summary>
        /// Main entry point to the alert engine service.
        /// </summary>
        /// <returns>Service Outcome</returns>
        protected override ServiceOutcome DoWork()
        {
            //Get the connection string.
            _connectionString = AppSettings.GetAbsolute("Easynet.Edge.Core.Workflow.AlertConnectionString");

            try
            {
                //Get the relevant workflow ID.
                int id = -1;
                Type type = null;
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

                try
                {
                    if (this.Instance.Configuration.Options.ContainsKey("WorkflowType"))
                    {
                        type = Type.GetType(this.Instance.Configuration.Options["WorkflowType"],true);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write("Exception occured while trying to get the type: " + this.Instance.Configuration.Options["WorkflowType"], ex);
                }


                if (id <= 0 && type == null)
                {
                    //No type and no ID. For now ignore and assume that we just ran the service waiting for requests.
                    Log.Write("No workflow ID or type passed. Please note, the alert engine is NOT executing any flows.", LogMessageType.Warning);
                    Console.WriteLine("AlertEngine Service is Running...");
                    return ServiceOutcome.Unspecified;
                }


                SettingsCollection scp = null;
                if (this.Instance.Configuration.Options.ContainsKey("WorkflowParameters"))
                    scp = new SettingsCollection(this.Instance.Configuration.Options["WorkflowParameters"]);

                /************************************************/
                /*              TESTING ONLY                    */
                /************************************************/
                //AlertType alertType = AlertType.Unknown;
                //if (scp.ContainsKey("AlertType"))
                //{
                //    if (scp["AlertType"].ToLower() == "period")
                //        alertType = AlertType.Period;
                //    else
                //        alertType = AlertType.Daily;
                //}
                //else
                //{
                //    alertType = AlertType.Daily;
                //}

                //if (alertType == AlertType.Daily)
                //{
                //    //Add the current date. - do this if we're doing a "daily" alert.
                //    if (scp.ContainsKey("CurrentDayCode"))
                //        scp["CurrentDayCode"] = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                //    else
                //        scp.Add("CurrentDayCode", DateTime.Now.AddDays(-1).ToString("yyyyMMdd"));

                //    if (scp.ContainsKey("CompareDayCode"))
                //        scp["CompareDayCode"] = DateTime.Now.AddDays(-2).ToString("yyyyMMdd");
                //    else
                //        scp.Add("CompareDayCode", DateTime.Now.AddDays(-2).ToString("yyyyMMdd"));
                //}
                //else
                //{
                //    //The delta between the base start and base end (i.e. from yesterday till when?)
                //    //As well as the delta between the compare dates.
                //    int baseDelta = 0;
                //    if (scp.ContainsKey("BasePeriodDelta"))
                //        baseDelta = Convert.ToInt32(scp["BasePeriodDelta"]);

                //    //The delta between the base and compare dates. (i.e. what's the
                //    //duration of time between the two date groups).
                //    int compareDelta = 0;
                //    if (scp.ContainsKey("ComparePeriodDelta"))
                //        compareDelta = Convert.ToInt32(scp["ComparePeriodDelta"]);

                //    //If specified, what is the time difference between the two
                //    //compare dates.
                //    int timeDelta = 0;
                //    if (scp.ContainsKey("TimePeriodDelta"))
                //        timeDelta = Convert.ToInt32(scp["TimePeriodDelta"]);

                //    DateTime baseEndTime = DateTime.Now.AddDays(-1);
                //    baseEndTime = new DateTime(2009, 1, 31);
                //    DateTime baseStartTime = baseEndTime.AddDays(baseDelta);
                //    DateTime compareEndTime = baseEndTime.AddDays(compareDelta);

                //    DateTime compareStartTime = DateTime.MinValue;
                //    if (timeDelta == 0)
                //        compareStartTime = compareEndTime.AddDays(baseDelta);
                //    else
                //        compareStartTime = compareEndTime.AddDays(timeDelta);

                //    scp.Add("CurrentStartDayCode", baseStartTime.ToString("yyyyMMdd"));
                //    scp.Add("CurrentEndDayCode", baseEndTime.ToString("yyyyMMdd"));
                //    scp.Add("CompareStartDayCode", compareStartTime.ToString("yyyyMMdd"));
                //    scp.Add("CompareEndDayCode", compareEndTime.ToString("yyyyMMdd"));
                //}
                /************************************************/
                /*              TESTING ONLY                    */
                /************************************************/

                SettingsCollection sccv = null;
                if (this.Instance.Configuration.Options.ContainsKey("WorkflowConditionValues"))
                    sccv = new SettingsCollection(this.Instance.Configuration.Options["WorkflowConditionValues"]);

                //Now run it!
                Hashtable wfParams = new Hashtable();
                if (this.Instance.Configuration.Options.ContainsKey("SMTPPort"))
                    wfParams.Add("SMTPPort", this.Instance.Configuration.Options["SMTPPort"]);

                if (this.Instance.Configuration.Options.ContainsKey("SMTPHost"))
                    wfParams.Add("SMTPHost", this.Instance.Configuration.Options["SMTPHost"]);

                wfParams.Add("ConnectionString", _connectionString);

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

                if (id <= 0)
                    RunNewFlow(type, wfParams, wfConditionValues);
                else
                    RunExistingFlow(id, wfParams, wfConditionValues);

                Console.WriteLine("AlertEngine Service is Running...");
                return ServiceOutcome.Unspecified;
            }
            catch (Exception ex)
            {
                Log.Write("The Alert Engine failed to process the flow", ex);
                return ServiceOutcome.Failure;
            }
        }
        #endregion

    }
}
