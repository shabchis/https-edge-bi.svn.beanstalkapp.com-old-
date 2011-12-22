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
	/// Interaction logic for PpcCreatives.xaml
	/// </summary>
	[AccountDependentPage]
	public partial class MasterCreatives: PageBase
	{
		#region Fields
		/*=========================*/

		private ObservableCollection<DataRow> _items;
		Oltp.CreativeDataTable _creatives;
		TextBox _titleField = null;
		ItemsControl _assoc_Campaigns;

		/*=========================*/
		#endregion

		#region Constructors
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		public MasterCreatives()
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
			GetCreatives(Window.CurrentAccount, null, false);
		}

		/// <summary>
		/// 
		/// </summary>
		private void _filterButton_Click(object sender, RoutedEventArgs e)
		{
			string searchString = _filterText.Text.IndexOf('*') > -1 ?
				_filterText.Text.Replace('*', '%') :
				(_filterText.Text.Length < 1 ? null : _filterText.Text + '%');

			GetCreatives(null, searchString, true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="include"></param>
		private void GetCreatives(Oltp.AccountRow account, string filter, bool include)
		{
			Oltp.AccountRow currentAccount = account == null ? this.Window.CurrentAccount : account;

			if (currentAccount == null)
				return;

			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					_creatives = proxy.Service.Creative_Get(currentAccount.ID, filter, include);
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
				foreach (DataRow r in _creatives.Rows)
					_items.Add(r);

				_listTable.ListView.ItemsSource = _items;
			});
		}

		/*=========================*/
		#endregion

		#region Creatives
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		private void Keyword_AddClick(object sender, RoutedEventArgs e)
		{
			//// Create an editable new row
			//Oltp.CreativeRow editVersion = Dialog_MakeEditVersion<Oltp.CreativeDataTable, Oltp.CreativeRow>(_creatives, null);
			//editVersion.AccountID = this.Window.CurrentAccount.ID;
			//editVersion.IsMonitored = true;

			//// Show the dialog
			//Creative_dialog.Title = "New Keyword";

			//// Enable editing keyword value
			//_keywordValueField.IsReadOnly = false;

			//Creative_dialog.BeginEdit(editVersion, _creatives);
		}

		/// <summary>
		/// Open the dialog
		/// </summary>
		private void Creative_dialog_Open(object sender, RoutedEventArgs e)
		{
			// Set dataItem as current item
			ListViewItem currentItem = _listTable.GetParentListViewItem(e.OriginalSource as FrameworkElement);
			Oltp.CreativeRow row = currentItem.Content as Oltp.CreativeRow;

			// Show the dialog
			Creative_dialog.Title = row.Title;
			Creative_dialog.TitleTooltip = "GK #" + row.GK.ToString();

			Creative_dialog.BeginEdit(
				Dialog_MakeEditVersion<Oltp.CreativeDataTable, Oltp.CreativeRow>(row),
				row
			);

			TabControl tabs = VisualTree.GetChild<TabControl>(Creative_dialog);
			if (tabs.SelectedIndex == 1)
			{
				AssociationsTabItem_GotFocus(null, null);
			}
			
			// Select the item
			currentItem.IsSelected = true;
		}


		/// <summary>
		/// 
		/// </summary>
		private void AssociationsTabItem_GotFocus(object sender, RoutedEventArgs e)
		{
			if (!(Creative_dialog.TargetContent is Oltp.CreativeRow))
				return;
	
			// Show 
			if (_assoc_Campaigns == null)
				_assoc_Campaigns = VisualTree.GetChild<ItemsControl>(Creative_dialog, "_assoc_Campaigns");

			if (_assoc_Campaigns.ItemsSource != null)
				return;

			Oltp.CreativeRow creative = (Creative_dialog.TargetContent as Oltp.CreativeRow);
			List<CampaignAdgroupCombination> duos = null;

			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					duos = new List<CampaignAdgroupCombination>();
					Oltp.AdgroupDataTable adgroups = proxy.Service.Adgroup_GetByCreative(creative.GK);

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
		private void Creative_dialog_ApplyingChanges(object sender, CancelRoutedEventArgs e)
		{
			if (_titleField.Text.Trim().Length < 1)
			{
				VisualTree.GetChild<TabControl>(Creative_dialog).SelectedIndex = 0;
				_titleField.Focus();

				MessageBoxError("Please enter a valid title", null);
				e.Cancel = true;
				return;
			}

			Dialog_ApplyingChanges<Oltp.CreativeDataTable, Oltp.CreativeRow>(
				_creatives,
				Creative_dialog,
				typeof(IOltpLogic).GetMethod("Creative_Save"),
				e, null, true, null);
		}

		/// <summary>
		/// 
		/// </summary>
		private void Creative_dialog_AppliedChanges(object sender, CancelRoutedEventArgs e)
		{
			// Call the univeral applied change handler
			Dialog_AppliedChanges<Oltp.CreativeRow>(Creative_dialog, "Editing Creative", _listTable._listView, e);
		}

		/// <summary>
		/// 
		/// </summary>
		private void Creative_dialog_Closing(object sender, CancelRoutedEventArgs e)
		{
			// Cancel if user regrets
			e.Cancel = MessageBoxPromptForCancel(Creative_dialog);

			if (!e.Cancel)
				_creatives.RejectChanges();

			if (_assoc_Campaigns != null)
				_assoc_Campaigns.ItemsSource = null;
		}

		/// <summary>
		/// 
		/// </summary>
		private void Creative_dialog_Loaded(object sender, RoutedEventArgs e)
		{
			// Needed for validation
			_titleField = VisualTree.GetChild<TextBox>(Creative_dialog, "_titleField");
		}

		private void Creative_ChangeMonitoring(object sender, RoutedEventArgs e)
		{
			bool monitor = sender == _buttonMonitor;

			// First mark the correct monitoring state
			foreach (Oltp.CreativeRow creative in _listTable._listView.SelectedItems)
				creative.IsMonitored = monitor;

			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					proxy.Service.Creative_Save(Oltp.Prepare<Oltp.CreativeDataTable>(_creatives));
				}
			},
			delegate(Exception ex)
			{
				// Failed, so cancel and display a message
				MessageBoxError("Creatives could not be updated.", ex);
				_creatives.RejectChanges();
				return false;
			},
			delegate()
			{
				Oltp.CreativeRow[] rowsToIterate = new Oltp.CreativeRow[_listTable._listView.SelectedItems.Count];
				_listTable._listView.SelectedItems.CopyTo(rowsToIterate, 0);

				foreach (Oltp.CreativeRow creative in rowsToIterate)
				{
					if (_filterCheckbox.IsChecked == false && !monitor)
					{
						// Remove creatives that have been unmonitored
						creative.Delete();
						_items.Remove(creative);
					}
					else
					{
						ListViewItem item = _listTable._listView.ItemContainerGenerator.ContainerFromItem(creative) as ListViewItem;

						// Apply the correcy template
						(this.Resources["NameTemplateSelector"] as MasterCreativesLocal.NameTemplateSelector)
							.ApplyTemplate(creative, item);
					}
				}

				_creatives.AcceptChanges();
			});
		}

		/*=========================*/
		#endregion

	}
}
namespace Easynet.Edge.UI.Client.MasterCreativesLocal
{

	public class NameTemplateSelector: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			Oltp.CreativeRow creative = item as Oltp.CreativeRow;

			if (creative == null)
				return new DataTemplate();
			else if (creative.IsMonitored)
				return App.CurrentPage
					.FindResource("MonitoredNameTemplate") as DataTemplate;
			else
				return App.CurrentPage
					.FindResource("UnmonitoredNameTemplate") as DataTemplate;
		}

		public void ApplyTemplate(Oltp.CreativeRow dataItem, ListViewItem item)
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