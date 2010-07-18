using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easynet.Edge.Services.Facebook
{
    public class CampaignClass
    {
        public string _campaignId;
        public string _campaign_name;
        public string _time_start;
        public string _time_stop;
        public string _campaign_status;
        public string _daily_budget;


        public CampaignClass(string campaignId, string campaign_name, 
            string time_start, string time_stop, 
          string daily_budget,  string campaign_status)
        {
            _campaignId = campaignId ;
            _campaign_name = campaign_name ;
            _time_start = time_start;
            _time_stop = time_stop;
            _campaign_status = campaign_status;
            _daily_budget = daily_budget;
           



        }

    }
}
