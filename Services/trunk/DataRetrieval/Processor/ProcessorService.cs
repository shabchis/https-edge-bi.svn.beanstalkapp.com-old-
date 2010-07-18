using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
using Easynet.Edge.BusinessObjects;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Services.DataRetrieval.Configuration;
using Easynet.Edge.Services.DataRetrieval.DataReader;

namespace Easynet.Edge.Services.DataRetrieval
{	
	public class ProcessorService : BaseService
	{
		#region Consts		
		/*=========================*/

		//private const string FieldNullValue = "0";
		private const string StringFieldNullValue = "Unknown";
		private const string DefaultValue = "Default adgroup";
		private const int GatewayIdNullValue = -99;
		private const int MaxNumOFGkManagerCallsTries = 3;

		protected const long DBNullValue = -1;
		protected const int ChannelIDNull = -1;
	
		/*=========================*/
		#endregion

		#region Protected Mehtods
		/*=========================*/

		[Obsolete("TargetParentInstanceID is not fully implemented. Processors do not fully recognize a target instance's configuration.")]
		protected long TargetParentInstanceID
		{
			get
			{
				long id;
				if (Int64.TryParse(Instance.ParentInstance.Configuration.Options["TargetParentInstanceID"], out id))
					return id;
				else
					return Instance.ParentInstance.InstanceID;
			}
		}
		
		protected int GetChannelID(string channelName)
		{
			int channelID = -1;
			// Initalize select command to channel ID by channel name.
			SqlCommand selectCommand = DataManager.CreateCommand(@"
                SELECT Channel_ID 
					FROM  Constant_Channel
					WHERE Channel_Name = @channelName:NVarChar",
				CommandType.Text);

			using (DataManager.Current.OpenConnection())
			{
				// Initalize connetion.
				DataManager.Current.AssociateCommands(selectCommand);

				selectCommand.Parameters["@channelName"].Value = channelName;

				try
				{
					// Execute select command.
					channelID = (int)selectCommand.ExecuteScalar();
				}
				catch (Exception ex)
				{
					//Log.Write(string.Format("Error channelID for channel name {0}", channelName), ex);
					return channelID;
				}
			}
			return channelID;
		}

		protected void InitalizeMatchType(SqlCommand insertCommand, string matchType)
		{
			if (matchType.ToLower() == "broad" || matchType == string.Empty)
				insertCommand.Parameters["@MatchType"].Value = MatchType.Broad;
			else if (matchType.ToLower() == "content")
				insertCommand.Parameters["@MatchType"].Value = MatchType.Content;
			else if (matchType.ToLower() == "phrase")
				insertCommand.Parameters["@MatchType"].Value = MatchType.Phrase;
			else if (matchType.ToLower() == "exact")
				insertCommand.Parameters["@MatchType"].Value = MatchType.Exact;
			else if (matchType.ToLower() == "website")
				insertCommand.Parameters["@MatchType"].Value = MatchType.WebSite;
			else // Invalid match type name
				insertCommand.Parameters["@MatchType"].Value = MatchType.Unidentified;
		}

		/// <summary>
		/// Initalize Gateway_id by the value in the destUrl.
		/// </summary>
		/// <param name="insertCommand"></param>
		/// <param name="channelID"></param>
		/// <param name="gatewayNameFields"></param>
		protected void InitalizeGatewayID(SqlCommand insertCommand, int channelID, Dictionary<string, string> gatewayNameFields)
		{	
			try
			{
				string uri = insertCommand.Parameters["@destUrl"].Value.ToString().ToLower();
				string gateway = string.Empty;
				
				foreach (string gatewayName in gatewayNameFields.Keys)
				{
					if (uri.Contains(gatewayName.ToLower()))
					{
						// Check if we need to add channel ID to the gateway.
						if (Convert.ToBoolean(gatewayNameFields[gatewayName.ToLower()]))
							gateway = channelID.ToString() + uri.Substring(uri.IndexOf(gatewayName.ToLower()) + gatewayName.Length);
						else
							gateway = uri.Substring(uri.IndexOf(gatewayName.ToLower()) + gatewayName.Length);

						break;
					}					
				}		
				
				// if the gid isn't in the end we remove everything after it.
				if (gateway.Contains("&"))
					gateway = gateway.Substring(0,gateway.IndexOf("&"));
				
				insertCommand.Parameters["@Gateway"].Value = gateway;
				int gid = ResolveInt(gateway.Contains("pid=") ? gateway.Substring(0, gateway.IndexOf("pid=")) : gateway);

				//WID
				string widStr = uri.Substring(uri.IndexOf("wid=") + ("wid=").Length);
				int wid = ResolveInt(widStr.Contains("wid=") ? widStr.Substring(0, widStr.IndexOf("wid=")) : widStr);

				// Initalize Gateway_id
				insertCommand.Parameters["@Gateway_id"].Value = gid > 0 ? gid : wid;
			}
			catch
			{
				insertCommand.Parameters["@Gateway_id"].Value = GatewayIdNullValue;
				insertCommand.Parameters["@Gateway"].Value = string.Empty;
			}
		}	
			
