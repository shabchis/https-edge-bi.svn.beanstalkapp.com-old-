using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CjcAwesomiumWrapper;
using Microsoft.Win32.SafeHandles;

namespace Cjc.ChromiumBrowser
{
	/// <summary>
	///     <Cjc.ChromiumBrowser:WebBrowser/>
	/// </summary>
	[TemplatePart( Name = "PART_Browser", Type = typeof( Image ) )]
	public class WebBrowser : ContentControl, IDisposable
	{
		public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
			"Source",
			typeof( string ),
			typeof( WebBrowser ),
			new PropertyMetadata( OnSourceChanged ) );

		public string Source
		{
			get { return (string)GetValue( SourceProperty ); }
			set { SetValue( SourceProperty, value ); }
		}

		public static readonly DependencyProperty RenderPriorityProperty = DependencyProperty.Register(
			"RenderPriority",
			typeof( DispatcherPriority ),
			typeof( WebBrowser ),
			new PropertyMetadata( DispatcherPriority.Render ) );

		public DispatcherPriority RenderPriority
		{
			get { return (DispatcherPriority)GetValue( RenderPriorityProperty ); }
			set { SetValue( RenderPriorityProperty, value ); }
		}

		public static readonly DependencyProperty IsTransparentProperty = DependencyProperty.Register(
			"IsTransparent",
			typeof( bool ),
			typeof( WebBrowser ),
			new PropertyMetadata( false, OnIsTransparentChanged ) );

		public bool IsTransparent
		{
			get { return (bool)GetValue( IsTransparentProperty ); }
			set { SetValue( IsTransparentProperty, value ); }
		}

