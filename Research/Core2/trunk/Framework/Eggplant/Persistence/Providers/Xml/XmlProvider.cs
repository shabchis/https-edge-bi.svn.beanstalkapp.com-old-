using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading;
using System.Text.RegularExpressions;
using System.Data;
using System.Reflection;
using System.IO;
using System.Xml;

namespace Eggplant.Persistence.Providers.Xml
{
	public class XmlProvider: PersistenceProvider
	{
		/*public readonly Stream Stream = null;*/
		public readonly string Url = null;
		public readonly string SchemaUrl = null;

		public XmlProvider(string url, string schemaUrl)
		{
			Url = url;
			SchemaUrl = schemaUrl;
			LoadMappingsFromXsd(schemaUrl);
		}

		internal void LoadMappingsFromXsd(string schemalUrl)
		{
			XmlTextReader reader = new XmlTextReader(schemalUrl);
			reader.WhitespaceHandling = WhitespaceHandling.None;

			while (!reader.EOF)
			{
				reader.Read();
				if (reader.NodeType == XmlNodeType.Comment)
					continue;

				if (reader.NodeType == XmlNodeType.Element)
				{
					while (reader.MoveToNextAttribute())
					{
					}
				}
			}
		}

		protected override PersistenceConnection CreateNewConnection()
		{
			return new XmlConnection(this);
		}
	}
}
