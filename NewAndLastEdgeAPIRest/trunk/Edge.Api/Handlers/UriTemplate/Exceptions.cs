using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;

namespace Edge.Api.Handlers.Template
{
	[Serializable]
	public class TemplateException : HttpStatusException
	{
		public TemplateException(HttpStatusCode httpStatusCode,string paramName):this(httpStatusCode,"Parameter mismatch.", paramName) { }
		public TemplateException(HttpStatusCode httpStatusCode,string message, string paramName) : base(httpStatusCode, message + "\nParameter: " + paramName) { }
		//protected TemplateException(
		//  System.Runtime.Serialization.SerializationInfo info,
		//  System.Runtime.Serialization.StreamingContext context)
		//    : base(info, context) { }
	}
	[Serializable]
	public class HttpStatusException : Exception
	{
		public HttpStatusCode StatusCode;
		
		public HttpStatusException(HttpStatusCode httpStatusCode,string message) :base (message)
		{
			StatusCode = httpStatusCode;
			

		}
		//protected HttpStatusException(System.Runtime.Serialization.SerializationInfo info,
		//  System.Runtime.Serialization.StreamingContext context)
		//    : base(info, context) { }

	}

}