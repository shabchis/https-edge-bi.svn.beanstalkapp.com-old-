using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Activation;
using EdgeBI.Objects;
using Easynet.Edge.Core.Data;
using System.Net;
using Easynet.Edge.Core.Utilities;
using System.Data.SqlClient;
using Microsoft.ServiceModel.Http;
using Microsoft.Http;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// Summary description for AlonService
/// </summary>
/// 
namespace EdgeBI.API.Web
{
	[ServiceContract]
	[ServiceBehavior(IncludeExceptionDetailInFaults=true)]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class EdgeApiService
	{
		private const string KeyEncrypt = "5c51374e366f41297356413c71677220386c534c394742234947567840";
		/// <summary>
		/// Get user
		/// </summary>
		/// <param name="ID">The User Primery Key</param>
		/// <returns></returns>
		[WebGet(UriTemplate = "users/{ID}", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
		public User GetUserByID(string ID)
		{
			int id = int.Parse(ID);
			return User.GetUserByID(id);
		}

		[WebGet(UriTemplate = "menu?Path={parentID}", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
		public List<Menu> GetMenu(string menuID)
		{
			int currentUser;
			currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);

			List<Menu> m = Menu.GetMenuByParentID(menuID, currentUser);
			if (m == null || m.Count == 0)
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.NotFound,string.Format("No menu found for userId {0} ",currentUser));
			return m;
		}

		[WebGet(UriTemplate = "Accounts/{accountID}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
		[OperationContract(Name = "GetAccountByID")]
		public List<Account> GetAccount(string accountID)
		{
			int currentUser;
			currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
			int? accId = int.Parse(accountID);
			List<Account> acc = Account.GetAccount(accId, true, currentUser);
			if (acc.Count == 0)
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.NotFound, String.Format("No account with permission found for user {0}", currentUser));
			return acc;
		}

		[WebGet(UriTemplate = "Accounts", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
		public List<Account> GetAccount()
		{
			int currentUser;
			currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
			List<Account> acc = Account.GetAccount(null, true, currentUser);
			if (acc.Count == 0)
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.NotFound, String.Format("No account with permission found for user {0}", currentUser));
			return acc;
		}

		[WebInvoke(Method = "POST", UriTemplate = "sessions")]
		public SessionResponseData LogIn(SessionRequestData sessionData)
		{
			SqlCommand sqlCommand;
			int unEncrypterSession;
			SessionResponseData returnsessionData =new SessionResponseData();
			int? id=null;
			using (DataManager.Current.OpenConnection())
			{
				Encryptor encryptor = new Encryptor(KeyEncrypt);
				if (sessionData.OperationType== OperationTypeEnum.New) //login with email and password
				{
					 sqlCommand = DataManager.CreateCommand(@"SELECT UserID 
																				FROM User_GUI_User
																					WHERE Email=@Email:NVarchar AND Password=@Password:NVarchar");
					sqlCommand.Parameters["@Email"].Value = sessionData.Email;
					sqlCommand.Parameters["@Password"].Value = sessionData.Password;

					id = (int?)sqlCommand.ExecuteScalar();
				}
				else //login with session and id
				{
					unEncrypterSession = int.Parse(encryptor.Decrypt(sessionData.Session));
					sqlCommand = DataManager.CreateCommand(@"SELECT UserID 
																				FROM User_GUI_Session
																					WHERE UserID=@UserID:Int AND SessionID=@SessionID:Int");
					sqlCommand.Parameters["@UserID"].Value = sessionData.UserID;
					sqlCommand.Parameters["@SessionID"].Value = unEncrypterSession;

					id = (int?)sqlCommand.ExecuteScalar();

				}
				
				if (id != null)
				{
					returnsessionData.UserID = id.Value;
					sqlCommand = DataManager.CreateCommand(@"INSERT INTO User_GUI_Session
																			(UserID)
																			VALUES(@UserID:Int);SELECT @@IDENTITY");
					sqlCommand.Parameters["@UserID"].Value = id;
					returnsessionData.Session = sqlCommand.ExecuteScalar().ToString();
					
					returnsessionData.Session = encryptor.Encrypt(returnsessionData.Session);

				}
				else
				{
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "User Name/Password is wrong!");


				}
			}
			return returnsessionData;
		}









	}
}