using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;

using Easynet.Edge.UI.Data;
using System.Text.RegularExpressions;
using Easynet.Edge.BusinessObjects;
using Easynet.Edge.UI.Server;

namespace Easynet.Edge.UI.Client.Pages
{
	/// <summary>
	/// Interaction logic for PpcAdUnits.xaml
	/// </summary>
	[AccountDependentPage]
	public partial class PpcGateways: PageBase
	{
		#region Fields
		/*=========================*/

		private ObservableCollection<DataRow> _items;
		long? _gatewayIdentifier = null;
		public Oltp.ChannelDataTable _channelTable;
		bool _changingChannel = false;
		TextBox _input_originalID;
		AutocompleteCombo _input_gatewayPage;
		StackPanel _gatewayAppliedTo;
		Oltp.PageDataTable _dropDownPages;
		ComboBox _gatewayChannelPicker;
		Oltp.SegmentDataTable _segmentTable;
		Dictionary<Oltp.SegmentRow, Oltp.SegmentValueDataTable> _segmentValueTables;
		bool _tabSegmentsInitialized = false;
		
		Oltp.GatewayDataTable _gatewayTable;
		/*=========================*/
		#endregion

		#region Constructors
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		public PpcGateways()
		{
			this.Loaded += new RoutedEventHandler(Page_Loaded);

			Window.AsyncStart("PpcGateways.ctor");
			Window.AsyncOperation(delegate()
			{
				// Get the channel list
				using (OltpProxy proxy = new OltpProxy())
				{
					_channelTable = proxy.Service.Channel_Get();
				}
			},
			delegate()
			{
				// Add the table as a resource
				Resources.Add("ChannelTableRows", _channelTable.Rows);
				InitializeComponent();

				Window.AsyncEnd("PpcGateways.ctor");

				ObservableCollection<object> channelPickerSource = new ObservableCollection<object>();
				channelPickerSource.Add("All channels");
				foreach (Oltp.ChannelRow ch in _channelTable.Rows)
					channelPickerSource.Add(ch);
				_channelPicker.ItemsSource = channelPickerSource;
				_channelPicker.SelectedIndex = 0;

				// Batch channel picker
				Oltp.ChannelDataTable batchChannels = (Oltp.ChannelDataTable)_channelTable.Copy();
				Oltp.ChannelRow noChannelChangeRow = batchChannels.NewChannelRow();
				noChannelChangeRow.ID = -99;
				noChannelChangeRow.Name = "(no change)";
				noChannelChangeRow.DisplayName = "(no change)";
				noChannelChangeRow.HasPpcApi = false;
				batchChannels.Rows.InsertAt(noChannelChangeRow, 0);
				batchChannels.AcceptChanges();
				_batchChannelPicker.ItemsSource = batchChannels;
				_batchChannelPicker.UpdateLayout();
				_batchChannelPicker.SelectedValue = 0;
			});
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

		void _channelPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_changingChannel)
				return;

