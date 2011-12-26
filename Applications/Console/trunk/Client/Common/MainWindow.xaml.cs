using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using Easynet.Edge.UI.Data;
using System.Collections;
using Easynet.Edge.UI.Client.Pages;
using System.Runtime.Remoting.Messaging;
using Easynet.Edge.Core.Utilities;
using System.Windows.Controls.Primitives;
using System.Deployment.Application;

namespace Easynet.Edge.UI.Client
{
	/// <summary>
	/// Main window of the Edge2 Manager.
	/// </summary>
	public partial class MainWindow: Page
	{
		#region Fields
		/*=========================*/

		private Oltp.AccountRow _currentAccount = null;
		private PageBase _currentPage = null;
		private SelectionChangedEventHandler _accountsSelector_SelectionChangedHandler;
		private Oltp.AccountDataTable _accountsTable = null;
		private DataTable _userPermissions = null;

		/*=========================*/
		#endregion

		#region Start + Login
		/*=========================*/

		

		/// <summary>
		/// 
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();

			// HACK due to shitty namescope-related bug
			_mainMenu.RegisterName("_menuColumn", _menuColumn);

			// Event handlers
			this.Loaded += new RoutedEventHandler(MainWindow_Loaded);

			_accountsSelector.SelectionChanged += _accountsSelector_SelectionChangedHandler =
				new SelectionChangedEventHandler(_accountsSelector_SelectionChanged);
		}

		/// <summary>
		/// 
		/// </summary>
		void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			Version v = ApplicationDeployment.IsNetworkDeployed ? ApplicationDeployment.CurrentDeployment.CurrentVersion : Assembly.GetExecutingAssembly().GetName().Version;
			//Version v = Assembly.GetExecutingAssembly().GetName().Version;
			_version.Content = "Version " + v.ToString();

			// Log in from cookie
			int? userID = GetUserIDFromCookie();
			bool logout = userID == null;

			if (userID != null)
			{
				try
				{
					OltpProxy.SessionStart(userID.Value);
				}
				catch
				{
					logout = true;
				}
			}

