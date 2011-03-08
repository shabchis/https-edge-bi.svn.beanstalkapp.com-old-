using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core;
using EdgeBI.Data.Pipeline;
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;
using System.Data;

namespace Services.Data.Pipeline
{
    [TableMap("Delivery")]
	public class Delivery
	{
        [FieldMap("AccountID")]
        public int AccountID;
        [FieldMap("DeliveryID")]
        public int DeliveryID;
        [FieldMap("Description")]
		public string Description;
        [FieldMap("DateCreated")]
		public DateTime DateCreated;
        [FieldMap("DateModified")]
		public DateTime DateModified;
        [FieldMap("CreatedByServiceInstanceID")]
        public long CreatedByServiceInstanceID;
        [ListMap()]
        public List<DeliveryFile> Files;
        [FieldMap("Parameters")]
        public SettingsCollection Parameters;
        [FieldMap("DeliveryState")]
        public eDeliveryState DeliveryState;
        
        public void Save()
        {
            try
            {
                SaveDelivery2DB();
            }
            catch
            {
                throw;
            }
        }

        public void Download()
        {
            foreach (DeliveryFile dfile in Files)
            {
                dfile.Download();
            }
        }

        private void SaveDelivery2DB()
        {
            try
            {
                string command;

                command = "SP_AddUpdateDelivery(@DeliveryID:int,@Description:nvarchar(50),@DeliveryState:int,@CreatedByServiceInstanceID:int,@Parameters:text,@DateCreated:datetime,@DateModified:datetime,@RETVAL:int)";
                this.DeliveryID = mappper.SaveOrRemoveSimpleObject<Delivery>(command, this);

                foreach (DeliveryFile dfile in Files)
                {
                    dfile.DeliveryID = this.DeliveryID;
                    dfile.FilePath = FilesManager.GetDeliveryFilePath(dfile.FileRootPath, DateTime.Now, dfile.DeliveryID, dfile.FileName, this.AccountID);
                    command = "SP_AddUpdateDeliveryFile(@FileID:Int,@DeliveryID:Int,@FilePath:text,@DownloadUrl:text,@TargetDateTime:datetime,@ReaderType:nvarchar(50),@Parameters:text,@DateCreated:datetime,@DateModified:datetime,@RETVAL:int)"; 
                    dfile.FileID = mappper.SaveOrRemoveSimpleObject<DeliveryFile>(command, dfile);
                    
                    foreach (DeliveryFileStatus Status in dfile.HandledStatus)
                    {
                        Status.DeliveryID = this.DeliveryID;
                        command = "SP_AddUpdateDeliveryStatus(@DeliveryID:Int,@ServiceInstanceID:Int,@State:smallint,@RETVAL:int)";
                        mappper.SaveOrRemoveSimpleObject<DeliveryFileStatus>(command, Status);
                    }
                }
            }
            catch
            {
                throw;
            }

        }

