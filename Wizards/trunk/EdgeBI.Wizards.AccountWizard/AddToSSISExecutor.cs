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

			Dictionary<string, object> collectedData = this.GetStepCollectedData(Instance.Configuration.Options["CollectorStep"]);
			this.ReportProgress(0.1f);

			Log.Write("Add to SSIS", LogMessageType.Information);

			try
			{
				AddToSSIS(collectedData);
			}
			catch (Exception ex)
			{
				Log.Write("Problem adding to SSIS", ex);
				throw new Exception("Problem adding to SSIS",ex);
			}
			this.ReportProgress(0.7f);
			Log.Write("Added to SSIS ", LogMessageType.Information);

			Log.Write("Update OLTP datbase", LogMessageType.Information);
			UpdateOltpDataBASE(collectedData);
			this.ReportProgress(0.9f);
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

		private void AddToSSIS(Dictionary<string, object> collectedData)
		{
			string pkgPath = AppSettings.Get(this, "SSIS.TemplatePackagePath");
			Application application = new Application();

			Package package = application.LoadPackage(pkgPath, null);
			Executable fromTask = package.Executables[AppSettings.Get(this, "SSIS.BaseTask")];
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
	"</Batch>", AppSettings.Get(this, "AnalysisServer.Database.ID"), "BO" +collectedData["AccountSettings.CubeName"].ToString());
			
			
			tNewTask.Properties["ProcessingCommands"].SetValue(tNewTask, procCmd);
           
			tNewTask.Name ="BO"+ collectedData["AccountSettings.CubeName"].ToString();
			package.PrecedenceConstraints.Add(fromTask, newTask);


			application.SaveToXml(Path.Combine(AppSettings.Get(this, "SSIS.SSISNewTaskPath"),collectedData["AccountSettings.CubeName"].ToString()+".dtsx"), package, null);
			//  app.SaveToDtsServer(p, null, @"D:\SSIS_Projects\test10.dtsx", "localhost"); //ToDo: may be this is better check with amit
           
			
		}
	}
}
