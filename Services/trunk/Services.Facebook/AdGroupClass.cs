using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easynet.Edge.Services.Facebook
{
    public class AdGroupClass
    {
        public string _ad_Id;
        public string _campain_Id;
        public string _name;
        public string _status;
        public string _bidType;
        public string _priority;
        public string _max_bid;
        public string _adgroup_status;
        public string _adgroup_id;
        public string _impressions;
        public string _clicks;
        public string _spent;
        public string _actions;
        public string _campainName;
        public string _creative_Id;
        public string _creativeType;
        public string _creativeTitle;
        public string _creativeLink;
        public string _creativeLinkType;
        public string _creativeIamgeURL;
        public string _creativePreviewRL;
        public string _creative_Tracking_pixel_url;
        public string _creativeBody;

        public AdGroupClass(string adID, string campain_Id, string name, string status, string bidType, string priority
            , string max_bid, string adgroup_status, string adgroup_id, string impressions,  
              string clicks, string spent, string actions)
        {
            _ad_Id = adID;
            _campain_Id = campain_Id;
            _name = name;
            _status = status;
            _bidType = bidType;
            _priority = priority;
            _max_bid = max_bid;
            _adgroup_id = adgroup_id;
            _adgroup_status = adgroup_status;
            _impressions = impressions;
            _clicks = clicks;
            _spent = spent;
            _actions = actions;




        }

        public AdGroupClass(string ad_Id, string impressions,  
              string clicks, string spent, string actions)
        {
            _ad_Id = ad_Id;            
            _impressions = impressions;
            _clicks = clicks;
            _spent = spent;
            _actions = actions;

        }
    }
}
