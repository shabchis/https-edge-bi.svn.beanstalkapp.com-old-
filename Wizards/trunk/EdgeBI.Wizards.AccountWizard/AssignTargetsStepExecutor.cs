using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Utilities;

namespace EdgeBI.Wizards.AccountWizard
{
    class AssignTargetsStepExecutor : StepExecuter
    {
        protected override Easynet.Edge.Core.Services.ServiceOutcome DoWork()
        {

            Log.Write("Getting collected data ", LogMessageType.Information);

            Dictionary<string, object> collectedData = this.GetCollectedData();
            if (collectedData.ContainsKey(ApplicationIDKey))
                SetAccountWizardSettingsByApllicationID(Convert.ToInt32(collectedData[ApplicationIDKey]));
            this.ReportProgress(0.1f);

            Log.Write("Adding measure", LogMessageType.Information);
            AssignTargets(collectedData);
            this.ReportProgress(0.7f);
            Log.Write("Measure Added", LogMessageType.Information);

            Log.Write("Update OLTP datbase", LogMessageType.Information);
            // UpdateOltpDataBASE(collectedData);
            UpdateOltpDataBase(collectedData);
            this.ReportProgress(1);

            return base.DoWork();
        }
        private void AssignTargets(Dictionary<string, object> collectedData)
        {
            double? targetCpa1 = null;
            double? targetCpa2 = null;
            int? accountID = null;
            if (collectedData.ContainsKey("AccountSettings.TargetValue1") && !string.IsNullOrEmpty(collectedData["AccountSettings.TargetValue1"].ToString()))
                targetCpa1 = Convert.ToDouble(collectedData["AccountSettings.TargetValue1"]);
            if (collectedData.ContainsKey("AccountSettings.TargetValue2") && !string.IsNullOrEmpty(collectedData["AccountSettings.TargetValue2"].ToString()))
                targetCpa2 = Convert.ToDouble(collectedData["AccountSettings.TargetValue2"]);
            if (collectedData.ContainsKey("AccountSettings.MeasureAccountID") && !string.IsNullOrEmpty(collectedData["AccountSettings.MeasureAccountID"].ToString()))
                accountID = Convert.ToInt32(collectedData["AccountSettings.MeasureAccountID"]);


            if ((targetCpa1 != null || targetCpa2 != null) && accountID != null)
            {
                using (SqlConnection sqlConnection = new SqlConnection(this.accountWizardSettings.Get("OLTP.Connection.string")))
                {
                    SqlTransaction sqlTransaction = null;

                    sqlConnection.Open();

                    SqlCommand sqlCommand;

                    List<string> campaigns = new List<string>();

                    sqlCommand = DataManager.CreateCommand(@"SELECT Campaign_GK 
                                                                    FROM UserProcess_GUI_PaidCampaign
                                                                    WHERE Account_ID=@Account_ID:Int");

                    sqlCommand.Connection = sqlConnection;

                    sqlCommand.Parameters["@Account_ID"].Value = accountID;
                    using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            campaigns.Add(sqlDataReader[0].ToString());
                        }

                    }





                    if (campaigns.Count > 0)
                        sqlTransaction = sqlConnection.BeginTransaction();
                    foreach (string campaign in campaigns)
                    {

                        sqlCommand = DataManager.CreateCommand(@"INSERT INTO User_GUI_CampaignTargets
                                                                (AccountID,CampaignGK,AdgroupGK,SegmentID,CPA_new_users,CPA_new_activations)
                                                                VALUES
                                                                (@AccountID:Int,@CampaignGK:Int,-1,-1,@CPA_new_users:Float,@CPA_new_activations:Float)");

                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.Transaction = sqlTransaction;
                        sqlCommand.Parameters["@AccountID"].Value = accountID;
                        sqlCommand.Parameters["@CampaignGK"].Value = campaign;
                        if (targetCpa1 != null)
                            sqlCommand.Parameters["@CPA_new_users"].Value = targetCpa1;
                        else
                            sqlCommand.Parameters["@CPA_new_users"].Value = DBNull.Value;

                        if (targetCpa2 != null)
                            sqlCommand.Parameters["@CPA_new_activations"].Value = targetCpa2;
                        else
                            sqlCommand.Parameters["@CPA_new_activations"].Value = DBNull.Value;

                        sqlCommand.ExecuteNonQuery();
                    }
                    if (sqlTransaction != null)
                    {
                        sqlTransaction.Commit();
                    }

                }

            }
        }
    }

}
