// Cjc.AwesomiumWrapper.h

#pragma once
#include <stdlib.h>
#include <string.h>
#include <msclr\marshal.h>
#include "WebCore.h"
#include "StringConvertor.h"

using namespace System;

typedef void (*BeginNavigationHandler)( const std::string& url, const std::wstring& frameName );
typedef void (*BeginLoadingHandler)( const std::string& url, const std::wstring& frameName, int statusCode, const std::wstring& mimeType );
typedef void (*FinishLoadingHandler)();
typedef void (*CallbackHandler)( const std::string& name );
typedef void (*ReceiveTitleHandler)( const std::wstring& title, const std::wstring& frameName );
typedef void (*ChangeTooltipHandler)( const std::wstring& tooltip );
typedef void (*ChangeCursorHandler)( const HCURSOR& cursor );
typedef void (*ChangeKeyboardFocusHandler)( bool isFocused );
typedef void (*ChangeTargetURLHandler)( const std::string& url );

namespace CjcAwesomiumWrapper
{
	typedef void (*BeginNavigation)();

	ref class WebViewListener;

	class DelegatingWebViewListener : public Awesomium::WebViewListener
	{
		public:

			DelegatingWebViewListener()
			{
				BeginLoading = 0;
				BeginNavigation = 0;
				FinishLoading = 0;
				Callback = 0;
				ReceiveTitle = 0;
				ChangeTooltip = 0;
				ChangeCursor = 0;
				ChangeKeyboardFocus = 0;
				ChangeTargetURL = 0;
			}

			virtual ~DelegatingWebViewListener() {}

			void onBeginNavigation( const std::string& url, const std::wstring& frameName )
			{
				if ( BeginNavigation ) (*BeginNavigation)( url, frameName );
			}

			BeginNavigationHandler BeginNavigation;

			void onBeginLoading( const std::string& url, const std::wstring& frameName, int statusCode, const std::wstring& mimeType )
			{
				if ( BeginLoading ) (*BeginLoading)( url, frameName, statusCode, mimeType );
			}

			BeginLoadingHandler BeginLoading;

			void onFinishLoading()
			{
				if ( FinishLoading ) (*FinishLoading)();
			}

			FinishLoadingHandler FinishLoading;

			void onCallback(const std::string& name, const Awesomium::JSArguments& args)
			{
				if ( Callback ) (*Callback)( name );
			}

			CallbackHandler Callback;

			void onReceiveTitle(const std::wstring& title, const std::wstring& frameName )
			{
				if ( ReceiveTitle ) (*ReceiveTitle)( title, frameName );
			}

			ReceiveTitleHandler ReceiveTitle;

			void onChangeTooltip(const std::wstring& tooltip)
			{
				if ( ChangeTooltip ) (*ChangeTooltip)( tooltip );
			}

			ChangeTooltipHandler ChangeTooltip;

#if defined(_WIN32)

			void onChangeCursor(const HCURSOR& cursor)
			{
				if ( ChangeCursor ) (*ChangeCursor)( cursor );
			}

			ChangeCursorHandler ChangeCursor;

#endif

			void onChangeKeyboardFocus(bool isFocused)
			{
				if ( ChangeKeyboardFocus ) (*ChangeKeyboardFocus)( isFocused );
			}

			ChangeKeyboardFocusHandler ChangeKeyboardFocus;

			void onChangeTargetURL(const std::string& url)
			{
				if ( ChangeTargetURL ) (*ChangeTargetURL)( url );
			}

			ChangeTargetURLHandler ChangeTargetURL;
	};

	public ref class JSValue
	{
		private:

			Awesomium::JSValue* value;

		public:

			JSValue() { this->value = new Awesomium::JSValue(); }
			JSValue( bool value ) { this->value = new Awesomium::JSValue( value ); }
			JSValue( int value ) { this->value = new Awesomium::JSValue( value ); }
			JSValue( double value ) { this->value = new Awesomium::JSValue( value ); }
			JSValue( String^ value ) { this->value = new Awesomium::JSValue( StringUtilities::StringConvertor( value ).STLAnsiString ); }

			virtual ~JSValue() { delete value; }

			bool IsBoolean() { return value->isBoolean(); }
			bool IsInteger() { return value->isInteger(); }
			bool IsDouble() { return value->isDouble(); }
			bool IsNumber() { return value->isNumber(); }
			bool IsString() { return value->isString(); }
			bool IsNull() { return value->isNull(); }

			operator String^() { return StringUtilities::StringConvertor( value->toString() ); }
			int ToInteger() { return value->toInteger(); }
			double ToDouble() { return value->toDouble(); }
			bool ToBoolean() { return value->toBoolean(); }

		internal:

			operator Awesomium::JSValue() { return *value; }
	};

