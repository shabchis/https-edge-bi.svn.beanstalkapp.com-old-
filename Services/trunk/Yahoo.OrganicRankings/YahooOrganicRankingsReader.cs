using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using Easynet.Edge.Services.DataRetrieval;
using Easynet.Edge.Services.DataRetrieval.DataReader;

namespace Easynet.Edge.Services.Yahoo.OrganicRankings
{
	/// <summary>
	/// 
	/// </summary>
	public class YahooOrganicRankingsReader: XmlDataRowReader<OrganicRankingsRow>
	{
		public YahooOrganicRankingsReader(string xmlPath) : base(xmlPath)
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
						nodeName = XmlReader.Name.ToLower();

						if (nodeName == "result")
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
							case "abstract":
								row.Description = XmlReader.Value;
								break;
							case "title":
								row.Title = XmlReader.Value;
								break;
							case "url":
								row.Url = XmlReader.Value;
								break;
						}
						break;
					}
					case XmlNodeType.EndElement:
					{
						if (XmlReader.Name.ToLower() == "result")
							return row;
						break;
					}
				}
			}

			return null;
		}
	}
}
