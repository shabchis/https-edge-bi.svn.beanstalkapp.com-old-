using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using System.Reflection;
using System.Data;
using Easynet.Edge.Core;
using System.Text.RegularExpressions;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;


namespace EdgeBI.Objects
{
	[DataContract]
	[TableMap("Constant_Menu")]
	public class Menu
	{
		[DataMember(Order = 1)]
		[FieldMap("ID", RecursiveLookupField = true)]
		public int ID;

		[DataMember(Order = 2)]
		[FieldMap("Name")]
		public string Name;

		[DataMember(Order = 3)]
		[FieldMap("Path")]
		public string Path;

		[DataMember(Order = 4)]
		[FieldMap("Ordinal")]
		public int Ordinal;


		[DataMember(Order = 5)]
		[FieldMap("MetaData", UseApplyFunction = true)]  //todo change to true and get as dictionary
		public Dictionary<string, string> MetaData;

		[DataMember(Order = 6)]
		public List<Menu> ChildItems = new List<Menu>();


		public static List<Menu> GetMenu(int userId)
		{

			Stack<Menu> stackMenu = new Stack<Menu>();

			string newPath = string.Format("{0}%", string.Empty); //for future get specific menu and sub menu			
			ThingReader<Menu> thingReader;
			List<Menu> returnObject = new List<Menu>();
			Dictionary<int, List<Menu>> menuesByLevel = new Dictionary<int, List<Menu>>();
			Func<FieldInfo, IDataRecord, object> customApply = CustomApply;
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand("Menu_GetMenuByUserPermission(@userID:Int,@menuPath:NvarChar)", CommandType.StoredProcedure);
				sqlCommand.Parameters["@menuPath"].Value = newPath;
				sqlCommand.Parameters["@userID"].Value = userId;

				thingReader = new ThingReader<Menu>(sqlCommand.ExecuteReader(), CustomApply);
				
				while (thingReader.Read())
				{

					Menu menu = (Menu)thingReader.Current;

					if (stackMenu.Count == 0)
						stackMenu.Push(menu);
					else if (menu.Path.StartsWith(stackMenu.Peek().Path))
					{
						stackMenu.Peek().ChildItems.Add(menu);
						stackMenu.Push(menu);
					}
					else
					{
						while (stackMenu.Count != 1 && !menu.Path.StartsWith(stackMenu.Peek().Path))
						{

							stackMenu.Pop();
						}
						if (!menu.Path.StartsWith(stackMenu.Peek().Path))
						{
							returnObject.Add(stackMenu.Pop());
							stackMenu.Push(menu);
						}
						else
						{
							stackMenu.Peek().ChildItems.Add(menu);
							stackMenu.Push(menu); //bug 157 it was with comment
						}
					}
				}
			}
			//stackMenu.Pop();
			while (stackMenu.Count > 1)
			{
				stackMenu.Pop();
			}
			if (stackMenu.Count > 0)
				returnObject.Add(stackMenu.Pop());

			returnObject = Order(returnObject);
			
			return returnObject;

		}

		private static List<Menu> Order(List<Menu> returnObject)
		{
			if (returnObject != null && returnObject.Count > 0)
			{
				IEnumerable<Menu> menues = returnObject.OrderBy(menu => menu.Ordinal);

				foreach (Menu menu in menues)
				{
					menu.ChildItems = Order(menu.ChildItems);
				}
				returnObject = menues.ToList();
			}
			return returnObject;
		}
		private static Dictionary<string, string> CustomApply(FieldInfo info, IDataRecord reader)
		{

			SettingsCollection settings=null;
			
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

		//System.Data.SqlClient.SqlCommand cmd = Easynet.Edge.Core.Data.DataManager.CreateCommand("User_GetAllPermissions(@userID:int)");
	}
}
