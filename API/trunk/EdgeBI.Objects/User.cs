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




namespace EdgeBI.Objects
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

		
		[FieldMap("IsActive", Show = false)]
		public bool IsActive=true;

		[DataMember(Order = 3)]
		[FieldMap("AccountAdmin")]
		public bool? IsAcountAdmin;

		//[DataMember(Order = 4)]
		//[FieldMap("UserAdmin")]
		//public bool? IsUserAdmin;

		[DataMember(Order = 4)]
		[FieldMap("Email")]
		public string Email;

		[DataMember]
		[FieldMap("Password",Show=false)]
		public string Password;

		//TODO: Remember to change back to 1 the TargetIsGroup
		//[DataMember(Order=6)]
		//[DictionaryMap("User_GUI_AccountPermission", "AccountID", WhereClause = "TargetIsGroup = 1 and TargetID = @PrimeryKey:Int", OrderBy = " AccountID", AdditionalFields = "AccountID")] 

		//[DataMember(Order = 5)]
		//[DictionaryMap(DictionaryKey = "AccountID", Command = "SELECT AccountID,PermissionType,cast (Value  as int) AS 'Value' FROM User_GUI_AccountPermission where TargetIsGroup = 1 and TargetID =  @UserID:Int Order by AccountID")]
		//public Dictionary<int, List<AssignedPermission>> AssignedPermissions = new Dictionary<int, List<AssignedPermission>>();

		

		public static User GetUserByID(int id)
		{
			User user=null;
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
				user=MapperUtility.ExpandObject<User>(user, customApply);
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
			List<User> users=new List<User>();
			ThingReader<User> thingReader;
			Func<FieldInfo, IDataRecord, object> customApply = CustomApply;			
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand("SELECT UserID,Name,AccountAdmin,Email FROM User_GUI_User ORDER BY UserID");
				

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
			return users;
		}

		public void UserOperations(SqlOperation sqlOperation)
		{
			string command = @"User_Operations(@Action:Int,@UserID:Int,@Name:NvarChar,1,@AccountAdmin:bit,@Email:NvarChar,@Password:NvarChar)";
			MapperUtility.SaveOrRemoveSimpleObject<User>(command, CommandType.StoredProcedure, sqlOperation, this,string.Empty);
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
		

		

		
	}
}
