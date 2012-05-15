using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.IO;
using Edge.Core.Configuration;


namespace Edge.Api.Base
{
	public class JsonSerializer : IHttpSerializer
	{
		static bool JsonFormating = (bool.Parse(AppSettings.Get("JsonSerializer","JsonFormatting")));
		#region IHttpSerializer Members

		public object DeserializeValue(string contentType, System.IO.Stream stream, Type type)
		{
			
			var serializer = new Newtonsoft.Json.JsonSerializer();

			using (StreamReader sr = new StreamReader(stream))
			{
				using (JsonTextReader reader = new JsonTextReader(sr))
				{
					object result = serializer.Deserialize(reader, type);
					return result;
				}
			}
		}

		public void SerializeValue(string contentType, System.IO.Stream stream, object value)
		{
			var serializer = new Newtonsoft.Json.JsonSerializer();

			using (StreamWriter sw = new StreamWriter(stream))
			{
				using (JsonTextWriter writer = new JsonTextWriter(sw))
				{
					if (JsonFormating)
					{
						writer.Formatting = Formatting.Indented;
						writer.Indentation = 1;
						writer.IndentChar = '\t';
					}
					serializer.Serialize(writer, value);
				}
			}
		}

		#endregion
	}
}