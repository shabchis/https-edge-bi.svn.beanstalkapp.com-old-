using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections;

namespace Easynet.Edge.Services.DataRetrieval.DataReader
{
	/// <summary>
	/// Base class for wrappers of xml data.
	/// </summary>
	public abstract class SourceDataRow
	{
	}

	/// <summary>
	/// An abstract class of xml reader that allow to read 
	/// an xml row from a BackOffice and transform it to DB BackOffice row.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>15/09/2008</creation_date>
	public abstract class SourceDataRowReader<RowT>: IDisposable where RowT: SourceDataRow, new()
	{
		#region Members
		/*=========================*/

		private bool _readerOpen = false;
		private RowT _currentRow = null;

		/*=========================*/
		#endregion    

		#region Constructor
        /*=========================*/

		public SourceDataRowReader()
		{
		}

        /*=========================*/
        #endregion    
	
		#region Access Methods
		/*=========================*/

		public RowT CurrentRow
		{
			get 
			{ 
				return _currentRow; 
			}
		}

		/*=========================*/
        #endregion   

		#region Public Methods
		/*=========================*/

		/// <summary>
		/// Read a BackOffice row from the EasyForex XML file and 
		/// parse the row into currentRow.
		/// </summary>
		/// <returns>Read result: True - read succssed, False - end of file.</returns>
		public bool Read()
		{
			// Check if the the xml reader is open.
			if (!_readerOpen)
			{
				OpenReader();
				_readerOpen = true;
			}

			// Call this abstract function to 
			// initalize the current row with the xml row.
			_currentRow = GetRow();

			if (_currentRow == null)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		/*=========================*/
		#endregion   

		#region Virtual Members
		/*=========================*/

		protected virtual RowT GetRow()
		{
			throw new NotImplementedException("GetRow must be implemented.");
		}

		protected virtual void OpenReader()
		{
			throw new NotImplementedException("OpenReader must be implemented.");
		}
	
		/*=========================*/
		#endregion

		#region IDisposable Members
		/*=========================*/

		public virtual void Dispose()
		{
		}

		/*=========================*/
		#endregion

		#region Reslove Methods
        /*=========================*/

		/// <summary>
		/// Check if we can convert the input string to int. 
		/// Used for converting fields from BO to insert command parameters.
		/// </summary>
		/// <param name="inputString">String to convert.</param>
		/// <returns>The number in the string or 0 if it can't be converted.</returns>
		protected int ResolveInt(string inputString)
		{
			int tempInt;
			if (!int.TryParse(inputString, out tempInt))
			{
				tempInt = 0;
			}

			return tempInt;
		}
		
		/// <summary>
		/// Check if we can convert the input string to int. 
		/// Used for converting fields from BO to insert command parameters.
		/// </summary>
		/// <param name="inputString">String to convert.</param>
		/// <returns>The number in the string or 0 if it can't be converted.</returns>
		protected double ResolveDouble(string inputString)
		{
			double tempDouble;
			if (!double.TryParse(inputString, out tempDouble))
			{
				tempDouble = 0;
			}

			return tempDouble;
		}

		/*=========================*/
        #endregion    
	}

	/// <summary>
	/// An abstract class of xml reader that allow to read 
	/// an xml row from a BackOffice and transform it to DB BackOffice row.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>15/09/2008</creation_date>
	public abstract class XmlDataRowReader<RowT>: SourceDataRowReader<RowT> where RowT: SourceDataRow, new()
	{
		#region Members
		/*=========================*/

		private XmlTextReader _xmlReader = null;
		private string _xmlPath;
		protected string _rowName;
		private Dictionary<string, string> _boFieldsMapping;

		public Dictionary<string, string> BoFieldsMapping
		{
			get { return _boFieldsMapping; }
			set { _boFieldsMapping = value; }
		}

		/*=========================*/
		#endregion

		#region Constructor
		/*=========================*/

		public XmlDataRowReader(string xmlPath): this(xmlPath, null)
		{
		}

		public XmlDataRowReader(string xmlPath, string rowName)//, Dictionary<string, string> boFieldsMapping)
		{
			_xmlPath = xmlPath;
			_rowName = rowName;

			// DORON: this is no good
			//if (string.IsNullOrEmpty(_rowName))
			//	throw new Exception("Empty tow name");
			//_boFieldsMapping = boFieldsMapping;
		}

		/*=========================*/
		#endregion

		#region Access Methods
		/*=========================*/

		protected XmlTextReader XmlReader
		{
			get { return _xmlReader; }
		}

		/*=========================*/
		#endregion

		#region Overriden Methods
		/*=========================*/

		protected override void OpenReader()
		{
			// Initalize XmlTextReader with XML file.
			_xmlReader = new XmlTextReader(_xmlPath);
			_xmlReader.WhitespaceHandling = WhitespaceHandling.None;
		}

		public override void Dispose()
		{
			if (_xmlReader != null)
				_xmlReader.Close();
		}

		/*=========================*/
		#endregion
	}

	public abstract class TextDataRowReader<RowT>: SourceDataRowReader<RowT> where RowT: SourceDataRow, new()
	{
		#region Members
		/*=========================*/

		private StreamReader _reader = null;
		private string _filePath;

		/*=========================*/
		#endregion

		#region Constructor
		/*=========================*/

		public TextDataRowReader(string filePath)
		{
			_filePath = filePath;
		}

		/*=========================*/
		#endregion

		#region Access Methods
		/*=========================*/

		protected StreamReader InternalReader
		{
			get { return _reader; }
		}

		/*=========================*/
		#endregion

		#region Overriden Methods
		/*=========================*/

		protected override void OpenReader()
		{
			_reader = new StreamReader(_filePath);
		}

		public override void Dispose()
		{
			if (_reader != null)
				_reader.Close();
		}

		/*=========================*/
		#endregion
	}
}
