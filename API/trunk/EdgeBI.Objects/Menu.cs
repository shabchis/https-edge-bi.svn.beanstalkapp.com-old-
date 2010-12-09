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
	[TableMap("API_Menus")]
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
		[FieldMap("MetaData", UseApplyFunction = true)]  //todo change to true and get as dictionary
		public Dictionary<string, string> MetaData;

		[DataMember(Order = 5)]
		public List<Menu> ChildMenues = new List<Menu>();


		public static List<Menu> GetMenuByParentID(string path)
		{
			string newPath = string.Format("{0}%", path);
			int level = -1;
			ThingReader<Menu> thingReader;
			List<Menu> returnObject = new List<Menu>();
			Func<FieldInfo, IDataRecord, object> customApply = CustomApply;
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand sqlCommand = DataManager.CreateCommand("select [ID],[Name],[Path],[MetaData] FROM [API_Menus] WHERE [Path] LIKE @Path:NVarChar ORDER BY Path");
				sqlCommand.Parameters["@Path"].Value = newPath;

				thingReader = new ThingReader<Menu>(sqlCommand.ExecuteReader(), CustomApply);
				List<Menu> currrentList = null;
				while (thingReader.Read())
				{

					Menu menu = (Menu)thingReader.Current;
					if (Regex.Matches(menu.Path, "/").Count < 1)
					{
						level = Regex.Matches(menu.Path, "/").Count;
						returnObject.Add(menu);
						currrentList = returnObject;

					}
					else
					{
						if (level < Regex.Matches(menu.Path, "/").Count)
						{
							currrentList = currrentList[currrentList.Count - 1].ChildMenues;
						}
						level = Regex.Matches(menu.Path, "/").Count;
						currrentList.Add(menu);

					}

				}
			}
			
			return returnObject;


		}
		
		private static Dictionary<string, string> CustomApply(FieldInfo info, IDataRecord reader)
		{


			Dictionary<string, string> metadata = new Dictionary<string, string>();
			Regex settingParser = new Regex(@"\b[A-Za-z]+[A-Za-z0-9-_/=\+]*\s*:[^;:]*");
			Regex keyParser = new Regex(@"^\b[A-Za-z]+[A-Za-z0-9-_/=\+]*");
			Regex valueParser = new Regex(@"[^;:]*$");

			//SettingsCollection settings = null;
			try
			{
				if (reader != null)
				{
					//settings = new SettingsCollection(reader[info.Name].ToString());
					foreach (Match setting in settingParser.Matches(reader[info.Name].ToString()))
					{
						string key, val;
						Match keyMatch = keyParser.Match(setting.Value);
						if (keyMatch.Success)
							key = keyMatch.Value.Trim();
						else
							continue;

						// Extract the value
						Match valMatch = valueParser.Match(setting.Value);
						if (valMatch.Success)
							val = valMatch.Value.Trim();
						else
							continue;

						metadata.Add(key, val);

					}
				}

			}
			catch (Exception)
			{

				throw;
			}
			return metadata;


		}

		//System.Data.SqlClient.SqlCommand cmd = Easynet.Edge.Core.Data.DataManager.CreateCommand("User_GetAllPermissions(@userID:int)");
	}
}
