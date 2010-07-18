using System;
using System.Collections.ObjectModel;
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
using Easynet.Edge2.Data;
using Easynet.Edge2.UI.OltpLogic;
using System.Windows.Controls.Primitives;
using System.Data;
using System.Reflection;
using Easynet.Edge2.Core.Data;
using Easynet.Edge2.Data.Objects;
using System.Collections;
using System.Threading;
using Easynet.Edge2.Core.Data.Proxy;

namespace Easynet.Edge2.UI.Pages
{
	/// <summary>
	/// Interaction logic for MasterKeywordsTest.xaml
	/// </summary>
	[AccountDependentPage]
	public partial class MasterKeywordsTest: PageBase
	{
		#region Fields
		/*=========================*/

		ObservableCollection<Keyword> _displayItems;
		AccountKeywordTable _keywords;

		/*=========================*/
		#endregion

		#region Constructors
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		public MasterKeywordsTest()
		{
			InitializeComponent();
			this.Loaded += new RoutedEventHandler(Page_Loaded);
		}

		/*=========================*/
		#endregion

		#region General Methods
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		void Page_Loaded(object sender, RoutedEventArgs e)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool ChangeAccount(Oltp.AccountRow value)
		{
			// Permission check
			if (!HasPermission(value))
			{
				HidePageContents();
				return true;
			}
			else
				RestorePageContents();

			GetKeywords(value, null, false);
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		private void _filterButton_Click(object sender, RoutedEventArgs e)
		{
			string searchString = _filterText.Text.IndexOf('*') > -1 ?
				_filterText.Text.Replace('*', '%') :
				(_filterText.Text.Length < 1 ? null : _filterText.Text);

			GetKeywords(null, searchString, _filterCheckbox.IsChecked == true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="include"></param>
		private void GetKeywords(Account account, string filter, bool include)
		{
			Account currentAccount = account == null ? this.Window.CurrentAccount : account;

			if (currentAccount == null)
				return;

			// Initialize the display item collection
			_displayItems = new ObservableCollection<Keyword>();
			using (Proxy.Start(true))
			{
				AccountKeywordTable.Get(currentAccount, filter, 0, include, false, delegate(object s, BoundItemRecievedEventArgs e)
				{
					// Called on every item received during the bind
					_displayItems.Add(item);
				});

				Proxy.OnComplete = delegate()
				{
					_keywords = (AccountKeywordTable) Proxy.Result[0];
					_listTable.InnerListView.ItemsSource = _displayItems;
				};
			}
		}

		/*=========================*/
		#endregion

		#region Keywords
		/*=========================*/

		/// <summary>
		/// Add a gateway to an dataItem
		/// </summary>
		private void NewItem(object sender, RoutedEventArgs e)
		{
			// Create an editable new row
			Keyword newDataItem = new Keyword();
			newDataItem.Account = this.Window.CurrentAccount;
			newDataItem.IsMonitored = true;

			// Show the dialog
			_keywordDialog.Title = "New Keyword";
			_keywordDialog.BeginEdit(newDataItem); // , _keywords);
		}

		/// <summary>
		/// Open the dialog
		/// </summary>
		private void OpenItem(object sender, RoutedEventArgs e)
		{
			// Set dataItem as current item
			ListViewItem currentListItem = _listTable.GetParentListViewItem(e.OriginalSource as FrameworkElement);
			Keyword dataItem = currentListItem.Content as Oltp.KeywordRow;
			dataItem.NotifyOnPropertyChanged = false;
			//Keyword editVersion = dataItem.Duplicate();

			// Show the dialog
			_keywordDialog.Title = row.Keyword;
			_keywordDialog.BeginEdit(dataItem);
			//_keywordDialog.BeginEdit(editVersion, dataItem);

			// When opening, select it only if no more than one is already selected
			if (_listTable.InnerListView.SelectedItems.Count < 2)
			{
				_listTable.InnerListView.SelectedItems.Clear();
				currentListItem.IsSelected = true;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		private void ChangeMonitoring(object sender, RoutedEventArgs e)
		{
			bool monitor = sender == _buttonMonitor;
			Keyword[] itemsArray = new Keyword[_listTable.InnerListView.SelectedItems.Count];
			_listTable.InnerListView.SelectedItems.CopyTo(itemsArray, 0);

			// First mark the correct monitoring state
			foreach (Keyword item in itemsArray)
			{
				item.IsMonitored = monitor;

				if (!_filterCheckbox.IsChecked)
				{
					_displayItems.Remove(item);
				}
				else
				{
					// Apply the correct template
					ListViewItem listViewItem = _listTable.InnerListView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
					(this.Resources["NameTemplateSelector"] as MasterKeywordsTestLocal.NameTemplateSelector).ApplyTemplate(item, listViewItem);
				}
			}

			using (Proxy.Start(true))
			{
				_keywords.Save();
			}
		}

		/*=========================*/
		#endregion

		private void _dialog_ApplyingChanges(object sender, CancelRoutedEventArgs e)
		{
			Keyword item = _dialog.Content as Keyword;
			if (item.DataState == DataRowState.Unchanged)
				return;

			bool isNew = item.DataState == DataRowState.Added;

			ProxyResult result = null;
			using (Proxy.Start(true))
			{
				_keywords.Save();

				Proxy.OnComplete = delegate()
				{
					result = Proxy.Result;
				};
			}

			// Process UI events and such while updating
			while (result == null)
				App.DoEvents();

			if (result.Error != null)
			{
				MessageBoxError("Failed to save keyword.", result.Error);
				e.Cancel = true;
			}
			else
			{
				item.NotifyOnPropertyChanged = true;

				if (isNew)
				{
					_displayItems.Add(item);
					_listTable.InnerListView.SelectedItem = item;
					_listTable.InnerListView.ScrollIntoView(item);
				}
				else
				{
					// Update property changes
					ListViewItem listViewItem = (ListViewItem) _listTable.InnerListView.ItemContainerGenerator.ContainerFromItem(item);
					listViewItem.UpdateLayout();
				}
			}
		}
	}
}

#region Local utility classes
namespace Easynet.Edge2.UI.MasterKeywordsTestLocal
{

	public class NameTemplateSelector: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			Keyword kw = item as Keyword;

			if (kw == null)
				return new DataTemplate();
			else if (kw.IsMonitored)
				return App.CurrentPage
					.FindResource("MonitoredNameTemplate") as DataTemplate;
			else
				return App.CurrentPage
					.FindResource("UnmonitoredNameTemplate") as DataTemplate;
		}

		public void ApplyTemplate(Keyword dataItem, ListViewItem listViewItem)
		{
			if (listViewItem == null)
				return;

			Button nameButton = Visual.GetDescendant<Button>(listViewItem, "_itemName");
			if (nameButton == null)
				return;

			ContentPresenter cp = VisualTreeHelper.GetParent(nameButton) as ContentPresenter;
			cp.ContentTemplate = SelectTemplate(dataItem, listViewItem);
		}
	}
}
#endregion