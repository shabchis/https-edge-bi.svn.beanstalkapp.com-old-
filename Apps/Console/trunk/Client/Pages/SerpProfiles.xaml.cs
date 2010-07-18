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

namespace Easynet.Edge.UI.Client.Pages
{
	/// <summary>
	/// Interaction logic for SerpProfiles.xaml
	/// </summary>
	[AccountDependentPage]
	public partial class SerpProfiles: PageBase
	{
		#region Fields
		/*=========================*/

		ObservableCollection<DataRow> _items;
		Oltp.SerpProfileDataTable _profiles;
		
		AutocompleteCombo _keywordPicker;
		Oltp.KeywordDataTable _keywordPickerKeywords;
		Oltp.SerpProfileKeywordDataTable _profileKeywordsTable;
		ListTable _profileKeywordsListView;
		
		TextBox _domainTextbox;
		Oltp.SerpProfileDomainGroupDataTable _profileDomainGroupsTable;
		Oltp.SerpProfileDomainDataTable _profileDomainsTable;
		ListTable _profileDomainsListView;

		ComboBox _searchEnginePicker;
		Oltp.SearchEngineDataTable _searchEnginePickerEngines;
		ObservableCollection<Oltp.SearchEngineRow> _searchEnginePickerItems;
		Oltp.SerpProfileSearchEngineDataTable _profileSearchEnginesTable;
		ListTable _profileSearchEnginesListView;

		ComboBox _comboScheduling;
		Grid _monthCalendar;
		StackPanel _weekDay;

		/*=========================*/
		#endregion

		#region Constructors
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		public SerpProfiles()
		{
			InitializeComponent();

			this.Loaded += new RoutedEventHandler(Page_Loaded);
		}

