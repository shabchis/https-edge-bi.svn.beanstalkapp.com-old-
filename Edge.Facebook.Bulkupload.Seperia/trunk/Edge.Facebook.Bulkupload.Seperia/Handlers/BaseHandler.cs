using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace Edge.Facebook.Bulkupload.Handlers
{
	public abstract class BaseHandler : IHttpHandler 
	{
		#region IHttpHandler Members
		
		public virtual bool IsReusable
		{
			get { return true; }
		}

		public abstract void ProcessRequest(HttpContext context);

	

		#endregion

	}
}