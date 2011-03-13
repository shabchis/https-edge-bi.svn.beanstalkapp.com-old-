using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace EdgeApiRest
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

		#region Helper Methods

		public static readonly Dictionary<HttpStatusCode, string> StatusDescriptions;

		static BaseHandler()
		{
			
			StatusDescriptions = new Dictionary<HttpStatusCode, string>();

			StatusDescriptions.Add(HttpStatusCode.OK, "OK");
			StatusDescriptions.Add(HttpStatusCode.NoContent, "No Content");
			StatusDescriptions.Add(HttpStatusCode.PartialContent, "Partial Content");
			StatusDescriptions.Add(HttpStatusCode.Found, "Found");
			StatusDescriptions.Add(HttpStatusCode.NotModified, "Not Modified");
			StatusDescriptions.Add(HttpStatusCode.BadRequest, "Bad Request");
			StatusDescriptions.Add(HttpStatusCode.Forbidden, "Forbidden");
			StatusDescriptions.Add(HttpStatusCode.NotFound, "Not Found");
			StatusDescriptions.Add(HttpStatusCode.MethodNotAllowed, "Method Not Allowed");
			StatusDescriptions.Add(HttpStatusCode.InternalServerError, "Internal Server Error");
		}

		public void SetResponse(HttpContext context, HttpStatusCode status, string message = null, string contentType = null)
		{
			// Set status if not set yet
			context.Response.StatusCode = (int)status;
			context.Response.StatusDescription = StatusDescriptions.ContainsKey(status) ? StatusDescriptions[status] : null;
			context.Response.ContentType = contentType;
			context.Response.ContentEncoding = Encoding.UTF8;
			
			// Response body
			if (message != null)
			{
				context.Response.Write(message);
			}
		}

		public void SetResponse(HttpContext context, HttpStatusCode status, object jsonObject, string contentType = "application/json")
		{
			SetResponse(context: context, status: status, contentType: contentType);
			var serializer = JsonSerializer.Create(null);
			serializer.Serialize(context.Response.Output, jsonObject);
		}


		#endregion
	}
}