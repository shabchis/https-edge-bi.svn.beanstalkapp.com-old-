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
using System.Collections.Specialized;
using System.Web;

namespace Easynet.Edge.UI.Client
{
	/// <summary>
	/// Main window of the Edge2 Manager.
	/// </summary>
	public partial class MainWindow: Page
	{
		#region Fields
		/*=========================*/

		private Oltp.AccountDataTable _accountsTable = null;
		private Oltp.AccountRow _currentAccount = null;
		private PageBase _currentPage = null;
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

			// Event handlers
			this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
		}

		/// <summary>
		/// 
		/// </summary>
		void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			Version v = ApplicationDeployment.IsNetworkDeployed ?
				ApplicationDeployment.CurrentDeployment.CurrentVersion :
				Assembly.GetExecutingAssembly().GetName().Version;

			//Version v = Assembly.GetExecutingAssembly().GetName().Version;
			_version.Content = "Version " + v.ToString();

			// Check access
			bool allowedAccess = false;
			if (ApplicationDeployment.CurrentDeployment.ActivationUri.Query == null)
			{
				HidePageContents("Local XBAP access not supported.");
				return;
			}

			NameValueCollection urlParams =
				HttpUtility.ParseQueryString(ApplicationDeployment.CurrentDeployment.ActivationUri.Query);

			// Invalid account
			string accountIDParam = urlParams["account"];
			int accountID;
			if (accountIDParam == null || !Int32.TryParse(accountIDParam, out accountID))
			{
				HidePageContents("Invalid account specified. Please select another account.");
				return;
			}

			// Log in from cookie
			string sessionID = urlParams["session"];
			if (sessionID != null)
			{
				try
				{
					OltpProxy.SessionStart(sessionID);
					allowedAccess = true;
				}
				catch {}
			}

			if (!allowedAccess)
			{
				HidePageContents("Your session has expired. Please refresh the page.");
				return;
			}
			CurrentUser = OltpProxy.CurrentUser;

			string menuItemPath = urlParams["path"];
			if (String.IsNullOrEmpty(menuItemPath))
			{
				HidePageContents("No menu path specified. Please select an item from the menu.");
				return;
			}

			// Get user settings
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
				DataRow[] rs = _accountsTable.Select("ID = " + accountIDParam);
				if (rs.Length < 1)
				{
					HidePageContents("Specified account was not found. Please select another account.");
					return;
				}
				else
				{
					_currentAccount = (Oltp.AccountRow)rs[0];
					_currentPageViewer.Content = CurrentPage = null;
				}
			});
		}


		/*=========================*/
		#endregion

		#region Public Properties
		/*=========================*/

		/// <summary>
		/// Gets or sets the currently selected account
		/// </summary>
		public Oltp.UserRow CurrentUser
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
					_main = VisualTree.GetChild<MainWindow>(App.Current.Windows[0]);

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
		private void LoadPage(ApiMenuItem menuItem)
		{
			if (CurrentPage != null)
			{
				bool unloadCurrent = CurrentPage.Unload();

				// Cancel
				if (!unloadCurrent)
					return;
			}

			// Kill all async wait handles
			foreach (System.Threading.AutoResetEvent handle in _asyncHandles.Values)
			{
				handle.Set();
			}
			_asyncHandles.Clear();

			// Get page XML data
			XmlElement pageData = e.AddedItems[0] as XmlElement;

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
				string error =
					String.Format("Failed to load page. \n\n {0} ({1})", ex.Message, ex.GetType().FullName);

				Exception innerEx = ex.InnerException;
				while (innerEx != null)
				{
					error += String.Format("\n {0} ({1})", innerEx.Message, innerEx.GetType().FullName);
					innerEx = innerEx.InnerException;
				}

				_currentPageViewer.Content = error;
			}
			
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

		public void HidePageContents(string message)
		{
			TextBlock msg = new TextBlock();
			msg.FontSize = 14;
			msg.Foreground = Brushes.Red;
			msg.FontWeight = FontWeights.Bold;
			msg.Text = message;
			_currentPageViewer.Content = msg;
		}

		public void HidePageContents(HideContentsReason reason)
		{
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
			HidePageContents(msgText);
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