		void Page_Loaded(object sender, RoutedEventArgs e)
		{
			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					_searchEnginePickerEngines = proxy.Service.SearchEngine_Get();
				}
			});
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
			GetProfiles(Window.CurrentAccount, null, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="include"></param>
		private void GetProfiles(Oltp.AccountRow account, string filter, bool include)
		{
			Oltp.AccountRow currentAccount = account == null ? this.Window.CurrentAccount : account;

			if (currentAccount == null)
				return;

			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					_profiles = proxy.Service.SerpProfile_Get(currentAccount.ID, Oltp.SerpProfileType.SEO);
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
				foreach (DataRow r in _profiles.Rows)
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
		private void Profile_Add(object sender, RoutedEventArgs e)
		{
			// Create an editable new ro
			Oltp.SerpProfileRow editVersion = Dialog_MakeEditVersion<Oltp.SerpProfileDataTable, Oltp.SerpProfileRow>(_profiles, null);
			editVersion.AccountID = this.Window.CurrentAccount.ID;
			editVersion.ProfileType = Oltp.SerpProfileType.SEO;

			// Show the dialog
			Profile_dialog.Title = "New Profile";
			Profile_dialog.BeginEdit(editVersion, _profiles);
		}

		private void Profile_RunNow(object sender, RoutedEventArgs e)
		{
			ListViewItem currentItem = _listTable.GetParentListViewItem(e.OriginalSource as FrameworkElement);
			Oltp.SerpProfileRow row = currentItem.Content as Oltp.SerpProfileRow;

			MessageBoxResult result = MessageBox.Show(
				String.Format("Run the profile \"{0}\" now?\n\nWarning: if you run a profile more than once per day conflicting results may be displayed.", row.Name),
				"Warning",
				MessageBoxButton.OKCancel,
				MessageBoxImage.Exclamation);

			if (result != MessageBoxResult.OK)
				return;

			int accountID = Window.CurrentAccount.ID;
			bool canRun = false;
			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{

					canRun = proxy.Service.SerpProfile_CanRunNow(accountID, row.ID);

				}
			},
			delegate(Exception ex)
			{
				MessageBoxError("Failed to check if this profile can be run now.", ex);
				return false;
			},
			delegate()
			{
				if (!canRun)
				{
					MessageBoxError("The profile has already been run today and can not be run again.", null);
				}
				else
				{
					Window.AsyncOperation(delegate()
					{
						using (OltpProxy proxy = new OltpProxy())
						{
							proxy.Service.SerpProfile_RunNow(Window.CurrentAccount.ID, row.ID);
						}
					},
					delegate(Exception ex)
					{
						MessageBoxError("Failed to schedule a profile run.", ex);
						return false;
					},
					delegate()
					{
						MessageBox.Show("The profile has been scheduled for a run. Check the Rankings Browser within an hour for the results.",
							"Information",
							MessageBoxButton.OK,
							MessageBoxImage.Information);
					});
				}
			});
		}

		/// <summary>
		/// Open the dialog
		/// </summary>
		private void Profile_dialog_Open(object sender, RoutedEventArgs e)
		{
			// Set dataItem as current item
			ListViewItem currentItem = _listTable.GetParentListViewItem(e.OriginalSource as FrameworkElement);
			Oltp.SerpProfileRow row = currentItem.Content as Oltp.SerpProfileRow;

			// Show the dialog
			Profile_dialog.Title = row.Name;
			Profile_dialog.TitleTooltip = row.ID.ToString();

			Profile_dialog.BeginEdit(
				Dialog_MakeEditVersion<Oltp.SerpProfileDataTable, Oltp.SerpProfileRow>(_profiles, row),
				row
			);

			TabControl tabs = Visual.GetDescendant<TabControl>(Profile_dialog);
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

		private bool IsMissingData(bool isNew, params DataTable[] tables)
		{
			bool missingData = false;
			
			foreach(DataTable tbl in tables)
			{
				if (isNew)
				{
					missingData |= tbl == null || tbl.Rows.Count == 0;
				}
				else
				{
					missingData |=
						tbl != null &&
						(
							tbl.Rows.Count == 0 ||
							(
								tbl.GetChanges(DataRowState.Deleted) != null &&
								tbl.GetChanges(DataRowState.Deleted).Rows.Count == tbl.Rows.Count
							)
						);
				}
			}

			return missingData;
		}

		/// <summary>
		/// 
		/// </summary>
		private void Profile_dialog_ApplyingChanges(object sender, CancelRoutedEventArgs e)
		{
			Oltp.SerpProfileRow editVersion = Profile_dialog.Content as Oltp.SerpProfileRow;
			bool isNew = editVersion.RowState == DataRowState.Added;

			if (IsMissingData(isNew, _profileKeywordsTable, _profileDomainsTable, _profileSearchEnginesTable))
			{
				MessageBoxError("At least one keyword, one domain filter, and one search engine is required for a profile.", null);
				e.Cancel = true;
				return;
			}

			Dialog_ApplyingChanges<Oltp.SerpProfileDataTable, Oltp.SerpProfileRow>(
				_profiles,
				Profile_dialog,
				typeof(IOltpLogic).GetMethod("SerpProfile_Save"),
				e,
				null,
				true,
				delegate()
			{

				if (e.Cancel)
					return;

				bool keywordsNull = (_profileKeywordsTable == null || _profileKeywordsTable.GetChanges() == null);
				bool domainGroupsNull = (_profileDomainGroupsTable == null || _profileDomainGroupsTable.GetChanges() == null);
				bool domainsNull = (_profileDomainsTable == null || _profileDomainsTable.GetChanges() == null);
				bool searchEnginesNull = (_profileSearchEnginesTable == null || _profileSearchEnginesTable.GetChanges() == null);

				if (keywordsNull && domainGroupsNull && domainsNull && searchEnginesNull)
				{
					Profile_dialog.EndApplyChanges(e);
					return;
				}

				// For keyword saving
				Oltp.KeywordDataTable masterKeywords = null;
				ArrayList newKeywords = null;

				// Keywords
				if (!keywordsNull)
				{
					ObservableCollection<Oltp.SerpProfileKeywordRow> source =
					_profileKeywordsListView.ListView.ItemsSource as ObservableCollection<Oltp.SerpProfileKeywordRow>;

					for (int i = 0; i < source.Count; i++)
					{
						Oltp.SerpProfileKeywordRow kw = source[i];
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

						kw.DisplayPosition = i;
						kw.ProfileID = (Profile_dialog.TargetContent as Oltp.SerpProfileRow).ID;
					}
				}

				// Domain groups
				Dictionary<int, int> groupPositions = null;
				if (!domainGroupsNull)
				{
					groupPositions = new Dictionary<int, int>();

					ObservableCollection<DataRow> source =
						_profileDomainsListView.ListView.ItemsSource as ObservableCollection<DataRow>;

					int i = 0;
					foreach (DataRow r in source)
					{
						if (r is Oltp.SerpProfileDomainGroupRow)
						{
							groupPositions.Add(-1 - _profileDomainGroupsTable.Rows.IndexOf(r), i);
							(r as Oltp.SerpProfileDomainGroupRow).DisplayPosition = i;
							(r as Oltp.SerpProfileDomainGroupRow).ProfileID = (Profile_dialog.TargetContent as Oltp.SerpProfileRow).ID;
							i++;
						}
					}
				}

				// Domains
				if (!domainsNull)
				{
					foreach (Oltp.SerpProfileDomainRow domain in _profileDomainsTable.Rows)
					{
						// Ignore deleted rows
						if (domain.RowState == DataRowState.Deleted || domain.RowState == DataRowState.Detached)
							continue;

						domain.ProfileID = (Profile_dialog.TargetContent as Oltp.SerpProfileRow).ID;
						if (domain.GroupID < 0)
						{
							domain.GroupID =
								(_profileDomainGroupsTable.Rows[groupPositions[domain.GroupID]] as Oltp.SerpProfileDomainGroupRow).ID;
						}
					}
				}

				// Search engines
				if (!searchEnginesNull)
				{
					ObservableCollection<Oltp.SerpProfileSearchEngineRow> source =
						_profileSearchEnginesListView.ListView.ItemsSource as ObservableCollection<Oltp.SerpProfileSearchEngineRow>;

					for (int i = 0; i < source.Count; i++)
					{
						source[i].DisplayPosition = i;
						source[i].ProfileID = (Profile_dialog.TargetContent as Oltp.SerpProfileRow).ID;
					}
				}

				// Save -- async
				Window.AsyncOperation(delegate()
				{
					using (OltpProxy proxy = new OltpProxy())
					{
						// Keywords
						if (!keywordsNull)
						{
							if (masterKeywords != null)
							{
								masterKeywords = proxy.Service.Keyword_Save(masterKeywords);
								for (int i = 0; i < masterKeywords.Rows.Count; i++)
									(newKeywords[i] as Oltp.SerpProfileKeywordRow).KeywordGK = (masterKeywords.Rows[i] as Oltp.KeywordRow).GK;
							}

							proxy.Service.SerpProfileKeyword_Save(_profileKeywordsTable);
						}

						// Domain groups
						if (!domainGroupsNull)
							_profileDomainGroupsTable = proxy.Service.SerpProfileDomainGroup_Save(_profileDomainGroupsTable);

						// Domains
						if (!domainsNull)
							_profileDomainsTable = proxy.Service.SerpProfileDomain_Save(_profileDomainsTable);

						// Search engines
						if (!searchEnginesNull)
							_profileSearchEnginesTable = proxy.Service.SerpProfileSearchEngine_Save(_profileSearchEnginesTable);
					}
				},
				delegate(Exception ex)
				{
					MessageBoxError("Error while saving profile settings.", ex);
					e.Cancel = true;
					return false;
				},
				delegate()
				{
					if (!keywordsNull)
						_profileKeywordsTable.AcceptChanges();

					if (!searchEnginesNull)
						_profileSearchEnginesTable.AcceptChanges();

					if (!domainGroupsNull)
						SetListSource<DataRow>(_profileDomainGroupsTable, _profileDomainsListView.ListView);

					// Complete the apply process
					Profile_dialog.EndApplyChanges(e);
				});
			});
		}

		/// <summary>
		/// 
		/// </summary>
		private void Profile_dialog_AppliedChanges(object sender, CancelRoutedEventArgs e)
		{
			Oltp.SerpProfileRow dataItem = Profile_dialog.TargetContent as Oltp.SerpProfileRow;

			// Call the univeral applied change handler
			Dialog_AppliedChanges<Oltp.SerpProfileRow>(Profile_dialog, dataItem.Name, _listTable._listView, e);
		}

		/// <summary>
		/// 
		/// </summary>
		private void Profile_dialog_Closing(object sender, CancelRoutedEventArgs e)
		{
			if ((_profileKeywordsTable != null && _profileKeywordsTable.GetChanges() != null) ||
				(_profileDomainsTable != null && _profileDomainsTable.GetChanges() != null))
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
				e.Cancel = MessageBoxPromptForCancel(Profile_dialog);

			if (e.Cancel)
				return;

			_profiles.RejectChanges();
			_profileKeywordsTable = null;
			_profileKeywordsListView.ListView.ItemsSource = null;
			_profileDomainGroupsTable = null;
			_profileDomainsTable = null;
			_profileDomainsListView.ListView.ItemsSource = null;
			_profileSearchEnginesTable = null;
			_profileSearchEnginesListView.ListView.ItemsSource = null;
			_searchEnginePicker.ItemsSource = null;
			_searchEnginePickerItems = null;
		}

		/*=========================*/
		#endregion

		/// <summary>
		/// 
		/// </summary>
		private void SetListSource<RowType>(DataTable inputTable, ListView targetListView)
			where RowType: DataRow
		{
			ObservableCollection<RowType> items = new ObservableCollection<RowType>();

			// Bind to the list
			foreach (RowType r in inputTable.Rows)
				items.Add(r);

			targetListView.ItemsSource = items;
		}


		#region Keywords
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void KeywordsTab_GotFocus(object sender, RoutedEventArgs e)
		{
			if (_profileKeywordsListView.ListView.ItemsSource == null)
			{
				Oltp.SerpProfileRow profile = (Profile_dialog.Content as Oltp.SerpProfileRow);
				Window.AsyncOperation(delegate()
				{
					if (profile.RowState != DataRowState.Added)
					{
						using (OltpProxy proxy = new OltpProxy())
						{
							_profileKeywordsTable = proxy.Service.SerpProfileKeyword_Get(profile.ID);
						}
					}
					else
						_profileKeywordsTable = new Oltp.SerpProfileKeywordDataTable();
				},
				delegate()
				{

					SetListSource<Oltp.SerpProfileKeywordRow>(_profileKeywordsTable, _profileKeywordsListView.ListView);
				});
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		private void _keywordPicker_ItemsSourceRequired(object sender, EventArgs e)
		{
			AutocompleteCombo combo = sender as AutocompleteCombo;

			if (combo.Text.Trim().Length < 1)
			{
				_keywordPickerKeywords = new Oltp.KeywordDataTable();
				combo.ItemsSource = null;
			}
			else
			{
				int accountID = Window.CurrentAccount.ID;
				string comboText = combo.Text;
				Window.AsyncOperation(delegate()
				{
					using (OltpProxy proxy = new OltpProxy())
					{
						_keywordPickerKeywords = proxy.Service.Keyword_Get(accountID, true, comboText + '%', true);
					}
				},
				delegate()
				{
					combo.ItemsSource = _keywordPickerKeywords.Rows;
				});
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void _profileKeywordsListView_Loaded(object sender, RoutedEventArgs e)
		{
			_profileKeywordsListView = sender as ListTable;
			_profileKeywordsListView.ChildItemTypes = new Type[] {typeof(Oltp.SerpProfileDomainRow)};
		}

		private void KeywordsTab_AddKeyword(object sender, RoutedEventArgs e)
		{
			Oltp.KeywordRow keywordRow = _keywordPicker.SelectedItem as Oltp.KeywordRow;
			string keywordText;

			// No keyword is selected
			if (keywordRow == null)
			{
				keywordText = _keywordPicker.Text;

				// Check if it's already been added
				if (_profileKeywordsTable.Select(String.Format("{0} = '{1}'", _profileKeywordsTable.KeywordColumn.ColumnName, keywordText.Replace("'", "''"))).Length > 0)
				{
					MessageBox.Show(String.Format("'{0}' has already been added.", keywordText));
					_keywordPicker.InnerTextBox.SelectAll();
					_keywordPicker.Focus();
					return;
				}

				DataRow[] rs = _keywordPickerKeywords.Select(String.Format("{0} = {1} AND {2} = '{3}'",
					_keywordPickerKeywords.AccountIDColumn.ColumnName, Window.CurrentAccount.ID,
					_keywordPickerKeywords.KeywordColumn.ColumnName, keywordText.Replace("'", "''")));

				if (rs.Length > 0)
				{
					keywordRow = rs[0] as Oltp.KeywordRow;
				}
			}
			else
				keywordText = keywordRow.Keyword;

			Oltp.SerpProfileKeywordRow profileKeywordRow = _profileKeywordsTable.NewSerpProfileKeywordRow();
			profileKeywordRow.AccountID = Window.CurrentAccount.ID;
			profileKeywordRow.KeywordGK = keywordRow != null ? keywordRow.GK : -1;
			profileKeywordRow.Keyword = keywordText;
			_profileKeywordsTable.Rows.Add(profileKeywordRow);

			ObservableCollection<Oltp.SerpProfileKeywordRow> source = _profileKeywordsListView.ListView.ItemsSource as ObservableCollection<Oltp.SerpProfileKeywordRow>;
			source.Add(profileKeywordRow);

			// Deselect whatever
			_keywordPicker.Text = String.Empty;
			_keywordPicker.InnerTextBox.SelectAll();
			_keywordPicker.Focus();
		}

		private void _keywordPicker_Loaded(object sender, RoutedEventArgs e)
		{
			_keywordPicker = sender as AutocompleteCombo;
		}

		private void _profileKeywordsListView_ItemDragged(object sender, ItemDraggedRoutedEventArgs e)
		{
			ObservableCollection<Oltp.SerpProfileKeywordRow> sourceCollection = _profileKeywordsListView.ListView.ItemsSource as ObservableCollection<Oltp.SerpProfileKeywordRow>;
			Oltp.SerpProfileKeywordRow kw = sourceCollection[e.SourceIndex];
			int indexToInsert = e.SourceIndex < e.TargetIndex ? e.TargetIndex-1 : e.TargetIndex;

			// Insert
			sourceCollection.RemoveAt(e.SourceIndex);
			sourceCollection.Insert(indexToInsert, kw);
			_profileKeywordsListView.ListView.SelectedIndex = indexToInsert;
			kw.DisplayPosition = indexToInsert;
		}

		private void KeywordsTab_RemoveKeyword(object sender, RoutedEventArgs e)
		{
			ObservableCollection<Oltp.SerpProfileKeywordRow> sourceCollection = _profileKeywordsListView.ListView.ItemsSource as ObservableCollection<Oltp.SerpProfileKeywordRow>;
			foreach (Oltp.SerpProfileKeywordRow kw in new ArrayList(_profileKeywordsListView.ListView.SelectedItems))
			{
				sourceCollection.Remove(kw);
				kw.Delete();
			}
		}

		private void KeywordsTab_OpenBatch(object sender, RoutedEventArgs e)
		{
			Grid batchKeywordsDialog = Visual.GetDescendant<Grid>(Profile_dialog, "_batchKeywordsDialog");
			if (batchKeywordsDialog == null)
				return;

			batchKeywordsDialog.Visibility = Visibility.Visible;

			TextBox batchKeywordsText = Visual.GetDescendant<TextBox>(batchKeywordsDialog, "_batchKeywordsText");
			if (batchKeywordsText == null)
				return;
			batchKeywordsText.Focus();
			batchKeywordsText.SelectAll();
		}

		private void KeywordsTab_CloseBatch(object sender, RoutedEventArgs e)
		{
			Grid batchKeywordsDialog = Visual.GetDescendant<Grid>(Profile_dialog, "_batchKeywordsDialog");
			if (batchKeywordsDialog == null)
				return;

			batchKeywordsDialog.Visibility = Visibility.Collapsed;
		}

		private void KeywordsTab_AddBatch(object sender, RoutedEventArgs e)
		{
			Grid batchKeywordsDialog = Visual.GetDescendant<Grid>(Profile_dialog, "_batchKeywordsDialog");
			if (batchKeywordsDialog == null)
				return;

			TextBox batchKeywordsText = Visual.GetDescendant<TextBox>(batchKeywordsDialog, "_batchKeywordsText");
			if (batchKeywordsText == null)
				return;
			
			// Get the delimeter
			string rawText = batchKeywordsText.Text;
			string delimeter = rawText.IndexOf("\r\n") >= 0 ? "\r\n" : ",";

			// Parse the text into keyword values
			string[] words = rawText.Split(new string[] {delimeter}, StringSplitOptions.RemoveEmptyEntries);
			int totalAdded = 0;
			
			int accountID = Window.CurrentAccount.ID;
			List<Oltp.SerpProfileKeywordRow> addedRows = new List<Oltp.SerpProfileKeywordRow>();

			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					foreach (string word in words)
					{
						string val = word.Trim();
						Oltp.KeywordDataTable kws;
						try
						{
							kws = proxy.Service.Keyword_Get(accountID, false, val, true);
						}
						catch
						{
							// Ignore errors
							continue;
						}

						totalAdded++;

						// Check if it's already been added
						if (_profileKeywordsTable.Select(String.Format("{0} = '{1}'", _profileKeywordsTable.KeywordColumn.ColumnName, val.Replace("'", "''"))).Length > 0)
							continue;

						Oltp.KeywordRow keywordRow = null;
						if (kws.Rows.Count > 0)
							keywordRow = kws.Rows[0] as Oltp.KeywordRow;

						Oltp.SerpProfileKeywordRow profileKeywordRow = _profileKeywordsTable.NewSerpProfileKeywordRow();
						profileKeywordRow.AccountID = accountID;
						profileKeywordRow.KeywordGK = keywordRow != null ? keywordRow.GK : -1;
						profileKeywordRow.Keyword = val;
						_profileKeywordsTable.Rows.Add(profileKeywordRow);

						addedRows.Add(profileKeywordRow);
					}
				}
			},
			delegate()
			{
				ObservableCollection<Oltp.SerpProfileKeywordRow> source = _profileKeywordsListView.ListView.ItemsSource as ObservableCollection<Oltp.SerpProfileKeywordRow>;
				foreach (Oltp.SerpProfileKeywordRow profileKeywordRow in addedRows)
					source.Add(profileKeywordRow);
				
				batchKeywordsDialog.Visibility = Visibility.Collapsed;

				if (totalAdded < words.Length)
					MessageBox.Show(String.Format("{0} keywords out of {1} could not be added.", totalAdded, words.Length), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
			});
		}

		private void KeywordsTab_TextBlockMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount < 2)
				return;

			TextBlock source = sender as TextBlock;
			source.Visibility = Visibility.Collapsed;
			TextBox target = Visual.GetDescendant<TextBox>(source.Parent);
			target.Visibility = Visibility.Visible;
			target.Focus();

			e.Handled = true;
		}

		private void KeywordsTab_TextBoxLostFocus(object sender, RoutedEventArgs e)
		{
			TextBox source = sender as TextBox;
			(source as TextBox).Visibility = Visibility.Collapsed;
			TextBlock target = Visual.GetDescendant<TextBlock>(source.Parent);
			target.Visibility = Visibility.Visible;

			(source.DataContext as IPropertyChangeNotifier).OnAllPropertiesChanged();
		}


		private void KeywordsTab_TextBoxKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape || e.Key == Key.Enter)
			{
				KeywordsTab_TextBoxLostFocus(sender, new RoutedEventArgs());
				e.Handled = true;
			}
		}

		/*=========================*/
		#endregion

		#region Domains
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		private void DomainsTab_GotFocus(object sender, RoutedEventArgs e)
		{
			if (_profileDomainsListView.ListView.ItemsSource == null)
			{
				Oltp.SerpProfileRow profile = Profile_dialog.Content as Oltp.SerpProfileRow;
				Window.AsyncOperation(delegate()
				{
					if (profile.RowState != DataRowState.Added)
					{
						using (OltpProxy proxy = new OltpProxy())
						{
							_profileDomainGroupsTable = proxy.Service.SerpProfileDomainGroup_Get(profile.ID);
							_profileDomainsTable = proxy.Service.SerpProfileDomain_Get(profile.ID);
						}
					}
					else
					{
						_profileDomainGroupsTable = new Oltp.SerpProfileDomainGroupDataTable();
						_profileDomainsTable = new Oltp.SerpProfileDomainDataTable();
					}
				},
				delegate()
				{

					SetListSource<DataRow>(_profileDomainGroupsTable, _profileDomainsListView.ListView);
				});
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void DomainsTab_AddGroup(object sender, RoutedEventArgs e)
		{
			ObservableCollection<DataRow> source = _profileDomainsListView.ListView.ItemsSource as ObservableCollection<DataRow>;

			Oltp.SerpProfileDomainGroupRow newGroup =  _profileDomainGroupsTable.NewSerpProfileDomainGroupRow();
			newGroup.Name = "Domain #" + _profileDomainGroupsTable.Rows.Count;
			_profileDomainGroupsTable.Rows.Add(newGroup);
			source.Add(newGroup);
		}
	
		/// <summary>
		/// 
		/// </summary>
		private void DomainsTab_AddDomain(object sender, RoutedEventArgs e)
		{
			if (_domainTextbox.Text.Trim().Length < 1)
				return;

			//if (!Uri.IsWellFormedUriString(_domainTextbox.Text.Trim(), UriKind.Absolute))
			//{
			//	MessageBoxError("Domain must be a proper URL including http: or https:", null);
			//	return;
			//}

			if (_profileDomainsTable.Select(String.Format("{0} = '{1}'",
				_profileDomainsTable.DomainColumn.ColumnName,
				_domainTextbox.Text.Trim().Replace("'", "''"))).Length > 0)
			{
				MessageBoxError("This filter is already used in this profile.", null);
				return;
			}

			ObservableCollection<DataRow> source = _profileDomainsListView.ListView.ItemsSource as ObservableCollection<DataRow>;

			// If a group is selected, use it as parent; if a domain row is selected, find its parent
			Oltp.SerpProfileDomainGroupRow group = DomainsTab_GetGroupItem(_profileDomainsListView.ListView.SelectedIndex);

			if (group == null)
			{
				DomainsTab_AddGroup(null, new RoutedEventArgs());
				_profileDomainsListView.ListView.UpdateLayout();

				if (source.Count < 1)
					return;
				else
					group = source[0] as Oltp.SerpProfileDomainGroupRow;
			}

			// Expand if necessary
			ToggleButton expander = Visual.GetDescendant<ToggleButton>(_profileDomainsListView.ListView.ItemContainerGenerator.ContainerFromItem(group));
			if (expander.IsChecked != true)
				expander.IsChecked = true;

			Oltp.SerpProfileDomainRow profileDomainRow = _profileDomainsTable.NewSerpProfileDomainRow();
			profileDomainRow.AccountID = Window.CurrentAccount.ID;
			profileDomainRow.Domain = _domainTextbox.Text;
			// Group ID - use negative added index for the meantime till we save the new group
			profileDomainRow.GroupID = group.IsIDNull() ? -1 - group.Table.Rows.IndexOf(group) : group.ID;

			_profileDomainsTable.Rows.Add(profileDomainRow);

			// Insert into new location - index of group + 1 + number of existing domains 
			int insertIndex = source.IndexOf(group) + _profileDomainsTable.Select(String.Format("{0} = {1}",
				_profileDomainsTable.GroupIDColumn.ColumnName,
				profileDomainRow.GroupID)).Length;

			source.Insert(insertIndex, profileDomainRow);
			_profileDomainsListView.ListView.SelectedIndex = insertIndex;

			// Deselect whatever
			_domainTextbox.Text = String.Empty;
			_domainTextbox.Focus();
		}

		private void DomainsTab_Remove(object sender, RoutedEventArgs e)
		{
			ObservableCollection<DataRow> sourceCollection = _profileDomainsListView.ListView.ItemsSource as ObservableCollection<DataRow>;
			DataRow row = (sender as FrameworkElement).DataContext as DataRow;

			// If deleting a group
			if (row is Oltp.SerpProfileDomainGroupRow)
			{
				// Collapse the toggle button to get rid of the visible items
				ListViewItem item = (ListViewItem) _profileDomainsListView.ListView.ItemContainerGenerator.ContainerFromItem(row);
				Visual.GetDescendant<ToggleButton>(item).IsChecked = false;

				// Get rid of the rows from the domain table
				Oltp.SerpProfileDomainRow[] rowsToDelete = (Oltp.SerpProfileDomainRow[])
					_profileDomainsTable.Select(
						String.Format("{0} = {1}", _profileDomainsTable.GroupIDColumn.ColumnName,
							((Oltp.SerpProfileDomainGroupRow) row).IsIDNull() ? 
							-1 - _profileDomainGroupsTable.Rows.IndexOf(row) : 
							((Oltp.SerpProfileDomainGroupRow)row).ID
							));

				foreach (Oltp.SerpProfileDomainRow domain in rowsToDelete)
					domain.Delete();
			}

			sourceCollection.Remove(row);
			row.Delete();
		}

		private void DomainsTab_Reorder(object sender, ItemDraggedRoutedEventArgs e)
		{
			ObservableCollection<DataRow> sourceCollection = _profileDomainsListView.ListView.ItemsSource as ObservableCollection<DataRow>;
			Oltp.SerpProfileDomainGroupRow group = sourceCollection[e.SourceIndex] as Oltp.SerpProfileDomainGroupRow;
			int indexToInsert = e.SourceIndex < e.TargetIndex ? e.TargetIndex-1 : e.TargetIndex;

			Oltp.SerpProfileDomainGroupRow targetGroup = DomainsTab_GetGroupItem(indexToInsert);
			ToggleButton targetExpander = Visual.GetDescendant<ToggleButton>(_profileDomainsListView.ListView.ItemContainerGenerator.ContainerFromItem(targetGroup));
			if (targetExpander.IsChecked == true)
			{
				if (indexToInsert < sourceCollection.IndexOf(targetGroup) + DomainsTab_GetGroupChildren(targetGroup).Length + 1)
					return;
			}

			ToggleButton sourceExpander = Visual.GetDescendant<ToggleButton>(_profileDomainsListView.ListView.ItemContainerGenerator.ContainerFromItem(group));
			if (sourceExpander.IsChecked == true)
			{
				sourceExpander.IsChecked = false;
				_profileDomainsListView.ListView.UpdateLayout();

				// Since we collapse the group before moving it, we need to update the inserting index
				if (e.SourceIndex < e.TargetIndex)
					indexToInsert -= DomainsTab_GetGroupChildren(group).Length;
			}

			// Insert
			sourceCollection.RemoveAt(e.SourceIndex);
			sourceCollection.Insert(indexToInsert, group);
			_profileDomainsListView.ListView.SelectedIndex = indexToInsert;
			group.DisplayPosition = indexToInsert;
		}

		Oltp.SerpProfileDomainGroupRow DomainsTab_GetGroupItem(int index)
		{
			if (index < 0)
				return null;

			ObservableCollection<DataRow> source = _profileDomainsListView.ListView.ItemsSource as ObservableCollection<DataRow>;

			// If a group is selected, use it as parent; if a domain row is selected, find its parent
			Oltp.SerpProfileDomainGroupRow group = null;
			if (source[index] is Oltp.SerpProfileDomainGroupRow)
			{
				group = source[index] as Oltp.SerpProfileDomainGroupRow;
			}
			else
			{
				int i = index;
				while (i  >= 0)
				{
					if (source[i] is Oltp.SerpProfileDomainGroupRow)
					{
						group = source[i] as Oltp.SerpProfileDomainGroupRow;
						break;
					}
					i--;
				}
			}

			return group;
		}

		Oltp.SerpProfileDomainRow[] DomainsTab_GetGroupChildren(Oltp.SerpProfileDomainGroupRow group)
		{
			return (Oltp.SerpProfileDomainRow[]) _profileDomainsTable.Select(
						String.Format("{0} = {1}", _profileDomainsTable.GroupIDColumn.ColumnName,
							group.IsIDNull() ? -1 - _profileDomainGroupsTable.Rows.IndexOf(group) : group.ID));
		}

		/// <summary>
		/// 
		/// </summary>
		private void DomainsTab_GroupExpanded(object sender, RoutedEventArgs e)
		{
			Oltp.SerpProfileDomainGroupRow group = (Oltp.SerpProfileDomainGroupRow) (e.OriginalSource as ListViewItem).Content;
			ObservableCollection<DataRow> sourceCollection = _profileDomainsListView.ListView.ItemsSource as ObservableCollection<DataRow>;

			DataRow[] domains = _profileDomainsTable.Select(String.Format("{0} = {1}",
				_profileDomainsTable.GroupIDColumn.ColumnName,
				group.IsIDNull() ? -1 - _profileDomainGroupsTable.Rows.IndexOf(group) : group.ID));

			if (domains.Length > 0)
			{
				int insertIndex = sourceCollection.IndexOf(group)+1;
				foreach (DataRow r in domains)
					sourceCollection.Insert(insertIndex++, r);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void DomainsTab_GroupCollapsed(object sender, RoutedEventArgs e)
		{
			Oltp.SerpProfileDomainGroupRow group = (Oltp.SerpProfileDomainGroupRow) (e.OriginalSource as ListViewItem).Content;
			ObservableCollection<DataRow> sourceCollection = _profileDomainsListView.ListView.ItemsSource as ObservableCollection<DataRow>;

			int removableIndex = sourceCollection.IndexOf(group)+1;

			// Keep removing all items after the campaign until we reach the end or another campaign
			while (sourceCollection.Count > removableIndex && sourceCollection[removableIndex] is Oltp.SerpProfileDomainRow)
				sourceCollection.RemoveAt(removableIndex);
		}

		/// <summary>
		/// 
		/// </summary>
		private void _profileDomainsListView_Loaded(object sender, RoutedEventArgs e)
		{
			_profileDomainsListView = sender as ListTable;
		}


		private void _domainTextbox_Loaded(object sender, RoutedEventArgs e)
		{
			_domainTextbox = sender as TextBox;
		}

		private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount < 2)
				return;

			TextBlock source = sender as TextBlock;
			source.Visibility = Visibility.Collapsed;
			TextBox target = Visual.GetDescendant<TextBox>(source.Parent);
			target.Visibility = Visibility.Visible;
			target.Focus();

			e.Handled = true;
		}

		private void TextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			TextBox source = sender as TextBox;
			(source as TextBox).Visibility = Visibility.Collapsed;
			TextBlock target = Visual.GetDescendant<TextBlock>(source.Parent);
			target.Visibility = Visibility.Visible;

			(source.DataContext as IPropertyChangeNotifier).OnAllPropertiesChanged();
		}

		//private void TextBox_MouseDown(object sender, MouseButtonEventArgs e)
		//{
		//    TextBox source = sender as TextBox;
		//    Point p = e.GetPosition(source);
		//    if (p.X >= 0 && p.X <= source.RenderSize.Width && p.Y >= 0 && p.Y <= source.RenderSize.Height)
		//    {
		//    }
		//    else
		//    {
		//        TextBox_LostFocus(sender, new RoutedEventArgs());
		//        e.Handled = true;
		//    }
		//}

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

		#region Search engines
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SearchEnginesTab_GotFocus(object sender, RoutedEventArgs e)
		{
			if (_profileSearchEnginesListView.ListView.ItemsSource == null)
			{
				Oltp.SerpProfileRow profile = Profile_dialog.Content as Oltp.SerpProfileRow;

				Window.AsyncOperation(delegate()
				{
					if (profile.RowState != DataRowState.Added)
					{
						using (OltpProxy proxy = new OltpProxy())
						{
							_profileSearchEnginesTable = proxy.Service.SerpProfileSearchEngine_Get(profile.ID);
						}
					}
					else
						_profileSearchEnginesTable = new Oltp.SerpProfileSearchEngineDataTable();
					
				},
				delegate()
				{
					SetListSource<Oltp.SerpProfileSearchEngineRow>(_profileSearchEnginesTable, _profileSearchEnginesListView.ListView);
					SearchEnginesTab_LoadPicker();
				});
			}
			else
				SearchEnginesTab_LoadPicker();
		}

		private void SearchEnginesTab_LoadPicker()
		{
			if (_searchEnginePicker.ItemsSource == null)
			{
				_searchEnginePickerItems = new ObservableCollection<Oltp.SearchEngineRow>();
				foreach (Oltp.SearchEngineRow se in _searchEnginePickerEngines)
				{
					if (_profileSearchEnginesTable.Select(String.Format("{0} = {1}", _profileSearchEnginesTable.ProfileIDColumn.ColumnName, se.ID)).Length < 1)
						_searchEnginePickerItems.Add(se);
				}

				_searchEnginePicker.ItemsSource = _searchEnginePickerItems;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void _profileSearchEnginesListView_Loaded(object sender, RoutedEventArgs e)
		{
			_profileSearchEnginesListView = sender as ListTable;
		}

		private void SearchEnginesTab_Add(object sender, RoutedEventArgs e)
		{
			Oltp.SearchEngineRow searchEngineRow = _searchEnginePicker.SelectedItem as Oltp.SearchEngineRow;

			// No searchEngine is selected
			if (searchEngineRow == null)
				return;
			
			Oltp.SerpProfileSearchEngineRow profileSearchEngineRow = _profileSearchEnginesTable.NewSerpProfileSearchEngineRow();
			profileSearchEngineRow.AccountID = Window.CurrentAccount.ID;
			profileSearchEngineRow.SearchEngineID = searchEngineRow.ID;
			profileSearchEngineRow.Name = searchEngineRow.Name;
			_profileSearchEnginesTable.Rows.Add(profileSearchEngineRow);

			ObservableCollection<Oltp.SerpProfileSearchEngineRow> source = _profileSearchEnginesListView.ListView.ItemsSource as ObservableCollection<Oltp.SerpProfileSearchEngineRow>;
			source.Add(profileSearchEngineRow);

			int currentPickerIndex = _searchEnginePicker.SelectedIndex;
			_searchEnginePickerItems.Remove(searchEngineRow);
			_searchEnginePicker.SelectedIndex = currentPickerIndex > _searchEnginePicker.Items.Count-1 ? _searchEnginePicker.Items.Count-1 : currentPickerIndex;
		}

		private void _searchEnginePicker_Loaded(object sender, RoutedEventArgs e)
		{
			_searchEnginePicker = sender as ComboBox;
		}

		private void _profileSearchEnginesListView_ItemDragged(object sender, ItemDraggedRoutedEventArgs e)
		{
			ObservableCollection<Oltp.SerpProfileSearchEngineRow> sourceCollection = _profileSearchEnginesListView.ListView.ItemsSource as ObservableCollection<Oltp.SerpProfileSearchEngineRow>;
			Oltp.SerpProfileSearchEngineRow se = sourceCollection[e.SourceIndex];
			int indexToInsert = e.SourceIndex < e.TargetIndex ? e.TargetIndex-1 : e.TargetIndex;

			// Insert
			sourceCollection.RemoveAt(e.SourceIndex);
			sourceCollection.Insert(indexToInsert, se);
			_profileSearchEnginesListView.ListView.SelectedIndex = indexToInsert;
			se.DisplayPosition = indexToInsert;
		}

		private void SearchEnginesTab_Remove(object sender, RoutedEventArgs e)
		{
			ObservableCollection<Oltp.SerpProfileSearchEngineRow> sourceCollection = _profileSearchEnginesListView.ListView.ItemsSource as ObservableCollection<Oltp.SerpProfileSearchEngineRow>;
			foreach (Oltp.SerpProfileSearchEngineRow se in new ArrayList(_profileSearchEnginesListView.ListView.SelectedItems))
			{
				int seID = se.SearchEngineID;
				sourceCollection.Remove(se);
				se.Delete();

				// Add the engine back to the available list
				Oltp.SearchEngineRow pickerEngine = _searchEnginePickerEngines.FindByID(seID);
				if (pickerEngine != null)
					_searchEnginePickerItems.Insert(0, pickerEngine);
			}
		}

		/*=========================*/
		#endregion

		#region Scheduling
		/*=========================*/

		private void _comboScheduling_Loaded(object sender, RoutedEventArgs e)
		{
			_comboScheduling = sender as ComboBox;
		}

		private void _monthCalendar_Loaded(object sender, RoutedEventArgs e)
		{
			_monthCalendar = sender as Grid;
		}

		private void _weekDay_Loaded(object sender, RoutedEventArgs e)
		{
			_weekDay = sender as StackPanel;
		}

		private void ApplySchedule(object sender, RoutedEventArgs e)
		{
			//ScheduleUnit su = null;
			//List<int> days;

			//// Set any selected days
			//switch (_comboScheduling.SelectedIndex)
			//{
			//    case 1:
			//        su = new ScheduleUnit("(w|1,2,3,4,5,6,7)");
			//        break;

			//    case 2:
			//        days = new List<int>();
			//        for (int i = 0; i < 7; i++)
			//            if ((_weekDay.Children[i] as CheckBox).IsChecked == true)
			//                days.Add(i+1);

			//        su = new ScheduleUnit(new int[0], new int[0], days.ToArray(), new int[0]);

			//        break;

			//    case 3:
			//        days = new List<int>();
			//        for (int i = 0; i < 31; i++)
			//            if ((_monthCalendar.Children[i] as ToggleButton).IsChecked == true)
			//                days.Add(i+1);

			//        su = new ScheduleUnit(days.ToArray(), new int[0], new int[0], new int[0]);

			//        break;
			//}

			//(Profile_dialog.Content as Oltp.SerpProfileRow).HiFrequencySchedule = su == null ? 
			//    null : su.ScheduleUnitStringView;
		}

		private void _comboScheduling_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_comboScheduling == null)
				return;

			_monthCalendar.Visibility = _comboScheduling.SelectedIndex != 2 ? Visibility.Visible : Visibility.Collapsed;
			_weekDay.Visibility = _comboScheduling.SelectedIndex == 2 ? Visibility.Visible : Visibility.Collapsed;

			_monthCalendar.IsEnabled = _weekDay.IsEnabled = _comboScheduling.SelectedIndex > 1;

			ApplySchedule(null, null);
		}

		/*=========================*/
		#endregion

	}
}

