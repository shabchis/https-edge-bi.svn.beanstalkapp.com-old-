using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eggplant.LightweightORM;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;

namespace Services.Checksum.Objects
{
	public class TestType: Thing
	{
		public int ID;
		public string Name;
		public DateTime DateCreated;
		public DateTime DateModified;

		public override void ApplyValues(FieldDictionary fields)
		{
			this.ID = fields.GetValue<int>("TestTypeID", -1);
			this.Name = fields.GetValue<string>("TestTypeName");
			this.DateCreated = fields.GetValue<DateTime>("DateCreated", DateTime.MinValue);
			this.DateModified = fields.GetValue<DateTime>("DateModified", DateTime.MinValue);
		}

		#region Queries
		//======================

		public static TestType GetByID(int id)
		{
			string cmdText = "select * from Checksum_Config_TestType where TestTypeID = @id:int";
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand cmd = DataManager.CreateCommand(cmdText);
				cmd.Parameters["@id"].Value = id;
				using (ThingReader<TestType> reader = new ThingReader<TestType>(cmd.ExecuteReader()))
				{
					if (reader.Read())
						return reader.Current;
					else
						return null;
				}
			}
		}

		//======================
		#endregion
	}

	public class TestItemGroup : Thing
	{
		public int ID;
		public string Name;
		public DateTime DateCreated;
		public DateTime DateModified;

		public override void ApplyValues(FieldDictionary fields)
		{
			this.ID = fields.GetValue<int>("GroupID", -1);
			this.Name = fields.GetValue<string>("GroupName");
			this.DateCreated = fields.GetValue<string>("DateCreated", DateTime.MinValue);
			this.DateModified = fields.GetValue<string>("DateModified", DateTime.MinValue);
		}

		#region Queries
		//======================

		public static List<TestItemGroup> GetByTestType(TestType testType)
		{
			string cmdText = @"
			select
				*
			from
				Checksum_Config_ItemGroup
			where
				ID in (select GroupID from Checksum_Config_TestType_Groups where TestTypeID = @testTypeID:int)";

			List<TestItemGroup> list = new List<TestItemGroup>();
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand cmd = DataManager.CreateCommand(cmdText);
				cmd.Parameters["@testTypeID"].Value = testType.ID;

				using (ThingReader<TestItemGroup> reader = new ThingReader<TestItemGroup>(cmd.ExecuteReader()))
				{
					while(reader.Read())
						list.Add(reader.Current);
				}
			}

			return list;
		}

		//======================
		#endregion



	}

	public class TestItemDefinition : Thing
	{
		public int ID;
		public string PhaseName;
		public DateTime DateCreated;
		public DateTime DateModified;

		public override void ApplyValues(FieldDictionary fields)
		{
			this.ID = fields.GetValue<int>("DefinitionID", -1);
			this.PhaseName = fields.GetValue<string>("PhaseName");
			this.DateCreated = fields.GetValue<string>("DateCreated", DateTime.MinValue);
			this.DateModified = fields.GetValue<string>("DateModified", DateTime.MinValue);
		}

		#region Queries
		//======================

		public static List<TestItemDefinition> GetByGroup(TestItemGroup group)
		{
			string cmdText = @"
			select
				*
			from
				Checksum_Config_TestDefinition
			where
				DefinitionID in (select DefinitionID from Checksum_Config_ItemGroup_Definitions where GroupID = @groupID:int)";

			List<TestItemDefinition> list = new List<TestItemDefinition>();
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand cmd = DataManager.CreateCommand(cmdText);
				cmd.Parameters["@groupID"].Value = flow.ID;

				using (ThingReader<TestItemDefinition> reader = new ThingReader<TestItemDefinition>(cmd.ExecuteReader(),
					new ThingTranslation[]{
						new ThingTranslation(fields => group, "GroupID")
					}))
				{
					while (reader.Read())
						list.Add(reader.Current);
				}
			}

			return list;
		}

		//======================
		#endregion

	}
}
