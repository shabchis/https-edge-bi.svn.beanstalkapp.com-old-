using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.Data;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;

namespace EdgeBI.Objects
{
	[DataContract]
	[TableMap("User_GUI_UserGroup")]
	public class Group
	{
		[DataMember(Order = 0)]
		[FieldMap("GroupID", IsKey = true)]
		public int GroupID;

		[DataMember(Order = 1)]
		[FieldMap("Name")]
		public string Name;

		[DataMember(Order = 2)]
		[FieldMap("IsActive")]
		public bool IsActive;

		[DataMember(Order = 3)]
		[FieldMap("AccountAdmin")]
		public bool? IsAcountAdmin;

		[DataMember(Order=4)]
		[DictionaryMap(Command = "Group_AssignedPermission(@GroupID:Int)", IsStoredProcedure = true, ValueIsGenericList = true, KeyName = "AccountID", ValueFieldsName = "PermissionName,PermissionType,Value")]
		public Dictionary<int, List<AssignedPermission>> AssignedPermissions = new Dictionary<int, List<AssignedPermission>>();

		[DataMember(Order = 5)]
		[DictionaryMap(Command = "SELECT T0.UserID,T1.Name FROM User_GUI_UserGroupUser T0 INNER JOIN User_GUI_User T1 ON T0.UserID=T1.UserID WHERE T0.GroupID=@GroupID:Int", IsStoredProcedure = false, ValueIsGenericList = false, KeyName = "UserID", ValueFieldsName = "Name")]
		public Dictionary<int, string> Members = new Dictionary<int, string>();
		


		private static object CustomApply(FieldInfo info, IDataRecord reader)
		{
			throw new NotImplementedException();
		}

		public static List<Group> GetAllGroups()
		{
			List<Group> groups = new List<Group>();
			ThingReader<Group> thingReader;
			Func<FieldInfo, IDataRecord, object> customApply = CustomApply;
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand("SELECT GroupID,Name,AccountAdmin FROM User_GUI_UserGroup ORDER BY GroupID");


				thingReader = new ThingReader<Group>(sqlCommand.ExecuteReader(), CustomApply);
				while (thingReader.Read())
				{
					groups.Add((Group)thingReader.Current);
				}
			}
			if (groups != null && groups.Count > 0)
			{
				for (int i = 0; i < groups.Count; i++)
				{
					groups[i] = MapperUtility.ExpandObject<Group>(groups[i], customApply);
				}
			}
			return groups;
		}

		public static Group GetGroupByID(int groupID)
		{
			Group group = null;
			ThingReader<Group> thingReader;
			Func<FieldInfo, IDataRecord, object> customApply = CustomApply;
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand(@"SELECT GroupID,
																			Name,
																			IsActive,
																			AccountAdmin     
																			FROM User_GUI_UserGroup
																			WHERE GroupID=@GroupID:Int");
				sqlCommand.Parameters["@GroupID"].Value = groupID;

				thingReader = new ThingReader<Group>(sqlCommand.ExecuteReader(), CustomApply);
				if (thingReader.Read())
				{
					group = (Group)thingReader.Current;
				}
			}
			if (group != null)
			{
				group = MapperUtility.ExpandObject<Group>(group, customApply);
			}

			return group;
		}
		public void AssignUser(int userID)
		{
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand(@"INSERT INTO User_GUI_UserGroupUser
																	(GroupID,UserID)
																	VALUES
																	(@GroupID,@UserID)");
				sqlCommand.Parameters["@GroupID"].Value = this.GroupID;
				sqlCommand.Parameters["@UserID"].Value = userID;

				sqlCommand.ExecuteNonQuery();

			}
		}
		public static List<User> GetUserAssociateUsers(int ID)
		{
			List<User> associateUsers = new List<User>();

			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand(@"SELECT DISTINCT  T0.UserID ,T1.Name
																	FROM User_GUI_UserGroupUser T0
																	INNER JOIN User_GUI_User T1 ON T0.UserID=T1.UserID 
																	WHERE GroupID=@GroupID:Int");
				sqlCommand.Parameters["@GroupID"].Value = ID;

				using (ThingReader<User> thingReader = new ThingReader<User>(sqlCommand.ExecuteReader(), null))
				{
					while (thingReader.Read())
					{
						associateUsers.Add((User)thingReader.Current);
					}

				}

			}
			return associateUsers;
		}

		public  int GroupOperations(SqlOperation sqlOperation)
		{
			SqlTransaction sqlTransaction = null;
			int returnValue = -1;
			try
			{
				//Insert/Update/Remove user (also this clean all permissions and assigned groups)
				string command = @"Group_Operations(@Action:Int,@Name:NvarChar,@AccountAdmin:bit,@IsActive:bit,@GroupID:Int)";
				SqlConnection sqlConnection = new SqlConnection(DataManager.ConnectionString);
				sqlConnection.Open();
				sqlTransaction = sqlConnection.BeginTransaction("SaveGroup");
				returnValue=this.GroupID = Convert.ToInt32(MapperUtility.SaveOrRemoveSimpleObject<Group>(command, CommandType.StoredProcedure, sqlOperation, this, sqlConnection, sqlTransaction));

				//insert the new permission
				foreach (KeyValuePair<int, List<AssignedPermission>> assignedPermissionPerAccount in this.AssignedPermissions)
				{
					foreach (AssignedPermission assignedPermission in assignedPermissionPerAccount.Value)
					{
						SqlCommand sqlCommand = DataManager.CreateCommand("Permissions_Operations(@AccountID:Int,@TargetID:Int,@TargetIsGroup:Bit,@PermissionType:NvarChar,@Value:Bit)", CommandType.StoredProcedure);
						sqlCommand.Connection = sqlConnection;
						sqlCommand.Transaction = sqlTransaction;
						sqlCommand.Parameters["@AccountID"].Value = assignedPermissionPerAccount.Key;
						sqlCommand.Parameters["@TargetID"].Value = this.GroupID;
						sqlCommand.Parameters["@TargetIsGroup"].Value = 1;
						sqlCommand.Parameters["@PermissionType"].Value = assignedPermission.PermissionType;						
						sqlCommand.Parameters["@Value"].Value = assignedPermission.Value;
						sqlCommand.ExecuteNonQuery();
					}
				}
				foreach (int userID in this.Members.Keys)
				{
					SqlCommand sqlCommand = DataManager.CreateCommand(@"INSERT INTO User_GUI_UserGroupUser
																	(GroupID,UserID)
																	VALUES
																	(@GroupID:Int,@UserID:Int)");
					sqlCommand.Parameters["@GroupID"].Value = this.GroupID;
					sqlCommand.Parameters["@UserID"].Value = userID;
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
		
	}
}
