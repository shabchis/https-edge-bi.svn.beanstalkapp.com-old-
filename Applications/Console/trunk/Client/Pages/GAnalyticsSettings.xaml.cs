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
using Easynet.Edge2.Scheduling;

namespace Easynet.Edge2.UI.Pages
{
	/// <summary>
	/// Interaction logic for GAnalyticsSettings.xaml
	/// </summary>
	[AccountDependentPage]
	public partial class GAnalyticsSettings: PageBase
	{
		#region Fields
		/*=========================*/

		private ObservableCollection<DataRow> _items;
		Oltp.SettingsGAnalyticsDataTable _sites;
		PasswordBox _accessPassword;
		ComboBox _comboResolution;
		bool _loaded = false;
		bool _isLoadingDialog = false;
		bool _isAccountChanging = false;

		/*=========================*/
		#endregion

		#region Constructors
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		public GAnalyticsSettings()
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
		public bool HasChanges
		{
			get
			{
				return
					Window.CurrentAccount != null &&
					Window.CurrentAccount.RowState == DataRowState.Modified;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		void Page_Loaded(object sender, RoutedEventArgs e)
		{
			_loaded = true;
			_comboScheduling_SelectionChanged(null, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool ChangeAccount(Oltp.AccountRow account)
		{
			if (this.HasChanges)
			{
				MessageBoxResult result = MessageBox.Show(
					"Save changes?",
					"Warning",
					MessageBoxButton.YesNoCancel,
					MessageBoxImage.Warning,
					MessageBoxResult.Cancel);

				if (result == MessageBoxResult.Cancel)
					return false;

				if (result == MessageBoxResult.Yes)
				{
					if (!SaveSettings())
						return false;
				}
				else
				{
					if (Window.CurrentAccount != null)
						Window.CurrentAccount.RejectChanges();
				}
			}


			// No account, ignore
			if (account == null)
				return true;

			// Permission check
			if (!HasPermission(account))
			{
				HidePageContents();
				return true;
			}
			else
				RestorePageContents();

			_isAccountChanging = true;

			// Get the current frequency settings
			ScheduleUnit schedule = account.IsGAnalyticsFrequencyNull() ?
				new ScheduleUnit(string.Empty) :
				new ScheduleUnit(account.GAnalyticsFrequency);

			// Set the correct schedule type
			_comboScheduling.SelectedIndex = schedule.WeekDays.Length == 7 ?
				0 :
				(schedule.WeekDays.Length > 0 ? 1 : 2);

			// Mark any selected days
			switch (_comboScheduling.SelectedIndex)
			{
				case 1:
					for (int i = 0; i < 7; i++)
						(_weekDay.Children[i] as CheckBox).IsChecked = schedule.WeekDays.Contains<int>(i+1);
					break;
				case 2:
					for (int i = 0; i < 31; i++)
						(_monthCalendar.Children[i] as ToggleButton).IsChecked = schedule.MonthDays.Contains<int>(i+1);
					break;
			}

			GetSites(account, null, false);
			_isAccountChanging = false;

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override bool Unload()
		{
			if (this.HasChanges)
			{
				MessageBoxResult result = MessageBox.Show(
					"Save changes?",
					"Warning",
					MessageBoxButton.YesNoCancel,
					MessageBoxImage.Warning,
					MessageBoxResult.Cancel);

				if (result == MessageBoxResult.Cancel)
					return false;

				if (result == MessageBoxResult.Yes)
				{
					if (!SaveSettings())
						return false;
				}
				else
				{
					if (Window.CurrentAccount != null)
						Window.CurrentAccount.RejectChanges();
				}
			}

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="include"></param>
		private void GetSites(Oltp.AccountRow account, string filter, bool include)
		{
			Oltp.AccountRow currentAccount = account == null ? this.Window.CurrentAccount : account;

			if (currentAccount == null)
				return;

			OltpLogicClient proxy = new OltpLogicClient();
			using (proxy)
			{
				_sites = proxy.SettingsGAnalytics_Get(currentAccount.ID);
			}

			// Get an empty new list
			if (_items == null)
				_items = new ObservableCollection<DataRow>();
			else
				_items.Clear();

			// Add all items
			foreach (DataRow r in _sites.Rows)
				_items.Add(r);

			_listTable.ListView.ItemsSource = _items;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private bool SaveSettings()
		{
			OltpLogicClient proxy = new OltpLogicClient();
			using (proxy)
			{
				try
				{
					if (Window.CurrentAccount.RowState != DataRowState.Unchanged)
					{
						proxy.Account_Save(Oltp.Prepare<Oltp.AccountDataTable>(Window.CurrentAccount.Table));
						Window.CurrentAccount.AcceptChanges();
					}
				}
				catch (Exception ex)
				{
					MessageBoxError("Failed to save settings.", ex);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		private void _saveButton_Click(object sender, RoutedEventArgs e)
		{
			if (Window.CurrentAccount == null)
				return;

			if (SaveSettings())
			{
				MessageBox.Show(
					"Settings saved.",
					"Information",
					MessageBoxButton.OK,
					MessageBoxImage.Information);

			}
		}

		private void _comboScheduling_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_loaded)
				return;

			_monthCalendar.Visibility = _comboScheduling.SelectedIndex != 1 ? Visibility.Visible : Visibility.Collapsed;
			_weekDay.Visibility = _comboScheduling.SelectedIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
			
			_monthCalendar.IsEnabled = _weekDay.IsEnabled = _comboScheduling.SelectedIndex > 0;

			if (_isAccountChanging || Window.CurrentAccount == null)
				return;

			ApplySchedule(null, null);
		}

		/// <summary>
		/// 
		/// </summary>
		private void ApplySchedule(object sender, RoutedEventArgs e)
		{
			if (_isAccountChanging || Window.CurrentAccount == null)
				return;

			List<int> days = new List<int>();
			ScheduleUnit su = null;

			// Set any selected days
			switch (_comboScheduling.SelectedIndex)
			{
				case 0:
					su = new ScheduleUnit("(w|1,2,3,4,5,6,7)");
					break;

				case 1:
					for (int i = 0; i < 7; i++)
						if ((_weekDay.Children[i] as CheckBox).IsChecked == true)
							days.Add(i+1);

					su = new ScheduleUnit(new int[0], new int[0], days.ToArray(), new int[0]);

					break;

				case 2:
					for (int i = 0; i < 31; i++)
						if ((_monthCalendar.Children[i] as ToggleButton).IsChecked == true)
							days.Add(i+1);

					su = new ScheduleUnit(days.ToArray(), new int[0], new int[0], new int[0]);

					break;
			}

			Window.CurrentAccount.GAnalyticsFrequency = su.ScheduleUnitStringView;

		}
		/*=========================*/
		#endregion

		#region Sites
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		private void Site_Delete(object sender, RoutedEventArgs e)
		{
			ListViewItem item = Visual.GetRoot<ListViewItem>(sender as DependencyObject);
			Oltp.SettingsGAnalyticsRow site = item.Content as Oltp.SettingsGAnalyticsRow;

			MessageBoxResult result = MessageBox.Show(String.Format("Delete the site '{0}'?", site.Title),
				"Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.OK);

			if (result != MessageBoxResult.OK)
				return;

			// Perform the delete
			site.Delete();

			using (OltpLogicClient proxy = new OltpLogicClient())
			{
				try
				{
					proxy.SettingsGAnalytics_Save(site.Table.GetChanges() as Oltp.SettingsGAnalyticsDataTable);
				}
				catch(Exception ex)
				{
					MessageBoxError("Failed to delete the site.", ex);
					site.Table.RejectChanges();
					return;
				}
			}

			site.AcceptChanges();
			_items.Remove(site);
		}

		/// <summary>
		/// 
		/// </summary>
		private void Site_Add(object sender, RoutedEventArgs e)
		{
			// Create an editable new row
			Oltp.SettingsGAnalyticsRow editVersion = Dialog_MakeEditVersion<Oltp.SettingsGAnalyticsDataTable, Oltp.SettingsGAnalyticsRow>(_sites, null);
			editVersion.AccountID = this.Window.CurrentAccount.ID;

			// Show the dialog
			Site_dialog.Title = "New Site";

			Site_dialog.BeginEdit(editVersion, _sites);
		}

		/// <summary>
		/// Open the dialog
		/// </summary>
		private void Site_dialog_Open(object sender, RoutedEventArgs e)
		{
			// Set dataItem as current item
			ListViewItem currentItem = _listTable.GetParentListViewItem(e.OriginalSource as FrameworkElement);
			Oltp.SettingsGAnalyticsRow row = currentItem.Content as Oltp.SettingsGAnalyticsRow;

			currentItem.IsSelected = true;

			// Indicate we're loading values so the event handlers don't apply these values
			_isLoadingDialog = true;

			// Set the password fields
			_accessPassword.Password = row.AccessPassword.Trim();
			
			// Set the aggregation type
			string aggrType = row.IsAggregationPendingNull() ?
				row.AggregationActive :
				row.AggregationPending;

			_comboResolution.SelectedIndex = aggrType != null && aggrType.IndexOf('M') == 0 ? 1 : 0;

			_isLoadingDialog = false;

			// Show the dialog
			Site_dialog.Title = "Editing Site";

			Site_dialog.BeginEdit(
				Dialog_MakeEditVersion<Oltp.SettingsGAnalyticsDataTable, Oltp.SettingsGAnalyticsRow>(_sites, row),
				row
			);


		}

		/// <summary>
		/// 
		/// </summary>
		private void Site_dialog_ApplyingChanges(object sender, CancelRoutedEventArgs e)
		{
			Dialog_ApplyingChanges<Oltp.SettingsGAnalyticsDataTable, Oltp.SettingsGAnalyticsRow>(
				_sites,
				Site_dialog,
				typeof(OltpLogic.OltpLogicClient).GetMethod("SettingsGAnalytics_Save"),
				e);
		}

		/// <summary>
		/// 
		/// </summary>
		private void Site_dialog_AppliedChanges(object sender, CancelRoutedEventArgs e)
		{
			// Update fields without an automatic binding
			(Site_dialog.TargetContent as Oltp.SettingsGAnalyticsRow).AggregationPending = 
				(Site_dialog.Content as Oltp.SettingsGAnalyticsRow).AggregationPending;
			(Site_dialog.TargetContent as Oltp.SettingsGAnalyticsRow).AccessPassword = 
				(Site_dialog.Content as Oltp.SettingsGAnalyticsRow).AccessPassword;

			// Call the univeral applied change handler
			Dialog_AppliedChanges<Oltp.SettingsGAnalyticsRow>(Site_dialog, "Editing Site", _listTable._listView, e);
		}

		/// <summary>
		/// 
		/// </summary>
		private void Site_dialog_Closing(object sender, CancelRoutedEventArgs e)
		{
			// Cancel if user regrets
			e.Cancel = MessageBoxPromptForCancel(Site_dialog);

			if (!e.Cancel)
				_sites.RejectChanges();
		}

		private void _accessPassword_Loaded(object sender, RoutedEventArgs e)
		{
			_accessPassword = sender as PasswordBox;
		}

		/// <summary>
		/// 
		/// </summary>
		private void _accessPassword_PasswordChanged(object sender, RoutedEventArgs e)
		{
			if (_isLoadingDialog || Site_dialog.Content == null)
				return;

			(Site_dialog.Content as Oltp.SettingsGAnalyticsRow).AccessPassword = _accessPassword.Password;
		}

		/// <summary>
		/// 
		/// <param name="e"></param>
		private void _comboResolution_Loaded(object sender, RoutedEventArgs e)
		{
			_comboResolution = sender as ComboBox;
		}

		/// <summary>
		/// 
		/// </summary>
		private void _comboResolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_isLoadingDialog || Site_dialog.Content == null)
				return;

			// Set the pending aggregation type
			(Site_dialog.Content as Oltp.SettingsGAnalyticsRow).AggregationPending = 
				_comboResolution.SelectedIndex == 0 ? "Daily" : "Monthly";
		}


		/*=========================*/
		#endregion

	}
}