	public ref class FutureJSValue
	{
		private:

			Awesomium::FutureJSValue* value;

		public:

			FutureJSValue( Awesomium::FutureJSValue& value ) { this->value = &value; }
			virtual ~FutureJSValue() { delete value; }

		internal:

			operator Awesomium::FutureJSValue() { return *value; }
	};

	public ref class JSArguments
	{
		private:

			const Awesomium::JSArguments* arguments;

		public:

			JSArguments( const Awesomium::JSArguments& args ) { this->arguments = &args; }
			virtual ~JSArguments() { delete arguments; }

		internal:

			operator const Awesomium::JSArguments&() { return *arguments; }
	};

	public ref class WebViewListener
	{
		public:

		private:

			DelegatingWebViewListener* webViewListener;

			void OnBeginNavigation( const std::string& url, const std::wstring& frameName )
			{
				BeginNavigation( StringUtilities::StringConvertor( url ), StringUtilities::StringConvertor( frameName ) );
			};

			void OnBeginLoading( const std::string& url, const std::wstring& frameName, int statusCode, const std::wstring& mimeType )
			{
				BeginLoading( StringUtilities::StringConvertor( url ), StringUtilities::StringConvertor( frameName ), statusCode, StringUtilities::StringConvertor( mimeType ) );
			};

			void OnFinishLoading()
			{
				FinishLoading();
			};

			void OnCallback( const std::string& name, const Awesomium::JSArguments& args )
			{
				Callback( StringUtilities::StringConvertor( name ), gcnew JSArguments( args ) );
			};

			void OnReceiveTitle( const std::wstring& title, const std::wstring& frameName )
			{
				ReceiveTitle( StringUtilities::StringConvertor( title ), StringUtilities::StringConvertor( frameName ) );
			};

			void OnChangeTooltip( const std::wstring& tooltip )
			{
				ChangeTooltip( StringUtilities::StringConvertor( tooltip ) );
			};

			void OnChangeCursor( const HCURSOR& cursor )
			{
				ChangeCursor( (IntPtr)cursor );
			};

			void OnChangeKeyboardFocus( bool isFocused )
			{
				ChangeKeyboardFocus( isFocused );
			};

			void OnChangeTargetURL( const std::string& url )
			{
				ChangeTargetURL( StringUtilities::StringConvertor( url ) );
			};

			delegate void BeginNavigationInternal( const std::string& url, const std::wstring& frameName );
			delegate void BeginLoadingInternal( const std::string& url, const std::wstring& frameName, int statusCode, const std::wstring& mimeType );
			delegate void FinishLoadingInternal();
			delegate void CallbackInternal( const std::string& name, const Awesomium::JSArguments& args );
			delegate void ReceiveTitleInternal( const std::wstring& title, const std::wstring& frameName );
			delegate void ChangeTooltipInternal( const std::wstring& tooltip );
			delegate void ChangeCursorInternal( const HCURSOR& cursor );
			delegate void ChangeKeyboardFocusInternal( bool isFocused );
			delegate void ChangeTargetURLInternal( const std::string& url );

			// Keey delegates alive
			BeginNavigationInternal^ beginNavigationInternal;
			BeginLoadingInternal^ beginLoadingInternal;
			FinishLoadingInternal^ finishLoadingInternal;
			CallbackInternal^ callbackInternal;
			ReceiveTitleInternal^ receiveTitleInternal;
			ChangeTooltipInternal^ changeTooltipInternal;
			ChangeCursorInternal^ changeCursorInternal;
			ChangeKeyboardFocusInternal^ changeKeyboardFocusInternal;
			ChangeTargetURLInternal^ changeTargetURLInternal;

		public:

			WebViewListener()
			{
				this->webViewListener = new DelegatingWebViewListener();

				this->webViewListener->BeginNavigation = (BeginNavigationHandler)System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(
					beginNavigationInternal = gcnew BeginNavigationInternal( this, &WebViewListener::OnBeginNavigation ) ).ToPointer();

				this->webViewListener->BeginLoading = (BeginLoadingHandler)System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(
					beginLoadingInternal = gcnew BeginLoadingInternal( this, &WebViewListener::OnBeginLoading ) ).ToPointer();

				this->webViewListener->FinishLoading = (FinishLoadingHandler)System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(
					finishLoadingInternal = gcnew FinishLoadingInternal( this, &WebViewListener::OnFinishLoading ) ).ToPointer();

				this->webViewListener->Callback = (CallbackHandler)System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(
					callbackInternal = gcnew CallbackInternal( this, &WebViewListener::OnCallback ) ).ToPointer();

				this->webViewListener->ReceiveTitle = (ReceiveTitleHandler)System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(
					receiveTitleInternal = gcnew ReceiveTitleInternal( this, &WebViewListener::OnReceiveTitle ) ).ToPointer();

				this->webViewListener->ChangeTooltip = (ChangeTooltipHandler)System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(
					changeTooltipInternal = gcnew ChangeTooltipInternal( this, &WebViewListener::OnChangeTooltip ) ).ToPointer();

				this->webViewListener->ChangeCursor = (ChangeCursorHandler)System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(
					changeCursorInternal = gcnew ChangeCursorInternal( this, &WebViewListener::OnChangeCursor ) ).ToPointer();

				this->webViewListener->ChangeKeyboardFocus = (ChangeKeyboardFocusHandler)System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(
					changeKeyboardFocusInternal = gcnew ChangeKeyboardFocusInternal( this, &WebViewListener::OnChangeKeyboardFocus ) ).ToPointer();

				this->webViewListener->ChangeTargetURL = (ChangeTargetURLHandler)System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(
					changeTargetURLInternal = gcnew ChangeTargetURLInternal( this, &WebViewListener::OnChangeTargetURL ) ).ToPointer();
			}

			virtual ~WebViewListener() { delete webViewListener; webViewListener = NULL; }

			delegate void BeginNavigationDelegate( String^ url, String^ frameName );
			event BeginNavigationDelegate^ BeginNavigation;

			delegate void BeginLoadingDelegate( String^ url, String^ frameName, int statusCode, String^ mimeType );
			event BeginLoadingDelegate^ BeginLoading;

			delegate void FinishLoadingDelegate();
			event FinishLoadingDelegate^ FinishLoading;

			delegate void CallbackDelegate( String^ url, JSArguments^ args );
			event CallbackDelegate^ Callback;

			delegate void ReceiveTitleDelegate( String^ title, String^ frameName );
			event ReceiveTitleDelegate^ ReceiveTitle;

			delegate void ChangeTooltipDelegate( String^ tooltip );
			event ChangeTooltipDelegate^ ChangeTooltip;

			delegate void ChangeCursorDelegate( IntPtr^ cursor );
			event ChangeCursorDelegate^ ChangeCursor;

			delegate void ChangeKeyboardFocusDelegate( bool isFocused );
			event ChangeKeyboardFocusDelegate^ ChangeKeyboardFocus;

			delegate void ChangeTargetURLDelegate( String^ url );
			event ChangeTargetURLDelegate^ ChangeTargetURL;

		internal:

			operator DelegatingWebViewListener*() { return webViewListener; }
	};