		protected void HandleMatchType(SqlCommand insertCommand, SourceDataRowReader<RetrieverDataRow> reader)
		{
			// Initalize match type
			string matchType;

			if (reader.CurrentRow.Fields.ContainsKey("kwType"))
			{
				matchType = ResolveString(reader.CurrentRow.Fields["kwType"].ToString());
				insertCommand.Parameters["@kwSite"].Value = ResolveString(reader.CurrentRow.Fields["keyword"]);
				insertCommand.Parameters["@siteKwStatus"].Value = ResolveString(reader.CurrentRow.Fields["kwStatus"]);
			}
			else if (reader.CurrentRow.Fields.ContainsKey("kwSiteType"))
			{
				matchType = ResolveString(reader.CurrentRow.Fields["kwSiteType"]);
				insertCommand.Parameters["@kwSite"].Value = ResolveString(reader.CurrentRow.Fields["kwSite"]);
				insertCommand.Parameters["@siteKwStatus"].Value = ResolveString(reader.CurrentRow.Fields["siteKwStatus"]);
			}
			else if (insertCommand.Parameters["@MatchType"].Value.ToString() != DBNullValue.ToString()) // data from ppc importer
				matchType = insertCommand.Parameters["@MatchType"].Value.ToString();
			else
				matchType = string.Empty;		

			InitalizeMatchType(insertCommand, matchType);
		}

		protected void HandleMatchType(SqlCommand insertCommand, XmlTextReader xmlReader)
		{
			// Initalize match type
			string matchType;

			if (xmlReader.GetAttribute("kwType") != null)
			{
				matchType = ResolveString(xmlReader.GetAttribute("kwType").ToString());
				insertCommand.Parameters["@kwSite"].Value = ResolveString(xmlReader.GetAttribute("keyword"));
				insertCommand.Parameters["@siteKwStatus"].Value = ResolveString(xmlReader.GetAttribute("kwStatus"));
			}
			else if (xmlReader.GetAttribute("kwSiteType") != null)
			{
				matchType = ResolveString(xmlReader.GetAttribute("kwSiteType"));
				insertCommand.Parameters["@kwSite"].Value = ResolveString(xmlReader.GetAttribute("kwSite"));
				insertCommand.Parameters["@siteKwStatus"].Value = ResolveString(xmlReader.GetAttribute("siteKwStatus"));
			}
			else if (insertCommand.Parameters["@MatchType"].Value.ToString() != DBNullValue.ToString()) // data from ppc importer
				matchType = insertCommand.Parameters["@MatchType"].Value.ToString();
			else
				matchType = string.Empty;

			InitalizeMatchType(insertCommand, matchType);
		}

		protected void InitalizeAdVariation(SqlCommand insertCommand, string adVariation)
		{
			if (adVariation.ToLower() == "text")
				insertCommand.Parameters["@AdVariation"].Value = Convert.ToInt32(AdVariation.Text);
			else if (adVariation.ToLower() == "flash")
				insertCommand.Parameters["@AdVariation"].Value = Convert.ToInt32(AdVariation.Flash);
			else if (adVariation.ToLower() == "image")
				insertCommand.Parameters["@AdVariation"].Value = Convert.ToInt32(AdVariation.Image);
			else // Invalid AdVariation name
				insertCommand.Parameters["@AdVariation"].Value = Convert.ToInt32(AdVariation.Unidentified);
		}
	
		/*=========================*/
		#endregion

		#region Protected Virtual Mehtods
		/*=========================*/

