using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;
using Easynet.Edge.Core.Configuration;
using System.ServiceModel.Channels;
using Microsoft.Http;
using System.ServiceModel;
using Microsoft.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Net;
using System.Xml;
using System.IO;
using EdgeBI.Objects;
using Microsoft.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.ServiceModel.Http;

namespace EdgeBI.API.Web
{
	public class ErrorMessageInterceptor: MessageInterceptor
	{
		private static string StatusCodeProperty = "edge-status-code";
		private static string ErrorObjectProperty = "edge-error";

		public static void ThrowError(HttpStatusCode statusCode, object error = null)
		{
			OperationContext.Current.OutgoingMessageProperties[StatusCodeProperty] = statusCode;
			OperationContext.Current.OutgoingMessageProperties[ErrorObjectProperty] = error;
			
			throw new WebFaultException(statusCode);
		}

		public override void ProcessResponse(ref System.ServiceModel.Channels.Message request, ref System.ServiceModel.Channels.Message response)
		{
			// Don't do anything if there was no error
			if (!OperationContext.Current.OutgoingMessageProperties.ContainsKey(StatusCodeProperty))
				return;

			HttpStatusCode statusCode = (HttpStatusCode)OperationContext.Current.OutgoingMessageProperties[StatusCodeProperty];
			//object error = OperationContext.Current.OutgoingMessageProperties[ErrorObjectProperty];

			// TODO: add text message to output
			HttpResponseMessage responseMessage = request.ToHttpRequestMessage().CreateResponse(statusCode);



			//var httpMessageProperty = OperationContext.Current.IncomingMessageProperties[HttpMessageProperty.Name] as HttpMessageProperty;
			//var httpRequest = httpMessageProperty.Request as HttpRequestMessage;

			//var endpoint = OperationContext.Current.Host.Description.Endpoints.Find(OperationContext.Current.EndpointDispatcher.EndpointAddress.Uri);
			//var uriMatch = httpRequest.Properties.First(p => p.GetType() == typeof(UriTemplateMatch)) as UriTemplateMatch;
			//var dispatchOperation = OperationContext.Current.EndpointDispatcher.DispatchRuntime.Operations.Where(op => op.Name == uriMatch.Data).First();
			//var operationDescription = endpoint.Contract.Operations.Find(dispatchOperation.Name);
			//var httpBehavoir = endpoint.Behaviors.Find<HttpEndpointBehavior>();
			//var processors = httpBehavoir.GetResponseProcessors(operationDescription.ToHttpOperationDescription()).ToList<Processor>();

			//foreach (var processor in processors)
			//{
			//    var mediaTypeProcessor = processor as MediaTypeProcessor;
			//    if (mediaTypeProcessor == null)
			//        continue;

			//    if (mediaTypeProcessor.SupportedMediaTypes.Contains<string>("application/json"))
			//    {
			//        Exception e = new Exception(OperationContext.Current.OutgoingMessageProperties[ErrorObjectProperty].ToString());

			//        responseMessage.Content = HttpContent.Create(s => mediaTypeProcessor.WriteToStream(e, s, httpRequest));
			//        break;
			//    }
			//}


			
			responseMessage.Content = HttpContent.Create( OperationContext.Current.OutgoingMessageProperties[ErrorObjectProperty].ToString() , "application/json");
			response = responseMessage.ToMessage();
			

			
		 
		
		
			
		}
	


		public override void ProcessRequest(ref Message request)
		{
			//
		}
	}
}