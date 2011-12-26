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

namespace Easynet.Edge.UI.Client.Pages
{
	/// <summary>
	/// Interaction logic for AccountSettings.xaml
	/// </summary>
	[AccountDependentPage]
	public partial class AccountSettings: PageBase
	{
		#region Fields
		/*=========================*/

		Oltp.RelatedAccountDataTable _relatedAccountsTable;
		bool _isAccountChanging = false;

		/*=========================*/
		#endregion

		#region Constructors
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		public AccountSettings()
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
					(
						Window.CurrentAccount.RowState == DataRowState.Modified ||
						(_relatedAccountsTable != null && _relatedAccountsTable.GetChanges() != null)
					);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		void Page_Loaded(object sender, RoutedEventArgs e)
		{
		}

		public override bool OnAccountChanging(Oltp.AccountRow account)
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
		/// <param name="value"></param>
		/// <returns></returns>
		public override void OnAccountChanged()
		{
			_isAccountChanging = true;
			
			// Data context for general fields
			this.DataContext = Window.CurrentAccount;

			if (_relatedAccounts.ItemsSource  == null)
			{
				_relatedAccounts.ItemsSource = Window.OtherAccounts;
				_relatedAccounts.UpdateLayout();
			}

			LoadRelatedAccounts(Window.CurrentAccount.ID);
			
			_isAccountChanging = false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override bool Unload()
		{
			return OnAccountChanging(null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private bool SaveSettings()
		{
			OltpProxy proxy = new OltpProxy();
			using (proxy)
			{
				try
				{
					if (Window.CurrentAccount.RowState != DataRowState.Unchanged)
					{
						proxy.Service.Account_Save(Oltp.Prepare<Oltp.AccountDataTable>(Window.CurrentAccount.Table));
						Window.CurrentAccount.AcceptChanges();
					}

				}
				catch (Exception ex)
				{
					MessageBoxError("Failed to save settings.", ex);
					return false;
				}

				try
				{
					if (_relatedAccountsTable.GetChanges() != null)
					{
						proxy.Service.RelatedAccount_Save(_relatedAccountsTable);
					}
				}
				catch(Exception ex)
				{
					MessageBoxError("Failed to save related accounts.", ex);
					return false;
				}
			}
			_relatedAccountsTable.AcceptChanges();

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

		/// <summary>
		/// Loads related accounts to the listbox.
		/// </summary>
		/// <param name="accountID"></param>
		private void LoadRelatedAccounts(int accountID)
		{
			try
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					_relatedAccountsTable = proxy.Service.RelatedAccount_Get(accountID);
				}
			}
			catch (Exception ex)
			{
				MessageBoxError("Failed to retrieve related accounts.", ex);
				return;
			}

			foreach (Oltp.AccountRow account in Window.OtherAccounts)
			{
				Visual.GetDescendant<CheckBox>(_relatedAccounts.ItemContainerGenerator.ContainerFromItem(account)).IsChecked = 
					_relatedAccountsTable.FindByAccountIDRelatedAccountID(accountID, account.ID) != null;
			}
		}

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			if (_isAccountChanging)
				return;

			Oltp.AccountRow account = (sender as CheckBox).DataContext as Oltp.AccountRow;
			_relatedAccountsTable.AddRelatedAccountRow(Window.CurrentAccount.ID, account.ID);
		}

		private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			if (_isAccountChanging)
				return;

			Oltp.AccountRow account = (sender as CheckBox).DataContext as Oltp.AccountRow;
			_relatedAccountsTable.FindByAccountIDRelatedAccountID(Window.CurrentAccount.ID, account.ID).Delete();
		}

		/*=========================*/
		#endregion


	}
}