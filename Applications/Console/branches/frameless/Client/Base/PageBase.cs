using System;
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
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using System.Xml;
using System.ServiceModel;
using System.Collections;


namespace Easynet.Edge.UI.Client
{
	/// <summary>
	/// Base class for XAML pages.
	/// </summary>
	public class PageBase: UserControl
	{
		MainWindow _main = null;
        ApiMenuItem _pageData = null;

		#region Startup
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		public PageBase()
		{
			this.Loaded += new RoutedEventHandler(PageBase_Loaded);
		}

		void PageBase_Loaded(object sender, RoutedEventArgs e)
		{
			if (App.InDesignMode)
				return;
		}

		/*=========================*/
		#endregion

		#region Dialog-related Methods
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		protected void Dialog_Open<TableT, RowT>
			(
				FloatingDialog dialog,
				ListTable listTable,
				ListViewItem clickedItem,
				bool allowBatch,
				Func<RowT, bool, string> dialogTitle,
				Func<RowT, bool, string> dialogTooltip,
				Func<DataColumn, object> batchFlatten
			)
			where TableT : DataTable, new()
			where RowT : DataRow
		{
			// When a single item is clicked, select it
			if (clickedItem != null && clickedItem.DataContext != null && !listTable.ListView.SelectedItems.Contains(clickedItem.DataContext))
			{
				listTable.ListView.SelectedItems.Clear();
				clickedItem.IsSelected = true;
			}

			bool batch = listTable.ListView.SelectedItems.Count > 1;
			if (!allowBatch)
			{
				MainWindow.MessageBoxError("You cannot edit more than one item at a time.", null);
				return;
			}

			RowT[] targetRows = new RowT[listTable.ListView.SelectedItems.Count];
			try { listTable.ListView.SelectedItems.CopyTo(targetRows, 0); }
			catch (InvalidCastException)
			{
				MainWindow.MessageBoxError("To edit multiple items, select items of the same type.", null);
				return;
			}

			RowT controlRow = Dialog_MakeEditVersion<TableT, RowT>(targetRows, batchFlatten);

			// Show the dialog
			dialog.Title = dialogTitle != null ? dialogTitle(controlRow, batch) : "Editing " + typeof(RowT).Name;
			dialog.TitleTooltip = dialogTooltip != null ? dialogTooltip(controlRow, batch) : null;
			dialog.BeginEdit(controlRow, targetRows.Length > 1 ? (object)targetRows : (object)targetRows[0]);
		}
	
		/// <summary>
		/// 
		/// </summary>
		protected static RowT Dialog_MakeEditVersion<TableT, RowT>(RowT row)
			where TableT: DataTable, new()
			where RowT: DataRow
		{
			return Dialog_MakeEditVersion<TableT, RowT>(new RowT[] { row }, null);
		}

		protected static RowT Dialog_MakeEditVersion<TableT, RowT>(RowT[] rows, Func<DataColumn,object> nullify)
			where TableT : DataTable, new()
			where RowT : DataRow
		{
			TableT tempTable = new TableT();
			RowT controlRow = null;

			if (rows == null || rows.Length < 1)
			{
				// New row
				tempTable.Rows.Add(tempTable.NewRow());
				controlRow = tempTable.Rows[0] as RowT;
			}
			else
			{
				bool batch = rows.Length > 1;
				Dictionary<DataColumn, object> nullified = batch ? new Dictionary<DataColumn, object>() : null;
				Type[] numericTypes = new Type[]{typeof(int), typeof(long), typeof(double), typeof(float)};

				foreach (RowT row in rows)
				{
					if (controlRow == null)
					{
						// Since we're in batch mode and we need a control row
						tempTable.ImportRow(row);
						controlRow = (RowT)tempTable.Rows[0];
					}
					else if (batch && nullified.Count < tempTable.Columns.Count)
					{
						// This will only happen in batch mode
						foreach (DataColumn column in tempTable.Columns)
						{
							// Ignore columns already nullified
							if (nullified.ContainsKey(column))
								continue;
							
							// Get the null value from the delegate
							object nullValue = nullify(column);
							object valA = row[column.ColumnName] is DBNull ? column.DefaultValue : row[column.ColumnName];
							object valB = controlRow[column] is DBNull ? column.DefaultValue : controlRow[column];
							
							if (nullValue != null && !Object.Equals(valA, valB))
							{
								if (nullValue is DBNull)
								{
									// special treatment
									if (column.DataType == typeof(string))
										nullValue = string.Empty;
									else if (numericTypes.Contains(column.DataType))
										nullValue = Int32.MinValue;
								}

								controlRow[column] = nullValue;
								nullified[column] = nullValue;
							}
						}
					}
				}
				controlRow.AcceptChanges();
			}

			return controlRow;
		}

