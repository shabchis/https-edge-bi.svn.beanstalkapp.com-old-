using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Services.DataRetrieval.Retriever;
using Easynet.Edge.Core.Services;
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;
using System.Data;
using Easynet.Edge.Core.Utilities;
using Excel = Microsoft.Office.Interop.Excel;

namespace Easynet.Edge.Services.StatusManager
{
    class BaseStatusService : BaseRetriever
    {

        protected StringBuilder errorSTR = new StringBuilder("ErrorBuilder");
        protected int MaxRes = 400;//500;
        protected bool needToRunResults = true;
        protected string _updateStatusSqlCommand =  string.Empty;
        protected string _sp_SelecConsts = string.Empty;
        protected Excel.Workbook xlWorkBook;
        protected string updateStatusSqlCommand;
        protected CommandType commandType;
        protected Dictionary<string, string> sqlParamsDictionary = new Dictionary<string,string>();
        protected string _accountEmail = string.Empty;
        protected AccountData _accountData = new AccountData();
        protected const string UserAgent = "Easynet(seperia) -- C# get clients information";
         

        protected System.Collections.Hashtable StatusHashSet = new System.Collections.Hashtable(); //holds the status id from DB

        protected override ServiceOutcome DoWork()
        {
            try
            {
                if (false == InitObject())
                {
                    Core.Utilities.Log.Write("error in DoWork() InitObject(): ", Core.Utilities.LogMessageType.Error);
                    return ServiceOutcome.Failure;
                }
                GetStatusDicionaryFromDB(_sp_SelecConsts);
                 
                if (false == InitRequestHeader())
                {
                    Core.Utilities.Log.Write("error in DoWork() InitRequestHeader: ", Core.Utilities.LogMessageType.Error);
                    return ServiceOutcome.Failure;
                }

                if (false == RunRequest())
                {
                    Core.Utilities.Log.Write("error in DoWork()  RunRequest(): ", Core.Utilities.LogMessageType.Error);
                    return ServiceOutcome.Failure;
                }

                if (true == needToRunResults)
                {
                    if (false == RunOnResults())
                    {
                        Core.Utilities.Log.Write("error in DoWork() RunOnResults(): ", Core.Utilities.LogMessageType.Error);
                        return ServiceOutcome.Failure;
                    }
                }
                return ServiceOutcome.Success;
            }
            catch (Exception ex)
            {
                Core.Utilities.Log.Write("error in DoWork() DoWork(): "+ex.Message, Core.Utilities.LogMessageType.Error);
                return ServiceOutcome.Failure;
            }
        }

        protected virtual void UpdateStatusValuesInDB(int accountId, int channelId, string campaignName, int status, int campaignID)
        {

        }

        protected virtual void BuildSqlParamsDictionray()
        {

        }

        protected virtual bool InitObject()
        {
            return true;
        }

        protected virtual bool RunOnResults()
        {
            return true;
        }


        protected bool SaveExcelFile(Microsoft.Office.Interop.Excel.Workbook xlWorkBook, string path)
        {
            try
            {

                object misValue = System.Reflection.Missing.Value;

                
                xlWorkBook.SaveAs(path, Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
                xlWorkBook.Close(true, misValue, misValue);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        protected Excel.Workbook CreatExcelFile()
        {
           // try
            {
                Excel.Application xlApp;
               
                
                object misValue = System.Reflection.Missing.Value;

               
               //  xlApp = new Excel.Application();
                 xlApp = new Excel.ApplicationClass();
                Excel.Workbooks workBooks = xlApp.Workbooks;

            //   workBooks.GetType().InvokeMember("Add", System.Reflection.BindingFlags.InvokeMethod,null, workBooks, null, ci);

              //  Excel.Workbook c = xlApp.workboo
               xlWorkBook = xlApp.Workbooks.Add(misValue);

                
             //  xlWorkBook = workBooks[1];
                return xlWorkBook; 

            }
         //   catch (Exception ex)
            {
                return null;

            }



        }



     //   protected virtual void UpdateStatusValuesInDB(Dictionary<string, string> sqlParamsDictionary, string sqlCommand, CommandType commandType)
        protected virtual void UpdateStatusValuesInDB()
        { 
            using (DataManager.Current.OpenConnection())
            {
                string toPrint="";
                try
                {
                    //SqlCommand SPCmd = DataManager.CreateCommand(@"GkManager_SetCampaignStatus
                    SqlCommand SPCmd = DataManager.CreateCommand((_updateStatusSqlCommand), commandType);
                  //  DataManager.ConnectionString = @"Data Source=RnD;Initial Catalog=easynet_Oltp;Integrated Security=True";
                    SPCmd.CommandTimeout = 120;

                    foreach (var item in sqlParamsDictionary)
                    {
                        toPrint = toPrint + item.Key + ": " + item.Value;
                         SPCmd.Parameters[item.Key].Value = item.Value;
                    }
                
                   
                    object result = SPCmd.ExecuteScalar();

                    try
                    {
                        if (result != null)
                        {
                            int _campaignID = Convert.ToInt32(result);
                           
                            //Console.WriteLine("gk: " + _campaignID +" ,   " +SPCmd.Parameters[8].Value.ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        errorSTR.Append("failed to upadte in db: " + toPrint + "  ,  ");
                      //  Console.WriteLine("failed to upadte in db. " + toPrint);
                    }
                    

                }
                catch (Exception ex)
                { 
                    Log.Write("Failed to write keepalive time to DB.", ex);
                }
             
            } 
        }


        protected virtual void GetStatusDicionaryFromDB(string sp)
        {
            try
            {

                string queryString = sp;

                SqlDataAdapter adapter = new SqlDataAdapter(queryString, new SqlConnection(DataManager.ConnectionString));

                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                foreach (DataRow row in dataTable.Rows)
                {
                    StatusHashSet.Add(row[0].ToString().ToUpper(), row[1]);
                }
            }

            catch (Exception ex) {
                Core.Utilities.Log.Write("error in GetStatusDicionaryFromDB(): " + ex.Message, Core.Utilities.LogMessageType.Error);
                   }
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
        protected void GetAccountAccessData()
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
                    Core.Utilities.Log.Write("Error get account email from DB. " + ex.Message, Core.Utilities.LogMessageType.Error);
             
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
                    Core.Utilities.Log.Write("The field 'Email' don't exist in the row. Can't get account email from DB. " + ex.Message, Core.Utilities.LogMessageType.Error);
             
                    throw new Exception("The field 'Email' don't exist in the row. Can't get account email from DB.", ex);
                }
            }
        }   

        protected virtual bool InitRequestHeader()
        {
            return true;
        }


        protected virtual bool RunRequest()
        {
            return true;
        }


        protected string FetchAutoToken(string useragent)
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
                "&Passwd=" + Encryptor.Decrypt(GetConfigurationOptionsField("Password"),GetConfigurationOptionsField("Password"))
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
    }
}
