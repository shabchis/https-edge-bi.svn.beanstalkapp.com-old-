using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections;
using System.IO.IsolatedStorage;
using System.IO;

namespace Easynet.Edge.UI.Client
{

	/// <summary>
	/// Constant strings/numbers/etc.
	/// </summary>
	public partial class Const
	{
		public class Cookies
		{
			public const string Prefix = "cookie_";
			public const string Login = "Login";
			public const string AccountsForGatewayIdCheck = "AccountsForGatewayIdCheck";
		}
	}

	/// <summary>
	/// Recreates the functionality of cookies by using isolated storage.
	/// </summary>
	public class CookieManager
	{
		IsolatedStorageFile _io;

		public CookieManager(IsolatedStorageFile io)
		{
			if (io == null)
				throw new ArgumentNullException("io");

			_io = io;
		}

		/// <summary>
		/// Gets or sets the cookie value with the corresponding name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public string this[string name]
		{
			get
			{
				if (_io.GetFileNames(Const.Cookies.Prefix + name).Length < 1)
					return null;

				// Get the "cookie" text file from the isolated storage
				IsolatedStorageFileStream iostr = new IsolatedStorageFileStream(
					Const.Cookies.Prefix + name,
					System.IO.FileMode.Open,
					_io
					);

				using (iostr)
				{
					using (StreamReader stream = new StreamReader(iostr))
					{
						return stream.ReadToEnd();
					}
				}
			}
			set
			{
				ClearCookie(name);

				IsolatedStorageFileStream iostr = new IsolatedStorageFileStream(
					Const.Cookies.Prefix + name,
					System.IO.FileMode.Create,
					_io
					);

				using (iostr)
				{
					using (StreamWriter stream = new StreamWriter(iostr))
					{
						stream.Write(value);
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		public void ClearCookie(string name)
		{
			if (_io.GetFileNames(Const.Cookies.Prefix + name).Length > 0)
				_io.DeleteFile(Const.Cookies.Prefix + name);
		}

		/// <summary>
		/// 
		/// </summary>
		public void ClearAll()
		{
			foreach (string file in _io.GetFileNames(Const.Cookies.Prefix + "*"))
				_io.DeleteFile(file);
		}
	}

}
