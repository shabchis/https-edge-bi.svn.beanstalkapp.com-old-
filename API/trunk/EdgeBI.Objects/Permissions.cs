using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using System.Reflection;
using System.Data;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;



namespace EdgeBI.Objects
{
	//[TableMap("User_GUI_User")]
	public class AssignedPermission
	{

		[DataMember]
		[FieldMap("PermissionType")]
		public string Path;

		[DataMember]
		[FieldMap("Value", Cast = "cast (Value  as int) AS 'Value' ")]
		public PermissionAssignmentType Assignment;

	}
	public class CalculatedPermission
	{
		[DataMember]
		[FieldMap("AccountID")]
		public int AccountID;
		[DataMember]
		[FieldMap("PermissionType")]
		public string Path;

	}
	public class PermissionRequest
	{
		public int AccountID { get; set; }
		public string Path { get; set; }
	}
	public enum PermissionAssignmentType
	{
		Allow = 1,
		Deny = 0
	}
	public class AssignedPermissionData
	{
		public SqlOperation permissionOperation;
		public List<AccountPermissionData> accountsPermissionsData;
		public static void PermissionOperations(int userID, List<AccountPermissionData> accountsPermissionsData, bool targetIsGroup, SqlOperation sqlOperation)
		{
			foreach (AccountPermissionData accountPermissionData in accountsPermissionsData)
			{
				using (DataManager.Current.OpenConnection())
				{
					foreach (AssignedPermission assignedPermission in accountPermissionData.assignedPermissions)
					{

						SqlCommand sqlCommand = null;
						sqlCommand = DataManager.CreateCommand("Permissions_Operations(@Action:Int,@AccountID:Int,@TargetID:Int,@TargetIsGroup:Bit,@PermissionType:NvarChar,@Value:Bit)", CommandType.StoredProcedure);
						sqlCommand.Parameters["@Action"].Value = sqlOperation;
						sqlCommand.Parameters["@AccountID"].Value = accountPermissionData.AccountID;
						sqlCommand.Parameters["@TargetID"].Value = userID;
						sqlCommand.Parameters["@TargetIsGroup"].Value = targetIsGroup;
						sqlCommand.Parameters["@PermissionType"].Value = assignedPermission.Path;
						if (sqlOperation==SqlOperation.Insert || sqlOperation==SqlOperation.Update)																
									sqlCommand.Parameters["@Value"].Value = assignedPermission.Assignment;						
						sqlCommand.ExecuteNonQuery();
					}
				}
			}
		}
	}
	public class AccountPermissionData
	{
		public int AccountID;
		public List<AssignedPermission> assignedPermissions;
	}
	[TableMap("Constant_PermissionType")]
	public class Permission
	{
		[DataMember(Order = 1)]
		[FieldMap("Path")]
		public string Path;
		[DataMember(Order = 2)]
		public List<Permission> ChildPermissions = new List<Permission>();
		public static List<Permission> GetAllPermissionTypeTree()
		{
			List<Permission> returnObject = new List<Permission>();
			ThingReader<Permission> thingReader;
			Stack<Permission> stackPermission = new Stack<Permission>();



			using (DataManager.Current.OpenConnection())
			{
				using (SqlCommand sqlCommand = DataManager.CreateCommand("SELECT Path FROM Constant_PermissionType ORDER BY Path"))
				{
					thingReader = new ThingReader<Permission>(sqlCommand.ExecuteReader(), null);
					while (thingReader.Read())
					{
						Permission permission = (Permission)thingReader.Current;
						if (stackPermission.Count == 0)
							stackPermission.Push(permission);
						else if (permission.Path.StartsWith(stackPermission.Peek().Path))
						{
							stackPermission.Peek().ChildPermissions.Add(permission);
							stackPermission.Push(permission);
						}
						else
						{
							while (stackPermission.Count != 1 && !permission.Path.StartsWith(stackPermission.Peek().Path))
							{

								stackPermission.Pop();
							}
							if (!permission.Path.StartsWith(stackPermission.Peek().Path))
							{
								returnObject.Add(stackPermission.Pop());
								stackPermission.Push(permission);
							}
							else
							{
								stackPermission.Peek().ChildPermissions.Add(permission);
								stackPermission.Push(permission);
							}
						}
					}
				}
			}
			while (stackPermission.Count > 1)
			{
				stackPermission.Pop();
			}
			if (stackPermission.Count > 0)
				returnObject.Add(stackPermission.Pop());

			returnObject = Order(returnObject);

			return returnObject;
		}

		public static List<string> GetAllPermissionTypeList()
		{
			List<string> permissions = new List<string>();
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

		private static List<Permission> Order(List<Permission> returnObject)
		{
			if (returnObject != null && returnObject.Count > 0)
			{
				IEnumerable<Permission> permissions = returnObject.OrderBy(permission => permission.Path);

				foreach (Permission permission in permissions)
				{
					permission.ChildPermissions = Order(permission.ChildPermissions);
				}
				returnObject = permissions.ToList();
			}
			return returnObject;
		}

	}


	public enum SqlOperation
	{
		Insert = 1,
		Update = 2,
		Delete = 3
	}



}
