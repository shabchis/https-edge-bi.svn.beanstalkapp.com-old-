using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Easynet.Edge.Services.DataRetrieval;
using Easynet.Edge.Services.DataRetrieval.DataReader;

namespace Easynet.Edge.Services.Microsoft.Bing.OrganicRankings
{
    class BingOrganicRankingsReader : XmlDataRowReader<OrganicRankingsRow>
    {
        public BingOrganicRankingsReader(string xmlPath): base(xmlPath)
        {
		}

		/// <summary>
		/// </summary>
		/// <returns>current row, null value mean end of file.</returns>
		protected override OrganicRankingsRow GetRow()
		{
			OrganicRankingsRow row = null;
			string nodeName = null;

			while (XmlReader.Read())
			{
				switch (XmlReader.NodeType)
				{
					case XmlNodeType.Element:
					{
						nodeName = XmlReader.Name;

						if (nodeName == "web:WebResult")
						{
							row = new OrganicRankingsRow();
						}
						break;
					}
					case XmlNodeType.Text:
					case XmlNodeType.CDATA:
					{
						switch (nodeName)
						{
							case "web:Description":
								row.Description = XmlReader.Value;
								break;
							case "web:Title":
								row.Title = XmlReader.Value;
								break;
							case "web:Url":
								row.Url = XmlReader.Value;
								break;
						}
						break;
					}
					case XmlNodeType.EndElement:
					{
						if (XmlReader.Name == "web:WebResult")
							return row;
						break;
					}
				}
			}

			return null;
		}
    }
}
