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
		#region IStepExecuter Members		
		public int SessionID
		{
			get
			{
				return int.Parse(this.Instance.ParentInstance.ParentInstance.Configuration.Options["SessionID"]);
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
		protected Dictionary<string, object> GetStepCollectedData(string StepName, int SessionID)
		{
			Dictionary<string, object> collectedData=null;
			using (DataManager.Current.OpenConnection())
			{
				using (SqlCommand sqlCommand = DataManager.CreateCommand(@"SELECT Field,Value
																		FROM Wizards_Data_Per_WizardID_SessionID_Step_And_Field 
																			WHERE StepName=@StepName:NVarChar 
																			AND SessionID=@SessionID:Int"))
				{
					sqlCommand.Parameters["@SessionID"].Value = SessionID;
					sqlCommand.Parameters["@StepName"].Value = StepName;
					using (SqlDataReader reader=sqlCommand.ExecuteReader())
					{
						if (reader!=null && reader.HasRows)
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
		/// Update the current step and session on Wizards_Sessions_Data
		/// </summary>
		/// <param name="CurrentStepNameOrStatus"></param>
		protected void UpdateCurrentStepNameAndStatus(string CurrentStepNameOrStatus)
		{
			using (DataManager.Current.OpenConnection())
			{
				using (SqlCommand sqlCommand = DataManager.CreateCommand(@"UPDATE Wizards_Sessions_Data
																			SET CurrentStepName=@CurrentStepNameOrStatus:NVarChar
																			WHERE SessionID=@SessionID:Int"))
				{
					sqlCommand.Parameters["@SessionID"].Value = SessionID;
					sqlCommand.Parameters["@CurrentStepNameOrStatus"].Value = CurrentStepNameOrStatus;
					sqlCommand.ExecuteNonQuery();
				}
			}

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
