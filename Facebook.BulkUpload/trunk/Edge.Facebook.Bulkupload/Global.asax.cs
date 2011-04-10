using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Edge.Facebook.Bulkupload.Base;

namespace Edge.Facebook.Bulkupload
{
	public class Global : System.Web.HttpApplication
	{
		public static readonly bool AllowDefaultErrors;
		void Application_Start(object sender, EventArgs e)
		{
			// Code that runs on application startup
			//Bulkupload.Objects.BulkFile.Path = Server.MapPath("~/Files");
			
		}

		void Application_End(object sender, EventArgs e)
		{
			//  Code that runs on application shutdown

		}

		void Application_Error(object sender, EventArgs e)
		{
			if (AllowDefaultErrors)
				return;

			Exception ex = Server.GetLastError();
			Server.ClearError();

			HttpManager.SetResponse(this.Context, System.Net.HttpStatusCode.Forbidden, ex);
			//Response.End();
			
		}

		void Session_Start(object sender, EventArgs e)
		{
			// Code that runs when a new session is started

		}

		void Session_End(object sender, EventArgs e)
		{
			// Code that runs when a session ends. 
			// Note: The Session_End event is raised only when the sessionstate mode
			// is set to InProc in the Web.config file. If session mode is set to StateServer 
			// or SQLServer, the event is not raised.

		}

	}
}
