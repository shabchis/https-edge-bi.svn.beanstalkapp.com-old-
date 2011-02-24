using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Data;
using System.Text.RegularExpressions;

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
			/*
			using (DataManager.Current.OpenConnection())
			{
				// SELECT GatewayBaseUrl FROM User_GUI_Account where
			}
			*/
			throw new NotImplementedException();
		}
	}
}
