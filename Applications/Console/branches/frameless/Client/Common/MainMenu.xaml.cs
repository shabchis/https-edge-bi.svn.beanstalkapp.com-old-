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
using System.Reflection;
using System.Windows.Controls.Primitives;
using System.Xml;
using Easynet.Edge.UI.Data;
using System.Data;
using Easynet.Edge.Core.Configuration;
using System.Deployment.Application;



namespace Easynet.Edge.UI.Client
{
	/// <summary>
	/// Categorized main menu, binds to XML.
	/// </summary>
	public partial class MainMenu: UserControl
	{
		#region Constructors
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		public MainMenu()
		{
			InitializeComponent();

			// Event handlers
			this.Loaded += new RoutedEventHandler(MainMenu_Loaded);
		}

		/*=========================*/
		#endregion

		#region WPF Routed Events
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent
		(
			"SelectionChanged",
			RoutingStrategy.Bubble,
			typeof(SelectionChangedEventHandler),
			typeof(MainMenu)
		);

		/// <summary>
		/// 
		/// </summary>
		public event SelectionChangedEventHandler SelectionChanged
		{
			add { AddHandler(SelectionChangedEvent, value); }
			remove { RemoveHandler(SelectionChangedEvent, value); }
		}

		public static readonly RoutedEvent OpenedEvent = EventManager.RegisterRoutedEvent
		(
			"Opened",
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(MainMenu)
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
			typeof(MainMenu)
		);
		
		public event RoutedEventHandler Closed
		{
			add { AddHandler(ClosedEvent, value); }
			remove { RemoveHandler(ClosedEvent, value); }
		}
		public bool IsOpen
		{
			get { return (bool)GetValue(IsOpenProperty); }
			set
			{
				bool wasOpen = IsOpen;
				SetValue(IsOpenProperty, value);

				if (wasOpen && !value)
					RaiseEvent(new RoutedEventArgs(ClosedEvent));
				else if (!wasOpen && value)
					RaiseEvent(new RoutedEventArgs(OpenedEvent));
			}
		}

		// Using a DependencyProperty as the backing store for IsOpen.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsOpenProperty =
			DependencyProperty.Register("IsOpen", typeof(bool), typeof(MainMenu), new UIPropertyMetadata(true));


		/*=========================*/
		#endregion

		#region Public Properties
		/*=========================*/

		public XmlDataProvider XmlProvider
		{
			get { return (this.Resources["MenuData"] as XmlDataProvider); }
		}

		/*=========================*/
		#endregion

		#region Internal methods
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		void MainMenu_Loaded(object sender, RoutedEventArgs e)
		{
			// Get the menu xml URL
			XmlDataProvider xmlProvider = this.XmlProvider;
			if (!ApplicationDeployment.IsNetworkDeployed)
			{
				string absolute = AppSettings.Get(this, "MenuXmlAddress.Absolute");
				xmlProvider.Source = new Uri(absolute);
			}
			else
			{
				string relative = AppSettings.Get(this, "MenuXmlAddress.Relative");
				xmlProvider.Source = new Uri(ApplicationDeployment.CurrentDeployment.ActivationUri, relative);
			}

			DeselectCollapse(true, true, null);
		}

		ListBox _currentListBox = null;
		object _currentItem = null;
		bool _raise = true;

		/// <summary>
		/// 
		/// </summary>
		void LayoutListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_raise)
				return;

			if (e.AddedItems.Count < 1)
				return;

			SelectionChangedEventArgs args = new SelectionChangedEventArgs(
				SelectionChangedEvent, e.RemovedItems, e.AddedItems);
			
			args.Handled = false;
			RaiseEvent(args);

