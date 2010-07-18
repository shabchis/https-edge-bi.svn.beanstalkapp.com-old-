using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Easynet.Edge.UI.Data;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;


namespace Easynet.Edge.UI.Client
{
	/// <summary>
	/// Base class for settings pages in the Manager.
	/// </summary>
	public class AutocompleteCombo: ComboBox
	{
		TextBox _textbox;
		bool _selectionChanging = false;
		bool _textChanging = false;
		public event EventHandler ItemsSourceRequired;

		/// <summary>
		/// 
		/// </summary>
		public AutocompleteCombo()
		{
			this.Loaded += new RoutedEventHandler(AutocompleteCombo_Loaded);
			this.IsTextSearchEnabled = false;
			this.StaysOpenOnEdit = true;
			this.IsEditable = true;
		}

		public TextBox InnerTextBox
		{
			get
			{
				return _textbox;
			}
		}

		void AutocompleteCombo_Loaded(object sender, RoutedEventArgs e)
		{
			_textbox = Visual.GetDescendant<TextBox>(this);

			if (_textbox != null)
			{
				_textbox.TextChanged += new TextChangedEventHandler(_textbox_TextChanged);
			}

			//if (ItemsSourceRequired != null)
			//	ItemsSourceRequired(this, EventArgs.Empty);
		}

		/// <summary>
		/// 
		/// </summary>
		protected override void OnSelectionChanged(SelectionChangedEventArgs e)
		{
			if (_textChanging)
				return;

			_selectionChanging = true;
			base.OnSelectionChanged(e);
			_selectionChanging = false;

			if (_textbox != null)
			{
				_textbox.SelectionStart = _textbox.Text.Length;
				_textbox.SelectionLength = 0;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void _textbox_TextChanged(object source, TextChangedEventArgs e)
		{
			if (_selectionChanging)
				return;

			// This is so that selection change doesn't fire when changing the ItemsSource
			_textChanging = true;

			// Tell the host that this control needs updating based on input
			if (ItemsSourceRequired != null)
			    ItemsSourceRequired(this, EventArgs.Empty);

			if (this.ItemsSource == null)
			{
			    this.IsDropDownOpen = false;
			}
			else
			{
			    if (!this.IsDropDownOpen)
			    {
			        this.IsDropDownOpen = true;
			        _textbox.SelectionStart = _textbox.Text.Length;
			        _textbox.SelectionLength = 0;
			    }
			}

			_textChanging = false;
		}
	}
}
