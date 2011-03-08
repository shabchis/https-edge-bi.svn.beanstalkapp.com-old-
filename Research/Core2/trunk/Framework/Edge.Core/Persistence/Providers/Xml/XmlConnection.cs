using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.IO;

namespace Eggplant.Persistence.Providers.SqlServer
{
	public class XmlConnection : PersistenceConnection
	{
		Stream _stream = null;
		string _filePath = null;

		internal XmlConnection(XmlProvider provider, Stream stream) : base(provider)
		{
			_stream = stream;
		}

		internal XmlConnection(XmlProvider provider, string filePath):base(provider)
		{
			_filePath = filePath;
		}

		protected override void OnOpen()
		{
		}

		protected override void OnClose()
		{
		}

		public override void TransactionStart()
		{
		}

		public override void TransactionCommit()
		{
		}

		public override void TransactionRollback()
		{
		}
	}


}
