using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eggplant.Model
{
	public class ObjectModel
	{
		public Dictionary<string,ObjectDefinition> Definitions { get; }

		public static ObjectModel Load()
		{
			return (ObjectModel)
				Eggplant.Persistence.Persistence
				.Current
				.Provider
				.Model
				.For<ObjectModel>()
				.Queries["Load"]
				.GetSingle();
		}

		public ObjectDefinition For<T>()
		{
			return null;
		}
		public ObjectDefinition For(Type t)
		{
			return null;
		}
	}

	public class ObjectDefinition
	{
		public string Name { get; set; }
		public string Namespace { get; set; }
		public bool IsAbstract {get; set;}
		public Type TargetType { get; }
		public ObjectDefinition BaseDefinition { get; set; }
		public Dictionary<string, PropertyDefinition> Properties { get; }
		public Dictionary<string, QueryDefinition> Queries { get; }
	}

	public abstract class PropertyDefinition
	{
		public string Name { get; set; }
		
		public Type Type { get; }
		public string TypeName { get; set; }
		public ObjectDefinition TypeDef { get; }
		
		public PropertyAccess Access { get; set; }
		public object EmptyValue {get; set;}
		public object DefaultValue { get; set; }
		public bool AllowEmpty { get; set; }
		public List<PropertyConstraint> Constraints { get; }
	}

	public class PropertyDefinition<T>:PropertyDefinition
	{
		public new T EmptyValue
		{
			get { return (T)base.EmptyValue; }
			set { base.EmptyValue = value; }
		}
		public new T DefaultValue
		{
			get { return (T)base.DefaultValue; }
			set { base.DefaultValue = value; }
		}
	}

	public abstract class PropertyConstraint
	{
		public PropertyDefinition Property { get; set; }
		public object Value { get; set; }
		public bool EmptyValueOnDetach { get; set; }
	}

	public class PropertyConstraint<T>: PropertyConstraint
	{
		public new PropertyDefinition<T> Property
		{
			get { return (PropertyDefinition<T>)base.Property; }
		}

		public new T Value
		{
			get { return (T)base.Value; }
			set { base.Value = value; }
		}

	}

	public enum PropertyAccess
	{
		ReadWrite,
		ReadOnly,
		ReadWriteDetached
	}

	public class QueryDefinition
	{
		public string Name {get; set;}
		public QueryScope Scope { get; set; }
		public Type ReturnType { get; set; }
		public ObjectDefinition ReturnTypeObjectDefinition { get; set; }
		public List<QueryParameter> Parameters { get; }
	}

	public class QueryParameter
	{
		public string Name { get; set; }
		public string MapsTo { get; set; }
		public string Type {get; set;}
		public bool AllowEmpty { get; set; }
	}

	public enum QueryScope
	{
		Global,
		Local
	}

	public enum QueryReturn
	{
		Nothing,
		Value,
		Array,
		Reader,
		ResultSet
	}
}
