using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Data;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace Easynet.Edge.UI.Client
{
	#region Validators

	/// <summary>
	/// Base class for custom validating bindings.
	/// </summary>
	public class ValidatingBinding<RuleType>: Binding where RuleType: ValidatingBindingRuleBase, new()
	{
		public ValidatingBinding(): base()
		{
			this.ValidationRules.Add(new ExceptionValidationRule());
			this.ValidationRules.Add(new RuleType());
		}
		public ValidatingBinding(string path): base(path)
		{
			this.ValidationRules.Add(new ExceptionValidationRule());
			this.ValidationRules.Add(new RuleType());
		}

		public RuleType Rule
		{
			get
			{
				return this.ValidationRules[1] as RuleType;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string ErrorMessage
		{
			get
			{
				return this.Rule.ErrorMessage;
			}
			set
			{
				this.Rule.ErrorMessage = value;
			}
		}

	}

	/// <summary>
	/// Base class for custom validating binding rules.
	/// </summary>
	public class ValidatingBindingRuleBase: ValidationRule
	{
		string _errorMessage = null;
		
		/// <summary>
		/// 
		/// </summary>
		public string ErrorMessage
		{
			get
			{
				return _errorMessage;
			}
			set
			{
				_errorMessage = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="cultureInfo"></param>
		/// <returns></returns>
		public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
		{
			return new ValidationResult(true, null);
		}

	}


	/// <summary>
	/// Temporary till bug fix of Markup Extension parser
	/// (see http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=289238)
	/// </summary>
	public class StringBinding: ValidatingBinding<StringValidationRule>
	{
		/// <summary>
		/// 
		/// </summary>
		public string RegexPattern
		{
			get
			{
				return this.Rule.RegexPattern;
			}
			set
			{
				this.Rule.RegexPattern = value;
			}
		}


	}

	/// <summary>
	/// 
	/// </summary>
	public class StringValidationRule: ValidatingBindingRuleBase
	{
		Regex _validator = null;

		/// <summary>
		/// 
		/// </summary>
		public string RegexPattern
		{
			get
			{
				return _validator != null ? _validator.ToString() : null;
			}
			set
			{
				_validator = new Regex(value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
		{
			string errorMsg;
			Regex rx;

			if (_validator == null)
			{
				rx = new Regex(".+");
				errorMsg = "This field is required";
			}
			else
			{
				rx = _validator;
				errorMsg = String.Format("This field's value must match the pattern {0}", rx.ToString());
			}
			errorMsg = ErrorMessage != null ? ErrorMessage : errorMsg;

			bool valid = rx.IsMatch(value as string);
			if (valid)
				return new ValidationResult(true, null);
			else
				return new ValidationResult(false, errorMsg);
		}

	}


	/// <summary>
	/// Temporary till bug fix of Markup Extension parser
	/// (see http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=289238)
	/// </summary>
	public class NumberBinding: ValidatingBinding<NumberValidationRule>
	{
		public bool AllowEmpty
		{
			get
			{
				return this.Rule.AllowEmpty;
			}
			set
			{
				this.Rule.AllowEmpty = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double MinValue
		{
			get
			{
				return this.Rule.MinValue;
			}
			set
			{
				this.Rule.MinValue = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double MaxValue
		{
			get
			{
				return this.Rule.MaxValue;
			}
			set
			{
				this.Rule.MaxValue = value;
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class NumberValidationRule: ValidatingBindingRuleBase
	{
		double _minValue = double.MinValue;
		double _maxValue = double.MaxValue;
		bool _allowEmpty = false;

		/// <summary>
		/// 
		/// </summary>
		public double MinValue
		{
			get
			{
				return _minValue;
			}
			set
			{
				_minValue = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double MaxValue
		{
			get
			{
				return _maxValue;
			}
			set
			{
				_maxValue = value;
			}
		}

		public bool AllowEmpty
		{
			get
			{
				return _allowEmpty;
			}
			set
			{
				_allowEmpty = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
		{
			string errorMsg = ErrorMessage != null ?
				ErrorMessage :
				String.Format("Value must be a number between {0} and {1}", _minValue, _maxValue);

			double val;
			bool empty = false;

			if (value is int || value is long || value is double || value is float)
			{
				val = Convert.ToDouble(value);
			}
			else if (this.AllowEmpty && value != null && value.ToString().Trim().Length < 1)
			{
				empty = true;
				val = 0; // just to pass compilation, this value is not used
			}
			else if (!Double.TryParse(value.ToString(), out val))
			{
				// Failed to parse
				return new ValidationResult(false, errorMsg);
			}

			// Check the range
			if (empty || (val >= _minValue && val <= _maxValue))
				return new ValidationResult(true, null);
			else
				return new ValidationResult(false, errorMsg);
		}

		public double? GetNumber(object value)
		{
			double val;

			if (value is int || value is long || value is double || value is float)
			{
				return Convert.ToDouble(value);
			}
			else if (this.AllowEmpty && value != null && value.ToString().Trim().Length < 1)
			{
				return null;
			}
			else if (Double.TryParse(value.ToString(), out val))
			{
				return val;
			}
			else
			{
				// Failed to parse
				return null;
			}
		}

	}

#endregion


}
