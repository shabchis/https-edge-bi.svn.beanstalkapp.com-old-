using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Threading;

namespace Easynet.Edge.UI.Client
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App: Application
	{
		public static PageBase CurrentPage
		{
			get
			{
				if (!InDesignMode)
					return VisualTree.GetChild<MainWindow>(Current.Windows[0]).CurrentPage;
				else
					return null;
			}
		}

		public static bool InDesignMode
		{
			get
			{
				return 
					Current.Windows.Count < 1 ||
					System.ComponentModel.DesignerProperties.GetIsInDesignMode(Current.Windows[0]);
			}
		}

		private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			CurrentPage.Window.HidePageContents(e.Exception);
			e.Handled = true;
		}
	}
}
