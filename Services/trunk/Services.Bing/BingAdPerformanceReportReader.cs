﻿using System;
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
    class BingAdPerformanceReportReader : RowReader<PpcDataUnit>
    {
        string _zipPath;
        string _xmlPath;
        XmlTextReader _innerReader;

        public BingAdPerformanceReportReader(string zipPath)
        {
            _zipPath = zipPath;
        }

        protected override void Open()
        {
            //_xmlPath = FilesManager.ZipFiles(_zipPath,"1.xml",null);
            _xmlPath = @"C:\tempbing\Accounts\1005\2011\02\07\14\1555439817.xml";
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
                 
                    PpcDataUnit objPpcData =  new PpcDataUnit();
                    objPpcData.AccountID = GetAccountIDFromName(xd.DocumentElement["AccountName"].GetAttribute("value"));
                    objPpcData.AdDistribution =xd.DocumentElement["AdDistribution"].GetAttribute("value");
                    objPpcData.AdId = Convert.ToInt32(xd.DocumentElement["AdId"].GetAttribute("value"));
                    objPpcData.AdGroupName = xd.DocumentElement["AdGroupName"].GetAttribute("value");
                    objPpcData.AdType = Convert.ToInt32(xd.DocumentElement["AdType"].GetAttribute("value"));
                    objPpcData.CampaignName = xd.DocumentElement["CampaignName"].GetAttribute("value");
                    //p.DestinationUrl =xd.DocumentElement["DestinationUrl"].GetAttribute("value");
                    objPpcData.Impressions = Convert.ToInt32(xd.DocumentElement["Impressions"].GetAttribute("value"));
                    objPpcData.Clicks = Convert.ToInt32(xd.DocumentElement["Clicks"].GetAttribute("value"));
                    objPpcData.Ctr = decimal.Parse(xd.DocumentElement["Ctr"].GetAttribute("value"));
                    objPpcData.AverageCpc = decimal.Parse(xd.DocumentElement["AverageCpc"].GetAttribute("value"));
                    objPpcData.Spend = decimal.Parse(xd.DocumentElement["Spend"].GetAttribute("value"));
                    objPpcData.AveragePosition = decimal.Parse(xd.DocumentElement["AveragePosition"].GetAttribute("value"));
                    objPpcData.Conversions = decimal.Parse(xd.DocumentElement["Conversions"].GetAttribute("value"));
                    objPpcData.ConversionRate = decimal.Parse(xd.DocumentElement["ConversionRate"].GetAttribute("value"));
                    //p.Keyword = xd.DocumentElement["Keyword"].GetAttribute("value");
                    string d = xd.DocumentElement["GregorianDate"].GetAttribute("value");// +" 00:00:01";
                    objPpcData.GregorianDate = DateTime.ParseExact(d, "MM/dd/yyyy",null); //xd.DocumentElement["GregorianDate"].GetAttribute("value"));
                    //p.Matchtype = xd.DocumentElement["MatchType"].GetAttribute("value");
                    objPpcData.AdGroupId =  Convert.ToInt32(xd.DocumentElement["AdGroupId"].GetAttribute("value"));
                    objPpcData.Channel_id = 14;
                    objPpcData.Downloaded_date = DateTime.Now;
                    objPpcData.Day_code = Core.Utilities.DayCode.ToDayCode(DateTime.Now);
                    objPpcData.GatewayId = Convert.ToInt32(BusinessObjects.Tracker.ExtractTracker(_innerReader.ReadElementString("Gateway"),BusinessObjects.Tracker.GetAccountTrackerPattern(objPpcData.AccountID)));//  GetGateway(_innerReader.ReadElementString("Gateway"));
                    objPpcData.CampaignGk = GkManager.GetCampaignGK(objPpcData.AccountID, objPpcData.Channel_id,objPpcData.CampaignName,null);
                    objPpcData.AdgroupGk = GkManager.GetAdgroupGK(objPpcData.AccountID,objPpcData.Channel_id,objPpcData.CampaignGk,objPpcData.AdGroupName,objPpcData.AdGroupId);
                    //p.CreativeGk = GkManager.GetCreativeGK (p.AccountID,);
                    //objPpcData.GatewayGk = GkManager.GetGatewayGK(objPpcData.AccountID,objPpcData.GatewayId,objPpcData.Channel_id,objPpcData.CampaignGK,objPpcData.Adgroup_Gk,null,);
                    //p.PPC_CreativeGk = GkManager.GetAdgroupCreativeGK(p.AccountID,p.Channel_id,p.CampaignGk,p.AdgroupGk,p.CreativeGk,p.DestinationUrl,p.GatewayGk);
                    objPpcData.KeywordGk = GkManager.GetKeywordGK(objPpcData.AccountID,objPpcData.Keyword);
                    //objPpcData.PPC_KeywordGk = GkManager.GetAdgroupKeywordGK(objPpcData.AccountID,objPpcData.Channel_id,objPpcData.CampaignGk,objPpcData.AdgroupGk,objPpcData.Keywordid ,objPpcData.Matchtype,objPpcData.DestinationUrl,objPpcData.GatewayGk);
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
}
