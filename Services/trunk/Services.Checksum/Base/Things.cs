using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Eggplant.LightweightORM
{
	/// <summary>
	/// Base class for data things.
	/// </summary>
	public abstract class Thing
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="fields"></param>
		public abstract void ApplyValues(FieldDictionary fields);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dataRecord"></param>
		public void ApplyValues(IDataRecord dataRecord)
		{
			ApplyValues(new FieldDictionary(dataRecord));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fieldValues"></param>
		public void ApplyValues(params FieldValue[] fieldValues)
		{
			ApplyValues(new FieldDictionary(fieldValues));
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceName"></param>
		/// <param name="targetName"></param>
		/// <returns></returns>
		protected FieldTranslation TranslateField(string sourceName, string targetName)
		{
			return new FieldTranslation(sourceName, targetName);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="translationHandler"></param>
		/// <param name="sourceFields"></param>
		/// <returns></returns>
		protected ThingTranslation TranslateThing(Func<FieldDictionary, Thing> translationHandler, params string[] sourceFields)
		{
			return new ThingTranslation(translationHandler, sourceFields);
		}

		public virtual void Save()
		{
		}

	}

	/// <summary>
	/// 
	/// </summary>
	public struct ThingTranslation
	{
		public string[] SourceFields;
		public Func<FieldDictionary, Thing> TranslationHandler;

		public ThingTranslation(Func<FieldDictionary, Thing> translationHandler, params string[] sourceFields)
		{
			TranslationHandler = translationHandler;
			SourceFields = sourceFields;
		}
	}
	
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="ThingT"></typeparam>
	public class ThingReader<ThingT> : IDisposable where ThingT : Thing, new()
	{
		public readonly Dictionary<FieldKey, ThingTranslation> _thingTranslations = null;

		ThingT _current = null;
		IDataReader _reader = null; 

		public ThingReader(IDataReader reader, params ThingTranslation[] thingTranslations)
		{
			_reader = reader;

			if (thingTranslations == null)
				return;

			for (int i = 0; i < thingTranslations.Length; i++)
			{
				if (_thingTranslations == null)
					_thingTranslations = new Dictionary<FieldKey, ThingTranslation>();

				_thingTranslations.Add(new FieldKey(thingTranslations[i].SourceFields), thingTranslations[i]);
			}
		}

		public bool Read()
		{
			bool hasData = _reader.Read();

			if (hasData)
			{
				_current = new ThingT();
				
				FieldDictionary fields = new FieldDictionary(_reader);
				fields.ThingTranslations = _thingTranslations;

				_current.ApplyValues(fields);
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
