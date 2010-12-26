using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eggplant.LightweightORM;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;
using Easynet.Edge.Core;

namespace Services.Checksum.Objects
{
	public enum TestStatus
	{
		Pending = 0,
		Succeeded = 1,
		Failed = 2
	}

	public class Test: Thing
	{
		public int ID = -1;
		public TestType TestType;
		public TestStatus Status = TestStatus.Pending;
		public SettingsCollection Metadata = new SettingsCollection();
		public DateTime DateCreated = DateTime.MinValue;
		public DateTime DateUpdated = DateTime.MinValue;

		public override void ApplyValues(FieldDictionary fields)
		{
			this.ID = fields.GetValue<int>("TestID", -1);
			this.TestType = fields.GetThing<TestType>("TestTypeID");
			this.Status = fields.GetThing<TestStatus>("Status", TestStatus.Pending);
			this.Metadata.Definition = fields.GetValue<string>("Metadata");
			this.DateCreated = fields.GetValue<DateTime>("DateCreated", DateTime.MinValue);
			this.DateUpdated = fields.GetValue<DateTime>("DateUpdated", DateTime.MinValue);
		}

		public Test(TestType ofType)
		{
			this.TestType = ofType;
		}

		public override void Save()
		{
			const string INSERT = @"
				insert into Checksum_Data_Test (TestTypeID, Metadata)
				values (@testTypeID:Int, @metadata:NVarChar);
				select @@IDENTITY;
			";
			const string UPDATE = @"
				update Checksum_Data_Test set
					Metadata = @metadata:NVarChar
				where
					TestID = @testID:int
			";
			string cmdText = ID < 0 ? INSERT : UPDATE;

			using (DataManager.Current.OpenConnection())
			{
				SqlCommand cmd = DataManager.CreateCommand(cmdText);
				if (cmd.Parameters.Contains("@testTypeID"))
					cmd.Parameters["@testTypeID"].Value = ofType.ID;
				else if (cmd.Parameters.Contains("@testID"))
					cmd.Parameters["@testID"].Value = this.ID;
				cmd.Parameters["@metadata"].Value = Metadata.Definition;
				object result = cmd.ExecuteScalar();
				
				// Update ID if necessary
				if (result is int)
					this.ID = (int)result;
			}
		}

		public static Test GetByID(int testID)
		{
			const string SELECT = "select * from Checksum_Data_Test where TestID = @testID:int";
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand cmd = DataManager.CreateCommand(SELECT);
				cmd.Parameters["@testID"].Value = testID;
				using (ThingReader<Test> reader = new ThingReader<Test>(cmd.ExecuteReader))
				{
					return reader.Read() ? reader.Current : null;
				}
			}
		}
	}

	public class asd : Thing
	{
		public int ID;
		public string Name;
		public DateTime DateCreated;
		public DateTime DateModified;

		public override void ApplyValues(FieldDictionary fields)
		{
			this.ID = fields.GetValue<int>("TestFlowID", -1);
			this.Name = fields.GetValue<string>("TestFlowName");
			this.DateCreated = fields.GetValue<string>("DateCreated", DateTime.MinValue);
			this.DateModified = fields.GetValue<string>("DateModified", DateTime.MinValue);
		}
	}
}
