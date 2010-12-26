using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Services;
using System.Data.SqlClient;
using EdgeBI.Data.Readers;
using Easynet.Edge.Core;
using Services.Checksum.Objects;

namespace Services.Checksum
{
	public abstract class PhaseHandler
	{
		public ServiceInstanceInfo Instance;
		public Test Test;

		public virtual void Init() { }
		public virtual void Prepare() { }
		public abstract void Execute();
	}

	public class FileToOltpPhaseHandler: PhaseHandler
	{
		static int BulkBatchSize = Int32.Parse(AppSettings.Get(typeof(PhaseHandler), "Prepare.BatchSize"));
		int _channelID;
		Type _readerType;


		public override void Init()
		{
			string filePath = Instance.Configuration.Options["SourceFilePath"];
			if (String.IsNullOrEmpty(filePath))
				throw new ConfigurationException(
					"SourceFilePath option is missing.");

			Test.Metadata.Add("FileToOLTP.SourceFilePath", filePath);
			
			string raw_channelID = Instance.Configuration.Options["ChannelID"];
			if (!Int32.TryParse(raw_channelID, out _channelID))
				throw new ConfigurationException(
					"ChannelID option is invalid.");

			Test.Metadata.Add("FileToOLTP.ChannelID", raw_channelID);

			string readerTypeName = Instance.Configuration.Options["ReaderType"];
			_readerType = Type.GetType(readerTypeName, false);
			if (_readerType == null)
				throw new ConfigurationException(String.Format(
					"Specified reader type '{0}' could not be found.", readerTypeName));

			Test.Metadata.Add("FileToOLTP.ReaderType", readerTypeName);
		}

		public override void Prepare()
		{
			// Connection should be open at this point, but just in case
			using (DataManager.Current.OpenConnection())
			{
				using (SqlBulkCopy bulk = DataManager.CreateBulkCopy(SqlBulkCopyOptions.Default))
				{
					bool tableInit = true;
					bulk.BatchSize = BulkBatchSize;
					bulk.DestinationTableName = "TEMP_ImportedData";

					using (RowReader<ImportFileRow> reader = Activator.CreateInstance(_readerType))
					{
						while (reader.Read())
						{
							if (tableInit)
							{
								// Init the temp table using the first row
								SqlCommand createTableCmd = DataManager.CreateCommand(
									"create table #TEMP_ImportedData
							}
							//bulk.WriteToServer(
						}
					}
				}
			}
		}

	}
}
