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
using Easynet.Edge.UI.Data;

using System.Windows.Controls.Primitives;
using System.Data;
using System.Reflection;
using Easynet.Edge.UI.Server;

namespace Easynet.Edge.UI.Client.Pages
{
	/// <summary>
	/// Interaction logic for MasterKeywords.xaml
	/// </summary>
	[AccountDependentPage]
	public partial class MasterKeywords: PageBase
	{
		#region Fields
		/*=========================*/

		private ObservableCollection<DataRow> _items;
		Oltp.KeywordDataTable _keywords;
		TextBox _keywordValueField = null;
		ItemsControl _assoc_Campaigns;

		/*=========================*/
		#endregion

		#region Constructors
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		public MasterKeywords()
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
		public override void OnAccountChanged()
		{
			GetKeywords(Window.CurrentAccount, null, false);
		}

		/// <summary>
		/// 
		/// </summary>
		private void _filterButton_Click(object sender, RoutedEventArgs e)
		{
			//if (String.IsNullOrEmpty(_filterText.Text.Trim()) && _filterCheckbox.IsChecked != false)
			//{
			//    MessageBox.Show("Filter cannot be empty when displaying unmonitored keywords.",
			//        "Not allowed", MessageBoxButton.OK, MessageBoxImage.Exclamation
			//        );
			//    return;
			//}

			string searchString = _filterText.Text.IndexOf('*') > -1 ?
				_filterText.Text.Replace('*', '%') :
				(_filterText.Text.Length < 1 ? null : _filterText.Text + "%");

			GetKeywords(null, searchString, true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="include"></param>
		private void GetKeywords(Oltp.AccountRow account, string filter, bool include)
		{
			Oltp.AccountRow currentAccount = account == null ? this.Window.CurrentAccount : account;

			if (currentAccount == null)
				return;

			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					_keywords = proxy.Service.Keyword_Get(currentAccount.ID, false, filter, include);
				}
			},
			delegate()
			{

				// Get an empty new list
				if (_items == null)
					_items = new ObservableCollection<DataRow>();
				else
					_items.Clear();

				// Add all items
				foreach (DataRow r in _keywords.Rows)
					_items.Add(r);

				_listTable.ListView.ItemsSource = _items;
			});
		}

		/*=========================*/
		#endregion

		#region Keywords
		/*=========================*/

		/// <summary>
		/// Add a gateway to an dataItem
		/// </summary>
		private void Keyword_AddClick(object sender, RoutedEventArgs e)
		{
			// Create an editable new row
			Oltp.KeywordRow editVersion = Dialog_MakeEditVersion<Oltp.KeywordDataTable, Oltp.KeywordRow>(null);
			editVersion.AccountID = this.Window.CurrentAccount.ID;
			editVersion.IsMonitored = true;

			// Show the dialog
			Keyword_dialog.Title = "New Keyword";

			// Enable editing keyword value
			_keywordValueField.IsEnabled = true;

			Keyword_dialog.BeginEdit(editVersion, _keywords);
		}

		/// <summary>
		/// Open the dialog
		/// </summary>
		private void Keyword_dialog_Open(object sender, RoutedEventArgs e)
		{
			// Set dataItem as current item
			ListViewItem currentItem = _listTable.GetParentListViewItem(e.OriginalSource as FrameworkElement);
			Oltp.KeywordRow row = currentItem.Content as Oltp.KeywordRow;

			// Show the dialog
			Keyword_dialog.Title = row.Keyword;
			Keyword_dialog.TitleTooltip = "GK #" + row.GK.ToString();

			// Disable editing keyword value
			if (_keywordValueField != null)
				_keywordValueField.IsEnabled = false;

			Keyword_dialog.BeginEdit(
				Dialog_MakeEditVersion<Oltp.KeywordDataTable, Oltp.KeywordRow>(row),
				row
			);
		}

