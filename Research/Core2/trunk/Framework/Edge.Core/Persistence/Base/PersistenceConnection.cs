using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Threading;

namespace Edge.Core.Persistence
{
	/// <summary>
	/// Represents a connection with a persistence store.
	/// </summary>
	public class PersistenceConnection: IDisposable
	{
		#region Fields
		/*=========================*/

		bool _isOpen = false;
		bool _threadLocal = true;
		string _name;

		/// <summary>
		/// The provider for which the connection is open.
		/// </summary>
		public readonly PersistenceProvider Provider;

		/// <summary>
		/// 
		/// </summary>
		public const char ConnectionNameSeparator = '#';

		/*=========================*/
		#endregion

		#region Constructors
		/*=========================*/

		internal protected PersistenceConnection(PersistenceProvider provider)
		{
			Provider = provider;
		}

		/*=========================*/
		#endregion

		#region Properties
		/*=========================*/

		/// <summary>
		/// Indicates whether the connection is open.
		/// </summary>
		public bool IsOpen
		{
			get { return _isOpen; }
			internal set { _isOpen = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string Name
		{
			get { return _name; }
			internal set { _name = value; }
		}

		/// <summary>
		/// The full name of the connection, including the provider name.
		/// </summary>
		public string FullName
		{
			get { return FormatConnectionName(Provider.Name, this.Name); }
		}

		/// <summary>
		/// Indicates whether the connection is open on the current thread only.
		/// </summary>
		public bool IsThreadLocal
		{
			get { return _threadLocal; }
			internal set { _threadLocal = value; }
		}

		/*=========================*/
		#endregion

		#region Methods
		/*=========================*/

		internal void Open()
		{
			OnOpen();
		}

		protected virtual void OnOpen()
		{
		}

		internal void Close()
		{
			OnClose();
		}

		protected virtual void OnClose()
		{
		}

		void IDisposable.Dispose()
		{
			if (IsOpen)
				Persistence.Close(this);
		}

		public virtual void TransactionStart()
		{
		}

		public virtual void TransactionCommit()
		{
		}

		public virtual void TransactionRollback()
		{
		}

		/*=========================*/
		#endregion

		#region Helpers
		/*=========================*/
		
		internal static string FormatConnectionName(string provider, string connection)
		{
			return provider + ConnectionNameSeparator + connection;
		}

		/*=========================*/
		#endregion

	}
}
