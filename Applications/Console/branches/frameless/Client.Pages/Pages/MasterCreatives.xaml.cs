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
using System.Text.RegularExpressions;
using Easynet.Edge.BusinessObjects;

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
		ItemsControl _assoc_Campaigns;
		Oltp.SegmentDataTable _segmentTable;
		Dictionary<Oltp.SegmentRow, Oltp.SegmentValueDataTable> _segmentValueTables;
		bool _tabSegmentsInitialized = false;

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
			Oltp.AccountRow value = Window.CurrentAccount;

			//..............................
			// Segments
			_tabSegmentsInitialized = false;
			_segmentValueTables = new Dictionary<Oltp.SegmentRow, Oltp.SegmentValueDataTable>();

			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					_segmentTable = proxy.Service.Segment_Get(value.ID, false);
					foreach (Oltp.SegmentRow segment in _segmentTable.Rows)
					{
						Oltp.SegmentValueDataTable values = proxy.Service.SegmentValue_Get(value.ID, segment.SegmentID);
						Oltp.SegmentValueRow defaultRow = values.NewSegmentValueRow();
						defaultRow.AccountID = value.ID;
						defaultRow.SegmentID = segment.SegmentID;
						defaultRow.ValueID = -1;
						defaultRow.Value = "(none)";
						values.Rows.InsertAt(defaultRow, 0);
						values.AcceptChanges();
						_segmentValueTables.Add(segment, values);
					}
				}
			},
			delegate()
			{
				GetCreatives(value, null, false);
			});
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

		private void _listTable_SelectAll(object sender, RoutedEventArgs e)
		{
			if (_listTable.ListView.SelectedItems.Count == _listTable.ListView.Items.Count)
				_listTable.ListView.SelectedItems.Clear();
			else
				_listTable.ListView.SelectAll();
		}

		/*=========================*/
		#endregion

		#region Creatives
		/*=========================*/

		/// <summary>
		/// Open the dialog
		/// </summary>
		private void Creative_dialog_Open(object sender, RoutedEventArgs e)
		{
			Dialog_Open<Oltp.CreativeDataTable, Oltp.CreativeRow>
			(
				// dialog:
				Creative_dialog,

				// listTable:
				_listTable,

				// clickedItem:
				_listTable.GetParentListViewItem(e.OriginalSource as FrameworkElement) as ListViewItem,

				// allowBatch:
				true,

				// dialogTitle:
				(row, isBatch) => row.Title,

				// dialogTooltip:
				(row, isBatch) => isBatch ? null : "GK #" + row.GK.ToString(),

				// batchFlatten:
				col =>
					col.ColumnName == _creatives.GKColumn.ColumnName ? null :
					col.ColumnName == _creatives.TitleColumn.ColumnName ? "(multiple creatives)" as object :
					DBNull.Value as object
			);

			VisualTree.GetChild<TabItem>(Creative_dialog, "_tabAssociation").Visibility = Creative_dialog.IsBatch ? Visibility.Collapsed : Visibility.Visible;
			if (Creative_dialog.IsBatch)
				VisualTree.GetChild<TabItem>(Creative_dialog, "_tabSegments").Focus();
		}


		/// <summary>
		/// 
		/// </summary>
		private void Creative_dialog_ApplyingChanges(object sender, CancelRoutedEventArgs e)
		{
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
			Dialog_AppliedChanges<Oltp.CreativeRow>(Creative_dialog, "Editing Creative", _listTable.ListView, e);
		}

		/// <summary>
		/// 
		/// </summary>
		private void Creative_dialog_Closing(object sender, CancelRoutedEventArgs e)
		{
			// Cancel if user regrets
			e.Cancel = MainWindow.MessageBoxPromptForCancel(Creative_dialog);

			if (!e.Cancel)
				_creatives.RejectChanges();

			if (_assoc_Campaigns != null)
				_assoc_Campaigns.ItemsSource = null;
		}


		private void Creative_ChangeMonitoring(object sender, RoutedEventArgs e)
		{
			bool monitor = sender == _buttonMonitor;

			// First mark the correct monitoring state
			foreach (Oltp.CreativeRow creative in _listTable.ListView.SelectedItems)
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
				MainWindow.MessageBoxError("Creatives could not be updated.", ex);
				_creatives.RejectChanges();
				return false;
			},
			delegate()
			{
				Oltp.CreativeRow[] rowsToIterate = new Oltp.CreativeRow[_listTable.ListView.SelectedItems.Count];
				_listTable.ListView.SelectedItems.CopyTo(rowsToIterate, 0);

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
						ListViewItem item = _listTable.ListView.ItemContainerGenerator.ContainerFromItem(creative) as ListViewItem;

						// Apply the correcy template
						(this.Resources["NameTemplateSelector"] as MasterCreativesLocal.NameTemplateSelector)
							.ApplyTemplate(creative, item);
					}
				}

				_creatives.AcceptChanges();
			});
		}

		private void TabSegments_GotFocus(object sender, RoutedEventArgs e)
		{
			if (_tabSegmentsInitialized)
				return;

			foreach (KeyValuePair<Oltp.SegmentRow, Oltp.SegmentValueDataTable> pair in _segmentValueTables)
			{
				// Ignore segments that aren't creative related
				if ((pair.Key.AssociationFlags & SegmentAssociationFlags.AdgroupCreative) == 0)
					continue;

				StackPanel segmentPanel = VisualTree.GetChild<StackPanel>(Creative_dialog, "_segment" + pair.Key.SegmentNumber.ToString());
				if (segmentPanel == null)
					continue;

				segmentPanel.Visibility = Visibility.Visible;

				VisualTree.GetChild<Label>(segmentPanel).Content = pair.Key.Name;
				VisualTree.GetChild<ComboBox>(segmentPanel).ItemsSource = pair.Value.Rows;
			}

			_tabSegmentsInitialized = true;
		}

		/// <summary>
		/// 
		/// </summary>
		private void TabAssociations_GotFocus(object sender, RoutedEventArgs e)
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