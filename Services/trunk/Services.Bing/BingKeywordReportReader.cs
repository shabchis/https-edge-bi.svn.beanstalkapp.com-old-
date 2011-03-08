using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Services.Data.Pipeline;
using EdgeBI.Data.Pipeline.Objects;
using System.Xml;
using Easynet.Edge.BusinessObjects;
using EdgeBI.Data.Pipeline;
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;
using System.Data;
using Easynet.Edge.Services.Bing.ReportingService;


namespace Easynet.Edge.Services.Bing
{
    public class BingKeywordReportReader: RowReader<PpcDataUnit>
    {
        string _zipPathKeywordFile;
        string _xmlPathKeywordFile;
        string _zipPathAdFile;
        string _xmlPathAdFile;
        Dictionary<long, AdPerformanceValues> Ads;

        XmlTextReader _innerReader;
        XmlDocument _xd;
        int _RowNumber = 0;
        int _Columns = 0;

        public BingKeywordReportReader(string[] zipPath)
        {
            _zipPathKeywordFile = zipPath[0];
            _zipPathAdFile = zipPath[1];
            //TODO Get the file from zip
            //_xmlPathKeywordFile = FilesManager.ZipFiles(_zipPathKeywordFile,"Keyword.xml",null);
            //_xmlPathAdFile = FilesManager.ZipFiles(_zipPathAdFile,"Ad.xml",null);

            _xmlPathKeywordFile = @"C:\tempbing\Accounts\1005\2011\03\01\21\1556480431.xml";
            _xmlPathAdFile = @"C:\tempbing\Accounts\1005\2011\03\01\21\1556481035.xml";
            GetAdDictionary();

        }

        private void GetAdDictionary()
        {
            try
            {
                Ads = new Dictionary<long, AdPerformanceValues>();
                XmlTextReader adReader = new XmlTextReader(_xmlPathAdFile);
                while (adReader.Read())
                {
                    if (adReader.Name == "Row")
                    {
                        string row = adReader.ReadOuterXml();
                        XmlDocument xd = new XmlDocument();
                        xd.LoadXml(row);
                        if(!Ads.ContainsKey(Convert.ToInt64(xd.DocumentElement["AdId"].GetAttribute("value"))))
                        {
                            AdPerformanceValues ad = new AdPerformanceValues();
                            ad.AdDescription = xd.DocumentElement["AdDescription"].GetAttribute("value");
                            ad.AdTitle = xd.DocumentElement["AdTitle"].GetAttribute("value");
                            Ads.Add(Convert.ToInt64(xd.DocumentElement["AdId"].GetAttribute("value")), ad);
                        }
                        
                    }
                }
            }
            catch( Exception ex)
            {
                throw new Exception(ex.Message);
            }

            
        }

        protected override void Open()
        {
            _innerReader = new XmlTextReader(_xmlPathKeywordFile);
            MoveXmlToTable();

        }
        private void MoveXmlToTable()
        {
            while (_innerReader.Read())
                if (_innerReader.Name == "Table")
                {
                    string row = _innerReader.ReadOuterXml();
                    _xd = new XmlDocument();
                    _xd.LoadXml(row);
                    if (_xd.ChildNodes[0].ChildNodes.Count > 0)
                        _Columns = _xd.ChildNodes[0].ChildNodes[_RowNumber].ChildNodes.Count;
                    break;
                }
        }

