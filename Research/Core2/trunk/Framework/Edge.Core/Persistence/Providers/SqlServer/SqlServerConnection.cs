﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Edge.Core.Persistence.Providers.SqlServer
{
	public class SqlServerConnection : PersistenceConnection
	{
		public SqlConnection InternalConnection { get; private set; }
		public SqlTransaction InternalTransaction { get; private set; }

		internal SqlServerConnection(SqlServerProvider provider, SqlConnection internalConnection) : base(provider)
		{
			InternalConnection = internalConnection;
		}

		protected override void OnOpen()
		{
			InternalConnection.Open();
		}

		protected override void OnClose()
		{
			// Rollback uncommitted operations
			if (InternalTransaction != null)
				TransactionRollback();

			InternalConnection.Close();
		}

		public override void TransactionStart()
		{
			InternalTransaction = InternalConnection.BeginTransaction();
		}

		public override void TransactionCommit()
		{
			InternalTransaction.Commit();
		}

		public override void TransactionRollback()
		{
			InternalTransaction.Rollback();
		}
	}


}
