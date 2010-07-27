using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Services.DataRetrieval;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Services.DataRetrieval.Retriever;
using System.Data;

namespace Easynet.Edge.Services.StatusManager
{

    class CampaignStatus : BaseStatusService
    {
        protected int campaignID = 0;
        protected CampaignWebService.CampaignStatus status;
        protected string campaignName = string.Empty;
        protected CampaignWebService.CampaignSelector selector;
        protected CampaignWebService.CampaignServiceInterfaceClient CampaignService;
        protected CampaignWebService.MySoapHeader header;
        protected CampaignWebService.CampaignPage page;
        protected CampaignWebService.MySoapResponseHeader response;
        //protected override void UpdateStatusValuesInDB(int accountId, int channelId, string campaignName, int status, int campaignID)
        //{            
        //    using (DataManager.Current.OpenConnection())
        //    {
		// COMMENT HADASG
		// OD COMMENT

              
        //        try
        //        { 
        //            //SqlCommand SPCmd = DataManager.CreateCommand(@"GkManager_SetCampaignStatus
        //            SqlCommand SPCmd = DataManager.CreateCommand(@"GkManager_SetCampaignStatus(@Account_ID:Int,@Channel_ID:Int,@campaign:NVarChar,@campaignStatus:Int,@campaignid:Int)", 
        //                      CommandType.StoredProcedure);

        //            SPCmd.CommandTimeout = 120;
        //            SPCmd.Parameters["@Account_ID"].Value = accountId;
        //            SPCmd.Parameters["@Channel_ID"].Value = channelId;
        //            SPCmd.Parameters["@campaign"].Value = campaignName;
        //            SPCmd.Parameters["@campaignStatus"].Value = status;
        //            SPCmd.Parameters["@campaignid"].Value = campaignID;
 
        //            object result = SPCmd.ExecuteScalar();

        //            try
        //            {
        //                if (result != null)
        //                {
        //                    int _campaignID = Convert.ToInt32(result);
        //                    Console.WriteLine("campaignID: " + campaignID + " ,   campaignName: " + campaignName);
 
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                Console.WriteLine("failed to upadte in db: campaign id=" + campaignID + " ,   campaignName: " + campaignName);
        //            }                     
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Write("Failed to write keepalive time to DB.", ex);
        //        }
        //    } 
        //}

        protected override bool InitObject()
        {
            try
            {
                _updateStatusSqlCommand = @"GkManager_SetCampaignStatus(@Account_ID:Int,@Channel_ID:Int,@campaign:NVarChar,@campaignStatus:Int,@campaignid:Int)";

                _sp_SelecConsts = @"Select * from Constant_CampaignStatus";
                selector = new CampaignWebService.CampaignSelector();


                selector.campaignStatuses = new Easynet.Edge.Services.StatusManager.CampaignWebService.CampaignStatus[3];
                selector.campaignStatuses[0] = Easynet.Edge.Services.StatusManager.CampaignWebService.CampaignStatus.ACTIVE;
                selector.campaignStatuses[1] = Easynet.Edge.Services.StatusManager.CampaignWebService.CampaignStatus.PAUSED;
                selector.campaignStatuses[2] = Easynet.Edge.Services.StatusManager.CampaignWebService.CampaignStatus.DELETED;
                InitalizeServiceData();
                CampaignService = new CampaignWebService.CampaignServiceInterfaceClient();

                page = new CampaignWebService.CampaignPage();
                header = new CampaignWebService.MySoapHeader();

                status = new CampaignWebService.CampaignStatus();

                return true;
            }
            catch (Exception ex)
            {
                Core.Utilities.Log.Write("error in InitObject(): " + ex.Message, Core.Utilities.LogMessageType.Error);
                return false;
            }
        }
        protected override bool InitRequestHeader()
        {
            try
            {
               
                header.userAgent = _accountData.UserAgent;
                header.applicationToken = _accountData.AppToken;
                header.developerToken = _accountData.Token;

                header.authToken = FetchAutoToken(_accountData.UserAgent);
                header.developerToken = _accountData.Token;
                header.applicationToken = _accountData.AppToken;
                GetAccountAccessData();
                _accountData.ClientEmail = _accountEmail;
                header.clientEmail = _accountEmail;

                return true;
            }

            catch (Exception ex)
            {
                Core.Utilities.Log.Write("error in InitRequestHeader(): " + ex.Message, Core.Utilities.LogMessageType.Error);
                return false;
            }
        }

        protected override bool RunRequest()
        {
            try
            {
                errorSTR.Append("errors: ");
                int index = 0;
                response = CampaignService.get(header, selector, out page);
                RunOnResults();
                index += page.entries.Count<Easynet.Edge.Services.StatusManager.CampaignWebService.Campaign>();
                if (index > 0)
                {
                    while (index % MaxRes == 0)
                    {
                        selector.paging.startIndex = index;
                        response =CampaignService.get(header, selector, out page);
                        RunOnResults();
                        index += page.entries.Count<Easynet.Edge.Services.StatusManager.CampaignWebService.Campaign>();              
                    }
                    needToRunResults = false;
                    return true;
                }
                
               
                return true;


            }
            catch (Exception ex)
            {

                Core.Utilities.Log.Write("error in RunRequest(): " + ex.Message, Core.Utilities.LogMessageType.Error);
                return false;
            }
            finally
            {
                 Log.Write(errorSTR.ToString(), LogMessageType.Error);
            }

        }

        protected override bool RunOnResults()
        {
            try
            {  
                foreach (var item in page.entries)
                {

                    status = item.status;
                   
                    campaignID = Convert.ToInt32(item.id);
                    campaignName = item.name;


                   
                    commandType = CommandType.StoredProcedure;
                    BuildSqlParamsDictionray();
                    UpdateStatusValuesInDB();


                }
                return true;
            }
            catch (Exception ex)
            {

                Core.Utilities.Log.Write("error in RunOnResults(): " + ex.Message, Core.Utilities.LogMessageType.Error);
                return false;
            }

        }
        //protected override ServiceOutcome DoWork()
        //{
            
        //}



        protected override void BuildSqlParamsDictionray()
        {
            /*
             * 
            SqlCommand SPCmd = DataManager.CreateCommand(@"GkManager_SetCampaignStatus(@Account_ID:Int,@Channel_ID:Int,@campaign:NVarChar,@campaignStatus:Int,@campaignid:Int)", 
                              CommandType.StoredProcedure);

                    SPCmd.CommandTimeout = 120;
                    SPCmd.Parameters["@Account_ID"].Value = accountId;
                    SPCmd.Parameters["@Channel_ID"].Value = channelId;
                    SPCmd.Parameters["@campaign"].Value = campaignName;
                    SPCmd.Parameters["@campaignStatus"].Value = status;
                    SPCmd.Parameters["@campaignid"].Value = campaignID;
 
 
             
             * */
            sqlParamsDictionary.Clear();
            sqlParamsDictionary.Add("@Account_ID", _accountID.ToString());
            sqlParamsDictionary.Add("@Channel_ID", "1");
            sqlParamsDictionary.Add("@campaign", campaignName);
            sqlParamsDictionary.Add("@campaignStatus", StatusHashSet[status.ToString()].ToString());
            sqlParamsDictionary.Add("@campaignid", campaignID.ToString());
             
        }


    }
}