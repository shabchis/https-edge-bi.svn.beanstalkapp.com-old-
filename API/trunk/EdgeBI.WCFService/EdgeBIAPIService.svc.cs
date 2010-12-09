using System;
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

namespace EdgeBI.WCFService
{
	/// <summary>
	/// WCF SERVICE CLASS
	/// </summary>
	[ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
	public class EdgeBIAPIService : IEdgeBIAPIService
	{

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
			if (IsSessionValid())
			{
				List<Menu> m = Menu.GetMenuByParentID(menuID);
				if (m == null || m.Count == 0)
					WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.NotFound;

				return m;
			}
			return null; 

		}

		[WebGet(UriTemplate = "Accounts/{accountID}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
		public List<Account> GetAccount(string accountID)
		{
			int accId = int.Parse(accountID);
			List<Account> acc = Account.GetAccount(accId, true);
			return acc;
		}

		[WebInvoke(Method = "POST", UriTemplate = "sessions", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		public int LogIN(string email, string password)
		{
			int session = -1;


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
					session = System.Convert.ToInt32(sqlCommand.ExecuteScalar());

				}


			}
			return (int)session;
		}

		private bool IsSessionValid()
		{
			bool isValid = false;
			int sessionID = 0;
			DateTime lastModified;
			if (WebOperationContext.Current.IncomingRequest.Headers["x-edgebi-session"] == null)
				WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
			else
			{

				sessionID = int.Parse(WebOperationContext.Current.IncomingRequest.Headers["x-edgebi-session"]);
				using (DataManager.Current.OpenConnection())
				{
					using (SqlCommand sqlCommand = DataManager.CreateCommand("Session_ValidateSession(@SessionID:Int",System.Data.CommandType.StoredProcedure))
					{
						sqlCommand.Parameters["@SessionID"].Value = sessionID;
						isValid = System.Convert.ToBoolean(sqlCommand.ExecuteScalar());


					}

				}
			}
			if (!isValid)
				WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.Forbidden;
			return isValid;

		}







	}
}
