using System;
using System.Xml;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Scheduling;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using Easynet.Edge.Core;
using Easynet.Edge.Services.DataRetrieval;
using System.Text.RegularExpressions;
using System.Web;
using Easynet.Edge.Services.DataRetrieval.DataReader;

namespace Easynet.Edge.Services.DataRetrieval
{
	public class OrganicProcessorService : ProcessorService //Easynet.Edge.Services.DataRetrieval.ProcessorService
	{
		class RetrievedFile
		{
			public readonly string Path;
			public readonly DateTime DateTime;
			public readonly SettingsCollection Parameters;

			public RetrievedFile(string path, DateTime dateTime, string parameters)
			{
				Path = path;
				DateTime = dateTime;
				Parameters = new SettingsCollection(parameters);
			}
		}

		class SearchEngine
		{
			public readonly int ID;
			public readonly int ChannelID;

			public SearchEngine(int id, int channelID)
			{
				ID = id;
				ChannelID = channelID;
			}
		}

		class ProfileDomain
		{
			public readonly int ID;
			public readonly string DomainFilter;
			public readonly int ProfileID;
			public readonly int GroupID;

			public ProfileDomain(int id, int profileID, int groupID, string filter)
			{
				ID = id;
				ProfileID = profileID;
				GroupID = groupID;

				if (filter == null)
				{
					DomainFilter = null;
				}
				else
				{
					// Strip spaces
					DomainFilter = filter.Trim();

					const string prefix = "://";
					int prefixIndex = DomainFilter.IndexOf(prefix);

					// Strip prefixes
					if (prefixIndex >= 0)
						DomainFilter = DomainFilter.Substring(prefixIndex + prefix.Length);

					// Strip last separators
					if (DomainFilter.LastIndexOf('/') == DomainFilter.Length-1)
						DomainFilter = DomainFilter.Substring(0, DomainFilter.Length-1);
				}
			}
		}

