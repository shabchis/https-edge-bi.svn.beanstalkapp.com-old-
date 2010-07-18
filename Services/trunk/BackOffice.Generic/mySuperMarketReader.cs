using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Easynet.Edge.Services.DataRetrieval;
using Easynet.Edge.Services.DataRetrieval.DataReader;

namespace Easynet.Edge.Services.BackOffice.Generic
{
	/// <summary>
	/// This class convert Generic BackOffice xml row to an BackOffice row.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>11/02/2009</creation_date>
	public class mySuperMarketReader : XmlDataRowReader<BackOfficeRow>
	{
		#region Constructor
		/*=========================*/

		public mySuperMarketReader(string xmlPath, string rowName)//, Dictionary<string, string> boFieldsMapping)
			: base(xmlPath, rowName)//, boFieldsMapping)
		{
		}

		/*=========================*/
		#endregion

		#region Protected Methods
		/*=========================*/

		#region obsolete code
		/*=========================*/

		//protected  string GetRowFields()
		//{
		//    string returnString ;
		//    returnString =  "TrackerID - " + CurrentRow.GatewayID;
		//    returnString += " TotalClicks - " + CurrentRow.BOClicks;
		//    returnString += " TotalSignups - " + CurrentRow.NewLeads;
		//    returnString += " SentTrolleys - " + CurrentRow.Users;
		//    returnString += " TrolleyPrinters - " + CurrentRow.NewActiveUsers;
		//    returnString += " TrolleySenders - " + CurrentRow.NewUsers;
		//    returnString += " PrintedTrolleys - " + CurrentRow.ActiveUsers;
		//    return returnString;
		//}

		/*=========================*/
		#endregion

		/// <summary>
		/// Read a BackOffice row from the Generic XML file and 
		/// parse the row into currentRow.
		/// </summary>
		/// <returns>current row, null value mean end of file.</returns>
		protected override BackOfficeRow GetRow()
		{
			string nodeName = string.Empty;
			BackOfficeRow currentRow = new BackOfficeRow();

			// Read the xml till we read an entire Generic BackOffice row.
			// End if the row mark with 
			while (XmlReader.Read())
			{
				switch (XmlReader.NodeType)
				{
					case XmlNodeType.Element:
						nodeName = XmlReader.Name;
						break;
					case XmlNodeType.Text:

						if (BoFieldsMapping.ContainsKey(nodeName))
						{
							currentRow.BoValues.Add(BoFieldsMapping[nodeName], ResolveInt(XmlReader.Value));	
						}
						/*
						switch (nodeName)
						{
							case "TrackerID":
								currentRow.GatewayID = ResolveInt(XmlReader.Value);
								break;
							case "TotalClicks":
								currentRow.BOClicks = ResolveInt(XmlReader.Value);
								break;
							case "TotalSignups":
								currentRow.NewLeads = ResolveInt(XmlReader.Value);
								break;
							case "SentTrolleys":
								currentRow.Users = ResolveInt(XmlReader.Value);
								break;
							case "TrolleyPrinters":
								currentRow.NewActiveUsers = ResolveInt(XmlReader.Value);
								break;
							case "TrolleySenders":
								currentRow.NewUsers = ResolveInt(XmlReader.Value);
								break;
							case "PrintedTrolleys":
								currentRow.ActiveUsers = ResolveInt(XmlReader.Value);
								break;
							default:
								break;
						}
						 */ 
						break;

					case XmlNodeType.EndElement:
						// Arrived to end of row.
						if (XmlReader.Name.Contains("TrackerInfo"))
						{
							return currentRow;
						}

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

