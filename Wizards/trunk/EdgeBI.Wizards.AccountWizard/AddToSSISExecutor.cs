using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Utilities;
using Microsoft.SqlServer.Dts.Runtime;
using System.IO;
using System.Data.SqlClient;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Smo.Agent;

namespace EdgeBI.Wizards.AccountWizard
{
    class AddToSSISExecutor : StepExecuter
    {
        private const string TaskType = "Microsoft.DataTransformationServices.Tasks.DTSProcessingTask.DTSProcessingTask, " +
     "Microsoft.SqlServer.ASTasks, Version=10.0.0.0, " +
     "Culture=neutral, PublicKeyToken=89845dcd8080cc91";
        protected override Easynet.Edge.Core.Services.ServiceOutcome DoWork()
        {

            Log.Write("Getting collected data from suitable collector", LogMessageType.Information);

            Dictionary<string, object> collectedData = this.GetCollectedData();
            if (collectedData.ContainsKey(ApplicationIDKey))
                SetAccountWizardSettingsByApllicationID(Convert.ToInt32(collectedData[ApplicationIDKey]));
            this.ReportProgress(0.1f);

            Log.Write("Add to SSIS", LogMessageType.Information);

            try
            {
                AddToSSIS(collectedData);
            }
            catch (Exception ex)
            {
                Log.Write("Problem adding to SSIS", ex);
                throw new Exception("Problem adding to SSIS", ex);
            }
            this.ReportProgress(0.7f);
            Log.Write("Added to SSIS ", LogMessageType.Information);

            //Log.Write("Update OLTP datbase", LogMessageType.Information);
            //UpdateOltpDataBASE(collectedData);
            this.ReportProgress(1);
            return base.DoWork();
        }

