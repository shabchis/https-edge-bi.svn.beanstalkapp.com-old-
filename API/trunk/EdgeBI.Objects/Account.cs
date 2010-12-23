using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data;
using System.Reflection;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;

namespace EdgeBI.Objects
{
	[DataContract]
	[TableMap("User_GUI_Account")]
	public class Account
	{
		[DataMember(Order = 1)]
		[FieldMap("ID", IsKey = true, RecursiveLookupField = true, IsDistinct = true)]
		public int? ID;

		[DataMember(Order = 2)]
		[FieldMap("Name")]
		public string Name;


		[FieldMap("Parent_ID")]
		public int? ParentID;

		[DataMember(Order=4)]
		public List<string> CalculatedPermission;

		[DataMember(Order = 3)]
		public List<Account> ChildAccounts = new List<Account>();

		private static object CustomApply(FieldInfo info, IDataRecord reader)
		{
			throw new NotImplementedException();
		}

		public static List<Account> GetAccount(int? id, bool firstTime,int userId)
		{
			ThingReader<Account> accountReader;
			ThingReader<CalculatedPermission> calculatedPermissionReader;
			List<CalculatedPermission> calculatedPermissionList = new List<CalculatedPermission>();
			List<CalculatedPermission> tempCalculatedPermission;
			List<Account> returnObject = new List<Account>();
			Dictionary<int?, Account> parents = new Dictionary<int?, Account>();
			Func<FieldInfo, IDataRecord, object> customApply = CustomApply;
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = null;
				sqlCommand = DataManager.CreateCommand("User_GetAllPermissions(@UserID:Int)", CommandType.StoredProcedure);
				sqlCommand.Parameters["@UserID"].Value = userId;
				calculatedPermissionReader = new ThingReader<CalculatedPermission>(sqlCommand.ExecuteReader(), customApply);
				while (calculatedPermissionReader.Read())
				{
					calculatedPermissionList.Add(calculatedPermissionReader.Current);					
				}
				calculatedPermissionReader.Dispose();

				sqlCommand = DataManager.CreateCommand("SELECT DISTINCT ID,Name,Parent_ID FROM [V_User_GUI_Accounts]   ORDER BY Parent_ID", CommandType.Text);
				accountReader = new ThingReader<Account>(sqlCommand.ExecuteReader(), CustomApply);
				while (accountReader.Read())
				{
					Account account = accountReader.Current;
					account.CalculatedPermission = calculatedPermissionList.FindAll(calculatedPermission => calculatedPermission.AccountID == account.ID).Select(calc=>calc.Path).ToList();
					
					
					if (account.ParentID == null || !parents.ContainsKey(account.ParentID))
						returnObject.Add(account);
					else					
						parents[account.ParentID].ChildAccounts.Add(account);
			
					if (!parents.ContainsKey(account.ID))
						parents.Add(account.ID, account);
				}
			}
			return returnObject;
		}
		


	}
	[DataContract]
	public class ChildAccount
	{
		[DataMember(Order = 1)]
		[FieldMap("Account_ID")]
		public int ID;

		[DataMember(Order = 2)]
		[FieldMap("Account_Name")]
		public string Name;


	}
}
