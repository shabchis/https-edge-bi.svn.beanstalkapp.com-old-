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
	/// Interaction logic for SerpProfileSettings.xaml
	/// </summary>
	[AccountDependentPage]
	public partial class SerpSettings: PageBase
	{
		#region Fields
		/*=========================*/

		//private ObservableCollection<DataRow> _items;
		//Oltp.SettingsGAnalyticsDataTable _searchEngines;
		bool _loaded = false;
		bool _isAccountChanging = false;

		/*=========================*/
		#endregion

		#region Constructors
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		public SerpSettings()
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
			_isAccountChanging = true;
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
			ScheduleUnit schedule = account.IsSeoFrequencyNull() ?
				new ScheduleUnit(string.Empty) :
				new ScheduleUnit(account.SeoFrequency);

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

		///// <summary>
		///// 
		///// </summary>
		///// <param name="filter"></param>
		///// <param name="include"></param>
		//private void GetSearchEngines(Oltp.AccountRow account)
		//{
		//    Oltp.AccountRow currentAccount = account == null ? this.Window.CurrentAccount : account;

		//    if (currentAccount == null)
		//        return;

		//    OltpLogicClient proxy = new OltpLogicClient();
		//    using (proxy)
		//    {
		//        _searchEngines = proxy.SerpProfileSearchEngine_Get(currentAccount.ID);
		//    }

		//    // Get an empty new list
		//    if (_items == null)
		//        _items = new ObservableCollection<DataRow>();
		//    else
		//        _items.Clear();

		//    // Add all items
		//    foreach (DataRow r in _searchEngines.Rows)
		//        _items.Add(r);

		//    _listTable.ListView.ItemsSource = _items;
		//}

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

			Window.CurrentAccount.SeoFrequency = su.ScheduleUnitStringView;

		}
		/*=========================*/
		#endregion

		#region Search engines
		/*=========================*/

		///// <summary>
		///// 
		///// </summary>
		//private void SearchEngine_Delete(object sender, RoutedEventArgs e)
		//{
		//    ListViewItem item = Visual.GetVisualRoot<ListViewItem>(sender as DependencyObject);
		//    Oltp.SettingsGAnalyticsRow searchEngine = item.Content as Oltp.SettingsGAnalyticsRow;

		//    searchEngine.Delete();
		//    _items.Remove(searchEngine);
		//}

		///// <summary>
		///// 
		///// </summary>
		//private void SearchEngine_Add(object sender, RoutedEventArgs e)
		//{
		//    Oltp.SearchEngineRow searchEngineSelection = _searchEnginePicker.SelectedItem;
		//    if (searchEngineSelection == null)
		//        return;

		//    if (_searchEngines.Select(String.Format("{0} = {1}", _searchEngines.sear
		//}

		//private void SearchEngine_ItemDragged(object sender, ItemDraggedRoutedEventArgs e)
		//{
		//    Oltp.SerpProfileSearchEngine searchEngine = _items[e.SourceIndex];
		//    int indexToInsert = e.SourceIndex < e.TargetIndex ? e.TargetIndex-1 : e.TargetIndex;

		//    // Insert
		//    _items.RemoveAt(e.SourceIndex);
		//    _items.Insert(indexToInsert, searchEngine);
		//    _listTable.ListView.SelectedIndex = indexToInsert;
		//    searchEngine.DisplayPosition = indexToInsert;
		//}
		
		/*=========================*/
		#endregion

	}
}