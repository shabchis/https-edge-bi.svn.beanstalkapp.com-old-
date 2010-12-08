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
	/// Interaction logic for MasterPages.xaml
	/// </summary>
	[AccountDependentPage]
	public partial class MasterPages: PageBase
	{
		#region Fields
		/*=========================*/

		private ObservableCollection<DataRow> _items;
		Oltp.PageDataTable _pages;
		TextBox _urlField = null;
		ListTable _reservationListView;
		ObservableCollection<DataRow> _reservationItems;
		Oltp.GatewayReservationDataTable _reservationTable;
		TextBox _reservationFrom;
		TextBox _reservationTo;

		/*=========================*/
		#endregion

		#region Constructors
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		public MasterPages()
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
			GetPages(Window.CurrentAccount, null, true);
		}

		/// <summary>
		/// 
		/// </summary>
		private void _filterButton_Click(object sender, RoutedEventArgs e)
		{
			//if (String.IsNullOrEmpty(_filterText.Text.Trim()) && _filterCheckbox.IsChecked != false)
			//{
			//    MessageBox.Show("Filter cannot be empty when displaying unmonitored pages.",
			//        "Not allowed", MessageBoxButton.OK, MessageBoxImage.Exclamation
			//        );
			//    return;
			//}

			string searchString = _filterText.Text.IndexOf('*') > -1 ?
				_filterText.Text.Replace('*', '%') :
				(_filterText.Text.Length < 1 ? null : _filterText.Text);

			GetPages(null, searchString, true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="include"></param>
		private void GetPages(Oltp.AccountRow account, string filter, bool include)
		{
			Oltp.AccountRow currentAccount = account == null ? this.Window.CurrentAccount : account;

			if (currentAccount == null)
				return;

			OltpLogicClient proxy = new OltpLogicClient();
			using (proxy)
			{
				_pages = proxy.Service.Page_Get(currentAccount.ID, filter, include, -1);
			}

			// Get an empty new list
			if (_items == null)
				_items = new ObservableCollection<DataRow>();
			else
				_items.Clear();

			// Add all items
			foreach (DataRow r in _pages.Rows)
				_items.Add(r);

			_listTable.ListView.ItemsSource = _items;
		}

		/*=========================*/
		#endregion

		#region Keywords
		/*=========================*/

		/// <summary>
		/// Add a gateway to an dataItem
		/// </summary>
		private void Page_dialog_New(object sender, RoutedEventArgs e)
		{
			// Create an editable new row
			Oltp.PageRow editVersion = Dialog_MakeEditVersion<Oltp.PageDataTable, Oltp.PageRow>(_pages, null);
			editVersion.AccountID = this.Window.CurrentAccount.ID;
			editVersion.IsMonitored = true;

			// Show the dialog
			Page_dialog.Title = "New Page";

			_reservationTable = new Oltp.GatewayReservationDataTable();
			_reservationItems = new ObservableCollection<DataRow>();
			_reservationListView.ListView.ItemsSource = _reservationItems;

			Page_dialog.BeginEdit(editVersion, _pages);
		}

		/// <summary>
		/// Open the dialog
		/// </summary>
		private void Page_dialog_Open(object sender, RoutedEventArgs e)
		{
			// Set dataItem as current item
			ListViewItem currentItem = _listTable.GetParentListViewItem(e.OriginalSource as FrameworkElement);
			Oltp.PageRow row = currentItem.Content as Oltp.PageRow;

			// Show the dialog
			Page_dialog.Title = "Editing Page";

			// Disable editing page value
			//_urlField.IsReadOnly = true;

			Page_dialog.BeginEdit(
				Dialog_MakeEditVersion<Oltp.PageDataTable, Oltp.PageRow>(_pages, row),
				row
			);

			using (OltpLogicClient proxy = new OltpLogicClient())
			{
				_reservationTable = proxy.Service.GatewayReservation_GetByPage(Window.CurrentAccount.ID, row.GK);
			}

			_reservationItems = new ObservableCollection<DataRow>();
			foreach(DataRow r in _reservationTable.Rows)
				_reservationItems.Add(r);

			_reservationListView.ListView.ItemsSource = _reservationItems;
				
			// When opening, select it only if no more than one is already selected
			if (_listTable._listView.SelectedItems.Count < 2)
			{
				_listTable._listView.SelectedItems.Clear();
				currentItem.IsSelected = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void Page_dialog_ApplyingChanges(object sender, CancelRoutedEventArgs e)
		{
			bool isNew = (Page_dialog.Content as Oltp.PageRow).RowState == DataRowState.Added;

			// When adding a new page, make sure it is unique
			if (isNew)
			{
				Oltp.PageDataTable tbl;
				using (OltpLogicClient proxy = new OltpLogicClient())
				{
					tbl = proxy.Service.Page_Get(this.Window.CurrentAccount.ID, _urlField.Text, true, -1);
				}

				if (tbl.Rows.Count > 0)
				{
					MessageBoxError("Page already exists.", null);
					e.Cancel = true;
					return;
				}
			}

			Dialog_ApplyingChanges<Oltp.PageDataTable, Oltp.PageRow>(
				_pages,
				Page_dialog,
				typeof(IOltpLogic).GetMethod("Page_Save"),
				e);

			if (e.Cancel)
				return;

			// Update with new page GK
			if (isNew && _reservationTable != null)
			{
				foreach (Oltp.GatewayReservationRow row in _reservationTable.Rows)
				{
					row.PageGK = (Page_dialog.Content as Oltp.PageRow).GK;
				}
			}

			Oltp.GatewayReservationDataTable changes = (Oltp.GatewayReservationDataTable) _reservationTable.GetChanges();
			if (changes != null)
			{
				try
				{
					using (OltpLogicClient proxy = new OltpLogicClient())
					{
						proxy.Service.GatewayReservation_Save(changes);
						_reservationTable.AcceptChanges();
					}
				}
				catch(Exception ex)
				{
					MessageBoxError("Page was saved but gateway reservations were not.", ex);
					e.Cancel = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void Page_dialog_AppliedChanges(object sender, CancelRoutedEventArgs e)
		{
			// Call the univeral applied change handler
			Dialog_AppliedChanges<Oltp.PageRow>(Page_dialog, "Editing Page", _listTable._listView, e);

			// Disable editing page value
			//_urlField.IsReadOnly = true;

			// Set correct data template
			Oltp.PageRow dataItem = Page_dialog.TargetContent as Oltp.PageRow;
			ListViewItem item = _listTable._listView.ItemContainerGenerator.ContainerFromItem(dataItem) as ListViewItem;
			(this.Resources["NameTemplateSelector"] as MasterPagesLocal.NameTemplateSelector)
				.ApplyTemplate(dataItem, item);
		}

		/// <summary>
		/// 
		/// </summary>
		private void Page_dialog_Closing(object sender, CancelRoutedEventArgs e)
		{
			if (_reservationTable.GetChanges() != null)
			{
				MessageBoxResult result = MessageBox.Show(
					"Discard changes?",
					"Confirm",
					MessageBoxButton.OKCancel,
					MessageBoxImage.Warning,
					MessageBoxResult.Cancel);

				if (result == MessageBoxResult.Cancel)
					e.Cancel = true;
			}

			// Cancel if user regrets
			if (!e.Cancel)
				e.Cancel = MessageBoxPromptForCancel(Page_dialog);

			if (!e.Cancel)
			{
				_pages.RejectChanges();
				_reservationListView.ListView.ItemsSource = null;
				_reservationTable.RejectChanges();
				_reservationTable = null;
				_reservationItems = null;
				_reservationFrom.Text = String.Empty;
				_reservationTo.Text = String.Empty;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void Page_dialog_Loaded(object sender, RoutedEventArgs e)
		{
			_urlField = Visual.GetDescendant<TextBox>(Page_dialog, "_urlField");
		}

		#region Obsolete
		/*
		/// <summary>
		/// 
		/// </summary>
		private void Page_ChangeMonitoring(object sender, RoutedEventArgs e)
		{
			bool monitor = sender == _buttonMonitor;

			// First mark the correct monitoring state
			foreach (Oltp.PageRow dataItem in _listTable._listView.SelectedItems)
				dataItem.IsMonitored = monitor;
			

			try
			{
				using (OltpLogicClient proxy = new OltpLogicClient())
				{
					proxy.Service.Page_Save( Oltp.Prepare<Oltp.PageDataTable>(_pages) );
				}
			}
			catch (Exception ex)
			{
				// Failed, so cancel and display a message
				MessageBoxError("Pages could not be updated.", ex);
				_pages.RejectChanges();
				return;
			}

			Oltp.PageRow[] rowsToIterate = new Oltp.PageRow[_listTable._listView.SelectedItems.Count];
			_listTable._listView.SelectedItems.CopyTo(rowsToIterate, 0);

			foreach (Oltp.PageRow dataItem in rowsToIterate)
			{
				if (_filterCheckbox.IsChecked == false && !monitor)
				{
					// Remove pages that have been unmonitored
					dataItem.Delete();
					_items.Remove(dataItem);
				}
				else
				{
					ListViewItem item = _listTable._listView.ItemContainerGenerator.ContainerFromItem(dataItem) as ListViewItem;
					
					// Apply the correcy template
					(this.Resources["NameTemplateSelector"] as MasterPagesLocal.NameTemplateSelector)
						.ApplyTemplate(dataItem, item);
				}
			}

			_pages.AcceptChanges();
		}
		*/
		#endregion

		private void _reservationListView_Loaded(object sender, RoutedEventArgs e)
		{
			_reservationListView = sender as ListTable;
		}

		private void GatewayReservation_Delete(object sender, RoutedEventArgs e)
		{
			Oltp.GatewayReservationRow reservation = (Oltp.GatewayReservationRow)
				_reservationListView.ListView.ItemContainerGenerator.ItemFromContainer(Visual.GetRoot<ListViewItem>(sender as Button));

			//MessageBoxResult result = MessageBox.Show("Are you sure you want to cancel the reservation for {0} to {1}?",
			//    "Confirm",
			//    MessageBoxButton.OKCancel,
			//    MessageBoxImage.Warning,
			//    MessageBoxResult.OK);

			//if (result != MessageBoxResult.OK)
			//    return;

			_reservationItems.Remove(reservation);
			reservation.Delete();
		}

		private void GatewayReservation_Add(object sender, RoutedEventArgs e)
		{
			Oltp.PageRow currentPage = Page_dialog.Content as Oltp.PageRow;
			long fromID = 0;
			long toID = 0;

			if (!Int64.TryParse(_reservationFrom.Text, out fromID) || !Int64.TryParse(_reservationTo.Text, out toID))
			{
				// Validation
				MessageBoxError("Please enter 2 valid numbers.", null);
				_reservationFrom.SelectAll();
				_reservationFrom.Focus();
				return;
			}

			long fromIDTemp = fromID;
			fromID = fromID <= toID ? fromID : toID;
			toID = toID >= fromIDTemp ? toID: fromIDTemp;

			// Get other accounts to check against
			string[] otherAccountStrings = Window.CurrentAccount.GatewayAccountScope != null ? Window.CurrentAccount.GatewayAccountScope.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries) : new string[0];
			int[] otherAccounts = Array.ConvertAll<string, int>(otherAccountStrings, new Converter<string,int>(delegate(string input)
				{
					int val;
					if (!Int32.TryParse(input, out val))
						val = -1;
					return val;
				}));


			// Check if any gateways are already in use or reserved
			int nonReservableCount = 0;
			using (OltpLogicClient proxy = new OltpLogicClient())
			{
				Oltp.GatewayReservationDataTable existingReservations =
					proxy.Service.GatewayReservation_GetByOverlap(Window.CurrentAccount.ID, fromID, toID, otherAccounts);

				// Get local additions as well
				DataRow[] localReservations =
					_reservationTable.Select(String.Format(
					"({0} >= {2} AND {0} <= {3}) OR ({1} >= {2} AND {1} <= {3})",
					_reservationTable.FromGatewayColumn.ColumnName,
					_reservationTable.ToGatewayColumn.ColumnName,
					fromID,
					toID
					));

				if (existingReservations.Rows.Count > 0 || localReservations.Length > 0)
				{
					Oltp.GatewayReservationRow existingReservation =
						(existingReservations.Rows.Count > 0 ?  existingReservations.Rows[0] : localReservations[0]) as Oltp.GatewayReservationRow;

					MessageBoxError(String.Format(
						"An overlapping range ({0} to {1}) has already been reserved for {2} page.",
						existingReservation.FromGateway, existingReservation.ToGateway, existingReservation.PageGK == currentPage.GK ? "this" : "another"
						), null);
					_reservationFrom.SelectAll();
					_reservationFrom.Focus();
					return;
				}

				nonReservableCount = proxy.Service.Gateway_CountByRange(Window.CurrentAccount.ID, fromID, toID, otherAccounts);
			}

			if (nonReservableCount > 0)
			{
				MessageBoxResult result = MessageBox.Show(
					String.Format("{0} gateway{1} already in use within the specified range. Only available numbers within the range will be reserved.\n\nProceed?",
						nonReservableCount > toID - fromID ? "All" : nonReservableCount.ToString(),
						nonReservableCount > 1 ? "s are" : " is"
						),
					"Confirmation",
					MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK
					);

				if (result == MessageBoxResult.Cancel)
				{
					_reservationFrom.SelectAll();
					_reservationFrom.Focus();
					return;
				}
			}

			Oltp.GatewayReservationRow reservation = _reservationTable.NewGatewayReservationRow();
			reservation.AccountID = Window.CurrentAccount.ID;
			reservation.PageGK = currentPage.GK;
			reservation.FromGateway = fromID;
			reservation.ToGateway = toID;
			reservation.ReservedByUserID = Window.CurrentUser.ID;
			reservation.ReservedByUserName = Window.CurrentUser.Name;
			_reservationTable.AddGatewayReservationRow(reservation);
			_reservationItems.Add(reservation);
		}

		private void _reservationFrom_Loaded(object sender, RoutedEventArgs e)
		{
			_reservationFrom = sender as TextBox;
		}

		private void _reservationTo_Loaded(object sender, RoutedEventArgs e)
		{
			_reservationTo = sender as TextBox;
		}

		/*=========================*/
		#endregion

	}
}

namespace Easynet.Edge.UI.Client.MasterPagesLocal
{

	public class NameTemplateSelector: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			Oltp.PageRow dataItem = item as Oltp.PageRow;

			if (dataItem == null)
				return new DataTemplate();

			if (dataItem.Title != null && dataItem.Title.Trim().Length > 0)
			{
				return App.CurrentPage
					.FindResource("MonitoredNameTemplate") as DataTemplate;
					//.FindResource(dataItem.IsMonitored == true ? "MonitoredNameTemplate" : "UnmonitoredNameTemplate") as DataTemplate;
			}
			else
			{
				return App.CurrentPage
					.FindResource("MonitoredUrlTemplate") as DataTemplate;
					//.FindResource(dataItem.IsMonitored == true ? "MonitoredUrlTemplate" : "UnmonitoredUrlTemplate") as DataTemplate;
			}
		}

		public void ApplyTemplate(Oltp.PageRow dataItem, ListViewItem item)
		{
			if (item == null)
				return;

			Button nameButton = Visual.GetDescendant<Button>(item, "_itemName");
			if (nameButton == null)
				return;

			ContentPresenter cp = VisualTreeHelper.GetParent(nameButton) as ContentPresenter;
			cp.ContentTemplate = SelectTemplate(dataItem, item);
		}
	}

}