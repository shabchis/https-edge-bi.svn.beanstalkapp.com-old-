using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Edge.Objects
{
	
	public class SessionRequestData
	{
		public OperationTypeEnum OperationType;
		public int? UserID { get; set; }
		public string Email { get; set; }		
		public string Password { get; set; }
		public string Session { get; set; }
		
	}
	public class SessionResponseData
	{

		public int UserID { get; set; }		
		public string Session { get; set; }

	}

	public enum OperationTypeEnum
	{
		New,
		Renew

	}
}
