using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data;
using System.Reflection;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;
using Easynet.Edge.Core;


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

		[DataMember(Order = 3)]
		public List<string> Permissions;

		[DataMember(Order = 4)]
		[FieldMap("Level")]
		public int Level;

		[DataMember(Order = 5)]
		[FieldMap("AccountSettings", UseApplyFunction = true)] 
		public Dictionary<string, string> AccountSettings;

		[DataMember(Order = 6)]
		public List<Account> ChildAccounts = new List<Account>();

		private static object CustomApply(FieldInfo info, IDataRecord reader)
		{
			SettingsCollection settings = null;

			try
			{
				if (reader != null)
				{
					settings = new SettingsCollection(reader[info.Name].ToString());

				}

			}
			catch (Exception)
			{

				throw;
			}
			return settings.ToDictionary();
		}

		public static List<Account> GetAccount(int? id, bool firstTime, int userId)
		{
			ThingReader<Account> accountReader;
			ThingReader<CalculatedPermission> calculatedPermissionReader;
			List<CalculatedPermission> calculatedPermissionList = new List<CalculatedPermission>();			
			List<Account> returnObject = new List<Account>();
			Dictionary<int?, Account> parents = new Dictionary<int?, Account>();
			Func<FieldInfo, IDataRecord, object> customApply = CustomApply;
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = null;
				sqlCommand = DataManager.CreateCommand("User_CalculatePermissions(@UserID:Int)", CommandType.StoredProcedure);
				sqlCommand.Parameters["@UserID"].Value = userId;
				calculatedPermissionReader = new ThingReader<CalculatedPermission>(sqlCommand.ExecuteReader(), customApply);
				while (calculatedPermissionReader.Read())
				{
					calculatedPermissionList.Add(calculatedPermissionReader.Current);
				}
				calculatedPermissionReader.Dispose();

				sqlCommand = DataManager.CreateCommand("SELECT DISTINCT ID,Name,Parent_ID,AccountSettings,Level FROM [V_User_GUI_Accounts]   ORDER BY Parent_ID", CommandType.Text);
				accountReader = new ThingReader<Account>(sqlCommand.ExecuteReader(), CustomApply);
				while (accountReader.Read())
				{
					Account account = accountReader.Current;
					account.Permissions = calculatedPermissionList.FindAll(calculatedPermission => calculatedPermission.AccountID == account.ID).Select(calc => calc.Path).ToList();


					if (account.Permissions!=null && account.Permissions.Count > 0)
					{
						if (account.ParentID == null || !parents.ContainsKey(account.ParentID)) //If has no parent or parentid==null(is main father)
							returnObject.Add(account);
						else
							parents[account.ParentID].ChildAccounts.Add(account); //has father then add it has a child

						if (!parents.ContainsKey(account.ID)) //always add it to the parents
							parents.Add(account.ID, account);
					}
					
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
