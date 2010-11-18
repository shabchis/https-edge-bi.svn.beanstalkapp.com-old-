using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Utilities;
using Microsoft.AnalysisServices;
using Easynet.Edge.Core.Configuration;

namespace EdgeBI.Wizards.AccountWizard
{
    class CubesProcessingExecutor : StepExecuter
    {
        protected override Easynet.Edge.Core.Services.ServiceOutcome DoWork()
        {
            Log.Write("Getting Create Cube executor data", LogMessageType.Information);
            this.ReportProgress(0.1f);
            Dictionary<string, object> lastExecutorStepData = GetExecutorData("CreateNewCubeExecutor");
            if (bool.Parse(lastExecutorStepData["AccountSettings.ProcessCubes"].ToString()) == true)
            {


                using (Server analysisServer = new Server())
                {
                    analysisServer.Connect(AppSettings.Get(this, "AnalysisServer.ConnectionString"));
                    this.ReportProgress(0.2f);
                     Database analysisDatabase = analysisServer.Databases.GetByName(AppSettings.Get(this, "AnalysisServer.Database"));
                     
                    Cube boCube =analysisDatabase.Cubes.Find( AppSettings.GetAbsolute("EdgeBI.Wizards.StepExecuter.Cube.BO.Name.Perfix") + lastExecutorStepData["AccountSettings.CubeName"].ToString());
                   
                    Cube contentCube = analysisDatabase.Cubes.Find(AppSettings.GetAbsolute("EdgeBI.Wizards.StepExecuter.Cube.Content.Name.Perfix") + lastExecutorStepData["AccountSettings.CubeName"].ToString());

                    try
                    {

                        if (boCube!=null)
                        {
                            Log.Write("Processing BoCube", LogMessageType.Information);
                            boCube.Process(ProcessType.ProcessFull);
                            Log.Write("BOCube Processed", LogMessageType.Information);
                            this.ReportProgress(0.7f);     
                        }          

                    }
                    catch (Exception ex)
                    {
                        Log.Write("Problem Prosessing BOCube", ex);
                       

                    }
                    try
                    {
                        if (contentCube != null)
                        {
                            Log.Write("Processing ContentCube", LogMessageType.Information);
                            contentCube.Process(ProcessType.ProcessFull);
                            Log.Write("ContentCube Processed", LogMessageType.Information);
                            this.ReportProgress(0.9f);
                        }

                    }
                    catch (Exception ex)
                    {

                        Log.Write("Problem Prosessing ContentCube", ex);
                       
                    }
                    this.ReportProgress(1);

                   

                }
                
            }
            return base.DoWork();
        }

    }
}
