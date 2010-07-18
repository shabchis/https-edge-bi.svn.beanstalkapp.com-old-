using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using System.ServiceModel;
using myFacebook = Facebook ;
using Easynet.Edge.Services.DataRetrieval.Retriever;
using Excel =  Microsoft.Office.Interop.Excel; 

namespace Easynet.Edge.Services.Facebook
{
    public class FacebookRetriever : FacebookBaseRetriever
    {
       
        protected List<FacebookRow> listOfFaceBookRows;
        protected List<FacebookRow> listOfRows = new List<FacebookRow>();
        protected List<AdGroupClass> listOfAdGroup;
        public List<CampaignClass> campaignList;
      

    //    private List<FacebookRow> listOfFaceBookRows;
    //    private List<FacebookRow> listOfRows = new List<FacebookRow>();
    //    private string targetDirectory;
    //    private System.IO.StreamWriter wrtTxtFile;
    //    private string _ApplicationID = string.Empty;
    //    private string _FBaccountID = string.Empty;
    //    private string _apiKey = string.Empty;
    //    private string _ap_secret = string.Empty;
    //    private string _session = string.Empty;
    //    private string _sessionSecret = string.Empty;
    //    private string _UserId = string.Empty;
    //    private string _accoutnName = string.Empty;
    //    private string _createExcelDirectoryPath = string.Empty;
    //    private List<AdGroupClass> listOfAdGroup;
    //    private List<CampaignClass> campaignList;
    //    private myFacebook.Session.ConnectSession connSession;
 
    //    protected override void InitalzieReportData()
    //    {


    //        try
    //        {
              
    //            rawDataFields = (Easynet.Edge.Services.DataRetrieval.Configuration.FieldElementSection)System.Configuration.ConfigurationManager.GetSection
    //         (GetConfigurationOptionsField("FieldsMapping"));
               
    //            if (Instance.Configuration.Options["APIKey"] == null)
    //                _apiKey = Instance.ParentInstance.Configuration.Options["APIKey"].ToString();
    //            else
    //                _apiKey = Instance.Configuration.Options["APIKey"].ToString();

                 
    //            if (Instance.Configuration.Options["sessionKey"] == null)
    //                _session = Instance.ParentInstance.Configuration.Options["sessionKey"].ToString();
    //            else
    //                _session = Instance.Configuration.Options["sessionKey"].ToString();
           
    //            //ffff
    //            if (Instance.Configuration.Options["applicationSecret"] == null)
    //                _ap_secret = Instance.ParentInstance.Configuration.Options["applicationSecret"].ToString();
    //            else
    //                _ap_secret = Instance.Configuration.Options["applicationSecret"].ToString();

 
    //            if (Instance.Configuration.Options["FBaccountID"] == null)
    //                _FBaccountID = Instance.ParentInstance.Configuration.Options["FBaccountID"].ToString();
    //            else
    //                _FBaccountID = Instance.Configuration.Options["FBaccountID"].ToString();
               
               
    //            //if (Instance.Configuration.Options["userId"] == null)
    //            //    _UserId = Instance.ParentInstance.Configuration.Options["userId"].ToString();
    //            //else
    //            //    _UserId = Instance.Configuration.Options["userId"].ToString();

               

    //            if (Instance.Configuration.Options["applicationID"] == null)
    //                _ApplicationID = Instance.ParentInstance.Configuration.Options["applicationID"].ToString();
    //            else
    //                _ApplicationID = Instance.Configuration.Options["applicationID"].ToString();

              

    //            if (Instance.Configuration.Options["accountName"] == null)
    //                _accoutnName = Instance.ParentInstance.Configuration.Options["accountName"].ToString();
    //            else
    //                _accoutnName = Instance.Configuration.Options["accountName"].ToString();
    ////if (Instance.Configuration.Options["CreateExcelDirectoryPath"] == null)
    //            //    _createExcelDirectoryPath = Instance.ParentInstance.Configuration.Options["CreateExcelDirectoryPath"].ToString();
    //            //else
    //            //    _createExcelDirectoryPath = Instance.Configuration.Options["CreateExcelDirectoryPath"].ToString();

    //            if (Instance.Configuration.Options["TargetDirectory"] == null)
    //                _sessionSecret = Instance.ParentInstance.Configuration.Options["TargetDirectory"].ToString();
    //            else
    //                _sessionSecret = Instance.Configuration.Options["TargetDirectory"].ToString();

                 

