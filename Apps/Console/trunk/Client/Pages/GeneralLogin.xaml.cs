using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Easynet.Edge.UI.Data;
using Easynet.Edge.Core.Utilities;


namespace Easynet.Edge.UI.Client.Pages
{
	/// <summary>
	/// Interaction logic for GeneralLogin.xaml
	/// </summary>
	public partial class GeneralLogin: PageBase
	{
		public GeneralLogin()
		{
			InitializeComponent();
		}

		private void _loginButton_Click(object sender, RoutedEventArgs e)
		{
			_errorLabel.Visibility = Visibility.Hidden;

			if (_email.Text.Trim().Length < 1 || _password.Password.Length < 1)
			{
				MessageBoxError("Please enter both email and password", null);
				return;
			}

			string email = _email.Text.Trim();

			Window.AsyncOperation(delegate()
			{
				OltpProxy.SessionStart(email, _password.Password);
			},
			delegate(Exception ex)
			{
				LoginFailed(ex.Message);
				return false;
			},
			delegate()
			{
				if (_remember.IsChecked == true)
				{
					// Save the encrypted user ID to file
					App.Cookies[Const.Cookies.Login] = Encryptor.Encrypt(OltpProxy.CurrentUser.ID.ToString());
				}

				Window.LoginUser();
			});
		}

		void LoginFailed(string text)
		{
			_errorLabel.Text = text;
			_errorLabel.Visibility = Visibility.Visible;
		}
	}
}