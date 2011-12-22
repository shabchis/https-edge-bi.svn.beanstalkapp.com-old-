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
using System.Collections;
using Easynet.Edge.UI.Server;
using Easynet.Edge.Core;

namespace Easynet.Edge.UI.Client.Pages
{
	/// <summary>
	/// Interaction logic for SerpProfiles.xaml
	/// </summary>
	[AccountDependentPage]
	public partial class SegmentsPage: PageBase
	{
		#region Fields
		/*=========================*/

		ObservableCollection<DataRow> _items;
		Oltp.SegmentDataTable _segments;
		
		Oltp.SegmentValueDataTable _segmentValuesTable;
		ListTable _segmentValuesListTable;
		TextBox _segmentValueTextbox;

		/*=========================*/
		#endregion

		#region Constructors
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		public SegmentsPage()
		{
			InitializeComponent();
		}

		/*=========================*/
		#endregion

		#region General Methods
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override void OnAccountChanged()
		{
			GetSegments(Window.CurrentAccount);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="include"></param>
		private void GetSegments(Oltp.AccountRow account)
		{
			Oltp.AccountRow currentAccount = account == null ? this.Window.CurrentAccount : account;

			if (currentAccount == null)
				return;

			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					_segments = proxy.Service.Segment_Get(currentAccount.ID, true);
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
				foreach (DataRow r in _segments.Rows)
					_items.Add(r);

				_listTable.ListView.ItemsSource = _items;
			});
		}

		/*=========================*/
		#endregion

		#region Profiles
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		private void Segment_Add(object sender, RoutedEventArgs e)
		{
			// Create an editable new ro
			Oltp.SegmentRow editVersion = Dialog_MakeEditVersion<Oltp.SegmentDataTable, Oltp.SegmentRow>(null);
			editVersion.AccountID = this.Window.CurrentAccount.ID;

			// Show the dialog
			Segment_dialog.Title = "New Segment";
			Segment_dialog.BeginEdit(editVersion, _segments);
		}

