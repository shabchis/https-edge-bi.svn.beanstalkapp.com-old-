using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;
using System.ComponentModel;
using Easynet.Edge.Core.Configuration;

namespace EdgeBI.Wizards
{
    /// <summary>
    /// Base executer 
    /// </summary>
    public class StepExecuter : Service
    {
        #region consts

        protected const string ApplicationIDKey = "AccountSettings.ApplicationID";
        #endregion
        #region Fields
        protected AccountWizardSettings accountWizardSettings;
        #endregion
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
        protected Dictionary<string, object> GetCollectedData()
        {

            Dictionary<string, object> collectedData = null;
            using (DataManager.Current.OpenConnection())
            {
                using (SqlCommand sqlCommand = DataManager.CreateCommand(@"SELECT Field,Value,ValueType
																		FROM Wizards_Data_Per_WizardID_SessionID_Step_And_Field 
																			WHERE  SessionID=@SessionID:Int 
																			AND ServiceInstanceID=@ServiceInstanceID:BigInt"))
                {
                    sqlCommand.Parameters["@SessionID"].Value = WizardSession.SessionID;
                    sqlCommand.Parameters["@ServiceInstanceID"].Value = this.Instance.ParentInstance.ParentInstance.InstanceID;
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        if (reader != null && reader.HasRows)
                        {
                            collectedData = new Dictionary<string, object>();
                        }
                        while (reader.Read())
                        {
                            Type t = Type.GetType(reader.GetString(2));
                            if (!collectedData.ContainsKey(reader.GetString(0)))
                                collectedData.Add(reader.GetString(0), TypeDescriptor.GetConverter(t).ConvertFromString(reader.GetString(1)));
                        }
                    }
                }
            }
            return collectedData;
        }
        protected void SetAccountWizardSettingsByApllicationID(int apllicationID)
        {
            accountWizardSettings = new AccountWizardSettings(apllicationID);
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
																			(WizardID,SessionID,ServiceInstanceID,StepName,Field,Value,ValueType)
																			Values 
																			(@WizardID:Int,
																			@SessionID:Int,
																			@ServiceInstanceID:BigInt,
																			@StepName:NvarChar,
																			@Field:NVarChar,
																			@Value:NVarChar,
                                                                            @ValueType:NVarChar)"))
                    {
                        sqlCommand.Parameters["@WizardID"].Value = WizardSession.WizardID;
                        sqlCommand.Parameters["@SessionID"].Value = WizardSession.SessionID;
                        sqlCommand.Parameters["@ServiceInstanceID"].Value = Instance.ParentInstance.ParentInstance.InstanceID;
                        sqlCommand.Parameters["@StepName"].Value = WizardSession.CurrentStep.StepName;
                        sqlCommand.Parameters["@Field"].Value = input.Key;
                        sqlCommand.Parameters["@Value"].Value = input.Value.ToString();
                        sqlCommand.Parameters["@ValueType"].Value = input.Value.GetType().AssemblyQualifiedName;
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


                using (SqlCommand sqlCommand = DataManager.CreateCommand(@"SELECT Field,Value,ValueType
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
                                    Type t = Type.GetType(reader.GetString(2));

                                    executorData.Add(reader.GetString(0), TypeDescriptor.GetConverter(t).ConvertFromString(reader.GetString(1)));

                                }

                            }

                        }


                    }

                }

            }
            return executorData;
        }
        protected override void OnInit()
        {
            base.OnInit();
            WriteInstanceData(WizardSession.SessionID, Instance.InstanceID);
        }

        private void WriteInstanceData(int sessionID, long instanceID)
        {
            using (DataManager.Current.OpenConnection())
            {
                using (SqlCommand sqlCommand = DataManager.CreateCommand(@"INSERT INTO Session_Instance_Data
                                                                         (Session,InstanceID)
                                                                         VALUES
                                                                        (@Session:Int,@InstanceID:bigint)"))
                {
                    sqlCommand.Parameters["@Session"].Value = sessionID;
                    sqlCommand.Parameters["@InstanceID"].Value = instanceID;
                    sqlCommand.ExecuteNonQuery();


                }

            }
        }

        protected void UpdateOltpDataBase(Dictionary<string, object> collectedData)
        {
            using (SqlConnection sqlConnection = new SqlConnection(accountWizardSettings.Get("OLTP.Connection.string")))
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
                            sqlCommand.Parameters["@ScopeID"].Value = collectedData["AccountSettings.BI_Scope_ID"]; //TODO: TEMPORARLY WILL COME FROM OTHER COLLECTOR (GENERAL COLLECTOR-ASK DORON)
                            sqlCommand.Parameters["@AccountID"].Value = DBNull.Value; //TODO: CHECK THIS FOR NOW IT'S NULL
                            sqlCommand.Parameters["@Name"].Value = input.Key;
                            if (input.Value is Replacment)
                            {
                                Replacment replacement = (Replacment)input.Value;
                                sqlCommand.Parameters["@Value"].Value = replacement.ReplaceTo;

                            }
                            else
                            {
                                sqlCommand.Parameters["@Value"].Value = input.Value;

                            }
                            sqlCommand.Parameters["@sys_creation_date"].Value = DateTime.Now;

                            sqlCommand.ExecuteNonQuery();

                        }
                    }


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
