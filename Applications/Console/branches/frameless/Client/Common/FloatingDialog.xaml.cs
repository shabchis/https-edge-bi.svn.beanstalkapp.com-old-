using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Easynet.Edge.UI.Client;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Media.Animation;

namespace Easynet.Edge.UI.Client
{
	/// <summary>
	/// Used for event handlers
	/// </summary>
	public partial class FloatingDialogResources: ResourceDictionary
	{
	}

	/// <summary>
	/// 
	/// </summary>
	public class FloatingDialog: ContentControl
	{
		#region Fields
		/*=========================*/

		private class BEData
		{
			public BEData(BindingExpression exp, DependencyObject obj, DependencyProperty prp)
			{
				Exp = exp;
				Obj = obj;
				Prp = prp;
			}

			public BindingExpression Exp;
			public DependencyObject Obj;
			public DependencyProperty Prp;
			public bool Changed = false;
			public bool BatchApply = false;
		}

		List<BEData> _bindings = null;
		List<DependencyObject> _scannedControls;
		Button _buttonOk;
		Button _buttonCancel;
		Button _buttonApply;

		/*=========================*/
		#endregion

		#region WPF Dependency Properties
		/*=========================*/

		public string Title
		{
			get { return (string) GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public static readonly DependencyProperty TitleProperty = 
			DependencyProperty.Register("Title", typeof(string), typeof(FloatingDialog));


		public string TitleTooltip
		{
			get { return (string) GetValue(TitleTooltipProperty); }
			set { SetValue(TitleTooltipProperty, value); }
		}

		public static readonly DependencyProperty TitleTooltipProperty = 
			DependencyProperty.Register("TitleTooltip", typeof(string), typeof(FloatingDialog));



		public object TargetContent
		{
			get { return (object) GetValue(TargetContentProperty); }
			set { SetValue(TargetContentProperty, value); }
		}

		public static readonly DependencyProperty TargetContentProperty = 
			DependencyProperty.Register("TargetContent", typeof(object), typeof(FloatingDialog));



		public bool IsOpen
		{
			get { return (bool) GetValue(IsOpenProperty); }
			set
			{
				SetValue(IsOpenProperty, value);

				if (value == true)
				{
					this.Visibility = Visibility.Visible;
					App.CurrentPage.Window.FloatingDialogMask.Visibility = Visibility.Visible;

					// Play a fade in animation
					this.BeginAnimation(OpacityProperty, NewFadeAnim(1.0, null), HandoffBehavior.SnapshotAndReplace);
					//App.CurrentPage.Window.FloatingDialogMask.BeginAnimation(OpacityProperty, NewFadeAnim(0.45, null), HandoffBehavior.SnapshotAndReplace);

					// Set keyboard
					Keyboard.Focus(this);
				}
				else
				{
					App.CurrentPage.Window.FloatingDialogMask.Visibility = Visibility.Collapsed;

					// Play a fade out animation
					this.BeginAnimation(OpacityProperty, NewFadeAnim(0, new EventHandler(FadeOutAnim_CompletedDialog)), HandoffBehavior.SnapshotAndReplace);
					//App.CurrentPage.Window.FloatingDialogMask.BeginAnimation(OpacityProperty, NewFadeAnim(0, new EventHandler(FadeOutAnim_CompletedMask)), HandoffBehavior.SnapshotAndReplace);

					// Clear any attached metadata
					this.Tag = null;
				}

				_buttonOk.IsEnabled = value;
				_buttonApply.IsEnabled = value;
				_buttonCancel.IsEnabled = value;
			}
		}


		public static readonly DependencyProperty IsOpenProperty = 
		    DependencyProperty.Register("IsOpen", typeof(bool), typeof(FloatingDialog), new UIPropertyMetadata(false));




		public Visibility ApplyButtonVisibility
		{
			get { return (Visibility) GetValue(ApplyButtonVisibilityProperty); }
			set { SetValue(ApplyButtonVisibilityProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ShowApplyButton.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ApplyButtonVisibilityProperty = 
			DependencyProperty.Register("ApplyButtonVisibility", typeof(Visibility), typeof(FloatingDialog), new UIPropertyMetadata(Visibility.Visible));

		public bool IsBatch
		{
			get { return this.TargetContent is IEnumerable; }
		}

		/*=========================*/
		#endregion

		#region WPF Attached Properties
		/*=========================*/

		public static string GetDialogFields(DependencyObject obj)
		{
			return (string) obj.GetValue(DialogFieldsProperty);
		}

		public static void SetDialogFields(DependencyObject obj, string value)
		{
			obj.SetValue(DialogFieldsProperty, value);
		}

		// Using a DependencyProperty as the backing store for DialogFields.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty DialogFieldsProperty =
		    DependencyProperty.RegisterAttached("DialogFields", typeof(string), typeof(FloatingDialog));



		public static bool GetReadOnlyField(DependencyObject obj)
		{
			return (bool)obj.GetValue(ReadOnlyFieldProperty);
		}

		public static void SetReadOnlyField(DependencyObject obj, bool value)
		{
			obj.SetValue(ReadOnlyFieldProperty, value);
		}

		// Using a DependencyProperty as the backing store for ReadOnlyField.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ReadOnlyFieldProperty =
			DependencyProperty.RegisterAttached("ReadOnlyField", typeof(bool), typeof(FloatingDialog), new UIPropertyMetadata(false));



		/*=========================*/
		#endregion

		#region WPF Routed Events
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		public static readonly RoutedEvent ApplyingChangesEvent = EventManager.RegisterRoutedEvent
		(
			"ApplyingChanges",
			RoutingStrategy.Bubble,
			typeof(ContinueRoutedEventHandler),
			typeof(FloatingDialog)
		);

		/// <summary>
		/// 
		/// </summary>
		public event ContinueRoutedEventHandler ApplyingChanges
		{
			add { AddHandler(ApplyingChangesEvent, value); }
			remove { RemoveHandler(ApplyingChangesEvent, value); }
		}

		/// <summary>
		/// 
		/// </summary>
		public static readonly RoutedEvent AppliedChangesEvent = EventManager.RegisterRoutedEvent
		(
			"AppliedChanges",
			RoutingStrategy.Bubble,
			typeof(ContinueRoutedEventHandler),
			typeof(FloatingDialog)
		);

		/// <summary>
		/// 
		/// </summary>
		public event ContinueRoutedEventHandler AppliedChanges
		{
			add { AddHandler(AppliedChangesEvent, value); }
			remove { RemoveHandler(AppliedChangesEvent, value); }
		}

		/// <summary>
		/// 
		/// </summary>
		public static readonly RoutedEvent ClosingEvent = EventManager.RegisterRoutedEvent
		(
			"Closing",
			RoutingStrategy.Bubble,
			typeof(ContinueRoutedEventHandler),
			typeof(FloatingDialog)
		);

		/// <summary>
		/// 
		/// </summary>
		public event ContinueRoutedEventHandler Closing
		{
			add { AddHandler(ClosingEvent, value); }
			remove { RemoveHandler(ClosingEvent, value); }
		}


		/*=========================*/
		#endregion

		#region Internal Methods
		/*=========================*/

		public FloatingDialog()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
			this.Loaded += new RoutedEventHandler(FloatingDialog_Loaded);
		}

		void FloatingDialog_Loaded(object sender, RoutedEventArgs e)
		{
			if (App.InDesignMode)
				return;

			VisualTree.GetParent<Grid>(this).Children.Remove(this);
			App.CurrentPage.Window.FloatingDialogContainer.Children.Add(this);
		}

		/// <summary>
		/// 
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// Apply event handlers if available
			_buttonOk = VisualTree.GetChild<Button>(this, "_buttonOK");
			_buttonCancel = VisualTree.GetChild<Button>(this, "_buttonCancel");
			_buttonApply = VisualTree.GetChild<Button>(this, "_buttonApply");

			if (_buttonOk != null)
				_buttonOk.Click += new RoutedEventHandler(buttonOK_Click);
			if (_buttonCancel != null)
				_buttonCancel.Click += new RoutedEventHandler(buttonCancel_Click);
			if (_buttonApply != null)
				_buttonApply.Click += new RoutedEventHandler(buttonApply_Click);

			if (_bindings != null && _bindings.Count == 0)
				GetDialogFieldBindings();
		}

		void Storyboard_Completed(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		protected override void OnContentTemplateChanged(DataTemplate oldContentTemplate, DataTemplate newContentTemplate)
		{
			base.OnContentTemplateChanged(oldContentTemplate, newContentTemplate);

			_bindings = new List<BEData>();
			GetDialogFieldBindings();
		}

		protected override void OnContentChanged(object oldContent, object newContent)
		{
			base.OnContentChanged(oldContent, newContent);

			if (_bindings != null && _bindings.Count == 0)
				GetDialogFieldBindings();
		}


		/// <summary>
		/// Gets the dialog field bindings starting from the content presenter
		/// </summary>
		void GetDialogFieldBindings()
		{
			ContentPresenter contentPresenter = VisualTree.GetChild<ContentPresenter>(this, "_contentPresenter");

			// Get the bindings when a template is applied
			if (contentPresenter == null)
				return;

			_scannedControls = new List<DependencyObject>();

			// Recursively find all bindings currently available
			GetDialogFieldBindings(contentPresenter);
		}
 
		/// <summary>
		/// Recursively finds any binding expressions marked with the DialogFields attached property.
		/// </summary>
		/// <param name="parent"></param>
		void GetDialogFieldBindings(DependencyObject parent)
		{
			if (parent == null)
				return;

			int childCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < childCount; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(parent, i);
				
				// Disregard anything that isn't an element
				if (!(child is FrameworkElement))
					continue;

				if (!_scannedControls.Contains(child))
				{
					_scannedControls.Add(child);

					// Get the list of bindings
					object bindingTargets = child.GetValue(DialogFieldsProperty);
					if (bindingTargets is string)
					{
						// Split the string into names
						string[] properties = (bindingTargets as string).Split(',');
						foreach (string property in properties)
						{
							DependencyProperty dependencyProp =
								GetDependencyProperty(child.GetType(), property + "Property");

							if (dependencyProp == null)
								continue;

							// If the property is valid, retrieve the attached binding expression and add it
							BindingExpression exp = (child as FrameworkElement).GetBindingExpression(dependencyProp);
							(child as FrameworkElement).SourceUpdated += new EventHandler<DataTransferEventArgs>(FloatingDialog_SourceUpdated);
							var bedata = new BEData(exp, child, dependencyProp);
							_bindings.Add(bedata);
						}

						// if (life.IsABitch)
						//		this.Commit("suicide");
					}

					// Bind to tabcontrol
					if (child is TabControl)
					{
						foreach (TabItem tab in ((TabControl) child).Items)
							tab.GotFocus += new RoutedEventHandler(tab_GotFocus);
					}
				}

				GetDialogFieldBindings(child);
			}
		}

		// Registers any controls that have changed, so that when ApplyBindingsToItems is called, only updated controls are applied
		void FloatingDialog_SourceUpdated(object sender, DataTransferEventArgs e)
		{
			FrameworkElement control = (FrameworkElement)sender;
			BindingExpression exp = control.GetBindingExpression(e.Property);

			// Find the binding data and mark it as changed
			BEData binding = _bindings.Find(bdata => bdata.Exp == exp);
			if (binding != null)
				binding.Changed = true;
		}

		void tab_GotFocus(object sender, RoutedEventArgs e)
		{
			GetDialogFieldBindings((sender as TabItem).Parent);
		}

		DependencyProperty GetDependencyProperty(Type t, string fieldName)
		{
			// Get the dependency for the property from type
			while (t.IsSubclassOf(typeof(DependencyObject)))
			{
				MemberInfo[] found = t.GetMember(fieldName);

				if (found.Length > 0 && found[0] is FieldInfo)
				{
					object dependencyProp = (found[0] as FieldInfo).GetValue(null);
					if (dependencyProp is DependencyProperty)
						return dependencyProp as DependencyProperty;
				}

				t = t.BaseType;
			}

			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		void buttonOK_Click(object sender, RoutedEventArgs e)
		{
			BeginApplyChanges(true);
		}

		/// <summary>
		/// 
		/// </summary>
		void buttonApply_Click(object sender, RoutedEventArgs e)
		{
			BeginApplyChanges(false);
		}

		/// <summary>
		/// 
		/// </summary>
		void buttonCancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close(CloseReason.Cancel);
		}

		void FadeOutAnim_CompletedDialog(object sender, EventArgs e)
		{
			this.Visibility = Visibility.Hidden;
		}

		void FadeOutAnim_CompletedMask(object sender, EventArgs e)
		{
			App.CurrentPage.Window.FloatingDialogMask.Visibility = Visibility.Collapsed;
		}

		DoubleAnimation NewFadeAnim(double to, EventHandler onComplete)
		{
			DoubleAnimation anim = new DoubleAnimation();
			anim.To = to;
			anim.Duration = new Duration(TimeSpan.Parse("0:0:0.2"));
			if (onComplete != null)
				anim.Completed += onComplete;

			return anim;
		}

		protected virtual void OnApplyingChanges(CancelRoutedEventArgs args)
		{
			if (!args.Cancel)
				RaiseEvent(args);
		}

		protected virtual void OnAppliedChanges(CancelRoutedEventArgs args)
		{
			if (!args.Cancel)
				RaiseEvent(args);
		}

		protected virtual void OnClosing(CancelRoutedEventArgs args)
		{
			if (!args.Cancel)
				RaiseEvent(args);
		}

		/*=========================*/
		#endregion

		#region Public Methods
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tempContent">
		///		A version (copy) of the object that the input controls will directly affect, based on bindings in the ContentTemplate.
		///		This will be applied to the Content property and can be used in the ApplyingChanges event
		///		handler to validate the changes.
		///	</param>
		/// <param name="targetContent">
		///		The object to which changes will be applied when ApplyChanges is called.
		/// </param>
		public void	BeginEdit(object tempContent, object targetContent)
		{
			if (this.IsOpen)
				throw new InvalidOperationException("Dialog is already open");

			//OnBeforeBeginEdit(tempContent, targetContent);

			// Don't mark as changed while applying changes
			SetValue(TargetContentProperty, targetContent);
			this.Content = tempContent;

			foreach(BEData bedata in _bindings)
			{
				// In batch mode, attach a checkbox to this control for marking it as 'please override'
				if (bedata.Obj is Control && ((Control)bedata.Obj).Parent is StackPanel && !GetReadOnlyField(bedata.Obj))
				{
					Control control = (Control)bedata.Obj;
					StackPanel stackPanel = (StackPanel)control.Parent;
					CheckBox checkbox = VisualTree.GetChild<CheckBox>(stackPanel, "__batch__cb");

					if (checkbox == null && this.IsBatch)
					{
						checkbox = new CheckBox();
						checkbox.Name = "__batch__cb";
						checkbox.IsChecked = false;
						Action<object, RoutedEventArgs> del = delegate(object sender, RoutedEventArgs e)
						{
							control.IsEnabled = checkbox.IsChecked.Value;
							bedata.BatchApply = control.IsEnabled;
						};

						checkbox.Checked += new RoutedEventHandler(del);
						checkbox.Unchecked += new RoutedEventHandler(del);
						stackPanel.Children.Add(checkbox);
					}

					if (checkbox != null)
					{
						control.IsEnabled = !IsBatch;
						checkbox.Visibility = IsBatch ? Visibility.Visible : Visibility.Collapsed;
						checkbox.IsChecked = false;
					}
				}
			}

			IsOpen = true;
		}

		/// <summary>
		///		The object to which changes will be applied when ApplyChanges is called.
		/// </summary>
		/// <param name="targetContent"></param>
		public void BeginEdit(object targetContent)
		{
			// We're really setting tempContent
			BeginEdit(targetContent, null);
		}

		/// <summary>
		/// 
		/// </summary>
		public void BeginApplyChanges(bool closeDialog)
		{
			if (!this.IsOpen)
				throw new InvalidOperationException("Dialog is not open");
			
			// Update source because the binding might not have been activated, so change focus to trigger it
			this.Focus();

			// Validations
			List<string> errors  = new List<string>();
			if (_bindings != null)
			{
				foreach (BEData bdata in _bindings)
				{
					if (bdata.Exp.HasError)
					{
						errors.Add(bdata.Exp.ValidationError.ErrorContent as string);
					}
					else if (bdata.Exp.ParentBinding.ValidationRules.Count > 0)
					{
						foreach (ValidationRule rule in bdata.Exp.ParentBinding.ValidationRules)
						{
							// Validate the binding again (just in case)
							ValidationResult result = rule.Validate(bdata.Obj.GetValue(bdata.Prp), null);

							if (!result.IsValid)
							{
								errors.Add(result.ErrorContent as string);

								// Trigger a validation error
								bdata.Exp.UpdateSource();
								break;
							}
						}
					}
				}
			}

			// Fail on any validation error
			if (errors.Count > 0)
			{
				string output = string.Empty;
				foreach (string error in errors)
				{
					if (!string.IsNullOrEmpty(error))
						output += error + "\n";
				}
				MessageBox.Show(output, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			// Raise the before event, this puts the DataRow in edit mode
			CancelRoutedEventArgs bargs = new CancelRoutedEventArgs();
			bargs.RoutedEvent = ApplyingChangesEvent;
			bargs.CloseDialog = closeDialog;
			bargs.Source = this;

			OnApplyingChanges(bargs);
		}

		public void EndApplyChanges(CancelRoutedEventArgs bargs)
		{
			if (bargs.Cancel)
				return;

			if (bargs.Skip)
			{
				if (bargs.CloseDialog)
					this.Close();

				return;
			}

			// Update final content, if it is null it means it's already been updated
			if (this.TargetContent != null && this._bindings != null)
			{
				// If target is a list, apply changes to all of them
				IEnumerable targetItems;
				if (this.TargetContent is IEnumerable)
					targetItems = this.TargetContent as IEnumerable;
				else
					targetItems = new object[] { this.TargetContent };

				ApplyBindingsToItems(targetItems);
			}

			// Raise the post-apply event, this is for validations and persistence
			CancelRoutedEventArgs aargs = new CancelRoutedEventArgs();
			aargs.RoutedEvent = AppliedChangesEvent;
			aargs.Source = this;
			aargs.CloseDialog = bargs.CloseDialog;
			OnAppliedChanges(aargs);

			// Close the dialog if necessary
			if (bargs.CloseDialog && !aargs.Cancel)
				this.Close();
		}

		public void ApplyBindingsToItems(IEnumerable targetItems)
		{
			int updated = 0;
			foreach (BEData bdata in _bindings)
			{
				// Don't do anything if the binding has not been changed
				if (
						(this.IsBatch && !bdata.BatchApply) ||
						!bdata.Changed
					)
					continue;

				// Get current val
				object val = bdata.Obj.GetValue(bdata.Prp);	
				object datacontext = (bdata.Obj as FrameworkElement).DataContext;

				foreach (object target in targetItems)
				{
					// Change the data context
					(bdata.Obj as FrameworkElement).DataContext = target;

					// Set the value back to the textbox, and update the permanent source
					bdata.Obj.SetValue(bdata.Prp, val);
					bdata.Exp.UpdateSource();
				}

				// Restore the temp. content
				(bdata.Obj as FrameworkElement).ClearValue(FrameworkElement.DataContextProperty);
				updated++;
			}

			if (updated < 1)
				throw new Exception("Nothing was applied. This should not happen, and could indicate a binding that was not changed to this:DialogBinding.");
		}

		/// <summary>
		/// 
		/// </summary>
		public bool Close()
		{
			return Close(CloseReason.None);
		}

		public bool Close(CloseReason reason)
		{
			// Raise the confirmation event
			CancelRoutedEventArgs args = new CancelRoutedEventArgs();
			args.RoutedEvent = ClosingEvent;
			args.Source = this;
			args.Reason = reason;
			OnClosing(args);

			if (args.Cancel)
				return false;

			// Don't mark as changed while applying changes
			this.Content = null;
			SetValue(TargetContentProperty, null);
			IsOpen = false;

			return true;
		}

		/*=========================*/
		#endregion
	}

	#region EventArgs & delegates
	/*=========================*/

	public class CancelRoutedEventArgs: RoutedEventArgs
	{
		public bool Skip = false;
		public bool Cancel = false;
		public CloseReason Reason = CloseReason.None;
		public Exception Exception = null;
		public bool CloseDialog = false;
	}

	public enum CloseReason
	{
		None,
		Cancel
	}

	public delegate void ContinueRoutedEventHandler(object source, CancelRoutedEventArgs e);
	/*=========================*/
	#endregion
}