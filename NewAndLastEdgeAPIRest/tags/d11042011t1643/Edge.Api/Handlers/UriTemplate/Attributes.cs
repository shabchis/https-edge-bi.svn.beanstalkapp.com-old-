using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Text.RegularExpressions;

namespace Edge.Api.Handlers.Template
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class UriMappingAttribute : Attribute
	{
		public string Template { get; set; }
		public string Method { get; set; }
		public string BodyParameter { get; set; }

		public Regex Regex { get; set;  }
	}	
}