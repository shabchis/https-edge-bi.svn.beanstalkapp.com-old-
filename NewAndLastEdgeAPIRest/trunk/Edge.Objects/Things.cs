using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Edge.Objects
{
	
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="ThingT"></typeparam>
	public class ThingReader<ThingT> : IDisposable where ThingT : class, new()
	{

		Func<FieldInfo, IDataRecord, object> _onApplyValue;
		ThingT _current;
		IDataReader _reader = null;

		public ThingReader(IDataReader reader, Func<FieldInfo, IDataRecord, object> onApplyValue)
		{
			_reader = reader;
			_onApplyValue = onApplyValue;

			
		}

		public bool Read()
		{
			bool hasData = _reader.Read();

			if (hasData)
			{
				
				_current = new ThingT();



				_current = MapperUtility.CreateMainObject<ThingT>(_reader,_onApplyValue);
			}

			return hasData;
		}

		public ThingT Current
		{
			get
			{
				return _current;
			}
		}

		public IDataReader InnerReader
		{
			get { return _reader; }
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (_reader != null)
				_reader.Dispose();
		}

		#endregion
	}
}
