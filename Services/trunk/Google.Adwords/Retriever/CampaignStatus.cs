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

namespace Easynet.Edge.Services.Google.Adwords.Retriever
{
    class CampaignServiceWrapper : Easynet.Edge.Services.Google.Adwords.com.google.adwords.CampaignService
    {

    }

    //class mySoapHeader : Easynet.Edge.Services.Google.Adwords.com.google.adwords.SoapHeader
    //{

    //}
    class CampaignStatus : BaseRetriever
    {
        private string _accountEmail = string.Empty;
        AccountData _accountData = new AccountData();
        private const string UserAgent = "Easynet(seperia) -- C# get clients information";
 
         
        
        System.Collections.Hashtable campaignStatusHashSet = new System.Collections.Hashtable();

        private void GetCampaignStatusDicFromDB( )
        {
            try
            {

                string queryString = @"Select * from Constant_CampaignStatus";

                SqlDataAdapter adapter = new SqlDataAdapter(queryString, new SqlConnection(DataManager.ConnectionString));

                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                foreach (DataRow row in dataTable.Rows)
                {
                    campaignStatusHashSet.Add(row[0].ToString().ToUpper(), row[1]);
                } 
            }               
 
            catch(Exception ex){}
             

        }
        /// <summary>
        /// 
        /// </summary>
        private void UpdateCampaignStatusInDB(int accountId,int channelId,string campaignName, int status,int campaignID)
        {            
            using (DataManager.Current.OpenConnection())
            {
                try
                { 
                    //SqlCommand SPCmd = DataManager.CreateCommand(@"GkManager_SetCampaignStatus
                    SqlCommand SPCmd = DataManager.CreateCommand(@"GkManager_SetCampaignStatus(@Account_ID:Int,@Channel_ID:Int,@campaign:NVarChar,@campaignStatus:Int,@campaignid:Int)", 
                              CommandType.StoredProcedure);

                    SPCmd.CommandTimeout = 120;
                    SPCmd.Parameters["@Account_ID"].Value = accountId;
                    SPCmd.Parameters["@Channel_ID"].Value = channelId;
                    SPCmd.Parameters["@campaign"].Value = campaignName;
                    SPCmd.Parameters["@campaignStatus"].Value = status;
                    SPCmd.Parameters["@campaignid"].Value = campaignID;
 
                    object result = SPCmd.ExecuteScalar();

                    try
                    {
                        if (result != null)
                        {
                            int _campaignID = Convert.ToInt32(result);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("failed to upadte in db: campaign id=" + campaignID.ToString() + " , status=" + status.ToString());
                    }                     
                }
                catch (Exception ex)
                {
                    Log.Write("Failed to write keepalive time to DB.", ex);
                }
            } 
        }

        private void InitializeRequestHeader(ref com.google.adwords.CampaignService CampaignService )
        {
            CampaignService.RequestHeader.userAgent = _accountData.UserAgent;
            CampaignService.RequestHeader.applicationToken = _accountData.AppToken;
            CampaignService.RequestHeader.developerToken = _accountData.Token;
            CampaignService.RequestHeader.authToken = FetchAutoToken(_accountData.UserAgent);
            CampaignService.RequestHeader.developerToken = _accountData.Token;
            CampaignService.RequestHeader.applicationToken = _accountData.AppToken;
            GetAccountAccessData();
            _accountData.ClientEmail = _accountEmail;
            CampaignService.RequestHeader.clientEmail = _accountEmail;

        }
        protected override ServiceOutcome DoWork()
        {
           
            Console.WriteLine("-------ccccccccccc-");
            Console.WriteLine("AccountID: {0}", Instance.AccountID);
            Console.WriteLine("TestMode (option): {0}", Instance.Configuration.Options["TestMode"]);
            Console.WriteLine("----------vvvvvvvvvvvvvvvvvvvvvvvvvvv---------");

            com.google.adwords.CampaignSelector selector =new Easynet.Edge.Services.Google.Adwords.com.google.adwords.CampaignSelector();
            
     
            InitalizeServiceData();
            com.google.adwords.CampaignService CampaignService = new com.google.adwords.CampaignService();           
 

             CampaignService.RequestHeader = new Easynet.Edge.Services.Google.Adwords.com.google.adwords.MySoapHeader();

             InitializeRequestHeader(ref CampaignService );
            
          
 
            
       
            com.google.adwords.CampaignPage page = new Easynet.Edge.Services.Google.Adwords.com.google.adwords.CampaignPage();
            
            int campaignID=0;
          
            Google.Adwords.com.google.adwords.CampaignStatus campStatus= new Easynet.Edge.Services.Google.Adwords.com.google.adwords.CampaignStatus();
            string campaignName = string.Empty;
           page =  CampaignService.get(selector);


           GetCampaignStatusDicFromDB();
           int count = 0;
           foreach (var item in page.entries)
           {      
              
               campStatus = item.status;
               if(item.id == null)
                  count++;
               else
                campaignID = Convert.ToInt32(item.id);
               campaignName = item.name;        
            
               UpdateCampaignStatusInDB(_accountID, 1, campaignName, Convert.ToInt32(campaignStatusHashSet[campStatus.ToString()]), campaignID);

           }      
            return ServiceOutcome.Success;
            
        }


        private string FetchAutoToken(string useragent)
        {
            string loginUrl = @"https://www.google.com/accounts/ClientLogin";

            // Initalize HttpWebRequest
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(loginUrl);
            request.UserAgent = useragent;
            request.Timeout = 12333;
            request.ContentType = "text/plain";

            // Initalize post Data
            request.ContentType = "application/x-www-form-urlencoded ";
            request.Method = "POST";
            ASCIIEncoding encoding = new ASCIIEncoding();
            string postData = "accountType=GOOGLE&Email=ppc.easynet@gmail.com" +
               // _accountEmail + 
                "&Passwd=" + Encryptor.Decrypt(GetConfigurationOptionsField("Password"), GetConfigurationOptionsField("Password")) 
                + "&source=curl-tester-1.0&service=adwords";
            byte[] data = encoding.GetBytes(postData);

            // Prepare web request...
            request.ContentLength = data.Length;
            System.IO.Stream newStream = request.GetRequestStream();
            // Send the data.
            newStream.Write(data, 0, data.Length);
            newStream.Close();

            // Fetch auth token from HttpWebResponse.  
            string fileName = String.Empty;
            string contents;
            int authIndex;
            using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
            {
                // Initalize urlStream.
                System.IO.Stream urlStream = response.GetResponseStream();
                urlStream.ReadTimeout = 30000;

                System.IO.StreamReader reader = new System.IO.StreamReader(urlStream);
                contents = reader.ReadToEnd();
                authIndex = contents.IndexOf("Auth=");
            }

            return contents.Substring(authIndex + 5);
        }

        protected override bool InitalizeServiceData()
        {
            if (!base.InitalizeServiceData())
                return false;
            _accountData = new AccountData(UserAgent,
                GetConfigurationOptionsField("User"), Encryptor.Decrypt(GetConfigurationOptionsField("Password"), GetConfigurationOptionsField("Password")),
                _accountEmail, GetConfigurationOptionsField("DeveloperToken"), GetConfigurationOptionsField("ApplicationToken"));
            return true;
        }

        /// <summary>
        /// Search account settings by account id and retrun 
        /// the access data to this account.
        /// </summary>
        /// <returns>account setings</returns>
        private void GetAccountAccessData()
        {
            //adWordsAccess = new AdWordsAccess(string.Empty, string.Empty, string.Empty);
            SqlDataReader selectResults;

            using (DataManager.Current.OpenConnection())
            {
                // Initalize select command to table User_GUI_SettingsAdwords to fetch 
                // account access data by accountID.
                SqlCommand selectCommand = DataManager.CreateCommand(@"
                    select Email
                        from User_GUI_SettingsAdwords 
                        where Account_ID = @accountID:Int");

                // Init insertCommand with the data manger connection 
                DataManager.Current.AssociateCommands(selectCommand);

               
                // Initalize select command parameters.
                selectCommand.Parameters["@accountID"].Value = _accountID; ;

                try
                {
                    // Execute select command.
                    selectResults = selectCommand.ExecuteReader();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error get account email from DB.", ex);
                }

                // Check if we got data from the DB.
                if (!selectResults.HasRows)
                    throw new Exception("There is no row with google account email in the DB.");


                // Get the account access data from the DB.
                selectResults.Read();

                try
                {
                    _accountEmail = selectResults["Email"].ToString();
                }
                catch (Exception ex)
                {
                    throw new Exception("The field 'Email' don't exist in the row. Can't get account email from DB.", ex);
                }
            }
        }


    }
}
