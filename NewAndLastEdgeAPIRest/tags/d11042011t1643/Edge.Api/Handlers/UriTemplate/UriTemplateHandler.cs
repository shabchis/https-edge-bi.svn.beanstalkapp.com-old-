using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;
using System.Net;

namespace Edge.Api.Handlers.Template
{
	public abstract class TemplateHandler : BaseHandler
	{
		public static Dictionary<Type, string> TypeExpressions;
		private const string KeyEncrypt = "5c51374e366f41297356413c71677220386c534c394742234947567840";
		private const string SessionHeader = "x-edgebi-session";
		private const string LogIn = "/sessions";
		//static bool CheckSession = (bool.Parse(AppSettings.GetAbsolute("CheckSession")));
		static TemplateHandler()
		{
			TypeExpressions = new Dictionary<Type, string>();
			TypeExpressions[typeof(string)] = @"[^\/\?\&]+";
			TypeExpressions[typeof(int)] = "[1-9][0-9]*";
		}

		private HttpContext _currentContext;

		public HttpContext CurrentContext
		{
			get { return _currentContext; }

		}

		public override bool IsReusable
		{
			get
			{
				return false;
			}
		}


		static Regex FindParametersRegex = new Regex(@"\{([A-Za-z_][A-Za-z_0-9]*)\}");
		public sealed override void ProcessRequest(HttpContext context)
		{
			_currentContext = context;


			if (ShouldValidateSession)
				{
					if (context.Request.AppRelativeCurrentExecutionFilePath.Replace("~", string.Empty).ToLower() != LogIn.ToLower())
					{
						int userCode;
						string session = context.Request.Headers[SessionHeader];
						if (String.IsNullOrEmpty(session) || !IsSessionValid(session, out userCode))
						{
							throw new HttpException("Invalid session information",(int)HttpStatusCode.Forbidden);

						}
						else
						{
							context.Request.Headers.Add("edge-user-id", userCode.ToString());
						}
					}
				}

				// TODO -later: permissions per request
			

			MethodInfo[] methods = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			MethodInfo foundMethod = null;
			UriMappingAttribute foundAttribute = null;
			Match foundMatch = null;
			foreach (MethodInfo method in methods)
			{
				// Get only method with the 'UriMapping' attribute
				UriMappingAttribute attr = (UriMappingAttribute)Attribute.GetCustomAttribute(method, typeof(UriMappingAttribute));
				if (attr == null)
					continue;

				if (attr.Regex == null)
				{
					// Assign the regex to the attribute for later use
					attr.Regex = BuildRegex(method, attr);
				}
				if (!string.IsNullOrEmpty(attr.Regex.ToString()))
				{
					string netopath = context.Request.Url.PathAndQuery.Remove(0, context.Request.ApplicationPath.Length);
					if (!netopath.StartsWith("/"))
						netopath = '/' + netopath;

					Match match = attr.Regex.Match(netopath);
					if (match.Success)
					{
						if (context.Request.HttpMethod == attr.Method)
						{
							foundMethod = method;
							foundMatch = match;
							foundAttribute = attr;
							break;
						}
					}
				}
			}

			if (foundMethod == null || foundMatch == null)
				throw new UriTemplateException("method not exist", context.Request.Url.PathAndQuery, HttpStatusCode.NotFound);

			// Build a list of method arguments
			ParameterInfo[] methodParams = foundMethod.GetParameters();
			object[] methodArgs = new object[methodParams.Length];
			for (int i = 0; i < methodParams.Length; i++)
			{
				ParameterInfo param = methodParams[i];
				object paramVal;
				if (param.Name == foundAttribute.BodyParameter)
				{
					// Handle POST body deserialization
					// TODO: allow deserializing as stream
					paramVal = HttpManager.DeserializeFromInput(context, param.ParameterType);
				}
				else
				{
					// foundMatch.Groups[param.Name] will always find a group because if it were empty - the regex would not have succeeded
					string rawValue = foundMatch.Groups[param.Name].Value;
					paramVal = Convert.ChangeType(rawValue, param.ParameterType);
				}
				methodArgs[i] = paramVal;
			}

			// Run the MOTHERFUCKER
			object val = foundMethod.Invoke(this, methodArgs);

			// return as JSON for now
			HttpManager.SetResponse(context, System.Net.HttpStatusCode.OK, val, "text/plain");
		}

		private static Regex BuildRegex(MethodInfo method, UriMappingAttribute attr)
		{
			Regex targetRegex;

			MatchCollection paramMatches = FindParametersRegex.Matches(attr.Template);
			if (paramMatches.Count < 1)
			{
				targetRegex = new Regex(string.Format(@"^{1}{0}$",
					attr.Template,
					attr.Template.StartsWith("/") ? string.Empty : "/"
					),
				RegexOptions.IgnoreCase);
			}
			else
			{
				// Always start URLs with a leading slash
				string targetRegexPattern = "^" + (attr.Template.StartsWith("/") ? string.Empty : "/");

				ParameterInfo[] parameters = method.GetParameters();
				int lastIndex = 0;

				foreach (Match m in paramMatches)
				{
					targetRegexPattern += Regex.Escape(string.Format("{0}", attr.Template.Substring(lastIndex, m.Index - lastIndex)));
					lastIndex = m.Index + m.Value.Length;

					ParameterInfo foundParam = null;
					foreach (ParameterInfo param in parameters)
					{
						string paramName = m.Groups[1].Value; // get the param name
						if (param.Name == paramName)
						{
							foundParam = param;
							break;
						}
					}

					string paramExpression;

					if (foundParam == null)
					{
						paramExpression = m.Value;
					}
					else
					{
						string typeExpression;
						if (!TypeExpressions.TryGetValue(foundParam.ParameterType, out typeExpression))
							throw new UriTemplateException(foundParam.Name, System.Net.HttpStatusCode.NotFound);
						

						// we found a matching parameter!
						paramExpression = "(?<" + foundParam.Name + ">" + typeExpression + ")";
					}

					targetRegexPattern += paramExpression;
				}

				if (attr.Template.Length > lastIndex)
				{
					targetRegexPattern += Regex.Escape(attr.Template.Substring(lastIndex, attr.Template.Length - lastIndex));
				}

				targetRegex = new Regex(targetRegexPattern+"$");
			}
			return targetRegex;
		}
		private bool IsSessionValid(string session, out int userCode)
		{
			bool isValid = false;
			int sessionID = 0;


			
			userCode = -1;
			Encryptor encryptor = new Encryptor(KeyEncrypt);

			try { sessionID = int.Parse(encryptor.Decrypt(session)); }
			catch (Exception ex)
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

		public abstract bool ShouldValidateSession { get; }

	}
}