using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EdgeApiRest.Handlers.UriTemplate
{
	[Serializable]
	public class UriTemplateException : Exception
	{
		public UriTemplateException(string paramName):this("Parameter mismatch.", paramName) { }
		public UriTemplateException(string message, string paramName) : base(message + "\nParameter: " + paramName) { }
		protected UriTemplateException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

}