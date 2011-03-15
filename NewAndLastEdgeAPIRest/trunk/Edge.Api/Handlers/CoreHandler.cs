using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Edge.Api.Handlers.Template;
using Edge.Objects;
using System.Data;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Utilities;
using System.Data.SqlClient;
using System.Net;

namespace Edge.Api.Handlers
{
	public class CoreHandler : TemplateHandler
	{
		private const string KeyEncrypt = "5c51374e366f41297356413c71677220386c534c394742234947567840";
		public override bool ShouldValidateSession
		{
			get { return true; }
		}


		#region Login
		[UriMapping(Method = "POST", Template = "sessions", BodyParameter = "sessionData")]
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
							throw new Exception("Invalid Session,session could no be parse!");


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
					throw new Exception("User Name/Password is wrong!");
			}
			catch (Exception ex)
			{

				throw new Exception(ex.Message);
			}

			return returnsessionData;
		}
		#endregion

		#region Groups
		[UriMapping(Template = "groups/{ID}")]
		public Group GetGroupByID(string ID)
		{
			int groupID;
			Group group = null;
			try
			{
				int currentUser;

				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
				groupID = int.Parse(ID);

				User user = User.GetUserByID(currentUser);
				if (user.IsAcountAdmin != true)
					throw new HttpStatusException("Only Account Administrator, can get user that is diffrent then current user!", HttpStatusCode.Forbidden);
				group = Group.GetGroupByID(groupID);
			}
			catch (Exception ex)
			{

				throw new HttpStatusException(ex.Message, HttpStatusCode.InternalServerError);
			}

			return group;

		}

		[UriMapping(Template = "groups")]
		public List<Group> GetAllGroups()
		{
			List<Group> groups = null;
			
				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
				User user = User.GetUserByID(currentUser);
				if (user.IsAcountAdmin != true)
					throw new HttpStatusException("Only Account Administrator, can get the list of all groups", HttpStatusCode.Forbidden);
				groups = Group.GetAllGroups();
			

			return groups;

		}

		[UriMapping(Method = "POST", Template = "groups", BodyParameter = "group")]
		public int AddNewGroup(Group group)
		{
			int groupID = 1;
			//todo: dont forget on production to change the userID field to auto increment
			
				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
				User activeUser = User.GetUserByID(currentUser);
				if (activeUser.IsAcountAdmin != true)
					throw new HttpStatusException("Only Account Administrator, can add group ", HttpStatusCode.Forbidden);
				groupID = group.GroupOperations(SqlOperation.Insert);
			
			return groupID;

		}

		[UriMapping(Method = "PUT", Template = "groups/{ID}", BodyParameter = "group")]
		public int UpdateGroup(string ID, Group group)
		{
			int groupID = -1;
			
				if (ID.Trim() != group.GroupID.ToString())
					throw new HttpStatusException("Updated groupID is different from ID", HttpStatusCode.Forbidden);
				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
				User activeUser = User.GetUserByID(currentUser);
				if (activeUser.IsAcountAdmin != true)
					throw new HttpStatusException("Only Account Administrator, can updated users", HttpStatusCode.Forbidden);

				groupID = group.GroupOperations(SqlOperation.Update);

			
			return groupID;


		}

		[UriMapping(Method = "DELETE", Template = "groups/{ID}")]
		public int DeleteGroup(string ID)
		{
			int groupID = -1;
			
				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
				User activeUser = User.GetUserByID(currentUser);
				if (activeUser.IsAcountAdmin != true)
					throw new HttpStatusException("Only Account Administrator, can delete users", HttpStatusCode.Forbidden);
				Group group = new Group() { GroupID = int.Parse(ID) };
				groupID = group.GroupOperations(SqlOperation.Delete);
			
			return groupID;

		}
		/*AssignUserToGroup
		[UriMapping(Method ="POST", Template = "groups/{groupID}/users/{userID}")]
		public void AssignUserToGroup(string groupID, string userID)
		{
			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
				User activeUser = User.GetUserByID(currentUser);
				if (activeUser.IsAcountAdmin != true)
					throw new HttpStatusException(HttpStatusCode.Forbidden, "Only Account Administrator, can assign users to groups ");
				Group group = Group.GetGroupByID(int.Parse(groupID));
				group.AssignUser(int.Parse(userID));
			}
			catch (Exception ex)
			{
				throw new HttpStatusException(HttpStatusCode.NotFound, ex.Message);
			}

		}
		 */

		[UriMapping(Template = "groups/{ID}/users")]
		public List<User> GetGroupAssociateUsers(string ID)
		{
			List<User> users = null;

			
				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
				User user = User.GetUserByID(currentUser);
				if (user.IsAcountAdmin != true)
					throw new HttpStatusException("Only Account Administrator, can get the list of Associate groups", HttpStatusCode.Forbidden);
				users = Group.GetUserAssociateUsers(int.Parse(ID));
			

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

		[UriMapping(Method="GET", Template = "users/{ID}")]
		public User GetUserByID(string ID)
		{
			int userID;
			User returnUser = null;
			
				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
				userID = int.Parse(ID);
				if (userID != currentUser)
				{
					User user = User.GetUserByID(currentUser);
					if (user.IsAcountAdmin != true)
						throw new HttpStatusException("Only Account Administrator, can get user that is diffrent then current user!", HttpStatusCode.Forbidden);

				}
				returnUser = User.GetUserByID(userID);
			
			return returnUser;
		}

		[UriMapping(Template = "users")]
		public List<User> GetAllUsers()
		{
			List<User> users = null;
			

				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
				User user = User.GetUserByID(currentUser);
				if (user.IsAcountAdmin != true)
					throw new HttpStatusException("Only Account Administrator, can get user that is diffrent then current user!", HttpStatusCode.Forbidden);
				users = User.GetAllUsers();
			

			return users;
		}

		[UriMapping(Method = "POST", Template = "users", BodyParameter = "user")]
		public int AddNewUser(User user)
		{
			int userID = -1;
			//todo: dont forget on production to change the userID field to auto increment
			
				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
				User activeUser = User.GetUserByID(currentUser);
				if (activeUser.IsAcountAdmin != true)
					throw new HttpStatusException("Only Account Administrator, can add users ", HttpStatusCode.Forbidden);
				userID = user.UserOperations(SqlOperation.Insert);
		
			return userID;

		}

		[UriMapping(Method = "PUT", Template = "users/{ID}")]
		public int UpdateUser(string ID, User user)
		{
			int userID = -1;
			
				if (ID.Trim() != user.UserID.ToString())
					throw new HttpStatusException("Updated userId is different from ID", HttpStatusCode.Forbidden);

				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
				User activeUser = User.GetUserByID(currentUser);
				if (activeUser.IsAcountAdmin != true)
					throw new HttpStatusException("Only Account Administrator, can updated users", HttpStatusCode.Forbidden);

				userID = user.UserOperations(SqlOperation.Update);
			
			return userID;



		}

		[UriMapping(Method = "DELETE", Template = "users/{ID}")]
		public int DeleteUser(string ID)
		{
			int userID = -1;
			
				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
				User activeUser = User.GetUserByID(currentUser);
				if (activeUser.IsAcountAdmin != true)
					throw new HttpStatusException("Only Account Administrator, can delete users", HttpStatusCode.Forbidden);
				User user = new User() { UserID = int.Parse(ID) };
				userID = user.UserOperations(SqlOperation.Delete);
			
			return userID;

		}
		/*AssignGroupToUser
		[UriMapping(Method = "POST", Template = "users/{userID}/groups/{groupID}")]
		public void AssignGroupToUser(string userID, string groupID)
		{
			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
				User activeUser = User.GetUserByID(currentUser);
				if (activeUser.IsAcountAdmin != true || activeUser.UserID == int.Parse(userID))
					throw new HttpStatusException(HttpStatusCode.Forbidden, "Only Account Administrator, can assign groups to other useres ");
				User user = User.GetUserByID(int.Parse(userID));
				user.AssignGroup(int.Parse(groupID));
			}
			catch (Exception ex)
			{
				throw new HttpStatusException(HttpStatusCode.NotFound, ex.Message);
			}

		}
		 * */
		[UriMapping(Template = "users/{ID}/groups")]
		public List<Group> GetUserAssociateGroups(string ID)
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
		[UriMapping(Template = "menu")]
		public List<Menu> GetMenu()
		{
			List<Menu> m = null;
			
				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);

				m = Menu.GetMenu(currentUser);
				if (m == null || m.Count == 0)
					throw new HttpStatusException(string.Format("No menu found for userId {0} ", currentUser), HttpStatusCode.NotFound);
			
			return m;
		}
		#endregion

		

		#region permissions
		[UriMapping(Method = "POST", Template = "permissions", BodyParameter = "permissionRequest")]
		public bool GetSpecificPermissionValue(PermissionRequest permissionRequest)
		{

			bool hasPermission = false;
			

				int currentUser;
				ThingReader<CalculatedPermission> calculatedPermissionReader;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
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

		[UriMapping(Template = "permissions/list")]
		public List<string> GetListOfAllPermissionType()
		{
			List<string> permissions = new List<string>();

			
				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);

				User user = User.GetUserByID(currentUser);
				if (user.IsAcountAdmin != true)
					throw new HttpStatusException("Only Account Administrator, can get user that is diffrent then current user!", HttpStatusCode.Forbidden);

				permissions = Permission.GetAllPermissionTypeList();
			
			return permissions;
		}

		[UriMapping(Template = "permissions/tree")]
		public List<Permission> GetTreeOfAllPermissionType()
		{
			List<Permission> returnObject = new List<Permission>();
			
				Stack<Permission> stackPermission = new Stack<Permission>();

				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);

				User user = User.GetUserByID(currentUser);
				if (user.IsAcountAdmin != true)
					throw new HttpStatusException("Only Account Administrator, can get user that is diffrent then current user!", HttpStatusCode.Forbidden);
				returnObject = Permission.GetAllPermissionTypeTree();
			

			return returnObject;


		}



		[UriMapping(Method = "POST", Template = "groups/{groupID}/permissions", BodyParameter = "assignedPermissions")]
		public void InsertUpdateRemovePermissionForGroup(string groupID, AssignedPermissionData assignedPermissions)
		{

			
				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
				User user = User.GetUserByID(currentUser);
				if (user.IsAcountAdmin != true)
					throw new HttpStatusException("Only Account Administrator, can edit permissions", HttpStatusCode.Forbidden);
				AssignedPermissionData.PermissionOperations(int.Parse(groupID), assignedPermissions.accountsPermissionsData, true, assignedPermissions.permissionOperation);
			
		}

		[UriMapping(Method = "POST", Template = "users/{userID}/permissions", BodyParameter = "assignedPermissions")]
		public void InsertUpdateRemovePermissionForUser(string userID, AssignedPermissionData assignedPermissions)
		{

			
				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
				User user = User.GetUserByID(currentUser);
				if (user.IsAcountAdmin != true)
					throw new HttpStatusException("Only Account Administrator, can edit permissions", HttpStatusCode.Forbidden);
				AssignedPermissionData.PermissionOperations(int.Parse(userID), assignedPermissions.accountsPermissionsData, false, assignedPermissions.permissionOperation);
			
		}




		#endregion






	}
}