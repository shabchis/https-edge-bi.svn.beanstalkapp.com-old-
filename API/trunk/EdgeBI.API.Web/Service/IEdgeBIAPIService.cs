using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using EdgeBI.Objects;


namespace EdgeBI.API.Web
{
	/// <summary>
	/// Summary description for Class1
	/// </summary>
	[ServiceContract]
	public interface IEdgeBIAPIService
	{
		[OperationContract]
		User GetUserByID(string ID);

		[OperationContract]
		List<Menu> GetMenu(string parentID);

		//[OperationContract(Name = "GetAccountByID")]
		//List<Account> GetAccount(string accountID);

		[OperationContract]
		List<Account> GetAccount();

		//[OperationContract(Name = "LogIN")]
		//string LogIN(string email, string password);








	}
}