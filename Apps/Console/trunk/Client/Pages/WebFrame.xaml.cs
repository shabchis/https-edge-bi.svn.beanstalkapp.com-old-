using System;
using System.Deployment.Application;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Windows;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.UI.Data;

namespace Easynet.Edge.UI.Client.Pages
{
    [AccountDependentPage]
    public partial class WebFramePage : PageBase
    {
		static string RootAddress;

		public WebFramePage()
        {
			if (RootAddress == null)
			{
				string rootAddressRelative = AppSettings.Get(typeof(WebFramePage), "RootAddress.Relative", false);
				string rootAddressAbsolute = AppSettings.Get(typeof(WebFramePage), "RootAddress.Absolute", false);
				if (ApplicationDeployment.IsNetworkDeployed)
					RootAddress = new Uri(System.Deployment.Application.ApplicationDeployment.CurrentDeployment.ActivationUri, rootAddressRelative).ToString();
				else
					RootAddress = rootAddressAbsolute;
			}

            InitializeComponent();

			Browser.ObjectForScripting = new ProgressIndicator(this);
			Browser.Navigating += new System.Windows.Navigation.NavigatingCancelEventHandler(Browser_Navigating);
			Browser.LoadCompleted += new System.Windows.Navigation.LoadCompletedEventHandler(Browser_LoadCompleted);
        }


		void Browser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
		{
			(Browser.ObjectForScripting as ProgressIndicator).ProgressIndicatorOn();
		}

		void Browser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			(Browser.ObjectForScripting as ProgressIndicator).ProgressIndicatorOff();
		}

        public override void OnAccountChanged()
        {
            Oltp.AccountRow currentAccount = Window.CurrentAccount;

            string lurl;
			if (!this.PageData.HasAttribute("Url"))
			{
				lurl = "about:No URL defined for this page.";
			}
			else
			{
				string pageUrl = this.PageData.GetAttribute("Url");
				lurl =
					(RootAddress != null && !pageUrl.Contains("://") ? RootAddress : null) +
					ExpandSymbols(this.PageData.GetAttribute("Url"));
			}

			Browser.Navigate(new Uri(lurl));

        }

		static Regex SymbolFinder = new Regex(@"\{[^}]+}", RegexOptions.IgnoreCase);
		static Regex EncryptFinder = new Regex(@"\[\[.+\]\]", RegexOptions.IgnoreCase);

		private string ExpandSymbols(string url)
		{
			string expanded = SymbolFinder.Replace(url, delegate(Match m)
			{
				string withoutBraces = m.Value.Substring(1, m.Value.Length - 2);
				string[] parts = withoutBraces.Split('.');
				switch (parts[0].ToLower())
				{
					case "account":
						return parts[1].ToLower() == "settings" ?
							Window.CurrentAccount.Settings[parts[2]] :
							Window.CurrentAccount[parts[1]].ToString();
					default:
						return null;
				};
			});

			string done = EncryptFinder.Replace(expanded, delegate(Match m)
			{
				return Encryptor.Encrypt(m.Value.Replace("[[", string.Empty).Replace("]]", string.Empty)); ;
			});

			return done;
		}
    }

	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	[ComVisible(true)]
	public class ProgressIndicator
	{
		internal WebFramePage Page;

		public ProgressIndicator(WebFramePage page)
		{
			Page = page;
		}

		public void ProgressIndicatorOn()
		{
			if (Page.Window.AsyncInProgress)
				return;

			Page.Browser.IsEnabled = false;
			Page.Window.AsyncProgressIndicatorOn();
		}

		public void ProgressIndicatorOff()
		{
			Page.Window.AsyncProgressIndicatorOff();
			Page.Browser.IsEnabled = true;
		}
	}


}
