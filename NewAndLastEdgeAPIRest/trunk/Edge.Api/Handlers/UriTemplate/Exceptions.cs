using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;

namespace Edge.Api.Handlers.Template
{
	[Serializable]
	public class UriTemplateException : HttpStatusException
	{
		public UriTemplateException(string paramName, HttpStatusCode httpStatusCode) :
			this("Parameter mismatch.", paramName, httpStatusCode)
		{
		}

		public UriTemplateException(string message, string paramName, HttpStatusCode httpStatusCode) :
			base(message + "\nParameter: " + paramName, httpStatusCode)
		{
		}
	}
}