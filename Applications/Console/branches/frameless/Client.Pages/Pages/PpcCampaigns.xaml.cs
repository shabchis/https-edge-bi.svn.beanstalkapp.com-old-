﻿using System;
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
using Easynet.Edge.BusinessObjects;
using System.Diagnostics;
using Easynet.Edge.UI.Server;

namespace Easynet.Edge.UI.Client.Pages
{
	/// <summary>
	/// Interaction logic for PpcAdUnits.xaml
	/// </summary>
	[AccountDependentPage]
	public partial class PpcCampaigns: PageBase
	{
		#region Fields
		/*=========================*/

		private ObservableCollection<DataRow> _items;
		Oltp.CampaignDataTable _campaigns;
		ListTable _adgroupKeywords;
		ListTable _adgroupCreatives;
		public Oltp.ChannelDataTable _channelTable;
		public Oltp.CampaignStatusDataTable _campaignStatusTable;
		Oltp.KeywordDataTable _adgroupKeywordPickerKeywords;
		Oltp.CreativeDataTable _adgroupCreativePickerCreatives;
		ComboBox _campaignChannelPicker;
		Oltp.AdgroupKeywordDataTable _adgroupKeywordTable;
		Oltp.AdgroupCreativeDataTable _adgroupCreativeTable;
		AutocompleteCombo _adgroupKeywordPicker;
		AutocompleteCombo _adgroupCreativePicker;
		Oltp.PageDataTable _pagesTable;
		Oltp.SegmentDataTable _segmentTable;
		Dictionary<Oltp.SegmentRow, Oltp.SegmentValueDataTable> _segmentValueTables;
		bool _campaignTabSegmentsInitialized = false;
		bool _adgroupTabSegmentsInitialized = false;
		Dictionary<long, List<ComboBox>> _adgroupCreativeSegmentCombos;
		ItemsControl _campaignTargetsControl;
		
		TargetsTable _targetData;
		List<GridViewColumn> _targetColumns;
		bool _targetsEnabled = false;
		//bool _targetsAttached = false; // decided to attach every time
		Measure[] _measures;

		string _filter;
		bool _filterAdgroups;

		/*=========================*/
		#endregion

		#region Constructors
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		public PpcCampaigns()
		{
			Window.AsyncStart("PpcCampaigns.ctor");

			Window.AsyncOperation(delegate()
			{
				// Get the channel list
				using (OltpProxy proxy = new OltpProxy())
				{
					_channelTable = proxy.Service.Channel_Get();
					_campaignStatusTable = proxy.Service.CampaignStatus_Get();
				}

				Oltp.CampaignStatusRow unknownRow = _campaignStatusTable.NewCampaignStatusRow();
				unknownRow.ID = -1;
				unknownRow.Name = "Unknown";
				_campaignStatusTable.Rows.InsertAt(unknownRow, 0);
				_campaignStatusTable.AcceptChanges();
			},
			delegate()
			{
				// Add the table as a resource
				Resources.Add("ChannelTableRows", _channelTable.Rows);
				InitializeComponent();
				_buttonMerge.Visibility = !OltpProxy.CurrentUser.IsAccountAdminNull() && OltpProxy.CurrentUser.AccountAdmin ?
					Visibility.Visible :
					Visibility.Collapsed;

				// Notify anyone waiting for the ctor that it is done
				Window.AsyncEnd("PpcCampaigns.ctor");

				// Channel picker
				ObservableCollection<object> channelPickerSource = new ObservableCollection<object>();
				channelPickerSource.Add("All channels");
				foreach (Oltp.ChannelRow ch in _channelTable.Rows)
					channelPickerSource.Add(ch);
				_channelPicker.ItemsSource = channelPickerSource;
				_channelPicker.SelectedIndex = 0;
				//_channelPicker.SelectionChanged += new SelectionChangedEventHandler(_channelPicker_SelectionChanged);

				// Status picker
				ObservableCollection<object> statusPickerSource = new ObservableCollection<object>();
				statusPickerSource.Add("All Status");
				Oltp.CampaignStatusRow activeRow = null;
				foreach (Oltp.CampaignStatusRow ch in _campaignStatusTable.Rows)
				{
					// select active by default
					if (ch.Name.ToLower() == "active")
						activeRow = ch;

					statusPickerSource.Add(ch);
				}
				_campaignStatusPicker.ItemsSource = statusPickerSource;
				_campaignStatusPicker.SelectedIndex = activeRow == null ? 0 : statusPickerSource.IndexOf(activeRow);
				//_campaignStatusPicker.SelectionChanged += new SelectionChangedEventHandler(_statusPicker_SelectionChanged);

				_listTable.ChildItemTypes = new Type[] { typeof(Oltp.AdgroupRow) };
			});
		}

		/*=========================*/
		#endregion

		#region General Methods
		/*=========================*/

		
		bool _changingChannel = false;
		bool _changingStatus = false;

		void _channelPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_changingChannel)
				return;

