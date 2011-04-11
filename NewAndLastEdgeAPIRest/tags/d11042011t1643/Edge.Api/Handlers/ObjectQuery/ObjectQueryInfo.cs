using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Edge.Api
{

	class RequestLevel
	{
		public string Property;
		public string Index;
	}

	class ObjectQueryInfo
	{
		static Regex REGEX_MAIN = new Regex(@"^\/(?<object>[a-z][a-z0-9_]*)(?:\/(?<id>[0-9]+))?(?<subpath>[\/a-z0-9_]*)(?:\?[^\/]*)?$", RegexOptions.IgnoreCase);
		static Regex REGEX_PROPS = new Regex(@"\/(?<property>[a-z][a-z0-9_]*)(?:\/(?<index>[0-9]+))?", RegexOptions.IgnoreCase);

		public string ObjectType;
		public string ObjectID;
		public List<RequestLevel> Levels = new List<RequestLevel>();
		//public string Include;
		public Dictionary<string,string> Filter = new Dictionary<string,string>();

		public ObjectQueryInfo(string path, NameValueCollection queryParams)
		{
			// get query parameters
			if (queryParams != null)
			{
				foreach (string param in queryParams.AllKeys)
					this.Filter.Add(param, queryParams[param]);
			}

			// extract main object information
			Match mainMatches = REGEX_MAIN.Match(path);
			if (!mainMatches.Success)
				return;

			// build the top level object request
			this.ObjectType = mainMatches.Groups["object"].Captures[0].Value;
			this.ObjectID = mainMatches.Groups["id"].Captures.Count > 0 ? mainMatches.Groups["id"].Captures[0].Value : null;

			if (mainMatches.Groups["subpath"].Captures.Count < 1)
				return;

			// extract lower levels info
			MatchCollection propMatches = REGEX_PROPS.Matches(mainMatches.Groups["subpath"].Captures[0].Value);
			if (propMatches.Count < 1)
				return;

			// start populating properties from top level
			foreach (Match match in propMatches)
			{
				RequestLevel level = new RequestLevel();
				level.Property = match.Groups["property"].Value;
				level.Index = String.IsNullOrWhiteSpace(match.Groups["index"].Value) ? null : match.Groups["index"].Value;
				this.Levels.Add(level);
			}
		}
	}
}