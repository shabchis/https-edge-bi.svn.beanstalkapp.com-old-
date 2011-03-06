using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading;
using Edge.Core.Persistence.Providers.SqlServer;

namespace Edge.Core.Persistence
{
	/// <summary>
	/// Factory base class for persistence connections.
	/// </summary>
	public abstract class PersistenceProvider
	{
		#region Properties
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		public string Name
		{
			get;
			set;
		}

		public ObjectSchema ObjectSchema
		{
			get;
			set;
		}

		public PersistenceConfiguration ObjectMappings
		{
			get;
			set;
		}

		public virtual bool AlwaysUsesTransactions
		{
			get { return false; }
		}

		/*=========================*/
		#endregion

		#region Methods
		/*=========================*/

		/// <summary>
		/// 
		/// </summary>
		/// <param name="threadLocal"></param>
		/// <returns></returns>
		internal PersistenceConnection NewConnection(string connectionName, bool threadLocal)
		{
			PersistenceConnection conn = CreateNewConnection();
			conn.IsThreadLocal = threadLocal;
			conn.Name = connectionName;
			return conn;
		}

		/// <summary>
		/// Creates a new connection object. Can be overriden by derived classes to return
		/// a provider-specific connection object.
		/// </summary>
		/// <param name="threadLocal"></param>
		/// <returns></returns>
		protected virtual PersistenceConnection CreateNewConnection()
		{
			return new PersistenceConnection(this);
		}
		
		/*=========================*/
		#endregion

	}
}
