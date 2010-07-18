using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Data;

namespace Easynet.Edge.BusinessObjects
{
	public class Channel: DataItem
	{
		public int ID;
		public string Name;
		public bool HasPpcApi;
		public string DisplayName;

		public static Channel WithID(int id) { return null; }
		public static Channel Get(int id) { return null; }
		public static Channel[] Get() { return null; }
	}

	public class SearchEngine: DataItem
	{
		public int ID;
		public Channel Channel;
		public string Name;

		public static SearchEngine WithID(int id) { return null; }
		public static SearchEngine Get(int id) { return null; }
		public static SearchEngine[] Get(Channel channel) { return null; }
	}

	[Serializable]
	public class Account//: DataItem
	{
		public int ID;
		public string Name;
		public string GatewayBaseUrl;
		public List<Account> GatewayAccountScope;

		public static Account WithID(int id)
		{
			Account ac = new Account();
			ac.ID = id;
			return ac;
		}

		public static Account Get(int id) { return null; }
		public static Account[] Get(User user) { return null; }
	}

	public class Segment: DataItem
	{
		public int ID;
		public List<string> Values;
	}

	/*
	public class Keyword: DataItem
	{
		public long ID;
		public Account Account;
		public string StringValue;
		public bool IsMonitored;
		public Dictionary<Segment, string> Segments;

		public static Keyword WithID(long id) { return null; }
		public static Keyword Get(long id) { return null; }
		public static Keyword[] Get(Account account) { return null; }
		public static Keyword[] Get(Account account, string filter, int resultLimit, bool includeUnmonitored, bool includeRelated) { return null; }
	}
	*/

	public class Creative: DataItem
	{
		public static class ColumnNames
		{
			public const string GK = "Creative_GK";
			public const string AccountID = "Account_ID";
			public const string Title = "Creative_Title";
			public const string Desc1 = "Creative_Desc1";
			public const string Desc2 = "Creative_Desc2";
			public const string LastUpdated = "LastUpdated";
		}

		public long ID;
		public Account Account;
		public string Title;
		public string Description1;
		public string Description2;
		public bool IsMonitored;
		public Dictionary<Segment, string> Segments;

		public static Creative WithID(long id) { return null; }
		public static Creative Get(long id) { return null; }
		public static Creative[] Get(Account account) { return null; }
		public static Creative[] Get(Account account, string filter, int resultLimit, bool includeUnmonitored) { return null; }
	}

	public class Site: DataItem
	{
		public static class ColumnNames
		{
			public const string GK = "Site_GK";
			public const string AccountID = "Account_ID";
			public const string Name = "Site";
		}
	}

	public class Page: DataItem
	{
		public long ID;
		public Account Account;
		public string Title;
		public string Url;
		public bool IsMonitored;
		public Dictionary<Segment, string> Segments;

		public static Page WithID(long id) { return null; }
		public static Page Get(long id) { return null; }
		public static Page[] Get(Account account) { return null; }
		public static Page[] Get(Account account, string filter, int resultLimit, bool includeUnmonitored) { return null; }
	}

	public class Adunit: DataItem
	{
		public long ID;
		public Account Account;
		public string Name;
		public bool IsDefaultAdunit;
		public AdunitTargetCollection Targets;

		public static Adunit WithID(long id) { return null; }
		public static Adunit Get(long id) { return null; }
		public static Adunit[] Get(Account account) { return null; }
		public static Adunit[] Get(Account account, string filter) { return null; }
	}

	public class AdunitTargetCollection: DataItemCollection
	{
		public Adunit Adunit;
	}

	public class AdunitTarget: DataItem
	{
		public DateTime TargetMonth;
		public double Cost;
		public int NewActiveUsers;
		public int NewUsers;
		public double CPA;
		public double CPC;
		public double Clicks;
		public double CPL;
	}

	public enum GatewayReferenceType
	{
		Creative = 0,
		Keyword = 1,
		Site = 2
	}