        public void GetDelivery(int deliveryId)
        {
            using (DataManager.Current.OpenConnection())
            {
                SqlCommand EngineCmd = DataManager.CreateCommand("SP_GetDeliveryData(@DeliveryID:int)", CommandType.StoredProcedure);
                EngineCmd.Parameters["@DeliveryID"].Value = deliveryId;
                SqlDataReader reader = EngineCmd.ExecuteReader();
                reader.Read();
                if (!reader["DeliveryID"].Equals(System.DBNull.Value)) this.DeliveryID = (int)reader["DeliveryID"];
                if (!reader["Description"].Equals(System.DBNull.Value)) this.Description = (string)reader["Description"];
                if (!reader["DeliveryState"].Equals(System.DBNull.Value)) this.DeliveryState = (eDeliveryState)reader["DeliveryState"];
                if (!reader["Parameters"].Equals(System.DBNull.Value)) this.Description = (string)reader["Parameters"];
                reader.Close();

                EngineCmd = DataManager.CreateCommand("SP_GetDeliveryFileData(@DeliveryID:int)", CommandType.StoredProcedure);
                EngineCmd.Parameters["@DeliveryID"].Value = deliveryId;
                reader = EngineCmd.ExecuteReader();
                while (reader.Read())
                {
                    DeliveryFile df = new DeliveryFile();
                    if (!reader["FileID"].Equals(System.DBNull.Value)) df.FileID = (int)reader["FileID"];
                    if (!reader["DeliveryID"].Equals(System.DBNull.Value)) df.DeliveryID = (int)reader["DeliveryID"];
                    if (!reader["FileName"].Equals(System.DBNull.Value)) df.FileName = (string)reader["FileName"];
                    if (!reader["FilePath"].Equals(System.DBNull.Value)) df.FilePath = (string)reader["FilePath"];
                    if (!reader["DownloadUrl"].Equals(System.DBNull.Value)) df.DownloadUrl  = (string)reader["DownloadUrl"];
                    if (!reader["TargetDateTime"].Equals(System.DBNull.Value)) df.TargetDateTime  = (DateTime)reader["TargetDateTime"];
                    if (!reader["ReaderType"].Equals(System.DBNull.Value)) df.ReaderType = (string)reader["ReaderType"];
                    if (!reader["Parameters"].Equals(System.DBNull.Value)) df.Parameters  = (string)reader["Parameters"];
                    if (this.Files == null) this.Files = new List<DeliveryFile>();
                    this.Files.Add(df);
                }
                reader.Close();
            }
        }

        public int GetDeliveryIdByInstance(long serviceId)
        {
            int DeliveryID =0;
            using (DataManager.Current.OpenConnection())
            {
                SqlCommand EngineCmd = DataManager.CreateCommand("SP_GetDeliveryIdByInstance(@ServiceInstanceID:int)", CommandType.StoredProcedure);
                EngineCmd.Parameters["@ServiceInstanceID"].Value = serviceId;
                SqlDataReader reader = EngineCmd.ExecuteReader();
                reader.Read();
                if (!reader["DeliveryID"].Equals(System.DBNull.Value))
                    DeliveryID = (int)reader["DeliveryID"];
                else
                    throw new Exception("No delivery id by instance id");
                reader.Close();
                return DeliveryID;
            }

        }
    }
    
    [TableMap("DeliveryFile")]
	public class DeliveryFile
	{
        [FieldMap("FileID")]
        public int FileID;
        [FieldMap("AccountID")]
        public int? AccountID;
        [FieldMap("DeliveryID")]
		public int DeliveryID;
        [FieldMap("FileName")]
        public string FileName;
        public string FileRootPath;
        [FieldMap("FilePath")]
		public string FilePath;
        [FieldMap("DownloadUrl")]
        public string DownloadUrl;
        [FieldMap("TargetDateTime")]
		public DateTime TargetDateTime;
        [ListMap()]
        public List<DeliveryFileStatus> HandledStatus;
        [FieldMap("ReaderType")]
		public string ReaderType;
        [FieldMap("Parameters")]
        public string Parameters;
        [FieldMap("DateCreated")]
		public DateTime DateCreated;
        [FieldMap("DateModified")]
        public DateTime DateModified;

        public void Download()
        {
            FilesManager.DownloadFile(DownloadUrl, FilePath); 
        }

		public IDeliveryFileReader CreateReader()
		{
			throw new NotImplementedException();
		}
	}
    [TableMap("DeliveryFileStatus")]
    public class DeliveryFileStatus
    {
        [FieldMap("DeliveryID")]
        public int DeliveryID;
        [FieldMap("State")]
        public eDeliveryFileState State;
        [FieldMap("ServiceInstanceID")]
        public int ServiceInstanceID;
    }

    public enum eDeliveryFileState
    {
        New = 0,
        Retrieved = 1,
        Processed = 2
    }
    public enum eDeliveryState
    {
        New = 0,
        Run = 1,
        Rerun = 2,
        Complete = 3
    }
}
