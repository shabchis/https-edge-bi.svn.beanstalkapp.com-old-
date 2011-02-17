using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Easynet.Edge.UI.Data;

namespace Easynet.Edge.UI.Client
{

	#region Visual helpers
	public class VisualTree
	{
		public static List<T> GetChildren<T>(DependencyObject parent) where T : DependencyObject
		{
			if (parent == null)
				throw new ArgumentNullException("parent");

			// Popup needs special treament
			int childCount = parent is Popup ?
				1 :
				VisualTreeHelper.GetChildrenCount(parent);

			List<T> list = new List<T>();

			for (int i = 0; i < childCount; i++)
			{
				DependencyObject child = parent is Popup ?
					(parent as Popup).Child :
					VisualTreeHelper.GetChild(parent, i);

				if (child is T)
					list.Add(child as T);
				else
					list.AddRange(GetChildren<T>(child));
			}

			return list;

		}

		public static T GetChild<T>(DependencyObject parent, string name) where T: DependencyObject
		{
			if (parent == null)
				throw new ArgumentNullException("parent");

			// Popup needs special treament
			int childCount = parent is Popup ? 
				1 :
				VisualTreeHelper.GetChildrenCount(parent);

			for (int i = 0; i < childCount; i++)
			{
				DependencyObject child = parent is Popup ?
					(parent as Popup).Child :
					VisualTreeHelper.GetChild(parent, i);

				if (
						child is T &&
						(name == null ||(child is FrameworkElement && (child as FrameworkElement).Name == name))
					)
					return child as T;

				child = GetChild<T>(child, name);
				if (child != null)
					return child as T;
			}

			return null;
		}


		public static T GetChild<T>(DependencyObject parent) where T: DependencyObject
		{
			return GetChild<T>(parent, null);
		}


		public static T GetParent<T>(DependencyObject element, Func<DependencyObject,bool> test) where T : DependencyObject
		{
			if (element == null)
				return null;

			DependencyObject parent = VisualTreeHelper.GetParent(element);
			if (parent is T && (test == null || test(parent)))
				return parent as T;
			else
				return GetParent<T>(parent);
		}

		public static T GetParent<T>(DependencyObject element) where T : DependencyObject
		{
			return GetParent<T>(element, null);
		}


	}

	#endregion

	#region Access key scope

	/// <summary>
	/// Thanks to Neil Mosafi
	/// http://neilmosafi.blogspot.com/2007/04/default-buttons-in-wpf-and-multiple.html
	/// </summary>
    public static class AccessKeyScoper
    {
        /// <summary>
        ///    Identifies the IsAccessKeyScope attached dependency property
        /// </summary>
        public static readonly DependencyProperty IsAccessKeyScopeProperty =
            DependencyProperty.RegisterAttached("IsAccessKeyScope", typeof(bool), typeof(AccessKeyScoper), new PropertyMetadata(false, HandleIsAccessKeyScopePropertyChanged));

        /// <summary>
        ///    Sets the IsAccessKeyScope attached property value for the specified object
        /// </summary>
        /// <param name="obj">The object to retrieve the value for</param>
        /// <param name="value">Whether the object is an access key scope</param>
        public static void SetIsAccessKeyScope(DependencyObject obj, bool value)
        {
            obj.SetValue(AccessKeyScoper.IsAccessKeyScopeProperty, value);
        }

        /// <summary>
        ///    Gets the value of the IsAccessKeyScope attached property for the specified object
        /// </summary>
        /// <param name="obj">The object to retrieve the value for</param>
        /// <returns>The value of IsAccessKeyScope attached property for the specified object</returns>
        public static bool GetIsAccessKeyScope(DependencyObject obj)
        {
            return (bool) obj.GetValue(AccessKeyScoper.IsAccessKeyScopeProperty);
        }

        private static void HandleIsAccessKeyScopePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue.Equals(true))
            {
                AccessKeyManager.AddAccessKeyPressedHandler(d, HandleScopedElementAccessKeyPressed);
            }
            else
            {
                AccessKeyManager.RemoveAccessKeyPressedHandler(d, HandleScopedElementAccessKeyPressed);
            }
        }

        private static void HandleScopedElementAccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt) && GetIsAccessKeyScope((DependencyObject)sender))
            {
                e.Scope = sender;
                e.Handled = true;
            }
        }
    }

#endregion

	public class CampaignAdgroupCombination
	{
		protected Oltp.CampaignRow _campaign = null;
		protected Oltp.AdgroupRow _adgroup = null;

		public CampaignAdgroupCombination(Oltp.CampaignRow campaign, Oltp.AdgroupRow adgroup)
		{
			_campaign = campaign;
			_adgroup = adgroup;
		}

		public Oltp.CampaignRow Campaign { get { return _campaign; } }
		public Oltp.AdgroupRow Adgroup { get { return _adgroup; } }
	}

	[ValueConversion(typeof(bool), typeof(bool))]
	public class BoolInverter: IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value is bool ? !((bool)value) : value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return Convert(value, targetType, parameter, culture);
		}

		#endregion
	}

	[ValueConversion(typeof(Visibility), typeof(bool))]
	public class InvisibleToBoolConverter: IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value is Visibility ? (Visibility) value != Visibility.Visible : false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value is bool && ((bool) value) == true ? Visibility.Visible : Visibility.Hidden;
		}

		#endregion
	}

	[ValueConversion(typeof(Visibility), typeof(bool))]
	public class VisibleToBoolConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value is Visibility ? (Visibility) value == Visibility.Visible : false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value is bool && ((bool) value) == true ? Visibility.Collapsed : Visibility.Visible;
		}

		#endregion
	}

	[ValueConversion(typeof(Visibility), typeof(bool))]
	public class BoolToInvisibleConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value is bool && ((bool)value) == true ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value is Visibility ? (Visibility)value != Visibility.Visible : false;
		}

		#endregion
	}

	[ValueConversion(typeof(Visibility), typeof(bool))]
	public class BoolToVisibleConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value is bool && ((bool)value) == true ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value is Visibility ? (Visibility)value == Visibility.Visible : false;
		}

		#endregion
	}

	[ValueConversion(typeof(Visibility), typeof(double))]
	public class DoubleToVisibleConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value is double && ((double)value) == 1.0 ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value is Visibility ? (Visibility)value == Visibility.Visible ? 1.0 : 0.0 : 0.0;
		}

		#endregion
	}

	[ValueConversion(typeof(bool), typeof(object))]
	public class BoolConverter<T> : IValueConverter
	{
		public T TrueValue;
		public T FalseValue;

		public BoolConverter(T trueValue, T falseValue)
		{
			TrueValue = trueValue;
			FalseValue = falseValue;
		}

		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is bool && (bool)value)
			{
				return TrueValue;
			}
			else
				return FalseValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is T && ((T)value).Equals(TrueValue))
			{
				return true;
			}
			else
				return false;
		}

		#endregion
	}

	[ValueConversion(typeof(string), typeof(object))]
	public class NullableNumberConverter<ValueT> : IValueConverter where ValueT : struct
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value == null ? string.Empty : value.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			ValueT? retVal;

			if (value is ValueT)
			{
				retVal = (ValueT)value;
			}
			else if (value == null || value.ToString().Length < 1)
			{
				retVal = null;
			}
			else
			{
				retVal = (ValueT)System.Convert.ChangeType(value, typeof(ValueT));
			}

			return retVal;
		}
	}

}