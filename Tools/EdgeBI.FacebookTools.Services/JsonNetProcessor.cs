using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Microsoft.Http;
using Newtonsoft.Json;
using Microsoft.ServiceModel.Http;
using System.ServiceModel.Description;
using Newtonsoft.Json.Bson;
using System.ServiceModel.Dispatcher;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;
using Easynet.Edge.Core.Configuration;
using System.ServiceModel.Channels;
using System.Net;
using Microsoft.ServiceModel.Dispatcher;

/// <summary>
/// Summary description for Json
/// </summary>
/// 
namespace EdgeBI.FacebookTools.Services.Service
{
	public class JsonNetProcessor : MediaTypeProcessor
	{
		static bool JsonFormating = (bool.Parse(AppSettings.GetAbsolute("JsonFormatting")));
		private Type parameterType;

		/// <summary>
		/// 
		/// </summary>
		public JsonNetProcessor(HttpOperationDescription operation, MediaTypeProcessorMode mode) : base(operation, mode)
		{
			if (this.Parameter != null)
			{
				this.parameterType = this.Parameter.ParameterType;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		

		public override IEnumerable<string> SupportedMediaTypes
		{
			get
			{
				return new List<string> { "text/json", "application/json", "text/json; charset=UTF-8", "application/json; charset=UTF-8","Content-Type: application/json" };
			}

		}
		
	
		
		public override void WriteToStream(object instance, Stream stream, HttpRequestMessage request)
		{
			

			JsonSerializer serializer = new JsonSerializer();

			using (StreamWriter sw = new StreamWriter(stream))
			{
				using (JsonTextWriter writer = new JsonTextWriter(sw))
				{
					if (JsonFormating)
					{
						writer.Formatting = Formatting.Indented;
						writer.Indentation = 1;
						writer.IndentChar = '\t'; 
					}
					serializer.Serialize(writer, instance);
				}
			}
		}
		

		public override object ReadFromStream(Stream stream, HttpRequestMessage request)
		{
			try
			{
				var serializer = new JsonSerializer();

				using (StreamReader sr = new StreamReader(stream))
				{
					using (JsonTextReader reader = new JsonTextReader(sr))
					{
						object result = serializer.Deserialize(reader, parameterType);
						return result;
					}
				}
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, ex);
				throw;
			}

		}

	}
	public class TabDelimited : MediaTypeProcessor
	{
		public TabDelimited(HttpOperationDescription operation, MediaTypeProcessorMode mode) : base(operation, mode)
		{

		}
		public override object ReadFromStream(Stream stream, HttpRequestMessage request)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<string> SupportedMediaTypes
		{
			get 
			{
				return new List<string>() { "text/tab-separated-values" };
			}
		}

		public override void WriteToStream(object instance, Stream stream, HttpRequestMessage request)
		{
			BulkFile l = (BulkFile)instance;
			l.stream = stream;
			l.test();
		}
	}
	
}

	
	
	