			if (logout)
			{
				LogoutUser();
			}
			else
			{
				LoginUser();
			}
		}

		public int? GetUserIDFromCookie()
		{
			// Get the cookie value
			string signInCookie = App.Cookies[Const.Cookies.Login];
			if (signInCookie == null)
				return null;

			// Decode cookie value
			string decodedString;
			try { decodedString = Encryptor.Decrypt(signInCookie); }
			catch { return null; }
			if (decodedString == null)
				return null;

			int userID;
			if (!Int32.TryParse(decodedString, out userID))
				return null;

			return userID;
		}

		public void LoginUser()
		{
			CurrentUser = OltpProxy.CurrentUser;

			// Try to get the user's client list
			AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					_accountsTable = proxy.Service.Account_Get();
					_userPermissions = proxy.Service.User_GetAllPermissions();
				}
			},
			delegate(Exception ex)
			{
				PageBase.MessageBoxError("Failed to load user settings.", ex);
				return false;
			},
			delegate()
			{
				// Hide all menu items/sections that have NO PERMISSIONS at all (account = null)
				_mainMenu.Visibility = Visibility.Visible;
				_mainMenu.UpdateLayout();
				_mainMenu.ApplyPermissions(null);

				_header.Visibility = Visibility.Visible;
				_currentPageViewer.Content = CurrentPage = null;
				_pageTitle.Content = "";

				_accountsSelector.ItemsSource = _accountsTable;

				string selectedAccountCookie = String.Format("{0}.SelectedAccount", OltpProxy.CurrentUser.ID);
				string selectedAccount = App.Cookies[selectedAccountCookie];
				if (selectedAccount != null)
				{
					DataRow[] rs = _accountsTable.Select("ID = " + selectedAccount);
					if (rs.Length > 0)
					{
						_accountsSelector.SelectedIndex = _accountsTable.Rows.IndexOf(rs[0]);
					}
				}
			});
		}

		public void LogoutUser()
		{
			if (CurrentPage != null && !CurrentPage.TryToCloseDialogs())
				return;

			OltpProxy.SessionEnd();
			App.Cookies.ClearCookie(Const.Cookies.Login);

			_mainMenu.Visibility = Visibility.Hidden;
			_mainMenu.CollapseAll();
			_accountsSelector.ItemsSource = null;
			_accountsSelector.SelectedItem = null;
			_header.Visibility = Visibility.Hidden;

			CurrentPage = new Pages.GeneralLogin();
			_currentPageViewer.Content = CurrentPage;
			_pageTitle.Content = "Login";

		}


		/*=========================*/
		#endregion

		#region Public Properties
		/*=========================*/

		/// <summary>
		/// Gets or sets the currently selected account
		/// </summary>
		protected Oltp.UserRow CurrentUser
		{
			get { return (Oltp.UserRow) GetValue(CurrentUserProperty); }
			set { SetValue(CurrentUserProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CurrentUser.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CurrentUserProperty = 
			DependencyProperty.Register("CurrentUser", typeof(Oltp.UserRow), typeof(MainWindow));
		
	
		/// <summary>
		/// Gets or sets the currently selected account
		/// </summary>
		public Oltp.AccountRow CurrentAccount
		{
			get
			{
				return _currentAccount;
			}
			set
			{
				_currentAccount = value;
			}
		}

		public Oltp.AccountDataTable AvailableAccounts
		{
			get
			{
				return _accountsTable;
			}
		}

		public DataTable UserPermissions
		{
			get { return _userPermissions; }
		}

		public DataRow[] OtherAccounts
		{
			get
			{
				if (CurrentAccount == null)
					return _accountsTable.Select(null, _accountsTable.NameColumn.ColumnName);
				else
					return _accountsTable.Select(String.Format("{0}<>{1}", _accountsTable.IDColumn.ColumnName, CurrentAccount.ID), _accountsTable.NameColumn.ColumnName);
			}
		}
		/// <summary>
		/// Gets or sets the currently visible page.
		/// </summary>
		public PageBase CurrentPage
		{
			get
			{
				return _currentPage;
			}
			set
			{
				_currentPage = value;
			}
		}

		static MainWindow _main = null;
		public static MainWindow Current
		{
			get
			{
				if (_main == null)
					_main = Visual.GetDescendant<MainWindow>(App.Current.Windows[0]);

				return _main;
			}
		}

		/*=========================*/
		#endregion

		#region Internal methods
		/*=========================*/
		/// <summary>
		/// 
		/// </summary>
		private void _accountsSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count < 1)
				return;

			// Get the selected account
			Oltp.AccountRow accountToChangeTo = (e.AddedItems[0] as DataRowView).Row as Oltp.AccountRow;

			if (CurrentPage != null && !CurrentPage.OnAccountChanging(accountToChangeTo))
			{
				// Cancel the change (disable the event)
				_accountsSelector.SelectionChanged -= _accountsSelector_SelectionChangedHandler;
				_accountsSelector.SelectedItem = CurrentAccount;
				_accountsSelector.SelectionChanged += _accountsSelector_SelectionChangedHandler;
			}

			// Disable unavialable menu items for this account
			_mainMenu.ApplyPermissions(accountToChangeTo);

			if (CurrentPage != null && !HasPermission(accountToChangeTo, CurrentPage.PageData))
			{
				// Hide page if the current page is not enabled
				HidePageContents(HideContentsReason.AccessDenied);
			}
			else
			{
				// Complete the change
				CurrentAccount = accountToChangeTo;

				string selectedAccountCookie = String.Format("{0}.SelectedAccount", OltpProxy.CurrentUser.ID);
				App.Cookies[selectedAccountCookie] = accountToChangeTo.ID.ToString();
			}

			if (CurrentPage != null)
			{
				RestorePageContents();
				CurrentPage.OnAccountChanged();
			}
		}


		/// <summary>
		/// 
		/// </summary>
		private void _mainMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_mainMenu.IsLoaded)
				return;

			if (e.AddedItems.Count < 1)
				return;

			if (CurrentPage != null)
			{
				bool unloadCurrent = CurrentPage.Unload();

				// Cancel
				if (!unloadCurrent)
				{
					e.Handled = true;
					return;
				}
			}

			// Kill all async wait handles
			foreach (System.Threading.AutoResetEvent handle in _asyncHandles.Values)
			{
				handle.Set();
			}
			_asyncHandles.Clear();

			// Get page XML data
			XmlElement pageData = e.AddedItems[0] as XmlElement;

			// Load the defined class (PageBase-derived) if CurrentPage allows it
			if (pageData.HasAttribute("Class"))
			{
				try
				{
					Type newPage = Type.GetType(pageData.Attributes["Class"].Value);
					CurrentPage = (PageBase)Activator.CreateInstance(newPage);

                    CurrentPage.PageData = pageData;

                    _currentPageViewer.Content = CurrentPage;

					if (newPage.GetCustomAttributes(typeof(AccountDependentPageAttribute), false).Length > 0)
					{
						if (CurrentAccount != null)
						{
							// Tell the page to load the account
							CurrentPage.OnAccountChanged();
						}
						else
						{
							// Tell the page to hide the account
							HidePageContents(HideContentsReason.NoAccountSelected);
						}
					}

					// Hack for avoiding hiding the account dropdown behind the web browser
					if (newPage == typeof(WebFramePage))
					{
						_accountsSelector.MaxDropDownHeight = 50.0;
						AsyncOperationIndicator.VerticalAlignment = VerticalAlignment.Top;
						AsyncOperationIndicator.MaxHeight = 60;

					}
					else
					{
						_accountsSelector.ClearValue(ComboBox.MaxDropDownHeightProperty);
						AsyncOperationIndicator.VerticalAlignment = VerticalAlignment.Center;
						AsyncOperationIndicator.LayoutTransform = null;
						AsyncOperationIndicator.ClearValue(Grid.MaxHeightProperty);
					}
				}
				catch (Exception ex)
				{
					CurrentPage = null;
					_currentPageViewer.Content = 
						String.Format("Failed to load page. \n\n {0} ({1})", ex.Message, ex.GetType().FullName);
				}

			}

			// Change the title
			_pageTitle.Content = pageData.Attributes["Title"].Value;
			
			// Force garbage collection
			this.FloatingDialogContainer.Children.Clear();
			GC.Collect();
		}

		public bool HasPermission(Oltp.AccountRow account, XmlElement pageData)
		{
			if (OltpProxy.CurrentUser.AccountAdmin)
				return true;

			string permissionName = pageData.HasAttribute("Permission") ?
				pageData.Attributes["Permission"].Value :
				pageData.Attributes["Class"].Value;

			string condition = account != null ?
				String.Format("AccountID = {0} and PermissionType = '{1}'", account.ID, permissionName) :
				String.Format("PermissionType = '{0}'", permissionName);

			return _userPermissions.Select(condition).Length > 0;
		}

		public void HidePageContents(HideContentsReason reason)
		{
			TextBlock msg = new TextBlock();
			msg.FontSize = 14;
			msg.Foreground = Brushes.Red;
			msg.FontWeight = FontWeights.Bold;
			string msgText = "";
			switch (reason)
			{
				case HideContentsReason.AccessDenied:
					msgText = "\nYou do not have permission to view this page for the selected account.";
					break;
				case HideContentsReason.NoAccountSelected:
					msgText = "\nPlease select an account.";
					break;
				case HideContentsReason.BlockPage:
					msgText = "\n";
					break;

			}
			msg.Text = msgText;

			_currentPageViewer.Content = msg;
		}

		public void RestorePageContents()
		{
			_currentPageViewer.Content = CurrentPage;
		}

        

		private void _logoutButton_Click(object sender, RoutedEventArgs e)
		{
			LogoutUser();
		}

		private void _mainMenu_Hidden(object sender, EventArgs e)
		{
		}

		private void _mainMenu_Revealed(object sender, EventArgs e)
		{

		}
		/*=========================*/
		#endregion

		#region Async methods
		/*=========================*/

		// Fields
		static object _asyncIDLock = new object();
		static int _asyncOperationCount = 0;
		Dictionary<string, System.Threading.AutoResetEvent> _asyncHandles =
			new Dictionary<string, System.Threading.AutoResetEvent>();

		public bool AsyncInProgress
		{
			get { return _asyncOperationCount > 0; }
		}

		public void AsyncProgressIndicatorOn()
		{
			lock (_asyncIDLock)
			{
				_asyncOperationCount++;

				if (_asyncOperationCount == 1)
					AsyncOperationMask.Visibility = Visibility.Visible;
			}
		}

		public void AsyncProgressIndicatorOff()
		{
			AsyncProgressIndicatorOff(false);
		}

		public void AsyncProgressIndicatorOff(bool force)
		{
			lock (_asyncIDLock)
			{
				if (_asyncOperationCount > 0)
					_asyncOperationCount--;

				if (_asyncOperationCount == 0)
					AsyncOperationMask.Visibility = Visibility.Collapsed;
			}
		}

		public void AsyncOperation(Action operation)
		{
			AsyncOperation(operation, null, null);
		}

		public void AsyncOperation(Action operation, Action onComplete)
		{
			AsyncOperation(operation, null, onComplete);
		}

		public void AsyncOperation(Action operation, Func<Exception, bool> onException, Action onComplete)
		{
			AsyncProgressIndicatorOn();

			operation.BeginInvoke(delegate(IAsyncResult result)
			{
				this.Dispatcher.Invoke(new Action(delegate()
				{

					bool cont = true;
					try { operation.EndInvoke(result); }
					catch (Exception ex)
					{
						if (onException == null)
						{
							PageBase.MessageBoxError("Operation failed.", ex);
							cont = false;
						}
						else
						{
							try
							{
								cont = onException.Invoke(ex);
							}
							catch (Exception innerEx)
							{
								PageBase.MessageBoxError("Operation failed.", ex);
								cont = false;
							}
						}
					}

					// Proceed to completed handler
					if (cont && onComplete != null)
					{
						try
						{
							onComplete.Invoke();
						}
						catch (Exception ex)
						{
							PageBase.MessageBoxError("An error occured after the operation completed.", ex);
						}
					}

					AsyncProgressIndicatorOff();
				}
				), null);
			}, null);
		}

		public void AsyncStart(string operationName)
		{
			if (_asyncHandles.ContainsKey(operationName))
				throw new InvalidOperationException(String.Format("Operation '{0}' is already started.", operationName));

			_asyncHandles.Add(operationName, new System.Threading.AutoResetEvent(false));
		}

		public void AsyncEnd(string operationName)
		{
			if (!_asyncHandles.ContainsKey(operationName))
				return;

			_asyncHandles[operationName].Set();
			_asyncHandles.Remove(operationName);
		}

		public void AsyncWait(string operationName)
		{
			if (!_asyncHandles.ContainsKey(operationName))
				return;

			_asyncHandles[operationName].WaitOne();
		}

		/*=========================*/
		#endregion
	}

	public class AsyncMask : Grid
	{
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.Property != Grid.VisibilityProperty)
				return;
			
			if ((Visibility)e.NewValue == Visibility.Visible)
				this.RaiseEvent(new RoutedEventArgs(OpenedEvent));
			else
				this.RaiseEvent(new RoutedEventArgs(ClosedEvent));
		}

		public static readonly RoutedEvent OpenedEvent = EventManager.RegisterRoutedEvent
		(
			"Opened",
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(AsyncMask)
		);

		public event RoutedEventHandler Opened
		{
			add { AddHandler(OpenedEvent, value); }
			remove { RemoveHandler(OpenedEvent, value); }
		}

		public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent
		(
			"Closed",
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(AsyncMask)
		);

		public event RoutedEventHandler Closed
		{
			add { AddHandler(ClosedEvent, value); }
			remove { RemoveHandler(ClosedEvent, value); }
		}
	}

    namespace MainWindowLocal
    {
        /// <summary>
        /// 
        /// </summary>
        public class AccountItemTemplateSelector : DataTemplateSelector
        {
            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
                if (item is DataRowView && ((DataRowView)item).Row is Oltp.AccountRow)
                {
					Oltp.AccountRow accountRow = (Oltp.AccountRow)((DataRowView)item).Row;
					string template =
						accountRow.ID == accountRow.ClientID ?
							"AccountItem_AccountTopLevel_{0}" :
							"AccountItem_AccountSubLevel_{0}";

					template = String.Format(template,
						!OltpProxy.CurrentUser.IsAccountAdminNull() && OltpProxy.CurrentUser.AccountAdmin ?
							"Admin" :
							"Normal");

                    return MainWindow.Current
                        .FindResource(template) as DataTemplate;
                }
                else
                    return null;
            }
        }
    }
}
