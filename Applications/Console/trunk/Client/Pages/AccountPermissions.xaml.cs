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
using System.Xml;

namespace Easynet.Edge.UI.Client.Pages
{
	/// <summary>
	/// Interaction logic for AccountPermissions.xaml
	/// </summary>
	[AccountDependentPage]
	public partial class AccountPermissions: PageBase
	{
		#region Fields
		/*=========================*/

		private ObservableCollection<DataRow> _items;
		private ObservableCollection<object> _groupsToAdd;
		private ObservableCollection<object> _usersToAdd;
		bool _openingDialog = false;

		/*=========================*/
		#endregion

		#region Constructors
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		public AccountPermissions()
		{
			InitializeComponent();
		}

		/*=========================*/
		#endregion

		#region General Methods
		/*=========================*/

		public override void OnAccountChanged()
		{
			GetPermissionTargets(Window.CurrentAccount);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="include"></param>
		private void GetPermissionTargets(Oltp.AccountRow account)
		{
			Oltp.AccountRow currentAccount = account == null ? this.Window.CurrentAccount : account;

			if (currentAccount == null)
				return;

			Oltp.UserGroupDataTable groupsWith = null;
			Oltp.UserDataTable usersWith = null;
			Oltp.UserGroupDataTable groupsWithout = null;
			Oltp.UserDataTable usersWithout = null;

			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					groupsWith = proxy.Service.UserGroup_GetGroupsWithPermissions(currentAccount.ID);
					usersWith = proxy.Service.User_GetUsersWithPermissions(currentAccount.ID);
					groupsWithout = proxy.Service.UserGroup_GetGroupsWithoutPermissions(currentAccount.ID);
					usersWithout = proxy.Service.User_GetUsersWithoutPermissions(currentAccount.ID);
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
				foreach (DataRow r in groupsWith.Rows)
					_items.Add(r);
				foreach (DataRow r in usersWith.Rows)
					_items.Add(r);

				_listTable.ListView.ItemsSource = _items;

				//-------------------------------//

				// Populate 'add' combo boxes
				if (_groupsToAdd == null)
					_groupsToAdd = new ObservableCollection<object>();
				else
					_groupsToAdd.Clear();

				if (_usersToAdd == null)
					_usersToAdd = new ObservableCollection<object>();
				else
					_usersToAdd.Clear();

				_groupsToAdd.Add("Add group");
				_usersToAdd.Add("Add user");

				foreach (DataRow r in groupsWithout.Rows)
					_groupsToAdd.Add(r);
				foreach (DataRow r in usersWithout.Rows)
					_usersToAdd.Add(r);

				_comboAddGroup.ItemsSource = _groupsToAdd;
				_comboAddUser.ItemsSource = _usersToAdd;

				_comboAddGroup.SelectedIndex = 0;
				_comboAddUser.SelectedIndex = 0; ;
			});
		}

		string GetPermissionValue(XmlLinkedNode x)
		{
			return x.Attributes["Permission"] != null ?
				x.Attributes["Permission"].Value :
				x.Attributes["Class"] != null ? x.Attributes["Class"].Value :
					null;
		}

		/*=========================*/
		#endregion

