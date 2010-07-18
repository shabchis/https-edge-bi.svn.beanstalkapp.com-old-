using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Xml;
using Easynet.Edge.Core;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Scheduling;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Services.DataRetrieval;
using Easynet.Edge.Services.DataRetrieval.Configuration;
using System.IO;
using System.Web;

namespace Easynet.Edge.Services.DataRetrieval
{
	public class OrganicRetrieverService : Easynet.Edge.Services.DataRetrieval.Retriever.RetrieverService
	{
		class KeywordSettings
		{
			public readonly long GK;
			public readonly string Value;
			public readonly int TotalResults;
			public readonly List<string[]> Combinations = new List<string[]>();

			public KeywordSettings(long gk, string value, int totalResults)
			{
				GK = gk;
				Value = value;
				TotalResults = totalResults;
			}

			public override string ToString()
			{
				return Value;
			}
		}

		protected override void OnInit()
		{
			base.OnInit();
			DataManager.ConnectionString = AppSettings.GetAbsolute(@"Easynet.Edge.Services.DataRetrieval.Organic.Connection.String");
		}

		/// <summary>
		/// Main entry point of the service.
		/// </summary>
		/// <returns></returns>
		protected override ServiceOutcome DoWork()
		{
			// This is some crap added by Yaniv which needs to be called otherwise filenaming is screwed up
			InitalizeAccountID();

			// Threading mode
			bool multiThreaded = false;
			bool.TryParse(Instance.Configuration.Options["Multithreaded"], out multiThreaded);

			TimeSpan requestDelay;
			if (!TimeSpan.TryParse(Instance.Configuration.Options["DelayBetweenRequests"], out requestDelay))
				requestDelay = TimeSpan.FromSeconds(0);

			string urlFormat = Instance.Configuration.Options["UrlFormat"];
			if (urlFormat == null)
				throw new Exception("UrlFormat is required in the service's Options.");

			List<KeywordSettings> keywords = new List<KeywordSettings>();
			
			int profileID = -1;
			if (Instance.ParentInstance.Configuration.Options.ContainsKey("ProfileID"))
			{
				if (!Int32.TryParse(Instance.ParentInstance.Configuration.Options["ProfileID"], out profileID))
				{
					// Invalid profile ID, report failure
					throw new ArgumentException(String.Format("'{0}' is an invalid profile ID.", Instance.ParentInstance.Configuration.Options["ProfileID"]));
				}
			}

			int defaultTotalResults;
			int maxTotalResults;
			int resultsPerPage;
			int startOffset;

			if (!Int32.TryParse(Instance.Configuration.Options["DefaultTotalResults"], out defaultTotalResults))
				defaultTotalResults = 20;
			if (!Int32.TryParse(Instance.Configuration.Options["MaxTotalResults"], out maxTotalResults))
				maxTotalResults = 100;
			if (!Int32.TryParse(Instance.Configuration.Options["ResultsPerPage"], out resultsPerPage))
				resultsPerPage = 20;
			if (!Int32.TryParse(Instance.Configuration.Options["StartOffset"], out startOffset))
				startOffset = 0;

			using (DataManager.Current.OpenConnection())
			{
				// Make a table of every keyword matched up with all gl/hl combinations that need to be run for it
				SqlCommand seCmd = DataManager.CreateCommand(String.Format(@"
					select kw.Keyword_GK as GK, kw.Keyword as Value, pkw.TotalResults as TotalResults, cse.GeoLocation as GL, cse.HostLanguage as HL
					from
						User_GUI_SerpProfileKeyword pkw
						inner join User_GUI_SerpProfile p
							on p.Profile_ID = pkw.Profile_ID {0}
						inner join UserProcess_GUI_Keyword kw
							on	kw.Keyword_GK = pkw.Keyword_GK
						inner join User_GUI_SerpProfileSearchEngine pse
							on	pse.Profile_ID = pkw.Profile_ID
						inner join Constant_SearchEngine cse
							on	cse.ServiceName = @serviceName:NVarChar and
								cse.Search_Engine_ID = pse.Search_Engine_ID
					where
						pkw.Account_ID = @accountID:Int
					group by
						kw.Keyword_GK, kw.Keyword, pkw.TotalResults, cse.GeoLocation, cse.HostLanguage
					order by
						GK, Value, GL, HL
				", profileID > -1 ? String.Format("and p.Profile_ID = {0}", profileID) : "and p.IsActive = 1"));

				// Apply general parameters
           
				seCmd.Parameters["@accountID"].Value = Instance.AccountID;
				seCmd.Parameters["@serviceName"].Value = Instance.ParentInstance.Configuration.Name;				

				// Retrieve and add keywords
				using (SqlDataReader reader = seCmd.ExecuteReader())
				{
					KeywordSettings prevKwSettings = null;
					while (reader.Read())
					{
						// Either use the last kw settings (since the same kw can appear a few times in a row with different combinations)
						//	or create a new kw settings object
						long kwGK = (long) reader["GK"];
						KeywordSettings kwSettings;
						if (prevKwSettings != null && kwGK == prevKwSettings.GK)
						{
							kwSettings = prevKwSettings;
						}
						else
						{
							int kwTotalsResults = reader["TotalResults"] is int ?
								(int)reader["TotalResults"] :
								defaultTotalResults;
							if (kwTotalsResults < resultsPerPage)
							{
								kwTotalsResults = resultsPerPage;
							}
							else if (kwTotalsResults > maxTotalResults)
							{
								kwTotalsResults = maxTotalResults;
							}

							kwSettings = new KeywordSettings(kwGK, reader["Value"] as string, kwTotalsResults);
							keywords.Add(kwSettings);
							prevKwSettings = kwSettings;
						}

						kwSettings.Combinations.Add(new string[] { reader["GL"] as string, reader["HL"] as string });
					}
				}

			}

			// Counts how many retrievals completed
			int completedCount = 0;
			int failedCount = 0;
			
			string errorList = string.Empty;

			// Go over each keyword and each parameter combination for each keyword
			foreach (KeywordSettings keyword in keywords)
			{
				foreach(string[] combination in keyword.Combinations)
				{
					for (int s = 0; s <= keyword.TotalResults-resultsPerPage; s += resultsPerPage)
					{
						// Run retrievals on concurrent threads
						ParameterizedThreadStart threadStart = delegate(object p)
						{
							object[] values = (object[]) p;
							KeywordSettings kw = (KeywordSettings) values[0];
							string[] combo = (string[]) values[1];
							int start = (int) values[2];
							
							SettingsCollection parameters = new SettingsCollection(String.Format("KeywordGK: {0}; GL: {1}; HL: {2}; Results: {3}-{4}", kw.GK, combo[0], combo[1], start+1, start+resultsPerPage));

							try
							{
								const int numTries = 3;
								string filePath = null;

								// Try numTries times to retrieve the file
								for (int i = 0; i < numTries; i++)
								{
									try
									{
										filePath = WriteResultToFile(
											BuildURL(urlFormat, kw.Value, combo[0], combo[1], resultsPerPage, start+startOffset),
											DateTime.Now,
											String.Format("{0} [{1}] {2}-{3} {4:00}-{5:00}", PathEscape(kw.Value), kw.GK, combo[0], combo[1], start+1, start+resultsPerPage),
											false);

										break;
									}
									catch
									{
										if (i == numTries-1)
											throw;
										else
											Thread.Sleep(requestDelay);
									}
								}

								SaveFilePathToDB(this.Instance.ParentInstance.Configuration.Name, filePath, parameters);
							}
							catch (Exception ex)
							{
								errorList += String.Format("Error on {0} ({1}): {2} - {3}\n\n", kw.Value, kw.GK, ex.GetType().Name, ex.Message);
								failedCount++;
							}

							completedCount++;
						};

						Thread thread = new Thread(threadStart);
						thread.Start(new object[]{keyword, combination, s});

						if (!multiThreaded)
						{
							// When not multithreaded we have to wait for the request thread to finish
							thread.Join();
							
							if (requestDelay.TotalMilliseconds > 0)
								Thread.Sleep(requestDelay);
						}
					}
				}
			}

			// Check every 100 ms if the retrievals have completed
			while (completedCount < keywords.Count)
				Thread.Sleep(100);

			if (failedCount > 0 && failedCount == completedCount)
			{
				Log.Write(String.Format("All {0} requests failed.\n\n{1}", completedCount, errorList), LogMessageType.Error);
				return ServiceOutcome.Failure;
			}
			else
			{
				if (failedCount > 0)
					Log.Write(String.Format("{0} out of {1} requests failed.\n\n{2}", failedCount, completedCount, errorList), LogMessageType.Error);

				return ServiceOutcome.Success;
			}
		}

		protected override void OnEnded(ServiceOutcome outcome)
		{
			GC.Collect();
			base.OnEnded(outcome);
		}

        protected virtual string BuildURL(string urlFormat, string kwValue, string gl, string hl, int resultsPerPage, int start)
        {
            string url = String.Empty;
            url = String.Format(urlFormat, HttpUtility.UrlEncode(kwValue), gl, hl, resultsPerPage, start);
            return url;
        }
	}
}