		public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
			"IsActive",
			typeof( bool ),
			typeof( WebBrowser ),
			new PropertyMetadata( true ) );

		public bool IsActive
		{
			get { return (bool)GetValue( IsActiveProperty ); }
			set { SetValue( IsActiveProperty, value ); }
		}

		public class StatusEventArgs : EventArgs
		{
			public string Message { get; private set; }

			public StatusEventArgs( string message )
			{
				this.Message = message;
			}
		}

		public class UrlEventArgs : EventArgs
		{
			public string Url { get; private set; }
			public string FrameName { get; private set; }

			public UrlEventArgs( string url, string frameName )
			{
				this.Url = url;
				this.FrameName = frameName;
			}
		}

		public class LoadingEventArgs : UrlEventArgs
		{
			public int StatusCode { get; private set; }
			public string MimeType { get; private set; }

			public LoadingEventArgs( string url, string frameName, int statusCode, string mimeType )
				: base( url, frameName )
			{
				this.StatusCode = statusCode;
				this.MimeType = mimeType;
			}
		}

		public event EventHandler<StatusEventArgs> Status;
		public event EventHandler<UrlEventArgs> BeginNavigation;
		public event EventHandler<LoadingEventArgs> BeginLoading;
		public event EventHandler FinishLoading;

		private static WebCore webCore;

		private bool disposed;
		private WebView webView;
		private WebViewListener webViewListener;

		private Image image;
		private WriteableBitmap bitmap;
		private ToolTip tooltip;
		private byte[] buffer;
		private PixelFormat pixelFormat = PixelFormats.Bgr32;

		private bool isBrowserFocused;
		private string loadedUrl;

		static WebBrowser()
		{
			DefaultStyleKeyProperty.OverrideMetadata( typeof( WebBrowser ), new FrameworkPropertyMetadata( typeof( WebBrowser ) ) );

			webCore = new WebCore();

			CompositionTarget.Rendering += delegate { webCore.Update(); };
		}

		public WebBrowser()
		{
			Unloaded += delegate { ReleaseWebView(); };

			KeyboardNavigation.SetAcceptsReturn( this, true );

			tooltip = new ToolTip
			{
				HasDropShadow = true,
				IsOpen = false,
				StaysOpen = true
			};

			CompositionTarget.Rendering += delegate
			{
				if ( IsActive || IsMouseOver ) Render();
			};
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			image = (Image)GetTemplateChild( "PART_Image" );

			if ( image == null )
			{
				Content = image = new Image
				{
					Focusable = false,
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top,
					Stretch = Stretch.None
				};
			}
		}

		public void Navigate( string url )
		{
			if ( webView != null ) webView.LoadURL( url );
		}

		public void GotoHistoryOffset( int offset )
		{
			if ( webView != null ) webView.GotoHistoryOffset( offset );
		}

		protected override Size ArrangeOverride( Size arrangeBounds )
		{
			var size = base.ArrangeOverride( arrangeBounds );

			if ( image != null )
			{
				var width = (int)size.Width;
				var height = (int)size.Height;

				if ( webView == null )
				{
					InitializeWebView( width, height );
					AttachEventHandlers();

					webView.LoadURL( Source );
				}
				else webView.Resize( width, height );

				var bufferSize = width * height * 4;
				if ( buffer == null || buffer.Length != bufferSize ) buffer = new byte[ bufferSize ];

				if ( ( bitmap == null || bitmap.PixelWidth != width || bitmap.PixelHeight != height ) )
				{
					bitmap = new WriteableBitmap( width, height, 96, 96, pixelFormat, null );
				}
			}

			return size;
		}

		private void InitializeWebView( int width, int height )
		{
			try
			{
				RaiseStatus( "Creating WebView" );

				webView = webCore.CreateWebView( width, height );
				webView.SetTransparent( IsTransparent );

				RaiseStatus( "Initializign WebViewListener" );

				webViewListener = new WebViewListener();

				webViewListener.BeginNavigation += delegate( string url, string frameName )
				{
					RaiseStatus( string.Format( "BeginNavigation: {0}", url ) );
					if ( BeginNavigation != null ) BeginNavigation( this, new UrlEventArgs( url, frameName ) );

					if ( string.IsNullOrEmpty( frameName ) )
					{
						Dispatcher.BeginInvoke( (Action)delegate
						{
							Source = loadedUrl = url;
						},
						DispatcherPriority.Render );
					}
				};

				webViewListener.BeginLoading += delegate( string url, string frameName, int statusCode, string mimeType )
				{
					RaiseStatus( string.Format( "BeginLoading: {0}", url ) );
					if ( BeginLoading != null ) BeginLoading( this, new LoadingEventArgs( url, frameName, statusCode, mimeType ) );
				};

				webViewListener.FinishLoading += delegate
				{
					RaiseStatus( string.Format( "FinishLoading" ) );
					if ( FinishLoading != null ) FinishLoading( this, EventArgs.Empty );
				};

				webViewListener.ReceiveTitle += delegate( string title, string frameName )
				{
					RaiseStatus( string.Format( "ReceiveTitle: {0}", title ) );
				};

				webViewListener.ChangeCursor += delegate( ValueType cursorHandle )
				{
					Dispatcher.BeginInvoke( (Action)delegate
					{
						Cursor = CursorInteropHelper.Create( new SafeFileHandle( (IntPtr)cursorHandle, false ) );
					},
					DispatcherPriority.Render );
				};

				webViewListener.ChangeTooltip += delegate( string text )
				{
					Dispatcher.BeginInvoke( (Action)delegate
					{
						try
						{
							if ( text != null && text.Trim().Length > 0 && IsFocused )
							{
								tooltip.Content = text;
								tooltip.IsOpen = true;
							}
							else tooltip.IsOpen = false;
						}
						catch
						{
						}
					},
					DispatcherPriority.Render );
				};

				webViewListener.ChangeKeyboardFocus += delegate( bool isFocused )
				{
					isBrowserFocused = isFocused;
				};

				webView.SetListener( webViewListener );
				webView.Focus();
			}
			catch ( Exception ex )
			{
				RaiseStatus( ex.Message + ex.StackTrace );
			}
		}

		private void AttachEventHandlers()
		{
			RaiseStatus( "Attaching event handlers" );

			GotFocus += delegate
			{
				RaiseStatus( "Got focus" );
				webView.Focus();
				isBrowserFocused = true;
			};

			LostFocus += delegate
			{
				RaiseStatus( "Lost focus" );
				webView.Unfocus();
				tooltip.IsOpen = false;
			};

			KeyDown += delegate( object sender, KeyEventArgs e )
			{
				if ( e.Key == Key.Tab && !isBrowserFocused )
				{
					RaiseStatus( "Allowed tab KeyDown for navigation" );
					return;
				}

				var k = e.ToKeyInfo();
				var isExtended = ( k.ControlKeyState & ControlKeyStates.EnhancedKey ) == ControlKeyStates.EnhancedKey;

				InjectKeyboardEvent( Win32Message.WM_KEYDOWN, (byte)k.VirtualKeyCode, isExtended, false, e.IsRepeat, false );

				if ( k.Character != 0 )
				{
					var leftAlt = ( k.ControlKeyState & ControlKeyStates.LeftAltPressed ) == ControlKeyStates.LeftAltPressed;
					var rightAlt = ( k.ControlKeyState & ControlKeyStates.LeftAltPressed ) == ControlKeyStates.RightAltPressed;

					InjectKeyboardEvent( Win32Message.WM_CHAR, (byte)k.Character, isExtended, leftAlt | rightAlt, e.IsRepeat, false );
				}

				RaiseStatus( "Handled KeyDown: " + e.Key );
				e.Handled = true;
			};

			KeyUp += delegate( object sender, KeyEventArgs e )
			{
				if ( e.Key == Key.Tab && !isBrowserFocused )
				{
					RaiseStatus( "Allowed tab KeyUp for navigation" );
					return;
				}

				var k = e.ToKeyInfo();
				var isExtended = ( k.ControlKeyState & ControlKeyStates.EnhancedKey ) == ControlKeyStates.EnhancedKey;

				InjectKeyboardEvent( Win32Message.WM_KEYUP, (byte)k.VirtualKeyCode, isExtended, true, !e.IsRepeat, true );

				RaiseStatus( "Handled KeyUp: " + e.Key );
				e.Handled = true;
			};

			MouseMove += delegate( object sender, MouseEventArgs e )
			{
				var pos = e.GetPosition( this );
				webView.InjectMouseMove( (int)pos.X, (int)pos.Y );

				tooltip.PlacementTarget = this;
				tooltip.Placement = PlacementMode.Top;
				tooltip.HorizontalOffset = pos.X;
				tooltip.VerticalOffset = pos.Y;
			};

			MouseLeave += delegate
			{
				tooltip.IsOpen = false;
			};

			MouseDown += delegate( object sender, MouseButtonEventArgs e )
			{
				CaptureMouse();
				Keyboard.Focus( this );
				webView.InjectMouseDown( GetMouseButton( e.ChangedButton ) );
			};

			MouseUp += delegate( object sender, MouseButtonEventArgs e )
			{
				webView.InjectMouseUp( GetMouseButton( e.ChangedButton ) );
				ReleaseMouseCapture();
			};

			MouseWheel += delegate( object sender, MouseWheelEventArgs e )
			{
				webView.InjectMouseWheel( e.Delta );
			};
		}

		private void Render()
		{
			if ( webView != null && webView.IsDirty() && bitmap != null && buffer != null )
			{
				Dispatcher.BeginInvoke( (Action)delegate
				{
					var r = webView.Render( buffer, bitmap.BackBufferStride, 4 );

					if ( r.Width > 0 && r.Height > 0 )
					{
						bitmap.WritePixels(
							new Int32Rect( r.X, r.Y, r.Width, r.Height ),
							buffer,
							bitmap.BackBufferStride,
							( r.Y * bitmap.BackBufferStride ) + (int)( r.X * ( bitmap.BackBufferStride / bitmap.PixelWidth ) ) );
					}

					if ( image.Source != bitmap ) image.Source = bitmap;
				},
				( IsFocused || IsMouseOver ) ? DispatcherPriority.Render : RenderPriority );
			}
		}

		private void RaiseStatus( string message )
		{
			if ( Status != null ) Status( this, new StatusEventArgs( message ) );
		}

		private void InjectKeyboardEvent( Win32Message message, byte virtualKeyCode, bool isExtended, bool contextCode, bool previousState, bool transitionState )
		{
			var lParam = new KeyLParam( 1, virtualKeyCode, isExtended, contextCode, previousState, transitionState );

			webView.InjectKeyboardEvent( (IntPtr)0, (int)message, virtualKeyCode, lParam );
		}

		private CjcAwesomiumWrapper.MouseButton GetMouseButton( System.Windows.Input.MouseButton button )
		{
			switch ( button )
			{
				case System.Windows.Input.MouseButton.Middle: return CjcAwesomiumWrapper.MouseButton.Middle;
				case System.Windows.Input.MouseButton.Right: return CjcAwesomiumWrapper.MouseButton.Right;
				default: return CjcAwesomiumWrapper.MouseButton.Left;
			}
		}

		private void ReleaseWebView()
		{
			if ( webView != null )
			{
				webView.Dispose();
				webView = null;
			}

			if ( webViewListener != null )
			{
				webViewListener.Dispose();
				webViewListener = null;
			}
		}

		private static void OnSourceChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
		{
			if ( args.NewValue != args.OldValue )
			{
				var webBrowser = obj as WebBrowser;
				var url = (string)args.NewValue;

				if ( webBrowser != null && url != webBrowser.loadedUrl ) webBrowser.Navigate( url );
			}
		}

		private static void OnIsTransparentChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
		{
			var webBrowser = obj as WebBrowser;
			var isTransparent = (bool)args.NewValue;
			if ( webBrowser.webView != null ) webBrowser.webView.SetTransparent( isTransparent );
			webBrowser.pixelFormat = isTransparent ? PixelFormats.Bgra32 : PixelFormats.Bgr32;
		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		protected virtual void Dispose( bool disposing )
		{
			if ( !disposed )
			{
				if ( disposing ) ReleaseWebView();

				disposed = true;
			}
		}

		#endregion
	}
}