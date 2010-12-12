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



		[DataMember(Order = 3)]
		public List<Account> ChildAccounts = new List<Account>();


		private static object CustomApply(FieldInfo info, IDataRecord reader)
		{
			throw new NotImplementedException();
		}

		public static List<Account> GetAccount(int? id, bool firstTime)
		{


			ThingReader<Account> thingReader;
			List<Account> returnObject = new List<Account>();
			Func<FieldInfo, IDataRecord, object> customApply = CustomApply;
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = null;
				if (firstTime)
				{
					if (id != null)
					{
						sqlCommand = DataManager.CreateCommand("SELECT DISTINCT ID,Name,Parent_ID FROM [V_User_GUI_Accounts] WHERE ID=@ID:Int ORDER BY Parent_ID", CommandType.Text);
						sqlCommand.Parameters["@ID"].Value = id;
					}
					else
						sqlCommand = DataManager.CreateCommand("SELECT DISTINCT ID,Name,Parent_ID FROM [V_User_GUI_Accounts] WHERE Parent_ID IS NULL  ORDER BY Parent_ID", CommandType.Text);
					firstTime = false;
				}
				else
				{
					if (id != null)
					{
						sqlCommand = DataManager.CreateCommand("SELECT DISTINCT ID,Name,Parent_ID FROM [V_User_GUI_Accounts] WHERE Parent_ID=@ID:Int  ORDER BY Parent_ID", CommandType.Text);
						sqlCommand.Parameters["@ID"].Value = id;
					}

					firstTime = false;

				}


				thingReader = new ThingReader<Account>(sqlCommand.ExecuteReader(), CustomApply);

				while (thingReader.Read())
				{
					Account account = thingReader.Current;
					returnObject.Add(account);

				}

			}
			if (returnObject != null && returnObject.Count != 0)
			{
				foreach (Account account in returnObject)
				{
					if (account.ID != account.ParentID)
						account.ChildAccounts = GetAccount(account.ID, firstTime);
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
