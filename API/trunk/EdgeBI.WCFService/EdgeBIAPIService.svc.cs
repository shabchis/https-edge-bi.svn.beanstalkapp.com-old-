﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using EdgeBI.Objects;
using System.ServiceModel.Activation;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;
using System.ServiceModel.Dispatcher;
using System.Web;
using System.Net;
using Easynet.Edge.Core.Utilities;

namespace EdgeBI.WCFService
{
	/// <summary>
	/// WCF SERVICE CLASS
	/// </summary>
	[ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
	public class EdgeBIAPIService : IEdgeBIAPIService
	{
		private const string KeyEncrypt = "5c51374e366f41297356413c71677220386c534c394742234947567840";
		/// <summary>
		/// Get user
		/// </summary>
		/// <param name="ID">The User Primery Key</param>
		/// <returns></returns>
		[WebGet(UriTemplate = "users/{ID}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
		public User GetUserByID(string ID)
		{
			int id = int.Parse(ID);
			return User.GetUserByID(id);

		}

		[WebGet(UriTemplate = "menu?Path={parentID}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
		public List<Menu> GetMenu(string menuID)
		{
			string currentUser = WebOperationContext.Current.IncomingRequest.Headers["user"];

			List<Menu> m = Menu.GetMenuByParentID(menuID, currentUser);
			if (m == null || m.Count == 0)
				WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.NotFound;

			return m;



		}

		[WebGet(UriTemplate = "Accounts/{accountID}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
		public List<Account> GetAccount(string accountID)
		{
			int? accId = int.Parse(accountID);
			List<Account> acc = Account.GetAccount(accId, true);
			return acc;
		}
		[WebGet(UriTemplate = "Accounts", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
		public List<Account> GetAccount()
		{

			List<Account> acc = Account.GetAccount(null, true);
			return acc;
		}

		[WebInvoke(Method = "POST", UriTemplate = "sessions", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		public string LogIN(string email, string password)
		{
			string session = "-1";


			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand(@"SELECT UserID 
																	FROM User_GUI_User
																	WHERE Email=@Email:NVarchar AND Password=@Password:NVarchar");
				sqlCommand.Parameters["@Email"].Value = email;
				sqlCommand.Parameters["@Password"].Value = password;

				int? id = (int?)sqlCommand.ExecuteScalar();
				if (id != null)
				{
					sqlCommand = DataManager.CreateCommand(@"INSERT INTO User_GUI_Session
															(UserID)
															VALUES(@UserID:Int);SELECT @@IDENTITY");
					sqlCommand.Parameters["@UserID"].Value = id;
					session = sqlCommand.ExecuteScalar().ToString();
					Encryptor encryptor = new Encryptor(KeyEncrypt);
					session = encryptor.Encrypt(session);



				}
				else
				{
					WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
					WebOperationContext.Current.OutgoingResponse.StatusDescription = "Wrong user name/password";

				}





			}
			return session;
		}









	}
}
