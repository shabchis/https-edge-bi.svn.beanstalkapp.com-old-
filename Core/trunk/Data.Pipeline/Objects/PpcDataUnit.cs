using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.BusinessObjects;
using Services.Data.Pipeline;


namespace EdgeBI.Data.Pipeline.Objects
{
    
    public class PpcDataUnit
    {
        [FieldMap("AccountID")]
        public int AccountID; // account_id
        [FieldMap("AdDistribution")]
        public string AdDistribution;//[adwordsType] ,value “Search” turn to “Search Only”, if “Content” -> “Content Only”
        [FieldMap("AdTitle")] //headline
        public string AdTitle;
        [FieldMap("AdId")]
        public long AdId;//creativeid
        [FieldMap("AdGroupName")]
        public string AdGroupName;//[adgroup]
        [FieldMap("AdType")]
        public int AdType;//[Advariation]
        [FieldMap("CampaignName")]
        public string CampaignName; //campaign
        [FieldMap("DestinationUrl")]
        public string DestinationUrl;//[destUrl]
        [FieldMap("Impressions")]
        public int Impressions;//imps
        [FieldMap("Clicks")]
        public int Clicks;//clicks
        [FieldMap("Ctr")]
        public decimal Ctr;//ctr
        [FieldMap("AverageCpc")]
        public decimal AverageCpc;//cpc
        [FieldMap("Spend")]
        public decimal Spend;//cost
        [FieldMap("AveragePosition")]
        public decimal AveragePosition;//pos
        [FieldMap("Conversions")]
        public decimal Conversions;//conv
        [FieldMap("ConversionRate")]
        public decimal ConversionRate;//convValue
        [FieldMap("Keyword")]
        public string Keyword;//kwsite
        [FieldMap("KeywordId")]
        public long KeywordId;
        [FieldMap("GregorianDate")]
        public DateTime GregorianDate;//date
        [FieldMap("Matchtype")]
        public MatchType Matchtype;//seperia.dbo.dwh_Dim_MatchType
        [FieldMap("Channel_id")]
        public int Channel_id; //Bing
        [FieldMap("Downloaded_date")]
        public DateTime Downloaded_date; //
        [FieldMap("Day_code")]
        public int Day_code ;//
        [FieldMap("GatewayId")]
        public long GatewayId;//Gateway_id = the number between “sr=” and the next character , http://www.888games.com/?sr=782726
        [FieldMap("CampaignGk")]
        public long CampaignGk;
        [FieldMap("AdgroupGk")]
        public long AdgroupGk;
        [FieldMap("CreativeGk")]
        public long CreativeGk;
        [FieldMap("PPC_CreativeGk")]
        public long PPC_CreativeGk;
        [FieldMap("KeywordGk")]
        public long KeywordGk;
        [FieldMap("PPC_KeywordGk")]
        public long PPC_KeywordGk;
        [FieldMap("GatewayGk")]
        public long GatewayGk;
        [FieldMap("AdGroupId")]
        public int AdGroupId;


        public void Save()
        {
            string command;

            command = "SP_BingSaveRow(@AccountID:int,@AdDistribution:nvarchar(255),@AdTitle:nvarchar(255),@AdId:bigint,@AdGroupName:nvarchar(255),@AdType:int,@CampaignName:nvarchar(255),@DestinationUrl:nvarchar(1000)," +
                      "@Impressions:bigint,@Clicks:bigint,@Ctr:float,@AverageCpc:float,@Spend:float,@AveragePosition:float,@Conversions:float,@ConversionRate:float,@Keyword:nvarchar(255),@KeywordId:bigint,@GregorianDate:datetime,@Matchtype:int,@Channel_id:int," +
                      "@Downloaded_date:datetime,@Day_code:int,@GatewayId:bigint,@CampaignGk:bigint,@AdgroupGk:bigint,@CreativeGk:bigint,@PPC_CreativeGk:bigint,@KeywordGk:bigint,@PPC_KeywordGk:bigint,@GatewayGk:bigint,@AdGroupId:bigint)";
            mappper.SaveOrRemoveSimpleObject<Delivery>(command, this);

           //insert to database 
        }

    }
}


