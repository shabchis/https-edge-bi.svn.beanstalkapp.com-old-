using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Easynet.Edge.Services.DataRetrieval.DataReader
{

	/// <summary>
	/// This class represents BackOffice row and contain all the relevent  
	/// fields of BackOffice table.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>15/09/2008</creation_date>
	public class RetrieverDataRow : SourceDataRow
	{
		#region Members
		/*=========================*/

		private Dictionary<string, string> _fields = new Dictionary<string, string>();
		
		/*=========================*/
        #endregion    
		
		#region Access Methods
		/*=========================*/

		public Dictionary<string, string> Fields
		{
			get { return _fields; }
			set { _fields = value; }
		}

		/*=========================*/
        #endregion   
	}
}