		/// <summary>
		/// Check if the account has backOffice.
		/// </summary>
		/// <param name="accountID">Account ID to check if has backOffice.</param>
		/// <returns>True - the account has backOffice. False - the account doesn't have backOffice.</returns>
		protected virtual bool HasBackOffice(int accountID)
		{
			bool hasBackOffice = false;

			if (accountID == SystemAccountID)
				return false;

			// Initalize select command to get BackOffice status of the account.
			SqlCommand selectCommand = DataManager.CreateCommand(@"
				select BackOffice_Status
					from User_GUI_Account
					where Account_ID = @accountID:Int",
				CommandType.Text);

			using (DataManager.Current.OpenConnection())
			{
				DataManager.Current.AssociateCommands(selectCommand);
				selectCommand.Parameters["@accountID"].Value = accountID;

				try
				{
					// Execute select command.
					hasBackOffice = (bool)selectCommand.ExecuteScalar();
				}
				catch (Exception ex)
				{
					throw new Exception("Error get report data from table User_GUI_Account in oltp DB.", ex);
				}
			}
			return hasBackOffice;
		}

		protected virtual long FetchCampaignID(SqlCommand insertCommand)
		{
			long lVal = 0;
			try
			{
				if (insertCommand.Parameters["@campaignid"].Value != null &&
					insertCommand.Parameters["@campaignid"].Value.ToString() != string.Empty &&
					insertCommand.Parameters["@campaignid"].Value != DBNull.Value)
				{
					lVal = Convert.ToInt64(insertCommand.Parameters["@campaignid"].Value);
				}
			}
			catch (Exception ex)
			{
				throw new Exception(String.Format("can't convert campaignid {0}, to int. ", insertCommand.Parameters["@campaignid"].Value));
			}
			return lVal;
		}

		protected virtual long? GetAdGroupID(SqlCommand insertCommand)
		{
			long? originalID = Convert.IsDBNull(insertCommand.Parameters["@adgroupid"].Value) ? (long?)null :
					 (long?)Convert.ToInt64(insertCommand.Parameters["@adgroupid"].Value);
			return originalID;
		}

		/*=========================*/
		#endregion

		#region Initalize GK methods
		/*=========================*/