		protected static void Dialog_ApplyingChanges<TableT, RowT>(TableT sourceTable, FloatingDialog dialog, MethodInfo saveMethod, CancelRoutedEventArgs e)
			where TableT : DataTable, new()
			where RowT: DataRow
		{
			Dialog_ApplyingChanges<TableT, RowT>(sourceTable, dialog, saveMethod, e, null, false, null);
		}

		protected static void Dialog_ApplyingChanges<TableT, RowT>(TableT sourceTable, FloatingDialog dialog, MethodInfo saveMethod, CancelRoutedEventArgs e, object[] additionalArgs)
			where TableT : DataTable, new()
			where RowT : DataRow
		{
			Dialog_ApplyingChanges<TableT, RowT>(sourceTable, dialog, saveMethod, e, additionalArgs, false, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="async">When true, runs the specified server method in a seperate thread.</param>
		/// <param name="postApplying">
		///		If specified, this function is called after applying is complete. It is the
		///		function's responsibility to call FloatingDialog.EndApplyChanges() in order to complete the apply cycle. 
		/// </param>
		protected static void Dialog_ApplyingChanges<TableT, RowT>
			(
				TableT sourceTable,
				FloatingDialog dialog,
				MethodInfo saveMethod,
				CancelRoutedEventArgs e,
				object[] additionalArgs,
				bool async,
				Action postApplying
			)
			where TableT : DataTable, new()
			where RowT : DataRow
		{
			RowT controlRow = dialog.Content as RowT;
			DataTable changes = controlRow.Table.GetChanges();

			// No changes were made, skip the apply (but don't cancel)
			if (changes == null)
			{
				e.Skip = true;

				if (postApplying != null)
					postApplying();
				else
					dialog.EndApplyChanges(e);
				
				return;
			}

			if (dialog.IsBatch)
			{
				// Copy all target rows and apply the changed values to them
				TableT clonedTargetTable = new TableT();
				foreach (RowT row in (IEnumerable)dialog.TargetContent)
					clonedTargetTable.ImportRow(row);
				clonedTargetTable.AcceptChanges();
				dialog.ApplyBindingsToItems(clonedTargetTable.Rows);
				// Get these changes
				changes = clonedTargetTable.GetChanges();
			}

			// No changes were made, skip the apply (but don't cancel)
			if (changes == null)
			{
				e.Skip = true;

				if (postApplying != null)
					postApplying();
				else
					dialog.EndApplyChanges(e);

				return;
			}

			// Remember data state
			DataRowState state = controlRow.RowState;

			// Will store updated data
			TableT savedVersion = null;

			// Create a typed version of the changes
			object[] actualArgs = additionalArgs != null? new object[additionalArgs.Length + 1] : new object[1];
			actualArgs[0] = Oltp.Prepare<TableT>(changes);
			if (additionalArgs != null)
				additionalArgs.CopyTo(actualArgs, 1);

			// Save the changes to the DB
			Delegate[] delegates = new Delegate[]{
			(Action) delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					savedVersion = (TableT)saveMethod.Invoke(proxy.Service, actualArgs);
				}
			},
			(Func<Exception, bool>) delegate(Exception ex)
			{
				// Failed, so cancel and display a message
				MainWindow.MessageBoxError("Error while updating.", ex);
				e.Cancel = true;
				return false;
			},
			(Action) delegate()
			{
				// Special case when adding a new row
				if (!dialog.IsBatch && state == DataRowState.Added)
				{
					// Import the saved row
					sourceTable.ImportRow(savedVersion.Rows[0]);

					// Even though nothing needs to be updated, mark it as added so AppliedChanges handled treats it properly
					RowT newRow = sourceTable.Rows[sourceTable.Rows.Count - 1] as RowT;
					newRow.SetAdded();

					// Set as new target content
					dialog.TargetContent = newRow;
					dialog.Content = Dialog_MakeEditVersion<TableT, RowT>(newRow);
				}

				// Activate the post applying action
				if (postApplying != null)
					postApplying();
				else
					dialog.EndApplyChanges(e);
			}};

			if (async)
			{
				App.CurrentPage.Window.AsyncOperation(
					delegates[0] as Action,					// out-of-ui action
					delegates[1] as Func<Exception, bool>,	// exception handler
					delegates[2] as Action					// in-ui action
				);
			}
			else
			{
				bool exception = false;
				try
				{
					// out-of-ui action, done in ui because async is not required
					(delegates[0] as Action).Invoke();
				}
				catch (Exception ex)
				{
					// exception handler
					(delegates[1] as Func<Exception, bool>).Invoke(ex);
					exception = true;
				}

				// in-ui action
				if (!exception)
					(delegates[2] as Action).Invoke();

				if (postApplying != null)
					postApplying();
				else
					dialog.EndApplyChanges(e);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected static void Dialog_AppliedChanges<RowT>(FloatingDialog dialog, string editingDialogTitle, ListView hostListView, CancelRoutedEventArgs e)
			where RowT : DataRow
		{
			Dialog_AppliedChanges<RowT>(dialog, editingDialogTitle, hostListView, 0, e);
		}
	
		/// <summary>
		/// 
		/// </summary>
		protected static void Dialog_AppliedChanges<RowT>(FloatingDialog dialog, string editingDialogTitle, ListView hostListView, int minPosition, CancelRoutedEventArgs e)
			where RowT: DataRow
		{
			// All went well, so just accept changes
			RowT[] targetRows = dialog.TargetContent is RowT ? new RowT[] { dialog.TargetContent as RowT } : dialog.TargetContent as RowT[];
			RowT finalRow = dialog.IsBatch ? null : targetRows[0];

			targetRows[0].Table.AcceptChanges();
			(dialog.Content as RowT).AcceptChanges();

			// Add to the table
			if (!dialog.IsBatch && finalRow.RowState == DataRowState.Added)
			{
				ObservableCollection<DataRow> items = hostListView.ItemsSource as ObservableCollection<DataRow>;

				// -2 because we already added the row to the table. We want the position of the last available
				int newIndex = finalRow.Table.Rows.Count > 1 ? 
					items.IndexOf(finalRow.Table.Rows[finalRow.Table.Rows.Count-2])+1 :
					minPosition;

				if (newIndex >= 0)
				{
					// Insert a new row to the items only if a new position has been found
					items.Insert(newIndex, finalRow);
					hostListView.SelectedIndex = newIndex;
					hostListView.ScrollIntoView(finalRow);
				}
			}

			foreach (RowT row in targetRows)
			{
				if (row is IPropertyChangeNotifier)
					(row as IPropertyChangeNotifier).OnAllPropertiesChanged();
			}

			if (!dialog.IsBatch)
				dialog.Title = editingDialogTitle;
		}

		protected bool IsMissingData(bool isNew, params DataTable[] tables)
		{
			bool missingData = false;

			foreach (DataTable tbl in tables)
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
		protected void SetListSource<RowType>(DataTable inputTable, ListView targetListView)
			where RowType : DataRow
		{
			ObservableCollection<RowType> items = new ObservableCollection<RowType>();

			// Bind to the list
			foreach (RowType r in inputTable.Rows)
				items.Add(r);

			targetListView.ItemsSource = items;
		}

		public bool TryToCloseDialogs()
		{
			foreach (FloatingDialog dialog in this.FloatingDialogs)
			{
				if (dialog.IsOpen && !dialog.Close())
					return false;
			}
			return true;
		}

		public virtual FloatingDialog[] FloatingDialogs
		{
			get { return new FloatingDialog[0]; } 
		}

		/*=========================*/
		#endregion

		#region Public Properties
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		public MainWindow Window
		{
			get
			{
				if (_main == null)
					_main = VisualTree.GetChild<MainWindow>(App.Current.Windows[0]);

				return _main;
			}
		}


        public ApiMenuItem PageData
        {
            get { return _pageData; }
            set { _pageData = value; }
        }

		/*=========================*/
		#endregion

		#region Public Methods
		/*=========================*/

		/// <summary>
		/// Called when the page is being unloaded.
		/// </summary>
		/// <returns>True to continue, false to cancel.</returns>
		public virtual bool Unload()
		{
			return TryToCloseDialogs();
		}

		/// <summary>
		/// Called when the selected client account is being changed.
		/// </summary>
		/// <returns>True to continue, false to cancel.</returns>
		public virtual bool OnAccountChanging(Oltp.AccountRow account)
		{
			return true;
		}

		public virtual void OnAccountChanged()
		{
		}

		/*=========================*/
		#endregion
	}
}
