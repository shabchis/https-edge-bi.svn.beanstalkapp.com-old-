using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Activation;
using Microsoft.ServiceModel.Web;
using System.ServiceModel.Web;
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;
using System.ServiceModel.Channels;
using System.Net;
using Easynet.Edge.Core.Utilities;
using System.Collections.Specialized;

namespace EdgeBI.WCFService
{

	public class SessionBasedServiceHostFactory : ServiceHostFactory
	{
		protected override System.ServiceModel.ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
		{
			WebServiceHost2 host = new WebServiceHost2(serviceType, true, baseAddresses);
			host.Interceptors.Add(new SessionInterceptor());
			return host;
		}
	}

	public class SessionInterceptor : RequestInterceptor
	{
		private const string Key = "5c51374e366f41297356413c71677220386c534c394742234947567840";
		private const string SessionHeader = "x-edgebi-session";
		private const string LogInMessageAdress = "http://localhost:54796/EdgeBIAPIService.svc/sessions";
		public SessionInterceptor()
			: base(false)
		{
		}

		public override void ProcessRequest(ref System.ServiceModel.Channels.RequestContext requestContext)
		{
			//Message reply;
			//reply = Message.CreateMessage(MessageVersion.None, null);
			//HttpResponseMessageProperty responseProp = new HttpResponseMessageProperty() { StatusCode = HttpStatusCode.Unauthorized };
			//if (requestContext == null || requestContext.RequestMessage == null)
			//{
			//    return;
			//}

			//Message request = requestContext.RequestMessage;


			//HttpRequestMessageProperty requestProp = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
			//NameValueCollection queryParams = HttpUtility.ParseQueryString(requestProp.QueryString);
			//if (request.Properties.Via.OriginalString!=LogInMessageAdress)
			//{
			//    if (requestProp.Headers[SessionHeader] == null)//case header not specified
			//    {
			//        responseProp.StatusDescription = ("You have not specified session header");
			//        responseProp.Headers[HttpResponseHeader.ContentType] = "text/html";
			//        reply.Properties[HttpResponseMessageProperty.Name] = responseProp;
			//        requestContext.Reply(reply);
			//        // set the request context to null to terminate processing of this request
			//        requestContext = null;
			//    }
			//    else
			//    {
			//        string session = requestProp.Headers[SessionHeader];
			//        if (!IsSessionValid(session)) //if session is valid
			//        {
			//            responseProp.StatusDescription = ("session is not exist or out of date");
			//            responseProp.Headers[HttpResponseHeader.ContentType] = "text/html";
			//            reply.Properties[HttpResponseMessageProperty.Name] = responseProp;
			//            requestContext.Reply(reply);
			//            // set the request context to null to terminate processing of this request
			//            requestContext = null;

			//        }
			//    }
			//}




		}
		private bool IsSessionValid(string session)
		{
			bool isValid = false;
			int sessionID = 0;
			DateTime lastModified;

			Encryptor encryptor = new Encryptor(Key);


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
}