			_changingChannel = true;
			//GetGateway(Window.CurrentAccount, null);
			_changingChannel = false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override void OnAccountChanged()
		{

			Oltp.AccountRow account = Window.CurrentAccount;

			Window.AsyncOperation(delegate()
			{
				Window.AsyncWait("PpcGateways.ctor");
			},
			delegate()
			{
				PagePicker_ItemsSourceRequired(_gatewayReserve_Page, EventArgs.Empty);

				// Clear items
				if (_items != null)
					_items.Clear();

				// Mark as segments uninitialized so the dialog refreshes the values
				_tabSegmentsInitialized = false;

				// Segments setup
				//=============

				// Reset segments
				StackPanel[] batchSegments = new StackPanel[]
				{
					_batchSegment1, _batchSegment2, _batchSegment3, _batchSegment4, _batchSegment5
				};
				StackPanel[] filterSegments = new StackPanel[]
				{
					_segment1Filter, _segment2Filter, _segment3Filter, _segment4Filter, _segment5Filter
				};
				foreach (StackPanel segment in batchSegments)
				{
					VisualTree.GetChild<ComboBox>(segment).ItemsSource = null;
					VisualTree.GetChild<Label>(segment).Content = null;
					segment.Visibility = Visibility.Collapsed;
				}
				foreach (StackPanel segment in filterSegments)
				{
					VisualTree.GetChild<ComboBox>(segment).ItemsSource = null;
					VisualTree.GetChild<Label>(segment).Content = null;
					segment.Visibility = Visibility.Collapsed;
				}

				_segmentValueTables = new Dictionary<Oltp.SegmentRow, Oltp.SegmentValueDataTable>();

				Window.AsyncOperation(delegate()
				{
					using (OltpProxy proxy = new OltpProxy())
					{
						_segmentTable = proxy.Service.Segment_Get(account.ID, false);
						foreach (Oltp.SegmentRow segment in _segmentTable.Rows)
						{
							if ((segment.AssociationFlags & SegmentAssociationFlags.Gateyway) == 0)
								continue;

							// Gateway dialog
							Oltp.SegmentValueDataTable values = proxy.Service.SegmentValue_Get(account.ID, segment.SegmentID);
							Oltp.SegmentValueRow defaultRow = values.NewSegmentValueRow();
							defaultRow.AccountID = account.ID;
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
					foreach (Oltp.SegmentRow segment in _segmentTable.Rows)
					{
						if ((segment.AssociationFlags & SegmentAssociationFlags.Gateyway) == 0)
							continue;

						Oltp.SegmentValueDataTable values = _segmentValueTables[segment];

						// Filter values
						Oltp.SegmentValueDataTable filterSegmentValues = (Oltp.SegmentValueDataTable)values.Copy();
						Oltp.SegmentValueRow anyValueRow = filterSegmentValues.NewSegmentValueRow();
						anyValueRow.AccountID = account.ID;
						anyValueRow.SegmentID = segment.SegmentID;
						anyValueRow.ValueID = -99;
						anyValueRow.Value = "(any)";
						filterSegmentValues.Rows.InsertAt(anyValueRow, 0);
						filterSegmentValues.AcceptChanges();

						StackPanel filter = VisualTree.GetChild<StackPanel>(_pageControls, String.Format("_segment{0}Filter", segment.SegmentNumber));
						filter.Visibility = Visibility.Visible;
						VisualTree.GetChild<Label>(filter).Content = segment.Name;
						ComboBox picker = VisualTree.GetChild<ComboBox>(filter);
						picker.ItemsSource = filterSegmentValues;
						picker.UpdateLayout();
						picker.SelectedIndex = 0;

						// Batch dialog
						Oltp.SegmentValueDataTable batchSegmentValues = (Oltp.SegmentValueDataTable)values.Copy();
						Oltp.SegmentValueRow noChangeValueRow = batchSegmentValues.NewSegmentValueRow();
						noChangeValueRow.AccountID = account.ID;
						noChangeValueRow.SegmentID = segment.SegmentID;
						noChangeValueRow.ValueID = -99;
						noChangeValueRow.Value = "(no change)";
						batchSegmentValues.Rows.InsertAt(noChangeValueRow, 0);
						batchSegmentValues.AcceptChanges();

						StackPanel batchSegment = batchSegments[segment.SegmentNumber - 1];
						batchSegment.Visibility = Visibility.Visible;
						VisualTree.GetChild<Label>(batchSegment).Content = segment.Name;
						ComboBox batchPicker = VisualTree.GetChild<ComboBox>(batchSegment);
						batchPicker.ItemsSource = batchSegmentValues;
						batchPicker.UpdateLayout();
						batchPicker.SelectedIndex = 0;
					}

				});
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="account"></param>
		/// <param name="gatewayGK"></param>
		private void GetGateways(Oltp.AccountRow account, int? channelID, int? identifier, int?[] segments)
		{
			ObservableCollection<DataRow> foundItems = new ObservableCollection<DataRow>();

			if (identifier != null)
			{
				// SPECIFIC GATEWAY LOOKUP
				Oltp.GatewayRow filterGateway = null;
				long gatewayIdentifier = identifier.Value;
				Oltp.GatewayDataTable filterGatewayTable = null;
				Oltp.GatewayReservationDataTable reservations = null;

				Window.AsyncOperation(delegate()
				{
					using (OltpProxy proxy = new OltpProxy())
					{
						// Check to see if requested gateway exists
						filterGatewayTable = proxy.Service.Gateway_GetByIdentifier(account.ID, gatewayIdentifier);
						if (filterGatewayTable.Rows.Count < 1)
						{
							reservations = proxy.Service.GatewayReservation_GetByIdentifier(account.ID, gatewayIdentifier);
						}
					}
				},
				delegate()
				{
					if (filterGatewayTable.Rows.Count < 1)
					{
						if (reservations.Rows.Count > 0)
						{
							MessageBox.Show(
								String.Format("Tracker {0} has been reserved by {1}.", gatewayIdentifier, (reservations.Rows[0] as Oltp.GatewayReservationRow).ReservedByUserName),
								"Information",
								MessageBoxButton.OK, MessageBoxImage.Warning);
						}
						else
						{
							MessageBox.Show(
								String.Format("Tracker {0} is not in use.", gatewayIdentifier),
								"Information",
								MessageBoxButton.OK, MessageBoxImage.Information);

						}

						// Make re-entering a number easier
						_filterText.SelectAll();
						_filterText.Focus();
					}
					else
					{
						filterGateway = filterGatewayTable.Rows[0] as Oltp.GatewayRow;
					}

					// Not found anything
					if (filterGateway == null)
						return;

					foundItems.Add(filterGateway);
					
					// Get an empty new list
					_items = foundItems;
					_listTable.ListView.ItemsSource = _items;
				});

			}
			else
			{
				// GENERAL GATEWAY SEARCH
				Window.AsyncOperation(delegate()
				{
					using (OltpProxy proxy = new OltpProxy())
					{
						_gatewayTable = proxy.Service.Gateway_GetGateways(
							account.ID,
							channelID,
							segments);
					}
				},
				delegate()
				{
					foreach (Oltp.GatewayRow gw in _gatewayTable)
						foundItems.Add(gw);

					// Get an empty new list
					_items = foundItems;
					_listTable.ListView.ItemsSource = _items;
				});
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void _filterButton_Click(object sender, RoutedEventArgs e)
		{
			int gatewayIdentifier = -1;
			if (!String.IsNullOrEmpty(_filterText.Text) && !Int32.TryParse(_filterText.Text, out gatewayIdentifier))
			{
			    // Ignore
			    MainWindow.MessageBoxError("Please enter a valid identifier (number bigger than 0)", null);

			    // Make re-entering a number easier
			    _filterText.SelectAll();
			    _filterText.Focus();

			    return;
			}


			GetGateways
			(
				Window.CurrentAccount,
				_channelPicker.SelectedIndex > 0 ? new Nullable<int>(((Oltp.ChannelRow)_channelPicker.SelectedValue).ID) : null,
				gatewayIdentifier < 0 ? null : new Nullable<int>(gatewayIdentifier),
				new int?[]
				{
					GetSegmentValue(_segment1Filter, _segment1Picker),
					GetSegmentValue(_segment2Filter, _segment2Picker),
					GetSegmentValue(_segment3Filter, _segment3Picker),
					GetSegmentValue(_segment4Filter, _segment4Picker),
					GetSegmentValue(_segment5Filter, _segment5Picker),
				}
			);
		}

		private int? GetSegmentValue(StackPanel filter, ComboBox picker)
		{
			return 
				filter.Visibility == Visibility.Visible && picker.SelectedIndex > 0 ?
					new Nullable<int>((int)picker.SelectedValue):
					null;
		}

		public override FloatingDialog[] FloatingDialogs
		{
			get
			{
				return new FloatingDialog[] { Gateway_dialog };
			}
		}

		private void _input_gatewayPage_Loaded(object sender, RoutedEventArgs e)
		{
			_input_gatewayPage = sender as AutocompleteCombo;
		}



		/*=========================*/
		#endregion

		#region Gateways
		/*=========================*/

		/// <summary>
		/// Add a gateway to an adunit
		/// </summary>
		private void Gateway_AddClick(object sender, RoutedEventArgs e)
		{
			if (!TryToCloseDialogs())
				return;

			// Get the parent adunit
			int selectedIndex = _listTable.ListView.SelectedIndex;
			Oltp.GatewayDataTable tbl = (_items[selectedIndex] as Oltp.GatewayRow).Table as Oltp.GatewayDataTable;

			// Create an editable new row
			Oltp.GatewayRow editVersion = Dialog_MakeEditVersion<Oltp.GatewayDataTable, Oltp.GatewayRow>(null);
			editVersion.AccountID = this.Window.CurrentAccount.ID;
			
			// Start with the last searched for ID (for user friendliness)
			int originalID;
			if (int.TryParse(_filterText.Text, out originalID))
				editVersion.Identifier = originalID;

			// Enable these fields for first use
			_input_originalID.IsReadOnly = false;

			// Show the dialog
			Gateway_dialog.Title = "New Tracker";
			Gateway_dialog.BeginEdit(editVersion, tbl);

			// Retrieve the gateway "applied to" data
			if (_gatewayAppliedTo == null)
				_gatewayAppliedTo = VisualTree.GetChild<StackPanel>(Gateway_dialog, "_gatewayAppliedTo");
		}

		/// <summary>
		/// Open the dialog
		/// </summary>
		private void Gateway_dialog_Open(object sender, RoutedEventArgs e)
		{
			if (!TryToCloseDialogs())
				return;

			// Set adunit as current item
			ListViewItem currentItem = _listTable.GetParentListViewItem(e.OriginalSource as FrameworkElement);
			Oltp.GatewayRow row = currentItem.Content as Oltp.GatewayRow;

			// Enable these fields for first use
			_input_originalID.IsReadOnly = true;

			// Get the selected page
			_input_gatewayPage.Text = String.Empty;
			PagePicker_ItemsSourceRequired(_input_gatewayPage, EventArgs.Empty);
			DataRow[] rs = _dropDownPages.Select(String.Format("GK = {0}", row.PageGK));
			if (rs.Length > 0)
				_input_gatewayPage.SelectedValue = rs[0];

			// Update the segments tab if it is selected
			TabControl tabs = VisualTree.GetChild<TabControl>(Gateway_dialog);
			TabItem tabSegments = VisualTree.GetChild<TabItem>(tabs, "_tabSegments");
			if (tabSegments.IsSelected)
				GatewaySegments_GotFocus(tabSegments, new RoutedEventArgs());

			// Show the dialog
			Gateway_dialog.Title = "Editing Tracker";
			Gateway_dialog.TitleTooltip = "GK #" + row.GK.ToString();

			Gateway_dialog.BeginEdit(
				Dialog_MakeEditVersion<Oltp.GatewayDataTable, Oltp.GatewayRow>(row),
				row
			);

			// Retrieve and bind the gateway "applied to" data
			if (_gatewayAppliedTo == null)
				_gatewayAppliedTo = VisualTree.GetChild<StackPanel>(Gateway_dialog, "_gatewayAppliedTo");
			_gatewayAppliedTo.DataContext = new GatewayReferenceData(row);

			// When opening, select it only if no more than one is already selected
			if (_listTable.ListView.SelectedItems.Count < 2)
			{
				_listTable.ListView.SelectedItems.Clear();
				currentItem.IsSelected = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void Gateway_dialog_ApplyingChanges(object sender, CancelRoutedEventArgs e)
		{
			// Check for unique ID
			Oltp.GatewayRow editVersion = Gateway_dialog.Content as Oltp.GatewayRow;
			if (editVersion.RowState == DataRowState.Added)
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					// Check to see if requested gateway exists
					Oltp.GatewayDataTable usedGW = proxy.Service.Gateway_GetByIdentifier(Window.CurrentAccount.ID, editVersion.Identifier);
					if (usedGW.Rows.Count > 0)
					{
						MainWindow.MessageBoxError(
							String.Format("Tracker {0} is already taken. Please use a different identifier.", editVersion.Identifier), null);

						_input_originalID.Focus();
						_input_originalID.SelectAll();
						e.Cancel = true;
						return;
					}
					else
					{
						Oltp.GatewayReservationDataTable reservations = proxy.Service.GatewayReservation_GetByIdentifier(Window.CurrentAccount.ID, editVersion.Identifier);
						if (reservations.Rows.Count > 0)
						{
							MainWindow.MessageBoxError(
								String.Format("Tracker {0} has been reserved by {1}. Please use a different identifier.", editVersion.Identifier, (reservations.Rows[0] as Oltp.GatewayReservationRow).ReservedByUserName), null);

							_input_originalID.Focus();
							_input_originalID.SelectAll();
							e.Cancel = true;
							return;
						}
					}
				}
			}

			// Get the target page
			Oltp.PageRow page = null;
			try { page = GetAutoCompletePage(_input_gatewayPage, _dropDownPages, true, true); }
			catch { e.Cancel = true; return; }

			if (page != null)
			{
				if (page.RowState == DataRowState.Added)
				{
					try
					{
						OltpProxy proxy = new OltpProxy();
						using (proxy)
						{
							page = proxy.Service.Page_Save(page.Table as Oltp.PageDataTable).Rows[0] as Oltp.PageRow;
						}
					}
					catch (Exception ex)
					{
						MainWindow.MessageBoxError("Failed to create a new page for this tracker.", ex);
						e.Cancel = true;
						return;
					}
				}

			}
			if (page != null && (editVersion.IsPageGKNull() || editVersion.PageGK != page.GK))
			{
				editVersion.PageGK = page.GK;
			}
			else if (page == null && !editVersion.IsPageGKNull())
			{
				editVersion.SetPageGKNull();
			}
			

	
			// Call the universal dialog apply changes handler method
			Dialog_ApplyingChanges<Oltp.GatewayDataTable, Oltp.GatewayRow>(
				Gateway_dialog.TargetContent is DataTable ?
					Gateway_dialog.TargetContent as Oltp.GatewayDataTable :
					(Gateway_dialog.TargetContent as Oltp.GatewayRow).Table as Oltp.GatewayDataTable,
				Gateway_dialog,
				typeof(IOltpLogic).GetMethod("Gateway_Save"),
				e, null, true, null);
		}

		/// <summary>
		/// 
		/// </summary>
		private void Gateway_dialog_AppliedChanges(object sender, CancelRoutedEventArgs e)
		{
			// Can't edit these anymore
			_input_originalID.IsReadOnly = true;

			Oltp.GatewayRow editVersion = Gateway_dialog.Content as Oltp.GatewayRow;
			Oltp.GatewayRow targetVersion = Gateway_dialog.TargetContent as Oltp.GatewayRow;
			
			// Little hack for page GK till this horrible UI is flushed down the toilet
			targetVersion.PageGK = editVersion.PageGK;

			int minIndex = -1;
			bool replaceTarget = false;

			// If the selected index is a gateway, that means the row was added correctly so we can quit
			int selectedIndex = _listTable.ListView.SelectedIndex;
			
			// Call the univeral applied change handler
			Dialog_AppliedChanges<Oltp.GatewayRow>(Gateway_dialog, "Editing Tracker", _listTable.ListView, minIndex, e);

			if (replaceTarget)
			{
				// Since we expanded the adunit to show the new gateway, set it as the new target content of the dialog
				Oltp.GatewayDataTable gwTbl = (_items[selectedIndex+1] as Oltp.GatewayRow).Table as Oltp.GatewayDataTable;
				DataRow[] rs = gwTbl.Select(String.Format("{0} = {1}", gwTbl.GKColumn.ColumnName, editVersion.GK));
				if (rs.Length > 0)
				{
					Oltp.GatewayRow newTarget = rs[0] as Oltp.GatewayRow;
					Gateway_dialog.TargetContent = newTarget;
					_listTable.ListView.SelectedItem = newTarget;
					_listTable.ListView.ScrollIntoView(newTarget);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void Gateway_dialog_Closing(object sender, CancelRoutedEventArgs e)
		{
			// Cancel if user regrets
			e.Cancel = MainWindow.MessageBoxPromptForCancel(Gateway_dialog);

			if (!e.Cancel)
			{
				_gatewayAppliedTo.DataContext = null;
				_dropDownPages = null;
				_input_gatewayPage.ItemsSource = null;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		private void Gateway_dialog_Loaded(object sender, RoutedEventArgs e)
		{
			_input_originalID = VisualTree.GetChild<TextBox>(Gateway_dialog, "_input_originalID");
		}

		private void _gatewayChannelPicker_Loaded(object sender, RoutedEventArgs e)
		{
			_gatewayChannelPicker = sender as ComboBox;
		}

		private void GatewaySegments_GotFocus(object sender, RoutedEventArgs e)
		{
			if (_tabSegmentsInitialized)
				return;

			foreach (KeyValuePair<Oltp.SegmentRow, Oltp.SegmentValueDataTable> pair in _segmentValueTables)
			{
				// Ignore segments that aren't adgroup related
				if ((pair.Key.AssociationFlags & SegmentAssociationFlags.Gateyway) == 0)
					continue;

				StackPanel segmentPanel = VisualTree.GetChild<StackPanel>(Gateway_dialog, "_segment" + pair.Key.SegmentNumber.ToString());
				segmentPanel.Visibility = Visibility.Visible;

				VisualTree.GetChild<Label>(segmentPanel).Content = pair.Key.Name;
				VisualTree.GetChild<ComboBox>(segmentPanel).ItemsSource = pair.Value.Rows;
			}

			_tabSegmentsInitialized = true;
		}

		private void GatewayBatch_Click(object sender, RoutedEventArgs e)
		{
			if (!TryToCloseDialogs())
				return;

			_batchChannelPicker.SelectedIndex = 0;
			VisualTree.GetChild<ComboBox>(_batchSegment1).SelectedIndex = 0;
			VisualTree.GetChild<ComboBox>(_batchSegment2).SelectedIndex = 0;
			VisualTree.GetChild<ComboBox>(_batchSegment3).SelectedIndex = 0;
			VisualTree.GetChild<ComboBox>(_batchSegment4).SelectedIndex = 0;
			VisualTree.GetChild<ComboBox>(_batchSegment5).SelectedIndex = 0;
			_batchRangeText.SelectAll();
			_batchRangeText.Focus();

			PagePicker_ItemsSourceRequired(_batchPage, EventArgs.Empty);
			GatewayBatch_dialog.IsOpen = true;
		}

		private void GatewayBatch_dialog_ApplyingChanges(object source, CancelRoutedEventArgs e)
		{
			List<long[]> ranges = new List<long[]>();
			string[] rangeStrings = _batchRangeText.Text.Split(',');
			foreach (string rangeString in rangeStrings)
			{
				bool error = false;
				string[] boundStrings = rangeString.Split(new char[]{'-'}, StringSplitOptions.RemoveEmptyEntries);
				if (boundStrings.Length == 1 || boundStrings.Length == 2)
				{
					long[] bounds = new long[boundStrings.Length];
					for(int i = 0; i < bounds.Length; i++)
					{
						if (!long.TryParse(boundStrings[i], out bounds[i]))
						{
							error = true;
							break;
						}
					}

					ranges.Add(bounds);
				}
				else
					error = true;

				if (error)
				{
					MainWindow.MessageBoxError("Make sure the range is in the correct format: 111-222,333-444,555,666 etc.", null);
					_batchRangeText.SelectAll();
					_batchRangeText.Focus();
					e.Cancel = true;
					return;
				}
			}

			Oltp.PageRow page = null;
			if (_batchPage.Text.Trim().Length > 0)
			{
				page = GetAutoCompletePage(_batchPage, _dropDownPages, true);
				if (page == null)
				{
					e.Cancel = true;
					return;
				}
			}
			
			int affected = 0;
			int existing = 0;
			try
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					// Save the new page if necessary
					if (page != null && page.RowState == DataRowState.Added)
						page = proxy.Service.Page_Save(page.Table as Oltp.PageDataTable).Rows[0] as Oltp.PageRow;

					// Update the gateways
					int[] results = proxy.Service.Gateway_BatchProperties(
						Window.CurrentAccount.ID,
						ranges.ToArray(),
						_batchChannelPicker.SelectedIndex > 0 ? new Nullable<int>((int)_batchChannelPicker.SelectedValue) : null,
						page == null ? null : new Nullable<long>(page.GK),
						new int?[]
						{
							GetSegmentValue(_batchSegment1, VisualTree.GetChild<ComboBox>(_batchSegment1)),
							GetSegmentValue(_batchSegment2, VisualTree.GetChild<ComboBox>(_batchSegment2)),
							GetSegmentValue(_batchSegment3, VisualTree.GetChild<ComboBox>(_batchSegment3)),
							GetSegmentValue(_batchSegment4, VisualTree.GetChild<ComboBox>(_batchSegment4)),
							GetSegmentValue(_batchSegment5, VisualTree.GetChild<ComboBox>(_batchSegment5))
						}
					);

					affected = results[0];
					existing = results[1];
				}
			}
			catch (Exception ex)
			{
				MainWindow.MessageBoxError("Failed to apply batch operation.", ex);
				e.Cancel = true;
				return;
			}

			// 
			MessageBox.Show(String.Format("{0} trackers were affected (of which {1} are new).", affected, affected-existing), "Success", MessageBoxButton.OK, MessageBoxImage.Information);
			_gatewayTable = null;
			_listTable.ListView.ItemsSource = _items = null;

		}

		private void GatewayBatch_dialog_Closing(object source, CancelRoutedEventArgs e)
		{
			e.Cancel = true;
			GatewayBatch_dialog.IsOpen = false;
			_dropDownPages = null;
			_gatewayCreate_Page.ItemsSource = null;
		}

		//=============================================================================================================================
		//=============================================================================================================================
		//=============================================================================================================================

		#region Other Dialogs
		//===================
		
		private int[] GetOtherAccountsToCheckAgainst()
		{
			return new int[0];
			#region Disabled
			/*
			// Get other accounts to check against
			List<int> otherAccounts = new List<int>();
			for (int i = 0; i < Window.AvailableAccounts.Count; i++)
			{
				ListBoxItem item = (ListBoxItem) _gatewayReserve_crossCheckAccounts.ItemContainerGenerator.ContainerFromIndex(i);
				if (VisualTree.GetChild<CheckBox>(item).IsChecked == true)
					otherAccounts.Add(Window.AvailableAccounts[i].ID);
			}

			// Save the list as a cookie
			if (otherAccounts.Count == 0)
			{
				App.Cookies.ClearCookie(Const.Cookies.AccountsForGatewayIdCheck + Window.CurrentAccount.ID.ToString());
			}
			else
			{
				App.Cookies[Const.Cookies.AccountsForGatewayIdCheck + Window.CurrentAccount.ID.ToString()] = ConvertToDelimitedString(otherAccounts, ',');
			}

			return otherAccounts.ToArray();
			*/
			#endregion
		}

		private string ConvertToDelimitedString(IEnumerable otherAccounts, char delimiter)
		{
			string output = string.Empty;
			foreach (object s in otherAccounts)
				output += (output.Length > 0 ? delimiter.ToString() : String.Empty) + s.ToString();
			return output;

		}


		/// <summary>
		/// 
		/// </summary>
		private void PagePicker_ItemsSourceRequired(object sender, EventArgs e)
		{
			AutocompleteCombo combo = sender as AutocompleteCombo;

			if (Window.CurrentAccount == null)
			{
				combo.ItemsSource = null;
				return;
			}

			// Filter the results for the autocomplete box
			using (OltpProxy proxy = new OltpProxy())
			{
				_dropDownPages = proxy.Service.Page_Get(Window.CurrentAccount.ID, combo.Text + "%", true, 100);
			}

			if (_dropDownPages.Rows.Count < 1)
				combo.ItemsSource = null;
			else
				combo.ItemsSource = _dropDownPages.Rows;
		}

		private Oltp.PageRow GetAutoCompletePage(AutocompleteCombo combo, Oltp.PageDataTable filteredPagesTable, bool createIfUnrecognized)
		{
			return GetAutoCompletePage(combo, filteredPagesTable, createIfUnrecognized, false);
		}

		private Oltp.PageRow GetAutoCompletePage(AutocompleteCombo combo, Oltp.PageDataTable filteredPagesTable, bool createIfUnrecognized, bool allowNone)
		{
			// Locate the page in the list
			DataRow[] rs = filteredPagesTable.Select(String.Format("Title = '{0}'", combo.Text));
			if (rs.Length == 0)
				rs = filteredPagesTable.Select(String.Format("URL = '{0}'", combo.Text));

			Oltp.PageRow page = null;
			if (rs.Length == 0 && createIfUnrecognized)
			{
				if (createIfUnrecognized)
				{
					bool invalid =
						(!allowNone && String.IsNullOrEmpty(combo.Text)) ||
						(
							((allowNone && !String.IsNullOrEmpty(combo.Text)) || !allowNone) &&
							!Uri.IsWellFormedUriString(combo.Text, UriKind.Absolute)
						);

					if (invalid)
					{
						MainWindow.MessageBoxError("The page you entered is not a valid URL. \n" +
						"Please enter a valid URL or the name of an existing page.", null);

						if (allowNone)
							throw new Exception("Invalid page URL.");

						return null;
					}
					else
					{
						// Create a new row
						Oltp.PageDataTable newPage = new Oltp.PageDataTable();
						page = newPage.NewPageRow();
						page.AccountID = Window.CurrentAccount.ID;
						page.URL = combo.Text;
						newPage.AddPageRow(page);
					}
				}
			}
			else if (rs.Length > 0)
			{
				page = rs[0] as Oltp.PageRow;
			}

			return page;
		}


		/// <summary>
		/// 
		/// </summary>
		private void Gateway_ReserveClick(object sender, RoutedEventArgs e)
		{
			GatewayReserve_dialog.IsOpen = true;
			PagePicker_ItemsSourceRequired(_gatewayReserve_Page, EventArgs.Empty);

			#region Disabled
			/*
			// Set last used check accounts
			string[] ids = null;
			string cookie = App.Cookies[Const.Cookies.AccountsForGatewayIdCheck + Window.CurrentAccount.ID.ToString()];
			if (cookie != null)
				ids = cookie.Split(',');

			for (int i = 0; i < Window.AvailableAccounts.Count; i++)
			{
				ListBoxItem item = (ListBoxItem) _gatewayReserve_crossCheckAccounts.ItemContainerGenerator.ContainerFromIndex(i);
				VisualTree.GetChild<CheckBox>(item).IsChecked = ids != null && ids.Contains<string>(Window.AvailableAccounts[i].ID.ToString());
			}
			*/
			#endregion
		}

		/// <summary>
		/// 
		/// </summary>
		private void GatewayReserve_dialog_ApplyingChanges(object source, CancelRoutedEventArgs e)
		{
			long fromID = 0;
			long toID = 0;

			if (!Int64.TryParse(_gatewayReserve_From.Text, out fromID) || !Int64.TryParse(_gatewayReserve_To.Text, out toID))
			{
				// Validation
				MainWindow.MessageBoxError("Please enter 2 valid identifiers.", null);
				_gatewayReserve_From.SelectAll();
				_gatewayReserve_From.Focus();
				e.Cancel = true;
				return;
			}

			// Get other accounts to check against
			int[] otherAccounts = GetOtherAccountsToCheckAgainst();

			// Check if any gateways are already in use or reserved
			int nonReservableCount = 0;
			using (OltpProxy proxy = new OltpProxy())
			{
				Oltp.GatewayReservationDataTable existingReservations =
					proxy.Service.GatewayReservation_GetByOverlap(Window.CurrentAccount.ID, fromID, toID, otherAccounts);

				if (existingReservations.Rows.Count > 0)
				{
					Oltp.GatewayReservationRow existingReservation = existingReservations.Rows[0] as Oltp.GatewayReservationRow;
					MainWindow.MessageBoxError(String.Format(
						"An overlapping range ({0} to {1}) has already been reserved by {2}.",
						existingReservation.FromGateway, existingReservation.ToGateway, existingReservation.ReservedByUserName
						), null);
					_gatewayReserve_From.SelectAll();
					_gatewayReserve_From.Focus();
					e.Cancel = true;
					return;
				}

				nonReservableCount = proxy.Service.Gateway_CountByRange(Window.CurrentAccount.ID, fromID, toID, otherAccounts);
			}

			if (nonReservableCount > 0)
			{
				MessageBoxResult result = MessageBox.Show(
					String.Format("{0} tracker{1} already in use within the specified range. Only available identifiers within the range will be reserved.\n\nProceed?",
						nonReservableCount > toID - fromID ? "All" : nonReservableCount.ToString(),
						nonReservableCount > 1 ? "s are" : " is"
						),
					"Confirmation",
					MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK
					);

				if (result == MessageBoxResult.Cancel)
				{
					_gatewayReserve_From.SelectAll();
					_gatewayReserve_From.Focus();
					e.Cancel = true;
					return;
				}
			}
			else
			{
				// Confirm the assignment
				MessageBoxResult mbresult = MessageBox.Show(
						String.Format("Reserve trackers {0} to {1}?", fromID, toID
					),
					"Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);

				if (mbresult == MessageBoxResult.Cancel)
				{
					_gatewayReserve_From.SelectAll();
					_gatewayReserve_From.Focus();
					e.Cancel = true;
					return;
				}
			}

			// Get the target page
			Oltp.PageRow page = null;
			if (_gatewayReserve_Unspecified.IsChecked == false)
			{
				page = GetAutoCompletePage(_gatewayReserve_Page, _dropDownPages, true);
				if (page == null)
				{
					// Failed to return a page
					e.Cancel = true;
					return;
				}
			}

			try
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					// Save the new page if necessary
					if (page != null && page.RowState == DataRowState.Added)
						page = proxy.Service.Page_Save(page.Table as Oltp.PageDataTable).Rows[0] as Oltp.PageRow;

					Oltp.GatewayReservationDataTable reservationTable = new Oltp.GatewayReservationDataTable();
					Oltp.GatewayReservationRow reservation = reservationTable.NewGatewayReservationRow();
					reservation.AccountID = Window.CurrentAccount.ID;
					reservation.FromGateway = fromID;
					reservation.ToGateway = toID;
					reservation.PageGK = page == null? -1 : page.GK;
					reservation.ReservedByUserID = OltpProxy.CurrentUser.ID;
					reservation.ReservedByUserName = OltpProxy.CurrentUser.Name;
					reservation.CrossCheckAccounts = otherAccounts.Length > 0 ? ConvertToDelimitedString(otherAccounts, ',') : null;
					reservationTable.AddGatewayReservationRow(reservation);

					proxy.Service.GatewayReservation_Save(reservationTable);
				}
			}
			catch (Exception ex)
			{
				MainWindow.MessageBoxError("Could not complete the reservation.", ex);
				e.Cancel = true;
				return;
			}


			// Done message
			MessageBox.Show(String.Format("Tracker identifiers {0} to {1} were successfully reserved.", fromID, toID),
				"Information",
				MessageBoxButton.OK, MessageBoxImage.Information);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void GatewayReserve_dialog_Closing(object source, CancelRoutedEventArgs e)
		{
			e.Cancel = true;
			GatewayReserve_dialog.IsOpen = false;
			_dropDownPages = null;
			_gatewayReserve_Page.ItemsSource = null;
		}

		private void _gatewayReserve_crossCheckAccounts_Loaded(object sender, RoutedEventArgs e)
		{
			_gatewayReserve_crossCheckAccounts.ItemsSource = Window.AvailableAccounts;
		}


		//===============================================================

		/// <summary>
		/// 
		/// </summary>
		private void Gateway_CreateClick(object sender, RoutedEventArgs e)
		{
			if (_gatewayCreate_Channel.SelectedIndex < 0)
			{
				_gatewayCreate_Channel.SelectedIndex = _channelPicker.SelectedIndex > 0 ? _channelPicker.SelectedIndex - 1 : 0;
			}
			GatewayCreate_dialog.IsOpen = true;
			PagePicker_ItemsSourceRequired(_gatewayCreate_Page, EventArgs.Empty);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		private void GatewayCreate_dialog_ApplyingChanges(object source, CancelRoutedEventArgs e)
		{
			int autoCount = 0;

			if (!Int32.TryParse(_gatewayCreate_auto.Text, out autoCount))
			{
				MainWindow.MessageBoxError("Please enter a valid number.", null);
				_gatewayCreate_auto.SelectAll();
				_gatewayCreate_auto.Focus();
				e.Cancel = true;
				return;
			}

			// Get the target page
			Oltp.PageRow page = null;
			if (_gatewayCreate_Unspecified.IsChecked == false)
			{
				page = GetAutoCompletePage(_gatewayCreate_Page, _dropDownPages, false);
				if (page == null)
				{
					// Failed to return a page
					MainWindow.MessageBoxError("Please select an existing page", null);
					e.Cancel = true;
					return;
				}
			}

			
			Oltp.GatewayReservationDataTable reservations;
			using (OltpProxy proxy = new OltpProxy())
			{
				reservations = proxy.Service.GatewayReservation_GetByPage(Window.CurrentAccount.ID, page == null ? -1 : page.GK);
			}

			if (reservations.Count < 1)
			{
				MainWindow.MessageBoxError(
					page == null ?
						"Cannot generate trackers with an unspecified page because no unspecified range has been reserved." :
						"Cannot generate trackers for this page because no range has been reserved for it."
					, null
				);
				e.Cancel = true;
				return;
			}


			// Get the destURL format
			string destUrlFormat = Window.CurrentAccount.GatewayBaseUrl;
			if (destUrlFormat == null)
			{
				destUrlFormat = "{0}";
			}
			else if (destUrlFormat.IndexOf("{page}") >= 0)
			{
				destUrlFormat = destUrlFormat.Replace("{page}", page.URL);
			}

			if (destUrlFormat.IndexOf("{0}") < 0)
			{
				destUrlFormat += "#{0}";
			}

			// Check if it is expanded
			Oltp.GatewayDataTable newGateways;
			try
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					newGateways = proxy.Service.Gateway_CreateQuantity(autoCount, Window.CurrentAccount.ID, -1, page.GK, destUrlFormat, true);
				}
			}
			catch (Exception ex)
			{
				MainWindow.MessageBoxError("Could not complete the reservation.", ex);
				e.Cancel = true;
				return;
			}

			// Auto count updated to reflect actually generated gateways
			if (newGateways.Rows.Count < 1)
			{
				MessageBox.Show("No trackers were generated.\n\nThis most likely means that all identifiers within the reserved range(s) are already taken.",
					"Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
				e.Cancel = true;
				return;
			}

			MessageBoxResult copytoClipboard = MessageBox.Show(
				String.Format("{0} new trackers generated! Copy new tracker URLs to the clipboard?", newGateways.Rows.Count),
				"Information",
				MessageBoxButton.YesNo,
				MessageBoxImage.Information,
				MessageBoxResult.Yes);

			if (copytoClipboard == MessageBoxResult.Yes)
			{
				string textForClipboard = String.Empty;
				foreach (Oltp.GatewayRow gw in newGateways.Rows)
				{
					textForClipboard += (textForClipboard.Length > 0 ? Environment.NewLine : String.Empty) + gw.DestinationURL.ToString();
				}
				try
				{
					Clipboard.SetText(textForClipboard);
				}
				catch (Exception ex)
				{
					MainWindow.MessageBoxError(String.Format(
						"Couldn't copy trackers to clipboard. The trackers that were generated are:\n\n{0}",
						textForClipboard)
						, ex);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void GatewayCreate_dialog_Closing(object source, CancelRoutedEventArgs e)
		{
			e.Cancel = true;
			GatewayCreate_dialog.IsOpen = false;
			_dropDownPages = null;
			_gatewayCreate_Page.ItemsSource = null;
		}
		/////////
		#endregion

		/*=========================*/
		#endregion
	}

	public class GatewayReferenceData: CampaignAdgroupCombination
	{
		private Oltp.GatewayRow _gateway = null;
		private DataRow _refObject = null;

		public GatewayReferenceData(Oltp.GatewayRow gateway): base(null, null)
		{
			_gateway = gateway;
			
			if (_gateway.AdgroupGK < 0)
				return;

			using (OltpProxy proxy = new OltpProxy())
			{
				// Get the parent adgroup
				Oltp.AdgroupDataTable ags = proxy.Service.Adgroup_GetSingle(_gateway.AdgroupGK);
				if (ags.Rows.Count < 1)
					return;
				_adgroup = ags.Rows[0] as Oltp.AdgroupRow;

				// Get the parent campaign
				Oltp.CampaignDataTable cmpn = proxy.Service.Campaign_GetSingle(_adgroup.CampaignGK);
				if (cmpn.Rows.Count > 0)
					_campaign = cmpn.Rows[0] as Oltp.CampaignRow;

				// Get the appropriate reference
				if (!_gateway.IsReferenceTypeNull())
				{
					DataTable refTable;
					refTable = _gateway.ReferenceType == Oltp.GatewayReferenceType.Creative ?
					(DataTable) proxy.Service.Creative_GetSingle(_gateway.ReferenceID) :
					(DataTable) proxy.Service.Keyword_GetSingle(_gateway.ReferenceID);

					if (refTable.Rows.Count > 0)
						_refObject = refTable.Rows[0];
				}
			}
		}

		public Oltp.GatewayRow Gateway
		{
			get
			{
				return _gateway;
			}
		}

		public string DisplayName
		{
			get
			{
				if (_refObject == null)
					return "(unavailable)";
				else
					return _gateway.ReferenceType == Oltp.GatewayReferenceType.Creative ? 
						(_refObject as Oltp.CreativeRow).Title : 
						(_refObject as Oltp.KeywordRow).Keyword;
			}
		}
		
		public string DisplayType
		{
			get
			{
				return _gateway.ReferenceType == Oltp.GatewayReferenceType.Creative ? 
					"Creative" : 
					"Keyword" ;
			}
		}
	}
}

namespace Easynet.Edge.UI.Client.PpcGatewaysLocal
{
	# region Value converters

	[ValueConversion(typeof(DateTime), typeof(string))]
	public class DateConverter: IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			DateTime d = (DateTime) value;
			return d.ToString("MMMM yyyy");
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string s = value.ToString();
			DateTime d;
			if (DateTime.TryParse(s, out d))
				return d;
			else
				return s;
		}

		#endregion
	}

	[ValueConversion(typeof(int), typeof(string))]
	public class ChannelNameConverter: IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is int)
			{
				int channelID = (int) value;
				Oltp.ChannelDataTable chs = (App.CurrentPage as Pages.PpcGateways)._channelTable;
				DataRow[] rs = chs.Select(String.Format("{0} = {1}", chs.IDColumn.ColumnName, channelID));
				if (rs.Length > 0)
					return (rs[0] as Oltp.ChannelRow)[parameter != null ? parameter.ToString() : "DisplayName"];
			}

			return value != null ? value.ToString() : null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is string)
			{
				Oltp.ChannelDataTable chs = (App.CurrentPage as Pages.PpcGateways)._channelTable;
				DataRow[] rs = chs.Select(String.Format("{0} = {1}", chs.NameColumn.ColumnName, value));
				if (rs.Length > 0)
					return (rs[0] as Oltp.ChannelRow).ID;
			}

			return value;
		}

		#endregion
	}

	[ValueConversion(typeof(int), typeof(string))]
	public class AdunitNameConverter: IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is Oltp.AdunitRow)
			{
				Oltp.AdunitRow adunit = (Oltp.AdunitRow) value;
				return adunit.IsDefaultAdunit ? "Default Adunit" : adunit.Name;
			}
			else
				return value != null ? value.ToString() : null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is string)
			{
				Oltp.ChannelDataTable chs = (App.CurrentPage as Pages.PpcGateways)._channelTable;
				DataRow[] rs = chs.Select(String.Format("{0} = {1}", chs.NameColumn.ColumnName, value));
				if (rs.Length > 0)
					return (rs[0] as Oltp.ChannelRow).ID;
			}

			return value;
		}

		#endregion
	}

	#endregion

	#region Attached properties

	public class Props
	{
		public static bool GetIsInProgress(DependencyObject obj)
		{
			return (bool) obj.GetValue(IsInProgressProperty);
		}

		public static void SetIsInProgress(DependencyObject obj, bool value)
		{
			obj.SetValue(IsInProgressProperty, value);
		}

		// Using a DependencyProperty as the backing store for IsInProgress.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsInProgressProperty =
		   DependencyProperty.RegisterAttached("IsInProgress", typeof(bool), typeof(Props), new UIPropertyMetadata(false));

	}

	#endregion

	#region Local

	/// <summary>
	/// 
	/// </summary>
	public class ExpandToggleTemplateSelector: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is Oltp.AdunitRow)
				return App.CurrentPage
					.FindResource("AdunitExpandToggleTemplate") as DataTemplate;
			else
				return new DataTemplate();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class ChannelPickerChannelTemplateSelector: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is Oltp.ChannelRow)
				return App.CurrentPage
					.FindResource("ChannelPickerChannelTemplate") as DataTemplate;
			else
				return null;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class ChannelTemplateSelector: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is Oltp.AdunitRow)
				return App.CurrentPage
					.FindResource("ChannelTemplate") as DataTemplate;
			else
				return new DataTemplate();
		}
	}

	public class NameTemplateSelector: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is Oltp.AdunitRow)
			{
				Oltp.AdunitRow adunit = item as Oltp.AdunitRow;

				if (adunit.IsDefaultAdunit)
				{
					return App.CurrentPage
						.FindResource("AdunitDefaultNameTemplate") as DataTemplate;
				}
				else if (adunit.ID < 0)
				{
					return App.CurrentPage
						.FindResource("AdunitOrphanNameTemplate") as DataTemplate;
				}

				else
				{
					return App.CurrentPage
						.FindResource("AdunitNameTemplate") as DataTemplate;
				}
			}
			else if (item is Oltp.GatewayRow)
			{
				return App.CurrentPage
					.FindResource("GatewayNameTemplate") as DataTemplate;
			}
			else
				return new DataTemplate();
		}
	}

	public class OptionsTemplateSelector: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is Oltp.AdunitRow)
			{
				Oltp.AdunitRow adunit = item as Oltp.AdunitRow;

				if (adunit.ID < 0)
				{
					return App.CurrentPage
						.FindResource("AdunitUnassociatedOptionsTemplate") as DataTemplate;
				}
				else
				{
					return App.CurrentPage
						.FindResource("AdunitOptionsTemplate") as DataTemplate;
				}
			}
			else if (item is Oltp.GatewayRow)
			{
				return App.CurrentPage
					.FindResource("GatewayOptionsTemplate") as DataTemplate;
			}
			else
				return new DataTemplate();
		}
	}

	#endregion

}