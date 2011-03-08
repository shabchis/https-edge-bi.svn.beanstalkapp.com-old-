using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading;
using System.Text.RegularExpressions;
using System.Data;
using System.Reflection;

namespace Eggplant.Persistence.Providers.SqlServer
{
	public class XmlProvider: PersistenceProvider
	{
		public override bool AlwaysUsesTransactions
		{
			get { return true; }
		}

		protected override PersistenceConnection CreateNewConnection()
		{
		}
	}
}
