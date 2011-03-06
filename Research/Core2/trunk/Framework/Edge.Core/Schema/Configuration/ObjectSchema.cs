using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Edge.Core.Schema.Configuration
{
	public class ObjectSchema
	{
		public List<ObjectSet> ObjectSets { get; }
	}

	public class ObjectSet
	{
		public string Namespace { get; set; }
		public List<ObjectDefinition> Objects { get; }
	}

	public class ObjectDefinition
	{
		public string Name { get; set; }
		public string Namespace { get; set; }
		public bool IsAbstract {get; set;}
		public Type TargetType { get; }
		public ObjectDefinition BaseDefinition { get; set; }
		public List<PropertyDefinition> Properties { get; }
		public List<QueryDefinition> Queries { get; }
	}

	public class PropertyDefinition
	{
		public string Name { get; set; }
		public string TypeName { get; set; }
		public Type Type { get; }
		public ObjectDefinition TypeDef { get; }
		public PropertyAccess Access { get; set; }
		public object EmptyValue {get; set;}
		public object DefaultValue { get; set; }
		public bool AllowEmpty { get; set; }
		public List<PropertyConstraint> Constraints { get; }
	}

	public class PropertyConstraint
	{
		public PropertyDefinition Property { get; set; }
		public object Value { get; set; }
		public bool EmptyValueOnRemove { get; set; }
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
