using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace EdgeApiRest
{
	/// <summary>
	/// 
	/// </summary>
	public class ObjectQueryHandler : BaseHandler
	{
		public override void ProcessRequest(HttpContext context)
		{
			ObjectQueryInfo info = new ObjectQueryInfo(context.Request.Path, context.Request.QueryString);
			SetResponse(context, System.Net.HttpStatusCode.OK, info);
		}
	}


}