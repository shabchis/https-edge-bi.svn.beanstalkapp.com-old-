using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Easynet.Edge.Services.DataRetrieval;
using Easynet.Edge.Services.DataRetrieval.DataReader;

namespace Easynet.Edge.Services.BackOffice.EasyForex
{
	/// <summary>
	/// This class convert EasyForex BackOffice xml row to an BackOffice row.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>15/09/2008</creation_date>
	public class EasyForexReader : XmlDataRowReader<BackOfficeRow>
	{
		#region Constructor
		/*=========================*/

		public EasyForexReader(string xmlPath, string rowName)//, Dictionary<string, string> boFieldsMapping)
			: base(xmlPath, rowName)//, boFieldsMapping)
		{
		}

		/*=========================*/
		#endregion

		#region Protected Methods
		/*=========================*/

		protected string GetRowFields()
		{
			string returnString ;
			returnString = "GID - " + CurrentRow.GatewayID;
			returnString += " TotalHits - " + CurrentRow.TotalHits;
			returnString += " NewLeads - " + CurrentRow.NewLeads;
			returnString += " NewUsers - " + CurrentRow.NewUsers;
			returnString += " NewActiveUsers - " + CurrentRow.NewActiveUsers;
			returnString += " NewNetDepostit - " + CurrentRow.NewNetDepostit;
			returnString += " ActiveUsers - " + CurrentRow.ActiveUsers;
			returnString += " TotalNetDeposit - " + CurrentRow.TotalNetDeposit;
			returnString += " SAT - " + CurrentRow.SAT;
			returnString += " GSS - " + CurrentRow.GSS;
			returnString += " EV - " + CurrentRow.EV;
			return returnString;
		}

		/// <summary>
		/// Read a BackOffice row from the EasyForex XML file and 
		/// parse the row into currentRow.
		/// </summary>
		/// <returns>current row, null value mean end of file.</returns>
		protected override BackOfficeRow GetRow()
		{
			string nodeName = string.Empty;
			BackOfficeRow currentRow = new BackOfficeRow();

			// Read the xml till we read an entire EasyForex BackOffice row.
			// End if the row mark with 
			while (XmlReader.Read())
			{
				switch (XmlReader.NodeType)
				{
					case XmlNodeType.Element:
						nodeName = XmlReader.Name;
						break;
					case XmlNodeType.Text:
						switch (nodeName)
						{
							case "GID":
								currentRow.GatewayID = ResolveInt(XmlReader.Value);
								break;
							case "TotalHits":
								currentRow.TotalHits = ResolveInt(XmlReader.Value);
								break;
							case "NewLeads":
								currentRow.NewLeads = ResolveInt(XmlReader.Value);
								break;
							case "NewUsers":
								currentRow.NewUsers = ResolveInt(XmlReader.Value);
								break;
							case "NewActiveUsers":
								currentRow.NewActiveUsers = ResolveInt(XmlReader.Value);
								break;
							case "NewNetDepostit":
								currentRow.NewNetDepostit = ResolveDouble(XmlReader.Value);
								break;
							case "ActiveUsers":
								currentRow.ActiveUsers = ResolveInt(XmlReader.Value);
								break;
							case "TotalNetDeposit":
								currentRow.TotalNetDeposit = ResolveDouble(XmlReader.Value);
								break;
							case "SAT":
								currentRow.SAT = ResolveInt(XmlReader.Value);
								break;
							case "Scoring":
								currentRow.GSS = ResolveInt(XmlReader.Value);
								break;
							case "TotalEV":
								currentRow.EV = ResolveInt(XmlReader.Value);
								break;
							default:
								break;
						}
						break;

					case XmlNodeType.EndElement:
						// Arrived to end of row.
						//if (XmlReader.Name.Contains("CampaignStatisticsForEasyNet"))
						if (XmlReader.Name.Contains("Table"))
							return currentRow;
						
						break;
					default:
						break;
				}
			}

			// Arrived to end of file.
			return null;
		}

		/*=========================*/
		#endregion
	}
}
