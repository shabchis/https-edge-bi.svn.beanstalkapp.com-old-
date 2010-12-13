﻿using System;
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
		/// <typeparam name="TableType"></typeparam>
		/// <typeparam name="RowType"></typeparam>
		/// <param name="table"></param>
		/// <param name="row"></param>
		/// <returns></returns>
		protected static RowType Dialog_MakeEditVersion<TableType, RowType>(RowType row)
			where TableType: DataTable, new()
			where RowType: DataRow
		{
			TableType editVersion = new TableType();

			if (row == null)
			{
				editVersion.Rows.Add(editVersion.NewRow());
			}
			else
				editVersion.ImportRow(row);

			return editVersion.Rows[0] as RowType;
		}


		protected static void Dialog_ApplyingChanges<TableType, RowType>(TableType sourceTable, FloatingDialog dialog, MethodInfo saveMethod, CancelRoutedEventArgs e)
			where TableType: DataTable, new()
			where RowType: DataRow
		{
			Dialog_ApplyingChanges<TableType, RowType>(sourceTable, dialog, saveMethod, e, null, false, null);
		}

		protected static void Dialog_ApplyingChanges<TableType, RowType>(TableType sourceTable, FloatingDialog dialog, MethodInfo saveMethod, CancelRoutedEventArgs e, object[] additionalArgs)
			where TableType : DataTable, new()
			where RowType : DataRow
		{
			Dialog_ApplyingChanges<TableType, RowType>(sourceTable, dialog, saveMethod, e, additionalArgs, false, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="async">When true, runs the specified server method in a seperate thread.</param>
		/// <param name="postApplying">
		///		If specified, this function is called after applying is complete. It is the
		///		function's responsibility to call FloatingDialog.EndApplyChanges() in order to complete the apply cycle. 
		/// </param>
		protected static void Dialog_ApplyingChanges<TableType, RowType>
			(
				TableType sourceTable,
				FloatingDialog dialog,
				MethodInfo saveMethod,
				CancelRoutedEventArgs e,
				object[] additionalArgs,
				bool async,
				Action postApplying
			)
			where TableType: DataTable, new()
			where RowType: DataRow
		{
			RowType editVersion = dialog.Content as RowType;
			DataTable changes = editVersion.Table.GetChanges();

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
			DataRowState state = changes.Rows[0].RowState;

			// Will store updated data
			TableType savedVersion = null;

			// Create a typed version of the changes
			object[] actualArgs = additionalArgs != null? new object[additionalArgs.Length + 1] : new object[1];
			actualArgs[0] = Oltp.Prepare<TableType>(changes);
			if (additionalArgs != null)
				additionalArgs.CopyTo(actualArgs, 1);

			// Save the changes to the DB
			Delegate[] delegates = new Delegate[]{
			(Action) delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					savedVersion = (TableType)saveMethod.Invoke(proxy.Service, actualArgs);
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
				if (state == DataRowState.Added)
				{
					// Import the saved row
					sourceTable.ImportRow(savedVersion.Rows[0]);

					// Even though nothing needs to be updated, mark it as added so AppliedChanges handled treats it properly
					RowType newRow = sourceTable.Rows[sourceTable.Rows.Count - 1] as RowType;
					newRow.SetAdded();

					// Set as new target content
					dialog.TargetContent = newRow;
					dialog.Content = Dialog_MakeEditVersion<TableType, RowType>(newRow);
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
		protected static void Dialog_AppliedChanges<RowType>(FloatingDialog dialog, string editingDialogTitle, ListView hostListView, CancelRoutedEventArgs e)
			where RowType: DataRow
		{
			Dialog_AppliedChanges<RowType>(dialog, editingDialogTitle, hostListView, 0, e);
		}
	
		/// <summary>
		/// 
		/// </summary>
		protected static void Dialog_AppliedChanges<RowType>(FloatingDialog dialog, string editingDialogTitle, ListView hostListView, int minPosition, CancelRoutedEventArgs e)
			where RowType: DataRow
		{
			// All went well, so just accept changes
			RowType finalRow = dialog.TargetContent as RowType;
			DataRowState state = finalRow.RowState;

			finalRow.Table.AcceptChanges();
			(dialog.Content as RowType).AcceptChanges();

			// Add to the table
			if (state == DataRowState.Added)
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

			if (finalRow is IPropertyChangeNotifier)
			{
				(finalRow as IPropertyChangeNotifier).OnAllPropertiesChanged();
			}

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
