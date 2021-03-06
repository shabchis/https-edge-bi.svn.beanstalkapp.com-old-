﻿using System;
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
using Easynet.Edge.Core.Configuration;

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
		private const string KeyEncrypt = "5c51374e366f41297356413c71677220386c534c394742234947567840";
		private const string SessionHeader = "x-edgebi-session";
		private const string LogInMessageAdress = "/api/EdgeBIAPIService.svc/sessions";
												
		public SessionInterceptor()
			: base(false)
		{
		}

		public override void ProcessRequest(ref System.ServiceModel.Channels.RequestContext requestContext)
		{
			Message request = requestContext.RequestMessage;
			HttpRequestMessageProperty requestProp = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
			
		
			if (bool.Parse(AppSettings.GetAbsolute("CheckSession")) == true)
			{
				Message reply;
				reply = Message.CreateMessage(MessageVersion.None, null);
				HttpResponseMessageProperty responseProp = new HttpResponseMessageProperty() { StatusCode = HttpStatusCode.Unauthorized };
				if (requestContext == null || requestContext.RequestMessage == null)
				{
					return;
				}				
				NameValueCollection queryParams = HttpUtility.ParseQueryString(requestProp.QueryString);
				if (request.Properties.Via.LocalPath.ToUpper() != LogInMessageAdress.ToUpper())
				{
					if (requestProp.Headers[SessionHeader] == null)//case header not specified
					{
						responseProp.StatusDescription = ("You have not specified session header");
						responseProp.Headers[HttpResponseHeader.ContentType] = "text/html";
						reply.Properties[HttpResponseMessageProperty.Name] = responseProp;
						requestContext.Reply(reply);
						// set the request context to null to terminate processing of this request
						requestContext = null;
					}
					else
					{
						string session = requestProp.Headers[SessionHeader];
						int userCode;
						if (!IsSessionValid(session,out userCode)) //if session is valid
						{
							responseProp.StatusCode = HttpStatusCode.Unauthorized;
							responseProp.StatusDescription = ("session is not exist or out of date");
							responseProp.Headers[HttpResponseHeader.ContentType] = "text/html";
							reply.Properties[HttpResponseMessageProperty.Name] = responseProp;
							requestContext.Reply(reply);
							// set the request context to null to terminate processing of this request
							requestContext = null;

						}
						else
						{

							requestProp.Headers.Add("xUserCode", "userCode");

						}
					}
				} 
			}




		}
		
		
		private bool IsSessionValid(string session,out int userCode)
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
					using (SqlDataReader sqlDataReader=sqlCommand.ExecuteReader())
					{
						isValid = System.Convert.ToBoolean(sqlDataReader[0]);
						userCode = System.Convert.ToInt32(sqlDataReader[1]);
					}
					
					


				}

			}



			return isValid;

		}
	}
}