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

    class AdGroupStatus : BaseStatusService
    {


        protected int adGroupID = 0;
        protected AdGroupWebService.AdGroupStatus status = new AdGroupWebService.AdGroupStatus();
        protected string adGroupName = string.Empty; 
        protected long campaignGK;
        protected AdGroupWebService.AdGroupSelector selector;
        protected AdGroupWebService.AdGroupServiceInterfaceClient AdGroupService;
        protected AdGroupWebService.MySoapHeader header;
        protected AdGroupWebService.AdGroupPage page;
        protected AdGroupWebService.MySoapResponseHeader response;

       

      

        protected override bool InitObject()
        {
            try
            {
                 _updateStatusSqlCommand = @"GKManager_SetAdGroupStatus(@Account_ID:Int,@Campaign_GK:int@Channel_ID:Int,@agStatus:Int,@adgroup:NVarChar,@adgroupID:Int)";
                

                _sp_SelecConsts =  @"Select * from Constant_AdGroupStatus";
                selector = new AdGroupWebService.AdGroupSelector();


                InitalizeServiceData();
                AdGroupService = new AdGroupWebService.AdGroupServiceInterfaceClient();

                page = new AdGroupWebService.AdGroupPage();
                header = new AdGroupWebService.MySoapHeader();

                status = new AdGroupWebService.AdGroupStatus();

                return true;
            }
            catch (Exception ex)
            {
                Core.Utilities.Log.Write("error in InitObject(): " + ex.Message, Core.Utilities.LogMessageType.Error);
                return false;
            }
        }
        
        protected override bool RunOnResults()
        {
            try
            {
                foreach (var item in page.entries)
                {

                    status = item.status;
                    
                    adGroupID = Convert.ToInt32(item.id);
                    adGroupName = item.name;

                    campaignGK = Easynet.Edge.BusinessObjects.GkManager.GetCampaignGK(_accountID, 1, item.campaignName, item.campaignId);


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
                 
                int index = 0;
                response = AdGroupService.get(header, selector, out page);
                RunOnResults();
                index += page.entries.Count<Easynet.Edge.Services.StatusManager.AdGroupWebService.AdGroup>();
                if (index > 0)
                {
                    while (index % MaxRes == 0)
                    {
                        selector.paging.startIndex = index;
                        response = AdGroupService.get(header, selector, out page);
                        RunOnResults();
                        index += page.entries.Count<Easynet.Edge.Services.StatusManager.AdGroupWebService.AdGroup>();
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

        }
        
        protected override void BuildSqlParamsDictionray()
        {
            /*
             * 
             SPCmd.CommandTimeout = 120;
                    SPCmd.Parameters["@Account_ID"].Value = accountId;
                    SPCmd.Parameters["@Channel_ID"].Value = channelId;
                    SPCmd.Parameters["@Campaign_GK"].Value = campaignGK;
                    SPCmd.Parameters["@agStatus"].Value = status;
                    SPCmd.Parameters["@adgroup"].Value = adGroupName;                    
                    SPCmd.Parameters["@adgroupID"].Value = adGroupID;
 
             
             * */
            sqlParamsDictionary.Clear();
            sqlParamsDictionary.Add("@Account_ID", _accountID.ToString());
            sqlParamsDictionary.Add("@Channel_ID", "1");
            sqlParamsDictionary.Add("@Campaign_GK", campaignGK.ToString());
            sqlParamsDictionary.Add("@agStatus", StatusHashSet[status.ToString()].ToString());
            sqlParamsDictionary.Add("@adgroup", adGroupName);              
            sqlParamsDictionary.Add("@adgroupID", adGroupID.ToString());
        }
    }
}