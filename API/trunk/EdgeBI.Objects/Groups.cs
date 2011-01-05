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


		[FieldMap("IsActive", Show = false)]
		public bool IsActive = true;

		[DataMember(Order = 2)]
		[FieldMap("AccountAdmin")]
		public bool? IsAcountAdmin;



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

		public static void AddNewGroup(Group group)
		{
			string command = @"INSERT INTO User_GUI_UserGroup
								(Name,IsActive,AccountAdmin)
								VALUES (@Name:NvarChar,1,@AccountAdmin:bit)";
			if (MapperUtility.SaveOrRemoveSimpleObject<Group>(command, group) < 1)
				throw new Exception("No Rows afected");
		}
		public static void UpdateGroup(Group group)
		{
			string command = @"UPDATE User_GUI_UserGroup
								SET Name=@Name:NvarChar,
									IsActive=@IsActive:bit,
									AccountAdmin=@AccountAdmin:bit									
									WHERE GroupID=@GroupID:Int";
			if (MapperUtility.SaveOrRemoveSimpleObject<Group>(command, group) < 1)
				throw new Exception("No Rows afected");
		}

		public static void DeleteGroup(int groupID)
		{
			string command = @"DELETE FROM User_GUI_UserGroup
								WHERE GroupID =@GroupID:Int";
			Group group = new Group() { GroupID = groupID };
			if (MapperUtility.SaveOrRemoveSimpleObject<Group>(command, group) < 1)
				throw new Exception("No Rows afected");
		}
	}
}
