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
namespace EdgeBI.API.Web
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
		protected override ProcessorResult OnExecute(object[] input)
		{
			return base.OnExecute(input);
		}

		public override IEnumerable<string> SupportedMediaTypes
		{

			get
			{

				return new List<string> { "text/json", "application/json", "text/json; charset=UTF-8", "application/json; charset=UTF-8" };

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

	
	//public class SessionProcessor : Processor
	//{



	//    protected override ProcessorResult OnExecute(object[] input)
	//    {
	//    //    HttpRequestMessage req;
	//    //    if (input.Length > 0 && input[0] is HttpRequestMessage)
	//    //    {
	//    //        req = (HttpRequestMessage)input[0];
	//    //        HttpResponseMessage res = req.CreateResponse(HttpStatusCode.Forbidden);

	//    //        return new ProcessorResult() { Output = new object[] { res }, Status = ProcessorStatus.Error, Error = new Exception("aa") };

	//    //    }
	//    //    else
	//    //        return new ProcessorResult();
	//        return new ProcessorResult();


	//    }

	//    //protected override IEnumerable<ProcessorArgument> OnGetInArguments()
	//    //{

	//    //    var args = new List<ProcessorArgument>();
			

	//    //    args.Add(new ProcessorArgument(HttpPipelineFormatter.ArgumentHttpRequestMessage, typeof(HttpRequestMessage)));	
			
	//    //    return args;
	//    //}

	//    //protected override IEnumerable<ProcessorArgument> OnGetOutArguments()
	//    //{
	//    //    var args = new List<ProcessorArgument>();
			
	//    //    args.Add(new ProcessorArgument(HttpPipelineFormatter.ArgumentHttpResponseMessage, typeof(HttpResponseMessage)));
	//    //    return args;

	//    //}
	//}
	public class HttpMessageInspector : IDispatchMessageInspector
	{
		#region IDispatchMessageInspector Members

		public object AfterReceiveRequest(ref Message request, System.ServiceModel.IClientChannel channel, System.ServiceModel.InstanceContext instanceContext)
		{
			throw new NotImplementedException();
		}

		public void BeforeSendReply(ref Message reply, object correlationState)
		{
			throw new NotImplementedException();
		}

		#endregion
	}


	public class MyResourceConfiguration : HostConfiguration
	{
		
		public override void RegisterRequestProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors, MediaTypeProcessorMode mode)
		{
			//processors.Add(new FormUrlEncodedProcessor(operation,mode));

			processors.Add(new JsonNetProcessor(operation, mode));

			//processors.Add(new BsonProcessor(operation, mode));			

		}

		public override void RegisterResponseProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors, MediaTypeProcessorMode mode)
		{
			processors.Add(new JsonNetProcessor(operation, mode));
			//processors.Add(new JsonProcessor(operation, mode));

			//processors.Add(new BsonProcessor(operation, mode));
		}
		

	}
}
