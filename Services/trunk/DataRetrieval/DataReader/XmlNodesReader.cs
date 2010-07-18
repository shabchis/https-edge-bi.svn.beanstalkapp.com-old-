using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Easynet.Edge.Services.DataRetrieval;

namespace Easynet.Edge.Services.DataRetrieval.DataReader
{
	/// <summary>
	/// This class convert Generic BackOffice xml row to an BackOffice row.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>11/02/2009</creation_date>
	public class XmlNodesReader : XmlDataRowReader<RetrieverDataRow>
	{
		bool _gotToFirstRow = false;

		#region Constructor
		/*=========================*/

		public XmlNodesReader(string xmlPath, string rowName)
			: base(xmlPath, rowName)
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
		protected override RetrieverDataRow GetRow()
		{
			// Get to first row.
			while (!_gotToFirstRow && XmlReader.Read())
			{
				if (XmlReader.Name.ToLower().Contains(_rowName))
				{
					_gotToFirstRow = true;
					break;
				}
			}

			string nodeName = string.Empty;
			RetrieverDataRow currentRow = new RetrieverDataRow();

			// Read the xml till we read an entire Row.
			while (XmlReader.Read())
			{
				switch (XmlReader.NodeType)
				{
					case XmlNodeType.Element:
						nodeName = XmlReader.Name;
						break;
					case XmlNodeType.Text:
						if (nodeName != _rowName)
							currentRow.Fields.Add(nodeName, XmlReader.Value.ToString());
						break;

					case XmlNodeType.EndElement:
						// Arrived to end of row.
						if (XmlReader.Name.ToLower().Contains(_rowName))
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

