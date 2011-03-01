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

            //Moves the reader to the root element.
            //_innerReader.MoveToContent();


            // TODO: create _innerReader
        }

        protected override PpcDataUnit NextRow()
        {
            while (_innerReader.Read())
            {
                if (_innerReader.Name == "Row")
                {
                    string row = _innerReader.ReadOuterXml();
                    XmlDocument xd = new XmlDocument();
                    xd.LoadXml(row);
                 
                    PpcDataUnit objPpcData =  new PpcDataUnit();
                    AdPerformanceValues ad = new AdPerformanceValues();
                    
                    objPpcData.AccountID = GetAccountIDFromName(xd.DocumentElement["AccountName"].GetAttribute("value"));
                    objPpcData.AdDistribution =xd.DocumentElement["AdDistribution"].GetAttribute("value");
                    objPpcData.AdId = Convert.ToInt32(xd.DocumentElement["AdId"].GetAttribute("value"));
                    objPpcData.AdDistribution = Ads[objPpcData.AdId].AdDescription;
                    objPpcData.AdTitle = Ads[objPpcData.AdId].AdTitle;
                    objPpcData.AdGroupName = xd.DocumentElement["AdGroupName"].GetAttribute("value");
                    objPpcData.AdType = xd.DocumentElement["AdType"].GetAttribute("value");
                    objPpcData.CampaignName = xd.DocumentElement["CampaignName"].GetAttribute("value");
                    objPpcData.DestinationUrl =xd.DocumentElement["DestinationUrl"].GetAttribute("value");
                    objPpcData.Impressions = Convert.ToInt32(xd.DocumentElement["Impressions"].GetAttribute("value"));
                    objPpcData.Clicks = Convert.ToInt32(xd.DocumentElement["Clicks"].GetAttribute("value"));
                    objPpcData.Ctr = decimal.Parse(xd.DocumentElement["Ctr"].GetAttribute("value"));
                    objPpcData.AverageCpc = decimal.Parse(xd.DocumentElement["AverageCpc"].GetAttribute("value"));
                    objPpcData.Spend = decimal.Parse(xd.DocumentElement["Spend"].GetAttribute("value"));
                    objPpcData.AveragePosition = decimal.Parse(xd.DocumentElement["AveragePosition"].GetAttribute("value"));
                    objPpcData.Conversions = decimal.Parse(xd.DocumentElement["Conversions"].GetAttribute("value"));
                    objPpcData.ConversionRate = decimal.Parse(xd.DocumentElement["ConversionRate"].GetAttribute("value"));
                    objPpcData.Keyword = xd.DocumentElement["Keyword"].GetAttribute("value");
                    string d = xd.DocumentElement["GregorianDate"].GetAttribute("value");// +" 00:00:01";
                    objPpcData.GregorianDate = DateTime.ParseExact(d, "M/d/yyyy",null); //xd.DocumentElement["GregorianDate"].GetAttribute("value"));
                    objPpcData.Matchtype = (MatchType)Enum.Parse(typeof(MatchType), xd.DocumentElement["MatchType"].GetAttribute("value"), true);
                    objPpcData.Channel_id = 14;
                    objPpcData.Downloaded_date = DateTime.Now;
                    objPpcData.Day_code = Core.Utilities.DayCode.ToDayCode(DateTime.Now);
                    //GatewayGk Gateway name ?
                    objPpcData.GatewayGk = GkManager.GetGatewayGK(objPpcData.AccountID, objPpcData.GatewayId, objPpcData.Channel_id, objPpcData.CampaignGK, objPpcData.AdgroupGk, null, objPpcData.DestinationUrl, GatewayReferenceType.Keyword, objPpcData.KeywordGk);
                    objPpcData.CampaignGk = GkManager.GetCampaignGK(objPpcData.AccountID, objPpcData.Channel_id,objPpcData.CampaignName,null);
                    objPpcData.AdgroupGk = GkManager.GetAdgroupGK(objPpcData.AccountID,objPpcData.Channel_id,objPpcData.CampaignGk,objPpcData.AdGroupName,objPpcData.AdGroupId);
                    objPpcData.CreativeGk = GkManager.GetCreativeGK(objPpcData.AccountID, objPpcData.AdTitle, objPpcData.AdDistribution, string.Empty);
                    objPpcData.PPC_CreativeGk = GkManager.GetAdgroupCreativeGK(objPpcData.AccountID, objPpcData.Channel_id, objPpcData.CampaignGk, objPpcData.AdgroupGk, objPpcData.CreativeGk, objPpcData.DestinationUrl, string.Empty, objPpcData.GatewayGk);
                    objPpcData.KeywordGk = GkManager.GetKeywordGK(objPpcData.AccountID,objPpcData.Keyword);
                    objPpcData.PPC_KeywordGk = GkManager.GetAdgroupKeywordGK(objPpcData.AccountID,objPpcData.Channel_id,objPpcData.CampaignGk,objPpcData.AdgroupGk,objPpcData.KeywordId,objPpcData.Matchtype,objPpcData.DestinationUrl,objPpcData.GatewayGk);
                    return objPpcData;
                }
            }
            return new PpcDataUnit();
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
        }
    }
    public class AdPerformanceValues
    {
        public string AdDescription;
        public string AdTitle;
    }
}
