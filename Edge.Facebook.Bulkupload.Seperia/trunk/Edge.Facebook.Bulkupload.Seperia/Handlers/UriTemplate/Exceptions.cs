using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Edge.Facebook.Bulkupload.Base;
using System.Net;

namespace Edge.Facebook.Bulkupload.Handlers.UriTemplate
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