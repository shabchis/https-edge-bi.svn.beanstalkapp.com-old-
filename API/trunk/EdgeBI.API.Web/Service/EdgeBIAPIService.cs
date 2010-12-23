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
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class EdgeBIAPIService
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
			//currentUser =int.Parse( WebOperationContext.Current.IncomingRequest.Headers["user"]);
			currentUser = 8;
			List<Menu> m = Menu.GetMenuByParentID(menuID,currentUser);
			if (m == null || m.Count == 0)
				WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.NotFound;
			return m;
		}

		[WebGet(UriTemplate = "Accounts/{accountID}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
		[OperationContract(Name = "GetAccountByID")]
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

		[WebInvoke(Method = "POST", UriTemplate = "sessions")]		
		public string LogIN(SessionOperationData sessionData)
		{
			//using (StreamReader reader=new StreamReader(HttpContext.Current.Request.InputStream))
			//{
			//     sessionData = Newtonsoft.Json.JsonConvert.DeserializeObject<SessionData>(reader.ReadToEnd());
				
			//}
			string session = "-1";
			
			//string email = sessionData.email;
			//string password = sessionData.password;

			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand(@"SELECT UserID 
																				FROM User_GUI_User
																					WHERE Email=@Email:NVarchar AND Password=@Password:NVarchar");
				sqlCommand.Parameters["@Email"].Value = sessionData.Email;
				sqlCommand.Parameters["@Password"].Value = sessionData.Password;

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