		/// <summary>
		/// Open the dialog
		/// </summary>
		private void Segment_dialog_Open(object sender, RoutedEventArgs e)
		{
			// Set dataItem as current item
			ListViewItem currentItem = _listTable.GetParentListViewItem(e.OriginalSource as FrameworkElement);
			Oltp.SegmentRow row = currentItem.Content as Oltp.SegmentRow;

			// Show the dialog
			Segment_dialog.Title = row.Name;
			Segment_dialog.TitleTooltip = row.SegmentID.ToString();

			Segment_dialog.BeginEdit(
				Dialog_MakeEditVersion<Oltp.SegmentDataTable, Oltp.SegmentRow>(row),
				row
			);

			TabControl tabs = VisualTree.GetChild<TabControl>(Segment_dialog);
			TabItem tabItem = (TabItem) tabs.ItemContainerGenerator.ContainerFromIndex(tabs.SelectedIndex);
			if (tabItem != null)
				tabItem.RaiseEvent(new RoutedEventArgs(TabItem.GotFocusEvent, tabItem));
				
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
		private void Segment_dialog_ApplyingChanges(object sender, CancelRoutedEventArgs e)
		{
			Oltp.SegmentRow editVersion = Segment_dialog.Content as Oltp.SegmentRow;
			bool isNew = editVersion.RowState == DataRowState.Added;

			if (IsMissingData(isNew, _segmentValuesTable))
			{
				MessageBoxError("At least one value is required for a segment.", null);
				e.Cancel = true;
				return;
			}

			Dialog_ApplyingChanges<Oltp.SegmentDataTable, Oltp.SegmentRow>(
				_segments,
				Segment_dialog,
				typeof(IOltpLogic).GetMethod("Segment_Save"),
				e,
				null,
				true,
				delegate()
			{

				if (e.Cancel)
					return;

				if (_segmentValuesTable == null || _segmentValuesTable.GetChanges() == null)
				{
					Segment_dialog.EndApplyChanges(e);
					return;
				}

				ObservableCollection<DataRow> source =
					_segmentValuesListTable.ListView.ItemsSource as ObservableCollection<DataRow>;

				for (int i = 0; i < source.Count; i++)
				{
					if (source[i].RowState != DataRowState.Added)
						continue;

					(source[i] as Oltp.SegmentValueRow).AccountID = Window.CurrentAccount.ID;
					(source[i] as Oltp.SegmentValueRow).SegmentID = (Segment_dialog.TargetContent as Oltp.SegmentRow).SegmentID;
				}

				// Save -- async
				Window.AsyncOperation(delegate()
				{
					using (OltpProxy proxy = new OltpProxy())
					{
						Oltp.SegmentValueDataTable returnedTable = proxy.Service.SegmentValue_Save(_segmentValuesTable);
						if (returnedTable != null)
							_segmentValuesTable = returnedTable;
						else
							_segmentValuesTable.AcceptChanges();
					}
				},
				delegate(Exception ex)
				{
					MessageBoxError("Failed to save segment values.", ex);
					e.Cancel = true;
					return false;
				},
				delegate()
				{
					SetListSource<DataRow>(_segmentValuesTable, _segmentValuesListTable.ListView);

					// Complete the apply process
					Segment_dialog.EndApplyChanges(e);
				});
			});
		}

		/// <summary>
		/// 
		/// </summary>
		private void Segment_dialog_AppliedChanges(object sender, CancelRoutedEventArgs e)
		{
			Oltp.SegmentRow dataItem = Segment_dialog.TargetContent as Oltp.SegmentRow;

			// Call the univeral applied change handler
			Dialog_AppliedChanges<Oltp.SegmentRow>(Segment_dialog, dataItem.Name, _listTable._listView, e);
		}

		/// <summary>
		/// 
		/// </summary>
		private void Segment_dialog_Closing(object sender, CancelRoutedEventArgs e)
		{
			if (_segmentValuesTable != null && _segmentValuesTable.GetChanges() != null)
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
				e.Cancel = MessageBoxPromptForCancel(Segment_dialog);

			if (e.Cancel)
				return;

			_segments.RejectChanges();
			_segmentValuesTable = null;
			_segmentValuesListTable.ListView.ItemsSource = null;
		}

		/*=========================*/
		#endregion

		#region Segment Values
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		private void SegmentValue_TabGotFocus(object sender, RoutedEventArgs e)
		{
			if (_segmentValuesListTable.ListView.ItemsSource == null)
			{
				Oltp.SegmentRow segment = Segment_dialog.Content as Oltp.SegmentRow;
				Window.AsyncOperation(delegate()
				{
					if (segment.RowState != DataRowState.Added)
					{
						using (OltpProxy proxy = new OltpProxy())
						{
							_segmentValuesTable = proxy.Service.SegmentValue_Get(Window.CurrentAccount.ID, segment.SegmentID);
						}
					}
					else
					{
						_segmentValuesTable = new Oltp.SegmentValueDataTable();
					}
				},
				delegate()
				{
					SetListSource<DataRow>(_segmentValuesTable, _segmentValuesListTable.ListView);
				});
			}
		}
	
		/// <summary>
		/// 
		/// </summary>
		private void SegmentValue_Add(object sender, RoutedEventArgs e)
		{
			string valueText = _segmentValueTextbox.Text.Trim();
			if (valueText.Length < 1)
				return;

			if (_segmentValuesTable.Select(String.Format("{0} = '{1}'",
				_segmentValuesTable.ValueColumn.ColumnName,
				valueText.Replace("'", "''"))).Length > 0)
			{
				MessageBoxError("This value is already defined.", null);
				return;
			}

			ObservableCollection<DataRow> source = _segmentValuesListTable.ListView.ItemsSource as ObservableCollection<DataRow>;

			Oltp.SegmentValueRow segmentValueRow = _segmentValuesTable.NewSegmentValueRow();
			segmentValueRow.AccountID = Window.CurrentAccount.ID;
			segmentValueRow.SegmentID = (Segment_dialog.TargetContent as Oltp.SegmentRow).SegmentID;
			segmentValueRow.Value = valueText;
			_segmentValuesTable.Rows.Add(segmentValueRow);

			source.Add(segmentValueRow);
			_segmentValuesListTable.ListView.SelectedItem = segmentValueRow;
			_segmentValuesListTable.ListView.ScrollIntoView(segmentValueRow);

			// Deselect whatever
			_segmentValueTextbox.Text = String.Empty;
			_segmentValueTextbox.Focus();
		}

		private void SegmentValue_Remove(object sender, RoutedEventArgs e)
		{
			ObservableCollection<DataRow> sourceCollection = _segmentValuesListTable.ListView.ItemsSource as ObservableCollection<DataRow>;
			DataRow row = (sender as FrameworkElement).DataContext as DataRow;

			sourceCollection.Remove(row);
			row.Delete();
		}

		private void _segmentValuesListTable_Loaded(object sender, RoutedEventArgs e)
		{
			_segmentValuesListTable = sender as ListTable;
		}

		private void _segmentValueTextBox_Loaded(object sender, RoutedEventArgs e)
		{
			_segmentValueTextbox = sender as TextBox;
		}

		private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount < 2)
				return;


			TextBlock source = sender as TextBlock;
			
			// Don't allow editing 'all accounts' values
			if ((source.DataContext as Oltp.SegmentValueRow).AccountID != Window.CurrentAccount.ID)
				return;

			source.Visibility = Visibility.Collapsed;
			TextBox target = VisualTree.GetChild<TextBox>(source.Parent);
			target.Visibility = Visibility.Visible;
			target.Focus();

			e.Handled = true;
		}

		private void TextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			TextBox source = sender as TextBox;
			(source as TextBox).Visibility = Visibility.Collapsed;
			TextBlock target = VisualTree.GetChild<TextBlock>(source.Parent);
			target.Visibility = Visibility.Visible;

			(source.DataContext as IPropertyChangeNotifier).OnAllPropertiesChanged();
		}

		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape || e.Key == Key.Enter)
			{
				TextBox_LostFocus(sender, new RoutedEventArgs());
				e.Handled = true;
			}
		}

		/*=========================*/
		#endregion
	}
}

namespace Easynet.Edge.UI.Client.SegmentsLocal
{
	[ValueConversion(typeof(DataRowState), typeof(Visibility))]
	public class RowStateToVisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is DataRowState && ((DataRowState)value) == DataRowState.Added)
			{
				return Visibility.Visible;
			}
			else
				return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is Visibility && (Visibility)value == Visibility.Visible)
			{
				return DataRowState.Added;
			}
			else
				return DataRowState.Unchanged;
		}

		#endregion
	}
}