			_changingChannel = true;
			GetCampaigns(() => _changingChannel = false);
		}

		void _statusPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_changingStatus)
				return;

			_changingStatus = true;
			GetCampaigns(() => _changingStatus = false);
		}

		void _filterButton_Click(object sender, RoutedEventArgs e)
		{
			GetCampaigns(null);
		}

		private void GetCampaigns(Action onComplete)
		{
			Oltp.AccountRow account = Window.CurrentAccount;

			int? channelID = _channelPicker.SelectedIndex < 1 ? null : new int?((_channelPicker.SelectedValue as Oltp.ChannelRow).ID);
			int? statusID = _campaignStatusPicker.SelectedIndex < 1 ? null : new int?((_campaignStatusPicker.SelectedValue as Oltp.CampaignStatusRow).ID);
			_filterAdgroups = _campaignFilterByAdgroup.IsChecked != null && _campaignFilterByAdgroup.IsChecked.Value;
			_filter = _campaignFilterTextBox.Text.Trim();
			_filter = _filter.Length < 1 ?
				null :
				_filter.Contains('*') ?
					_filter.Replace('*', '%') : _filter + '%';
			
			// Mark targets as detached so when showing targets again, they will be reattached to their parent campaigns
			//_targetsAttached = false; // not needed anymore, they're reattached every time

			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					// Get the channel filter from the channel picker
					_campaigns = proxy.Service.Campaign_Get(
						account.ID,
						channelID,
						statusID,
						_filter,
						_filterAdgroups
						);
				}
			},
			delegate(Exception ex)
			{
				if (onComplete != null)
					onComplete();

				MainWindow.MessageBoxError("Failed to get campaigns.", ex);
				return false;
			},
			delegate()
			{
				// Get an empty new list
				_items = new ObservableCollection<DataRow>();

				// Add all campaigns
				foreach (Oltp.CampaignRow r in _campaigns.Rows)
				{
					// Get target data if available
					if (_targetData != null)
					{
						r.Targets = _targetData.GetCampaignTargets(r.GK);
						r.OnAllPropertiesChanged();
					}

					_items.Add(r);
				}

				_listTable.ListView.ItemsSource = _items;

				// Expand groups to show adgroups we found
				if (_filter != null && _filterAdgroups && _items.Count > 1)
				{
					_listTable.UpdateLayout();
					List<ToggleButton> expandButtons = new List<ToggleButton>();
					for (int i = 0; i < _listTable.ListView.Items.Count; i++)
					{
						DependencyObject item = _listTable.ListView.ItemContainerGenerator.ContainerFromIndex(i);
						expandButtons.Add(VisualTree.GetChild<ToggleButton>(item));
					}
					foreach (ToggleButton button in expandButtons)
						button.IsChecked = true;
				}

				if (onComplete != null)
					onComplete();
			});
		}

		public override bool OnAccountChanging(Oltp.AccountRow account)
		{
			// Confirm save changes
			if (_targetsEnabled)
			{
				if (!Targets_Save())
					return false;
			}

			return true;
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
			// General
			_items = null;
			_campaigns = null;

			_filter = null;
			if (_campaignFilterTextBox != null)
				_campaignFilterTextBox.Clear();

			//..............................
			// Segments
			_campaignTabSegmentsInitialized = false;
			_adgroupTabSegmentsInitialized = false;

			_segmentValueTables = new Dictionary<Oltp.SegmentRow, Oltp.SegmentValueDataTable>();

			Window.AsyncOperation(delegate()
			{
				Window.AsyncWait("PpcCampaigns.ctor");

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

					// Get all measures
					_measures = proxy.Service.Measure_Get(value.ID);

					// TOP 1000 because we want all the pages
					_pagesTable = proxy.Service.Page_Get(value.ID, null, true, 1000);
					Oltp.PageRow noneRow = _pagesTable.NewPageRow();
					noneRow.AccountID = value.ID;
					noneRow.Title = "(none)";
					_pagesTable.Rows.InsertAt(noneRow, 0);
					_pagesTable.AcceptChanges();
				}
			},
			delegate()
			{
				// Clear target columns
				//..............................
				GridView view = (GridView)_listTable.ListView.View;
				if (_targetColumns != null)
				{
					foreach (GridViewColumn column in _targetColumns)
						view.Columns.Remove(column);
				}
				_targetData = null;
				_targetColumns = null;

				//..............................
				// Campaigns
				GetCampaigns(delegate()
				{
					if (_targetsEnabled)
						Targets_BuildDisplay(value.ID);
				});
				
			});
		}

		public override bool Unload()
		{
			// Confirm save changes
			return base.Unload() && (!_targetsEnabled || Targets_Save());
		}

		#region Render to file - for PNG
		/*
		private void SaveAsBitmap()
		{
			//ToggleButton tb = Visual.GetDescendant<ToggleButton>(_listTable.ListView);
			//RenderToFile(tb, "C:\\tb-closed-hover.png");
			//tb.IsChecked = true;
			//tb.UpdateLayout();
			//RenderToFile(tb, "C:\\tb-open-hover.png");
			Ellipse tb = Visual.GetDescendant<Ellipse>(_listTable.ListView, "bullet");
			RenderToFile(tb, "C:\\bullet.png");
		}

		private static void RenderToFile(UIElement tb, string filename)
		{
			Rect rect = new Rect(tb.RenderSize);
			RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Width, (int)rect.Height, 96d, 96d, PixelFormats.Default);
			rtb.Render(tb);

			//endcode as PNG
			BitmapEncoder pngEncoder = new PngBitmapEncoder();
			pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

			//save to memory stream
			System.IO.MemoryStream ms = new System.IO.MemoryStream();

			pngEncoder.Save(ms);
			ms.Close();
			System.IO.File.WriteAllBytes(filename, ms.ToArray());
		}
		*/
		#endregion

		private void _listTable_SelectAll(object sender, RoutedEventArgs e)
		{
			if (_listTable.ListView.SelectedItems.Count == _listTable.ListView.Items.Count)
				_listTable.ListView.SelectedItems.Clear();
			else
				_listTable.ListView.SelectAll();
		}

		/// <summary>
		/// 
		/// </summary>
		private void _listTable_GroupExpanded(object sender, RoutedEventArgs e)
		{
			Oltp.CampaignRow campaign = (Oltp.CampaignRow) (e.OriginalSource as ListViewItem).Content;

			Oltp.AdgroupDataTable adgroups = null;

			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					adgroups = proxy.Service.Adgroup_Get(campaign.GK, _filterAdgroups ? _filter : null);
				}
			},
			delegate()
			{
				if (adgroups.Rows.Count > 0)
				{
					bool makeActive = false;
					int insertIndex = _items.IndexOf(campaign) + 1;
					foreach (Oltp.AdgroupRow adgroup in adgroups.Rows)
					{
						// Get target data if available
						if (_targetData != null)
						{
							adgroup.Targets = _targetData.GetAdgroupTargets(campaign.GK, adgroup.GK);

							// If the campaign is active but empty while the adgroup is not empty!
							if (campaign.Targets.IsActive && campaign.Targets.IsEmpty && !adgroup.Targets.IsEmpty)
							{
								campaign.Targets.IsActive = false;
								adgroup.Targets.IsActive = true;
								makeActive = true;
							}
							else
							{
								// The adgroup is active it the campaign is not, and vice versa
								adgroup.Targets.IsActive = !campaign.Targets.IsActive;
							}
						}

						_items.Insert(insertIndex++, adgroup);
					}

					if (makeActive)
					{
						foreach (Oltp.AdgroupRow a in adgroups)
							a.Targets.IsActive = true;
					}

				}
			});
		}
		
		/// <summary>
		/// 
		/// </summary>
		private void _listTable_GroupCollapsed(object sender, RoutedEventArgs e)
		{
			Oltp.CampaignRow campaign = (Oltp.CampaignRow) (e.OriginalSource as ListViewItem).Content;
			
			int removableIndex = _items.IndexOf(campaign)+1;

			// Keep removing all items after the campaign until we reach the end or another campaign
			while(_items.Count > removableIndex && _items[removableIndex] is Oltp.AdgroupRow)
				_items.RemoveAt(removableIndex);
		}



		/*=========================*/
		#endregion

		#region Targets
		/*=========================*/

		void Targets_Toggle(object sender, RoutedEventArgs e)
		{
			const string showText = "Open targets";
			const string hideText = "Cancel";

			// Confirm save changes
			if (_targetsEnabled)
			{
				if (!Targets_Save())
					return;
			}

			_targetsEnabled = !_targetsEnabled;
			_buttonTargetsToggle.Content = _targetsEnabled ? hideText : showText;

			_targetButtons.Visibility = _targetsEnabled ? Visibility.Visible : Visibility.Collapsed;

			Targets_BuildDisplay(Window.CurrentAccount.ID);
		}

		void Targets_BuildDisplay(int accountID)
		{
			//const double c_columnWidth = 70d;
			const double c_textboxWidth = 50d;

			if (_targetsEnabled) //&& _targetData == null) // decided to rebuild this each time
			{
				// Clear target columns
				GridView view = (GridView) _listTable.ListView.View;
				_targetColumns = new List<GridViewColumn>();

				Window.AsyncOperation(delegate()
				{
					// Get targets from server - all targets for all campaigns
					using (OltpProxy proxy = new OltpProxy())
					{
						_targetData = new TargetsTable(proxy.Service.CampaignTargets_Get(accountID, null));
					}
				},
				delegate()
				{
					// Apply measures to batch dialog
					VisualTree.GetChild<ItemsControl>(BatchTargets_dialog).ItemsSource = _measures;

					foreach (Measure measure in _measures)
					{
						GridViewColumn column = new GridViewColumn();
						column.Header = measure.DisplayName;

						// Bind the value to the corresponding measure inside the row.Targets dictionary
						NumberBinding numberBinding = new NumberBinding();
						numberBinding.Path = new PropertyPath(String.Format("Targets[{0}]", measure.FieldName));
						numberBinding.AllowEmpty = true;
						numberBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
						numberBinding.Converter = new NullableNumberConverter<double>();

						// Build the above template
						DataTemplate tpl = new DataTemplate();

						FrameworkElementFactory textBox = new FrameworkElementFactory();
						textBox.Type = typeof(TextBox);
						textBox.SetValue(TextBox.NameProperty, "measure" + measure.MeasureID.ToString());
						textBox.SetValue(TextBox.WidthProperty, c_textboxWidth);
						textBox.SetValue(TextBox.TextAlignmentProperty, TextAlignment.Right);

						Binding visibilityBinding = new Binding("Targets.IsActive");
						textBox.SetBinding(TextBox.IsEnabledProperty, visibilityBinding);
						textBox.SetValue(TextBox.StyleProperty, App.Current.Resources["FormValidatedInput"]);

						textBox.SetBinding(TextBox.TextProperty, numberBinding);

						tpl.VisualTree = textBox;
						column.CellTemplateSelector = new PpcCampaignsLocal.TargetsTemplateSelector(tpl);
						
						// Disabled to remove adgroup-level targets
						//column.CellTemplate = tpl;

						_targetColumns.Add(column);
						view.Columns.Add(column);
					}

					// Add the switch level column - DISABLED
					/*
					GridViewColumn switchLevelColumn = new GridViewColumn();
					switchLevelColumn.CellTemplateSelector = new PpcCampaignsLocal.TargetsSwitchTemplateSelector();
					_targetColumns.Add(switchLevelColumn);
					view.Columns.Add(switchLevelColumn);
					*/

					_listTable.UpdateLayout();
					Targets_AttachToItems();
					Targets_UpdateLayout();
				});
			}
			else
			{
				if (_targetsEnabled)// && !_targetsAttached) //decided to attach every time
					Targets_AttachToItems();

				Targets_UpdateLayout();
			}
		}

		private void Targets_AttachToItems()
		{
			Oltp.CampaignRow campaign = null;
			List<Oltp.AdgroupRow> adgroups = new List<Oltp.AdgroupRow>();

			// Iterate the items list - adgroups are always placed after their parent campaign,
			// so this is taken into consideration when deciding whether the targets are currently
			// applied to the campaign or to the child adgroups
			foreach (DataRow r in _items)
			{
				if (r is Oltp.CampaignRow)
				{
					adgroups.Clear();

					campaign = r as Oltp.CampaignRow;
					campaign.Targets = _targetData.GetCampaignTargets(campaign.GK);
				}
				else if (r is Oltp.AdgroupRow)
				{
					Oltp.AdgroupRow adgroup = r as Oltp.AdgroupRow;
					adgroup.Targets = _targetData.GetAdgroupTargets(Convert.ToInt32(adgroup.CampaignGK), adgroup.GK);

					// If the campaign is active but empty while the adgroup is not empty!
					if (campaign.Targets.IsActive && campaign.Targets.IsEmpty && !adgroup.Targets.IsEmpty)
					{
						campaign.Targets.IsActive = false;
						adgroup.Targets.IsActive = true;

						foreach (Oltp.AdgroupRow a in adgroups)
							a.Targets.IsActive = true;
					}
					else
					{
						// The adgroup is active it the campaign is not, and vice versa
						adgroup.Targets.IsActive = !campaign.Targets.IsActive;

						// Keep the adgroup in a list in case we might have to change its state later in the iteration
						if (campaign.Targets.IsActive && campaign.Targets.IsEmpty)
							adgroups.Add(adgroup);
					}

				}
			}

			//_targetsAttached = true; // not necessary anymore
		}

		private void Targets_UpdateLayout()
		{
			// Refresh columns for correct widths
			foreach (GridViewColumn col in _targetColumns)
			{
				if (_targetsEnabled)
				{
					col.ClearValue(GridViewColumn.WidthProperty);
				}
				else
					col.Width = 0;
			}
			_listTable.UpdateLayout();
			_listTable.UpdateColumnWidths();
		}

		void Targets_SaveClick(object sender, RoutedEventArgs e)
		{
			Targets_Save(false);
		}

		bool Targets_Save()
		{
			return Targets_Save(true);
		}

		bool Targets_Save(bool showConfirmation)
		{
			return Targets_Save(showConfirmation, _targetData.InnerTable);
		}

		bool Targets_Save(bool showConfirmation, DataTable targetData)
		{
			DataTable changes = targetData.GetChanges();
			if (changes == null)
			{
				if (!showConfirmation)
					MessageBox.Show("No changes have been made.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

				return true;
			}

			MessageBoxResult result = showConfirmation ?
				MessageBox.Show("Save changes to targets?", "Confirm", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes) :
				MessageBoxResult.Yes;

			// Cancel means stop this operation
			if (result == MessageBoxResult.Cancel)
				return false;

			Oltp.AccountRow account = Window.CurrentAccount;
			if (result == MessageBoxResult.Yes)
			{
				try
				{
					using (OltpProxy proxy = new OltpProxy())
					{
						proxy.Service.CampaignTargets_Save(account.ID, changes);
					}
					targetData.AcceptChanges();

					if (showConfirmation)
						MessageBox.Show("Targets saved successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				catch (Exception ex)
				{
					MainWindow.MessageBoxError("Failed to save targets.", ex);
					return false;
				}

			}
			else
			{
				targetData.RejectChanges();

				// Call OnAllPropertiesChanged to force a refresh of the view
				if (_targetsEnabled && targetData == _targetData.InnerTable)
				{
					foreach (DataRow row in _items)
					{
						TargetsRow targets = null;
						if (row is Oltp.CampaignRow)
							targets = (row as Oltp.CampaignRow).Targets;
						else if (row is Oltp.AdgroupRow)
							targets = (row as Oltp.AdgroupRow).Targets;

						if (targets != null)
							targets.OnAllPropertiesChanged();
					}
				}
			}

			return true;
		}

		#region Unneeded stuff be gone
		/*
		private void Targets_SwitchLevel(object sender, RoutedEventArgs e)
		{
			MessageBoxResult warningResult = MessageBox.Show("Warning: existing targets will be deleted. Continue?", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Yes);
			if (warningResult == MessageBoxResult.Cancel)
				return;

			ListViewItem currentItem = _listTable.GetParentListViewItem(e.OriginalSource as FrameworkElement);
			Oltp.CampaignRow campaignRow = currentItem.Content as Oltp.CampaignRow;

			if (campaignRow.Targets.IsActive)
			{
				// Delete the campaign targets
				_targetData.DeleteCampaignTargets(campaignRow.GK);
			}
			else
			{
				foreach (DataRow adgroupTargetData in _targetData.InnerTable.Select(String.Format("CampaignGK = {0} and AdgroupGK > -1", campaignRow.GK)))
				{
					adgroupTargetData.Delete();
				}
			}

			// Swap properties and update
			campaignRow.Targets.IsActive = !campaignRow.Targets.IsActive;
			campaignRow.Targets.OnAllPropertiesChanged();

			// Iterate any adgroups beneath the campaign
			for (int i = _items.IndexOf(campaignRow) + 1; _items.Count > i && _items[i] is Oltp.AdgroupRow; i++)
			{
				Oltp.AdgroupRow adgroup = _items[i] as Oltp.AdgroupRow;
				adgroup.Targets.IsActive = !campaignRow.Targets.IsActive;
				adgroup.Targets.OnAllPropertiesChanged();
			}
		}

		private void Targets_TextBlockMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount < 2)
				return;

			TextBlock source = sender as TextBlock;
			source.Visibility = Visibility.Collapsed;
			TextBox target = VisualTree.GetChild<TextBox>(source.Parent);
			target.Visibility = Visibility.Visible;
			target.Focus();

			e.Handled = true;
		}

		private void Targets_TextBoxLostFocus(object sender, RoutedEventArgs e)
		{
			TextBox source = sender as TextBox;
			(source as TextBox).Visibility = Visibility.Collapsed;
			TextBlock target = VisualTree.GetChild<TextBlock>(source.Parent);
			target.Visibility = Visibility.Visible;

			(source.DataContext as IPropertyChangeNotifier).OnAllPropertiesChanged();
		}

		private void Targets_TextBoxKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape || e.Key == Key.Enter)
			{
				Targets_TextBoxLostFocus(sender, new RoutedEventArgs());
				e.Handled = true;
			}
		}
		
		void Targets_BatchOpen(object sender, RoutedEventArgs e)
		{
			List<Oltp.CampaignRow> campaigns = new List<Oltp.CampaignRow>();
			foreach (DataRow row in _listTable.ListView.SelectedItems)
			{
				if (!(row is Oltp.CampaignRow))
				{
					MainWindow.MessageBoxError("Please select campaigns only.", null);
					return;
				}
				else
					campaigns.Add((Oltp.CampaignRow)row);
			}
			if (campaigns.Count < 1)
			{
				MainWindow.MessageBoxError("Please select one or more campaigns.", null);
				return;
			}

			BatchTargets_dialog.IsOpen = true;
		}

		private void Targets_BatchApply(object sender, CancelRoutedEventArgs e)
		{
			// Get target campaigns
			List<Oltp.CampaignRow> campaigns = new List<Oltp.CampaignRow>();
			foreach (DataRow row in _listTable.ListView.SelectedItems)
			{
				if (row is Oltp.CampaignRow)
					campaigns.Add((Oltp.CampaignRow)row);
			}

			NumberValidationRule rule = new NumberValidationRule();
			rule.MinValue = 0;
			rule.AllowEmpty = true;

			ItemsControl dialogItems = VisualTree.GetChild<ItemsControl>(BatchTargets_dialog);
			Dictionary<Measure, double?> values = new Dictionary<Measure, double?>();
			
			// Get measure values and stop for errors
			foreach(Measure measue in _measures)
			{
				TextBox valueTextbox = VisualTree.GetChild<TextBox>(dialogItems.ItemContainerGenerator.ContainerFromItem(measue));
				
				ValidationResult validation =  rule.Validate(valueTextbox.Text, null);
				if (!validation.IsValid)
				{
					MainWindow.MessageBoxError("Please enter valid numbers only.", null);
					e.Cancel = true;
					return;
				}

				values[measue] = rule.GetNumber(valueTextbox.Text);
			}

			// Confirm the operation
			MessageBoxResult result = MessageBox.Show(
				"This will override targets for all selected campaigns. Are you sure?",
				"Warning",
				MessageBoxButton.OKCancel,
				MessageBoxImage.Warning);

			if (result != MessageBoxResult.OK)
			{
				e.Cancel = true;
				return;
			}

			// Apply values
			foreach (Oltp.CampaignRow campaign in campaigns)
			{
				DependencyObject container = (DependencyObject)_listTable.ListView.ItemContainerGenerator.ContainerFromItem(campaign);
				foreach (Measure measure in _measures)
				{
					if (values[measure] == null)
						continue;

					VisualTree.GetChild<TextBox>(container, "measure" + measure.MeasureID.ToString()).Text = values[measure].ToString();
				}
			}
		}
		*/
		#endregion

		/// <summary>
		/// The mechanism for displaying targets in a tab relies on the mechanism for table-wide targets.
		/// This is to leverage the binding capabilities of TargetsRow.
		/// </summary>
		private void CampaignTargets_GotFocus(object sender, RoutedEventArgs e)
		{
			if (_campaignTargetsControl == null)
				_campaignTargetsControl = VisualTree.GetChild<ItemsControl>(Campaign_dialog, "_campaignTargetsControl");

			if (_campaignTargetsControl.ItemsSource != null)
				return;

			TargetsRow targetsRow = null;
			TargetsTable targetsTable = null;
			int accountID = Window.CurrentAccount.ID;
			Oltp.CampaignRow tempCampaign = Campaign_dialog.Content as Oltp.CampaignRow;
			Oltp.CampaignRow targetCampaign = Campaign_dialog.TargetContent as Oltp.CampaignRow;
			bool isBatch = Campaign_dialog.IsBatch;

			Window.AsyncOperation(delegate()
			{
				if (_targetsEnabled) //(_targetData != null) // all that matters is whether they're enabled or not
				{
					// Since targets are enabled, clone the one attached to this campaign and use it as a temp row for dialog editing
					DataTable cloned = _targetData.InnerTable.Clone();
					if (!isBatch)
						cloned.ImportRow(targetCampaign.Targets.InnerRow);
						
					targetsTable = new TargetsTable(cloned);
					targetsRow = targetsTable.GetCampaignTargets(isBatch ? -1 : targetCampaign.GK);
				}
				else
				{
					// Targets on this campaign row are not available, so get them from server
					using (OltpProxy proxy = new OltpProxy())
					{
						// For batch mode, get an empty table
						DataTable t = proxy.Service.CampaignTargets_Get(
							accountID,
							isBatch ? -1 : targetCampaign.GK
						);
						targetsTable = new TargetsTable(t);
					}
					// Get a new row for batch mode
					targetsRow = targetsTable.GetCampaignTargets(isBatch ? -1 : targetCampaign.GK);					
				}
				tempCampaign.Targets = targetsRow;
			},
			delegate()
			{
				List<Measure> measuresTargets = new List<Measure>();

				// Clone the meausres (just in case) and build a list that has a value source
				foreach (Measure m in _measures)
				{
					Measure mt = m.Clone();
					mt.SetValueSource(targetsRow);
					measuresTargets.Add(mt);
				};

				_campaignTargetsControl.ItemsSource = measuresTargets;
				_campaignTargetsControl.UpdateLayout();
				
				List<TextBox> controls = VisualTree.GetChildren<TextBox>(_campaignTargetsControl);
				
				// Wiring for batch checkboxes
				// don't use foreach otherwise 'control' inside 'del' uses 'enumerator.Current' and thus will always reflect the last one
				for (int i = 0; i < controls.Count; i++)
				{
					TextBox control = controls[i];

					StackPanel stackPanel = control.Parent as StackPanel;
					CheckBox checkbox = stackPanel.Children[stackPanel.Children.Count - 1] as CheckBox;
					if (checkbox == null && isBatch)
					{
						checkbox = new CheckBox();
						checkbox.IsChecked = false;
						checkbox.Style = (Style)App.Current.Resources["FormFieldBatchCheckbox"];
						Action<object, RoutedEventArgs> del = delegate(object innerSender, RoutedEventArgs innerE)
						{
							control.IsEnabled = checkbox.IsChecked.Value;
							control.Tag = checkbox.IsEnabled;
						};

						checkbox.Checked += new RoutedEventHandler(del);
						checkbox.Unchecked += new RoutedEventHandler(del);
						stackPanel.Children.Add(checkbox);
					}

					if (checkbox != null)
					{
						control.IsEnabled = !isBatch;
						checkbox.Visibility = isBatch ? Visibility.Visible : Visibility.Collapsed;
					}

				}

				VisualTree.GetChild<TextBlock>(Campaign_dialog, "note").Visibility =
					_targetsEnabled ? Visibility.Visible : Visibility.Collapsed;
			});
		}


	
		/*=========================*/
		#endregion

		#region Campaign dialog
		/*=========================*/

		/// <summary>
		/// Open the dialog
		/// </summary>
		private void Campaign_AddClick(object sender, RoutedEventArgs e)
		{
			// Create an editable new row
			Oltp.CampaignRow editVersion = Dialog_MakeEditVersion<Oltp.CampaignDataTable, Oltp.CampaignRow>(null);
			editVersion.AccountID = this.Window.CurrentAccount.ID;
			editVersion.ChannelID = _channelPicker.SelectedItem is Oltp.ChannelRow ? ((Oltp.ChannelRow)_channelPicker.SelectedItem).ID : 0;

			// Show the dialog
			Campaign_dialog.Title = "New Campaign";
			Campaign_dialog.BeginEdit(editVersion, _campaigns);
		}

		

		/// <summary>
		/// Open the dialog
		/// </summary>
		private void Campaign_dialog_Open(object sender, RoutedEventArgs e)
		{
			Dialog_Open<Oltp.CampaignDataTable, Oltp.CampaignRow>
			(
				// dialog:
				Campaign_dialog,

				// listTable:
				_listTable,

				// clickedItem:
				_listTable.GetParentListViewItem(e.OriginalSource as FrameworkElement) as ListViewItem,

				// allowBatch:
				true,

				// dialogTitle:
				(row, isBatch) => row.Name,

				// dialogTooltip:
				(row, isBatch) => isBatch ? null : "GK #" + row.GK.ToString(),

				// batchFlatten:
				col =>
					col.ColumnName == _campaigns.GKColumn.ColumnName ? null :
					col.ColumnName == _campaigns.NameColumn.ColumnName ? "(multiple campaigns)" as object :
					DBNull.Value as object

			);
		}

		/// <summary>
		/// 
		/// </summary>
		private void Campaign_dialog_ApplyingChanges(object sender, CancelRoutedEventArgs e)
		{
			Oltp.CampaignRow tempRow = (Campaign_dialog.Content as Oltp.CampaignRow);
			Oltp.CampaignRow[] targetRows = Campaign_dialog.IsBatch ?
				Campaign_dialog.TargetContent as Oltp.CampaignRow[] :
				new Oltp.CampaignRow[] { (Oltp.CampaignRow)Campaign_dialog.TargetContent };

			// Check whether this is a new campaign
			if (Window.CurrentAccount.HasBackOffice && tempRow.RowState == DataRowState.Modified && Campaign_dialog.HasBindingsToApply)
			{
				MessageBoxResult result = MessageBox.Show("Applying changes will override all the segments of trackers associated with the selected campaigns. Continue?",
					"Warning",
					MessageBoxButton.OKCancel,
					MessageBoxImage.Warning);

				// Cancel
				if (result != MessageBoxResult.OK)
				{
					e.Cancel = true;
					return;
				}
			}

			// Check which segments have changes for refresh purposes
			bool[] segmentsHaveChanged = new bool[5];
			for (int i = 0; i < segmentsHaveChanged.Length; i++)
			{
				string field = String.Format("Segment{0}", i + 1);
				segmentsHaveChanged[i] = !tempRow[field, DataRowVersion.Current].Equals(tempRow[field, DataRowVersion.Original]);
			}

			// Use the default dialog apply handler
			Dialog_ApplyingChanges<Oltp.CampaignDataTable, Oltp.CampaignRow>(
				_campaigns,
				Campaign_dialog,
				typeof(IOltpLogic).GetMethod("Campaign_Save"),
				e,
				new object[] { Window.CurrentAccount.HasBackOffice },
				true,

				delegate()
				{
					if (e.Cancel)
						return;

					// Update segments of any displayed agroups
					Campaign_dialog_ApplyingChanges_CascadeSegments(
						tempRow,
						targetRows,
						segmentsHaveChanged);

					// Save (or update) targets
					bool isBatch = Campaign_dialog.IsBatch;
					int accountID = Window.CurrentAccount.ID;

					// Get targets to save
					List<Measure> measuresToSave = null;
					if (isBatch && _campaignTargetsControl != null)
					{
						// Check which target measures in batch mode are set to save
						if (isBatch)
						{
							measuresToSave = new List<Measure>();
							List<TextBox> controls = VisualTree.GetChildren<TextBox>(_campaignTargetsControl);

							for (int i = 0; i < controls.Count; i++)
							{
								TextBox control = controls[i];

								StackPanel stackPanel = control.Parent as StackPanel;
								CheckBox checkbox = stackPanel.Children[stackPanel.Children.Count - 1] as CheckBox;

								// Add the target measure to measuresToSave by finding one with the same ID
								// (they are not the same object because we cloned the measures when we opened the tab)
								if (checkbox.IsChecked != null && checkbox.IsChecked.Value)
									measuresToSave.Add(_measures.Single(m => m.MeasureID == (_campaignTargetsControl.Items[i] as Measure).MeasureID));
							}
						}
					}


					if (tempRow.Targets == null ||
						tempRow.Targets.InnerRow == null ||
						tempRow.Targets.InnerRow.RowState == DataRowState.Unchanged ||
						(measuresToSave != null && measuresToSave.Count == 0)
						)
					{
						// No targets to save, just end here
						Campaign_dialog.EndApplyChanges(e);
					}
					else
					{
						// Save targets in async mode
						Window.AsyncOperation(delegate()
						{

							if (_targetsEnabled)
							{

								// Since this row was borrowed from the full target data table, apply changes to it
								foreach(Oltp.CampaignRow targetCampaign in targetRows)
									foreach (Measure m in _measures)
										if (measuresToSave == null || measuresToSave.Contains(m))
											targetCampaign.Targets[m.FieldName] = tempRow.Targets[m.FieldName];
							}
							else
							{
								DataTable targetsDataToSave;
								if (!isBatch)
								{
									targetsDataToSave = tempRow.Targets.InnerRow.Table;
								}
								else
								{
									// For targets in batch mode with targets view disabled, use all campaign targets (slow but who gives a fuck)
									using (OltpProxy proxy = new OltpProxy())
									{
										targetsDataToSave = proxy.Service.CampaignTargets_Get(accountID,null);
									}
									
									TargetsTable tempTargetsTable = new TargetsTable(targetsDataToSave);

									foreach (Oltp.CampaignRow targetCampaign in targetRows)
									{
										TargetsRow targetsRowToSave = tempTargetsTable.GetCampaignTargets(targetCampaign.GK);
										foreach (Measure m in _measures)
											if (measuresToSave == null || measuresToSave.Contains(m))
												targetsRowToSave[m.FieldName] = tempRow.Targets[m.FieldName];
									}
								}

								Targets_Save(false, targetsDataToSave);
							}

						},
						delegate()
						{
							Campaign_dialog.EndApplyChanges(e);
						});
					}

				}
			);
			
		}

		private void Campaign_dialog_ApplyingChanges_CascadeSegments(Oltp.CampaignRow tempCampaign, Oltp.CampaignRow[] targetCampaigns, bool[] segmentsHaveChanged)
		{
			foreach (Oltp.CampaignRow targetCampaign in targetCampaigns)
			{
				// Update any displayed adgroups under this campaign
				int campaignIndex = _items.IndexOf(targetCampaign);
				if (campaignIndex > -1)
				{
					int i = campaignIndex + 1;
					while (i < _items.Count && _items[i] is Oltp.AdgroupRow)
					{
						(_items[i] as Oltp.AdgroupRow).ChannelID = tempCampaign.ChannelID;
						for (int j = 0; j < segmentsHaveChanged.Length; j++)
						{
							string field = String.Format("Segment{0}", j + 1);
							if (segmentsHaveChanged[j])
								_items[i][field] = tempCampaign[field];
						}

						_items[i].AcceptChanges();
						i++;
					}
				}
			}
		}


		/// <summary>
		/// 
		/// </summary>
		private void Campaign_dialog_AppliedChanges(object sender, CancelRoutedEventArgs e)
		{
			// Call the univeral applied change handler
			Dialog_AppliedChanges<Oltp.CampaignRow>(Campaign_dialog, "Editing Campaign", _listTable.ListView, e);
		}

		/// <summary>
		/// 
		/// </summary>
		private void Campaign_dialog_Closing(object sender, CancelRoutedEventArgs e)
		{
			// Cancel if user regrets
			if (!e.Cancel)
				e.Cancel = MainWindow.MessageBoxPromptForCancel(Campaign_dialog);

			if (!e.Cancel)
			{
				_campaigns.RejectChanges();
				//SetValue(CurrentItemHasPpcApiProperty, false);
				
				// Clear the measures source
				if (_campaignTargetsControl != null)
					_campaignTargetsControl.ItemsSource = null;
			}
		}

		private void CampaignSegments_GotFocus(object sender, RoutedEventArgs e)
		{
			if (_campaignTabSegmentsInitialized)
				return;

			foreach(KeyValuePair<Oltp.SegmentRow,Oltp.SegmentValueDataTable> pair in _segmentValueTables)
			{
				// Ignore segments that aren't adgroup related
				if ((pair.Key.AssociationFlags & SegmentAssociationFlags.Campaign) == 0)
					continue;

				StackPanel segmentPanel = VisualTree.GetChild<StackPanel>(Campaign_dialog, "_segment" + pair.Key.SegmentNumber.ToString());
				if (segmentPanel == null)
					continue;

				segmentPanel.Visibility = Visibility.Visible;

				VisualTree.GetChild<Label>(segmentPanel).Content = pair.Key.Name;
				VisualTree.GetChild<ComboBox>(segmentPanel).ItemsSource = pair.Value.Rows;
			}

			_campaignTabSegmentsInitialized = true;
		}

		/*=========================*/
		#endregion

		#region Campaign Merge
		/*=========================*/


		void CampaignMerge_Open(object sender, RoutedEventArgs e)
		{
			if (_listTable.ListView.SelectedItems.Count < 2)
			{
				MainWindow.MessageBoxError("Select two or more campaigns/adgroups to merge.", null);
				return;
			}

			const int campaignMerge = 0;
			const int adgroupMerge = 1;
			const int unspecifiedMerge = -1;
			int mergeType = -1;
			List<DataRow> targetItems = new List<DataRow>();
			
			foreach(object item in _listTable.ListView.SelectedItems)
			{
				if (mergeType == unspecifiedMerge)
				{
					mergeType = item is Oltp.CampaignRow ? campaignMerge : adgroupMerge;
				}
				else if
				(
					(item is Oltp.CampaignRow && mergeType != campaignMerge) ||
					(item is Oltp.AdgroupRow && mergeType != adgroupMerge)
				)
				{
					// Ensure only same types are selected
					MainWindow.MessageBoxError("The selected items are of different types. Select two or more items of the same type (campaign or adgroup).", null);
					return;
				}

				targetItems.Add(item as DataRow);
			}

			if (mergeType == adgroupMerge)
			{
				MainWindow.MessageBoxError("Merging adgroups is not supported in this version.", null);
				return;
			}

			ComboBox _mergeTargetComboBox = VisualTree.GetChild<ComboBox>(CampaignMerge_dialog, "_mergeTargetComboBox");
			_mergeTargetComboBox.ItemsSource = targetItems;
			_mergeTargetComboBox.SelectedIndex = 0;
			CampaignMerge_dialog.IsOpen = true;
			CampaignMerge_dialog.Tag = targetItems;
		}

		void CampaignMerge_Apply(object sender, CancelRoutedEventArgs e)
		{
			MessageBoxResult result = MessageBox.Show(
				"Merged data will take some time to appear in the Intelligence module.\n\nProceed?",
				"Warning",
				MessageBoxButton.OKCancel,
				MessageBoxImage.Warning,
				MessageBoxResult.OK
				);

			if (result != MessageBoxResult.OK)
			{
				e.Cancel = true;
				return;
			}

			ComboBox _mergeTargetComboBox = VisualTree.GetChild<ComboBox>(CampaignMerge_dialog, "_mergeTargetComboBox");
			Oltp.CampaignRow targetCampaign = _mergeTargetComboBox.SelectedItem as Oltp.CampaignRow;
			if (targetCampaign == null)
			{
				e.Cancel = true;
				MainWindow.MessageBoxError("Please select a target campaign.", null);
				return;
			}

			List<DataRow> targetItems = CampaignMerge_dialog.Tag as List<DataRow>;
			long[] otherCampaignGKs = new long[targetItems.Count-1];
			int i = 0;
			// Make an array of all GKs except the selected one
			foreach (DataRow row in targetItems)
			{
				Oltp.CampaignRow campaign = (Oltp.CampaignRow)row;
				if (campaign.GK == targetCampaign.GK)
					continue;
				otherCampaignGKs[i++] = campaign.GK;
			}

			// Start processing in async mode
			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					proxy.Service.Campaign_Merge(Window.CurrentAccount.ID, targetCampaign.GK, otherCampaignGKs);
				}
			},
			delegate(Exception ex)
			{
				e.Cancel = true;
				MainWindow.MessageBoxError("Failed to merge campaigns.", ex);
				return false;
			},
			delegate()
			{
				// Remove deleted rows from items list
				foreach (DataRow row in targetItems)
				{
					Oltp.CampaignRow campaign = (Oltp.CampaignRow)row;
					if (campaign.GK == targetCampaign.GK)
						continue;

					_campaigns.Rows.Remove(row);
					_items.Remove(row);
				}

				_campaigns.AcceptChanges();
				CampaignMerge_dialog.EndApplyChanges(e);

				// Select the target campaign, so we remember it
				_listTable.ListView.SelectedItem = targetCampaign;
			});
		}


		/*=========================*/
		#endregion

		#region Adgroups
		/*=========================*/

		/// <summary>
		/// Add a gateway to an adunit
		/// </summary>
		private void Adgroup_AddClick(object sender, RoutedEventArgs e)
		{
			throw new NotSupportedException("Feature not available.");

			/*
			// Get the parent adunit
			Oltp.AdgroupDataTable tbl = null;
			int selectedIndex = _listTable.ListView.SelectedIndex;
			Oltp.CampaignRow campaign = null;
			if (_items[selectedIndex] is Oltp.AdgroupRow)
			{
				tbl = (_items[selectedIndex] as Oltp.AdgroupRow).Table as Oltp.AdgroupDataTable;
				campaign = GetCampaignItem(_items[selectedIndex] as Oltp.AdgroupRow).Content as Oltp.CampaignRow;
			}
			else
			{
				campaign =_items[selectedIndex] as Oltp.CampaignRow;
				if (selectedIndex < _items.Count-1 && _items[selectedIndex+1] is Oltp.AdgroupRow)
				{
					tbl = (_items[selectedIndex+1] as Oltp.AdgroupRow).Table as Oltp.AdgroupDataTable;
				}
				else
				{
					tbl = new Oltp.AdgroupDataTable();
				}
			}

			// Create an editable new row
			Oltp.AdgroupRow editVersion = Dialog_MakeEditVersion<Oltp.AdgroupDataTable, Oltp.AdgroupRow>(null);
			editVersion.AccountID = this.Window.CurrentAccount.ID;
			editVersion.CampaignGK = campaign.GK;
			editVersion.ChannelID = campaign.ChannelID;

			// Make up values for this crap :: TEMPORARY
			editVersion.CampaignSourceID = 0 - campaign.GK;
			editVersion.AdgroupSourceID = 0 - DateTime.UtcNow.Ticks;

			// Show the dialog
			Adgroup_dialog.Title = "New Adgroup";
			Adgroup_dialog.BeginEdit(editVersion, tbl);

			TabControl tabs = VisualTree.GetChild<TabControl>(Adgroup_dialog);
			if (tabs.SelectedIndex == 1)
			{
				AdgroupKeywords_GotFocus(null, null);
			}
			else if (tabs.SelectedIndex == 2)
			{
				AdgroupCreatives_GotFocus(null, null);
			}
			*/
		}

		/// <summary>
		/// Open the dialog
		/// </summary>
		private void Adgroup_dialog_Open(object sender, RoutedEventArgs e)
		{
			Dialog_Open<Oltp.AdgroupDataTable, Oltp.AdgroupRow>
			(
				// dialog:
				Adgroup_dialog,

				// listTable:
				_listTable,

				// clickedItem:
				_listTable.GetParentListViewItem(e.OriginalSource as FrameworkElement) as ListViewItem,

				// allowBatch:
				true,

				// dialogTitle:
				(row, isBatch) => row.Name,

				// dialogTooltip:
				(row, isBatch) => isBatch ? null : "GK #" + row.GK.ToString(),

				// batchFlatten:
				col =>
					col.ColumnName == new Oltp.AdgroupDataTable().GKColumn.ColumnName ? null :
					col.ColumnName == new Oltp.AdgroupDataTable().NameColumn.ColumnName ? "(multiple adgroups)" as object :
					DBNull.Value as object

			);

			VisualTree.GetChild<TabItem>(Adgroup_dialog, "_tabKeywords").Visibility = Adgroup_dialog.IsBatch ? Visibility.Collapsed : Visibility.Visible;
			VisualTree.GetChild<TabItem>(Adgroup_dialog, "_tabCreatives").Visibility = Adgroup_dialog.IsBatch ? Visibility.Collapsed : Visibility.Visible;

			if (Adgroup_dialog.IsBatch)
				VisualTree.GetChild<TabItem>(Adgroup_dialog, "_tabSegments").Focus();

		}


		private void Adgroup_SetListSource<TableType, RowType>(TableType inputTable, out TableType targetTable, ListTable targetListView)
			where TableType: DataTable
			where RowType: DataRow
		{
			ObservableCollection<RowType> items = new ObservableCollection<RowType>();
			
			bool creativeMode = typeof(RowType) == typeof(Oltp.AdgroupCreativeRow);

			string[] names = new string[5];
			object[] values = new object[5];
			Visibility[] visibility = new Visibility[5];

			if (creativeMode)
			{
				foreach (KeyValuePair<Oltp.SegmentRow, Oltp.SegmentValueDataTable> pair in _segmentValueTables)
				{
					// Ignore segments that aren't adgroup related
					if ((pair.Key.AssociationFlags & SegmentAssociationFlags.AdgroupCreative) != 0)
					{
						names[pair.Key.SegmentNumber-1] = pair.Key.Name;
						values[pair.Key.SegmentNumber-1] = pair.Value.Rows;
						visibility[pair.Key.SegmentNumber-1] = Visibility.Visible;
					}
					else
					{
						visibility[pair.Key.SegmentNumber-1] = Visibility.Collapsed;
					}
				}
			}

			// Bind to the list
			foreach (RowType r in inputTable.Rows)
			{
				items.Add(r);

				// Horrible hack for binding purposes
				if (creativeMode)
				{
					Oltp.AdgroupCreativeRow creative = r as Oltp.AdgroupCreativeRow;
					creative.PageValues = _pagesTable.Rows;
					creative.Segment1Name = names[0];
					creative.Segment2Name = names[1];
					creative.Segment3Name = names[2];
					creative.Segment4Name = names[3];
					creative.Segment5Name = names[4];
					creative.Segment1Values = values[0];
					creative.Segment2Values = values[1];
					creative.Segment3Values = values[2];
					creative.Segment4Values = values[3];
					creative.Segment5Values = values[4];
					creative.Segment1Visibility = visibility[0];
					creative.Segment2Visibility = visibility[1];
					creative.Segment3Visibility = visibility[2];
					creative.Segment4Visibility = visibility[3];
					creative.Segment5Visibility = visibility[4];
				}
			}

			targetTable = inputTable;
			targetListView.ListView.ItemsSource = items;
		}

		/// <summary>
		/// 
		/// </summary>
		private void Adgroup_dialog_ApplyingChanges(object sender, CancelRoutedEventArgs e)
		{
			Oltp.AdgroupKeywordDataTable keywordChanges = null;
			Oltp.AdgroupCreativeDataTable creativeChanges = null;

			// Relevant only in single mode
			if (!Adgroup_dialog.IsBatch)
			{
				keywordChanges = _adgroupKeywordTable == null ? null : _adgroupKeywordTable.GetChanges() as Oltp.AdgroupKeywordDataTable;
				creativeChanges = _adgroupCreativeTable == null ? null : _adgroupCreativeTable.GetChanges() as Oltp.AdgroupCreativeDataTable;

				if (creativeChanges != null)
				{
					MessageBoxResult result = MessageBox.Show("Applying changes will update the segments of all occurences of this adgroup's creatives in other adgroups. Continue?",
						"Warning",
						MessageBoxButton.OKCancel,
						MessageBoxImage.Warning);

					// Cancel
					if (result != MessageBoxResult.OK)
					{
						e.Cancel = true;
						return;
					}
				}
			}
	
			// Call the universal dialog apply changes handler method
			Dialog_ApplyingChanges<Oltp.AdgroupDataTable, Oltp.AdgroupRow>
			(
				// sourceTable: // this is only relevant for add mode, so it doesn't interfere with batch when it's null
				Adgroup_dialog.IsBatch ? null:
					Adgroup_dialog.TargetContent is DataTable ?
						Adgroup_dialog.TargetContent as Oltp.AdgroupDataTable :
						(Adgroup_dialog.TargetContent as Oltp.AdgroupRow).Table as Oltp.AdgroupDataTable,
				
				// dialog:
				Adgroup_dialog,

				// saveMethod:
				typeof(IOltpLogic).GetMethod("Adgroup_Save"),
				
				// e:
				e,

				// additionalArgs:
				new object[]{Window.CurrentAccount.HasBackOffice},
				
				// async:
				true,
				
				// postApplying:
				delegate()
				{
					if (e.Cancel)
						return;

					if (Adgroup_dialog.IsBatch)
					{
						Adgroup_dialog.EndApplyChanges(e);
						return;
					}

					// After the adgroup save method is called, we need to populate the rest of the 
					ObservableCollection<Oltp.AdgroupCreativeRow> sourceCreatives = 
						_adgroupCreatives.ListView.ItemsSource as ObservableCollection<Oltp.AdgroupCreativeRow>;;
					ObservableCollection<Oltp.AdgroupKeywordRow> sourceKeywords =
						_adgroupKeywords.ListView.ItemsSource as ObservableCollection<Oltp.AdgroupKeywordRow>;
					Oltp.KeywordDataTable masterKeywords = null;
					ArrayList newKeywords  = null;

					// Create and add new keywords
					if (keywordChanges != null)
					{
						sourceKeywords = _adgroupKeywords.ListView.ItemsSource as ObservableCollection<Oltp.AdgroupKeywordRow>;
						foreach (Oltp.AdgroupKeywordRow kw in sourceKeywords)
						{
							if (kw.KeywordGK < 0)
							{
								// Null GK means this keyword does not exist, so create it
								if (masterKeywords == null)
								{
									masterKeywords = new Oltp.KeywordDataTable();
									newKeywords = new ArrayList();
								}

								Oltp.KeywordRow masterKw = masterKeywords.NewKeywordRow();
								masterKw.AccountID = Window.CurrentAccount.ID;
								masterKw.IsMonitored = false;
								masterKw.Keyword = kw.Keyword;
								masterKeywords.AddKeywordRow(masterKw);
								newKeywords.Add(kw);
							}

							kw.AdgroupGK = (Adgroup_dialog.TargetContent as Oltp.AdgroupRow).GK;
						}
					}
					
					// Add new creatives
					if (creativeChanges != null)
					{
						foreach (Oltp.AdgroupCreativeRow creative in sourceCreatives)
						{
							creative.AdgroupGK = (Adgroup_dialog.TargetContent as Oltp.AdgroupRow).GK;
						}
					}

					Window.AsyncOperation(delegate()
					{
						using (OltpProxy proxy = new OltpProxy())
						{
							if (keywordChanges != null)
							{
								// Save the new keywords to the master table, then update the GK for the profile keywords
								if (masterKeywords != null)
								{
									masterKeywords = proxy.Service.Keyword_Save(masterKeywords);
									for (int i = 0; i < masterKeywords.Rows.Count; i++)
										(newKeywords[i] as Oltp.AdgroupKeywordRow).KeywordGK = (masterKeywords.Rows[i] as Oltp.KeywordRow).GK;
								}

								proxy.Service.AdgroupKeyword_Save(_adgroupKeywordTable);
							}

							if (creativeChanges != null)
							{
								proxy.Service.AdgroupCreative_Save(_adgroupCreativeTable);
							}
						}

					},
					delegate(Exception ex)
					{
						MainWindow.MessageBoxError("Error occured while saving the adgroup's keywords and/or creatives.", ex);
						e.Cancel = true;
						return false;
					},
					delegate()
					{
						// Accept changes
						if (keywordChanges != null)
							_adgroupKeywordTable.AcceptChanges();

						// Accept changes
						if (creativeChanges != null)
							_adgroupCreativeTable.AcceptChanges();

						Adgroup_dialog.EndApplyChanges(e);
					});
				});
	
		}

		/// <summary>
		/// 
		/// </summary>
		private void Adgroup_dialog_AppliedChanges(object sender, CancelRoutedEventArgs e)
		{
			Oltp.AdgroupRow editVersion = Adgroup_dialog.Content as Oltp.AdgroupRow;
			int minIndex = -1;
			bool replaceTarget = false;

			// If the selected index is a child, that means the row was added correctly so we can quit
			int selectedIndex = _listTable.ListView.SelectedIndex;
			if (_items[selectedIndex] is Oltp.CampaignRow)
			{
				// Get the parent to expand
				ListViewItem parentItem = (ListViewItem)
					_listTable.ListView.ItemContainerGenerator.ContainerFromIndex(selectedIndex);

				// Expand the parent if necessary
				if (selectedIndex >= _items.Count-1 || _items[selectedIndex+1] is Oltp.CampaignRow)
				{
					ToggleButton tb = VisualTree.GetChild<ToggleButton>(parentItem);

					if (tb.IsChecked != true)
					{
						// If it isn't checked check it
						tb.IsChecked = true;
						_listTable.ListView.UpdateLayout();
						replaceTarget = true;
					}
					else
					{
						// Insert right after the selected parent
						minIndex = selectedIndex + 1;
					}
				}
			}
	
			// Call the univeral applied change handler
			Dialog_AppliedChanges<Oltp.AdgroupRow>(Adgroup_dialog, "Editing Adgroup", _listTable.ListView, minIndex, e);

			if (replaceTarget)
			{
				// Since we expanded the parent to show the new child, set it as the new target content of the dialog
				Oltp.AdgroupDataTable childTbl = (_items[selectedIndex+1] as Oltp.AdgroupRow).Table as Oltp.AdgroupDataTable;
				DataRow[] rs = childTbl.Select(String.Format("{0} = {1}", childTbl.GKColumn.ColumnName, editVersion.GK));
				if (rs.Length > 0)
				{
					Oltp.AdgroupRow newTarget = rs[0] as Oltp.AdgroupRow;
					Adgroup_dialog.TargetContent = newTarget;
					_listTable.ListView.SelectedItem = newTarget;
					_listTable.ListView.ScrollIntoView(newTarget);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void Adgroup_dialog_Closing(object sender, CancelRoutedEventArgs e)
		{
			if ((_adgroupKeywordTable != null && _adgroupKeywordTable.GetChanges() != null) ||
				(_adgroupCreativeTable != null && _adgroupCreativeTable.GetChanges() != null))
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
				e.Cancel = MainWindow.MessageBoxPromptForCancel(Adgroup_dialog);

			if (e.Cancel)
				return;

			if (_adgroupKeywords != null)
			{
				_adgroupKeywords.ListView.ItemsSource = null;
				_adgroupKeywordTable = null;
			}
			if (_adgroupCreatives != null)
			{
				_adgroupCreatives.ListView.ItemsSource = null;
				_adgroupCreativeTable = null;
			}
		}


		private void AdgroupKeywords_GotFocus(object sender, RoutedEventArgs e)
		{
			if (_adgroupKeywords.ListView.ItemsSource == null)
			{
				Oltp.AdgroupKeywordDataTable table = null;
				Oltp.AdgroupRow adgroup = Adgroup_dialog.TargetContent as Oltp.AdgroupRow;

				Window.AsyncOperation(delegate()
				{
					if (adgroup != null && adgroup.RowState != DataRowState.Added)
					{
						using (OltpProxy proxy = new OltpProxy())
						{
							table = proxy.Service.AdgroupKeyword_Get(adgroup.GK);
						}
					}
					else
						table = new Oltp.AdgroupKeywordDataTable();
				},
				delegate()
				{
					Adgroup_SetListSource<Oltp.AdgroupKeywordDataTable, Oltp.AdgroupKeywordRow>(table, out _adgroupKeywordTable, _adgroupKeywords);
				});
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void AdgroupKeywords_Add(object sender, RoutedEventArgs e)
		{
			Oltp.KeywordRow keywordRow = _adgroupKeywordPicker.SelectedItem as Oltp.KeywordRow;
			string keywordText;

			// No keyword is selected
			if (keywordRow == null)
			{
				keywordText = _adgroupKeywordPicker.Text;
				if (keywordText.Trim().Length < 1)
					return;

				// Check if it's already been added
				if (_adgroupKeywordTable.Select(String.Format("{0} = '{1}'", _adgroupKeywordTable.KeywordColumn.ColumnName, keywordText.Replace("'", "''"))).Length > 0)
				{
					MessageBox.Show(String.Format("'{0}' has already been added.", keywordText));
					_adgroupKeywordPicker.InnerTextBox.SelectAll();
					_adgroupKeywordPicker.Focus();
					return;
				}

				DataRow[] rs = _adgroupKeywordPickerKeywords.Select(String.Format("{0} = {1} AND {2} = '{3}'",
					_adgroupKeywordPickerKeywords.AccountIDColumn.ColumnName, Window.CurrentAccount.ID,
					_adgroupKeywordPickerKeywords.KeywordColumn.ColumnName, keywordText.Replace("'", "''")));

				if (rs.Length > 0)
				{
					keywordRow = rs[0] as Oltp.KeywordRow;
				}
			}
			else
				keywordText = keywordRow.Keyword;

			Oltp.AdgroupRow adgroup = (Adgroup_dialog.Content as Oltp.AdgroupRow);

			Oltp.AdgroupKeywordRow adgroupKeywordRow = _adgroupKeywordTable.NewAdgroupKeywordRow();
			adgroupKeywordRow.AccountID = Window.CurrentAccount.ID;
			adgroupKeywordRow.KeywordGK = keywordRow != null ? keywordRow.GK : -1;
			adgroupKeywordRow.Keyword = keywordText;
			adgroupKeywordRow.ChannelID = adgroup.ChannelID;
			adgroupKeywordRow.CampaignSourceID = adgroup.CampaignSourceID;
			adgroupKeywordRow.AdgroupSourceID = adgroup.AdgroupSourceID;
			adgroupKeywordRow.KeywordSourceID = (0 - DateTime.UtcNow.Ticks).ToString();
			_adgroupKeywordTable.Rows.Add(adgroupKeywordRow);

			ObservableCollection<Oltp.AdgroupKeywordRow> source = _adgroupKeywords.ListView.ItemsSource as ObservableCollection<Oltp.AdgroupKeywordRow>;
			source.Add(adgroupKeywordRow);

			// Deselect whatever
			_adgroupKeywordPicker.Text = String.Empty;
			_adgroupKeywordPicker.InnerTextBox.SelectAll();
			_adgroupKeywordPicker.Focus();
		}

		private void AdgroupKeywords_BatchInputOpen(object sender, RoutedEventArgs e)
		{
			Button btn = sender as Button;
			DockPanel panel = VisualTree.GetChild<DockPanel>((btn.Parent as FrameworkElement).Parent);

			panel.Visibility = panel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
			btn.Content = panel.Visibility == Visibility.Visible ? "Cancel" : "Batch add";
		}

		private void AdgroupKeywords_BatchInputPerform(object sender, RoutedEventArgs e)
		{
			TextBox batchInput = VisualTree.GetChild<TextBox>(((FrameworkElement)sender).Parent);
			if (batchInput.Text.Trim().Length < 1)
				return;

			string[] phrases = batchInput.Text.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);

			int currentAccountID = Window.CurrentAccount.ID;
			Oltp.KeywordDataTable keywords = null;

			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					keywords = proxy.Service.Keyword_FindForPhrases(currentAccountID, phrases);
				}
			},
			delegate(Exception ex)
			{
				MainWindow.MessageBoxError("Failed to check the keywords on the server.", ex);
				return false;
			},
			delegate()
			{
				Oltp.AdgroupRow adgroup = (Adgroup_dialog.Content as Oltp.AdgroupRow);
				ObservableCollection<Oltp.AdgroupKeywordRow> source = _adgroupKeywords.ListView.ItemsSource as ObservableCollection<Oltp.AdgroupKeywordRow>;

				int tickOffset = 0;
				foreach (string phrase in phrases)
				{
					string keywordText = phrase.Trim();
					DataRow[] rs;

					rs = _adgroupKeywordTable.Select(String.Format("{0} = '{1}'", _adgroupKeywordTable.KeywordColumn.ColumnName, keywordText.Replace("'", "''")));
					if (rs.Length > 0)
						continue;

					rs = keywords.Select(String.Format("{0} = '{1}'", keywords.KeywordColumn.ColumnName, keywordText.Replace("'", "''")));
					Oltp.KeywordRow keywordRow = null;
					if (rs.Length > 0)
						keywordRow = rs[0] as Oltp.KeywordRow;

					Oltp.AdgroupKeywordRow adgroupKeywordRow = _adgroupKeywordTable.NewAdgroupKeywordRow();
					adgroupKeywordRow.AccountID = Window.CurrentAccount.ID;
					adgroupKeywordRow.KeywordGK = keywordRow != null ? keywordRow.GK : -1;
					adgroupKeywordRow.Keyword = keywordRow != null ? keywordRow.Keyword : keywordText;
					adgroupKeywordRow.ChannelID = adgroup.ChannelID;
					adgroupKeywordRow.CampaignSourceID = adgroup.CampaignSourceID;
					adgroupKeywordRow.AdgroupSourceID = adgroup.AdgroupSourceID;
					adgroupKeywordRow.KeywordSourceID = (0 - DateTime.UtcNow.Ticks - (tickOffset++)).ToString();
					_adgroupKeywordTable.Rows.Add(adgroupKeywordRow);
					source.Add(adgroupKeywordRow);
				}

				batchInput.Text = String.Empty;
				AdgroupKeywords_BatchInputOpen(VisualTree.GetChild<Button>(VisualTree.GetParent<Canvas>(batchInput), "_batchAddButton"), new RoutedEventArgs());
			});
		}

		/// <summary>
		/// 
		/// </summary>
		private void AdgroupCreatives_GotFocus(object sender, RoutedEventArgs e)
		{
			if (_adgroupCreatives.ListView.ItemsSource == null)
			{
				Oltp.AdgroupCreativeDataTable table = null;
				Oltp.AdgroupRow adgroup = Adgroup_dialog.TargetContent as Oltp.AdgroupRow;

				Window.AsyncOperation(delegate()
				{
					if (adgroup != null && adgroup.RowState != DataRowState.Added)
					{
						using (OltpProxy proxy = new OltpProxy())
						{
							table = proxy.Service.AdgroupCreative_Get(adgroup.GK);
						}
					}
					else
						table = new Oltp.AdgroupCreativeDataTable();
				},
				delegate()
				{

					if (_adgroupCreativeSegmentCombos != null)
						_adgroupCreativeSegmentCombos.Clear();
					else
						_adgroupCreativeSegmentCombos = new Dictionary<long, List<ComboBox>>();

					Adgroup_SetListSource<Oltp.AdgroupCreativeDataTable, Oltp.AdgroupCreativeRow>(table, out _adgroupCreativeTable, _adgroupCreatives);
				});
			}
		}

		private void AdgroupCreatives_SegmentPickerLoaded(object sender, RoutedEventArgs e)
		{
			ComboBox segmentPicker = (ComboBox) sender;
			if (!segmentPicker.IsVisible)
				return;

			Oltp.AdgroupCreativeRow agCreative = segmentPicker.DataContext as Oltp.AdgroupCreativeRow;

			if (agCreative == null)
				return;

			// Find the list of all combos with the same GK
			List<ComboBox> pickers;
			if (!_adgroupCreativeSegmentCombos.TryGetValue(agCreative.CreativeGK, out pickers))
			{
				// Create a new picker list and add it
				pickers = new List<ComboBox>();
				pickers.Add(segmentPicker);
				_adgroupCreativeSegmentCombos.Add(agCreative.CreativeGK, pickers);

				segmentPicker.SelectionChanged += new SelectionChangedEventHandler(AdgroupCreatives_SegmentPickerChanged);
			}
			else
			{
				// Since the picker already exists, this is not the first segment
				if (!pickers.Contains(segmentPicker))
				{
					pickers.Add(segmentPicker);
					segmentPicker.IsEnabled = false;
					segmentPicker.SelectedIndex = pickers[0].SelectedIndex;
				}
			}
		}

		private void AdgroupCreatives_SegmentPickerChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBox segmentPicker = (ComboBox) sender;
			Oltp.AdgroupCreativeRow agCreative = segmentPicker.DataContext as Oltp.AdgroupCreativeRow;

			if (agCreative == null)
				return;

			List<ComboBox> pickers;
			if (!_adgroupCreativeSegmentCombos.TryGetValue(agCreative.CreativeGK, out pickers))
				return;

			for (int i = 1; i < pickers.Count; i++)
				pickers[i].SelectedIndex = segmentPicker.SelectedIndex;
		}

		/// <summary>
		/// 
		/// </summary>
		private void AdgroupCreatives_Add(object sender, RoutedEventArgs e)
		{
			Oltp.CreativeRow creativeRow = _adgroupCreativePicker.SelectedItem as Oltp.CreativeRow;
			string creativeText;

			// No creative is selected
			if (creativeRow == null)
			{
				//creativeText = _adgroupCreativePicker.Text;

				//// Check if it's already been added
				//if (_adgroupCreativeTable.Select(String.Format("{0} = '{1}'", _adgroupCreativeTable.TitleColumn.ColumnName, creativeText.Replace("'", "''"))).Length > 0)
				//{
				//    MessageBox.Show(String.Format("'{0}' has already been added.", creativeText));
				//    _adgroupCreativePicker.InnerTextBox.SelectAll();
				//    _adgroupCreativePicker.Focus();
				//    return;
				//}

				//DataRow[] rs = _adgroupCreativePickerCreatives.Select(String.Format("{0} = '{1}'", _adgroupCreativePickerCreatives.TitleColumn.ColumnName, creativeText.Replace("'", "''")));
				//if (rs.Length > 0)
				//{
				//    creativeRow = rs[0] as Oltp.CreativeRow;
				//}
				//else
				//{
					MainWindow.MessageBoxError("A creative must be selected from the list (click on one to select).", null);
					return;
				//}
			}
			else
				creativeText = creativeRow.Title;

			Oltp.AdgroupRow adgroup = (Adgroup_dialog.Content as Oltp.AdgroupRow);

			Oltp.AdgroupCreativeRow adgroupCreativeRow = _adgroupCreativeTable.NewAdgroupCreativeRow();
			adgroupCreativeRow.AccountID = Window.CurrentAccount.ID;
			adgroupCreativeRow.CreativeGK = creativeRow.GK;
			adgroupCreativeRow.Title = creativeRow.Title;
			adgroupCreativeRow.Desc1 = creativeRow.Desc1;
			adgroupCreativeRow.Desc2 = creativeRow.Desc2;
			adgroupCreativeRow.ChannelID = adgroup.ChannelID;
			adgroupCreativeRow.CampaignSourceID = adgroup.CampaignSourceID;
			adgroupCreativeRow.AdgroupSourceID = adgroup.AdgroupSourceID;
			adgroupCreativeRow.CreativeSourceID = (0 - DateTime.UtcNow.Ticks).ToString();
			_adgroupCreativeTable.Rows.Add(adgroupCreativeRow);

			ObservableCollection<Oltp.AdgroupCreativeRow> source = _adgroupCreatives.ListView.ItemsSource as ObservableCollection<Oltp.AdgroupCreativeRow>;
			source.Add(adgroupCreativeRow);

			// Deselect whatever
			_adgroupCreativePicker.Text = String.Empty;
			_adgroupCreativePicker.InnerTextBox.SelectAll();
			_adgroupCreativePicker.Focus();
		}

		/// <summary>
		/// Gets the campaign row above a gateway row, representing its parent.
		/// </summary>
		/// <param name="gateway"></param>
		/// <returns></returns>
		ListViewItem GetCampaignItem(Oltp.AdgroupRow adgroup)
		{
			int index = _items.IndexOf(adgroup);

			ListViewItem adunitItem = null;
			while (adunitItem == null)
			{
				DataRow previousItem = _items[index-1];
				adunitItem = previousItem is Oltp.AdgroupRow ?
					null : 
					(ListViewItem) _listTable.ListView.ItemContainerGenerator.ContainerFromItem(previousItem as Oltp.CampaignRow);
			}

			return adunitItem;
		}

		private void _adgroupKeywordPicker_ItemsSourceRequired(object sender, EventArgs e)
		{
			AutocompleteCombo combo = sender as AutocompleteCombo;

			if (combo.Text.Trim().Length < 1)
			{
				_adgroupKeywordPickerKeywords = new Oltp.KeywordDataTable();
				combo.ItemsSource = null;
			}
			else
			{
				int currentAccountID = Window.CurrentAccount.ID;
				string comboText = combo.Text;
				Window.AsyncOperation(delegate()
				{
					using (OltpProxy proxy = new OltpProxy())
					{
						_adgroupKeywordPickerKeywords = proxy.Service.Keyword_Get(currentAccountID, true, comboText + '%', true);
					}
				},
				delegate()
				{
					combo.ItemsSource = _adgroupKeywordPickerKeywords.Rows;
				});
			}
		}

		private void _adgroupCreativePicker_ItemsSourceRequired(object sender, EventArgs e)
		{
			AutocompleteCombo combo = sender as AutocompleteCombo;

			if (combo.Text.Trim().Length < 1)
			{
				_adgroupCreativePickerCreatives = new Oltp.CreativeDataTable();
				combo.ItemsSource = null;
			}
			else
			{
				int currentAccountID = Window.CurrentAccount.ID;
				string comboText = combo.Text;

				Window.AsyncOperation(delegate()
				{
					using (OltpProxy proxy = new OltpProxy())
					{
						_adgroupCreativePickerCreatives = proxy.Service.Creative_Get(currentAccountID, comboText + '%', true);
					}
				},
				delegate()
				{
					combo.ItemsSource = _adgroupCreativePickerCreatives.Rows;
				});
			}
		}

		private void _adgroupCreatives_Loaded(object sender, RoutedEventArgs e)
		{
			_adgroupCreatives = sender as ListTable;
		}

		private void _adgroupKeywords_Loaded(object sender, RoutedEventArgs e)
		{
			_adgroupKeywords = sender as ListTable;
		}

		private void _campaignChannelPicker_Loaded(object sender, RoutedEventArgs e)
		{
			_campaignChannelPicker = sender as ComboBox;
		}

		private void _adgroupKeywordPicker_Loaded(object sender, RoutedEventArgs e)
		{
			_adgroupKeywordPicker = sender as AutocompleteCombo;
		}

		private void _adgroupCreativePicker_Loaded(object sender, RoutedEventArgs e)
		{
			_adgroupCreativePicker = sender as AutocompleteCombo;
		}

		private void AdgroupSegments_GotFocus(object sender, RoutedEventArgs e)
		{
			if (_adgroupTabSegmentsInitialized)
				return;

			foreach (KeyValuePair<Oltp.SegmentRow, Oltp.SegmentValueDataTable> pair in _segmentValueTables)
			{
				// Ignore segments that aren't adgroup related
				if ((pair.Key.AssociationFlags & SegmentAssociationFlags.Adgroup) == 0)
					continue;

				StackPanel segmentPanel = VisualTree.GetChild<StackPanel>(Adgroup_dialog, "_segment" + pair.Key.SegmentNumber.ToString());
				if (segmentPanel == null)
					continue;

				segmentPanel.Visibility = Visibility.Visible;

				VisualTree.GetChild<Label>(segmentPanel).Content = pair.Key.Name;
				VisualTree.GetChild<ComboBox>(segmentPanel).ItemsSource = pair.Value.Rows;
			}

			_adgroupTabSegmentsInitialized = true;
		}

		/*=========================*/
		#endregion
	}
}

