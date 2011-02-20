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
            //להביא את הקובץ לשורה
            //_innerReader.ReadInnerXml();
            _innerReader.ReadToFollowing("Row");
            _innerReader.ReadAttributeValue();
            _innerReader.GetAttribute(0);

            if (_innerReader.Name == "Row")
                return new PpcDataUnit()
                {
                    AccountID = GetAccountIDFromName(_innerReader.ReadElementString("AccoutName")),
                    AdDistribution = _innerReader.ReadElementString("AdDistribution"),
                    AdId = Convert.ToInt32(_innerReader.ReadElementString("AdId")),
                    AdGroupName = _innerReader.ReadElementString("AdGroupName"),
                    AdType = _innerReader.ReadElementString("AdType"),
                    CampaignName = _innerReader.ReadElementString("CampaignName"),
                    DestinationUrl = _innerReader.ReadElementString("DestinationUrl"),
                    Impressions = Convert.ToInt32(_innerReader.ReadElementString("Impressions")),
                    Clicks = Convert.ToInt32(_innerReader.ReadElementString("Clicks")),
                    Ctr = Convert.ToInt32(_innerReader.ReadElementString("Ctr")),
                    AverageCpc = Convert.ToDecimal(_innerReader.ReadElementString("AverageCpc")),
                    Spend = Convert.ToDecimal(_innerReader.ReadElementString("Spend")),
                    AveragePosition = Convert.ToDecimal(_innerReader.ReadElementString("AveragePosition")),
                    Conversions = Convert.ToInt32(_innerReader.ReadElementString("Conversions")),
                    ConversionRate = Convert.ToInt32(_innerReader.ReadElementString("ConversionRate")),
                    Keyword = _innerReader.ReadElementString("Keyword"),
                    GregorianDate = Convert.ToDateTime(_innerReader.ReadElementString("GregorianDate")),
                    Matchtype = Convert.ToInt32(_innerReader.ReadElementString("matchtype")),
                    Channel_id = 14,
                    Downloaded_date = DateTime.Now,
                    Day_code = Core.Utilities.DayCode.ToDayCode(DateTime.Now),
                    //GatewayId = GetGateway(_innerReader.ReadElementString("Gateway")),
                    //Campaign_gk = GkManager.GetCampaignGK(AccountID, Channel_id,CampaignName),
                    //Adgroup_gk = GkManager.GetAdgroupGK(AccountID,Channel_id,Campaign_gk,AdGroupName),
                    //Creative_gk = GkManager.GetCampaignGK(AccountID,Channel_id,CampaignName),
                    //Gateway_gk = GkManager.GetGatewayGK(AccountID,GatewayId),
                    //PPC_Creative_gk = GkManager.GetAdgroupCreativeGK(AccountID,Channel_id,Campaign_gk,Adgroup_gk,Creative_gk,DestinationUrl,,Gateway_gk),
                    //Keyword_gk = GkManager.GetKeywordGK(AccountID,Keyword),
                    //PPC_Keyword_gk = GkManager.GetAdgroupKeywordGK(AccountID,Channel_id,Campaign_gk,Adgroup_gk,,Matchtype,DestinationUrl,Gateway_gk)
                };
            else
                return new PpcDataUnit();
        }

        private int GetAccountIDFromName(string accoutName)
        {
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
