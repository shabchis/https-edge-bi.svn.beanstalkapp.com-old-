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

		//	Dictionary<string, object> collectedData = this.GetStepCollectedData(Instance.Configuration.Options["CollectorStep"]); NO COLLECTED DATA ALL DATA IS CUBE NAME WHICH IS IN THE CUBE EXECUTOR AND WILL BE TAKEN LATER
			this.ReportProgress(0.1f);

			Log.Write("Add to SSIS", LogMessageType.Information);

			try
			{
				AddToSSIS();
			}
			catch (Exception ex)
			{
				Log.Write("Problem adding to SSIS", ex);
				throw new Exception("Problem adding to SSIS",ex);
			}
			this.ReportProgress(0.7f);
			Log.Write("Added to SSIS ", LogMessageType.Information);

			Log.Write("Update OLTP datbase", LogMessageType.Information);
			//UpdateOltpDataBASE(collectedData);
			this.ReportProgress(1);
			return base.DoWork();
		}

		private void UpdateOltpDataBASE(Dictionary<string, object> collectedData)
		{
			using (SqlConnection sqlConnection = new SqlConnection(AppSettings.Get(this, "OLTP.Connection.string")))
			{
				sqlConnection.Open();
				foreach (KeyValuePair<string, object> input in collectedData)
				{
					if (input.Key.StartsWith("AccountSettings"))
					{
						using (SqlCommand sqlCommand = DataManager.CreateCommand(@"INSERT INTO User_Gui_AccountSettings
																			(ScopeID,AccountID,Name,Value,sys_creation_date)
																			Values
																			(@ScopeID:Int,
																			 @AccountID:NVarchar,
																			 @Name:NVarchar,
																			 @Value:NVarchar,
																			 @sys_creation_date:DateTime)"))
						{
							sqlCommand.Connection = sqlConnection;
							sqlCommand.Parameters["@ScopeID"].Value = 3861; //TODO: TEMPORARLY WILL COME FROM OTHER COLLECTOR (GENERAL COLLECTOR-ASK DORON)
							sqlCommand.Parameters["@AccountID"].Value = DBNull.Value; //TODO: CHECK THIS FOR NOW IT'S NULL
							sqlCommand.Parameters["@Name"].Value = input.Key;
							sqlCommand.Parameters["@Value"].Value = input.Value; ;
							sqlCommand.Parameters["@sys_creation_date"].Value = DateTime.Now;

							sqlCommand.ExecuteNonQuery();

						}
					}

				}
			}
		}

		private void AddToSSIS()
        {
            #region ALL Tasks-update
            #region BOTask

            //Get params from last step
            Dictionary<string, object> executorData = GetExecutorData("CreateNewCubeExecutor");
            string pkgPath = AppSettings.Get(this, "SSIS.TemplateAllBoPackagePath"); //Load Bo Template
			Application application = new Application();
           
			Package package = application.LoadPackage(pkgPath, null);
            //BackUp the package
            string backupPath = AppSettings.Get(this, "SSIS.AllBoPackageBackupPath");
            if (!Directory.Exists(backupPath))           
                Directory.CreateDirectory(backupPath);

            application.SaveToXml(Path.Combine(backupPath, Path.GetFileName(pkgPath).Replace(".dtsx", DateTime.Now.ToString("ddMMyyyy_hhmm") + ".dtsx")), package, null);
            //Get the last executable 
            Executable fromTask = GetLastTaskOnSequence(package);
			//Add new task
			Executable newTask = package.Executables.Add(TaskType);
			TaskHost tNewTask = (TaskHost)newTask;
			tNewTask.Properties["ConnectionName"].SetValue(tNewTask, "localhost");
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
    "</Batch>", AppSettings.Get(this, "AnalysisServer.Database.ID"), AppSettings.GetAbsolute("EdgeBI.Wizards.StepExecuter.Cube.BO.Name.Perfix") + executorData["AccountSettings.CubeName"].ToString());
			
			
			tNewTask.Properties["ProcessingCommands"].SetValue(tNewTask, procCmd);

            tNewTask.Name = AppSettings.GetAbsolute("EdgeBI.Wizards.StepExecuter.Cube.BO.Name.Perfix") + executorData["AccountSettings.CubeName"].ToString();
			package.PrecedenceConstraints.Add(fromTask, newTask);


            application.SaveToXml(pkgPath, package, null);
            //  app.SaveToDtsServer(p, null, @"D:\SSIS_Projects\test10.dtsx", "localhost"); //ToDo: may be this is better check with amit
            #endregion
            #region ContentTask
            //Get params from last step
            if (bool.Parse(executorData["AccountSettings.AddContentCube"].ToString()) == true)
            {
                pkgPath = AppSettings.Get(this, "SSIS.TemplateAllContentPackagePath"); //Load ContentTemplate Template
                application = new Application();

                package = application.LoadPackage(pkgPath, null);
                //BackUp the package
                backupPath = AppSettings.Get(this, "SSIS.AllContentPackageBackupPath");
                if (!Directory.Exists(backupPath))
                    Directory.CreateDirectory(backupPath);

                application.SaveToXml(Path.Combine(backupPath, Path.GetFileName(pkgPath).Replace(".dtsx", DateTime.Now.ToString("ddMMyyyy_hhmm") + ".dtsx")), package, null);
                //Get the last executable 
                fromTask = GetLastTaskOnSequence(package);
                //Add new task
                newTask = package.Executables.Add(TaskType);
                tNewTask = (TaskHost)newTask;
                tNewTask.Properties["ConnectionName"].SetValue(tNewTask, "localhost");
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
        "</Batch>", AppSettings.Get(this, "AnalysisServer.Database.ID"), AppSettings.GetAbsolute("EdgeBI.Wizards.StepExecuter.Cube.Content.Name.Perfix") + executorData["AccountSettings.CubeName"].ToString());


                tNewTask.Properties["ProcessingCommands"].SetValue(tNewTask, procCmd);

                tNewTask.Name = AppSettings.GetAbsolute("EdgeBI.Wizards.StepExecuter.Cube.Content.Name.Perfix") + executorData["AccountSettings.CubeName"].ToString();
                package.PrecedenceConstraints.Add(fromTask, newTask);


                application.SaveToXml(pkgPath, package, null);
            }

            #endregion
            #endregion
            #region Specific Cube only BO

            pkgPath = AppSettings.Get(this, "SSIS.TemplateBoSpecific"); //Load Bo Template
            application = new Application();

            package = application.LoadPackage(pkgPath, null);
           
           

           
            //Get the last executable 
             fromTask = package.Executables[AppSettings.Get(this, "SSIS.BaseTask")];


             TaskHost taskToUpdate = (TaskHost)fromTask;
             taskToUpdate.Properties["ConnectionName"].SetValue(tNewTask, "localhost");
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
    "</Batch>", AppSettings.Get(this, "AnalysisServer.Database.ID"), AppSettings.GetAbsolute("EdgeBI.Wizards.StepExecuter.Cube.BO.Name.Perfix") + executorData["AccountSettings.CubeName"].ToString());


             taskToUpdate.Properties["ProcessingCommands"].SetValue(taskToUpdate, procCmd);

             taskToUpdate.Name = AppSettings.GetAbsolute("EdgeBI.Wizards.StepExecuter.Cube.BO.Name.Perfix") + executorData["AccountSettings.CubeName"].ToString();
             string fileName = Path.GetFileName(AppSettings.Get(this, "SSIS.TemplateBoSpecific")).Replace("BOTemplate", AppSettings.GetAbsolute("EdgeBI.Wizards.StepExecuter.Cube.BO.Name.Perfix") + executorData["AccountSettings.CubeName"].ToString());

             pkgPath = Path.Combine(AppSettings.Get(this, "SSIS.SSISNewTaskPath"), fileName);
            application.SaveToXml(pkgPath, package, null);


            #endregion


        }
        private TaskHost GetLastTaskOnSequence(Package p)
        {
            TaskHost fromTask=null;
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
