using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;




namespace Edge.Objects
{

	[DataContract]
	[TableMap("User_GUI_User")]
	public class User
	{

		[DataMember(Order = 0)]
		[FieldMap("UserID", IsKey = true)]
		public int UserID;

		[DataMember(Order = 1)]
		[FieldMap("Name")]
		public string Name;

		[DataMember(Order = 2)]
		[FieldMap("IsActive")]
		public bool IsActive;

		[DataMember(Order = 3)]
		[FieldMap("AccountAdmin")]
		public bool IsAcountAdmin;

		[DataMember(Order = 4)]
		[FieldMap("Email")]
		public string Email;

		[DataMember]
		[FieldMap("Password", Show = false)]
		public string Password;

		[DataMember(Order = 5)]
		[DictionaryMap(Command = "User_AssignedPermission(@UserID:Int)", IsStoredProcedure = true, ValueIsGenericList = true, KeyName = "AccountID", ValueFieldsName = "PermissionName,PermissionType,Value")]
		public Dictionary<int, List<AssignedPermission>> AssignedPermissions = new Dictionary<int, List<AssignedPermission>>();

		
		[DataMember(Order = 6)]
		[ListMap(Command = "SELECT T0.GroupID,T1.Name FROM User_GUI_UserGroupUser T0 INNER JOIN User_GUI_UserGroup T1 ON T0.GroupID=T1.GroupID WHERE T0.UserID=@UserID:Int ORDER BY Name", IsStoredProcedure = false)]
		public List<Group> AssignedToGroups = new List<Group>();

		[DataMember(Order = 7)]		
		public Dictionary<int, List<CalculatedPermission>> CalculatedPermission = new Dictionary<int, List<CalculatedPermission>>();

		public static User GetUserByID(int id)
		{
			User user = null;
			ThingReader<User> thingReader;
			Func<FieldInfo, IDataRecord, object> customApply = CustomApply;
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand("User_GetByID(@userID:int)", CommandType.StoredProcedure);
				sqlCommand.Parameters["@userID"].Value = id;

				thingReader = new ThingReader<User>(sqlCommand.ExecuteReader(), CustomApply);
				if (thingReader.Read())
				{
					user = (User)thingReader.Current;
				}
			}
			if (user != null)
			{
				user = MapperUtility.ExpandObject<User>(user, customApply);
			}

			return user;
			//System.Data.SqlClient.SqlCommand cmd = Easynet.Edge.Core.Data.DataManager.CreateCommand("User_GetAllPermissions(@userID:int)");
		}

		private static object CustomApply(FieldInfo info, IDataRecord reader)
		{
			throw new NotImplementedException();
		}


