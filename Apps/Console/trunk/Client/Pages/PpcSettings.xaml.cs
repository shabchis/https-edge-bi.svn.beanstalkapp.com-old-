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
using Easynet.Edge2.Scheduling;
using System.Windows.Controls.Primitives;
using System.Data;
using System.Threading;

namespace Easynet.Edge2.UI.Pages
{
	/// <summary>
	/// Interaction logic for PpcSettings.xaml
	/// </summary>
	[AccountDependentPage]
	public partial class PpcSettings: PageBase
	{
		bool _loaded = false;
		bool _isAccountChanging = false;
		Oltp.SettingsAdwordsDataTable _settingsTable = null;
		Oltp.SettingsAdwordsRow _settingsRow = null;

		public PpcSettings()
		{
			InitializeComponent();
			this.Loaded += new RoutedEventHandler(PpcSettings_Loaded);
		}

		void PpcSettings_Loaded(object sender, RoutedEventArgs e)
		{
			_loaded = true;

			_isAccountChanging = true;
			_comboResolution_SelectionChanged(null, null);
			_comboScheduling_SelectionChanged(null, null);
			_isAccountChanging = false;
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

			_isAccountChanging = true;

			_accessPassword.Password = null;
			

			// No account, ignore
			if (account == null)
			{
				_isAccountChanging = false;
				return true;
			}

			// Permission check
			if (!HasPermission(account))
			{
				HidePageContents();
				return true;
			}
			else
				RestorePageContents();

			// Get the general settings
			using (OltpLogicClient proxy = new OltpLogicClient())
			{
				_settingsTable = proxy.SettingsAdwords_Get(account.ID);
			}

			// Create a new settings row if it doesn't exist
			if (_settingsTable.Rows.Count < 1)
			{
				_settingsTable.Rows.Add(_settingsTable.NewRow());
			}
			
			// Get the first row, this is the special settings rows\
			_settingsRow = _settingsTable.Rows[0] as Oltp.SettingsAdwordsRow;
			if (_settingsRow.RowState == DataRowState.Added)
			{
				_settingsRow.AccountID = account.ID;
				_settingsRow.AggregationPending = "Monthly";
			}

			// Set the data context
			this.DataContext = account;
			_accessDetails.DataContext = _settingsRow;

			// Set the password field
			if (!_settingsRow.IsAccessPasswordNull())
				_accessPassword.Password = _settingsRow.AccessPassword.Trim();

			// Set the aggregation type
			string aggrType = _settingsRow.IsAggregationPendingNull() ?
				_settingsRow.AggregationActive :
				_settingsRow.AggregationPending;

			_comboResolution.SelectedIndex = aggrType != null && aggrType.IndexOf('M') == 0 ? 1 : 0;

			// Get the current frequency settings
			ScheduleUnit schedule = account.IsAdwordsFrequencyNull() ?
				new ScheduleUnit(string.Empty) :
				new ScheduleUnit(account.AdwordsFrequency);

			// Set the correct schedule type
			_comboScheduling.SelectedIndex = schedule.WeekDays.Length == 7 ?
				0 :
				(schedule.WeekDays.Length > 0 ? 1 : 2);

			// Mark any selected days
			switch (_comboScheduling.SelectedIndex)
			{
				case 1:
					for(int i = 0; i < 7; i++)
						(_weekDay.Children[i] as CheckBox).IsChecked = schedule.WeekDays.Contains<int>(i+1);
					break;
				case 2:
					for (int i = 0; i < 31; i++)
						(_monthCalendar.Children[i] as ToggleButton).IsChecked = schedule.MonthDays.Contains<int>(i+1);
					break;
			}

			if (_gatewayScopeAccounts.ItemsSource  == null)
			{
				_gatewayScopeAccounts.ItemsSource = Window.OtherAccounts;
				_gatewayScopeAccounts.UpdateLayout();
			}

			LoadScopeAccounts(account);

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
		/// <param name="row"></param>
		/// <returns></returns>
		private bool IsEmptyRow(Oltp.SettingsAdwordsRow row)
		{
			foreach (DataColumn col in row.Table.Columns)
			{
				if (!row[col].Equals(col.DefaultValue))
					return false;
			}

			return true;
		}

		private void _comboResolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_loaded)
				return;

			_scheduleType.Visibility = _scheduleDates.Visibility =
				_comboResolution.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;

			if (_isAccountChanging || Window.CurrentAccount == null)
				return;

			// Set the pending aggregation type
			_settingsRow.AggregationPending = _comboResolution.SelectedIndex == 0 ?
				"Daily" : "Monthly";
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
		private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
		{
			if (_isAccountChanging)
				return;

			_settingsRow.AccessPassword = _accessPassword.Password;
		}

		public bool HasChanges
		{
			get
			{
				return
					Window.CurrentAccount != null &&
					_settingsRow != null &&
					(
						Window.CurrentAccount.RowState == DataRowState.Modified ||
						_settingsRow.RowState == DataRowState.Modified ||
						(
							_settingsRow.RowState == DataRowState.Added &&
							!IsEmptyRow(_settingsRow)
						)
					);
			}
		}

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

					if (_settingsRow.RowState != DataRowState.Unchanged)
					{
						Oltp.SettingsAdwordsDataTable savedVersion = proxy.SettingsAdwords_Save(_settingsTable);

						if (savedVersion != null)
						{
							// Use the returned table
							_settingsTable = savedVersion;
							_settingsRow = savedVersion.Rows[0] as Oltp.SettingsAdwordsRow;

							// Bind to the new context
							_accessDetails.DataContext = _settingsRow;
						}

						_settingsTable.AcceptChanges();
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

		private void _saveButton_Click(object sender, RoutedEventArgs e)
		{
			if (Window.CurrentAccount == null)
				return;

			Window.CurrentAccount.GatewayAccountScope = GetGatewayAccountScope();

			if (SaveSettings())
			{
				MessageBox.Show(
					"Settings saved.",
					"Information",
					MessageBoxButton.OK,
					MessageBoxImage.Information);

			}
		}

		private string GetGatewayAccountScope()
		{
			List<string> selectedAccounts = new List<string>();
			foreach (Oltp.AccountRow otherAccount in Window.OtherAccounts)
			{
				if (Visual.GetDescendant<CheckBox>(_gatewayScopeAccounts.ItemContainerGenerator.ContainerFromItem(otherAccount)).IsChecked == true)
					selectedAccounts.Add(otherAccount.ID.ToString());
			}
			string joined = string.Empty;
			string[] ids = selectedAccounts.ToArray();
			for (int i = 0; i < ids.Length; i++)
				joined += ids[i] + (i < ids.Length-1 ? "," : "");

			return joined;
		}

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

			Window.CurrentAccount.AdwordsFrequency = su.ScheduleUnitStringView;

		}

		/// <summary>
		/// Loads related accounts to the listbox.
		/// </summary>
		/// <param name="accountID"></param>
		private void LoadScopeAccounts(Oltp.AccountRow account)
		{
			string[] accounts = account.GatewayAccountScope != null ? account.GatewayAccountScope.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries) : null;
			if (accounts == null)
				return;

			foreach (Oltp.AccountRow otherAccount in Window.OtherAccounts)
			{
				Visual.GetDescendant<CheckBox>(_gatewayScopeAccounts.ItemContainerGenerator.ContainerFromItem(otherAccount)).IsChecked = 
					accounts.Contains(otherAccount.ID.ToString());
			}
		}
	}
}