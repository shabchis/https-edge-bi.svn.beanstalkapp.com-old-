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
		[FieldMap("Value",Cast="cast (Value  as int) AS 'Value' ")]
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
		public PermissionOperation permissionOperation;
		public List<AccountPermissionData> accountsPermissionsData;
		public static void AddPermissions(int userID, List<AccountPermissionData> accountsPermissionsData,bool targetIsGroup)
		{
			foreach (AccountPermissionData accountPermissionData in accountsPermissionsData)
			{

				using (DataManager.Current.OpenConnection())
				{
					//Check if permission exisit

					foreach (AssignedPermission assignedPermission in accountPermissionData.assignedPermissions)
					{
						SqlCommand sqlCommand = DataManager.CreateCommand(@"SELECT COUNT(PermissionType) 
																			FROM User_GUI_AccountPermission
																			WHERE   TargetIsGroup=@TargetIsGroup:Bit AND 
																					AccountID=@AccountID:Int AND
																					TargetID=@TargetID:Int AND
																					PermissionType=@PermissionType:NvarChar");
						sqlCommand.Parameters["@AccountID"].Value = accountPermissionData.AccountID;
						sqlCommand.Parameters["@TargetID"].Value = userID;
						sqlCommand.Parameters["@PermissionType"].Value = assignedPermission.Path;
						sqlCommand.Parameters["@TargetIsGroup"].Value = targetIsGroup;
						if (Convert.ToInt32(sqlCommand.ExecuteScalar()) == 0) //permission nt exist then add it
						{
							sqlCommand = DataManager.CreateCommand(@"INSERT INTO  User_GUI_AccountPermission
																	(AccountID,TargetID,TargetIsGroup,PermissionType,Value)
																	VALUES
																	(@AccountID:Int,@TargetID:Int,@TargetIsGroup:Bit,@PermissionType:NvarChar,@Value:Bit)");
							sqlCommand.Parameters["@AccountID"].Value = accountPermissionData.AccountID;
							sqlCommand.Parameters["@TargetID"].Value = userID;
							sqlCommand.Parameters["@PermissionType"].Value = assignedPermission.Path;
							sqlCommand.Parameters["@Value"].Value = assignedPermission.Assignment;
							sqlCommand.Parameters["@TargetIsGroup"].Value = targetIsGroup;
							sqlCommand.ExecuteNonQuery();
						}

					}

				}

			}
		}
		public static void UpdatePermissions(int userID, List<AccountPermissionData> accountsPermissionsData, bool targetIsGroup)
		{
			foreach (AccountPermissionData accountPermissionData in accountsPermissionsData)
			{

				using (DataManager.Current.OpenConnection())
				{
					//Check if permission exisit

					foreach (AssignedPermission assignedPermission in accountPermissionData.assignedPermissions)
					{
						SqlCommand sqlCommand = DataManager.CreateCommand(@"UPDATE User_GUI_AccountPermission 
																			SET Value=@Value:Bit
																			WHERE   TargetIsGroup=@TargetIsGroup:Bit AND 
																					AccountID=@AccountID:Int AND
																					TargetID=@TargetID:Int AND
																					PermissionType=@PermissionType:NvarChar");
						sqlCommand.Parameters["@AccountID"].Value = accountPermissionData.AccountID;
						sqlCommand.Parameters["@TargetID"].Value = userID;
						sqlCommand.Parameters["@PermissionType"].Value = assignedPermission.Path;
						sqlCommand.Parameters["@Value"].Value = assignedPermission.Assignment;
						sqlCommand.Parameters["@TargetIsGroup"].Value = targetIsGroup;
						sqlCommand.ExecuteNonQuery();
					}

				}

			}
			
		}

		public static void RemovePermmissions(int userID, List<AccountPermissionData> accountsPermissionsData, bool targetIsGroup)
		{
			foreach (AccountPermissionData accountPermissionData in accountsPermissionsData)
			{

				using (DataManager.Current.OpenConnection())
				{
					//Check if permission exisit

					foreach (AssignedPermission assignedPermission in accountPermissionData.assignedPermissions)
					{
						SqlCommand sqlCommand = DataManager.CreateCommand(@"DELETE FROM User_GUI_AccountPermission 																			
																			WHERE   TargetIsGroup=@TargetIsGroup:Bit AND 
																					AccountID=@AccountID:Int AND
																					TargetID=@TargetID:Int AND
																					PermissionType=@PermissionType:NvarChar");
						sqlCommand.Parameters["@AccountID"].Value = accountPermissionData.AccountID;
						sqlCommand.Parameters["@TargetID"].Value = userID;
						sqlCommand.Parameters["@PermissionType"].Value = assignedPermission.Path;
						sqlCommand.Parameters["@TargetIsGroup"].Value = targetIsGroup;
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

	public enum PermissionOperation
	{
		Add=1,
		Update=2,
		Delete=3
	}

	

}