        //       
        private void AddToSSIS(Dictionary<string, object> collectedData)
        {
            #region ALL Tasks-update
            #region BOTask




            string pkgPath = accountWizardSettings.Get("SSIS.TemplateAllBoPackagePath"); //Load Bo Template
            Application application = new Application();

            Package package = application.LoadPackage(pkgPath, null);
            //BackUp the package
            string backupPath = accountWizardSettings.Get("SSIS.AllBoPackageBackupPath");
            if (!Directory.Exists(backupPath))
                Directory.CreateDirectory(backupPath);

            application.SaveToXml(Path.Combine(backupPath, Path.GetFileName(pkgPath).Replace(".dtsx", DateTime.Now.ToString("ddMMyyyy_hhmm") + ".dtsx")), package, null);
            //Get the last executable 
            Executable fromTask = GetLastTaskOnSequence(package);
            //Add new task
            Executable newTask = package.Executables.Add(TaskType);
            TaskHost tNewTask = (TaskHost)newTask;
            tNewTask.Properties["ConnectionName"].SetValue(tNewTask, accountWizardSettings.Get("SSIS.ConnectionName"));
            string procCmd = string.Format("<Batch xmlns=\"http://schemas.microsoft.com/analysisservices/2003/engine\">" +
        "<Process xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
            "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
          "<Object>" +
            "<DatabaseID>{0}</DatabaseID>" +
            "<CubeID>{1}</CubeID>" +
          "</Object>" +
          "<Type>ProcessFull</Type>" +
          "<WriteBackTableCreation>UseExisting</WriteBackTableCreation>" +
        "</Process>" +
    "</Batch>", accountWizardSettings.Get("AnalysisServer.Database.ID"), accountWizardSettings.Get("Cube.BO.Name.Perfix") + collectedData["AccountSettings.CubeName"].ToString());


            tNewTask.Properties["ProcessingCommands"].SetValue(tNewTask, procCmd);

            tNewTask.Name = accountWizardSettings.Get("Cube.BO.Name.Perfix") + collectedData["AccountSettings.CubeName"].ToString();
            PrecedenceConstraint precedenceConstraint = package.PrecedenceConstraints.Add(fromTask, newTask);
            precedenceConstraint.Value = DTSExecResult.Completion;



            application.SaveToXml(pkgPath, package, null);
            //  app.SaveToDtsServer(p, null, @"D:\SSIS_Projects\test10.dtsx", "localhost"); //ToDo: may be this is better check with amit
            #endregion
            #region ContentTask
            //Get params from last step
            if (bool.Parse(collectedData["AccountSettings.AddContentCube"].ToString()) == true)
            {
                pkgPath = accountWizardSettings.Get("SSIS.TemplateAllContentPackagePath"); //Load ContentTemplate Template
                application = new Application();

                package = application.LoadPackage(pkgPath, null);
                //BackUp the package
                backupPath = accountWizardSettings.Get("SSIS.AllContentPackageBackupPath");
                if (!Directory.Exists(backupPath))
                    Directory.CreateDirectory(backupPath);

                application.SaveToXml(Path.Combine(backupPath, Path.GetFileName(pkgPath).Replace(".dtsx", DateTime.Now.ToString("ddMMyyyy_hhmm") + ".dtsx")), package, null);
                //Get the last executable 
                fromTask = GetLastTaskOnSequence(package);
                //Add new task
                newTask = package.Executables.Add(TaskType);
                tNewTask = (TaskHost)newTask;
                tNewTask.Properties["ConnectionName"].SetValue(tNewTask, accountWizardSettings.Get("SSIS.ConnectionName"));
                procCmd = string.Format("<Batch xmlns=\"http://schemas.microsoft.com/analysisservices/2003/engine\">" +
            "<Process xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
              "<Object>" +
                "<DatabaseID>{0}</DatabaseID>" +
                "<CubeID>{1}</CubeID>" +
              "</Object>" +
              "<Type>ProcessFull</Type>" +
              "<WriteBackTableCreation>UseExisting</WriteBackTableCreation>" +
            "</Process>" +
        "</Batch>", accountWizardSettings.Get("AnalysisServer.Database.ID"), accountWizardSettings.Get("Cube.Content.Name.Perfix") + collectedData["AccountSettings.CubeName"].ToString());


                tNewTask.Properties["ProcessingCommands"].SetValue(tNewTask, procCmd);

                tNewTask.Name = accountWizardSettings.Get("Cube.Content.Name.Perfix") + collectedData["AccountSettings.CubeName"].ToString();
                precedenceConstraint = package.PrecedenceConstraints.Add(fromTask, newTask);
                precedenceConstraint.Value = DTSExecResult.Completion;


                application.SaveToXml(pkgPath, package, null);
            }

            #endregion
            #endregion
            #region Specific Cube only BO

            pkgPath = accountWizardSettings.Get("SSIS.TemplateBoSpecific"); //Load Bo Template
            application = new Application();

            package = application.LoadPackage(pkgPath, null);





            fromTask = GetLastTaskOnSequence(package);

            TaskHost taskToUpdate = (TaskHost)fromTask;
            taskToUpdate.Properties["ConnectionName"].SetValue(tNewTask, accountWizardSettings.Get("SSIS.ConnectionName"));
            procCmd = string.Format("<Batch xmlns=\"http://schemas.microsoft.com/analysisservices/2003/engine\">" +
       "<Process xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
           "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
         "<Object>" +
           "<DatabaseID>{0}</DatabaseID>" +
           "<CubeID>{1}</CubeID>" +
         "</Object>" +
         "<Type>ProcessFull</Type>" +
         "<WriteBackTableCreation>UseExisting</WriteBackTableCreation>" +
       "</Process>" +
   "</Batch>", accountWizardSettings.Get("AnalysisServer.Database.ID"), accountWizardSettings.Get("Cube.BO.Name.Perfix") + collectedData["AccountSettings.CubeName"].ToString());


            taskToUpdate.Properties["ProcessingCommands"].SetValue(taskToUpdate, procCmd);

            taskToUpdate.Name = accountWizardSettings.Get("Cube.BO.Name.Perfix") + collectedData["AccountSettings.CubeName"].ToString();
            string fileName = Path.Combine(Path.GetDirectoryName(accountWizardSettings.Get("SSIS.TemplateBoSpecific")), string.Format(accountWizardSettings.Get("SSIS.NewSpecificPackageName"), collectedData["AccountSettings.CubeName"].ToString()));

            pkgPath = Path.Combine(accountWizardSettings.Get("SSIS.SSISNewTaskPath"), fileName);
            application.SaveToXml(pkgPath, package, null);
            #region AddToSqlServerAgent

            //Connect to the local, default instance of SQL Server.
            Server srv = new Server();
            Job template = srv.JobServer.Jobs[accountWizardSettings.Get("AccountSettings.TemplateSqlJob")];


            Job jb = new Job(srv.JobServer, string.Format(@"Load_{0}_Data", collectedData["AccountSettings.CubeName"].ToString()));
            jb.Name = string.Format(@"Load_{0}_Data", collectedData["AccountSettings.CubeName"].ToString());
            //new Job(srv.JobServer, string.Format(@"Load_{0}_Data", collectedData["AccountSettings.CubeName"].ToString()));
            jb.IsEnabled = false;

            jb.Create();

            for (int i = 0; i < template.JobSteps.Count; i++)
            {
                JobStep sourceStep = template.JobSteps[i];
                JobStep target = new JobStep(jb, sourceStep.Name);
                target.Command = sourceStep.Command;
                target.SubSystem = sourceStep.SubSystem;
                target.OnSuccessAction = sourceStep.OnSuccessAction;
                target.OnFailAction = sourceStep.OnFailAction;


                target.Create();
            }
            string stepName="temp";
            try
            {
                 stepName = string.Format(accountWizardSettings.Get("SSIS.NewSpecificPackageName"), collectedData["AccountSettings.CubeName"].ToString()).Replace(".dtsx", string.Empty);
            }
            catch 
            {
                
               
            }
            //Define a JobStep object variable by supplying the parent job and name arguments in the constructor. 
            JobStep jbstp = new JobStep(jb, stepName);
            jbstp.Command = @"dtexec /FILE " + "\"" + pkgPath + "\"";

            jbstp.SubSystem = AgentSubSystem.CmdExec;

            jbstp.OnSuccessAction = StepCompletionAction.QuitWithSuccess; //CHANNGE SINCE DASHBOAD STEP IS CANCELED BY AMIT AND SHAY
            jbstp.OnFailAction = StepCompletionAction.QuitWithFailure; //CHANNGE SINCE DASHBOAD STEP IS CANCELED BY AMIT AND SHAY

            //Create the job step on the instance of SQL Agent. 
            jbstp.Create();

            /* CANCELD BY AMIT AND SHAY
            jbstp = new JobStep(jb, "Load Dashboard Data");
            jbstp.Command = "exec Seperia_dwh.dbo.DataService_ProcessMeasures";
            jbstp.DatabaseName = "master"; //TODO PUT THE REAL ONE WITH PARAMETER

            jbstp.SubSystem = AgentSubSystem.TransactSql;

            jbstp.OnSuccessAction = StepCompletionAction.QuitWithSuccess;
            jbstp.OnFailAction = StepCompletionAction.QuitWithFailure;

            //Create the job step on the instance of SQL Agent. 
            jbstp.Create();
             * /
             */
            try
            {
                jb.ApplyToTargetServer(accountWizardSettings.Get("SSIS.JobServer"));

            }
            catch (Exception)
            {

                //wil be fixed....
            }

            #endregion


            #endregion



        }
        private TaskHost GetLastTaskOnSequence(Package p)
        {
            TaskHost fromTask = null;
            foreach (Executable executable in p.Executables)
            {
                if (executable is TaskHost)
                {
                    fromTask = (TaskHost)executable;

                    bool isLast = true;
                    foreach (PrecedenceConstraint precedenceConstraint in p.PrecedenceConstraints)
                    {
                        if (fromTask.Name == ((TaskHost)precedenceConstraint.PrecedenceExecutable).Name)
                        {
                            isLast = false;
                            break;

                        }


                    }
                    if (isLast)
                        break;

                }

            }
            return fromTask;
        }
    }
}
