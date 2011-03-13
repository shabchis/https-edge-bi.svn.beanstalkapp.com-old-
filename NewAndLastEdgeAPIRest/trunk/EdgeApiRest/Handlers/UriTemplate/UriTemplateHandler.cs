using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace EdgeApiRest.Handlers.UriTemplate
{
	public abstract class UriTemplateHandler:BaseHandler
	{
		public static Dictionary<Type, string> TypeExpressions;
		static UriTemplateHandler()
		{
			TypeExpressions = new Dictionary<Type, string>();
			TypeExpressions[typeof(string)] = ".+";
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
				// TODO: check session

				// TODO -later: permissions per request
			}

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

				Match match = attr.Regex.Match(context.Request.Url.PathAndQuery);
				if (match.Success)
				{
					foundMethod = method;
					foundMatch = match;
					foundAttribute = attr;
					break;
				}
			}

			if (foundMethod == null || foundMatch == null)
				throw new Exception("400 Bad Request");

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
					paramVal = HttpSerializer.DeserializeValue(context, param.ParameterType);
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
			SetResponse(context, System.Net.HttpStatusCode.OK, val, "text/plain");
		}

		private static Regex BuildRegex(MethodInfo method, UriMappingAttribute attr)
		{
			Regex targetRegex;
			string urlToParse = attr.Template;
			MatchCollection paramMatches = FindParametersRegex.Matches(urlToParse);
			if (paramMatches.Count < 1)
			{
				targetRegex = new Regex("");
			}
			else
			{
				string targetRegexPattern = string.Empty;
				ParameterInfo[] parameters = method.GetParameters();
				int lastIndex = 0;

				foreach (Match m in paramMatches)
				{
					targetRegexPattern += urlToParse.Substring(lastIndex, m.Index - lastIndex);
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
							throw new UriTemplateException("Cannot map URL parameter to method parameter type.", foundParam.Name);

						// we found a matching parameter!
						paramExpression = "(?<" + foundParam.Name + ">" + typeExpression + ")";
					}

					targetRegexPattern += paramExpression;
				}

				targetRegex = new Regex(targetRegexPattern);
			}
			return targetRegex;
		}

		public abstract bool ShouldValidateSession { get; }

	}
}