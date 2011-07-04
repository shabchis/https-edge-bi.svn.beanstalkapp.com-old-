using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Edge.Facebook.Bulkupload.Base;

namespace Edge.Facebook.Bulkupload.Handlers.ObjectQuery
{
	public class ObjectQueryHandler : BaseHandler
	{
		public override void ProcessRequest(HttpContext context)
		{
			ObjectQueryInfo info = new ObjectQueryInfo(context.Request.Path, context.Request.QueryString);
			HttpManager.SetResponse(context, System.Net.HttpStatusCode.OK, info);
		}
	}
}