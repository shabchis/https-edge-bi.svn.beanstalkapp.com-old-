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
	public class CsvReader : TextDataRowReader<RetrieverDataRow>
	{
		bool _gotToFirstRow = false;

		#region Constructor
		/*=========================*/

		public CsvReader(string filePath)
			: base(filePath)
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
	
			RetrieverDataRow currentRow = new RetrieverDataRow();

			// Arrived to end of file.
			return null;
		}

		/*=========================*/
		#endregion	
	}
}

