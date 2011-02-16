using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Services.Data.Pipeline;
using EdgeBI.Data.Pipeline.Objects;
using System.Xml;
using Easynet.Edge.BusinessObjects;
using EdgeBI.Data.Pipeline;

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
            Open();
        }

        protected override void Open()
        {
            //_xmlPath = FilesManager.ZipFiles(_zipPath,"1.xml",null);

            _innerReader = new XmlTextReader(_xmlPath);
            // TODO: create _innerReader
        }

        protected override PpcDataUnit NextRow()
        {
            throw new NotImplementedException();

            //_innerReader.ReadInnerXml();

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
                matchtype = Convert.ToInt32(_innerReader.ReadElementString("matchtype")),
                Channel_id = 14,
                Matchtype= 1,
                Downloaded_date = DateTime.Now,
                Day_code = Core.Utilities.DayCode.ToDayCode(DateTime.Now),
                Gateway = GetGateway(_innerReader.ReadElementString("Gateway"))
            };
        }

        public override void Dispose()
        {
            _innerReader.Close();
        }
    }
}
