using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Utilities;
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;

namespace EdgeBI.Wizards.AccountWizard
{
    class CreateNewAccountStepExecutor : StepExecuter
    {
        protected override Easynet.Edge.Core.Services.ServiceOutcome DoWork()
        {
            Log.Write("Getting collected data ", LogMessageType.Information);

            Dictionary<string, object> collectedData = this.GetCollectedData();
            if (collectedData.ContainsKey(ApplicationIDKey))
                SetAccountWizardSettingsByApllicationID(Convert.ToInt32(collectedData[ApplicationIDKey]));
            this.ReportProgress(0.1f);

            if (Convert.ToBoolean(collectedData["AccountSettings.CreateAccount"]) == true)
            {
                Log.Write("Start createing account hirarchy", LogMessageType.Information);
                CreateAccount(collectedData);
                this.ReportProgress(0.7f);
                Log.Write("account hirarchy Created", LogMessageType.Information);
                Log.Write("Update OLTP datbase", LogMessageType.Information);
                // UpdateOltpDataBASE(collectedData);
                UpdateOltpDataBase(collectedData);
            }


            this.ReportProgress(1);

            return base.DoWork();
        }

        private void CreateAccount(Dictionary<string, object> collectedData)
        {
            string accountName = collectedData["AccountSettings.AccountName"].ToString();
            string clientName = collectedData["AccountSettings.ClientName"].ToString();
            string scopeName = collectedData["AccountSettings.ScopeName"].ToString();
            int clientID;
            int baseAccountId;
            long scopeID;



            using (SqlConnection sqlConnection = new SqlConnection(this.accountWizardSettings.Get("OLTP.Connection.string")))
            {
                sqlConnection.Open();
                using (SqlCommand sqlCommand = new SqlCommand("SELECT (MAX(Account_ID)+1) FROM User_GUI_Account", sqlConnection))
                {
                    baseAccountId = Convert.ToInt32(sqlCommand.ExecuteScalar());

                }
            }

            if (accountName == clientName)
                clientID = baseAccountId;
            else
                clientID = baseAccountId + 100000;
            scopeID = baseAccountId + 1000000;


            using (SqlConnection sqlConnection = new SqlConnection(this.accountWizardSettings.Get("OLTP.Connection.string")))
            {
                SqlTransaction sqlTransaction = null;
                try
                {

                    sqlConnection.Open();
                    sqlTransaction = sqlConnection.BeginTransaction();
                    SqlCommand sqlCommand = DataManager.CreateCommand(@"INSERT INTO User_GUI_Account
                                                                (Account_Name,Account_ID,BackOffice_Status,Status,Client_ID,Client_Name,AccountSettings,Scope_ID)
                                                                VALUES
                                                                (@Account_Name:NvarChar,@Account_ID:Int,1,9,@Client_ID:Int,@Client_Name:NvarChar,@AccountSettings:NvarChar,@Scope_ID:BigInt)");

                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.Transaction = sqlTransaction;
                    sqlCommand.Parameters["@Account_Name"].Value = accountName;
                    sqlCommand.Parameters["@Account_ID"].Value = baseAccountId;
                    sqlCommand.Parameters["@Client_ID"].Value = clientID;
                    sqlCommand.Parameters["@Client_Name"].Value = clientName;

                    sqlCommand.Parameters["@AccountSettings"].Value = string.Format(@"Book:{0};ADRole:edge\{1}", accountWizardSettings.Get("Cube.BO.Name.Perfix") + collectedData["AccountSettings.CubeName"].ToString(), collectedData["ActiveDirectory.UserName"].ToString());
                    sqlCommand.Parameters["@Scope_ID"].Value = scopeID;
                    int rowAffected = sqlCommand.ExecuteNonQuery();




                    if (rowAffected > 0)
                    {
                        if (scopeName != clientName || scopeName != accountName)
                        {
                            sqlCommand = DataManager.CreateCommand(@"INSERT INTO User_GUI_Scope
                                                                (Scope_ID,Scope_Name)
                                                                VALUES
                                                                (@Scope_ID:Int,@Scope_Name:NvarChar)");
                            sqlCommand.Connection = sqlConnection;
                            sqlCommand.Transaction = sqlTransaction;

                            sqlCommand.Parameters["@Scope_ID"].Value = scopeID;
                            sqlCommand.Parameters["@Scope_Name"].Value = scopeName;
                            rowAffected = sqlCommand.ExecuteNonQuery();
                        }
                    }
                    sqlTransaction.Commit();

                }
                catch (Exception ex)
                {
                    Log.Write("Error adding account hirarchy ", ex);
                    sqlTransaction.Rollback();
                    throw ex;
                }

            }




        }

    }
}