			// "Handled" event means it was canceled
			if (args.Handled)
			{
				DeselectCollapse(false, true, _currentListBox);
				if (_currentListBox != null)
				{
					_raise = false;
					try { _currentListBox.SelectedItem = _currentItem; }
					finally { _raise = true; }
				}
			}
			else
			{
				DeselectCollapse(false, true, sender);
				_currentListBox = sender as ListBox;
				_currentItem = e.AddedItems[0];
			}
		}

		
		/// <summary>
		/// Called by an expander when it is expanded.
		/// </summary>
		void CollapseOthers(object sender, RoutedEventArgs e)
		{
			DeselectCollapse(true, false, sender); 
		}


		/// <summary>
		/// Iterates all menu sections in order to collapse and deselect any sub-sections.
		/// </summary>
		/// <param name="collapse">True to collapse sections.</param>
		/// <param name="deselect">True to deselect all section items in each section.</param>
		/// <param name="excludeObject">
		/// The object to exclude from any collapse/deselect operations.
		/// </param>
		void DeselectCollapse(bool collapse, bool deselect, object excludeObject)
		{
			// Get the corresponding expander/listbox matching excludeObject
			Expander currentExpander = excludeObject is Expander ? (Expander) excludeObject : null;
			ListBox currentListbox = excludeObject is ListBox ? (ListBox) excludeObject : null;

			// Get the related control (depending on type of excludeObject)
			if (currentExpander != null)
				currentListbox = currentExpander.Content as ListBox;
			else if (currentListbox != null)
				currentExpander = currentListbox.Parent as Expander;

			// Iterate all menu sections
			for(int i = 0; i < _menuSections.Items.Count; i++)
			{
				ListBoxItem section = _menuSections.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;

				if (section == null)
					continue;

				DataTemplate tpl = section.ContentTemplate;
				ContentPresenter cp = VisualTreeHelper.GetChild(section, 0) as ContentPresenter;
				Expander exp = tpl.FindName("_sectionExpander", cp) as Expander;

				if (exp == null)
					continue;
				
				// Collapse
				if (collapse && exp != currentExpander)
					exp.IsExpanded = false;

				if (!deselect)
					continue;

				// Deselect any items
				ListBox sectionItems = exp.Content as ListBox;

				if (sectionItems != null && sectionItems != currentListbox)
					sectionItems.SelectedIndex = -1;
				
			}
		}

		/// <summary>
		/// Hides the menu.
		/// </summary>
		private void _hideButton_Click(object sender, RoutedEventArgs e)
		{
			this.IsOpen = !_hideButton.IsChecked.Value;
		}

		/*=========================*/
		#endregion

		#region Public Properties
		/*=========================*/

		public void CollapseAll()
		{
			DeselectCollapse(true, true, null);
		}

		/*=========================*/
		#endregion

		/// <summary>
		/// Appplies user's permissions to the menu. If account is null, then permission is checked
		/// for at least one account per page. If a page has no account allowed for it, is is made invisible.
		/// If a section has no visible pages, the entire section is made invisible.
		/// 
		/// When account is not null, pages are enabled/disabled according to selected account.
		/// </summary>
		/// <param name="account"></param>
		public void ApplyPermissions(Oltp.AccountRow account)
        {
            for(int s = 0; s < _menuSections.Items.Count; s++)
            {
				XmlElement section = (XmlElement) _menuSections.Items[s];
                ListBoxItem sectionListItem = (ListBoxItem)_menuSections.ItemContainerGenerator.ContainerFromIndex(s);                
				XmlNodeList sectionPages = section.GetElementsByTagName("Page");
				ListBox pagesListBox = VisualTree.GetChild<ListBox>(sectionListItem);

				bool isSectionEnabled = false;
                for (int p = 0; p < sectionPages.Count; p++)
                {
                    XmlElement page = (XmlElement)sectionPages[p];
                   
                    bool isPageEnabled = MainWindow.Current.HasPermission(account, page);
                    ListBoxItem pageListItem = (ListBoxItem)pagesListBox.ItemContainerGenerator.ContainerFromIndex(p);

					if (account == null)
						pageListItem.Visibility = isPageEnabled ? Visibility.Visible : Visibility.Collapsed;
					else
						pageListItem.IsEnabled = isPageEnabled;

					isSectionEnabled = isSectionEnabled || isPageEnabled;
                }

				if (account == null)
					sectionListItem.Visibility = isSectionEnabled ? Visibility.Visible : Visibility.Collapsed;
            }
        }
	}
}
