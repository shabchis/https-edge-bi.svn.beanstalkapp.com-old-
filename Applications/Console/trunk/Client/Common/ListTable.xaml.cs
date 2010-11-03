using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Data;
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
using Easynet.Edge.UI.Client;
using System.Windows.Controls.Primitives;
using System.Globalization;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Easynet.Edge.UI.Client
{
	/// <summary>
	/// Interaction logic for ListTable.xaml
	/// </summary>
	public partial class ListTable: UserControl
	{
		#region Fields
		/*=========================*/

		bool _drag_InProgress = false;
		int _drag_itemIndex = -1;
		ListViewItem _drag_itemContainer = null;
		ListViewItem _drag_lastDropTarget = null;
		NotifyCollectionChangedEventHandler _viewRefresher;
		INotifyCollectionChanged _collection = null;

		/*=========================*/
		#endregion

		#region General
		/*=========================*/
		
		/// <summary>
		/// 
		/// </summary>
		public ListTable()
		{
			InitializeComponent();
			
			// Hack
			(this.Resources["CornerRadiusConverter"] as ListTableColumnHeaderCornerRadiusConverter).ParentListView = _listView;

			//this.Loaded += new RoutedEventHandler(ListTable_Loaded);
			//this.Unloaded += new RoutedEventHandler(ListTable_Unloaded);
			
			// For alternate row refreshing
			//TypeDescriptor.GetProperties(this.ListView)["ItemsSource"].AddValueChanged(this.ListView, new EventHandler(ListView_ItemsSourceChanged));
			//_viewRefresher = new NotifyCollectionChangedEventHandler(ListView_CollectionChanged);
		}

		/// <summary>
		/// 
		/// </summary>
		void ListTable_Loaded(object sender, RoutedEventArgs e)
		{
		}

		void ListTable_Unloaded(object sender, RoutedEventArgs e)
		{
			this.ListView.ItemsSource = null;
		}

		#region Refresh alternate rows view
		//..............................

		/*
		void ListView_ItemsSourceChanged(object sender, EventArgs e)
		{
			if (_collection != null)
				_collection.CollectionChanged -= _viewRefresher;

			_collection = this.ListView.ItemsSource as INotifyCollectionChanged;
			
			if (_collection != null)
				_collection.CollectionChanged += _viewRefresher;
		}

		void ListView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// Don't care about replace
			if (e.Action == NotifyCollectionChangedAction.Replace)
				return;

			ICollectionView view = CollectionViewSource.GetDefaultView(this.ListView.ItemsSource);
			if (view != null)
				view.Refresh();
		}
		*/
		//..............................
		#endregion

		/// <summary>
		/// 
		/// </summary>
		public ListView ListView
		{
			get
			{
				return _listView;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		public ViewBase View
		{
			get
			{
				return _listView.View;
			}
			set
			{
				_listView.View = value;
			}
		}

		/*=========================*/
		#endregion

		#region WPF Dependency Properties
		/*=========================*/


		public static bool GetAutoSize(DependencyObject obj)
		{
			return (bool) obj.GetValue(AutoSizeProperty);
		}

		public static void SetAutoSize(DependencyObject obj, bool value)
		{
			obj.SetValue(AutoSizeProperty, value);
		}

		// Using a DependencyProperty as the backing store for AutoSize.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty AutoSizeProperty =
			DependencyProperty.RegisterAttached("AutoSize", typeof(bool), typeof(ListTable), new UIPropertyMetadata(false));



		public SelectionMode SelectionMode
		{
			get { return (SelectionMode) GetValue(SelectionModeProperty); }
			set { SetValue(SelectionModeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for SelectionMode.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SelectionModeProperty = 
    DependencyProperty.Register("SelectionMode", typeof(SelectionMode), typeof(ListTable), new UIPropertyMetadata(SelectionMode.Single));




		/*=========================*/
		#endregion

		#region Drag&Drop
		/*=========================*/

		public enum ListTableDragState
		{
			None,
			Above,
			Below
		}


		/// <summary>
		/// Drag state
		/// </summary>
		public static ListTableDragState GetDragOverState(DependencyObject obj)
		{
			return (ListTableDragState)obj.GetValue(DragOverStateProperty);
		}
		public static void SetDragOverState(DependencyObject obj, ListTableDragState value)
		{
			obj.SetValue(DragOverStateProperty, value);
		}
		public static readonly DependencyProperty DragOverStateProperty =
			DependencyProperty.RegisterAttached("DragOverState", typeof(ListTableDragState), typeof(ListTable));



		/// <summary>
		/// Is drag handle
		/// </summary>
		public static bool GetIsDragHandle(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsDragHandleProperty);
		}

		public static void SetIsDragHandle(DependencyObject obj, bool value)
		{
			obj.SetValue(IsDragHandleProperty, value);
		}

		// Using a DependencyProperty as the backing store for IsDragHandle.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsDragHandleProperty =
			DependencyProperty.RegisterAttached("IsDragHandle", typeof(bool), typeof(ListTable), new UIPropertyMetadata(false));

		/// <summary>
		/// 
		/// </summary>
		public static readonly RoutedEvent ItemDraggedEvent = EventManager.RegisterRoutedEvent
		(
			"ItemDragged",
			RoutingStrategy.Bubble,
			typeof(ItemDraggedRoutedEventHandler),
			typeof(ListTable)
		);

		/// <summary>
		/// 
		/// </summary>
		public event ItemDraggedRoutedEventHandler ItemDragged
		{
			add { AddHandler(ItemDraggedEvent, value); }
			remove { RemoveHandler(ItemDraggedEvent, value); }
		}

		/// <summary>
		/// 
		/// </summary>
		void _listView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (_drag_InProgress)
				return;

			// Make sure this element is defined as a drag handle
			DependencyObject hitElement = (DependencyObject) _listView.InputHitTest(Mouse.GetPosition(_listView));
			if (hitElement == null || !((bool)hitElement.GetValue(IsDragHandleProperty)))
				return;

			// Get the pressed ListViewItem
			ListViewItem pressedItem = _listView_GetItemUnderCursor(null);

			// No item was pressed, so exit
			if (pressedItem == null)
				return;

			// Store the item to drag
			_drag_itemIndex = _listView.ItemContainerGenerator.IndexFromContainer(pressedItem);
			_drag_itemContainer = pressedItem;
			
			// Init the drag
			_drag_InProgress = true;
		}


		/// <summary>
		/// 
		/// </summary>
		void _listView_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (!_drag_InProgress)
				return;

			e.Handled = true;

			// Keep our item selected
			if (_listView.SelectedIndex != _drag_itemIndex)
				_listView.SelectedIndex = _drag_itemIndex;

			// Process the drop
			DragDropEffects dragResult = DragDrop.DoDragDrop(_listView, _drag_itemContainer, DragDropEffects.Move);
			//if (dragResult == DragDropEffects.None)
			//	return;

			_drag_InProgress = false;
		}


		void _listView_DragEnter(object sender, DragEventArgs e)
		{
			e.Effects = DragDropEffects.Move;
		}


		/// <summary>
		/// 
		/// </summary>
		void _listView_DragOver(object sender, DragEventArgs e)
		{
			if (!_drag_InProgress)
			{
				// No target, so exit
				e.Effects = DragDropEffects.None;
				return;
			}

			e.Handled = true;

			// Get the drop target
			ListViewItem item = _listView_GetItemUnderCursor(e);
			if (item == null)
			{
				// No target, so exit
				e.Effects = DragDropEffects.None;
				return;
			}

			// Don't do anything about currently dragged item
			if (item == _drag_itemContainer)
			{
				e.Effects = DragDropEffects.None;
				return;
			}

			// Clear the previous drop indicator
			if (_drag_lastDropTarget != null && _drag_lastDropTarget != item)
				_drag_lastDropTarget.ClearValue(DragOverStateProperty);

			// Apply a drop indicator according to the mouse position
			Point p = e.GetPosition(item);
			item.SetValue(DragOverStateProperty,  p.Y > item.ActualHeight/2 ? ListTableDragState.Below : ListTableDragState.Above);
			item.BringIntoView();

			// Remember for later (so we can clear the target)
			_drag_lastDropTarget = item;
		}

		/// <summary>
		/// 
		/// </summary>
		void _listView_DragLeave(object sender, DragEventArgs e)
		{
			e.Handled = true;

			// Clear the previous drop indicator
			if (_drag_lastDropTarget != null)
				_drag_lastDropTarget.ClearValue(DragOverStateProperty);
			
			_drag_lastDropTarget = null;
		}

		/// <summary>
		/// 
		/// </summary>
		void _listView_Drop(object sender, DragEventArgs e)
		{
			e.Handled = true;

			// Clear the previous drop indicator
			if (_drag_lastDropTarget == null)
				return;

			// Get the last target state
			ListTableDragState dragState = (ListTableDragState) _drag_lastDropTarget.GetValue(DragOverStateProperty);
			_drag_lastDropTarget.ClearValue(DragOverStateProperty);

			int targetIndex = this.ListView.ItemContainerGenerator.IndexFromContainer(_drag_lastDropTarget)
				+ (dragState == ListTableDragState.Below ? 1 : 0);

			RaiseEvent(new ItemDraggedRoutedEventArgs(
				this,
				_drag_itemContainer,
				_drag_itemIndex,
				targetIndex
				));

			_drag_lastDropTarget = null;
		}

		/// <summary>
		/// 
		/// </summary>
		public ListViewItem GetParentListViewItem(FrameworkElement control)
		{
			FrameworkElement currentElement = control;
			ListViewItem item = null;

			// Go up the hierarchy till we find the ListViewItem
			while (currentElement != null)
			{
				// Try to get a templated parent
				FrameworkElement parent = (FrameworkElement)currentElement.TemplatedParent;

				// Is a parent defined?
				if (parent != null)
				{
					// It's defined, see if it's a list item
					if (parent is ListViewItem)
					{
						// Yes it is!
						item = (ListViewItem)parent;
						break;
					}
					else
					{
						// It isn't, continue up from this element
						currentElement = parent;
					}
				}
				else
				{
					// No parent defined, continue with the direct parent
					currentElement = (FrameworkElement)currentElement.Parent;
				}
			}

			return item;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dragArgs">Drag event args to use for calculating a position, null to use Mouse.GetPosition</param>
		/// <returns></returns>
		ListViewItem _listView_GetItemUnderCursor(DragEventArgs dragArgs)
		{
			Point p = dragArgs == null ? Mouse.GetPosition(_listView) : dragArgs.GetPosition(_listView);

			// Get the element that was actually clicked on
			FrameworkElement hitElement = (FrameworkElement)_listView.InputHitTest(p);

			// Go up the hierarchy till we find the ListViewItem
			ListViewItem item = GetParentListViewItem(hitElement);

			return item;
		}


		/*=========================*/
		#endregion

		#region Parent/Child grouping
		/*=========================*/

		/// <summary>
		/// Gets the types of items that will be considered child items.
		/// </summary>
		public Type[] ChildItemTypes
		{
			get { return (Type[])GetValue(ChildItemTypesProperty); }
			set { SetValue(ChildItemTypesProperty, value); }
		}
		public static readonly DependencyProperty ChildItemTypesProperty =
			DependencyProperty.Register("ChildItemTypes", typeof(Type[]), typeof(ListTable), new UIPropertyMetadata(null));


		/// <summary>
		/// 
		/// </summary>
		public static readonly RoutedEvent GroupExpandedEvent = EventManager.RegisterRoutedEvent
		(
			"GroupExpanded",
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(ListTable)
		);

		/// <summary>
		/// 
		/// </summary>
		public event RoutedEventHandler GroupExpanded
		{
			add { AddHandler(GroupExpandedEvent, value); }
			remove { RemoveHandler(GroupExpandedEvent, value); }
		}


		/// <summary>
		/// 
		/// </summary>
		public static readonly RoutedEvent GroupCollapsedEvent = EventManager.RegisterRoutedEvent
		(
			"GroupCollapsed",
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(ListTable)
		);

		/// <summary>
		/// 
		/// </summary>
		public event RoutedEventHandler GroupCollapsed
		{
			add { AddHandler(GroupCollapsedEvent, value); }
			remove { RemoveHandler(GroupCollapsedEvent, value); }
		}

		/// <summary>
		/// 
		/// </summary>
		void ToggleButton_Changed(object sender, RoutedEventArgs e)
		{
			// Raise only if the event is from a toggle button
			if (!(e.OriginalSource is ToggleButton))
				return;

			// Get the item
			ListViewItem item = GetParentListViewItem(e.OriginalSource as ToggleButton);
			if (item == null)
				return;

			// Raise the event as coming from the item
			RaiseEvent(new RoutedEventArgs(
				e.RoutedEvent == ToggleButton.CheckedEvent ? GroupExpandedEvent : GroupCollapsedEvent,
				item)
			);
		}


		/*=========================*/
		#endregion

		#region Auto-width columns
		/*=========================*/

		public void UpdateColumnWidths()
		{
			_listView_SizeChanged(_listView, null);
		}

		/// <summary>
		/// 
		/// </summary>
		private void _listView_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			// Only relevant to grid views
			if (!(_listView.View is GridView))
				return;

			GridView grid = _listView.View as GridView;

			// Only relevant for width
			if (e != null && !e.WidthChanged)
				return;

			// Get all AutoSize columns
			List<GridViewColumn> columns = new List<GridViewColumn>();
			double specifiedWidth = 0;
			foreach (GridViewColumn col in grid.Columns)
			{
				if ((bool)col.GetValue(AutoSizeProperty))
					columns.Add(col);
				else
					specifiedWidth += col.ActualWidth;
			}

			// Calculate
			//ScrollViewer scroll = (ScrollViewer) VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(_listView, 0), 0);
			//ItemsPresenter iPresenter = scroll.Content as ItemsPresenter;
			ItemsPresenter iPresenter = VisualTree.GetChild<ItemsPresenter>(_listView);
			ScrollBar scrollBar = VisualTree.GetChild<ScrollBar>(_listView);

			double newWidth = (iPresenter.ActualWidth - scrollBar.Width - specifiedWidth - 20) / columns.Count;

			// Give them a fair share of the remaining space
			foreach (GridViewColumn col in columns)
			{
				if (newWidth >= 0)
					col.Width = newWidth;
			}
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if(e.Property == VirtualizingStackPanel.IsVirtualizingProperty)
				_listView.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, e.NewValue);
		}

		/*=========================*/
		#endregion
	}

	/// <summary>
	/// 
	/// </summary>
	public class ItemDraggedRoutedEventArgs: RoutedEventArgs
	{
		ListViewItem _item;
		int _sourceIndex;
		int _targetIndex;

		public ItemDraggedRoutedEventArgs(ListTable listTable, ListViewItem item, int sourceIndex, int targetIndex):
			base(ListTable.ItemDraggedEvent, listTable)
		{
			_item = item;
			_sourceIndex = sourceIndex;
			_targetIndex = targetIndex;
		}

		public ListViewItem Item
		{
			get { return _item; }
		}

		public int SourceIndex
		{
			get { return _sourceIndex; }
		}

		public int TargetIndex
		{
			get { return _targetIndex; }
		}	
	}

	/// <summary>
	/// 
	/// </summary>
	public delegate void ItemDraggedRoutedEventHandler(object sender, ItemDraggedRoutedEventArgs e);

	/// <summary>
	/// 
	/// </summary>
	public class ListTableItemContainerStyleSelector : StyleSelector
	{
		public override Style SelectStyle(object item, DependencyObject container)
		{
			ListTable tbl = VisualTree.GetParent<ListTable>(container);

			// buggy
			//int index = tbl.ListView.ItemContainerGenerator.IndexFromContainer(container);
			//string alternate = index % 2 == 0 ? string.Empty : "-alternate";
			
			string alternate = null;

			if (tbl.ChildItemTypes != null && tbl.ChildItemTypes.Contains(item.GetType()))
				return tbl.ListView.FindResource(String.Format("ChildItemStyle{0}", alternate)) as Style;
			else
				return tbl.ListView.FindResource(String.Format("ParentItemStyle{0}", alternate)) as Style;
		}
	}

	[ValueConversion(typeof(GridViewColumn), typeof(CornerRadius))]
	public class ListTableColumnHeaderCornerRadiusConverter : IValueConverter
	{
		public int RadiusLeft { get; set; }
		public int RadiusRight { get; set; }
		public ListView ParentListView { get; set; }

		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			GridViewColumn column = value as GridViewColumn;
			GridView gridView = ParentListView.View as GridView;

			if (column == null)
				return new CornerRadius(0, RadiusRight, 0, 0);

			else if (gridView.Columns.IndexOf(column) == 0)
				return new CornerRadius(RadiusLeft, 0, 0, 0);

			else
				return new CornerRadius(0);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}