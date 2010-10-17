using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Services;
using EdgeBI.Wizards.AccountWizard.CubeCreation;
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
			//TODO: Update progress;
			Log.Write("Getting collected data from suitable collector", LogMessageType.Information);

			Dictionary<string, object> collectedData = this.GetStepCollectedData(Instance.Configuration.Options["CollectorStep"]);


			RoleCreation roleCreation = new RoleCreation();
			Database database = null;
			bool roleCreated = false;

			try
			{
				Log.Write("Connect To AnalysisServices Database", LogMessageType.Information);
				database = ConnectToAnalysisServicesDatabase(); //TODO: should it be here?


			}
			catch (Exception)
			{

				throw;
			}

			try
			{
				Log.Write("Creating New Role", LogMessageType.Information);
				roleCreated = roleCreation.RoleUpdate(database, collectedData["RoleName"].ToString(), collectedData["RoleID"].ToString(), collectedData["RoleMemberName"].ToString());



			}
			catch (Exception)
			{
				//TODO: EXCEPTIONS?
				throw;
			}
			if (!roleCreated)
			{
				//TODO: DO SOMTHING IN LIRAN CODE IT'S JUST RETURN


			}
			try
			{
				
				string scopName=string.Empty; //TODO: WHERE IS IT TAKING FROM
				FillDB("User_Gui_AccountSettings", null, null, "Scope Name", scopName, "Role Name", collectedData["RoleName"].ToString(), "Role ID", collectedData["RoleID"].ToString(), "Role Member Name", collectedData["RoleMemberName"].ToString());
			}
			catch (Exception ex)
			{
				//MessageBox.Show("Fill User_Gui_AccountSettings table in db error: " + ex.ToString());
				throw ex; //TODO: EXCEPTIONS?
			}
			return base.DoWork();
		}

		private Database ConnectToAnalysisServicesDatabase()
		{
			throw new NotImplementedException();
		}
		private void FillDB(string tableName, string listName, Dictionary<string, string> measures, params string[] values)
		{
			DataManager.ConnectionString = "";

			//TODO BETTER WAY TO WRITE THE QUERIES + ADD PARAMETERS
			using (SqlConnection sqlConnection = new SqlConnection())//TODO WHERE DO I take the connection string from?
			{
				using (SqlCommand sqlCommand = new SqlCommand()) //TODO sqlcmd.CommandText = "insert into easynet_OLTP.dbo.User_Gui_AccountSettings values('0', '" + textBox16.Text + "', 'Role Name', '" + textBox10.Text + "')";
				{
					sqlConnection.Open();
					sqlCommand.Connection = sqlConnection;
					sqlCommand.CommandType = CommandType.Text;
					string scopeID=string.Empty; //TODO WHERE DO I take the scopeID
					if (tableName.Equals("User_Gui_AccountSettings"))
					{
						string command = "insert into easynet_OLTP.dbo.User_Gui_AccountSettings values('" + scopeID + "', 'null', "; //TODO UNDERSTAD WHAT IS IT DO?
						if (measures != null)
						{
							IDictionaryEnumerator ide = measures.GetEnumerator();
							StringBuilder sb = new StringBuilder();
							while (ide.MoveNext())
							{
								sb.Append(ide.Key + "=");
								sb.Append(ide.Value + ";");
							}
							string[] pairVAlues = { listName, sb.ToString() };
							command += createCommand(tableName, pairVAlues);
							sqlCommand.CommandText = command;

							sqlCommand.ExecuteNonQuery();
							command = "insert into easynet_OLTP.dbo.User_Gui_AccountSettings values('" + scopeID + "', 'null', ";//TODO UNDERSTAD WHAT IS IT DO?
						}
						for (int i = 0; i < values.Length - 1; i += 2)
						{
							if (values[i + 1] != null && !values[i + 1].Equals(";"))
							{
								string[] pairVAlues = { values[i], values[i + 1] };
								command += createCommand(tableName, pairVAlues);
								sqlCommand.CommandText = command;

								sqlCommand.ExecuteNonQuery();
							}
							//initilization of query string
							command = "insert into easynet_OLTP.dbo.User_Gui_AccountSettings values('" + scopeID + "', 'null', ";
						}
					}
					else if (tableName.Equals("User_Gui_General_Settings"))
					{
						string command = "insert into easynet_OLTP.dbo.User_Gui_General_Settings values(";
						for (int i = 0; i < values.Length - 1; i += 2)
						{
							if (values[i + 1] != null && !values[i + 1].Equals(";"))
							{
								string[] pairVAlues = { values[i], values[i + 1] };
								command += createCommand(tableName, pairVAlues);
								sqlCommand.CommandText = command;

								sqlCommand.ExecuteNonQuery();
							}
							//initilization of query string
							command = "insert into easynet_OLTP.dbo.User_Gui_General_Settings values(";
						}
					}

				}

			}


		}
		private string createCommand(string tableName, string[] values)
		{
			string command = string.Empty;
			for (int i = 0; i < values.Length - 1; i++)
			{
				command += "'" + values[i] + "', ";
			}
			command += "'" + values[values.Length - 1] + "'";
			command += ", '" + DateTime.Now + "')";
			return command;

		}
	}

}
