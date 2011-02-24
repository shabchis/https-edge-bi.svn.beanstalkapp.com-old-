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
        string _zipPath;
        string _xmlPath;
        XmlTextReader _innerReader;

        public BingKeywordReportReader(string zipPath)
        {
            _zipPath = zipPath;
        }

        protected override void Open()
        {
            //_xmlPath = FilesManager.ZipFiles(_zipPath,"1.xml",null);
            _xmlPath = @"C:\tempbing\Accounts\1005\2011\02\07\14\1553253809.xml";
            _innerReader =  new XmlTextReader(_xmlPath);

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
                 
                    PpcDataUnit p =  new PpcDataUnit();
                    p.AccountID = GetAccountIDFromName(xd.DocumentElement["AccountName"].GetAttribute("value"));
                    p.AdDistribution =xd.DocumentElement["AdDistribution"].GetAttribute("value");
                    p.AdId = Convert.ToInt32(xd.DocumentElement["AdId"].GetAttribute("value"));
                    p.AdGroupName = xd.DocumentElement["AdGroupName"].GetAttribute("value");
                    p.AdType = xd.DocumentElement["AdType"].GetAttribute("value");
                    p.CampaignName = xd.DocumentElement["CampaignName"].GetAttribute("value");
                    p.DestinationUrl =xd.DocumentElement["DestinationUrl"].GetAttribute("value");
                    p.Impressions = Convert.ToInt32(xd.DocumentElement["Impressions"].GetAttribute("value"));
                    p.Clicks = Convert.ToInt32(xd.DocumentElement["Clicks"].GetAttribute("value"));
                    p.Ctr = decimal.Parse(xd.DocumentElement["Ctr"].GetAttribute("value"));
                    p.AverageCpc = decimal.Parse(xd.DocumentElement["AverageCpc"].GetAttribute("value"));
                    p.Spend = decimal.Parse(xd.DocumentElement["Spend"].GetAttribute("value"));
                    p.AveragePosition = decimal.Parse(xd.DocumentElement["AveragePosition"].GetAttribute("value"));
                    p.Conversions = decimal.Parse(xd.DocumentElement["Conversions"].GetAttribute("value"));
                    p.ConversionRate = decimal.Parse(xd.DocumentElement["ConversionRate"].GetAttribute("value"));
                    p.Keyword = xd.DocumentElement["Keyword"].GetAttribute("value");
                    string d = xd.DocumentElement["GregorianDate"].GetAttribute("value");// +" 00:00:01";
                    p.GregorianDate = DateTime.ParseExact(d, "MM/dd/yyyy",null); //xd.DocumentElement["GregorianDate"].GetAttribute("value"));
                    p.Matchtype = xd.DocumentElement["MatchType"].GetAttribute("value");
                    p.Channel_id = 14;
                    p.Downloaded_date = DateTime.Now;
                    p.Day_code = Core.Utilities.DayCode.ToDayCode(DateTime.Now);
                    p.GatewayId = GetGateway(_innerReader.ReadElementString("Gateway"));
                    p.CampaignGk = GkManager.GetCampaignGK(p.AccountID, p.Channel_id,p.CampaignName,null);
                    p.AdgroupGk = GkManager.GetAdgroupGK(p.AccountID,p.Channel_id,p.CampaignGk,p.AdGroupName,p.AdGroupId);
                    //p.CreativeGk = GkManager.GetCreativeGK (p.AccountID,);
                    p.GatewayGk = GkManager.GetGatewayGK(p.AccountID,p.GatewayId,p.Channel_id,p.CampaignGK,p.Adgroup_Gk,);
                    //p.PPC_CreativeGk = GkManager.GetAdgroupCreativeGK(p.AccountID,p.Channel_id,p.CampaignGk,p.AdgroupGk,p.CreativeGk,p.DestinationUrl,p.GatewayGk);
                    p.KeywordGk = GkManager.GetKeywordGK(p.AccountID,p.Keyword);
                    p.PPC_KeywordGk = GkManager.GetAdgroupKeywordGK(p.AccountID,p.Channel_id,p.CampaignGk,p.AdgroupGk,p.Keywordid ,p.Matchtype,p.DestinationUrl,p.GatewayGk);
                    return p;
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
}
