using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Services;
using EdgeBI.Wizards.AccountWizard.CubeCreation;
using Easynet.Edge.Core.Configuration;
using Microsoft.AnalysisServices;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using Easynet.Edge.Core.Data;


namespace EdgeBI.Wizards.AccountWizard
{
	class CreateRoleStepExecutor : StepExecuter
	{

		protected override Easynet.Edge.Core.Services.ServiceOutcome DoWork()
		{
			
			Log.Write("Getting collected data from suitable collector", LogMessageType.Information);

			Dictionary<string, object> collectedData = this.GetStepCollectedData(Instance.Configuration.Options["CollectorStep"]);
			this.ReportProgress(0.1f);
			Log.Write("Creating role on analysis server", LogMessageType.Information);
			CreateRole(collectedData);
			this.ReportProgress(0.7f);
			Log.Write("Role Created", LogMessageType.Information);

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

		private void CreateRole(Dictionary<string, object> collectedData)
		{
			//Connect To analysisServer
			using (Server analysisServer = new Server())
			{


				analysisServer.Connect(AppSettings.Get(this, "AnalysisServer.ConnectionString"));

				//Get the database
				Database analysisDatabase = analysisServer.Databases.GetByName(AppSettings.Get(this, "AnalysisServer.Database"));

				//Create new role
				Role newRole;
				try
				{
					newRole = analysisDatabase.Roles.Add(collectedData["AccountSettings.RoleName"].ToString(), collectedData["AccountSettings.RoleID"].ToString());
					newRole.Members.Add(new RoleMember(collectedData["AccountSettings.RoleMemberName"].ToString()));
					newRole.Update();
				}
				catch (Exception ex)
				{
					Log.Write("Error when adding a role", ex);
					throw new Exception("Error when adding a role", ex);
				}

			}
		}

		



	}

}