		// Initalize insert command
		SqlCommand _insertCommand = DataManager.CreateCommand(@"
                INSERT INTO Rankings_Data
                (
                    Account_ID,                 
                    Channel_ID,
                    Day_Code,
					ProcessorInstanceID,
					DateTime,
					ProfileID,
					SearchEngineID,
					KeywordGK,
					DomainGroupID,
					DomainID,
					DomainFilter,
					Rank,
					Url,
					Title,
					Description
                )
                VALUES
                (
                    @accountID:Int,                 
                    @channelID:Int,
                    @dayCode:Int,
					@processorInstanceID:Int,
					@dateTime:DateTime,
					@profileID:Int,
					@searchEngineID:Int,
					@keywordGK:BigInt,
					@domainGroupID:Int,
					@domainID:Int,
					@domainFilter:NVarChar,
					@rank:Int,
					@url:NVarChar,
					@title:NVarChar,
					@description:NVarChar
                )");


		protected override void OnInit()
		{
			base.OnInit();
			DataManager.ConnectionString = AppSettings.GetAbsolute(@"Easynet.Edge.Services.DataRetrieval.Organic.Connection.String");
			_insertCommand.Parameters["@accountID"].Value = Instance.AccountID;
		} 

		protected override ServiceOutcome DoWork()
		{
			// go over each file
				// get a list of domains to search for
				// for every OrganicRankingsRow, check if matches domain
					// insert to Rankings_Data

			// The reader type is required for processing; if it is not available to GetType then an exception will 
			// trigger a service failure
			Type organicReaderType = Type.GetType(Instance.Configuration.Options["ReaderType"], true);

			// ----------------------------------------
			// TODO: Delete old data if necessary

			// ----------------------------------------
			// Get the files list
			List<RetrievedFile> files = new List<RetrievedFile>();
			SqlCommand retrievedFileCmd = DataManager.CreateCommand(
				@"
					select RetrieveDate, Path, Parameters
					from RetrievedFiles
					where ParentInstanceID = @parentInstanceID:Int
				"
			);
			retrievedFileCmd.Parameters["@parentInstanceID"].Value = this.TargetParentInstanceID;

			using (SqlConnection sourceCn = new SqlConnection(AppSettings.Get(this, "SourceConnectionString")))
			{
				sourceCn.Open();
				retrievedFileCmd.Connection = sourceCn;

				using (SqlDataReader reader = retrievedFileCmd.ExecuteReader())
				{
					while (reader.Read())
						files.Add(new RetrievedFile(reader["Path"] as string, (DateTime) reader["RetrieveDate"], reader["Parameters"] as string));
				}
			}

			// ----------------------------------------
			// Get "search engines" - gl/hl combinations

			Dictionary<string, SearchEngine> searchEngines = new Dictionary<string, SearchEngine>();
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand searchEngineCmd = DataManager.CreateCommand(
					@"
						select Search_Engine_ID, Channel_ID, GeoLocation, HostLanguage
						from Constant_SearchEngine
						where ServiceName = @serviceName:NvarChar
					"
				);
				searchEngineCmd.Parameters["@serviceName"].Value = this.Instance.ParentInstance.Configuration.Name;

				using (SqlDataReader reader = searchEngineCmd.ExecuteReader())
				{
					while (reader.Read())
					{
						// Key is gl-hl, e.g. us-en
						searchEngines.Add
						(
							String.Format("{0}-{1}", reader["GeoLocation"], reader["HostLanguage"]),
							new SearchEngine((int) reader["Search_Engine_ID"], (int) reader["Channel_ID"])
						);
					}
				}
			}

			// ----------------------------------------
			// Get profile data
			
			// Check if this is a profile-specific run.
			// The profileID is added to the query to ensure it is active
			int profileID = -1;
			if (Instance.ParentInstance.Configuration.Options.ContainsKey("ProfileID"))
			{
				if (!Int32.TryParse(Instance.ParentInstance.Configuration.Options["ProfileID"], out profileID))
				{
					// Invalid profile ID, report failure
					throw new ArgumentException(String.Format("'{0}' is an invalid profile ID.", Instance.ParentInstance.Configuration.Options["ProfileID"]));
				}
			}

			List<ProfileDomain> domains = new List<ProfileDomain>();
			Dictionary<long, List<int>> profilesPerKeyword;
			Dictionary<int, List<int>> profilesPerSearchEngine;

			// Get keywords & search engines per profile
			using (DataManager.Current.OpenConnection())
			{
				profilesPerKeyword = GetProfilesPerItem<long>(profileID, -1,
					@"
						select
							pk.Keyword_GK as ItemID, pk.Profile_ID as ProfileID
						from
							User_GUI_SerpProfileKeyword pk
							inner join User_GUI_SerpProfile p on p.Profile_ID = pk.Profile_ID {0}
						where
							pk.Account_ID = @accountID:Int
						group by
							pk.Keyword_GK, pk.Profile_ID
						order by
							pk.Keyword_GK, pk.Profile_ID
					"
				 );

				profilesPerSearchEngine = GetProfilesPerItem<int>(profileID, -1,
					@"
						select
							pse.Search_Engine_ID as ItemID, pse.Profile_ID as ProfileID
						from
							User_GUI_SerpProfileSearchEngine pse
							inner join User_GUI_SerpProfile p on p.Profile_ID = pse.Profile_ID {0}
						where
							pse.Account_ID = @accountID:Int
						group by
							pse.Search_Engine_ID, pse.Profile_ID
						order by
							pse.Search_Engine_ID, pse.Profile_ID
					"
				);

				// Get the domain list
				SqlCommand domainCmd = DataManager.CreateCommand(String.Format
				(
					@"
						select
							pd.Domain_ID, pd.Profile_ID, pd.Group_ID, pd.Domain
						from
							User_GUI_SerpProfileDomain pd
							inner join User_GUI_SerpProfile p on p.Profile_ID = pd.Profile_ID {0}
						where
							pd.Account_ID = @accountID:Int
					",
					profileID > -1 ? String.Format("and p.Profile_ID = {0}", profileID) : "and p.IsActive = 1"
				));
				domainCmd.Parameters["@accountID"].Value = Instance.AccountID;

				using (SqlDataReader reader = domainCmd.ExecuteReader())
				{
					while (reader.Read())
					{
						try
						{
							domains.Add(new ProfileDomain((int) reader["Domain_ID"], (int) reader["Profile_ID"], (int) reader["Group_ID"], (string) reader["Domain"]));
						}
						catch(Exception ex)
						{
							Log.Write(String.Format("Invalid domain {0}.", reader["Domain"]), ex);
						}
					}
				}

			}

			// Create objects for bulk insert
			DataTable insertBuffer = new DataTable();
			insertBuffer.Columns.Add("Account_ID");
			insertBuffer.Columns.Add("Channel_ID");
			insertBuffer.Columns.Add("Day_Code");
			insertBuffer.Columns.Add("ProcessorInstanceID");
			insertBuffer.Columns.Add("DateTime");
			insertBuffer.Columns.Add("ProfileID");
			insertBuffer.Columns.Add("SearchEngineID");
			insertBuffer.Columns.Add("KeywordGK");
			insertBuffer.Columns.Add("DomainGroupID");
			insertBuffer.Columns.Add("DomainID");
			insertBuffer.Columns.Add("DomainFilter");
			insertBuffer.Columns.Add("Rank");
			insertBuffer.Columns.Add("Url");
			insertBuffer.Columns.Add("Title");
			insertBuffer.Columns.Add("Description");

			using (DataManager.Current.OpenConnection())
			{
				using (SqlBulkCopy bulk = DataManager.CreateBulkCopy(SqlBulkCopyOptions.Default))
				{
					bulk.DestinationTableName = "Rankings_Data";

					// ----------------------------------------
					// Go over each file
					foreach (RetrievedFile file in files)
					{
						// Keyword
						long keywordGK;
						if (!long.TryParse(file.Parameters["KeywordGK"], out keywordGK) || !profilesPerKeyword.ContainsKey(keywordGK))
						{
							Log.Write
							(
								String.Format("Keyword GK {0} is not defined in any profile; skipping file {1}", keywordGK, file.Path),
								LogMessageType.Warning
							);
						}

						// Check which search engine we're working with
						SearchEngine searchEngine = null;
						if (!searchEngines.TryGetValue(String.Format("{0}-{1}", file.Parameters["GL"], file.Parameters["HL"]), out searchEngine))
						{
							Log.Write
							(
								String.Format("There is no search engine definition with GL = {0} and HL = {1} for the current service; skipping file {2}.", file.Parameters["GL"], file.Parameters["HL"], file.Path),
								LogMessageType.Warning
							);
							continue;
						}

						// Check which result we're starting with
						int currentRank = 1;
						if (file.Parameters.ContainsKey("Results"))
						{
							if (!Int32.TryParse(file.Parameters["Results"].Split('-')[0], out currentRank))
							{
								Log.Write(String.Format("Invalid rankings range \"{0}\" (Results parameter in Parameters field) so skipping file {1}.", file.Parameters["Results"], file.Path), LogMessageType.Warning);
								continue;
							}
						}

						using (SourceDataRowReader<OrganicRankingsRow> rankingsReader = (SourceDataRowReader<OrganicRankingsRow>) Activator.CreateInstance(organicReaderType, file.Path))
						{
							while (rankingsReader.Read())
							{
								// Check each domain to see if it fits
								foreach (ProfileDomain domain in domains)
								{
									if (!profilesPerKeyword[keywordGK].Contains(domain.ProfileID))
										continue;

									if (!profilesPerSearchEngine[searchEngine.ID].Contains(domain.ProfileID))
										continue;

									// Escape the value but unescape the wild card characters
									string regexFilter = Regex.Escape(domain.DomainFilter);
									regexFilter = "[(//)\\.]" + regexFilter + "[(//)\\.]";
									regexFilter = regexFilter.Replace(@"\*", ".*");

									// Check if we have a domain match
                                    //AP 12/7/09 - for some reason, there appears a situation where the URL is null - fix this
                                    if (rankingsReader.CurrentRow.Url != null)
                                    {
                                        if (!Regex.IsMatch(rankingsReader.CurrentRow.Url, regexFilter, RegexOptions.IgnoreCase))
                                            continue;
                                    }

									DataRow row = insertBuffer.NewRow();
									row["Account_ID"] = Instance.AccountID;
									row["Channel_ID"]= searchEngine.ChannelID;
									row["Day_Code"] = GetDayCode(file.DateTime);
									row["ProcessorInstanceID"] = Instance.InstanceID;
									row["DateTime"] = file.DateTime;
									row["ProfileID"] = domain.ProfileID;
									row["SearchEngineID"] = searchEngine.ID;
									row["KeywordGK"] = keywordGK;
									row["DomainGroupID"] = domain.GroupID;
									row["DomainID"] = domain.ID;
									row["DomainFilter"] = domain.DomainFilter;
									row["Rank"] = currentRank;
									row["Url"] = rankingsReader.CurrentRow.Url;
									row["Title"] = rankingsReader.CurrentRow.Title;
									row["Description"] = rankingsReader.CurrentRow.Description;
									insertBuffer.Rows.Add(row);

									// Look for profiles matching this domain
									//using (DataManager.Current.OpenConnection())
									//{
									//    DataManager.Current.AssociateCommands(_insertCommand);

									//    _insertCommand.Parameters["@channelID"].Value = searchEngine.ChannelID;
									//    _insertCommand.Parameters["@dayCode"].Value = DayCode(file.DateTime);
									//    _insertCommand.Parameters["@processorInstanceID"].Value = Instance.InstanceID;
									//    _insertCommand.Parameters["@dateTime"].Value = file.DateTime;
									//    _insertCommand.Parameters["@profileID"].Value = domain.ProfileID;
									//    _insertCommand.Parameters["@searchEngineID"].Value = searchEngine.ID;
									//    _insertCommand.Parameters["@keywordGK"].Value = keywordGK;
									//    _insertCommand.Parameters["@domainGroupID"].Value = domain.GroupID;
									//    _insertCommand.Parameters["@domainID"].Value = domain.ID;
									//    _insertCommand.Parameters["@domainFilter"].Value = domain.DomainFilter;
									//    _insertCommand.Parameters["@rank"].Value = currentRank;
									//    _insertCommand.Parameters["@url"].Value = rankingsReader.CurrentRow.Url;
									//    _insertCommand.Parameters["@title"].Value = rankingsReader.CurrentRow.Title;
									//    _insertCommand.Parameters["@description"].Value = rankingsReader.CurrentRow.Description;

									//    //try
									//    //{
									//        _insertCommand.ExecuteNonQuery();
									//    //}
									//    //catch(Exception ex)
									//    //{
									//    //	Log.Write(String.Format("Failed to add rank for keyword GK {0} in profile {1}", keywordGK, domain.ProfileID), ex);
									//    //}

									//}
								}

								// Increate rank counter
								currentRank++;
							}
						}
					}

					try
					{
						bulk.WriteToServer(insertBuffer);
					}
					catch(Exception ex)
					{
						Log.Write("Failed to insert rankings to database.", ex);
					}
				}
			}

			return ServiceOutcome.Success;
	
		}

		/// <summary>
		/// Creates a lookup table for a certain item's profiles
		/// </summary>
		Dictionary<KeyT, List<int>> GetProfilesPerItem<KeyT>(int profileID, KeyT emptyValue, string query)
		{
			Dictionary<KeyT, List<int>> profilesPerItem = new Dictionary<KeyT, List<int>>();

			// Add filter by profile if necessary
			SqlCommand cmd = DataManager.CreateCommand(String.Format(
				query,
				profileID > -1 ? String.Format("and p.Profile_ID = {0}", profileID) : "and p.IsActive = 1"
			));
			cmd.Parameters["@accountID"].Value = Instance.AccountID;

			KeyT prevItem = emptyValue;
			using (SqlDataReader reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					// Add a new profile list for every different keyword
					KeyT cutItem = (KeyT) reader["ItemID"];
					List<int> profiles;
					if (!cutItem.Equals(prevItem))
					{
						profiles = new List<int>();
						profilesPerItem.Add(cutItem, profiles);
						prevItem = cutItem;
					}
					else
					{
						profiles = profilesPerItem[prevItem];
					}

					// Add the profile 
					profiles.Add((int) reader["ProfileID"]);
				}
			}

			return profilesPerItem;
		}

	}
}