namespace Easynet.Edge.UI.Client.SerpProfilesLocal
{
	[ValueConversion(typeof(bool), typeof(Style))]
	public class DomainNameStyleConverter: IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is bool && (bool)value)
			{
				return App.CurrentPage.Resources["AccountDomainNameStyle"];
			}
			else
				return App.CurrentPage.Resources["CompetitorDomainNameStyle"];
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return false;
		}

		#endregion
	}

	[ValueConversion(typeof(bool), typeof(Visibility))]
	public class BoolToVisibilityConverter: IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is bool && (bool) value)
			{
				return Visibility.Visible;
			}
			else
				return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is Visibility && (Visibility) value == Visibility.Visible)
			{
				return true;
			}
			else
				return false;
		}

		#endregion
	}

	[ValueConversion(typeof(Visibility), typeof(bool))]
	public class VisibilityToBoolConverter: IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is Visibility && (Visibility) value == Visibility.Visible)
			{
				return true;
			}
			else
				return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is bool && (bool) value)
			{
				return Visibility.Visible;
			}
			else
				return Visibility.Collapsed;
		}

		#endregion
	}
	[ValueConversion(typeof(bool), typeof(Brush))]
	public class ProfileNameColorConverter: IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is bool && (bool) value)
			{
				return new BrushConverter().ConvertFromString("#759813");
			}
			else
				return new BrushConverter().ConvertFromString("#c0c0c0");
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return true;
		}

		#endregion
	}

	[ValueConversion(typeof(int), typeof(string))]
	public class HourConverter: IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is int)
			{
				int h = (int) value;
				return String.Format("{0:00}:{1:00}", h/100, h%100);
			}
			else
				return value.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			int originalValue = 0;
			if (value is string)
			{
				string[] parts = value.ToString().Split(':');
				
				int h, m;
				if (parts.Length == 2 && Int32.TryParse(parts[0], out h) && Int32.TryParse(parts[1], out m))
					originalValue = h * 100 + m; 
			}

			return originalValue;
		}

		#endregion
	}

	[ValueConversion(typeof(int), typeof(string))]
	public class TotalResultsDisplayConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is int)
			{
				int val = (int)value;
				return val < 0 ? "--" : val.ToString();
			}
			else
				return "?";
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			int retVal = -1;
			if (value is string)
				Int32.TryParse(value as string, out retVal);

			return retVal;
		}

		#endregion
	}

	[ValueConversion(typeof(int), typeof(string))]
	public class TotalResultsEditConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is int)
			{
				int val = (int)value;
				return val < 1 ? string.Empty : val.ToString();
			}
			else
				return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			int retVal;
			if (!(value is string) || !Int32.TryParse(value as string, out retVal))
				retVal = -1;

			return retVal;
		}

		#endregion
	}

	/// <summary>
	/// 
	/// </summary>
	public class ExpandToggleTemplateSelector: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is Oltp.SerpProfileDomainGroupRow)
				return App.CurrentPage
					.FindResource("GroupExpandToggleTemplate") as DataTemplate;
			else
				return new DataTemplate();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class DragHandleTemplateSelector: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is Oltp.SerpProfileDomainGroupRow)
				return App.CurrentPage
					.FindResource("GroupDragHandleTemplate") as DataTemplate;
			else
				return new DataTemplate();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class NameTemplateSelector: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is Oltp.SerpProfileDomainGroupRow)
				return App.CurrentPage
					.FindResource("GroupNameTemplate") as DataTemplate;
			else if (item is Oltp.SerpProfileDomainRow)
				return App.CurrentPage
					.FindResource("DomainNameTemplate") as DataTemplate;
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