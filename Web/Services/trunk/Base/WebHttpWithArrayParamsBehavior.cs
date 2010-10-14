using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Configuration;
using System.ComponentModel;
using System.Configuration;
using System.Text.RegularExpressions;

namespace EdgeBI.Web.DataServices
{

	public class WebHttpWithArrayParamsBehavior : WebHttpBehavior
	{
		string _delimeter = ",";

		public WebHttpWithArrayParamsBehavior():this(null)
		{
		}

		public WebHttpWithArrayParamsBehavior(string delimeter)
		{
			if (!String.IsNullOrEmpty(delimeter))
				_delimeter = delimeter;
		}

		protected override QueryStringConverter GetQueryStringConverter(OperationDescription operationDescription)
		{
			return new ArrayParameterConverter(_delimeter, base.GetQueryStringConverter(operationDescription));
		}

	}
	public class ArrayParameterConverter : QueryStringConverter
	{
		string _delimeter;
		QueryStringConverter _inner;

		public ArrayParameterConverter(string delimeter, QueryStringConverter inner)
		{
			this._inner = inner;
			this._delimeter = delimeter;
		}
		public override bool CanConvert(Type type)
		{
			if (type.IsArray)
				return _inner.CanConvert(type.GetElementType());
			else
				return _inner.CanConvert(type);
		}

		public override object ConvertStringToValue(string parameter, Type parameterType)
		{
			if (_inner.CanConvert(parameterType))
			{
				return _inner.ConvertStringToValue(parameter, parameterType);
			}
			else
			{
				if (parameter == null)
					return null;

				string[] parts = Regex.Split(parameter, @"(?<!\([^\)]*)"+_delimeter); // regex to not include commas in brackets
				Type arrayType = parameterType.GetElementType();
				Array result = Array.CreateInstance(arrayType, parts.Length);
				for (int i = 0; i < parts.Length; i++)
					result.SetValue(TypeDescriptor.GetConverter(arrayType).ConvertFrom(parts[i].Trim()), i);
				return result;
			}
		}
	}

	public class WebHttpWithArrayParamsBehaviorElement : BehaviorExtensionElement
	{
		[ConfigurationProperty("delimeter", DefaultValue=",")]
		public string Delimeter
		{
			get { return (string)this["delimeter"]; }
			set { this["delimeter"] = value; }
		}

		public override Type BehaviorType
		{
			get { return typeof(WebHttpWithArrayParamsBehavior); }
		}

		protected override object CreateBehavior()
		{
			return new WebHttpWithArrayParamsBehavior(this.Delimeter);
		}
	}
}