        protected override PpcDataUnit NextRow()
        {
            try
            {
                if (_xd.ChildNodes[0].ChildNodes.Count > _RowNumber)
                {
                    PpcDataUnit objPpcData = new PpcDataUnit();
                    AdPerformanceValues ad = new AdPerformanceValues();
                    for (int i = 0; i < _Columns; i++)
                    {
                        string nameNode = _xd.ChildNodes[0].ChildNodes[_RowNumber].ChildNodes[i].Name;
                        string valueNode = _xd.ChildNodes[0].ChildNodes[_RowNumber].ChildNodes[i].Attributes[0].Value;
                        switch (nameNode)
                        {
                            case "AccountName":
                                objPpcData.AccountID = GetAccountIDFromName(valueNode);
                                break;
                            case "AdDistribution":
                                objPpcData.AdDistribution = valueNode;
                                break;
                            case "AdId":
                                objPpcData.AdId = valueNode != string.Empty ? Convert.ToInt32(valueNode) : 0; 
                                break;
                            case "AdGroupName":
                                objPpcData.AdGroupName = valueNode;
                                break;
                            case "AdType":
                                objPpcData.AdType = (int)Enum.Parse(typeof(AdVariation), valueNode.Substring(0, valueNode.IndexOf(" ad")), true);// Convert.ToInt32(valueNode);
                                break;
                            case "CampaignName":
                                objPpcData.CampaignName = valueNode;
                                break;
                            case "DestinationUrl":
                                objPpcData.DestinationUrl = valueNode;
                                break;
                            case "Impressions":
                                objPpcData.Impressions = valueNode != string.Empty ? Convert.ToInt32(valueNode) : 0; 
                                break;
                            case "Clicks":
                                objPpcData.Clicks = valueNode != string.Empty ? Convert.ToInt32(valueNode) : 0; 
                                break;
                            case "Ctr":
                                objPpcData.Ctr = valueNode != string.Empty ? decimal.Parse(valueNode) : 0;
                                break;
                            case "AverageCpc":
                                objPpcData.AverageCpc = valueNode != string.Empty ? decimal.Parse(valueNode) : 0;
                                break;
                            case "Spend":
                                objPpcData.Spend = valueNode != string.Empty ? decimal.Parse(valueNode) : 0;
                                break;
                            case "AveragePosition":
                                objPpcData.AveragePosition = valueNode != string.Empty ? decimal.Parse(valueNode) : 0;
                                break;
                            case "Conversions":
                                objPpcData.Conversions = valueNode != string.Empty ? decimal.Parse(valueNode) : 0;
                                break;
                            case "ConversionRate":
                                objPpcData.ConversionRate = valueNode != string.Empty ?  decimal.Parse(valueNode):0;
                                break;
                            case "Keyword":
                                objPpcData.Keyword = valueNode;
                                break;
                            case "GregorianDate":
                                string d = valueNode;
                                objPpcData.GregorianDate = DateTime.ParseExact(d, "M/d/yyyy", null);
                                break;
                            case "MatchType":
                                objPpcData.Matchtype = (MatchType)Enum.Parse(typeof(MatchType), valueNode, true);
                                break;
                            default:
                                break;

                        }
                    }
                    objPpcData.AdDistribution = Ads[objPpcData.AdId].AdDescription;
                    objPpcData.AdTitle = Ads[objPpcData.AdId].AdTitle;
                    objPpcData.Channel_id = 14;
                    objPpcData.Downloaded_date = DateTime.Now;
                    objPpcData.Day_code = Core.Utilities.DayCode.ToDayCode(DateTime.Now);
                    objPpcData.CampaignGk = GkManager.GetCampaignGK(objPpcData.AccountID, objPpcData.Channel_id, objPpcData.CampaignName, null);
                    objPpcData.GatewayGk = GkManager.GetGatewayGK(objPpcData.AccountID, objPpcData.GatewayId, objPpcData.Channel_id, objPpcData.CampaignGk, objPpcData.AdgroupGk, null, objPpcData.DestinationUrl, GatewayReferenceType.Keyword, objPpcData.KeywordGk);
                    objPpcData.AdgroupGk = GkManager.GetAdgroupGK(objPpcData.AccountID, objPpcData.Channel_id, objPpcData.CampaignGk, objPpcData.AdGroupName, objPpcData.AdGroupId);
                    objPpcData.CreativeGk = GkManager.GetCreativeGK(objPpcData.AccountID, objPpcData.AdTitle, objPpcData.AdDistribution, string.Empty);
                    objPpcData.PPC_CreativeGk = GkManager.GetAdgroupCreativeGK(objPpcData.AccountID, objPpcData.Channel_id, objPpcData.CampaignGk, objPpcData.AdgroupGk, objPpcData.CreativeGk, objPpcData.DestinationUrl, string.Empty, objPpcData.GatewayGk);
                    objPpcData.KeywordGk = GkManager.GetKeywordGK(objPpcData.AccountID, objPpcData.Keyword);
                    objPpcData.PPC_KeywordGk = GkManager.GetAdgroupKeywordGK(objPpcData.AccountID, objPpcData.Channel_id, objPpcData.CampaignGk, objPpcData.AdgroupGk, objPpcData.KeywordId, objPpcData.Matchtype, objPpcData.DestinationUrl, objPpcData.GatewayGk);
                    _RowNumber += 1;
                    return objPpcData;

                }
                else
                    return null;

            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        private int GetAccountIDFromName(string accoutName)
        {
            return 10001;
            try
            {
                int accountId = 0;
                SqlCommand EngineCmd = DataManager.CreateCommand("SP_GetAccountIDByName(@AccoutName:nvarchar(50))", CommandType.StoredProcedure);
                EngineCmd.Parameters["@AccoutName"].Value = accoutName;
                SqlDataReader reader = EngineCmd.ExecuteReader();
                reader.Read();
                if (!reader["Account_ID"].Equals(System.DBNull.Value))
                    accountId = (int)reader["Account_ID"];
                else
                    throw new Exception("The accout name was not found!");

                reader.Close();
                return accountId;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public override void Dispose()
        {
            _innerReader.Close();
            _xd.Clone();
        }
    }
    public class AdPerformanceValues
    {
        public string AdDescription;
        public string AdTitle;
    }
}
