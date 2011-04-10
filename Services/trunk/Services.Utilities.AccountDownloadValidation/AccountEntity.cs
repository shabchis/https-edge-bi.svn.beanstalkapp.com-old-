using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Services.Utilities.AccountDownloadValidation
{
	public class AccountEntity
	{
		public AccountEntity()
		{
		}
		public AccountEntity(SqlDataReader _reader)
		{
			Account_id = Convert.ToUInt16(_reader[0]);
			DayCode = Convert.ToUInt16(_reader[1]);
			Channel = Convert.ToUInt16(_reader[2]);
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
		
		public int Account_id { set; get; }
		public int Channel { set; get; }
		public string CahnnelType { set; get; }
		public int DayCode { set; get; }



	}
}
