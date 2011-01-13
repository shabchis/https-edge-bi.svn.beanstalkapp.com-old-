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
using System.Data;

/// <summary>
/// Summary description for AlonService
/// </summary>
/// 
namespace EdgeBI.API.Web
{
	[ServiceContract]
	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class EdgeApiService
	{
		private const string KeyEncrypt = "5c51374e366f41297356413c71677220386c534c394742234947567840";
		/// <summary>
		/// Get user
		/// </summary>
		/// <param name="ID">The User Primery Key</param>
		/// <returns></returns>
		/// 
		#region Users and groups
		[WebGet(UriTemplate = "users/{ID}")]
		public User GetUserByID(string ID)
		{
			int currentUser;
			currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
			int userID = int.Parse(ID);
			if (userID != currentUser)
			{
				User user = User.GetUserByID(currentUser);
				if (user.IsAcountAdmin != true)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can get user that is diffrent then current user!");

			}
			return User.GetUserByID(userID);
		}

		[WebGet(UriTemplate = "users")]
		public List<User> GetAllUsers()
		{
			List<User> users = null;
			int currentUser;
			currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
			User user = User.GetUserByID(currentUser);
			if (user.IsAcountAdmin != true)
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can get user that is diffrent then current user!");
			users = User.GetAllUsers();

			return users;
		}

		[WebInvoke(Method = "POST", UriTemplate = "users")]
		public void AddNewUser(User user)
		{

			//todo: dont forget on production to change the userID field to auto increment
			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
				User activeUser = User.GetUserByID(currentUser);
				if (activeUser.IsAcountAdmin != true)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can add users ");
				User.AddNewUser(user);
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.NotFound, ex.Message);
			}

		}

		[WebInvoke(Method = "POST", UriTemplate = "users/{ID}")]
		public void UpdateUser(string ID, User user)
		{
			if (ID.Trim() != user.UserID.ToString())
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Updated userId is different from ID");

			int currentUser;
			currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
			User activeUser = User.GetUserByID(currentUser);
			if (activeUser.IsAcountAdmin != true)
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can updated users");

			User.UpdateUser(user);



		}

		[WebInvoke(Method = "DELETE", UriTemplate = "users/{ID}")]
		public void DeleteUser(string ID)
		{
			int currentUser;
			currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
			User activeUser = User.GetUserByID(currentUser);
			if (activeUser.IsAcountAdmin != true)
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can delete users");
			User.DeleteUser(int.Parse(ID));

		}

		[WebGet(UriTemplate = "groups/{ID}")]
		public Group GetGroupByID(string ID)
		{
			int currentUser;
			currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
			int groupID = int.Parse(ID);

			User user = User.GetUserByID(currentUser);
			if (user.IsAcountAdmin != true)
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can get user that is diffrent then current user!");


			return Group.GetGroupByID(groupID);

		}

		[WebGet(UriTemplate = "groups")]
		public List<Group> GetAllGroups()
		{
			List<Group> groups = null;
			int currentUser;
			currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
			User user = User.GetUserByID(currentUser);
			if (user.IsAcountAdmin != true)
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can get the list of all groups");
			groups = Group.GetAllGroups();

			return groups;

		}

		[WebInvoke(Method = "POST", UriTemplate = "groups")]
		public void AddNewGroup(Group group)
		{

			//todo: dont forget on production to change the userID field to auto increment
			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
				User activeUser = User.GetUserByID(currentUser);
				if (activeUser.IsAcountAdmin != true)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can add group ");
				Group.AddNewGroup(group);
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.NotFound, ex.Message);
			}

		}

		[WebInvoke(Method = "POST", UriTemplate = "groups/{ID}")]
		public void UpdateGroup(string ID, Group group)
		{

			if (ID.Trim() != group.GroupID.ToString())
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Updated groupID is different from ID");
			int currentUser;
			currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
			User activeUser = User.GetUserByID(currentUser);
			if (activeUser.IsAcountAdmin != true)
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can updated users");

			Group.UpdateGroup(group);



		}

		[WebInvoke(Method = "DELETE", UriTemplate = "groups/{ID}")]
		public void DeleteGroup(string ID)
		{
			int currentUser;
			currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
			User activeUser = User.GetUserByID(currentUser);
			if (activeUser.IsAcountAdmin != true)
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can delete users");
			Group.DeleteGroup(int.Parse(ID));

		}
		#endregion

		#region Menus
		[WebGet(UriTemplate = "menu")]
		public List<Menu> GetMenu()
		{
			int currentUser;
			currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);

			List<Menu> m = Menu.GetMenu(currentUser);
			if (m == null || m.Count == 0)
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.NotFound, string.Format("No menu found for userId {0} ", currentUser));
			return m;
		}
		#endregion

		#region Accounts

		[WebGet(UriTemplate = "Accounts/{accountID}")]
		[OperationContract(Name = "GetAccountByID")]
		public Account GetAccount(string accountID)
		{
			int currentUser;
			currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
			int? accId = int.Parse(accountID);
			List<Account> acc = Account.GetAccount(accId, true, currentUser);
			if (acc.Count == 0)
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.NotFound, String.Format("No account with permission found for user {0}", currentUser));
			return acc[0];
		}

		[WebGet(UriTemplate = "Accounts")]
		public List<Account> GetAccount()
		{
			int currentUser;
			currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
			List<Account> acc = Account.GetAccount(null, true, currentUser);
			if (acc.Count == 0)
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.NotFound, String.Format("No account with permission found for user {0}", currentUser));
			return acc;
		}
		#endregion

		#region permissions
		[WebInvoke(Method = "POST", UriTemplate = "permissions")]
		public bool GetSpecificPermissionValue(PermissionRequest permissionRequest)
		{


			bool hasPermission = false;
			int currentUser;
			ThingReader<CalculatedPermission> calculatedPermissionReader;
			currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
			List<CalculatedPermission> calculatedPermissionList = new List<CalculatedPermission>();
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand("User_CalculatePermissions(@UserID:Int)", CommandType.StoredProcedure);
				sqlCommand.Parameters["@UserID"].Value = currentUser;
				calculatedPermissionReader = new ThingReader<CalculatedPermission>(sqlCommand.ExecuteReader(), null);
				while (calculatedPermissionReader.Read())
				{
					calculatedPermissionList.Add(calculatedPermissionReader.Current);
				}
				calculatedPermissionReader.Dispose();


			}
			if (calculatedPermissionList != null && calculatedPermissionList.Count > 0)
			{
				if (string.IsNullOrEmpty(permissionRequest.Path))
				{
					if (calculatedPermissionList.Count > 0)
					{
						CalculatedPermission calculatedPermissions = calculatedPermissionList.Find(calculatedPermission => calculatedPermission.AccountID == permissionRequest.AccountID);
						if (calculatedPermissions != null)
							hasPermission = true;
					}
				}
				else
				{
					CalculatedPermission calculatedPermissions = calculatedPermissionList.Find(calculatedPermission => calculatedPermission.AccountID == permissionRequest.AccountID && calculatedPermission.Path.Trim().ToUpper() == permissionRequest.Path.Trim().ToUpper());
					if (calculatedPermissions != null)
						hasPermission = true;
				}

			}




			return hasPermission;
		}

		[WebGet(UriTemplate = "permissions")]
		public List<string> GetListOfAllPermissionType()
		{
			List<string> permissions = new List<string>();
			//TODO: MAYBE ONLY FOR ADMIN USER NOT SURE ASK DORON AND YARON
			//int currentUser;
			//currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);

			//User user = User.GetUserByID(currentUser);
			//if (user.IsAcountAdmin != true)
			//    ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can get user that is diffrent then current user!");

			using (DataManager.Current.OpenConnection())
			{
				using (SqlCommand sqlCommand = DataManager.CreateCommand("SELECT Path FROM Constant_PermissionType ORDER BY Path"))
				{
					using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
					{
						while (sqlDataReader.Read())
						{
							permissions.Add(sqlDataReader[0].ToString());
						}
					}

				}

			}
			return permissions;


		}

		[WebInvoke(Method = "POST", UriTemplate = "groups/{groupID}/permissions")]
		public void InsertUpdateRemovePermissionForGroup(string groupID, AssignedPermissionData assignedPermissions)
		{

			int currentUser;
			currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
			User user = User.GetUserByID(currentUser);
			if (user.IsAcountAdmin != true)
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can edit permissions");

			switch (assignedPermissions.permissionOperation)
			{
				case PermissionOperation.Add:
					{
						AssignedPermissionData.AddPermissions(int.Parse(groupID), assignedPermissions.accountsPermissionsData,true);
						break;
					}
				case PermissionOperation.Update:
					{
						AssignedPermissionData.UpdatePermissions(int.Parse(groupID), assignedPermissions.accountsPermissionsData,true);
						break;
					}
				case PermissionOperation.Delete:
					{
						AssignedPermissionData.RemovePermmissions(int.Parse(groupID), assignedPermissions.accountsPermissionsData,true);
						break;
					}

			}

		}

		[WebInvoke(Method="POST", UriTemplate="users/{userID}/permissions")]
		public void InsertUpdateRemovePermissionForUser(string userID,AssignedPermissionData assignedPermissions)
		{
			
			int currentUser;
			currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
			User user = User.GetUserByID(currentUser);
			if (user.IsAcountAdmin != true)
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can edit permissions");

			switch (assignedPermissions.permissionOperation)
			{
				case PermissionOperation.Add:
					{
						AssignedPermissionData.AddPermissions(int.Parse(userID), assignedPermissions.accountsPermissionsData,false);
						break;
					}
				case PermissionOperation.Update:
					{
						AssignedPermissionData.UpdatePermissions(int.Parse(userID), assignedPermissions.accountsPermissionsData,false);
						break;
					}
				case PermissionOperation.Delete:
					{
						AssignedPermissionData.RemovePermmissions(int.Parse(userID), assignedPermissions.accountsPermissionsData,false);
						break;
					}
				
			}

		}

		#endregion

		#region Login
		[WebInvoke(Method = "POST", UriTemplate = "sessions")]
		public SessionResponseData LogIn(SessionRequestData sessionData)
		{
			SqlCommand sqlCommand;
			int unEncrypterSession;
			SessionResponseData returnsessionData = new SessionResponseData();
			int? id = null;
			using (DataManager.Current.OpenConnection())
			{
				Encryptor encryptor = new Encryptor(KeyEncrypt);
				if (sessionData.OperationType == OperationTypeEnum.New) //login with email and password
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

		#endregion








	}
}