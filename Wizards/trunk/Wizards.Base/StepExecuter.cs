using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;

namespace Easynet.Edge.Wizards
{
	/// <summary>
	/// Base executer 
	/// </summary>
	public class StepExecuter : Service
	{
		#region Properties
		protected WizardSession WizardSession
		{

			get
			{
				int sessionID = Int32.Parse(Instance.ParentInstance.ParentInstance.Configuration.Options["SessionID"]);
				int wizardID = Int32.Parse(Instance.ParentInstance.ParentInstance.Configuration.Options["WizardID"]);
				return new WizardSession() { WizardID = wizardID, SessionID = sessionID, CurrentStep = new StepConfiguration() { StepName = this.Instance.Configuration.Name, MetaData = null } };
			}
		}
		#endregion

		#region Protected Methods
		/// <summary>
		/// Return the collected step respectively
		/// </summary>
		/// <param name="StepName"></param>
		/// <param name="SessionID"></param>
		/// <returns></returns>
		protected Dictionary<string, object> GetStepCollectedData(string StepName)
		{
			Dictionary<string, object> collectedData = null;
			using (DataManager.Current.OpenConnection())
			{
				using (SqlCommand sqlCommand = DataManager.CreateCommand(@"SELECT Field,Value
																		FROM Wizards_Data_Per_WizardID_SessionID_Step_And_Field 
																			WHERE StepName=@StepName:NVarChar 
																			AND SessionID=@SessionID:Int 
																			AND ServiceInstanceID=@ServiceInstanceID:BigInt"))
				{
					sqlCommand.Parameters["@SessionID"].Value = WizardSession.SessionID;
					sqlCommand.Parameters["@StepName"].Value = StepName;
					sqlCommand.Parameters["@ServiceInstanceID"].Value = this.Instance.ParentInstance.ParentInstance.InstanceID;
					using (SqlDataReader reader = sqlCommand.ExecuteReader())
					{
						if (reader != null && reader.HasRows)
						{
							collectedData = new Dictionary<string, object>();
						}
						while (reader.Read())
						{
							collectedData.Add(reader.GetString(0), reader.GetString(1));


						}
					}
				}
			}
			return collectedData;
		}
		/// <summary>
		/// Save current executor data for next steps
		/// </summary>
		/// <param name="executorData"></param>
		protected void SaveExecutorData(Dictionary<string, object> executorData)
		{
			using (DataManager.Current.OpenConnection())
			{
				foreach (KeyValuePair<string, object> input in executorData)
				{

					using (SqlCommand sqlCommand = DataManager.CreateCommand(@"INSERT INTO Wizards_Data_Per_WizardID_SessionID_Step_And_Field 
																			(WizardID,SessionID,ServiceInstanceID,StepName,Field,Value)
																			Values 
																			(@WizardID:Int,
																			@SessionID:Int,
																			@ServiceInstanceID:BigInt,
																			@StepName:NvarChar,
																			@Field:NVarChar,
																			@Value:NVarChar)"))
					{
						sqlCommand.Parameters["@WizardID"].Value = WizardSession.WizardID;
						sqlCommand.Parameters["@SessionID"].Value = WizardSession.SessionID;
						sqlCommand.Parameters["@ServiceInstanceID"].Value = Instance.ParentInstance.ParentInstance.InstanceID;
						sqlCommand.Parameters["@StepName"].Value = WizardSession.CurrentStep.StepName;
						sqlCommand.Parameters["@Field"].Value = input.Key;
						sqlCommand.Parameters["@Value"].Value = input.Value.ToString();
						sqlCommand.ExecuteNonQuery();

					}
				}
			}

		}
		/// <summary>
		/// Get executors data by executor name
		/// </summary>
		/// <param name="stepName"></param>
		/// <returns></returns>
		protected Dictionary<string, object> GetExecutorData(string stepName)
		{
			Dictionary<string, object> executorData = null;
			using (DataManager.Current.OpenConnection())
			{


				using (SqlCommand sqlCommand = DataManager.CreateCommand(@"SELECT Field,Value
																			   FROM Wizards_Data_Per_WizardID_SessionID_Step_And_Field
																			   WHERE StepName=@StepName:NvarChar
																		       AND ServiceInstanceID=@ServiceInstanceID:BigInt
																			   AND SessionID=@SessionID:Int
																			   AND WizardID=@WizardID:Int"))
				{
					sqlCommand.Parameters["@WizardID"].Value = WizardSession.WizardID;
					sqlCommand.Parameters["@SessionID"].Value = WizardSession.SessionID;
					sqlCommand.Parameters["@ServiceInstanceID"].Value = Instance.ParentInstance.ParentInstance.InstanceID;
					sqlCommand.Parameters["@StepName"].Value = stepName;
					using (SqlDataReader reader = sqlCommand.ExecuteReader())
					{
						if (reader != null)
						{
							if (reader.HasRows)
							{
								executorData = new Dictionary<string, object>();
								while (reader.Read())
								{
									executorData.Add(reader.GetString(0), reader.GetString(1));

								}

							}

						}


					}

				}

			}
			return executorData;
		}
		#endregion
	}



	#region Enums
	public enum StepExecuteStatus
	{
		Started,
		Done,
		Error

	}
	#endregion
}
