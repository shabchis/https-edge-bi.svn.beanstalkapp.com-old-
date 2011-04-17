using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Edge.Facebook.Bulkupload.Handlers.UriTemplate;
using Edge.Facebook.Bulkupload.Objects;

namespace Edge.Facebook.Bulkupload.Handlers
{
	public class FacebookHandler : TemplateHandler
	{
		[UriMapping(Method = "POST", Template = "facebook/createfile", BodyParameter = "fileDescription")]
		public string CreateFile(FileDescription fileDescription)
		{

			BulkFile bulkFile = new BulkFile();
			return  bulkFile.CreateFile(fileDescription);
			


		}


		[UriMapping( Method="GET", Template="test")]
		public FileDescription getJsonTemplate()
		{
			FileDescription f = new FileDescription();
			f.Settings.Add(0, new ColumnDescriptionAndValues() { ColumnName = "campaign_name", SettingName = "Default" });
			f.Settings[0].values.Add("Germany 30-39");
			f.Settings.Add(1, new ColumnDescriptionAndValues() { ColumnName = "campaign_daily_budget", SettingName = "Double" });
			f.Settings[1].values.Add("100");
			f.Settings.Add(2, new ColumnDescriptionAndValues() { ColumnName = "campaign_lifetime_budget", SettingName = "Default" });
			f.Settings[2].values.Add("");
			f.Settings.Add(3, new ColumnDescriptionAndValues() { ColumnName = "campaign_time_start", SettingName = "Default" });
			f.Settings[3].values.Add("");
			f.Settings.Add(4, new ColumnDescriptionAndValues() { ColumnName = "campaign_time_stop", SettingName = "Default" });
			f.Settings[4].values.Add("");
			f.Settings.Add(5, new ColumnDescriptionAndValues() { ColumnName = "campaign_run_status", SettingName = "Default" });
			f.Settings[5].values.Add("");
			f.Settings.Add(6, new ColumnDescriptionAndValues() { ColumnName = "ad_status", SettingName = "Default" });
			f.Settings[6].values.Add("active");
			f.Settings.Add(7, new ColumnDescriptionAndValues() { ColumnName = "ad_name *", SettingName = "Counter", PadLeftLength = 3 });
			f.Settings[7].values.Add("30-39 Male @ newmeter#@@");
			f.Settings.Add(8, new ColumnDescriptionAndValues() { ColumnName = "bid_type *", SettingName = "Default" });
			f.Settings[8].values.Add("CPC");
			f.Settings.Add(9, new ColumnDescriptionAndValues() { ColumnName = "max_bid *", SettingName = "Double" });
			f.Settings[9].values.Add("0.75");
			f.Settings.Add(10, new ColumnDescriptionAndValues() { ColumnName = "title *", SettingName = "Default" });
			f.Settings[10].values.Add("Title1");
			f.Settings[10].values.Add("Title2");
			f.Settings.Add(11, new ColumnDescriptionAndValues() { ColumnName = "body *", SettingName = "Default" });
			f.Settings[11].values.Add("Body1");
			f.Settings[11].values.Add("Body2");
			f.Settings.Add(12, new ColumnDescriptionAndValues() { ColumnName = "image", SettingName = "Default" });
			f.Settings[12].values.Add("Image1");
			f.Settings[12].values.Add("Image2");
			f.Settings.Add(13, new ColumnDescriptionAndValues() { ColumnName = "link *", SettingName = "Counter", from = 1000 });
			f.Settings[13].values.Add("http://www.pc-speed.org/scan/de30/scan.php?utm_source=facebook&utm_medium=cpc&utm_campaign=product&EdgeTrackerID=@@");
			f.Settings.Add(14, new ColumnDescriptionAndValues() { ColumnName = "country *", SettingName = "Default" });
			f.Settings[14].values.Add("DE");
			f.Settings.Add(15, new ColumnDescriptionAndValues() { ColumnName = "state", SettingName = "Default" });
			f.Settings[15].values.Add("");
			f.Settings.Add(16, new ColumnDescriptionAndValues() { ColumnName = "city", SettingName = "Default" });
			f.Settings[16].values.Add("Berlin, DE");
			f.Settings.Add(17, new ColumnDescriptionAndValues() { ColumnName = "radius", SettingName = "Int" });
			f.Settings[17].values.Add("10");
			f.Settings.Add(18, new ColumnDescriptionAndValues() { ColumnName = "age_min", SettingName = "Int" });
			f.Settings[18].values.Add("30");
			f.Settings.Add(19, new ColumnDescriptionAndValues() { ColumnName = "age_max", SettingName = "Int" });
			f.Settings[19].values.Add("39");
			f.Settings.Add(20, new ColumnDescriptionAndValues() { ColumnName = "broad_age", SettingName = "Default" });
			f.Settings[20].values.Add("no");
			f.Settings.Add(21, new ColumnDescriptionAndValues() { ColumnName = "gender", SettingName = "Default" });
			f.Settings[21].values.Add("male");
			f.Settings.Add(22, new ColumnDescriptionAndValues() { ColumnName = "Likes and Interests", SettingName = "Default" });
			f.Settings[22].values.Add("Fishing, Diving, Weeds");
			f.Settings.Add(23, new ColumnDescriptionAndValues() { ColumnName = "education_status", SettingName = "Default" });
			f.Settings[23].values.Add("college");
			f.Settings.Add(24, new ColumnDescriptionAndValues() { ColumnName = "college", SettingName = "Default" });
			f.Settings[24].values.Add("");
			f.Settings.Add(25, new ColumnDescriptionAndValues() { ColumnName = "major", SettingName = "Default" });
			f.Settings[25].values.Add("");
			f.Settings.Add(26, new ColumnDescriptionAndValues() { ColumnName = "college_year_min", SettingName = "Default" });
			f.Settings[26].values.Add("");
			f.Settings.Add(27, new ColumnDescriptionAndValues() { ColumnName = "college_year_max", SettingName = "Default" });
			f.Settings[27].values.Add("");
			f.Settings.Add(28, new ColumnDescriptionAndValues() { ColumnName = "company", SettingName = "Default" });
			f.Settings[28].values.Add("Edge.BI");
			f.Settings.Add(29, new ColumnDescriptionAndValues() { ColumnName = "relationship_status", SettingName = "Default" });
			f.Settings[29].values.Add("relationship, engaged, married");
			f.Settings.Add(30, new ColumnDescriptionAndValues() { ColumnName = "interested_in", SettingName = "Default" });
			f.Settings[30].values.Add("men");
			f.Settings.Add(31, new ColumnDescriptionAndValues() { ColumnName = "language", SettingName = "Default" });
			f.Settings[31].values.Add("de_DE");
			f.Settings.Add(32, new ColumnDescriptionAndValues() { ColumnName = "birthday", SettingName = "Default" });
			f.Settings[32].values.Add("");
			f.Settings.Add(33, new ColumnDescriptionAndValues() { ColumnName = "connections", SettingName = "Default" });
			f.Settings[33].values.Add("");
			f.Settings.Add(34, new ColumnDescriptionAndValues() { ColumnName = "excluded_connections", SettingName = "Default" });
			f.Settings[34].values.Add("");
			f.Settings.Add(35, new ColumnDescriptionAndValues() { ColumnName = "creative_type", SettingName = "Default" });
			f.Settings[35].values.Add("");
			f.Settings.Add(36, new ColumnDescriptionAndValues() { ColumnName = "link_object_id", SettingName = "Default" });
			f.Settings[36].values.Add("");
			f.Settings.Add(37, new ColumnDescriptionAndValues() { ColumnName = "friends_of_connections", SettingName = "Default" });
			f.Settings[37].values.Add("");


			return f;







			//f.Settings.Add(0, new ColumnDescriptionAndValues() { ColumnName = "ad_name", SettingName = "Default" });
			//f.Settings[0].values.Add("Ad1");
			//f.Settings[0].values.Add("Ad2");
			//f.Settings[0].values.Add("Ad3");
			//f.Settings.Add(1, new ColumnDescriptionAndValues() { ColumnName = "ad_status", SettingName = "Default" });
			//f.Settings[1].values.Add("ad_status1");
			//f.Settings[1].values.Add("ad_status2");
			//f.Settings[1].values.Add("ad_status3");
			//f.Settings.Add(2, new ColumnDescriptionAndValues() { ColumnName = "age_min", SettingName = "Int" });
			//f.Settings[2].values.Add("15");
			//f.Settings.Add(3, new ColumnDescriptionAndValues() { ColumnName = "age_max", SettingName = "Int" });
			//f.Settings[3].values.Add("20");

			//f.Settings.Add(4, new ColumnDescriptionAndValues() { ColumnName = "Link", SettingName = "Link" });
			//f.Settings[4].values.Add("www.ynet.co.il");			


			//return f;
			 
		}
		public override bool ShouldValidateSession
		{
			get { return false; }
		}
	}
}