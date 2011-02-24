using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Data;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace Easynet.Edge.BusinessObjects
{
	public class Tracker
	{
		public static string ExtractTracker(string url, string regex)
		{
			foreach (Group g in Regex.Match(url, regex).Groups)
			{
				if (!g.Success)
					continue;

				return g.Value;
			}

			// nothing found
			return null;
		}

		public static string GetAccountTrackerPattern(int accountID)
		{
			string accountTrackerPattern = null;
			using (DataManager.Current.OpenConnection())
			{
				using (SqlCommand sqlCommand=DataManager.CreateCommand(@"SELECT GatewayBaseUrl 
																		FROM User_GUI_Account
																		WHERE Account_ID=@Account_ID:Int"))
				{
					sqlCommand.Parameters["@Account_ID"].Value = accountID;
					accountTrackerPattern = sqlCommand.ExecuteScalar().ToString();

								
				}
				// SELECT GatewayBaseUrl FROM User_GUI_Account where
			}

			return accountTrackerPattern;
		}
	}
}