    //            if (Instance.Configuration.Options["sessionSecret"] == null)
    //                _sessionSecret = Instance.ParentInstance.Configuration.Options["sessionSecret"].ToString();
    //            else
    //                _sessionSecret = Instance.Configuration.Options["sessionSecret"].ToString();

               
    //            if (Instance.Configuration.Options["serviceType"] == null)
    //                serviceType = Instance.ParentInstance.Configuration.Options["serviceType"].ToString();
    //            else
    //                serviceType = Instance.Configuration.Options["serviceType"].ToString();

                

    //            connSession = new myFacebook.Session.ConnectSession(_apiKey, _ap_secret);
    //            connSession.SessionKey = _session;
    //            connSession.SessionSecret = _sessionSecret;
           
              
    //        }
    //        catch (Exception ex)
    //        {
    //            Edge.Core.Utilities.Log.Write("Error in FacebookRetriever.InitConfigurtaionData(): " + ex.Message, Edge.Core.Utilities.LogMessageType.Error);

    //        }

    //    }

        protected override void GetReportData()
        {
            try
            {
               


                myFacebook.Rest.Api _facebookAPI = new myFacebook.Rest.Api(connSession);

                _facebookAPI.Auth.Session.SessionExpires = false;


                Dictionary<string, string> parameterList = new Dictionary<string, string>();




                //=============================================================================               
                //GettAdGroupStats
                parameterList.Add("adgroup_ids", "");
                parameterList.Add("include_deleted", "true");
                parameterList.Add("campaign_ids", "");

               
                parameterList.Add("account_id", _FBaccountID);
                parameterList.Add("method", "facebook.Ads.getAdGroupStats");

                // _requiredDay = new DateTime(2009, 11, 11, 1, 1, 1);

                //Adding 10 hours because the time differences between Israel local time and CA,USA time
                DateTime fromDate = this._requiredDay.AddHours(10);
                DateTime toDate = this._requiredDay.AddDays(1).AddHours(10);
             

              


                long fromLongDate = myFacebook.Utility.DateHelper.ConvertDateToFacebookDate(fromDate);
                long toLongDate = myFacebook.Utility.DateHelper.ConvertDateToFacebookDate(toDate);

                Core.Utilities.Log.Write("  ** DATE **:" + fromDate.ToString(), Core.Utilities.LogMessageType.Information);
                //***** test
               //  fromLongDate = 0;
               //  toLongDate = 0;
                //***** test
                string date = "[{\"time_start\":" + fromLongDate + ",\"time_stop\":" + toLongDate + "}]";
                parameterList.Add("time_ranges", date);

                string res = _facebookAPI.Application.SendRequest(parameterList);
                System.Xml.XmlDocument getAdGroupStatsXmlDoc = new System.Xml.XmlDocument();
                getAdGroupStatsXmlDoc.LoadXml(res);
                int getAdGroupStatsCount = getAdGroupStatsXmlDoc.ChildNodes[1].ChildNodes[0].ChildNodes[2].ChildNodes.Count;
                //~GettAdGroupStats
                //======================================================================================

              //  Core.Utilities.Log.Write("AAAAAAAAAAAAA " + getAdGroupStatsXmlDoc.OuterXml, Core.Utilities.LogMessageType.Error);
                 

                //======================================================================================
                //GetAdGroups Data
                parameterList.Clear();
                parameterList.Add("adgroup_ids", "");
                parameterList.Add("include_deleted", "true");
                parameterList.Add("campaign_ids", "");
                parameterList.Add("account_id", _FBaccountID);
                parameterList.Add("method", "facebook.Ads.getAdGroups");

                System.Xml.XmlDocument getAdGroupsXmlDoc = new System.Xml.XmlDocument();
                string getAdGroupsRes = _facebookAPI.Application.SendRequest(parameterList);
                getAdGroupsXmlDoc.LoadXml(getAdGroupsRes);

                int count = getAdGroupsXmlDoc.ChildNodes[1].ChildNodes.Count;
                List<Dictionary<string, System.Xml.XmlNode>> ListOfAdGroupds = new List<Dictionary<string, System.Xml.XmlNode>>();

                if (count == 0)
                {
                    Core.Utilities.Log.Write("Empty report from facebook ", Core.Utilities.LogMessageType.Warning);
                    return;
                }
                for (int i = 0; i < count; i++)
                {
                    Dictionary<string, System.Xml.XmlNode> newItem = new Dictionary<string, System.Xml.XmlNode>();
                    newItem.Add(getAdGroupsXmlDoc.ChildNodes[1].ChildNodes[i].ChildNodes[0].InnerText, getAdGroupsXmlDoc.ChildNodes[1].ChildNodes[i]);
                    ListOfAdGroupds.Add(newItem);
                }
                //~GetAdGroups Data
                //======================================================================
                 

                //======================================================================
                //GetCampain Data
                parameterList.Clear();
                parameterList.Add("include_deleted", "true");
                parameterList.Add("campaign_ids", "");
                parameterList.Add("account_id", _FBaccountID);
                parameterList.Add("method", "facebook.Ads.getCampaigns");
                string getCampaignsRes = _facebookAPI.Application.SendRequest(parameterList);
                System.Xml.XmlDocument xmlCampaing = new System.Xml.XmlDocument();
                xmlCampaing.LoadXml(getCampaignsRes);
               
                int xmlCampaingCount = xmlCampaing.ChildNodes[1].ChildNodes.Count;
                List<Dictionary<string, System.Xml.XmlNode>> ListOfCampaigns = new List<Dictionary<string, System.Xml.XmlNode>>();
                for (int i = 0; i < xmlCampaingCount; i++)
                {
                    Dictionary<string, System.Xml.XmlNode> newItem = new Dictionary<string, System.Xml.XmlNode>();
                    newItem.Add(xmlCampaing.ChildNodes[1].ChildNodes[i].ChildNodes[1].InnerText, xmlCampaing.ChildNodes[1].ChildNodes[i]);
                    ListOfCampaigns.Add(newItem);
                }
                //~ //GetCampain Data
                //======================================================================


                //======================================================================               
                //GetAdGroupCreatives Data
                parameterList.Clear();
                parameterList.Add("include_deleted", "true");
                parameterList.Add("campaign_ids", "");
                parameterList.Add("account_id", _FBaccountID);
                parameterList.Add("method", "facebook.Ads.getAdGroupCreatives");
                string res3 = _facebookAPI.Application.SendRequest(parameterList);
                System.Xml.XmlDocument xmlCreative = new System.Xml.XmlDocument();
                xmlCreative.LoadXml(res3);

                int xmlCreativeCount = xmlCreative.ChildNodes[1].ChildNodes.Count;
                List<Dictionary<string, System.Xml.XmlNode>> ListOfCreative = new List<Dictionary<string, System.Xml.XmlNode>>();
                 for (int i = 0; i < xmlCreativeCount; i++)                 
                {    
                   //  xmlCreative.ChildNodes[1].ChildNodes[i].OuterXml
                    Dictionary<string, System.Xml.XmlNode> newItem = new Dictionary<string, System.Xml.XmlNode>();
                 //   System.Xml.XmlNode sr =  xmlCreative.ChildNodes[1].ChildNodes[i].SelectSingleNode("/adgroup_id");
                    string adgourp_id = FindIAdgroupID(xmlCreative.ChildNodes[1].ChildNodes[i]);
                  //  var elements = System.Xml.Linq.XDocument.Parse(xmlCreative.ChildNodes[1].ChildNodes[i].OuterXml).Descendants("adgroup_id").Select(e => e.Value);


                    newItem.Add(adgourp_id, xmlCreative.ChildNodes[1].ChildNodes[i]);
                    ListOfCreative.Add(newItem);
                }
                //~GetAdGroupCreatives Data
                //======================================================================



                //  getAdGroupTargeting Data
                //parameterList.Clear();
                //parameterList.Add("include_deleted", "true");
                //parameterList.Add("adgroup_ids", "");
                //parameterList.Add("campaign_ids", "");
                //parameterList.Add("account_id", _FBaccountID);
                //parameterList.Add("method", "facebook.Ads.getAdGroupTargeting");
                //string res4 = _facebookAPI.Application.SendRequest(parameterList);
                //res4 = res4.Replace("xsd:", "");
                //System.Xml.XmlDocument xmlTargeting = new System.Xml.XmlDocument();
                //xmlTargeting.LoadXml(res4);

                //int xmlTargetingount = xmlTargeting.ChildNodes[1].ChildNodes.Count;
                //List<Dictionary<string, System.Xml.XmlNode>> ListOfTargets = new List<Dictionary<string, System.Xml.XmlNode>>();
                //for (int i = 0; i < xmlTargetingount; i++)
                //{
                //    Dictionary<string, System.Xml.XmlNode> newItemTarget = new Dictionary<string, System.Xml.XmlNode>();
                //    newItemTarget.Add(xmlCreative.ChildNodes[1].ChildNodes[i].ChildNodes[0].InnerText, xmlCreative.ChildNodes[1].ChildNodes[i]);
                //    ListOfCreative.Add(newItemTarget);
                //}
                //~getAdGroupTargeting Data
                //======================================================================

                listOfAdGroup = new List<AdGroupClass>();
                string adGroupID,campaignID;

                listOfFaceBookRows = new List<FacebookRow>();
         
                    foreach (System.Xml.XmlNode node in getAdGroupStatsXmlDoc.ChildNodes[1].ChildNodes[0].ChildNodes[2])
                    {
                      //  getAdGroupStatsXmlDoc.Load(@"c:\dt.txt");
                        Easynet.Edge.Services.Facebook.FacebookRow newRow = new FacebookRow();
                    
                        foreach (System.Xml.XmlNode innerChild in node.ChildNodes)
                        {
                           
                            if (innerChild.Name.Equals("id"))
                            {
                                adGroupID = innerChild.InnerText;

                                //run on Creative results
                                for (int g = 0; g < ListOfCreative.Count; g++)
                                {
                                    if (ListOfCreative[g].Keys.First().Equals(adGroupID))
                                    {
                                        foreach (System.Xml.XmlNode innerACreativeChilds in ListOfCreative[g].Values.First().ChildNodes)
                                        {
                                            if (rawDataFields.Fields[innerACreativeChilds.Name] != null)
                                            {
                                                if (rawDataFields.Fields[innerACreativeChilds.Name].Enabled == true)
                                                {
                                                    newRow._Values.Add(rawDataFields.Fields[innerACreativeChilds.Name].Value, innerACreativeChilds.InnerText);
                                                }
                                            }
                                        }
                                        break;

                                    }
                                }
                                //~run on Creative results


                                //run on AdGroups results
                                for (int h = 0; h < ListOfAdGroupds.Count; h++)
                                {
                                    if (ListOfAdGroupds[h].Keys.First().Equals(adGroupID))
                                    {
                                        foreach (System.Xml.XmlNode innerAdGroupChilds in ListOfAdGroupds[h].Values.First().ChildNodes)
                                        {
                                            if (innerAdGroupChilds.Name.Equals("campaign_id"))
                                            {
                                                campaignID = innerAdGroupChilds.InnerText;

                                                for (int j = 0; j < ListOfCampaigns.Count; j++)
                                                {
                                                    if (ListOfCampaigns[j].Keys.First().Equals(campaignID))
                                                    {
                                                        foreach (System.Xml.XmlNode innerCampaignChilds in ListOfCampaigns[j].Values.First().ChildNodes)
                                                        {
                                                            if (rawDataFields.Fields[innerCampaignChilds.Name] != null)
                                                            {
                                                                if (rawDataFields.Fields[innerCampaignChilds.Name].Enabled == true)
                                                                {
                                                                    if (innerCampaignChilds.Name.Equals("name"))
                                                                    {
                                                                        newRow._Values.Add(rawDataFields.Fields["campaign name"].Value, innerCampaignChilds.InnerText);
                                                                    }
                                                                    else
                                                                        newRow._Values.Add(rawDataFields.Fields[innerCampaignChilds.Name].Value, innerCampaignChilds.InnerText);
                                                                }
                                                            }

                                                        }

                                                    }
                                                }
                                            }
                            


                                            if (rawDataFields.Fields[innerAdGroupChilds.Name] != null)
                                            {
                                                if (rawDataFields.Fields[innerAdGroupChilds.Name].Enabled == true)
                                                {
                                                    if(newRow._Values.Keys.Contains<string>( rawDataFields.Fields[innerAdGroupChilds.Name].Value)==false)
                                                        if (innerAdGroupChilds.Name.Equals("name"))
                                                        {


                                                            // PIPE handleing (&) part
                                                            if (innerAdGroupChilds.InnerText.IndexOf(_pipe) > -1)
                                                            {
                                                                string[] arr = innerAdGroupChilds.InnerText.Split(_pipe.ToCharArray());

                                                                
                                                                newRow._Values.Add(rawDataFields.Fields[innerAdGroupChilds.Name].Value, arr[0]);
                                                                newRow._Values.Add("adName", arr[1]);
                                                                // newRow._Values.Add(rawDataFields.Fields[innerAdGroupChilds.Name].Value, innerAdGroupChilds.InnerText.Substring(0, innerAdGroupChilds.InnerText.Length - innerAdGroupChilds.InnerText.IndexOf("@")));

                                                            }
                                                            else
                                                            {
                                                                newRow._Values.Add(rawDataFields.Fields[innerAdGroupChilds.Name].Value, innerAdGroupChilds.InnerText);
                                                                newRow._Values.Add("adName", "");

                                                                // newRow._Values.Add(rawDataFields.Fields[innerAdGroupChilds.Name].Value, innerAdGroupChilds.InnerText);
                                                            }
                                                        }
                                                        else
                                                     newRow._Values.Add(rawDataFields.Fields[innerAdGroupChilds.Name].Value, innerAdGroupChilds.InnerText);
                                                }
                                            }
                                                        
                                        }
                                        break;
                                    }
                                }
                               
                            }

                            //run on AdGroupsStats results
                            if (rawDataFields.Fields[innerChild.Name] != null)
                            {
                                if (rawDataFields.Fields[innerChild.Name].Enabled == true)
                                {
                                    newRow._Values.Add(rawDataFields.Fields[innerChild.Name].Value, innerChild.InnerText);
                                }
                            }
                        }
                        listOfFaceBookRows.Add(newRow);
                    }

                   // Core.Utilities.Log.Write("---------------listOfFaceBookRows.Count, " + listOfFaceBookRows.Count, Core.Utilities.LogMessageType.Error);
            }
 


            catch (Exception ex)
            {
                Core.Utilities.Log.Write("Error in GetReportData(), " + ex.Message, Core.Utilities.LogMessageType.Error);
            }
        }

 
        protected string FindIAdgroupID(System.Xml.XmlNode node)
        {
            foreach (System.Xml.XmlNode _node in node.ChildNodes)
            {
                if (_node.Name.Equals("adgroup_id"))
                    return _node.InnerText;
               
            }
            return ""; ;
        }
        protected override bool SaveReport()
        {
            GetReportData();


            Core.Utilities.Log.Write(" GetReportData() is over"    , Core.Utilities.LogMessageType.Information);
           


            string fileName = WriteResultToFile();

             


            if (null==fileName)
            {
                Core.Utilities.Log.Write("Saved report file name is empty!!  Date:" + _requiredDay, Core.Utilities.LogMessageType.Information);
                return true;
            }

            if ("Zero resaults".Equals(fileName))
            {
                Core.Utilities.Log.Write("Saved report file name is empty!!  Date:" + _requiredDay, Core.Utilities.LogMessageType.Information);
                return true;
            }
            Core.Utilities.Log.Write("fileName: " + fileName, Core.Utilities.LogMessageType.Information);
            //string fileName = WriteResultToFile(dataFromBO, _requiredDay);
            if (!String.IsNullOrEmpty(fileName))
                return SaveFilePathToDB(serviceType, fileName, _requiredDay, _adwordsFile);
            else
            {
                Core.Utilities.Log.Write("Saved report file name is empty.", Core.Utilities.LogMessageType.Information);
                return false;
            }
        }

        protected override string WriteResultToFile()
        {
            try
            {
                if(null==listOfFaceBookRows)
                    return null;
                  //Core.Utilities.Log.Write("---------------listOfFaceBookRows.Count, " + listOfFaceBookRows.Count, Core.Utilities.LogMessageType.Error);
                if (listOfFaceBookRows.Count > 0)
                    return CreateXLSFile(listOfFaceBookRows);
                else
                    Core.Utilities.Log.Write("Zero resaults ", Core.Utilities.LogMessageType.Information);
                {
                    return "Zero resaults";
                }
            }
            catch (Exception ex)
            {
                Core.Utilities.Log.Write("Error in WriteResultToFile(), " + ex.Message, Core.Utilities.LogMessageType.Error);
                return null;
            }
        }
    
      
    }
}
