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
	public class ConduitReader : XmlDataRowReader<BackOfficeRow>
	{
		#region Constructor
		/*=========================*/

		public ConduitReader(string xmlPath, string rowName)//, Dictionary<string, string> boFieldsMapping)
			: base(xmlPath, rowName)//, boFieldsMapping)
		{
		}

		/*=========================*/
		#endregion

		#region Protected Methods
		/*=========================*/

	

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
						nodeName = XmlReader.Name.ToLower();

						if (nodeName.Contains("tracker"))
						{
							// Read all the node attriburs.
							while (XmlReader.MoveToNextAttribute())
							{
								if (XmlReader.NodeType != XmlNodeType.EntityReference)
								{
									//attributeName = XmlReader.Name.ToLower();
									//if (BackOfficeFields.ContainsKey(attributeName))
									//{
									//    BackOfficeFields[attributeName] = ResolveInt(XmlReader.Value);
									//}

									switch (XmlReader.Name.ToLower())
									{
										case "id":
											currentRow.GatewayID = ResolveInt(XmlReader.Value);
											break;
										case "leads":
											currentRow.BOClicks = ResolveInt(XmlReader.Value);
											break;
										case "signups":
											currentRow.NewLeads = ResolveInt(XmlReader.Value);
											break;
										case "goodtoolbars":
											currentRow.Users = ResolveInt(XmlReader.Value);
											break;
										case "greattoolbars":
											currentRow.NewActiveUsers = ResolveInt(XmlReader.Value);
											break;
										case "onlineacquisitions":
											currentRow.NewUsers = ResolveInt(XmlReader.Value);
											break;
										case "salestotal":
											currentRow.ActiveUsers = ResolveInt(XmlReader.Value);
											break;
										default:
											break;
									}
								}
							}
						}
						break;							
					case XmlNodeType.EndElement:
						// Arrived to end of row.
						if (XmlReader.Name.ToLower().Contains("tracker"))
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

		#region Obsolete code
		/*=========================*/

		//protected string GetRowFields()
		//{
		//    string returnString;
		//    returnString = "ID - " + CurrentRow.GatewayID;
		//    //returnString += " TotalClicks - " + CurrentRow.BOClicks;
		//    //returnString += " Leads - " + CurrentRow.NewLeads;
		//    //returnString += " Signups - " + CurrentRow.Users;
		//    //returnString += " TrolleyPrinters - " + CurrentRow.NewActiveUsers;
		//    //returnString += " TrolleySenders - " + CurrentRow.NewUsers;
		//    //returnString += " PrintedTrolleys - " + CurrentRow.ActiveUsers;
		//    return returnString;
		//}

		/*=========================*/
		#endregion
	}
}