		public static List<User> GetAllUsers()
		{
			List<User> users = new List<User>();
			ThingReader<User> thingReader;
			Func<FieldInfo, IDataRecord, object> customApply = CustomApply;
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand("SELECT UserID,IsActive,Name,Email FROM User_GUI_User ORDER BY Name");


				thingReader = new ThingReader<User>(sqlCommand.ExecuteReader(), CustomApply);
				while (thingReader.Read())
				{
					users.Add((User)thingReader.Current);
				}
			}
			if (users != null && users.Count > 0)
			{
				for (int i = 0; i < users.Count; i++)
				{
					users[i] = MapperUtility.ExpandObject<User>(users[i], customApply);
				}
			}
			return users.OrderBy(N => N.Name).ToList() ;
		}
		public int UserOperations(SqlOperation sqlOperation)
		{
			SqlTransaction sqlTransaction = null;
			int returnValue = -1;
			try
			{

				//Insert/Update/Remove user (also this clean all permissions and assigned groups)
				string command = @"User_Operations(@Action:Int,@UserID:Int,@Name:NvarChar,@IsActive:bit,@AccountAdmin:bit,@Email:NvarChar,@Password:NvarChar)";
				SqlConnection sqlConnection = new SqlConnection(DataManager.ConnectionString);
				sqlConnection.Open();
				sqlTransaction = sqlConnection.BeginTransaction("SaveUser");
				returnValue = this.UserID = Convert.ToInt32(MapperUtility.SaveOrRemoveSimpleObject<User>(command, CommandType.StoredProcedure, sqlOperation, this, sqlConnection, sqlTransaction));
				int lastAccountID = -999;
				bool AddFictive = true;
				//insert the new permission
				foreach (KeyValuePair<int, List<AssignedPermission>> assignedPermissionPerAccount in this.AssignedPermissions)
				{
					foreach (AssignedPermission assignedPermission in assignedPermissionPerAccount.Value)
					{
						if (assignedPermission.PermissionName != "FictivePermission")
						{
							if (lastAccountID == assignedPermissionPerAccount.Key)
								AddFictive = false;
							else
								AddFictive = true;
							SqlCommand sqlCommand = DataManager.CreateCommand("Permissions_Operations(@AccountID:Int,@TargetID:Int,@TargetIsGroup:Bit,@PermissionType:NvarChar,@Value:Bit,@AddFictive:Bit)", CommandType.StoredProcedure);
							sqlCommand.Connection = sqlConnection;
							sqlCommand.Transaction = sqlTransaction;
							sqlCommand.Parameters["@AccountID"].Value = assignedPermissionPerAccount.Key;
							sqlCommand.Parameters["@TargetID"].Value = this.UserID;
							sqlCommand.Parameters["@TargetIsGroup"].Value = 0;
							sqlCommand.Parameters["@PermissionType"].Value = assignedPermission.PermissionType;
							sqlCommand.Parameters["@Value"].Value = assignedPermission.Value;
							sqlCommand.Parameters["@AddFictive"].Value = AddFictive;
							sqlCommand.ExecuteNonQuery();
							lastAccountID = assignedPermissionPerAccount.Key;
						}
					}
				}

				foreach (Group group in this.AssignedToGroups)
				{
					SqlCommand sqlCommand = DataManager.CreateCommand(@"INSERT INTO User_GUI_UserGroupUser
																	(GroupID,UserID)
																	VALUES
																	(@GroupID:Int,@UserID:Int)");
					sqlCommand.Parameters["@GroupID"].Value = group.GroupID;
					sqlCommand.Parameters["@UserID"].Value = this.UserID;
					sqlCommand.Connection = sqlConnection;
					sqlCommand.Transaction = sqlTransaction;
					sqlCommand.ExecuteNonQuery();

				}

				sqlTransaction.Commit();
			}
			catch (Exception ex)
			{
				if (sqlTransaction != null)
					sqlTransaction.Rollback();
				throw ex;
			}
			return returnValue;
		}
		public void AssignGroup(int groupID)
		{
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand(@"INSERT INTO User_GUI_UserGroupUser
																	(GroupID,UserID)
																	VALUES
																	(@GroupID,@UserID)");
				sqlCommand.Parameters["@GroupID"].Value = groupID;
				sqlCommand.Parameters["@UserID"].Value = this.UserID; ;

				sqlCommand.ExecuteNonQuery();

			}

		}
		public static void EnableDisableUser(int ID, bool isActive)
		{
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand(@"UPDATE User_GUI_User 
																	SET IsActive=@isActive:bit
																	WHERE UserID=@userID:int");
				sqlCommand.Parameters["@isActive"].Value = isActive;
				sqlCommand.Parameters["@userID"].Value = ID;

				sqlCommand.ExecuteNonQuery();
			}

		}
		public void ChangePasswords(string password)
		{

		 string DecPassword=Easynet.Edge.Core.Utilities.Encryptor.Dec(password);
		 using (DataManager.Current.OpenConnection())
		 {
			 SqlCommand sqlCommand = DataManager.CreateCommand(@"UPDATE User_GUI_User 
																SET password=@password:Nvarchar 
																WHERE UserID=@ID:int", CommandType.Text);
			 sqlCommand.Parameters["@password"].Value = DecPassword;
			 sqlCommand.Parameters["@ID"].Value = this.UserID;
			 sqlCommand.ExecuteNonQuery();


		 }


		}
	}
}
