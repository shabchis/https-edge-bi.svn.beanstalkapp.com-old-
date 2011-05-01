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
			try
			{
				Account_id = Convert.ToUInt64(_reader[0]);
				DayCode = Convert.ToUInt64(_reader[1]);
				Channel = Convert.ToInt64(_reader[2]);
				App = Convert.ToString(_reader[3]);
				switch (Channel)
				{
					case 0: CahnnelType = "BackOffice";
						break;
					case -1: CahnnelType = "Content";
						break;
					case 6: CahnnelType = "Facebook";
						break;
					case 1: CahnnelType = "Adwords";
						break;
					default: CahnnelType = "Undefined Cahnnel";
						break;
				}

				switch (Convert.ToInt32(_reader[4]))
				{
					case 0: Status = "Failed";
						break;
					case 1: Status = "Success";
						break;
					default: Status = "Uknown";
						break;

				}
				Account_Name = Convert.ToString(_reader[5]);
			}
			catch (Exception e)
			{
				throw new Exception("AccountEntity constructor", e); 
			}
			
		}
		
		public UInt64 Account_id { set; get; }
		public string Account_Name { set; get; }
		public Int64 Channel { set; get; }
		public string CahnnelType { set; get; }
		public UInt64 DayCode { set; get; }
		public string Status { set; get; }
		public string App { set; get; }

	}
}
