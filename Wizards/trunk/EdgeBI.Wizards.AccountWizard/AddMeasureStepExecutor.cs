using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;

namespace EdgeBI.Wizards.AccountWizard
{
    class AddMeasureStepExecutor : StepExecuter
    {
        protected override Easynet.Edge.Core.Services.ServiceOutcome DoWork()
        {

            Log.Write("Getting collected data ", LogMessageType.Information);

            Dictionary<string, object> collectedData = this.GetCollectedData();
            if (collectedData.ContainsKey(ApplicationIDKey))
                SetAccountWizardSettingsByApllicationID(Convert.ToInt32(collectedData[ApplicationIDKey]));
            this.ReportProgress(0.1f);

            Log.Write("Adding measure", LogMessageType.Information);
            AddMeasure(collectedData);
            this.ReportProgress(0.7f);
            Log.Write("Measure Added", LogMessageType.Information);

            Log.Write("Update OLTP datbase", LogMessageType.Information);
            // UpdateOltpDataBASE(collectedData);
            UpdateOltpDataBase(collectedData);
            this.ReportProgress(1);

            return base.DoWork();
        }

        private void AddMeasure(Dictionary<string, object> collectedData)
        {
            using (SqlConnection sqlConnection = new SqlConnection(this.accountWizardSettings.Get("OLTP.Connection.string")))
            {
                SqlTransaction sqlTransaction = null;
                try
                {

                    sqlConnection.Open();
                    sqlTransaction = sqlConnection.BeginTransaction();
                    SqlCommand sqlCommand;

                    
                    //cpa1
                    if (collectedData.ContainsKey("AccountSettings.ACQ1"))
                    {
                        sqlCommand = DataManager.CreateCommand(@"INSERT INTO Measure
                                                                (AccountID,BaseMeasureID,DisplayName,TargetMeasureID)
                                                                VALUES
                                                                (@AccountID:Int,5,@DisplayName:NvarChar,1)");

                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.Transaction = sqlTransaction;
                        sqlCommand.Parameters["@AccountID"].Value = collectedData["AccountSettings.MeasureAccountID"];
                        sqlCommand.Parameters["@DisplayName"].Value = ((Replacment)collectedData["AccountSettings.ACQ1"]).ReplaceTo;



                        sqlCommand.ExecuteNonQuery();




                        //target cpa 1
                        sqlCommand = DataManager.CreateCommand(@"INSERT INTO Measure
                                                                (AccountID,BaseMeasureID,DisplayName)
                                                                VALUES
                                                                (@AccountID:Int,1,@DisplayName:NvarChar)");

                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.Transaction = sqlTransaction;
                        sqlCommand.Parameters["@AccountID"].Value = collectedData["AccountSettings.MeasureAccountID"];
                        sqlCommand.Parameters["@DisplayName"].Value = string.Format("CPA({0})", ((Replacment)collectedData["AccountSettings.ACQ1"]).ReplaceTo);

                        sqlCommand.ExecuteNonQuery(); 
                    }

                    //Cpa2
                    if (collectedData.ContainsKey("AccountSettings.ACQ2"))
                    {
                        sqlCommand = DataManager.CreateCommand(@"INSERT INTO Measure
                                                                (AccountID,BaseMeasureID,DisplayName,TargetMeasureID)
                                                                VALUES
                                                                (@AccountID:Int,7,@DisplayName:NvarChar,9)");

                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.Transaction = sqlTransaction;
                        sqlCommand.Parameters["@AccountID"].Value = collectedData["AccountSettings.MeasureAccountID"];
                        sqlCommand.Parameters["@DisplayName"].Value = ((Replacment)collectedData["AccountSettings.ACQ2"]).ReplaceTo;



                        sqlCommand.ExecuteNonQuery();




                        //target cpa 2
                        sqlCommand = DataManager.CreateCommand(@"INSERT INTO Measure
                                                                (AccountID,BaseMeasureID,DisplayName)
                                                                VALUES
                                                                (@AccountID:Int,9,@DisplayName:NvarChar)");

                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.Transaction = sqlTransaction;
                        sqlCommand.Parameters["@AccountID"].Value = collectedData["AccountSettings.MeasureAccountID"];
                        sqlCommand.Parameters["@DisplayName"].Value = string.Format("CPA({0})", ((Replacment)collectedData["AccountSettings.ACQ2"]).ReplaceTo);

                        sqlCommand.ExecuteNonQuery();
                    }

                    
                    sqlTransaction.Commit();

                }
                catch (Exception ex)
                {
                    Log.Write("Error adding measure ", ex);
                    sqlTransaction.Rollback();
                }

            }
        }
    }
}
