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

    class AdGroupAdStatus : BaseStatusService
    {

        protected int adGroupID = 0;
        protected AdGroupAdWebService.AdGroupAdStatus status = new AdGroupAdWebService.AdGroupAdStatus();
        protected string adGroupName = string.Empty; 
        protected string desc1,desc2,headline,url,displayUrl;
        protected Microsoft.Office.Interop.Excel.Workbook xls;
        protected Microsoft.Office.Interop.Excel.Worksheet xlWorkSheet;


        protected AdGroupAdWebService.AdGroupAdSelector selector;
        protected AdGroupAdWebService.AdGroupAdServiceInterfaceClient AdGroupAdService;
        protected AdGroupAdWebService.MySoapHeader header;
        protected AdGroupAdWebService.AdGroupAdPage page;
        protected AdGroupAdWebService.MySoapResponseHeader response;


        //protected override ServiceOutcome DoWork()
        //{

        //    System.Globalization.CultureInfo oldci = System.Threading.Thread.CurrentThread.CurrentCulture;
        //    System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
        //    System.Threading.Thread.CurrentThread.CurrentCulture = ci;


        //    GetStatusDicionaryFromDB(@"Select * from Constant_AdGroupStatus");
        //    ServiceOutcome ret;
        //    int index = 0;


        //    xls = CreatExcelFile();

        //    xlWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)xls.Worksheets.get_Item(0);

        //    if (xlWorkBook == null)
        //        return ServiceOutcome.Failure;
        //    xlWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkBook.Worksheets[1];

        //    xlWorkSheet.Cells[1, 1] = "AdGroupID";
        //    xlWorkSheet.Cells[1, 2] = "Status";
        //    xlWorkSheet.Cells[1, 3] = "url";
        //    xlWorkSheet.Cells[1, 4] = "dispolay url";
        //    xlWorkSheet.Cells[1, 5] = "desc1";
        //    xlWorkSheet.Cells[1, 6] = "desc2";
        //    xlWorkSheet.Cells[1, 7] = "Headline";

        //    ret = DoWork(ref index);


        //    while (index % MaxRes == 0)
        //    {
        //        DoWork(ref index);
        //    }


        //    if (index == 8390)
        //    {
        //        DoWork(ref index);
        //    }
        //    SaveExcelFile(xls, System.IO.Directory.GetCurrentDirectory() + @"\SetAdGroupAdStatus.xls");

        //    System.Threading.Thread.CurrentThread.CurrentCulture = oldci;

        //    return ret;

        //}

        protected override bool InitObject()
        {
            try
            {
                _updateStatusSqlCommand = @"GkManager_SetAdGroupAdStatus(@Account_ID:Int,@Channel_ID:Int,@creativeStatus:Int,@Desc1:NVarChar,
//                        @Desc2:NVarChar,@Headline:NVarChar,@url:NVarChar,@displayURL:NVarChar,@adgroupID:Int)";


                _sp_SelecConsts = @"Select * from Constant_AdGroupStatus";
                selector = new AdGroupAdWebService.AdGroupAdSelector();


                InitalizeServiceData();
                AdGroupAdService = new AdGroupAdWebService.AdGroupAdServiceInterfaceClient();

                page = new AdGroupAdWebService.AdGroupAdPage();
                header = new AdGroupAdWebService.MySoapHeader();

                status = new AdGroupAdWebService.AdGroupAdStatus();

                selector.paging = new Easynet.Edge.Services.StatusManager.AdGroupAdWebService.Paging();
                selector.paging.startIndexSpecified = true;
                selector.paging.startIndex = 0;
                selector.paging.numberResults = MaxRes;
                selector.paging.numberResultsSpecified = true;


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
            if (page == null)
                return false;
            try
            {
                foreach (var item in page.entries)
                {
                    
                    status = item.status;
                    try
                    {
                        url = "";
                        displayUrl = "";
                        desc1 = "";
                        desc2 = "";
                        headline = "";

                        adGroupID = Convert.ToInt32(item.adGroupId);
                        displayUrl = item.ad.displayUrl;

                        url = item.ad.url;
                        status = item.status;
                     //   Easynet.Edge.Services.StatusManager.AdGroupAdWebService.TextAd f = new Easynet.Edge.Services.StatusManager.AdGroupAdWebService.TextAd();

                        object myObject = (object)item.ad;
                        Easynet.Edge.Services.StatusManager.AdGroupAdWebService.TextAd adText = (Easynet.Edge.Services.StatusManager.AdGroupAdWebService.TextAd)myObject;
                        desc1 = adText.description1;
                        desc2 = adText.description2;
                        headline = adText.headline;
 
                    }
                    catch (Exception ex)
                    {
                        adGroupID = -2; //for error
                    }

                    commandType = CommandType.StoredProcedure;

//                    updateStatusSqlCommand = @"GkManager_SetAdGroupAdStatus(@Account_ID:Int,@Channel_ID:Int,@creativeStatus:Int,@Desc1:NVarChar,
//                        @Desc2:NVarChar,@Headline:NVarChar,@url:NVarChar,@displayURL:NVarChar,@adgroupID:Int)";
                    BuildSqlParamsDictionray();

                    UpdateStatusValuesInDB();
                }
                errorSTR.Append("ADGroupAdError: ");
                Log.Write(errorSTR.ToString(), LogMessageType.Warning);
              //  index += page.entries.Count<Easynet.Edge.Services.StatusManager.AdGroupAdWebService.AdGroupAd>();
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
                response = AdGroupAdService.get(header, selector, out page);
                RunOnResults();
                index += page.entries.Count<Easynet.Edge.Services.StatusManager.AdGroupAdWebService.AdGroupAd>();
                if (index > 0)
                {
                    while (index % MaxRes == 0)
                    {
                        selector.paging.startIndex = index;
                        response = AdGroupAdService.get(header, selector, out page);
                        RunOnResults();
                        index += page.entries.Count<Easynet.Edge.Services.StatusManager.AdGroupAdWebService.AdGroupAd>();
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
             "GkManager_SetAdGroupAdStatus(@Account_ID:Int,@Channel_ID:Int,@creativeStatus:Int,@Desc1:NVarChar,
                        @Desc2:NVarChar,@Headline:NVarChar,@url:NVarChar,@displayURL:NVarChar,@adgroupID:Int)";
 
             
             * */
            sqlParamsDictionary.Clear();
            sqlParamsDictionary.Add("@Account_ID", _accountID.ToString());
            sqlParamsDictionary.Add("@Channel_ID", "1");
            sqlParamsDictionary.Add("@creativeStatus", StatusHashSet[status.ToString()].ToString());
            sqlParamsDictionary.Add("@Desc1", desc1);
            sqlParamsDictionary.Add("@Desc2", desc2);
            sqlParamsDictionary.Add("@Headline", headline);
            sqlParamsDictionary.Add("@url", url);
            sqlParamsDictionary.Add("@displayURL", displayUrl);
            sqlParamsDictionary.Add("@adgroupID", adGroupID.ToString());
        }
    }
}