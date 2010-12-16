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

/// <summary>
/// Summary description for Json
/// </summary>
/// 
namespace EdgeBI.API.Web
{
	public class JsonNetProcessor : MediaTypeProcessor
	{
		private const string KeyEncrypt = "5c51374e366f41297356413c71677220386c534c394742234947567840";
		private const string SessionHeader = "x-edgebi-session";
		private const string LogInMessageAdress = "http://localhost:54796/EdgeBIAPIService.svc/sessions";

		private Type parameterType;

		public JsonNetProcessor(HttpOperationDescription operation, MediaTypeProcessorMode mode)

			: base(operation, mode)
		{

			if (this.Parameter != null)
			{

				this.parameterType = this.Parameter.ParameterType;

			}

		}

		public override IEnumerable<string> SupportedMediaTypes
		{

			get
			{

				return new List<string> { "text/json", "application/json" };

			}

		}

		public override void WriteToStream(object instance, Stream stream, HttpRequestMessage request)
		{

			var serializer = new JsonSerializer();



			using (var sw = new StreamWriter(stream))

			using (var writer = new JsonTextWriter(sw))
			{

				serializer.Serialize(writer, instance);

			}

		}

		public override object ReadFromStream(Stream stream, HttpRequestMessage request)
		{
			
			var serializer = new JsonSerializer();

			using (var sr = new StreamReader(stream))

			using (var reader = new JsonTextReader(sr))
			{

				var result = serializer.Deserialize(reader, parameterType);

				return result;

			}

		}
		protected override ProcessorResult OnExecute(object[] input)
		{
			return base.OnExecute(input);
		}
		
		private bool IsSessionValid(string session)
		{
			bool isValid = false;
			int sessionID = 0;
			DateTime lastModified;

			Encryptor encryptor = new Encryptor(KeyEncrypt);


			sessionID = int.Parse(encryptor.Decrypt(session));
			using (DataManager.Current.OpenConnection())
			{
				using (SqlCommand sqlCommand = DataManager.CreateCommand("Session_ValidateSession(@SessionID:Int)", System.Data.CommandType.StoredProcedure))
				{
					sqlCommand.Parameters["@SessionID"].Value = sessionID;
					isValid = System.Convert.ToBoolean(sqlCommand.ExecuteScalar());


				}

			}



			return isValid;

		}

	}
	public class BsonProcessor : MediaTypeProcessor
	{

		private Type parameterType;

		public BsonProcessor(HttpOperationDescription operation, MediaTypeProcessorMode mode)

			: base(operation, mode)
		{

			if (this.Parameter != null)
			{

				this.parameterType = this.Parameter.ParameterType;

			}

		}

		public override IEnumerable<string> SupportedMediaTypes
		{

			get
			{

				return new List<string> { "application/bson" };

			}

		}

		public override void WriteToStream(object instance, Stream stream, HttpRequestMessage request)
		{

			var serializer = new JsonSerializer();



			using (var writer = new BsonWriter(stream))
			{

				serializer.Serialize(writer, instance);

			}

		}

		public override object ReadFromStream(Stream stream, HttpRequestMessage request)
		{

			var serializer = new JsonSerializer();

			using (var reader = new BsonReader(stream))
			{

				var result = serializer.Deserialize(reader, parameterType);

				return result;

			}

		}

	}

	public class MyResourceConfiguration : HostConfiguration
	{
		
	
		public override void RegisterRequestProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors, MediaTypeProcessorMode mode)
		{

			
			processors.Add(new JsonNetProcessor(operation, mode));

			processors.Add(new BsonProcessor(operation, mode));
			

		}

		public override void RegisterResponseProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors, MediaTypeProcessorMode mode)
		{

			processors.Add(new JsonNetProcessor(operation, mode));

			processors.Add(new BsonProcessor(operation, mode));

		}
		

	}
}
