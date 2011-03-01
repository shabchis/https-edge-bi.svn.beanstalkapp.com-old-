using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.BusinessObjects;


namespace EdgeBI.Data.Pipeline.Objects
{
    
    public class PpcDataUnit
    {
        public int AccountID; // account_id
        public string AdDistribution;//[adwordsType] ,value “Search” turn to “Search Only”, if “Content” -> “Content Only”
        public string AdTitle;
        public int AdId;//creativeid
        public string AdGroupName;//[adgroup]
        public string AdType;//[Advariation]
        public string CampaignName; //campaign
        public long CampaignGK; //
        public string DestinationUrl;//[destUrl]
        public int Impressions;//imps
        public int Clicks;//clicks
        public decimal Ctr;//ctr
        public decimal AverageCpc;//cpc
        public decimal Spend;//cost
        public decimal AveragePosition;//pos
        public decimal Conversions;//conv
        public decimal ConversionRate;//convValue
        public string Keyword;//kwsite
        public long KeywordId;
        public DateTime GregorianDate;//date
        public MatchType Matchtype;//seperia.dbo.dwh_Dim_MatchType
        public int Channel_id = 14; //Bing
        public DateTime Downloaded_date; //
        public int Day_code = Convert.ToInt32(DateTime.Now.Year.ToString("####") + DateTime.Now.Month.ToString("##") + DateTime.Now.Day.ToString("##"));//
        public long GatewayId;//Gateway_id = the number between “sr=” and the next character , http://www.888games.com/?sr=782726
        public long CampaignGk;
        public long AdgroupGk;
        public long CreativeGk;
        public long PPC_CreativeGk;
        public long KeywordGk;
        public long PPC_KeywordGk;
        public long GatewayGk;
        public int AdGroupId;

        public void Save()
        {
           //insert to database 
        }

    }
}
