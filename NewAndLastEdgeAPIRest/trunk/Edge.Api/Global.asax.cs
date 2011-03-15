using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace Edge.Api
{
	public class Global : System.Web.HttpApplication
	{
		public static readonly bool AllowDefaultErrors;// = AppSettings.Get(typeof(Global), "AllowDefaultErrors", false) == "true";

		protected void Application_Start(object sender, EventArgs e)
		{

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
			if (AllowDefaultErrors)
				return;

			Exception ex = Server.GetLastError();
			Server.ClearError();
			
			HttpManager.SetResponse(this.Context, System.Net.HttpStatusCode.Forbidden, ex);
			//Response.End();
		}

		protected void Session_End(object sender, EventArgs e)
		{

		}

		protected void Application_End(object sender, EventArgs e)
		{

		}
	}
}