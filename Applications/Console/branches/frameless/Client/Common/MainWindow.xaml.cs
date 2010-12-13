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
using System.Runtime.Remoting.Messaging;
using Easynet.Edge.Core.Utilities;
using System.Windows.Controls.Primitives;
using System.Deployment.Application;
using System.Collections.Specialized;
using System.Web;
using System.Net;
using Easynet.Edge.Core.Configuration;

namespace Easynet.Edge.UI.Client
{
	/// <summary>
	/// Main window of the Edge2 Manager.
	/// </summary>
	public partial class MainWindow: Page
	{
		#region Fields
		/*=========================*/
		public static string AssemblyAddressRoot;
		public static string AssemblyDownloadPath;

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

			if (AssemblyAddressRoot == null)
			{
				string rootAddressRelative = AppSettings.Get(typeof(MainWindow), "AssemblyDownloadRoot.Relative", false);
				string rootAddressAbsolute = AppSettings.Get(typeof(MainWindow), "AssemblyDownloadRoot.Absolute", false);
				if (ApplicationDeployment.IsNetworkDeployed)
					AssemblyAddressRoot = new Uri(System.Deployment.Application.ApplicationDeployment.CurrentDeployment.ActivationUri, rootAddressRelative).ToString();
				else
					AssemblyAddressRoot = rootAddressAbsolute;
			}
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
				HidePageContents("XBAP must be accessed via a web URL.");
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

			string menuItemParam = urlParams["page"];
			if (String.IsNullOrEmpty(menuItemParam))
			{
				HidePageContents("No page specified. Please select an item from the menu.");
				return;
			}
			int menuItemID;
			if (!int.TryParse(menuItemParam, out menuItemID))
			{
				HidePageContents("Invalid page specified. Please select an item from the menu.");
				return;
			}
			ApiMenuItem menuItem = null;

			// Get user settings
			AsyncOperation(delegate()
			{
				using (OltpProxy proxy = new OltpProxy())
				{
					_accountsTable = proxy.Service.Account_Get();
					_userPermissions = proxy.Service.User_GetAllPermissions();
					menuItem = proxy.Service.ApiMenuItem_Get(menuItemID);
				}
			},
			delegate(Exception ex)
			{
				MessageBoxError("Failed to load user settings.", ex);
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

				_currentAccount = (Oltp.AccountRow)rs[0];
				LoadPage(menuItem);
			});
		}

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

			string className;
			if (!menuItem.Metadata.TryGetValue("WPF-class", out className))
			{
				HidePageContents("This menu item cannot be loaded in WPF.");
				return;
			}

			// Kill all async wait handles
			foreach (System.Threading.AutoResetEvent handle in _asyncHandles.Values)
			{
				handle.Set();
			}
			_asyncHandles.Clear();

			// Try to get the target page type
			Type newPageType = Type.GetType(className, false);

			// This is performed after we load the target DLL
			Action displayPageAction = delegate()
			{
				if (newPageType == null)
				{
					HidePageContents("Could not load the requested page - class not found.");
					return;
				}

				try
				{
					CurrentPage = (PageBase)Activator.CreateInstance(newPageType);
				}
				catch (Exception ex)
				{
					CurrentPage = null;
					HidePageContents(ex);
				}

				CurrentPage.PageData = menuItem;
				_currentPageViewer.Content = CurrentPage;

				if (newPageType.GetCustomAttributes(typeof(AccountDependentPageAttribute), false).Length > 0)
				{
					if (CurrentAccount != null)
					{
						// Tell the page to load the account
						CurrentPage.OnAccountChanged();
					}
					else
					{
						// Tell the page to hide the account
						HidePageContents("You do not have permission to view this page for the selected account.");
					}
				}

				// Force garbage collection
				this.FloatingDialogContainer.Children.Clear();
				GC.Collect();
			};

			// Resolve the class reference
			if (newPageType == null)
			{
				string assemblyAddress = menuItem.Metadata["WPF-assembly"];
				if (String.IsNullOrEmpty(assemblyAddress))
				{
					HidePageContents("Could not load the requested page - DLL not specified.");
					return;
				}

				AsyncOperation(
					delegate()
					{
						Uri downloadUri = new Uri(new Uri(AssemblyAddressRoot), assemblyAddress);
						string downloadTarget = System.IO.Path.Combine(AssemblyDownloadPath, System.IO.Path.GetFileName(downloadUri.LocalPath));
						using (WebClient client = new WebClient())
						{
							client.DownloadFile(downloadUri, downloadTarget);
						}
					},
					HidePageContents,
					displayPageAction);

			}
			else
			{
				displayPageAction();
			}

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
							MessageBoxError("Operation failed.", ex);
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
								MessageBoxError("Operation failed.", innerEx);
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
							MessageBoxError("An error occured after the operation completed.", ex);
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

		#region Message box messages
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		/// <param name="excep"></param>
		public static void MessageBoxError(string message, Exception ex)
		{
			if (ex is TargetInvocationException)
				ex = ex.InnerException;

			MessageBox.Show(
				ex == null ?
					message :
					String.Format("{0}\n\n{1}\n\n({2})", message, ex.Message, ex.GetType().FullName),
				"Error",
				MessageBoxButton.OK, MessageBoxImage.Error
			);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dialog"></param>
		/// <returns></returns>
		public static bool MessageBoxPromptForCancel(FloatingDialog dialog)
		{
			DataRow row = dialog.Content as DataRow;
			if (row == null)
				return false;

			if (row.Table.GetChanges() == null)
				return false;

			MessageBoxResult result = MessageBox.Show(
				"Discard changes?",
				"Confirm",
				MessageBoxButton.OKCancel,
				MessageBoxImage.Warning,
				MessageBoxResult.Cancel);

			return result == MessageBoxResult.Cancel;
		}

		/*=========================*/
		#endregion
	
		#region Full page messages
		/*=========================*/

		public void HidePageContents(string message)
		{
			TextBlock msg = new TextBlock();
			msg.FontSize = 14;
			msg.Foreground = Brushes.Red;
			msg.FontWeight = FontWeights.Bold;
			msg.Text = message;
			_currentPageViewer.Content = msg;
		}

		public bool HidePageContents(Exception ex)
		{
			string error =
						String.Format("Failed to load page. \n\n {0} ({1})", ex.Message, ex.GetType().FullName);

			Exception innerEx = ex.InnerException;
			while (innerEx != null)
			{
				error += String.Format("\n {0} ({1})", innerEx.Message, innerEx.GetType().FullName);
				innerEx = innerEx.InnerException;
			}

			_currentPageViewer.Content = error;
			return false;
		}

		public void RestorePageContents()
		{
			_currentPageViewer.Content = CurrentPage;
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

	[global::System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class AccountDependentPageAttribute : Attribute
	{
		// This is a positional argument
		public AccountDependentPageAttribute()
		{
		}
	}

}
