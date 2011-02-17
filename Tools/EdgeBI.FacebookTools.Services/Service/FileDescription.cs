using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EdgeBI.FacebookTools.Services.Service
{
	public class FileDescription
	{
		public Dictionary<int, ColumnDescription> Settings=new Dictionary<int,ColumnDescription>();
		public ValuesLists ListsOfValues=new ValuesLists();
		


	}
	public class ColumnDescription
	{
		public string ColumnName;
		public string SettingName;
	}
	public class ValuesLists
	{
		public List<string> campaign_name = new List<string>();
		public List<string> campaign_daily_budget = new List<string>();
		public List<string> campaign_lifetime_budget = new List<string>();
		public List<string> campaign_time_start = new List<string>();
		public List<string> campaign_time_stop = new List<string>();
		public List<string> campaign_run_status = new List<string>();
		public List<string> ad_status = new List<string>();
		public List<string> ad_name = new List<string>();
		public List<string> bid_type = new List<string>();
		public List<string> max_bid = new List<string>();
		public List<string> title = new List<string>();
		public List<string> body = new List<string>();
		public List<string> image = new List<string>();
		public List<string> link = new List<string>();
		public List<string> country = new List<string>();
		public List<string> state = new List<string>();
		public List<string> city = new List<string>();
		public List<string> radius = new List<string>();
		public List<string> age_min = new List<string>();
		public List<string> age_max = new List<string>();
		public List<string> broad_age = new List<string>();
		public List<string> gender = new List<string>();
		public List<string> Likes_and_Interests = new List<string>();
		public List<string> education_status = new List<string>();
		public List<string> college = new List<string>();
		public List<string> major = new List<string>();
		public List<string> college_year_min = new List<string>();
		public List<string> college_year_max = new List<string>();
		public List<string> company = new List<string>();
		public List<string> relationship_status = new List<string>();
		public List<string> language = new List<string>();
		public List<string> birthday = new List<string>();
		public List<string> connections = new List<string>();
		public List<string> excluded_connections = new List<string>();
		public List<string> creative_type = new List<string>();
		public List<string> link_object_id = new List<string>();
		public List<string> friends_of_connections = new List<string>();
		public List<string> interested_in = new List<string>();
		

	}
		
}