	public class Gateway: DataItem
	{
		public static class ColumnNames
		{
			public const string GK = "Gateway_GK";
			public const string AccountID = "Account_ID";
			public const string ChannelID = "Channel_ID";
			public const string CampaignGK = "Campaign_GK";
			public const string AdgroupGK = "Adgroup_GK";
			public const string AdunitID = "Adunit_ID";
			public const string ReferenceType = "Reference_Type";
			public const string ReferenceGK = "Reference_ID";
			public const string Identifier = "Gateway_id";
			public const string Name = "Gateway";
			public const string DestUrl = "Dest_URL";
			public const string LastUpdated = "LastUpdated";
		}

		public long ID;
		public long Identifier;
		public string Name;
		public string DestinationUrl;
		public Account Account;
		public Adunit Adunit;
		public Campaign Campaign;
		public Adgroup Adgroup;
		public Page Page;
		public GatewayReferenceType TargetType;
		public AdgroupCreative AdgroupCreative;
		public AdgroupKeyword AdgroupKeyword;
		public Dictionary<Segment, string> Segments;

		public static Gateway WithID(long id) { return null; }
		public static Gateway Get(long id) { return null; }
		public static Gateway[] Get(Adunit adunit) { return null; }
		public static Gateway[] Get(AdgroupCreative creative) { return null; }
		public static Gateway[] Get(AdgroupKeyword keyword) { return null; }
		public static Gateway[] Get(Account account, long identifier) { return null; }
		public static long GetHighestIdentifier(Account account) { return -1; }
	}

	public class GatewayReservationCollection: DataItemCollection
	{
		public Page Page;
	}

	public class GatewayReservation: DataItem
	{
		public Account Account;
		public Page Page;
		public long FromGatewayIdentifier;
		public long ToGatewayIdentifier;
		public string ReserverUserName;
		public int ReserverUserID;
		public bool CrossCheckAccounts;

		public static GatewayReservation WithID(Account account, long identifier) { return null; }
		public static GatewayReservation Get(Account account, long identifier) { return null; }
		public static GatewayReservation[] GetOverlapping(Account account, long fromIdentifier, long toIdentifier) { return null; }
	}

	public class Campaign: DataItem
	{
		public static class ColumnNames
		{
			public const string GK = "Campaign_GK";
			public const string OriginalID = "campaignid";
			public const string AccountID = "Account_ID";
			public const string ChannelID = "Channel_ID";
			public const string Name = "campaign";
			public const string Status = "campStatus";
			public const string LastUpdated = "LastUpdated";
		}

		public long ID;
		public long OriginalID;
		public Account Account;
		public Channel Channel;
		public string Name;
		public string Status;

		public static Campaign WithID(long ID) { return null; }
		public static Campaign Get(long ID) { return null; }
		public static Campaign[] Get(Account account, Channel channel) { return null; }
	}

	public class CampaignTargetCollection: DataItemCollection
	{
		public Campaign Campaign;
	}

	public class CampaignTarget: DataItem
	{
		public DateTime TargetMonth;
		public double Cost;
		public int Clicks;
		public int Conversions;
	}

	public class Adgroup: DataItem
	{
		public static class ColumnNames
		{
			public const string GK = "Adgroup_GK";
			public const string OriginalID = "adgroupID";
			public const string AccountID = "Account_ID";
			public const string ChannelID = "Channel_ID";
			public const string CampaignGK = "Campaign_GK";
			public const string Name = "adgroup";
			public const string Status = "agStatus";
			public const string LastUpdated = "LastUpdated";
		}

		public long ID;
		public long OriginalID;
		public Account Account;
		public Channel Channel;
		public Campaign Campaign;
		public string Name;
		public string Status;

		public static Adgroup WithID(long ID) { return null; }
		public static Adgroup Get(long ID) { return null; }
		public static Adgroup[] Get(Account account, Channel channel) { return null; }
		public static Adgroup[] Get(Creative creative) { return null; }
		public static Adgroup[] Get(Keyword keyword) { return null; }
	}

