using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Easynet.Edge.UI.WebPages
{
	public class PageBase: Page
	{
		/// <summary>
		/// Gets the current account from the query string
		/// </summary>
		public int AccountID
		{
			get { return Int32.Parse(Request.QueryString["accountID"]); }
		}

	}
}