	public ref struct Rect
	{
		public:
			int X;
			int Y;
			int Width;
			int Height;
	};

	public enum class MouseButton { Left, Middle, Right };

	struct _KeyLParam
	{
		short RepeatCount : 16;
		unsigned char ScanCode : 8;
		bool IsExtended : 1;
		bool ContextCode : 1;
		bool PreviousState : 1;
		bool TransitionState : 1;
	};

	public ref struct KeyLParam
	{
		private:

			_KeyLParam* keyLParam;

		public:

			KeyLParam( short repeatCount, unsigned char scanCode, bool isExtended, bool contextCode, bool previousState, bool transitionState )
			{
				keyLParam = new _KeyLParam();
				keyLParam->RepeatCount = repeatCount;
				keyLParam->ScanCode = scanCode;
				keyLParam->IsExtended = isExtended;
				keyLParam->ContextCode = contextCode;
				keyLParam->PreviousState = previousState;
				keyLParam->TransitionState = transitionState;
			}

			virtual ~KeyLParam() { delete keyLParam; }

		internal:

			operator LPARAM() { return (LPARAM)*(unsigned int *)keyLParam; }
	};

	public ref class WebView
	{
		private:

			Awesomium::WebView* webView;
			WebViewListener^ listener;

		public:

			WebView( Awesomium::WebView* webView ) { this->webView = webView; }

			virtual ~WebView()
			{
				listener = nullptr;

				if ( webView != NULL )
				{
					webView->setListener( NULL );
					webView->destroy();
					webView = NULL;
				}
			}

			void LoadURL( String^ url )
			{
				webView->loadURL( StringUtilities::StringConvertor( url ).STLAnsiString );
			}

			void LoadHTML( String^ html )
			{
				webView->loadHTML( StringUtilities::StringConvertor( html ).STLAnsiString );
			}

			void LoadFile( String^ file )
			{
				webView->loadFile( StringUtilities::StringConvertor( file ).STLAnsiString );
			}

			void SetListener( WebViewListener^ webViewListener )
			{
				this->listener = webViewListener;

				webView->setListener( webViewListener != nullptr ? webViewListener : (Awesomium::WebViewListener*)NULL );
			}

			Rect^ Render( array<unsigned char, 1>^ buffer, int destRowSpan, int destDepth )
			{
				pin_ptr<unsigned char> pin_buffer = &buffer[ 0 ];
				Awesomium::Rect renderedRect;

				webView->render( pin_buffer, destRowSpan, destDepth, &renderedRect );

				Rect^ result = gcnew Rect();
				result->X = renderedRect.x;
				result->Y = renderedRect.y;
				result->Width = renderedRect.width;
				result->Height = renderedRect.height;

				return result;
			}

			void GotoHistoryOffset( int offset ) { webView->goToHistoryOffset( offset ); }

			void ExecuteJavascript( String^ javascript, String^ frameName )
			{
				webView->executeJavascript(
					StringUtilities::StringConvertor( javascript ).STLAnsiString,
					StringUtilities::StringConvertor( frameName ).STLWideString );
			}

			FutureJSValue^ ExecuteJavascriptWithResult( String^ javascript, String^ frameName )
			{
				return gcnew FutureJSValue( webView->executeJavascriptWithResult(
					StringUtilities::StringConvertor( javascript ).STLAnsiString,
					StringUtilities::StringConvertor( frameName ).STLWideString ) );
			}

			void SetProperty( String^ name, JSValue^ value ) { webView->setProperty( StringUtilities::StringConvertor( name ).STLAnsiString, value ); }

			void SetCallback( String^ name ) { webView->setCallback( StringUtilities::StringConvertor( name ).STLAnsiString ); }

			bool IsDirty() { return webView->isDirty(); }

			void Resize( int width, int height ) { webView->resize( width, height ); }

			void InjectMouseMove( int x, int y ) { webView->injectMouseMove( x, y ); }

			void InjectMouseDown( MouseButton button ) { webView->injectMouseDown( (Awesomium::MouseButton)(int)button ); }

			void InjectMouseUp( MouseButton button ) { webView->injectMouseUp( (Awesomium::MouseButton)(int)button ); }

			void InjectMouseWheel( int scrollAmount ) { webView->injectMouseWheel( scrollAmount ); }

			void Cut() { webView->cut(); }

			void Copy() { webView->copy(); }

			void Paste() { webView->paste(); }

			void SelectAll() { webView->selectAll(); }

			void DeselectAll() { webView->deselectAll(); }

			void ZoomIn() { webView->zoomIn(); }

			void ZoomOut() { webView->zoomOut(); }

			void ResetZoom() { webView->resetZoom(); }

			String^ GetContentAsText( int maxChars )
			{
				std::wstring result;
				webView->getContentAsText( result, maxChars );
				return StringUtilities::StringConvertor( result );
			}

			void Focus() { webView->focus(); }

			void Unfocus() { webView->unfocus(); }

			void SetTransparent( bool transparent ) { webView->setTransparent( transparent ); }

#if defined(_WIN32)

			void InjectKeyboardEvent( IntPtr^ hWnd, int message, IntPtr^ wParam, IntPtr^ lParam )
			{
				LPARAM test = reinterpret_cast<LPARAM>( lParam->ToPointer() );

				webView->injectKeyboardEvent(
					reinterpret_cast<HWND>( hWnd->ToPointer() ),
					message,
					reinterpret_cast<WPARAM>( wParam->ToPointer() ),
					reinterpret_cast<LPARAM>( lParam->ToPointer() ) );
			}

			void InjectKeyboardEvent( IntPtr^ hWnd, int message, int wParam, KeyLParam^ lParam )
			{
				webView->injectKeyboardEvent(
					0, //reinterpret_cast<HWND>( hWnd->ToPointer() ),
					message,
					wParam,
					lParam );
			}

#endif

		internal:

			operator Awesomium::WebView*() { return webView; }
	};

	public ref class WebCore
	{
		private:

			static Awesomium::WebCore* webCore;

		public:

			static WebCore() { webCore = new Awesomium::WebCore( Awesomium::LOG_NONE ); }

			WebCore() {}
			virtual ~WebCore() { delete webCore; webCore = NULL; }

			WebView^ CreateWebView( int width, int height )
			{
				return gcnew WebView( webCore->createWebView( width, height ) );
			}

			void Update()
			{
				webCore->update();
			}
	};
}