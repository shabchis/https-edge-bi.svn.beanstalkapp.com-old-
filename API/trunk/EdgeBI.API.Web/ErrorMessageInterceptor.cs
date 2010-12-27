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

			
			

			
			responseMessage.Content = HttpContent.Create( OperationContext.Current.OutgoingMessageProperties[ErrorObjectProperty].ToString() , "application/json");
			response = responseMessage.ToMessage();
			

			
		 
		
		
			
		}
	


		public override void ProcessRequest(ref Message request)
		{
			//
		}
	}
}