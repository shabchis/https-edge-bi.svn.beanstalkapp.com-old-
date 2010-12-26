using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Eggplant.LightweightORM
{
	/// <summary>
	/// 
	/// </summary>
	public struct FieldValue
	{
		public string Name;
		public object Value;

		public FieldValue(string name, object value)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			Name = name;
			Value = value;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public struct FieldTranslation
	{
		public string SourceName;
		public string TargetName;

		public FieldTranslation(string sourceName, string targetName)
		{
			SourceName = sourceName;
			TargetName = targetName;
		}

		public static implicit operator FieldTranslation(string sourceName)
		{
			return new FieldTranslation(sourceName, sourceName);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class FieldKey
	{
		private int _hashCode;
		public readonly string[] Fields;

		public FieldKey(string[] fields)
		{
			if (fields == null || fields.Length < 1)
				throw new ArgumentException("Fields cannot be null or empty.");

			Fields = (string[]) fields.Clone();

			// Combine hashcodes of individual segments
			_hashCode = Fields[0].GetHashCode();
			for (int i = 1; i < Fields.Length; i++)
			{
				_hashCode ^= Fields[i].GetHashCode();
			}
		}

		public override int GetHashCode()
		{
			return _hashCode;
		}

		public override bool Equals(object obj)
		{
			if (obj is FieldKey)
				return this.GetHashCode() == ((FieldKey)obj).GetHashCode();
			else
				return false;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class FieldDictionary : Dictionary<string, object>
	{
		public Dictionary<FieldKey, ThingTranslation> ThingTranslations;

		public FieldDictionary() : base()
		{
		}

		public FieldDictionary(FieldValue[] fieldValues) : base(fieldValues.Length)
		{
			for (int i = 0; i < fieldValues.Length; i++)
				this[fieldValues[i].Name] = fieldValues[i].Value;
		}

		public FieldDictionary(IDataRecord dataRecord) : base(dataRecord.FieldCount)
		{
			for (int i = 0; i < dataRecord.FieldCount; i++)
			{
				// Get rid of nulls
				object val = dataRecord.GetValue(i);
				this[dataRecord.GetName(i)] = val is DBNull ? null : val;
			}
		}

		public ValueT GetValue<ValueT>(string fieldName) where ValueT : class
		{
			object o = null;
			this.TryGetValue(fieldName, out o);
			return (ValueT)o;
		}

		public ValueT GetValue<ValueT>(string fieldName, ValueT emptyValue)
		{
			object o = null;
			this.TryGetValue(fieldName, out o);
			return o == null ? emptyValue : (ValueT) o;
		}

		public ThingT GetThing<ThingT>(params FieldTranslation[] fieldNames) where ThingT : Thing, new()
		{
			ThingT thing = null;
			if (ThingTranslations != null)
			{
				string[] sourceFieldNames = new string[fieldNames.Length];
				for (int i = 0; i < fieldNames.Length; i++)
					sourceFieldNames[i] = fieldNames[i].SourceName;

				ThingTranslation translation;
				if (ThingTranslations.TryGetValue(new FieldKey(sourceFieldNames), out translation))
					thing = (ThingT) translation.TranslationHandler(this);
			}

			if (thing != null)
				return thing;

			FieldDictionary fields = null;
			for (int i = 0; i < fieldNames.Length; i++)
			{
				// Try to get the value
				object o = null;
				this.TryGetValue(fieldNames[i].SourceName, out o);

				// Null of failed to retrieve - skip
				if (o == null)
					continue;

				// Init the dic if it hasn't been done yet
				if (fields == null)
					fields = new FieldDictionary();

				// Apply the value
				fields[fieldNames[i].TargetName] = o;
			}

			if (fields == null)
				return null;

			fields.ThingTranslations = this.ThingTranslations;

			// Create the new thing and apply its values
			thing = new ThingT();
			thing.ApplyValues(fields);

			// Return it
			return thing;
		}
	}


}