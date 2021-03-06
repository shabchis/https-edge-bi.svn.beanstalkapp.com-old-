﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using EdgeBI.Objects;
using Easynet.Edge.Core.Services;

namespace EdgeBI.WCFService
{
	/// <summary>
	/// Contract
	/// </summary>
	[ServiceContract]
	public interface IEdgeBIAPIService
	{
		[OperationContract(Name = "GetUserByID")]		
		User GetUserByID(string ID);

		[OperationContract][NetDataContract]
		List<Menu> GetMenu(string parentID);

		[OperationContract(Name = "GetAccountByID")]
		List<Account> GetAccount(string accountID);

		[OperationContract(Name = "GetAccount")]
		List<Account> GetAccount();

		[OperationContract(Name = "LogIN")]
		string LogIN(string email, string password);

		






	}


	
}
