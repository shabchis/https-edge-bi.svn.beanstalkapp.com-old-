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

namespace EdgeBI.API.Web
{
	public class SessionValidationInterceptor : MessageInterceptor
	{
		private const string KeyEncrypt = "5c51374e366f41297356413c71677220386c534c394742234947567840";
		private const string SessionHeader = "x-edgebi-session";
		private const string LogIn = "LogIn";
		static bool CheckSession = (bool.Parse(AppSettings.GetAbsolute("CheckSession")));

		public override void ProcessRequest(ref System.ServiceModel.Channels.Message request)
		{
			try
			{
				HttpRequestMessage httpRequestMessage = request.ToHttpRequestMessage();

				if (CheckSession)
				{
					UriTemplateMatch uriTemplateMatch = (UriTemplateMatch)httpRequestMessage.Properties.Where(prop => prop.GetType() == typeof(UriTemplateMatch)).First();

					if (uriTemplateMatch.Data.ToString().ToUpper() != LogIn.ToUpper())
					{
						if (httpRequestMessage.Headers.ContainsKey(SessionHeader))
						{
							int userCode;
							string session = httpRequestMessage.Headers[SessionHeader];
							if (String.IsNullOrEmpty(session) || !IsSessionValid(session, out userCode))
							{
								ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Invalid session information.");
								throw new InvalidOperationException();
							}
							else
							{
								OperationContext.Current.IncomingMessageProperties.Add("edge-user-id", userCode);

							}

						}
						else
						{
							ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Invalid session information.");
							throw new InvalidOperationException();

						}
					}
				}
			}
			catch (Exception ex)
			{
				
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden,ex);
			}
		}



		private bool IsSessionValid(string session, out int userCode)
		{
			bool isValid = false;
			int sessionID = 0;
			
			
			DateTime lastModified;
			userCode = -1;
			Encryptor encryptor = new Encryptor(KeyEncrypt);

			try { sessionID = int.Parse(encryptor.Decrypt(session)); }
			catch(Exception ex)
			{
				// TODO: log the real exception
				return false;
			}

			using (DataManager.Current.OpenConnection())
			{
				using (SqlCommand sqlCommand = DataManager.CreateCommand("Session_ValidateSession(@SessionID:Int)", System.Data.CommandType.StoredProcedure))
				{
					sqlCommand.Parameters["@SessionID"].Value = sessionID;
					using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
					{
						if (sqlDataReader.Read())
						{
						isValid = System.Convert.ToBoolean(sqlDataReader[0]);
						userCode = System.Convert.ToInt32(sqlDataReader[1]);
						}
					}
				}
			}



			return isValid;

		}

		public override void ProcessResponse(ref Message request, ref Message response)
		{
			//
		}
	}

}
