using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Easynet.Edge.Core.Data;
using System.Configuration;
using System.IO;

namespace Easynet.Edge.UI.WebPages
{
	public class Global : System.Web.HttpApplication
	{

		protected void Application_Start(object sender, EventArgs e)
		{
			DataManager.ConnectionString = ConfigurationManager.ConnectionStrings["Easynet.Edge.UI.Data.Properties.Settings.easynet_OltpConnectionString"].ConnectionString;
		}

		protected void Session_Start(object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{

		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{

		}

		protected void Application_Error(object sender, EventArgs e)
		{

		}

		protected void Session_End(object sender, EventArgs e)
		{
			WebImporterPage.Cleanup(Session);
		}

		protected void Application_End(object sender, EventArgs e)
		{

		}
	}
}