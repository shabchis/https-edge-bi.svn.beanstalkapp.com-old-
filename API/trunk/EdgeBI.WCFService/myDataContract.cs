using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Description;
using Newtonsoft.Json.Serialization;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Http;
using Microsoft.ServiceModel.Http;
using System.ServiceModel.Dispatcher;
using Newtonsoft.Json.Bson;



namespace EdgeBI.WCFService
{
	public class JsonNetProcessor : MediaTypeProcessor
	{

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