	public class AdgroupKeywordCollection: DataItemCollection
	{
		public Adgroup Adgroup;
	}

	public class AdgroupKeyword: DataItem
	{
		public static class ColumnNames
		{
			public const string GK = "PPC_Keyword_GK";
			public const string AccountID = "Account_ID";
			public const string ChannelID = "Channel_ID";
			public const string CampaignGK = "Campaign_GK";
			public const string AdgroupGK = "AdGroup_GK";
			public const string KeywordGK = "Keyword_GK";
			public const string GatewayGK = "Gateway_GK";
			public const string MatchType = "MatchType";
			public const string DestUrl = "kwDestUrl";
			public const string Status = "siteKwStatus";
			public const string LastUpdated = "LastUpdated";
		}

		public long ID;
		public Keyword Keyword;
		public Channel Channel;
		public Campaign Campaign;
		public Adgroup Adgroup;
		public Page Page;
		public Adunit Adunit;
		public Gateway Gateway;
		public string Status;
		public string DestinationUrl;

		public static AdgroupKeyword WithID(long id) { return null; }
		public static AdgroupKeyword Get(long id) { return null; }
	}

	public class AdgroupCreativeCollection: DataItemCollection
	{
		public Adgroup Adgroup;
	}

	public class AdgroupCreative: DataItem
	{
		public static class ColumnNames
		{
			public const string GK = "PPC_Creative_GK";
			public const string AccountID = "Account_ID";
			public const string ChannelID = "Channel_ID";
			public const string CampaignGK = "Campaign_GK";
			public const string AdgroupGK = "AdGroup_GK";
			public const string CreativeGK = "Creative_GK";
			public const string GatewayGK = "Gateway_GK";
			public const string DestUrl = "creativeDestUrl";
			public const string VisibleUrl = "creativeVisUrl";
			public const string Status = "creativeStatus";
			public const string LastUpdated = "LastUpdated";
		}

		public long ID;
		public Creative Creative;
		public Channel Channel;
		public Campaign Campaign;
		public Adgroup Adgroup;
		public Page Page;
		public Adunit Adunit;
		public Gateway Gateway;
		public string Status;

		public static AdgroupCreative WithID(long id) { return null; }
		public static AdgroupCreative Get(long id) { return null; }
	}

	public class AdgroupSite: DataItem
	{
		public static class ColumnNames
		{
			public const string GK = "PPC_Site_GK";
			public const string AccountID = "Account_ID";
			public const string ChannelID = "Channel_ID";
			public const string CampaignGK = "Campaign_GK";
			public const string AdgroupGK = "AdGroup_GK";
			public const string SiteGK = "Site_GK";
			public const string GatewayGK = "Gateway_GK";
			public const string DestUrl = "kwDestUrl";
			public const string MatchType = "MatchType";
		}
	}

	public class User: DataItem
	{
		public int ID;
		public string Name;
		public bool IsActive;
		public bool IsAccountAdministrator;
		public bool IsUserAdministrator;
		public Email Email;
		public string Password;

		public static User WithID(int id) { return null; }
		public static User Get(int id) { return null; }
		public static User Get(Email email) { return null; }
		public static User Get() { return null; }
		public static User[] Get(UserGroup group) { return null; }
	}

	public class UserGroup: DataItem
	{
		public int ID;
		public string Name;
		public bool IsActive;
		public bool IsAccountAdministrator;
		public bool IsUserAdministrator;

		public static UserGroup WithID(int id) { return null; }
		public static UserGroup Get(int id) { return null; }
		public static UserGroup[] Get() { return null; }
	}

	public class AccountPermission: DataItem
	{
	}

	public class RankerProfile: DataItem
	{
	}

	public class RankerProfileKeyword: DataItem
	{
	}

	public class RankerProfileDomain: DataItem
	{
	}

	public class RankerProfileDomainGroup: DataItem
	{
	}

	public class SerpProfileSearchEngine: DataItem
	{
	}
}