		protected void InitalizeKeywordGK(SqlCommand insertCommand, int accountID)
		{
			int gkManagerCallsCounter = 0;
			bool callSuccess = false;
			if (insertCommand.Parameters["@kwSite"].Value == DBNull.Value)
			{
				insertCommand.Parameters["@kwSite"].Value = StringFieldNullValue;
				insertCommand.Parameters["@Keyword_GK"].Value = 0;
				return;
			}

			// try 3 time to get KeywordGK from GKManager, if failed the trow Exception
			while (!callSuccess)
			{
				try
				{
					insertCommand.Parameters["@Keyword_GK"].Value = GkManager.GetKeywordGK(accountID,
							insertCommand.Parameters["@kwSite"].Value.ToString());

					callSuccess = true;
				}
				catch (TimeoutException ex)
				{
					++gkManagerCallsCounter;

					if (gkManagerCallsCounter > MaxNumOFGkManagerCallsTries)
						throw ex;				
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
		}

		protected void InitalizeCampaignGK(SqlCommand insertCommand, int channelID, int accountID)
		{
			int gkManagerCallsCounter = 0;
			bool callSuccess = false;

			if (insertCommand.Parameters["@campaign"].Value == DBNull.Value)
			{
				// Yaniv: add gatway to the log message
				//Log.Write(String.Format("campaign have no name.", insertCommand.Parameters["@campaignGK"].Value)
				//	, LogMessageType.Warning);

				insertCommand.Parameters["@campaign"].Value = StringFieldNullValue;
			}
			else
			{
				long lVal =  FetchCampaignID(insertCommand);

				// try 3 time to get campaignGK from GKManager, if failed the trow Exception
				while (!callSuccess)
				{
					try
					{
						// Have campaignid
						if (lVal != 0)
						{
							insertCommand.Parameters["@Campaign_GK"].Value = GkManager.GetCampaignGK(accountID,
							   channelID,
							   insertCommand.Parameters["@campaign"].Value.ToString(),
							   lVal);
						}
						else // Don't have campaignid
						{
							insertCommand.Parameters["@Campaign_GK"].Value = GkManager.GetCampaignGK(accountID,
							   channelID,
							   insertCommand.Parameters["@campaign"].Value.ToString(),
							   null);
						}
						callSuccess = true;

					}
					catch (TimeoutException ex)
					{
						++gkManagerCallsCounter;

						if (gkManagerCallsCounter > MaxNumOFGkManagerCallsTries)
							throw ex;
					}
					catch (Exception ex)
					{
						throw ex;
					}
				}
			}
		}

		protected void InitalizeAdGroupGK(SqlCommand insertCommand, int channelID, int accountID, long campaignGK)
		{
			int gkManagerCallsCounter = 0;
			bool callSuccess = false;

			if (String.IsNullOrEmpty(insertCommand.Parameters["@adgroup"].Value.ToString()) ||
				insertCommand.Parameters["@adgroup"].Value == DBNull.Value)
			{
				// If adgroup don't exist we initalize it with campaign name.
				insertCommand.Parameters["@adgroup"].Value = insertCommand.Parameters["@campaign"].Value;
			}

			if (insertCommand.Parameters["@adgroup"].Value.ToString() == StringFieldNullValue)
			{
				insertCommand.Parameters["@AdGroup_GK"].Value = 0;
				return;
			}

			// try 3 time to get campaignGK from GKManager, if failed the trow Exception
			while (!callSuccess)
			{
				try
				{
					long? originalID = GetAdGroupID(insertCommand);

					insertCommand.Parameters["@AdGroup_GK"].Value = GkManager.GetAdgroupGK(accountID,
					   channelID,
					   campaignGK,
					   insertCommand.Parameters["@adgroup"].Value.ToString(),
					   originalID);

					callSuccess = true;
				}
				catch (TimeoutException ex)
				{
					++gkManagerCallsCounter;

					if (gkManagerCallsCounter > MaxNumOFGkManagerCallsTries)
						throw ex;
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
		}

		protected void InitalizeCreativeGK(SqlCommand insertCommand, int accountID)
		{
			int gkManagerCallsCounter = 0;
			bool callSuccess = false;

			// Creative_gk
			if ((insertCommand.Parameters["@headline"].Value == DBNull.Value) &&
				(insertCommand.Parameters["@desc1"].Value == DBNull.Value) &&
				(insertCommand.Parameters["@desc2"].Value == DBNull.Value))
			{
				// Yaniv: report to log.
				insertCommand.Parameters["@headline"].Value = StringFieldNullValue;
				insertCommand.Parameters["@desc1"].Value = StringFieldNullValue;
				insertCommand.Parameters["@desc2"].Value = StringFieldNullValue;
				insertCommand.Parameters["@Creative_gk"].Value = 0;
				return;
			}

			// Convert DBNull to null
			if (insertCommand.Parameters["@headline"].Value == DBNull.Value)
				insertCommand.Parameters["@headline"].Value = string.Empty;
	
			if (insertCommand.Parameters["@desc1"].Value == DBNull.Value)
				insertCommand.Parameters["@desc1"].Value = string.Empty;
		
			if (insertCommand.Parameters["@desc2"].Value == DBNull.Value)
				insertCommand.Parameters["@desc2"].Value = string.Empty;
			
			// try 3 time to get campaignGK from GKManager, if failed the trow Exception
			while (!callSuccess)
			{
				try
				{
					insertCommand.Parameters["@Creative_gk"].Value =
						GkManager.GetCreativeGK(accountID,
							insertCommand.Parameters["@headline"].Value.ToString(),
							insertCommand.Parameters["@desc1"].Value.ToString(),
							insertCommand.Parameters["@desc2"].Value.ToString());

					callSuccess = true;
				}
				catch (TimeoutException ex)
				{
					++gkManagerCallsCounter;

					if (gkManagerCallsCounter > MaxNumOFGkManagerCallsTries)
						throw ex;
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
		}

		protected void InitalizeGatewayGK(SqlCommand insertCommand, int accountID, GatewayReferenceType gatewayReferenceType, int channelID)
		{
			int gkManagerCallsCounter = 0;
			bool callSuccess = false;

			// try 3 time to get campaignGK from GKManager, if failed the trow Exception
			while (!callSuccess)
			{
				try
				{
					if (insertCommand.Parameters["@Gateway_id"].Value == DBNull.Value)
						return;

					// Convert DBNull to null
					if (insertCommand.Parameters["@destUrl"].Value == DBNull.Value)
						insertCommand.Parameters["@destUrl"].Value = string.Empty;

					if (insertCommand.Parameters["@headline"].Value == DBNull.Value)
						insertCommand.Parameters["@headline"].Value = string.Empty;				
				
					insertCommand.Parameters["@Gateway_gk"].Value =
						 GkManager.GetGatewayGK(accountID,
						 (long)Convert.ToInt32(insertCommand.Parameters["@Gateway_id"].Value),
						 channelID,
						 (long)Convert.ToInt32(insertCommand.Parameters["@Campaign_GK"].Value),
						 (long)Convert.ToInt32(insertCommand.Parameters["@AdGroup_GK"].Value),
						 gatewayReferenceType == GatewayReferenceType.Keyword ?
							 "KW: " + insertCommand.Parameters["@kwSite"].Value.ToString() :
							 insertCommand.Parameters["@headline"].Value.ToString(),
						 insertCommand.Parameters["@destUrl"].Value.ToString(),
						 gatewayReferenceType,
						 gatewayReferenceType == GatewayReferenceType.Keyword ?
							(long)Convert.ToInt32(insertCommand.Parameters["@Keyword_GK"].Value) :
							(long)Convert.ToInt32(insertCommand.Parameters["@Creative_gk"].Value));

					callSuccess = true;
				}
				catch (TimeoutException ex)
				{
					++gkManagerCallsCounter;

					if (gkManagerCallsCounter > MaxNumOFGkManagerCallsTries)
						throw ex;
					
				}
				catch (Exception ex)
				{
					//Log.Write(String.Format("GkManager.GetGatewayGK failed."), ex, LogMessageType.Error);
					throw ex;
				}
			}

		}

		protected void InitalizeAdgroupCreativeGK(SqlCommand insertCommand, int accountID, int channelID, bool hasBackOffice)
		{
			int gkManagerCallsCounter = 0;
			bool callSuccess = false;

			// try 3 time to get campaignGK from GKManager, if failed the trow Exception
			while (!callSuccess)
			{
				try
				{
					if (insertCommand.Parameters["@Creative_gk"].Value.ToString() == string.Empty || 
						insertCommand.Parameters["@Creative_gk"].Value == DBNull.Value)
					{
						return;
					}

					// Convert DBNull to null
					if (insertCommand.Parameters["@creativeVisUrl"].Value == DBNull.Value)
						insertCommand.Parameters["@creativeVisUrl"].Value = string.Empty;					
			
					insertCommand.Parameters["@PPC_Creative_GK"].Value =
						GkManager.GetAdgroupCreativeGK(accountID,
							channelID,
							(long)Convert.ToInt32(insertCommand.Parameters["@Campaign_GK"].Value),
							(long)Convert.ToInt32(insertCommand.Parameters["@AdGroup_GK"].Value),
							(long)Convert.ToInt32(insertCommand.Parameters["@Creative_gk"].Value),
							insertCommand.Parameters["@destUrl"].Value.ToString(),
							insertCommand.Parameters["@creativeVisUrl"].Value.ToString(),
							hasBackOffice ? (long?)Convert.ToInt32(insertCommand.Parameters["@Gateway_gk"].Value) : null);
					
					callSuccess = true;
				}
				catch (TimeoutException ex)
				{
					++gkManagerCallsCounter;

					if (gkManagerCallsCounter > MaxNumOFGkManagerCallsTries)
						throw ex;
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
		}

		protected void InitalizeAdgroupKeywordGK(SqlCommand insertCommand, int accountID, int channelID, bool hasBackOffice)
		{
			int gkManagerCallsCounter = 0;
			bool callSuccess = false;

			// try 3 time to get campaignGK from GKManager, if failed the trow Exception
			while (!callSuccess)
			{
				try
				{
					if (insertCommand.Parameters["@Keyword_GK"].Value.ToString() == string.Empty ||
						insertCommand.Parameters["@Keyword_gk"].Value == DBNull.Value)
					{
						return;
					}

					// Convert DBNull to null
					if (insertCommand.Parameters["@MatchType"].Value == DBNull.Value)
						insertCommand.Parameters["@MatchType"].Value = MatchType.Unidentified;					
					
					insertCommand.Parameters["@PPC_Keyword_GK"].Value =
						GkManager.GetAdgroupKeywordGK(accountID,
							channelID,
							(long)Convert.ToInt32(insertCommand.Parameters["@Campaign_GK"].Value),
							(long)Convert.ToInt32(insertCommand.Parameters["@AdGroup_GK"].Value),
							(long)Convert.ToInt32(insertCommand.Parameters["@Keyword_GK"].Value),
							(MatchType)(insertCommand.Parameters["@MatchType"].Value),
							insertCommand.Parameters["@destUrl"].Value.ToString(),
							hasBackOffice ? (long?)Convert.ToInt32(insertCommand.Parameters["@Gateway_gk"].Value): null);
				
					//insertCommand.Parameters["@PPC_Keyword_GK"].Value =
					//    GkManager.GetAdgroupKeywordGK(accountID,
					//        channelID,
					//        (long)Convert.ToInt32(insertCommand.Parameters["@Campaign_GK"].Value),
					//        (long)Convert.ToInt32(insertCommand.Parameters["@AdGroup_GK"].Value),
					//        (long)Convert.ToInt32(insertCommand.Parameters["@Keyword_GK"].Value),
					//        (MatchType)(insertCommand.Parameters["@MatchType"].Value),
					//        insertCommand.Parameters["@kwDestUrl"].Value.ToString() == "defult url" ?
					//        null : insertCommand.Parameters["@kwDestUrl"].Value.ToString(),
					//        null);
											
					callSuccess = true;
				}
				catch (TimeoutException ex)
				{
					++gkManagerCallsCounter;

					if (gkManagerCallsCounter > MaxNumOFGkManagerCallsTries)
						throw ex;				
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
		}

		protected void InitalizePPCSiteGK(SqlCommand insertCommand, int channelID, int accountID, bool hasBackOffice)
		{
			int gkManagerCallsCounter = 0;
			bool callSuccess = false;

			if (insertCommand.Parameters["@site_GK"].Value.ToString() == string.Empty ||
				insertCommand.Parameters["@site_GK"].Value == DBNull.Value)
			{
				return;
			}		
			else
			{
				// try 3 time to get GetAdgroupSiteGK from GKManager, if failed the trow Exception
				while (!callSuccess)
				{
					try
					{
						// Initalize MatchType with unknwon value if it's empty.
						if (insertCommand.Parameters["@MatchType"].Value.ToString() == string.Empty)
							insertCommand.Parameters["@MatchType"].Value = MatchType.Unidentified;

						insertCommand.Parameters["@PPC_Site_gk"].Value = GkManager.GetAdgroupSiteGK(accountID,
							channelID,
							(long)insertCommand.Parameters["@Campaign_GK"].Value,
							(long)insertCommand.Parameters["@AdGroup_GK"].Value,
							(long)insertCommand.Parameters["@site_GK"].Value,
							insertCommand.Parameters["@site"].Value.ToString(),
							(MatchType)insertCommand.Parameters["@MatchType"].Value,
							hasBackOffice ? (long?)insertCommand.Parameters["@Gateway_gk"].Value : null);

						callSuccess = true;
					}
					catch (TimeoutException ex)
					{
						++gkManagerCallsCounter;

						if (gkManagerCallsCounter > MaxNumOFGkManagerCallsTries)
							throw ex;
					}
					catch (Exception ex)
					{
						throw ex;
					}
				}
			}
		}

		protected void InitalizeSiteGK(SqlCommand insertCommand, int accountID)
		{		
			int gkManagerCallsCounter = 0;
			bool callSuccess = false;

			if (insertCommand.Parameters["@site"].Value == DBNull.Value)
			{
				// Yaniv: add gatway to the log message
				Log.Write(String.Format("site have no name.", insertCommand.Parameters["@Campaign_GK"].Value)
					, LogMessageType.Warning);

				insertCommand.Parameters["@site"].Value = StringFieldNullValue;
			}
			else
			{
				string siteUrl = insertCommand.Parameters["@site"].Value.ToString();
				string url = string.Empty;

				siteUrl = GetCleanDomainName(siteUrl);
				
				try
				{
					Uri uri = new Uri(siteUrl);
					url = uri.Host;
				}
				catch (Exception ex)
				{
					url = insertCommand.Parameters["@site"].Value.ToString();
				}

				// try 3 time to get campaignGK from GKManager, if failed the trow Exception
				while (!callSuccess)
				{
					try
					{						
						insertCommand.Parameters["@site_GK"].Value = GkManager.GetSiteGK(accountID,
						   insertCommand.Parameters["@site"].Value.ToString());
						callSuccess = true;
					}
					catch (TimeoutException ex)
					{
						++gkManagerCallsCounter;

						if (gkManagerCallsCounter > MaxNumOFGkManagerCallsTries)
							throw ex;						
					}
					catch (Exception ex)
					{
						throw ex;
					}
				}
			}
		}

		/*=========================*/
		#endregion
	}	
}
