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
using System.Text;

/// <summary>
/// Summary description for AlonService
/// </summary>
/// 
namespace EdgeBI.API.Web
{
	[ServiceContract]
	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class EdgeApiCore
	{
		private const string KeyEncrypt = "5c51374e366f41297356413c71677220386c534c394742234947567840";
		#region Groups
		[WebGet(UriTemplate = "groups/{ID}")]
		public Group GetGroupByID(string ID)
		{
			int groupID;
			Group group = null;
			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
				groupID = int.Parse(ID);

				User user = User.GetUserByID(currentUser);
				if (user.IsAcountAdmin != true)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can get user that is diffrent then current user!");
				group = Group.GetGroupByID(groupID);
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.InternalServerError, ex.Message);
			}

			return group;

		}

		[WebGet(UriTemplate = "groups")]
		public List<Group> GetAllGroups()
		{
			List<Group> groups = null;
			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
				User user = User.GetUserByID(currentUser);
				if (user.IsAcountAdmin != true)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can get the list of all groups");
				groups = Group.GetAllGroups();
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.InternalServerError, ex.Message);
			}

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
				group.GroupOperations(SqlOperation.Insert);
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.NotFound, ex.Message);
			}

		}

		[WebInvoke(Method = "PUT", UriTemplate = "groups/{ID}")]
		public void UpdateGroup(string ID, Group group)
		{

			try
			{
				if (ID.Trim() != group.GroupID.ToString())
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Updated groupID is different from ID");
				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
				User activeUser = User.GetUserByID(currentUser);
				if (activeUser.IsAcountAdmin != true)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can updated users");

				group.GroupOperations(SqlOperation.Update);

			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.InternalServerError, ex.Message);
			}


		}

		[WebInvoke(Method = "DELETE", UriTemplate = "groups/{ID}")]
		public void DeleteGroup(string ID)
		{
			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
				User activeUser = User.GetUserByID(currentUser);
				if (activeUser.IsAcountAdmin != true)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can delete users");
				Group group = new Group() { GroupID = int.Parse(ID) };
				group.GroupOperations(SqlOperation.Delete);
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.InternalServerError, ex.Message);
			}

		}
		[WebInvoke(Method ="POST", UriTemplate = "groups/{groupID}/users/{userID}")]
		public void AssignUserToGroup(string groupID, string userID)
		{
			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
				User activeUser = User.GetUserByID(currentUser);
				if (activeUser.IsAcountAdmin != true)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can assign users to groups ");
				Group group = Group.GetGroupByID(int.Parse(groupID));
				group.AssignUser(int.Parse(userID));
			}
			catch (Exception ex)
			{
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.NotFound, ex.Message);
			}

		}

		[WebGet(UriTemplate = "groups/{ID}/users")]
		public List<User> GetGroupAssociateUsers(string ID)
		{
			List<User> users = null;

			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
				User user = User.GetUserByID(currentUser);
				if (user.IsAcountAdmin != true)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can get the list of Associate groups");
				users = Group.GetUserAssociateUsers(int.Parse(ID));
			}
			catch (Exception ex)
			{
				if (ex.Message != "Forbidden")
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.InternalServerError, ex.Message);
			}

			return users;
		}
		#endregion

		#region Users
		/// <summary>
		/// Get user
		/// </summary>
		/// <param name="ID">The User Primery Key</param>
		/// <returns></returns>
		/// 
		[WebGet(UriTemplate = "users/{ID}")]
		public User GetUserByID(string ID)
		{
			int userID;
			User returnUser = null;
			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
				userID = int.Parse(ID);
				if (userID != currentUser)
				{
					User user = User.GetUserByID(currentUser);
					if (user.IsAcountAdmin != true)
						ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can get user that is diffrent then current user!");

				}
				returnUser = User.GetUserByID(userID);
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.InternalServerError, ex.Message);
			}
			return returnUser;
		}

		[WebGet(UriTemplate = "users")]
		public List<User> GetAllUsers()
		{
			List<User> users = null;
			try
			{

				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
				User user = User.GetUserByID(currentUser);
				if (user.IsAcountAdmin != true)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can get user that is diffrent then current user!");
				users = User.GetAllUsers();
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.InternalServerError, ex.Message);
			}

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
				user.UserOperations(SqlOperation.Insert);
			}
			catch (Exception ex)
			{
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.NotFound, ex.Message);
			}

		}

		[WebInvoke(Method = "PUT", UriTemplate = "users/{ID}")]
		public void UpdateUser(string ID, User user)
		{
			try
			{
				if (ID.Trim() != user.UserID.ToString())
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Updated userId is different from ID");

				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
				User activeUser = User.GetUserByID(currentUser);
				if (activeUser.IsAcountAdmin != true)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can updated users");

				user.UserOperations(SqlOperation.Update);
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.InternalServerError, ex.Message);
			}



		}

		[WebInvoke(Method = "DELETE", UriTemplate = "users/{ID}")]
		public void DeleteUser(string ID)
		{
			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
				User activeUser = User.GetUserByID(currentUser);
				if (activeUser.IsAcountAdmin != true)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can delete users");
				User user = new User() { UserID = int.Parse(ID) };
				user.UserOperations(SqlOperation.Delete);
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.InternalServerError, ex.Message);
			}

		}
		[WebInvoke(Method = "POST", UriTemplate = "users/{userID}/groups/{groupID}")]
		public void AssignGroupToUser(string userID, string groupID)
		{
			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
				User activeUser = User.GetUserByID(currentUser);
				if (activeUser.IsAcountAdmin != true || activeUser.UserID == int.Parse(userID))
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can assign groups to other useres ");
				User user = User.GetUserByID(int.Parse(userID));
				user.AssignGroup(int.Parse(groupID));
			}
			catch (Exception ex)
			{
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.NotFound, ex.Message);
			}

		}
		[WebGet(UriTemplate = "users/{ID}/groups")]
		public  List<Group> GetUserAssociateGroups(string ID)
		{
			List<Group> associateGroups = new List<Group>();

			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand(@"SELECT DISTINCT  T0.GroupID ,T1.Name
																	FROM User_GUI_UserGroupUser T0
																	INNER JOIN User_GUI_UserGroup T1 ON T0.GroupID=T1.GroupID 
																	WHERE T0.UserID=@UserID:Int");
				sqlCommand.Parameters["@UserID"].Value = int.Parse(ID);

				using (ThingReader<Group> thingReader = new ThingReader<Group>(sqlCommand.ExecuteReader(), null))
				{
					while (thingReader.Read())
					{
						associateGroups.Add((Group)thingReader.Current);
					}

				}

			}
			return associateGroups;
		}


		#endregion

		#region Menus
		[WebGet(UriTemplate = "menu")]
		public List<Menu> GetMenu()
		{
			List<Menu> m = null;
			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);

				m = Menu.GetMenu(currentUser);
				if (m == null || m.Count == 0)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.NotFound, string.Format("No menu found for userId {0} ", currentUser));
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.InternalServerError, ex.Message);
			}
			return m;
		}
		#endregion

		#region Accounts

		[WebGet(UriTemplate = "Accounts/{accountID}")]
		[OperationContract(Name = "GetAccountByID")]
		public Account GetAccount(string accountID)
		{
			List<Account> acc = null;
			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
				int? accId = int.Parse(accountID);
				acc = Account.GetAccount(accId, true, currentUser);
				if (acc.Count == 0)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.NotFound, String.Format("No account with permission found for user {0}", currentUser));
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.InternalServerError, ex.Message);
			}
			return acc[0];
		}

		[WebGet(UriTemplate = "Accounts")]
		public List<Account> GetAccount()
		{
			List<Account> acc = null;
			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
				acc = Account.GetAccount(null, true, currentUser);
				if (acc.Count == 0)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.NotFound, String.Format("No account with permission found for user {0}", currentUser));
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.InternalServerError, ex.Message);
			}
			return acc;
		}
		#endregion

		#region permissions
		[WebInvoke(Method = "POST", UriTemplate = "permissions")]
		public bool GetSpecificPermissionValue(PermissionRequest permissionRequest)
		{

			bool hasPermission = false;
			try
			{

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
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.InternalServerError, ex.Message);
			}
			return hasPermission;
		}

		[WebGet(UriTemplate = "permissions/list")]
		public List<string> GetListOfAllPermissionType()
		{
			List<string> permissions = new List<string>();

			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);

				User user = User.GetUserByID(currentUser);
				if (user.IsAcountAdmin != true)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can get user that is diffrent then current user!");

				permissions = Permission.GetAllPermissionTypeList();
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.InternalServerError, ex.Message);
			}
			return permissions;
		}

		[WebGet(UriTemplate = "permissions/tree")]
		public List<Permission> GetTreeOfAllPermissionType()
		{
			List<Permission> returnObject = new List<Permission>();
			try
			{				
				Stack<Permission> stackPermission = new Stack<Permission>();

				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);

				User user = User.GetUserByID(currentUser);
				if (user.IsAcountAdmin != true)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can get user that is diffrent then current user!");
				returnObject = Permission.GetAllPermissionTypeTree();
			}
			catch (Exception ex)
			{
				ErrorMessageInterceptor.ThrowError(HttpStatusCode.InternalServerError, ex.Message);
			}
			string str = CreateHtml(returnObject);
			return returnObject;


		}

	

		[WebInvoke(Method = "POST", UriTemplate = "groups/{groupID}/permissions")]
		public void InsertUpdateRemovePermissionForGroup(string groupID, AssignedPermissionData assignedPermissions)
		{

			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
				User user = User.GetUserByID(currentUser);
				if (user.IsAcountAdmin != true)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can edit permissions");
				AssignedPermissionData.PermissionOperations(int.Parse(groupID), assignedPermissions.accountsPermissionsData, true, assignedPermissions.permissionOperation);
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		[WebInvoke(Method = "POST", UriTemplate = "users/{userID}/permissions")]
		public void InsertUpdateRemovePermissionForUser(string userID, AssignedPermissionData assignedPermissions)
		{

			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(OperationContext.Current.IncomingMessageProperties["edge-user-id"]);
				User user = User.GetUserByID(currentUser);
				if (user.IsAcountAdmin != true)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Only Account Administrator, can edit permissions");
				AssignedPermissionData.PermissionOperations(int.Parse(userID), assignedPermissions.accountsPermissionsData, false, assignedPermissions.permissionOperation);
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.InternalServerError, ex.Message);
			}
		}




		#endregion

		#region Login
		[WebInvoke(Method = "POST", UriTemplate = "sessions")]
		public SessionResponseData LogIn(SessionRequestData sessionData)
		{
			SqlCommand sqlCommand;
			SessionResponseData returnsessionData = null;
			int session;

			try
			{
				using (DataManager.Current.OpenConnection())
				{
					Encryptor encryptor = new Encryptor(KeyEncrypt);
					sqlCommand = DataManager.CreateCommand("User_Login(@OperationType:Int,@Email:NVarchar,@Password:NVarchar,@UserID:Int,@SessionID:Int)", CommandType.StoredProcedure);


					sqlCommand.Parameters["@OperationType"].Value = sessionData.OperationType;
					if (sessionData.OperationType == OperationTypeEnum.New)
					{
						sqlCommand.Parameters["@Email"].Value = sessionData.Email;
						sqlCommand.Parameters["@Password"].Value = sessionData.Password;
					}
					else
					{
						sqlCommand.Parameters["@UserID"].Value = sessionData.UserID;

						try
						{
							sqlCommand.Parameters["@SessionID"].Value = encryptor.Decrypt(sessionData.Session);
						}
						catch (Exception ex)
						{
							ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "Invalid Session,session could no be parse!");

						}
					}
					SqlDataReader sqlReader = sqlCommand.ExecuteReader();
					if (sqlReader.Read())
					{
						session = Convert.ToInt32(sqlReader[0]);
						if (session > 0)
						{
							returnsessionData = new SessionResponseData();
							returnsessionData.UserID = sqlReader.GetInt32(1);
							returnsessionData.Session = encryptor.Encrypt(session.ToString());
						}
					}
				}
				if (returnsessionData == null)
					ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, "User Name/Password is wrong!");
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.InternalServerError, ex.Message);
			}

			return returnsessionData;
		}

		#endregion

		private string CreateHtml(List<Permission> permissions)
		{
			StringBuilder str = new StringBuilder();
			foreach (Permission permission in permissions)
			{
				if (permission.ChildPermissions.Count > 0)
				{
					str.Append(string.Format("<ul><li>{0}{1}</li></ul>", permission.Path, CreateHtml(permission.ChildPermissions)));
				}
				else
				{
					str.Append(string.Format("<ul><li>{0}</li></ul>", permission.Path));

				}
			}
			return str.ToString();
		}








	}
}