		#region Permission targets
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		private void PermissionTarget_Delete(object sender, RoutedEventArgs e)
		{
			// Get the select item
			ListViewItem item = Visual.GetRoot<ListViewItem>(sender as DependencyObject);
			Oltp.UserRow user = item.Content as Oltp.UserRow;
			Oltp.UserGroupRow group = item.Content as Oltp.UserGroupRow;

			// Confirm with the user
			MessageBoxResult result = MessageBox.Show(String.Format("Remove all permissions for {0}?",
				user == null ? group.Name : user.Name),
				"Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.OK);

			if (result != MessageBoxResult.OK)
				return;

			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					proxy.Service.AccountPermission_RemovePermissions(
						Window.CurrentAccount.ID,
						user == null ? group.ID : user.ID,
						user == null);
				}
			},
			delegate(Exception ex)
			{
				MessageBoxError("Failed to remove permissions.", ex);
				return false;
			},
			delegate()
			{
				_items.Remove(user == null ? group as DataRow : user as DataRow);
				if (user == null)
					_groupsToAdd.Add(group);
				else
					_usersToAdd.Add(user);
			});
		}

		/// <summary>
		/// Open the dialog
		/// </summary>
		private void PermissionTarget_dialog_Open(object sender, RoutedEventArgs e)
		{
			// Set dataItem as current item
			ListViewItem currentItem = _listTable.GetParentListViewItem(e.OriginalSource as FrameworkElement);
			Oltp.UserRow user = currentItem.Content as Oltp.UserRow;
			Oltp.UserGroupRow group = currentItem.Content as Oltp.UserGroupRow;

			// Retrieve applied permissions from the server
			Oltp.AccountPermissionDataTable permissionsTable  = null;
			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					permissionsTable = proxy.Service.AccountPermission_Get(Window.CurrentAccount.ID,
						user == null ? group.ID : user.ID,
						user == null
						);
				}
			},
			delegate(Exception ex)
			{
				MessageBoxError("Failed to open permissions.", ex);
				return false;
			},
			delegate()
			{
				_openingDialog = true;

				// Apply retrieved permissions to the GUI radio buttons
				ListBox permissionsList = Visual.GetDescendant<ListBox>(PermissionTarget_dialog);
				for (int i = 0; i < permissionsList.Items.Count; i++)
				{
					XmlLinkedNode x = (XmlLinkedNode)permissionsList.Items[i];
					string permissionValue = GetPermissionValue(x);

					if (permissionValue == null)
						continue;

					Oltp.AccountPermissionRow perm = permissionsTable.FindByAccountIDTargetIDTargetIsGroupPermissionType(
						Window.CurrentAccount.ID,
						user == null ? group.ID : user.ID,
						user == null,
						permissionValue);

					ListBoxItem lbItem = (ListBoxItem)permissionsList.ItemContainerGenerator.ContainerFromIndex(i);

					if (perm == null)
						Visual.GetDescendant<RadioButton>(lbItem, "radioNotSet").IsChecked = true;
					else if (perm.Value)
						Visual.GetDescendant<RadioButton>(lbItem, "radioAllow").IsChecked = true;
					else
						Visual.GetDescendant<RadioButton>(lbItem, "radioDeny").IsChecked = true;
				}

				currentItem.IsSelected = true;

				// Show the dialog
				PermissionTarget_dialog.Title = String.Format("Editing permissions for {0}", user == null ? group.Name : user.Name);
				PermissionTarget_dialog.BeginEdit(new object[] { user == null ? group as DataRow : user as DataRow, permissionsTable });

				_openingDialog = false;
			});
		}

		private void radio_Checked(object sender, RoutedEventArgs e)
		{
			if (_openingDialog)
				return;

			// Check which radio button was pressed
			bool? state = (sender as RadioButton).Name == "radioAllow" ?
				(bool?) true :
				(sender as RadioButton).Name == "radioDeny" ? 
				(bool?) false : 
				(bool?) null;

			
			// Retrieve the data of the permission being set
			ListBoxItem lbItem = Visual.GetRoot<ListBoxItem>(sender as RadioButton);
			XmlLinkedNode x = (XmlLinkedNode) lbItem.Content;
			string permissionValue = GetPermissionValue(x);
			if (permissionValue == null)
				return;

			// Retrieve relevant parameters
			object[] parameters = PermissionTarget_dialog.Content as object[];
			Oltp.UserRow user = parameters[0] as Oltp.UserRow;
			Oltp.UserGroupRow group = parameters[0] as Oltp.UserGroupRow;
			Oltp.AccountPermissionDataTable permissionsTable  = parameters[1] as Oltp.AccountPermissionDataTable;

			// Get the row with the last value for this permissions
			Oltp.AccountPermissionRow perm = permissionsTable.FindByAccountIDTargetIDTargetIsGroupPermissionType(
				Window.CurrentAccount.ID,
				user == null ? group.ID : user.ID,
				user == null,
				permissionValue);

			if (state == null && perm != null)
			{
				// Moving to unset, so if a permission exists, delete it
				perm.Delete();
			}
			else if (state != null)
			{
				if (perm == null)
				{
					// Create a new permission target if none exists
					perm = permissionsTable.NewAccountPermissionRow();
					perm.AccountID = Window.CurrentAccount.ID;
					perm.TargetID = user != null ? user.ID : group.ID;
					perm.TargetIsGroup = user == null;
					perm.PermissionType = permissionValue;
					perm.Value = true;
					permissionsTable.Rows.Add(perm);
				}
				else if (perm.RowState == DataRowState.Deleted)
				{
					// If the target was deleted, restore it
					perm.RejectChanges();
				}

				// Apply the correct value
				if (state == true)
					perm.Value = true;
				else
					perm.Value = false;
			}


		}

		/// <summary>
		/// 
		/// </summary>
		private void PermissionTarget_dialog_ApplyingChanges(object sender, CancelRoutedEventArgs e)
		{
			// Retrieve relevant parameters
			object[] parameters = PermissionTarget_dialog.Content as object[];
			Oltp.AccountPermissionDataTable permissionsTable  = parameters[1] as Oltp.AccountPermissionDataTable;

			DataTable changes = permissionsTable.GetChanges();

			// No changes were made, skip the apply (but don't cancel)
			if (changes == null)
			{
				e.Skip = true;
				PermissionTarget_dialog.EndApplyChanges(e);
				return;
			}

			// Save the changes to the DB
			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					proxy.Service.AccountPermission_Save(changes as Oltp.AccountPermissionDataTable);
				}
			},
			delegate(Exception ex)
			{
				// Failed, so cancel and display a message
				MessageBoxError("Error while saving permissions.", ex);
				e.Cancel = true;
				return false;
			},
			delegate()
			{
				permissionsTable.AcceptChanges();
				PermissionTarget_dialog.EndApplyChanges(e);
			});
		}

		/// <summary>
		/// 
		/// </summary>
		private void PermissionTarget_dialog_AppliedChanges(object sender, CancelRoutedEventArgs e)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		private void PermissionTarget_dialog_Closing(object sender, CancelRoutedEventArgs e)
		{
			// Retrieve relevant parameters
			object[] parameters = PermissionTarget_dialog.Content as object[];
			Oltp.AccountPermissionDataTable permissionsTable  = parameters[1] as Oltp.AccountPermissionDataTable;

			if (permissionsTable.GetChanges() == null)
				return;

			MessageBoxResult result = MessageBox.Show(
				"Discard changes?",
				"Confirm",
				MessageBoxButton.OKCancel,
				MessageBoxImage.Warning,
				MessageBoxResult.Cancel);

			e.Cancel = result == MessageBoxResult.Cancel;
		}

		private void _comboAdd_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count < 1 || e.AddedItems[0] is string)
				return;

			// Get the item to add (one of 'em will always be null, eh, what)
			Oltp.UserRow user = e.AddedItems[0] as Oltp.UserRow;
			Oltp.UserGroupRow group = e.AddedItems[0] as Oltp.UserGroupRow;

			// Add default permissions
			Oltp.AccountPermissionDataTable pTable = new Oltp.AccountPermissionDataTable();
			foreach (Type pageClass in Const.DefaultPermissions)
			{
				Oltp.AccountPermissionRow pRow = pTable.NewAccountPermissionRow();
				pRow.AccountID = Window.CurrentAccount.ID;
				pRow.TargetID = user != null ? user.ID : group.ID;
				pRow.TargetIsGroup = user == null;
				pRow.PermissionType = pageClass.FullName;
				pRow.Value = true;
				pTable.Rows.Add(pRow);
			}

			// Save the new permissions
			Window.AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					proxy.Service.AccountPermission_Save(pTable);
				}
			},
			delegate(Exception ex)
			{
				MessageBoxError("Failed to add new permissions.", ex);
				return false;
			},
			delegate()
			{
				//... now update the GUI

				// Remove from the items of the combobox
				(user != null ? _usersToAdd : _groupsToAdd)
					.Remove(user != null ? user as DataRow : group as DataRow);

				// Insert in the cirrect new index
				int newIndex = user != null ? _items.Count : GetFirstUserIndex();
				_items.Insert(
					newIndex,
					user != null ? user as DataRow : group as DataRow
					);

				(sender as ComboBox).SelectedIndex = 0;
				_listTable._listView.SelectedIndex = newIndex;
			});
		}

		private int GetFirstUserIndex()
		{
			for (int i = 0; i < _items.Count; i++)
			{
				if (_items[i] is Oltp.UserRow)
				{
					return i;
				}
			}

			return _items.Count;
		}


		/*=========================*/
		#endregion
	}
}

namespace Easynet.Edge.UI.Client
{
	public partial class Const
	{
		public static readonly Type[] DefaultPermissions =
			new Type[] { /*typeof(Pages.MasterKeywords)*/ };
	}

}



namespace Easynet.Edge.UI.Client.AccountPermissionsLocal
{

	public class NameTemplateSelector: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is Oltp.UserGroupRow)
				return App.CurrentPage
					.FindResource("GroupNameTemplate") as DataTemplate;
			else
				return App.CurrentPage
					.FindResource("UserNameTemplate") as DataTemplate;
		}
	}
	
	public class AddComboBoxTemplateSelector: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is Oltp.UserGroupRow || item is Oltp.UserRow)
				return App.CurrentPage
					.FindResource("AddComboBoxItemTemplate") as DataTemplate;
			else
				return App.CurrentPage
					.FindResource("AddComboBoxHeaderTemplate") as DataTemplate;
		}
	}
}