		/// <summary>
		/// 
		/// </summary>
		private void AssociationsTabItem_GotFocus(object sender, RoutedEventArgs e)
		{
			if (!(Keyword_dialog.TargetContent is Oltp.KeywordRow))
				return;

			// Show 
			if (_assoc_Campaigns == null)
				_assoc_Campaigns = VisualTree.GetChild<ItemsControl>(Keyword_dialog, "_assoc_Campaigns");

			if (_assoc_Campaigns.ItemsSource != null)
				return;

			Oltp.KeywordRow keyword = (Keyword_dialog.TargetContent as Oltp.KeywordRow);
			List<CampaignAdgroupCombination> duos = null;

			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					duos = new List<CampaignAdgroupCombination>();
					Oltp.AdgroupDataTable adgroups = proxy.Service.Adgroup_GetByKeyword(keyword.GK);

					if (adgroups.Rows.Count > 0)
					{
						// Get all parent campaign IDs
						List<long> campaignGKs = new List<long>();
						foreach (Oltp.AdgroupRow ag in adgroups.Rows)
						{
							if (!campaignGKs.Contains(ag.CampaignGK))
								campaignGKs.Add(ag.CampaignGK);
						}

						// Now get the campaigns themselves
						Oltp.CampaignDataTable campaigns = proxy.Service.Campaign_GetIndividualCampaigns(campaignGKs.ToArray());
						foreach (Oltp.AdgroupRow ag in adgroups.Rows)
						{
							DataRow[] rs = campaigns.Select(String.Format("GK = {0}", ag.CampaignGK));
							if (rs.Length > 0)
							{
								duos.Add(new CampaignAdgroupCombination(rs[0] as Oltp.CampaignRow, ag));
							}
						}
					}
				}
			},
			delegate()
			{
				_assoc_Campaigns.ItemsSource = duos;
			});
		}

		/// <summary>
		/// 
		/// </summary>
		private void Keyword_dialog_ApplyingChanges(object sender, CancelRoutedEventArgs e)
		{
			Keyword_dialog.EndApplyChanges(e);
			return;

			#region Disabled, hopefully for good
			/*
			// Validate keyword field
			if (_keywordValueField.Text.Trim().Length < 1)
			{
				VisualTree.GetChild<TabControl>(Keyword_dialog).SelectedIndex = 0;
				_keywordValueField.Focus();

				MainWindow.MessageBoxError("Please enter a valid keyword", null);
				e.Cancel = true; // No significance since we are not calling dialog.EndApplyChanges()
				return;
			}

			int currentAccountID = this.Window.CurrentAccount.ID;
			string keywordValueFieldText = _keywordValueField.Text;
			Oltp.KeywordDataTable tbl = null;
			bool added = (Keyword_dialog.Content as Oltp.KeywordRow).RowState == DataRowState.Added;

			Window.AsyncOperation(delegate()
			{
				// When adding a new keyword, make sure it is unique
				if (added)
				{
					using (OltpProxy proxy = new OltpProxy())
					{
						tbl = proxy.Service.Keyword_Get(currentAccountID, false, keywordValueFieldText, true);
					}
				}
			},
			delegate()
			{
				if (tbl != null && tbl.Rows.Count > 0)
				{
					MainWindow.MessageBoxError("Keyword already exists.", null);
					e.Cancel = true;
					return;
				}

				Dialog_ApplyingChanges<Oltp.KeywordDataTable, Oltp.KeywordRow>(
					_keywords,
					Keyword_dialog,
					typeof(IOltpLogic).GetMethod("Keyword_Save"),
					e, null, true, null);
			});
			 */
			#endregion
		}

		/// <summary>
		/// 
		/// </summary>
		private void Keyword_dialog_AppliedChanges(object sender, CancelRoutedEventArgs e)
		{
			return;

			#region Disabled, hopefully for good
			/*
			Oltp.KeywordRow dataItem = Keyword_dialog.TargetContent as Oltp.KeywordRow;

			// Call the univeral applied change handler
			Dialog_AppliedChanges<Oltp.KeywordRow>(Keyword_dialog, dataItem.Keyword, _listTable.ListView, e);

			// Disable editing keyword value
			_keywordValueField.IsEnabled = false;

			// Set correct data template
			ListViewItem item = _listTable.ListView.ItemContainerGenerator.ContainerFromItem(dataItem) as ListViewItem;
			(this.Resources["NameTemplateSelector"] as MasterKeywordsLocal.NameTemplateSelector)
				.ApplyTemplate(dataItem, item);
			*/
			#endregion
		}

		/// <summary>
		/// 
		/// </summary>
		private void Keyword_dialog_Closing(object sender, CancelRoutedEventArgs e)
		{
			// Cancel if user regrets
			e.Cancel = MainWindow.MessageBoxPromptForCancel(Keyword_dialog);

			if (e.Cancel)
				return;

			_keywords.RejectChanges();

			if (_assoc_Campaigns != null)
				_assoc_Campaigns.ItemsSource = null;
		}

		/// <summary>
		/// 
		/// </summary>
		private void Keyword_dialog_Loaded(object sender, RoutedEventArgs e)
		{
			_keywordValueField = VisualTree.GetChild<TextBox>(Keyword_dialog, "_keywordValue");
		}

		/// <summary>
		/// 
		/// </summary>
		private void Keyword_ChangeMonitoring(object sender, RoutedEventArgs e)
		{
			bool monitor = sender == _buttonMonitor;

			// First mark the correct monitoring state
			foreach (Oltp.KeywordRow kw in _listTable.ListView.SelectedItems)
				kw.IsMonitored = monitor;
			

			try
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					proxy.Service.Keyword_Save( Oltp.Prepare<Oltp.KeywordDataTable>(_keywords) );
				}
			}
			catch (Exception ex)
			{
				// Failed, so cancel and display a message
				MainWindow.MessageBoxError("Keywords could not be updated.", ex);
				_keywords.RejectChanges();
				return;
			}

			Oltp.KeywordRow[] rowsToIterate = new Oltp.KeywordRow[_listTable.ListView.SelectedItems.Count];
			_listTable.ListView.SelectedItems.CopyTo(rowsToIterate, 0);

			foreach (Oltp.KeywordRow kw in rowsToIterate)
			{
				if (_filterCheckbox.IsChecked == false && !monitor)
				{
					// Remove keywords that have been unmonitored
					kw.Delete();
					_items.Remove(kw);
				}
				else
				{
					ListViewItem item = _listTable.ListView.ItemContainerGenerator.ContainerFromItem(kw) as ListViewItem;
					
					// Apply the correcy template
					(this.Resources["NameTemplateSelector"] as MasterKeywordsLocal.NameTemplateSelector)
						.ApplyTemplate(kw, item);
				}
			}

			_keywords.AcceptChanges();
		}

		/*=========================*/
		#endregion

	}
}

namespace Easynet.Edge.UI.Client.MasterKeywordsLocal
{

	public class NameTemplateSelector: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			Oltp.KeywordRow kw = item as Oltp.KeywordRow;

			if (kw == null)
				return new DataTemplate();
			else if (kw.IsMonitored)
				return App.CurrentPage
					.FindResource("MonitoredNameTemplate") as DataTemplate;
			else
				return App.CurrentPage
					.FindResource("UnmonitoredNameTemplate") as DataTemplate;
		}

		public void ApplyTemplate(Oltp.KeywordRow dataItem, ListViewItem item)
		{
			if (item == null)
				return;

			Button nameButton = VisualTree.GetChild<Button>(item, "_itemName");
			if (nameButton == null)
				return;

			ContentPresenter cp = VisualTreeHelper.GetParent(nameButton) as ContentPresenter;
			cp.ContentTemplate = SelectTemplate(dataItem, item);
		}
	}

}