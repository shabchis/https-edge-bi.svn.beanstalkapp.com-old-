using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Edge.Api.Base;
using System.Net;

namespace Edge.Api
{
	public static class HttpManager
	{
		public static Dictionary<string, IHttpSerializer> Serializers = new Dictionary<string, IHttpSerializer>();
		public static readonly Dictionary<HttpStatusCode, string> StatusDescriptions = new Dictionary<HttpStatusCode, string>();

		static HttpManager()
		{
			Serializers.Add("application/json", new JsonSerializer());

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

		public static void SetResponse(HttpContext context, HttpStatusCode status, string message = null, string contentType = null)
		{
			// Set status if not set yet
			context.Response.TrySkipIisCustomErrors = true;
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

		public static void SetResponse(HttpContext context, HttpStatusCode status, object jsonObject, string contentType = "application/json")
		{
			context.Response.TrySkipIisCustomErrors = true;
			SetResponse(context: context, status: status, contentType: contentType);
			SerializeToOutput(context, jsonObject);
		}

		private static void SerializeToOutput(HttpContext context, object value)
		{
			// TODO: clean up content type
			string contentType = context.Request.Headers["Accept"];			
			IHttpSerializer serializer = GetOrThrow(contentType, HttpStatusCode.NotAcceptable);
			serializer.SerializeValue(contentType, context.Response.OutputStream, value);
		}

		public static object DeserializeFromInput(HttpContext context, Type type)
		{
			// TODO: clean up content type
			string contentType = context.Request.Headers["content-type"];
			IHttpSerializer serializer = GetOrThrow(contentType, HttpStatusCode.UnsupportedMediaType);
			object val = serializer.DeserializeValue(contentType, context.Request.InputStream, type);
			return val;
		}

		private static IHttpSerializer GetOrThrow(string contentType, HttpStatusCode status)
		{
			IHttpSerializer serializer;
			if (!Serializers.TryGetValue(contentType, out serializer))
				throw new HttpSerializationException(contentType, status);
			return serializer;
		}

	}

	public interface IHttpSerializer
	{
		object DeserializeValue(string contentType, System.IO.Stream stream, Type type);
		void SerializeValue(string contentType, System.IO.Stream stream, object value);
	}


	public class HttpStatusException : Exception
	{
		public HttpStatusCode StatusCode;

		public HttpStatusException(string message, HttpStatusCode httpStatusCode): base(message)
		{
			StatusCode = httpStatusCode;
		}

	}

	public class HttpSerializationException : HttpStatusException
	{
		public HttpSerializationException(string contentType, HttpStatusCode httpStatusCode) :
			this("Cannot serialize/deserialize this content type.", contentType, httpStatusCode)
		{
		}

		public HttpSerializationException(string message, string contentType, HttpStatusCode httpStatusCode) :
			base(message + "\nContent type: " + contentType, httpStatusCode)
		{
		}
	}
}
