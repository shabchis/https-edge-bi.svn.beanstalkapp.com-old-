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

     
    


    class AdGroupStatus : BaseRetriever
    {
        private string _accountEmail = string.Empty;
        AccountData _accountData = new AccountData();
        private const string UserAgent = "Easynet(seperia) -- C# get clients information";
        int _counter = 0;
         
        
        System.Collections.Hashtable adGroupStatusHashSet = new System.Collections.Hashtable();


        /// <summary>
        /// get the status dictionary data ( ID for name) from DB
        /// </summary>
        private void GetadGroupStatusDicFromDB()
        {
            try
            {
             
                {
                    string queryString = @"Select * from Constant_AdGroupStatus";

                    SqlDataAdapter adapter = new SqlDataAdapter(queryString, new SqlConnection(DataManager.ConnectionString));

                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    foreach (DataRow row in dataTable.Rows)
                    {
                        adGroupStatusHashSet.Add(row[0].ToString().ToUpper(), row[1]);
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void UpdateAdGroupStatusInDB(int accountId,int channelId,int campaignGK,string adGroupName, int status,int adGroupID)
        {            
            using (DataManager.Current.OpenConnection())
            {
                try
                {

                    SqlCommand SPCmd = DataManager.CreateCommand(@"GKManager_SetAdGroupStatus(@Account_ID:Int,@Campaign_GK:int@Channel_ID:Int,@agStatus:Int,@adgroup:NVarChar,@adgroupID:Int)", 
                              CommandType.StoredProcedure);


                    SPCmd.CommandTimeout = 120;
                    SPCmd.Parameters["@Account_ID"].Value = accountId;
                    SPCmd.Parameters["@Channel_ID"].Value = channelId;
                    SPCmd.Parameters["@Campaign_GK"].Value = campaignGK;
                    SPCmd.Parameters["@agStatus"].Value = status;
                    SPCmd.Parameters["@adgroup"].Value = adGroupName;                    
                    SPCmd.Parameters["@adgroupID"].Value = adGroupID;
 
                    object result = SPCmd.ExecuteScalar();

                    try
                    {

                        if (result != null)
                        {
                            int _adGroupID = Convert.ToInt32(result);
                            _counter++;
                        }


                    }
                    catch (Exception e)
                    {
                        string queryString = @"insert into tempAdgroupStatusTable     (accountId,status,adGroupID,adGroupName,
                                            campaignGK,campaignID)
								Values (@accountId:int,@status:int,@adGroupID:int,@adGroupName:nvarchar
                                        ,@campaignGK:int,@campaignID:int)";

                        SqlCommand ErrorCommand = DataManager.CreateCommand( queryString,CommandType.Text);


                        ErrorCommand.CommandTimeout = 120;
                        ErrorCommand.Parameters["@accountId"].Value = accountId;
                        ErrorCommand.Parameters["@status"].Value = status;
                        ErrorCommand.Parameters["@adGroupID"].Value = adGroupID ;
                        ErrorCommand.Parameters["@adGroupName"].Value =adGroupName ;
                        ErrorCommand.Parameters["@campaignGK"].Value =campaignGK  ;
                        ErrorCommand.Parameters["@campaignID"].Value = adGroupID;
                      
                        result = ErrorCommand.ExecuteScalar();


                        try
                        {
                            // Execute command.
                            ErrorCommand.ExecuteNonQuery();
                           
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Error saving retrieved file path to the database.", ex);
                        }






                        Console.WriteLine("failed to upadte in db: adGroup id=" + adGroupID.ToString() + " , status=" + status.ToString());
                    }
                     
                }
                catch (Exception ex)
                {
                    Log.Write("Failed to write keepalive time to DB.", ex);
                }
            } 
        }



        private void InitializeRequestHeader(ref AdGroupWS.AdGroupService adGroupService)
        {
           
            adGroupService.RequestHeader.userAgent = _accountData.UserAgent;
            adGroupService.RequestHeader.applicationToken = _accountData.AppToken;
            adGroupService.RequestHeader.developerToken = _accountData.Token;

            adGroupService.RequestHeader.authToken = FetchAutoToken(_accountData.UserAgent);
            adGroupService.RequestHeader.developerToken = _accountData.Token;
            adGroupService.RequestHeader.applicationToken = _accountData.AppToken;
            GetAccountAccessData();
            _accountData.ClientEmail = _accountEmail;
            adGroupService.RequestHeader.clientEmail = _accountEmail;

        }


        /// <summary>
        /// test for ad groupd ad status
        /// </summary>
        /// <returns></returns>
        protected  ServiceOutcome testtttt()
        {
            AdGroupAdService.AdGroupAdSelector selector = new Easynet.Edge.Services.Google.Adwords.AdGroupAdService.AdGroupAdSelector();

            InitalizeServiceData();

            AdGroupAdService.AdGroupAdService AGAService = new Easynet.Edge.Services.Google.Adwords.AdGroupAdService.AdGroupAdService();

            AGAService.RequestHeader = new AdGroupAdService.MySoapHeader();
 


            AGAService.RequestHeader.userAgent = _accountData.UserAgent;
            AGAService.RequestHeader.applicationToken = _accountData.AppToken;
            AGAService.RequestHeader.developerToken = _accountData.Token;
            AGAService.RequestHeader.authToken = FetchAutoToken(_accountData.UserAgent);
            AGAService.RequestHeader.developerToken = _accountData.Token;
            AGAService.RequestHeader.applicationToken = _accountData.AppToken;
            GetAccountAccessData();
            _accountData.ClientEmail = _accountEmail;
            AGAService.RequestHeader.clientEmail = _accountEmail;


            AdGroupAdService.AdGroupAdPage page = new AdGroupAdService.AdGroupAdPage();

           

            //int adGroupID = 0;

            AdGroupAdService.AdGroupAdStatus status = new Easynet.Edge.Services.Google.Adwords.AdGroupAdService.AdGroupAdStatus();
            string adGroupName = string.Empty;


            page = AGAService.get(selector);

           // page.entries[0].status
            return ServiceOutcome.Success;
        }

        
        protected override ServiceOutcome DoWork()
        {
            
           

           
            AdGroupWS.AdGroupSelector selector = new AdGroupWS.AdGroupSelector();
            
            InitalizeServiceData();
            AdGroupWS.AdGroupService adGroupService = new AdGroupWS.AdGroupService();   
            adGroupService.RequestHeader = new AdGroupWS.MySoapHeader();

            InitializeRequestHeader(ref adGroupService);
            
             
            AdGroupWS.AdGroupPage page = new AdGroupWS.AdGroupPage();
            
            int adGroupID = 0;
           
            AdGroupWS.AdGroupStatus campStatus = new AdGroupWS.AdGroupStatus();
            string adGroupName = string.Empty;

            
            page =  adGroupService.get(selector);


           GetadGroupStatusDicFromDB();
           int count = 0;


           //run on every ad group and insert into DB
           foreach (var item in page.entries)
           {
             
               campStatus = item.status;
               if (item.id == null)
                   count++;
               else
                   adGroupID = Convert.ToInt32(item.id);
               adGroupName = item.name;
            //   if (adGroupName == "Placements")
             //      count = 2000000000;
               long campaignGK = Easynet.Edge.BusinessObjects.GkManager.GetCampaignGK(_accountID, 1, item.campaignName, item.campaignId);
            
               UpdateAdGroupStatusInDB(_accountID, 1, Convert.ToInt32(campaignGK), adGroupName, Convert.ToInt32(adGroupStatusHashSet[campStatus.ToString()]), adGroupID);

           }
           Console.WriteLine("countr: " + _counter.ToString());
           return ServiceOutcome.Success;            
        }

        /// <summary>
        /// call google to get AutoToken
        /// </summary>
        /// <param name="useragent"></param>
        /// <returns></returns>
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