namespace Easynet.Edge.UI.Client.PpcCampaignsLocal
{
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
				Oltp.ChannelDataTable chs = (App.CurrentPage as Pages.PpcCampaigns)._channelTable;
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
				Oltp.ChannelDataTable chs = (App.CurrentPage as Pages.PpcCampaigns)._channelTable;
				DataRow[] rs = chs.Select(String.Format("{0} = {1}", chs.DisplayNameColumn.ColumnName, value));
				if (rs.Length > 0)
					return (rs[0] as Oltp.ChannelRow).ID;
			}

			return value;
		}

		#endregion
	}

	[ValueConversion(typeof(int), typeof(string))]
	public class StatusNameConverter: IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is int)
			{
				int statusID = (int) value;
				Oltp.CampaignStatusDataTable chs = (App.CurrentPage as Pages.PpcCampaigns)._campaignStatusTable;
				DataRow[] rs = chs.Select(String.Format("{0} = {1}", chs.IDColumn.ColumnName, statusID));
				if (rs.Length > 0)
					return (rs[0] as Oltp.CampaignStatusRow)[parameter != null ? parameter.ToString() : "Name"];
			}

			return value != null ? value.ToString() : null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is string)
			{
				Oltp.CampaignStatusDataTable chs = (App.CurrentPage as Pages.PpcCampaigns)._campaignStatusTable;
				DataRow[] rs = chs.Select(String.Format("{0} = {1}", chs.NameColumn.ColumnName, value));
				if (rs.Length > 0)
					return (rs[0] as Oltp.CampaignStatusRow).ID;
			}

			return value;
		}

		#endregion
	}


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
	
	/// <summary>
	/// 
	/// </summary>
	public class GroupExpandTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is Oltp.CampaignRow)
				return App.CurrentPage
					.FindResource("GroupExpandTemplate") as DataTemplate;
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
	public class CampaignStatusPickerTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is Oltp.CampaignStatusRow)
				return App.CurrentPage
					.FindResource("StatusPickerTemplate") as DataTemplate;
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
			if (item is Oltp.CampaignRow)
				return App.CurrentPage
					.FindResource("ChannelTemplate") as DataTemplate;
			else
				return new DataTemplate();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class StatusTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is Oltp.CampaignRow)
				return App.CurrentPage
					.FindResource("CampaignStatusTemplate") as DataTemplate;
			else
				return new DataTemplate();
		}
	}

	public class TargetsTemplateSelector : DataTemplateSelector
	{
		DataTemplate _tpl;
		public TargetsTemplateSelector(DataTemplate tpl)
		{
			_tpl = tpl;
		}
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is Oltp.CampaignRow)
				return _tpl;
			else
				return new DataTemplate();
		}
	}

	public class NameTemplateSelector: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is Oltp.CampaignRow)
			{
				return App.CurrentPage
						.FindResource("CampaignNameTemplate") as DataTemplate;
			}
			else if (item is Oltp.AdgroupRow)
			{
				return App.CurrentPage
					.FindResource("AdgroupNameTemplate") as DataTemplate;
			}
			else
				return new DataTemplate();
		}
	}

	public class TargetsSwitchTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is Oltp.CampaignRow)
			{
				return App.CurrentPage
						.FindResource("TargetsSwitchTemplate") as DataTemplate;
			}
			else
				return new DataTemplate();
		}
	}

	public class KeywordPickerTemplateSelector: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is Oltp.KeywordRow)
			{
				Oltp.KeywordRow kw = (Oltp.KeywordRow) item;
				if (kw.AccountID == App.CurrentPage.Window.CurrentAccount.ID)
					return App.CurrentPage
							.FindResource("SelfKeywordTemplate") as DataTemplate;
				else
					return App.CurrentPage
							.FindResource("RelatedKeywordTemplate") as DataTemplate;
			}
			else
				return new DataTemplate();
		}
	}
}