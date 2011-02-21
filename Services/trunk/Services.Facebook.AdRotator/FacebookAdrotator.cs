using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Data.Proxy;
using Easynet.Edge.Core.Workflow;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using myFacebook = Facebook;
using Easynet.Edge.Services.DataRetrieval.Retriever;




namespace Easynet.Edge.Services.Facebook.Adrotator
{
    public class FacebookAdrotator : BaseRetriever 
    {
        public enum adgroup_status
        {
            ACTIVE = 1,
            DELETED = 3,
            CAMPAIGN_PAUSED = 8 
        }
        protected myFacebook.Session.ConnectSession connSession;
        protected string _targetDirectory;
        protected System.IO.StreamWriter wrtTxtFile;
        protected string _ApplicationID = string.Empty;
        protected string _FBaccountID = string.Empty;
        protected string _apiKey = string.Empty;
        protected string _ap_secret = string.Empty;
        protected string _session = string.Empty;
        protected string _sessionSecret = string.Empty;
        protected string _UserId = string.Empty;
        protected string _accoutnName = string.Empty;
        protected string _createExcelDirectoryPath = string.Empty;
        protected string _pipe = string.Empty;

        ArrayList _ExpiredList;
        ArrayList _SelectedCampainsList;
        Hashtable _UpdateList;
        ArrayList _ListOfActiveCampaigns;
        private Dictionary<string, string> parameterList;
        private myFacebook.Rest.Api _facebookAPI;

        
        protected override ServiceOutcome DoWork()
        {
            try
            {
                InitFacebook();
                LoadAdExpired();
                LoadActiveCampainsList();
                if (Convert.ToBoolean(Instance.Configuration.Options["UseSelectedCampains"].ToString()))
                    LoadSelectedCampainsList();
                switch (Instance.Configuration.Options["RuningMode"].ToString())
                {
                    case "FULL":
                        GetFacebookData();
                        UpdateAdExpired();
                        break;
                    case "AD_PRIORITY":
                        GetFacebookData();
                        break;
                    case "AD_EXPIRED":
                        UpdateAdExpired();
                        break;
                    default:
                        break;
                }
                return ServiceOutcome.Success;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        

        private void LoadActiveCampainsList()
        {
            parameterList.Clear();
            parameterList.Add("include_deleted", "false");
            parameterList.Add("campaign_ids", "");
            parameterList.Add("account_id", _FBaccountID);
            parameterList.Add("method", "facebook.Ads.getCampaigns");
            string getCampaignsRes = _facebookAPI.Application.SendRequest(parameterList);
            System.Xml.XmlDocument xmlCampaing = new System.Xml.XmlDocument();
            xmlCampaing.LoadXml(getCampaignsRes);

            int xmlCampaingCount = xmlCampaing.ChildNodes[1].ChildNodes.Count;
            _ListOfActiveCampaigns = new ArrayList();
            //List<Dictionary<string, System.Xml.XmlNode>> ListOfCampaigns = new List<Dictionary<string, System.Xml.XmlNode>>();
            for (int i = 0; i < xmlCampaingCount; i++)
            {
                //Dictionary<string, System.Xml.XmlNode> newItem = new Dictionary<string, System.Xml.XmlNode>();
                if(xmlCampaing.ChildNodes[1].ChildNodes[i].ChildNodes[7].InnerText == "1")

                //newItem.Add(xmlCampaing.ChildNodes[1].ChildNodes[i].ChildNodes[1].InnerText, xmlCampaing.ChildNodes[1].ChildNodes[i]);
                    _ListOfActiveCampaigns.Add(xmlCampaing.ChildNodes[1].ChildNodes[i].ChildNodes[1].InnerText);
            }
        }
        
        private void LoadSelectedCampainsList()
        {
            _SelectedCampainsList = new ArrayList();
            StreamReader re = File.OpenText("SelectedCampains.edg");
            string input = null;
            _SelectedCampainsList = new ArrayList();
            while ((input = re.ReadLine()) != null)
            {
                _SelectedCampainsList.Add(input);
            }
            re.Close();
        }
        private void LoadAdExpired()
        {
            _ExpiredList = new ArrayList();
            StreamReader re = File.OpenText("AD_Expired_List.edg");
            string input = null;
            char[] delimiterChars = { ',' };
            _UpdateList = new Hashtable();
            while ((input = re.ReadLine()) != null)
            {
                string[] arr = input.Split(delimiterChars);
                DateTime ExpiredDate = Convert.ToDateTime(arr[1].ToString());
                if (ExpiredDate.CompareTo(System.DateTime.Today) == 0)
                {
                    _ExpiredList.Add(arr[0]);
                }

            }
            re.Close();
        }
        private void UpdateAdExpired()
        {
            StreamReader re = File.OpenText("AD_Expired_List.edg");
            string input = null;
            char[] delimiterChars = { ',' };
            _UpdateList = new Hashtable();
            while ((input = re.ReadLine()) != null)
            {
                string[] arr = input.Split(delimiterChars);
                DateTime ExpiredDate = Convert.ToDateTime(arr[1].ToString());
                if (ExpiredDate.CompareTo(System.DateTime.Today) == 0)
                {
                    ArrayList arrList = new ArrayList();
                    arrList.Add(arr[0]);//ad_id
                    arrList.Add("ad_status"); //Parameter
                    arrList.Add("9"); //Value
                    arrList.Add(-1);
                    _UpdateList.Add(arr[0], arrList);
                }

            }
            re.Close();
            if (_UpdateList.Count > 0)
                UpdateFacebook();
        }
        
        private void InitFacebook()
        {

            parameterList = new Dictionary<string, string>();
    
            try
            {
             
                if (Instance.Configuration.Options["APIKey"] == null)
                    _apiKey = Instance.ParentInstance.Configuration.Options["APIKey"].ToString();
                else
                    _apiKey = Instance.Configuration.Options["APIKey"].ToString();

                 
                if (Instance.Configuration.Options["sessionKey"] == null)
                    _session = Instance.ParentInstance.Configuration.Options["sessionKey"].ToString();
                else
                    _session = Instance.Configuration.Options["sessionKey"].ToString();
           
                if (Instance.Configuration.Options["applicationSecret"] == null)
                    _ap_secret = Instance.ParentInstance.Configuration.Options["applicationSecret"].ToString();
                else
                    _ap_secret = Instance.Configuration.Options["applicationSecret"].ToString();

                if (Instance.Configuration.Options["FBaccountID"] == null)
                    _FBaccountID = Instance.ParentInstance.Configuration.Options["FBaccountID"].ToString();
                else
                    _FBaccountID = Instance.Configuration.Options["FBaccountID"].ToString();
               
                if (Instance.Configuration.Options["applicationID"] == null)
                    _ApplicationID = Instance.ParentInstance.Configuration.Options["applicationID"].ToString();
                else
                    _ApplicationID = Instance.Configuration.Options["applicationID"].ToString();

              

                if (Instance.Configuration.Options["accountName"] == null)
                    _accoutnName = Instance.ParentInstance.Configuration.Options["accountName"].ToString();
                else
                    _accoutnName = Instance.Configuration.Options["accountName"].ToString();

                if (Instance.Configuration.Options["sessionSecret"] == null)
                    _sessionSecret = Instance.ParentInstance.Configuration.Options["sessionSecret"].ToString();
                else
                    _sessionSecret = Instance.Configuration.Options["sessionSecret"].ToString();

               
                if (Instance.Configuration.Options["serviceType"] == null)
                    serviceType = Instance.ParentInstance.Configuration.Options["serviceType"].ToString();
                else
                    serviceType = Instance.Configuration.Options["serviceType"].ToString();

                connSession = new myFacebook.Session.ConnectSession(_apiKey, _ap_secret);
                connSession.SessionKey = _session;
                connSession.SessionSecret = _sessionSecret;


                _facebookAPI = new myFacebook.Rest.Api(connSession);

                _facebookAPI.Auth.Session.SessionExpires = false;
           
              
            }
            catch (Exception ex)
            {
                Edge.Core.Utilities.Log.Write("Error in FacebookBaseRetriever.InitConfigurtaionData(): " + ex.Message, Edge.Core.Utilities.LogMessageType.Error);

            }

        }

        private void GetFacebookData()
        {
            _UpdateList = new Hashtable();
            switch (Instance.Configuration.Options["ImportDataFrom"].ToString())
            {
                case "AdFileAction":
                    GetFacebookFileData();
                    break;
                case "CampaingnSimpleRotate":
                    GetDataFromFacebook();
                    break;
                case "CampaignStatRotate":
                    GetFacebookDBData();
                    break;
                default:
                    break;
            }
            if (_UpdateList.Count > 0)
                UpdateFacebook();
        }

        private void GetFacebookFileData()
        {
            StreamReader re = File.OpenText("FacebookData.edg");
            string input = null;
            char[] delimiterChars = {','};
            while ((input = re.ReadLine()) != null)
            {
                ArrayList arrList = new ArrayList();
                string[] arr = input.Split(delimiterChars);
                //arrList.Add(arr[0]);//campaign_id
                arrList.Add(arr[0]);//ad_id
                arrList.Add(arr[1]); //Parameter
                arrList.Add(arr[2]); //Value
                arrList.Add(-1);
                _UpdateList.Add(arr[0],arrList);
            }
            re.Close();
        }

        private string GetAdIds(Hashtable AdIDList)
        {
            string AdIds = "";
            foreach (Int64 AdId in AdIDList.Keys)
            {
                AdIds += AdId.ToString() + ",";
            }
            return AdIds.Substring(0, AdIds.Length - 1);
        }

        private string GetImgByAdId(Int64 AdId, Hashtable AdIDList)
        {
            string[] arr = (string[])AdIDList[AdId];
            return arr[1];
        }

        private void GetAdGroup(Hashtable AdIDList, out Dictionary<string, ArrayList > AdIDSortedList)
        {
            Dictionary<string, ArrayList> lAdIDSortedList;
            lAdIDSortedList = new Dictionary<string, ArrayList>();
            AdIDSortedList = new Dictionary<string, ArrayList>(); 

            using (DataManager.Current.OpenConnection())
            {
                SqlCommand searchEngineCmd = DataManager.CreateCommand(@"Get_AdGroupByAdId(@AccountID:Int,@AdIds:nvarchar(1000),@FromDate:Int,@ToDate:Int)", CommandType.StoredProcedure);
                searchEngineCmd.Parameters["@AccountID"].Value = Instance.AccountID;
                searchEngineCmd.Parameters["@AdIds"].Value = GetAdIds(AdIDList);
                string FromDate, ToDate;
                GetRangeDates(out FromDate, out ToDate);
                searchEngineCmd.Parameters["@FromDate"].Value = FromDate;
                searchEngineCmd.Parameters["@ToDate"].Value = ToDate;
                searchEngineCmd.CommandTimeout = 1200;
                using (SqlDataReader reader = searchEngineCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ArrayList AdGroupArray = new ArrayList();
                        if (lAdIDSortedList.ContainsKey(reader["adgroup_gk"].ToString()))
                        {
                            AdGroupArray = lAdIDSortedList[reader["adgroup_gk"].ToString()];
                            lAdIDSortedList.Remove(reader["adgroup_gk"].ToString());
                        }
                        string value = null;
                        if (Instance.Configuration.Options["AdSortMode"].ToString() == "ID")
                            value = reader["creativeid"].ToString();
                        else if (Instance.Configuration.Options["AdSortMode"].ToString() == "IMG")
                            value = GetImgByAdId((Int64)reader["creativeid"], AdIDList) + "@" + reader["creativeid"].ToString();
                        AdGroupArray.Add(value);
                        lAdIDSortedList.Add(reader["adgroup_gk"].ToString(), AdGroupArray);
                    }
                    foreach (string AdGroupId in lAdIDSortedList.Keys)
                    {
                        ArrayList AdGroup = lAdIDSortedList[AdGroupId];
                        AdGroup.Sort();
                        AdIDSortedList.Add(AdGroupId, AdGroup);
                    }
                }
        }
    }
        

        private void GetDataFromFacebook()
        {
            parameterList.Clear();
            parameterList.Add("include_deleted", "true");
            parameterList.Add("campaign_ids", "");
            parameterList.Add("account_id", _FBaccountID);
            foreach (string item in _SelectedCampainsList)
            {
                string data = "[{\"campaign_id\":" + item + "}]";
                parameterList.Add("adgroup_specs", data);
                parameterList.Add("method", "facebook.Ads.getAdGroups");
                string res = _facebookAPI.Application.SendRequest(parameterList);
                System.Xml.XmlDocument xmlCampaing = new System.Xml.XmlDocument();
                xmlCampaing.LoadXml(res);

                int xmlCampaingCount = xmlCampaing.ChildNodes[1].ChildNodes.Count;
                Hashtable AdIDList = new Hashtable();
                Dictionary<string, ArrayList> AdIDSortedList;
                for (int i = 0; i < xmlCampaingCount; i++)
                {
                    if (xmlCampaing.ChildNodes[1].ChildNodes[i].ChildNodes[1].InnerText == item)
                    {
                        string adid = xmlCampaing.ChildNodes[1].ChildNodes[i].ChildNodes[0].InnerText;
                        string adstatus = xmlCampaing.ChildNodes[1].ChildNodes[i].ChildNodes[3].InnerText;
                        int lpos = xmlCampaing.ChildNodes[1].ChildNodes[i].ChildNodes[2].InnerText.IndexOf("@");
                        string image = xmlCampaing.ChildNodes[1].ChildNodes[i].ChildNodes[2].InnerText;
                        image = image.Substring(lpos + 1);
                        image = image.ToLower();
                        string[] arr = {adstatus,image.Trim()} ;
                        AdIDList.Add(Convert.ToInt64(adid),arr);
                    }
                }
                GetAdGroup(AdIDList,out AdIDSortedList);
                foreach (ArrayList adGroup in AdIDSortedList.Values)
                {
                    string AdToActive, AdToStop;
                    for (int i = 1; i <= adGroup.Count; i++)
                    {
                        string AdStatus = GetAdStatus(adGroup[i-1].ToString(),AdIDList);
                        if (AdStatus == "1")
                        {
                            if (i-1 < adGroup.Count-1)
                            {
                                AdToActive = GetAdIds(adGroup[(i - 1) + 1].ToString(), AdIDList); 
                                AdToStop = GetAdIds(adGroup[i - 1].ToString(), AdIDList); 
                            }
                            else
                            {
                                AdToActive = GetAdIds(adGroup[0].ToString(), AdIDList);
                                AdToStop = GetAdIds(adGroup[i - 1].ToString(), AdIDList);
                            }
                            ArrayList arrList = new ArrayList();
                            arrList.Add(AdToActive);//ad_id
                            arrList.Add("ad_status"); //Parameter
                            arrList.Add("1"); //Value
                            arrList.Add(-1);
                            _UpdateList.Add(AdToActive, arrList);
                            ArrayList arrList2 = new ArrayList();
                            arrList2.Add(AdToStop);//ad_id
                            arrList2.Add("ad_status"); //Parameter
                            arrList2.Add("9"); //Value
                            arrList2.Add(-1);
                            _UpdateList.Add(AdToStop, arrList2); 
                        }
                    }
                }
            }
        }

        private string GetAdStatus(string ad, Hashtable AdIDList)
        {
            if (Instance.Configuration.Options["AdSortMode"].ToString() == "ID")
                return ad;
            else
            {
                int lpos = ad.IndexOf("@");
                Int64 AdId = Convert.ToInt64(ad.Substring(lpos + 1).ToString());
                string[] arr = (string[])AdIDList[AdId];
                return arr[0];
            }
        }

        private string GetAdIds(string ad, Hashtable AdIDList)
        {
            if (Instance.Configuration.Options["AdSortMode"].ToString() == "ID")
                return ad;
            else
            {
                int lpos = ad.IndexOf("@");
                Int64 AdId = Convert.ToInt64(ad.Substring(lpos + 1).ToString());
                return AdId.ToString();
            }
        }

        private ArrayList GetFirstElment(Dictionary<string, ArrayList> Dic)
        {
            foreach (ArrayList adGroup in Dic.Values)
                return adGroup;
            return null;
        }

        private void GetFacebookDBData()
        {
            using (DataManager.Current.OpenConnection())
            {
                SqlCommand searchEngineCmd = DataManager.CreateCommand(@"Get_FacebookCreative_ByDate(@AccountID:Int,@FromDate:Int,@ToDate:Int,@MinImprssions:Int)", CommandType.StoredProcedure);
                string FromDate, ToDate;
                GetRangeDates(out FromDate, out ToDate);
                searchEngineCmd.Parameters["@AccountID"].Value = Instance.AccountID;
                searchEngineCmd.Parameters["@FromDate"].Value =  FromDate;
                searchEngineCmd.Parameters["@ToDate"].Value = ToDate;
                searchEngineCmd.Parameters["@MinImprssions"].Value = Convert.ToInt32(Instance.Configuration.Options["CreativeImprssions"].ToString());
                using (SqlDataReader reader = searchEngineCmd.ExecuteReader())
                {
                    //"campaign_id" = "campaignid"
                    //"ad_id" = "Creativeid"
                    
                    bool firstRow = true;
                    Int64 adgroup_gk = 0;
                    while (reader.Read())
                    {
                        if (CheckSelectedCampain(Convert.ToInt64(reader["campaignid"])))
                        {
                            if (adgroup_gk != Convert.ToInt64(reader["adgroup_gk"]))
                                firstRow = true;
                            ArrayList arrList = new ArrayList();
                            //arrList.Add(reader["campaignid"]);//campaign_id  index[-]
                            arrList.Add(reader["creativeid"]); //ad_id index[0]
                            if (firstRow)
                            {
                                if (!CheckADExpired(Convert.ToInt64(reader["creativeid"])))
                                {
                                    arrList.Add("1");
                                    firstRow = false;
                                }
                                else
                                    arrList.Add("9");
                            }
                            else
                            {
                                if(reader["IsActiveCreative"].ToString()== "1")
                                    if (CheckADExpired( Convert.ToInt64(reader["creativeid"])))
                                        arrList.Add("9");// index[1]
                                    else
                                        arrList.Add(reader["IsActiveCreative"].ToString());// index[1]    
                                else
                                    arrList.Add(reader["IsActiveCreative"].ToString());// index[1]         
                            }
                            arrList.Add(reader["PPC_Creative_GK"].ToString());// index[2]

                            _UpdateList.Add(arrList[0], arrList);

                            
                            adgroup_gk = Convert.ToInt64(reader["adgroup_gk"]);
                        }
                        
                    }
                }
            }
        }
        private bool CheckSelectedCampain(Int64 Campainid)
        {
            if (Convert.ToBoolean(Instance.Configuration.Options["UseSelectedCampains"].ToString()))
            {
                if (_SelectedCampainsList.Contains(Campainid.ToString()))
                    return CheckActiveCampain(Campainid);
                else
                    return false;
            }
            return CheckActiveCampain(Campainid);
        }

        private bool CheckActiveCampain(Int64 Campainid)
        {
            if (_ListOfActiveCampaigns.Contains(Campainid.ToString()))
                return true;
            else
                return false;
        }

        private bool CheckADExpired(Int64 creativeid)
        {
            if(_ExpiredList.Contains(creativeid.ToString()))
                return true; 
            else
                return false;
        }

        private void GetRangeDates(out string  FromDate, out string  ToDate)
        {
            TimeSpan ts = new TimeSpan(Convert.ToInt32(Instance.Configuration.Options["RetrievalDays"].ToString()), 0, 0, 0, 0);
            DateTime  lFromDate = System.DateTime.Now.Subtract(ts);
            DateTime  lToDate =  System.DateTime.Now;
            string lDay, lMonth;
            lDay = lFromDate.Day < 10 ? "0" + lFromDate.Day.ToString() : lFromDate.Day.ToString();
            lMonth = lFromDate.Month < 10 ? "0" + lFromDate.Month.ToString() : lFromDate.Month.ToString();
            FromDate = lFromDate.Year.ToString() + lMonth + lDay;
            lDay = lToDate.Day < 10 ? "0" + lToDate.Day.ToString() : lToDate.Day.ToString();
            lMonth = lToDate.Month < 10 ? "0" + lToDate.Month.ToString() : lToDate.Month.ToString();
            ToDate = lToDate.Year.ToString() + lMonth + lDay;
            
        }
        
        private void UpdateFacebook()
        {
            string res = null;
            if (Instance.Configuration.Options["UpdateMode"].ToString() == "BATCH")
            {
                string data = null;
                foreach (ArrayList item in _UpdateList.Values)
                {
                    //data += "{\"adgroup_id\":" + item[0].ToString() + ",\"" + item[1].ToString() + "\":" + item[2].ToString() + "},";
                    //      [{\"adgroup_id\":6002448053919,\"ad_status\":1}]
                    data += "{\"adgroup_id\":" + item[0].ToString() + ",\"ad_status\":" + item[2].ToString() + "},";
                }
                if (data != null)
                {
                    data = data.Substring(0, data.Length - 1);
                    data = "[" + data + "]";
                    res = UpdateFacebook(null, null, data);
                }
                if(res != "true")
                    throw new Exception(res);   
                foreach (ArrayList item in _UpdateList.Values)
                {
                    SetFacebookLog(item[0].ToString(), item[2].ToString(), (int)item[3], item[1].ToString());
                }
            }
                   
            else if (Instance.Configuration.Options["UpdateMode"].ToString() == "ONE_BY_ONE")
            {
                foreach (ArrayList item in _UpdateList.Values)
                {
                    res = UpdateFacebook(item[0].ToString(), item[1].ToString(),null);
                    SetFacebookLog(item[0].ToString(), item[1].ToString(), (int)item[2], "ad_status");
                }
            }
        }

        private string UpdateFacebook(string Ad_ID, string CampaignStatus,string data)
        {
            parameterList.Clear();
            //parameterList.Add("adgroup_id", Ad_ID);
            parameterList.Add("account_id", _FBaccountID);
            //parameterList.Add("campaign_ids", CampaignID);
            parameterList.Add("method", "facebook.Ads.updateAdGroups");
            
            if(data == null)
                data = "[{\"adgroup_id\":" + Ad_ID + ",\"ad_status\":" + CampaignStatus + "}]";
            parameterList.Add("adgroup_specs", data);
            string res = _facebookAPI.Application.SendRequest(parameterList);
            System.Xml.XmlDocument retXml = new System.Xml.XmlDocument();
            retXml.LoadXml(res);
            return retXml.ChildNodes[1].ChildNodes[1].Attributes[0].Value;
        }

        private void SetFacebookLog(string Ad_ID, string NewValue, Int64 Creativegk, string Parameter)
        {
            using (DataManager.Current.OpenConnection())
            {
                SqlCommand searchEngineCmd = DataManager.CreateCommand(@"Set_FacebookUpdateCreative_Log(@AccountID:Int,@Ad_ID:bigint,@RuningDate:Int,@Parameter:nvarchar(50),@NewValue:nvarchar(50),@Creativegk:bigint", CommandType.StoredProcedure);
                string RuningDate, tmp;
                GetRangeDates(out tmp, out RuningDate);
                searchEngineCmd.Parameters["@AccountID"].Value = Instance.AccountID;
                searchEngineCmd.Parameters["@Ad_ID"].Value = Convert.ToInt64(Ad_ID);
                searchEngineCmd.Parameters["@RuningDate"].Value = Convert.ToInt32(RuningDate);
                searchEngineCmd.Parameters["@Parameter"].Value = Parameter;
                searchEngineCmd.Parameters["@NewValue"].Value = NewValue;
                searchEngineCmd.Parameters["@Creativegk"].Value = Creativegk;
                searchEngineCmd.ExecuteNonQuery();
            }
        }
    }
}
