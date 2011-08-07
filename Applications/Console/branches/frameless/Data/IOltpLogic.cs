using System.Data;
using System.ServiceModel;
using Easynet.Edge.UI.Data;
using Easynet.Edge.Core.Services;

namespace Easynet.Edge.UI.Server
{
	/// <summary>
	/// 
	/// </summary>
	[ServiceContract(SessionMode = SessionMode.Required)]
	public interface IOltpLogic
	{
		#region Users
		//=========================

		[OperationContract(IsInitiating = true)] [NetDataContract]
		Oltp.UserDataTable User_LoginBySessionID(string sessionID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.UserDataTable User_GetByGroup(int groupID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.UserDataTable User_GetUsersWithPermissions(int accountID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.UserDataTable User_GetUsersWithoutPermissions(int accountID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		DataTable User_GetAllPermissions();

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.UserDataTable User_Save(Oltp.UserDataTable userTable);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.UserGroupDataTable UserGroup_Get(int groupID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.UserGroupDataTable UserGroup_Save(Oltp.UserGroupDataTable userGroupTable);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.UserGroupDataTable UserGroup_GetGroupsWithPermissions(int accountID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.UserGroupDataTable UserGroup_GetGroupsWithoutPermissions(int accountID);

		//=========================
		#endregion

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Measure[] Measure_Get(int accountID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		DataTable CampaignTargets_Get(int accountID, long? campaignGK);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		void CampaignTargets_Save(int accountID, DataTable table);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.ChannelDataTable Channel_Get();

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.CampaignStatusDataTable CampaignStatus_Get();

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.SearchEngineDataTable SearchEngine_Get();

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.AccountDataTable Account_Get();

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.AccountDataTable Account_Save(Oltp.AccountDataTable accountTable);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.AccountDataTable Account_GetByPermission(string permissionType);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.RelatedAccountDataTable RelatedAccount_Get(int accountID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		void RelatedAccount_Save(Oltp.RelatedAccountDataTable relatedAccountTable);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.GatewayDataTable Gateway_GetByIdentifier(int accountID, string identifier);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.GatewayDataTable Gateway_Get(int adunitID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.GatewayDataTable Gateway_GetGateways(int accountID, int? channelID, int?[] segments);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.GatewayDataTable Gateway_Save(Oltp.GatewayDataTable gatewayTable);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.GatewayDataTable Gateway_CreateRange(int fromID, int toID, int accountID, int adunitID, long pageGK, string destinationBaseUrl, int[] otherAccounts, bool returnTable, out int count);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.GatewayDataTable Gateway_CreateQuantity(int quantity, int accountID, int adunitID, long pageGK, string destinationBaseUrl, bool returnTable);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.GatewayDataTable Gateway_GetByReference(int refType, long refID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		int[] Gateway_BatchProperties(int accountID, long[][] ranges, int? channelID, long? pageGK, int?[] segments);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		int Gateway_CountByRange(int accountID, long fromID, long toID, int[] otherAccounts);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.GatewayReservationDataTable GatewayReservation_Get(int accountID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.GatewayReservationDataTable GatewayReservation_Save(Oltp.GatewayReservationDataTable reservationTable);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.GatewayReservationDataTable GatewayReservation_GetByPage(int accountID, long pageGK);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.GatewayReservationDataTable GatewayReservation_GetByIdentifier(int accountID, string identifier);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.GatewayReservationDataTable GatewayReservation_GetByOverlap(int accountID, string fromID, string toID, int[] otherAccounts);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.KeywordDataTable Keyword_Get(int accountID, bool includeRelated, string keywordFilter, bool includeUnmonitored);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.KeywordDataTable Keyword_GetSingle(long keywordGK);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.KeywordDataTable Keyword_FindForPhrases(int accountID, string[] phrases);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.KeywordDataTable Keyword_Save(Oltp.KeywordDataTable keywordTable);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.CreativeDataTable Creative_Get(int accountID, string keywordFilter, bool includeUnmonitored);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.CreativeDataTable Creative_GetSingle(long creativeGK);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.CreativeDataTable Creative_Save(Oltp.CreativeDataTable creativeTable);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.PageDataTable Page_Get(int accountID, string pageFilter, bool includeUnmonitored, long resultLimit);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.PageDataTable Page_Save(Oltp.PageDataTable pageTable);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.CampaignDataTable Campaign_Get(int accountID, int? channelID, int? statusID, string filter, bool filterByAdgroup);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.CampaignDataTable Campaign_GetIndividualCampaigns(long[] campaignGKs);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.CampaignDataTable Campaign_GetSingle(long campaignGK);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.CampaignDataTable Campaign_Save(Oltp.CampaignDataTable campaignTable, bool useBackOffice);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		void Campaign_Merge(int accountID, long targetCampaignGK, long[] otherCampaignGKs);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.AdgroupDataTable Adgroup_Get(long campaignGK, string filter);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.AdgroupDataTable Adgroup_GetSingle(long adgroupGK);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.AdgroupDataTable Adgroup_GetByKeyword(long keywordGK);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.AdgroupDataTable Adgroup_GetByCreative(long creativeGK);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.AdgroupDataTable Adgroup_Save(Oltp.AdgroupDataTable adgroupTable, bool useBackOffice);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.AdgroupKeywordDataTable AdgroupKeyword_Get(long adgroupGK);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.AdgroupKeywordDataTable AdgroupKeyword_Save(Oltp.AdgroupKeywordDataTable adgroupKeywordTable);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.AdgroupKeywordDataTable AdgroupKeyword_GetSingle(long adgroupKeywordGK);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.AdgroupCreativeDataTable AdgroupCreative_Get(long adgroupGK);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.AdgroupCreativeDataTable AdgroupCreative_Save(Oltp.AdgroupCreativeDataTable adgroupCreativeTable);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.AdgroupCreativeDataTable AdgroupCreative_GetSingle(long adgroupCreativeGK);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.AccountPermissionDataTable AccountPermission_Get(int accountID, int targetID, bool isGroup);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.AccountPermissionDataTable AccountPermission_Save(Oltp.AccountPermissionDataTable permissionTable);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		void AccountPermission_Copy(int sourceAccountID, int[] targetAccountID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		int AccountPermission_RemovePermissions(int accountID, int targetID, bool isGroup);

		//[OperationContract(IsInitiating = false)] [NetDataContract]
		//bool AccountPermission_CheckUserPermission(int accountID, int userID, string permissionType);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.SerpProfileDataTable SerpProfile_Get(int accountID, string profileType);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		bool SerpProfile_CanRunNow(int accountID, int profileID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		void SerpProfile_RunNow(int accountID, int profileID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.SerpProfileDataTable SerpProfile_Save(Oltp.SerpProfileDataTable profileTable);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.SerpProfileKeywordDataTable SerpProfileKeyword_Get(int profileID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		void SerpProfileKeyword_Save(Oltp.SerpProfileKeywordDataTable profileKeywordTable);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.SerpProfileDomainGroupDataTable SerpProfileDomainGroup_Get(int profileID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.SerpProfileDomainGroupDataTable SerpProfileDomainGroup_Save(Oltp.SerpProfileDomainGroupDataTable profileDomainGroupTable);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.SerpProfileDomainDataTable SerpProfileDomain_Get(int profileID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.SerpProfileDomainDataTable SerpProfileDomain_Save(Oltp.SerpProfileDomainDataTable profileDomainTable);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.SerpProfileSearchEngineDataTable SerpProfileSearchEngine_Get(int profileID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.SerpProfileSearchEngineDataTable SerpProfileSearchEngine_Save(Oltp.SerpProfileSearchEngineDataTable profileSearchEngineTable);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.SegmentDataTable Segment_Get(int accountID, bool includePages);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.SegmentDataTable Segment_Save(Oltp.SegmentDataTable table);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.SegmentValueDataTable SegmentValue_Get(int accountID, int segmentID);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		Oltp.SegmentValueDataTable SegmentValue_Save(Oltp.SegmentValueDataTable table);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		ApiMenuItem ApiMenuItem_GetByPath(string path);

		[OperationContract(IsInitiating = false)] [NetDataContract]
		ApiMenuItem[] ApiMenuItem_GetAll();
	}
}
