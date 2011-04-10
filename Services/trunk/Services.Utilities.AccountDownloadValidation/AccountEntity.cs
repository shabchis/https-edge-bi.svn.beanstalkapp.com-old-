using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Easynet.Edge.Services.Utilities
{
	public class AccountEntity
	{
		public AccountEntity()
		{
		}
		public AccountEntity(SqlDataReader _reader)
		{
			Account_id = Convert.ToUInt64(_reader[0]);
			DayCode = Convert.ToUInt64(_reader[1]);
			Channel = Convert.ToInt64(_reader[2]);
			switch (Channel)
			{
				case 0: CahnnelType = "BackOffice";
						break;
				case -1:CahnnelType = "Content";
						break;
				case 6: CahnnelType = "Facebook";
						break;
				case 1: CahnnelType = "Adwords";
						break;
				default: CahnnelType = "Undefined Cahnnel";
						break;
			}
			
		}
		
		public UInt64 Account_id { set; get; }
		public Int64 Channel { set; get; }
		public string CahnnelType { set; get; }
		public UInt64 DayCode { set; get; }



	}
}
