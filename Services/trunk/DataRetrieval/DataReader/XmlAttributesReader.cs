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
	public class XmlAttributesReader : XmlDataRowReader<RetrieverDataRow>
	{
	    bool _gotToFirstRow = false;

	    #region Constructor
	    /*=========================*/

	    public XmlAttributesReader(string xmlPath, string rowName)
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
	            if (XmlReader.Name.ToLower().Contains(_rowName) && XmlReader.HasAttributes)
	            {
					_gotToFirstRow = true;
	                break;
	            }
	        }
	     
	        RetrieverDataRow currentRow = new RetrieverDataRow();
	        
			while (!(XmlReader.Name.ToLower().Contains(_rowName) && XmlReader.HasAttributes))
			{
				if (!XmlReader.Read())
					 return null;
			}

			// Read node attributes
			while (XmlReader.MoveToNextAttribute()) 
				currentRow.Fields.Add(XmlReader.Name, XmlReader.Value);
							
			//XmlReader.Read();
	        return currentRow;
	    }

	    /*=========================*/
	    #endregion	
	}
}

