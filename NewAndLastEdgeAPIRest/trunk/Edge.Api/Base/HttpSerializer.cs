using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace EdgeApiRest
{
	public class HttpSerializer
	{
		public static Dictionary<string, IHttpSerializer> Serializers;
		static HttpSerializer()
		{
			Serializers = new Dictionary<string, IHttpSerializer>();
			Serializers.Add("application/json", new JsonSerializer());
		}

		public static void SerializeValue(HttpContext context, object value)
		{
			// TODO: clean up content type
			string contentType = context.Request.Headers["accept-type"];

			IHttpSerializer serializer = GetOrThrow(contentType);
			serializer.SerializeValue(contentType, context.Response.OutputStream, value);
		}

		public static object DeserializeValue(HttpContext context, Type type)
		{
			// TODO: clean up content type
			string contentType = context.Request.Headers["content-type"];
			IHttpSerializer serializer = GetOrThrow(contentType);
			object val = serializer.DeserializeValue(contentType, context.Request.InputStream, type);
			return val;
		}

		private static IHttpSerializer GetOrThrow(string contentType)
		{
			IHttpSerializer serializer;
			if (!Serializers.TryGetValue(contentType, out serializer))
				throw new HttpSerializationException(contentType);
			return serializer;
		}

	}

	public interface IHttpSerializer
	{
		object DeserializeValue(string contentType, System.IO.Stream stream, Type type);
		void SerializeValue(string contentType, System.IO.Stream stream, object value);
	}


	[Serializable]
	public class HttpSerializationException : Exception
	{
		public HttpSerializationException(string contentType) : this("Cannot serialize/deserialize this content type.", contentType) { }
		public HttpSerializationException(string message, string contentType) : base(message + "\nContent type: " + contentType) { }
		protected HttpSerializationException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
