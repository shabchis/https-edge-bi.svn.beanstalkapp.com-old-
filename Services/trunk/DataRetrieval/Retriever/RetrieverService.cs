using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using System.Net;
using Easynet.Edge.Core;
using System.Threading;
using System.Text.RegularExpressions;

namespace Easynet.Edge.Services.DataRetrieval.Retriever
{
	/// <summary>
	/// This class contain a list of services of for a certain server type
	/// (like RankerGoogleRetriever for example) the list is contain in a Dictionary
	/// that the key is start time and the object is a list with all the services that
	/// need to run in start time.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>01/07/2008</creation_date>
	public  class RetrieverService : BaseService
	{
		#region Consts
		/*=========================*/

		protected const int MaxRetries = 1;
		protected const int MaxInstancesReRuns = 31;
		protected const double PrecentDiffrence = 0.4;

		/*=========================*/
		#endregion

		#region Fields
		/*=========================*/

		protected int _maxInstancesReRuns;
		protected double _precentDiffrence;
		protected int _maxRetries;
		protected bool _adwordsFile = false;

		/*=========================*/
		#endregion
		

		#region Startup
		/*=========================*/

		/// <summary>
		/// Constructor
		/// </summary>
		public RetrieverService()
		{			
			try
			{
				string configValue = AppSettings.Get(this, "MaxInstancesReRuns", false);
				if (string.IsNullOrEmpty(configValue) || !Int32.TryParse(configValue.ToString(), out _maxInstancesReRuns))
					_maxInstancesReRuns = MaxInstancesReRuns;

				configValue = AppSettings.Get(this, "PrecentDiffrence", false);
				if (string.IsNullOrEmpty(configValue) || !Double.TryParse(configValue.ToString(), out _precentDiffrence))
					_precentDiffrence = PrecentDiffrence;

				configValue = AppSettings.Get(this, "MaxRetries", false);
				if (string.IsNullOrEmpty(configValue) || !Int32.TryParse(configValue.ToString(), out _maxRetries))
					_maxRetries = MaxRetries;
			}
			catch (Exception ex)
			{
				Log.Write("Error Initalize RetrieverService.", ex);
			}			
		}

		protected override void OnInit()
		{
			base.OnInit();

			bool tempBool = true;
			if (Instance.Configuration.Options["DailyFileName"] != null)
			{
				bool.TryParse(Instance.Configuration.Options["DailyFileName"], out tempBool);
			}
			else if (Instance.ParentInstance != null && Instance.ParentInstance.Configuration.Options["DailyFileName"] != null)
			{
				bool.TryParse(Instance.ParentInstance.Configuration.Options["DailyFileName"], out tempBool);
			}

			this.DailyFileName = tempBool;
		}

		/*=========================*/
		#endregion

		#region Protected Methods
		/*=========================*/

		protected long ActivateAverageFileSizeSP(DateTime retrievedDay, string GoogleServiceType)
		{
			using (IDisposable mc = new SqlConnection(_sourceConnectionString))
			{
				SqlConnection manualConnection = (SqlConnection)mc;
				manualConnection.Open();

				// Initalize stored procedure Sp_Delete_Table_Days.
				SqlCommand contentSPCmd =
					DataManager.CreateCommand("SP_Core_AvgFileSize(@daycode:Int, @AccountID:NVarChar, @Servicetype:NVarChar)", CommandType.StoredProcedure);
				contentSPCmd.CommandTimeout = 30000;
				contentSPCmd.Connection = manualConnection;

				try
				{
					// Init Paramters
					contentSPCmd.Parameters["@daycode"].Value = GetDayCode(retrievedDay).ToString();
					contentSPCmd.Parameters["@AccountID"].Value = _accountID;
					contentSPCmd.Parameters["@Servicetype"].Value = GoogleServiceType;

					// Activate stored procedure.
					object temp = contentSPCmd.ExecuteScalar();
					long AverageFileSize;
					if (!long.TryParse(temp.ToString(), out AverageFileSize))
						AverageFileSize = 0;

					return AverageFileSize;
				}
				catch (Exception ex)
				{
					throw new Exception("Error Average File Size  stored procedure.", ex);
				}
			}
		}

		/// <summary>
		/// In each valid Google AdWords report there is a node of totals in the end of
		/// the xml.
		/// This function check if the the node "totals" exist in the file.
		/// </summary>
		/// <param name="filePath"> the path of the xml report file to check.</param>
		/// <returns>True - found totals node - valid file, 
		/// False - didn't find totals node - invalid file</returns>
		private bool ExistTotalsNode(string filePath)
		{
			XmlTextReader reader = new XmlTextReader(filePath);
			XmlDocument doc = new XmlDocument(); 
			doc.Load(reader);
			reader.Close();

			//Check if the node totals exist in the end of the xml file.
			XmlNode totalsNode;
			XmlElement root = doc.DocumentElement;
			totalsNode = root.SelectSingleNode("/report/totals");
			if (totalsNode == null)
				return false;
			else 
				return true;
		}

		/*=========================*/

		#endregion

		#region SaveFilePathToDB

		protected bool SaveFilePathToDB(string serviceType, string filePath, DateTime retrievedDay, bool adwordsFile)
		{
			return SaveFilePathToDB(serviceType, filePath, null, null, retrievedDay, adwordsFile);
		}

		// obsolete
		//protected bool SaveFilePathToDB(string serviceType, string filePath, DateTime retrievedDay)
		//{
		//    return SaveFilePathToDB(serviceType, filePath, null, null, retrievedDay, false, false);
		//}

		protected bool SaveFilePathToDB(string serviceType, string filePath, SettingsCollection parameters)
		{
			return SaveFilePathToDB(serviceType, filePath, null, parameters, DateTime.Now, false);
		} 

		/// <summary>
		/// Fills DB with the data that retrieved by the retriever.
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="reportFileName"></param>   
		/// <returns>True for success. False for failure</returns>
		protected bool SaveFilePathToDB(string serviceType, string filePath, string sourceUrl, SettingsCollection parameters, DateTime retrievedDay, bool adwordsFile)
		{
			long fileSize;
			if (!ValidateReportFile(serviceType, filePath, retrievedDay, adwordsFile, out fileSize))
				return false;

			SqlCommand insertCommand = CreateSaveReportInsertCommand();

			using (IDisposable mc = new SqlConnection(_sourceConnectionString))
			{
				SqlConnection manualConnection = (SqlConnection) mc;
				manualConnection.Open();
				insertCommand.Connection = manualConnection;

				// Initalize insert command parameters.
				insertCommand.Parameters["@accountID"].Value = _accountID;
				insertCommand.Parameters["@serviceType"].Value = serviceType;
				insertCommand.Parameters["@instanceID"].Value = Instance.InstanceID;
				insertCommand.Parameters["@parentInstanceID"].Value = Instance.ParentInstance != null ? (object) Instance.ParentInstance.InstanceID : DBNull.Value;
				insertCommand.Parameters["@retrieveDate"].Value = DateTime.Now;
				insertCommand.Parameters["@path"].Value = filePath;
				insertCommand.Parameters["@sourceUrl"].Value = sourceUrl == null ? (object) DBNull.Value : sourceUrl;
				insertCommand.Parameters["@params"].Value = parameters == null ? (object) DBNull.Value : parameters.Definition;
				insertCommand.Parameters["@fileSize"].Value = fileSize;
				insertCommand.Parameters["@daycode"].Value = GetDayCode(retrievedDay);

				try
				{
					// Execute command.
					insertCommand.ExecuteNonQuery();
					return true;
				}
				catch (Exception ex)
				{
					throw new Exception("Error saving retrieved file path to the database.", ex);
				}
			}
		}

		private bool ValidateReportFile(string serviceType, string filePath, DateTime retrievedDay, bool adwordsFile, out long fileSize)
		{
            try
            {
                FileInfo fi = new FileInfo(filePath);
                fileSize = (long)Math.Round((double)fi.Length / 1000);

                if (adwordsFile) // For adwords file we check if totals node exist in the file.
                {
                    if (!ExistTotalsNode(filePath))
                    {
                        Log.Write(string.Format("The Adwords reprot {1} for date {0} Don't have totals nodes in his end, invalid report, try to run it again manually.", retrievedDay, filePath), LogMessageType.Error);
                        return false;
                    }
                }
                else // for other files we check file size.
                {
                    // Check if the accoung has attribute PrecentDiffrence in the configuration.
                    double precentDiffrence;
                    if (!string.IsNullOrEmpty(GetConfigurationOptionsField("PrecentDiffrence")) &&
                        Double.TryParse(GetConfigurationOptionsField("PrecentDiffrence"), out precentDiffrence))
                    {
                        _precentDiffrence = precentDiffrence;
                    }

                    long averagefileSize = ActivateAverageFileSizeSP(retrievedDay, serviceType);

                    // We don't check file size for importer files.
                    if ((_accountID > SystemAccountID) && (averagefileSize * (1 - _precentDiffrence) > fileSize))
                    {
                        Log.Write(string.Format("The File {1} size {3}KB, for date {0} is smaller from the avergae file size {2}KB.", retrievedDay, filePath, averagefileSize.ToString(), fileSize.ToString()), LogMessageType.Error);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                
                Log.Write("serviceType :" + serviceType + " , filePath: " + filePath +" , ", LogMessageType.Error);
                fileSize = 0;
                Log.Write("Error in ValidateReportFile(), " + ex.Message, LogMessageType.Error);
                return false;
            }
		}

		private static SqlCommand CreateSaveReportInsertCommand()
		{
			// Initalize insert command to Source table
			SqlCommand insertCommand = DataManager.CreateCommand(@"
                INSERT INTO RetrievedFiles
				(
                    AccountID,
					ServiceType,
 					RetrieverInstanceID,
                    ParentInstanceID,
					RetrieveDate,
					Path,
					SourceUrl,
					Parameters,
					FileSize,
					Daycode
                )
                VALUES
                (
                    @accountID:Int,
                    @serviceType:NVarChar,
                    @instanceID:Int,
                    @parentInstanceID:Int,
					@retrieveDate:DateTime,
                    @path:NVarChar,
					@sourceUrl:NVarChar,
                    @params:NVarChar,
					@fileSize:BigInt,
					@daycode:Int
                )",
				CommandType.Text);
			return insertCommand;
		}
		/*=========================*/

		#endregion

		

		#region WriteResultToFile Methods
		/*=========================*/

		/// <summary>
		/// Write the data from the retriever to a file. 
		/// </summary>
		/// <param name="resultXml">Input XmlDocument to save to file<</param>
		/// <returns>True for success. False for failure</returns>
		protected string WriteResultToFile(XmlDocument document, DateTime retrieveDate, string suffix)
		{
			string fileName = fileName = InitalizeFileName(retrieveDate, suffix);
			document.Save(fileName);
			return fileName;
		}
		protected string WriteResultToFile(XmlDocument document, DateTime retrieveDate)
		{
			return WriteResultToFile(document, retrieveDate, string.Empty);
		}

		protected string WriteResultToFile(string url, DateTime retrieveDate, string suffix, bool treatAsXml)
		{
			return WriteResultToFile(url, retrieveDate, suffix, treatAsXml, new Dictionary<string, string>());
		}

		/// <summary>
		/// Write the data from the retriever to a file. 
		/// </summary>
		/// <param name="url">Url of the XML data to load and save.</param>
		/// <returns>The name of the saved file.</returns>
		protected string WriteResultToFile(string url, DateTime retrieveDate, string suffix, bool treatAsXml, Dictionary<string,string> headers)
		{
			// Initalize HttpWebRequest
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
			request.UserAgent = AppSettings.Get(this, "UserAgent");
			request.Timeout = (int)TimeSpan.Parse(AppSettings.Get(this, "RequestTimeout")).TotalMilliseconds; 
			request.ContentType = "text/plain";
			

			if (headers.Count > 0)
			{
				foreach (string header in headers.Keys)
				{
					request.Headers.Add(header, headers[header]);
				}
			}
			string fileName = string.Empty;
			int failureTimes = 0;

			while (fileName == string.Empty)
			{				
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				{
					// Initalize urlStream.
					Stream urlStream = response.GetResponseStream();
					urlStream.ReadTimeout = (int)TimeSpan.Parse(AppSettings.Get(this, "ReadTimeout")).TotalMilliseconds;

					#region Obsolete code - Read from urlstream to file stram byte by byte
					/*=========================*/
					/*
					if (File.Exists(@"c:\temp\asaf_test.xml"))
						File.Delete(@"c:\temp\asaf_test.xml");

					FileStream fs = new FileStream(@"c:\temp\asaf_test.xml", FileMode.CreateNew);
					int count = urlStream.ReadByte();
					while (count != -1)
					{
						fs.WriteByte((byte)count);
						count = urlStream.ReadByte();
					}

					fs.Flush();
					fs.Close();
					fs.Dispose();
				
					urlStream.Close();
					urlStream.Dispose();
					*/

					/*=========================*/
					#endregion

					try
					{
						if (treatAsXml)
						{
							XmlTextReader reader = new XmlTextReader(urlStream);
							reader.EntityHandling = EntityHandling.ExpandEntities;//  .ExpandCharEntities;
							fileName = WriteResultToFile(reader, retrieveDate, suffix);
						}
						else
						{
							StreamReader reader = new StreamReader(urlStream);
							fileName = WriteResultToFile(reader, retrieveDate, suffix);
						}
					}
					catch (Exception ex)
					{
						if (failureTimes < 3)
						{
							++failureTimes;
						}
						else
						{
							throw new Exception(string.Format("Can't get  Google AdWords report adwords api."), ex);
						}
					}
				} 
			}
			return fileName;
		}

		#region Obsolete code 
		/*=========================*/

		/*
		/// <summary>
		/// Write the data from the retriever to a file. 
		/// </summary>
		/// <param name="url">Url of the XML data to load and save.</param>
		/// <returns>The name of the saved file.</returns>
		protected string WriteResultToFile(string url, DateTime retrieveDate, string suffix)
		{
			// Initalize HttpWebRequest
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = AppSettings.Get(this, "UserAgent");
			request.Timeout = (int)TimeSpan.Parse(AppSettings.Get(this, "RequestTimeout")).TotalMilliseconds;
			request.ContentType = "text/plain";

			string fileName;
			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			{
				// Initalize urlStream.
				Stream urlStream = response.GetResponseStream();
				urlStream.ReadTimeout = (int)TimeSpan.Parse(AppSettings.Get(this, "ReadTimeout")).TotalMilliseconds;
				/*
				if (File.Exists(@"c:\temp\asaf_test.xml"))
					File.Delete(@"c:\temp\asaf_test.xml");

				FileStream fs = new FileStream(@"c:\temp\asaf_test.xml", FileMode.CreateNew);
				int count = urlStream.ReadByte();
				while (count != -1)
				{
					fs.WriteByte((byte)count);
					count = urlStream.ReadByte();
				}

				fs.Flush();
				fs.Close();
				fs.Dispose();
				
				urlStream.Close();
				urlStream.Dispose();
				

				if (treatAsXml)
				{
					XmlTextReader reader = new XmlTextReader(urlStream);
					reader.EntityHandling = EntityHandling.ExpandEntities;//  .ExpandCharEntities;
					fileName = WriteResultToFile(reader, retrieveDate, suffix);
				}
				else
				{
					StreamReader reader = new StreamReader(urlStream);
					fileName = WriteResultToFile(reader, retrieveDate, suffix);
				}
			}

			//Log.Write(string.Format("Wrote report xml file to {0}", fileName), LogMessageType.Information);

			return fileName;
		}
		*/

		/*=========================*/
		#endregion

		protected string WriteResultToFile(string url, DateTime retrieveDate, bool treatAsXml, bool postMethod)
		{
			return WriteResultToFile(url, retrieveDate, string.Empty, treatAsXml);
		}

		protected string WriteResultToFile(string url, DateTime retrieveDate, bool treatAsXml )
		{
			return WriteResultToFile(url, retrieveDate, string.Empty, treatAsXml);
		}

		/// <summary>
		/// Saves the contents of an XmlTextReader.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="retrieveDate"></param>
		/// <returns></returns>
		protected string WriteResultToFile(XmlTextReader reader, DateTime retrieveDate, string suffix)
		{
			string fileName = InitalizeFileName(retrieveDate, suffix);
			int numOfRetries = 0;
			
			using (XmlTextWriter writer = new XmlTextWriter(fileName, Encoding.UTF8))
			{
				writer.Formatting = Formatting.Indented;
				try
				{
					while (reader.Read())
					{
						writer.WriteNode(reader, true);
					}
				}
				catch (IOException ex)
				{	
					if (numOfRetries >= _maxRetries)	// too many retries, bail out
					{
						writer.Close();
						File.Delete(fileName);
						throw ex;
					}
					++numOfRetries;
				}
				catch (Exception ex)
				{
					writer.Close();
					File.Delete(fileName);
					throw ex;
				}
			}
		
			return fileName;					 
		}

		protected string WriteResultToFile(XmlTextReader reader, DateTime retrieveDate)
		{
			return WriteResultToFile(reader, retrieveDate, string.Empty);
		}


		/// <summary>
		/// Saves the contents of a StreamReader.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="retrieveDate"></param>
		/// <returns></returns>
		protected string WriteResultToFile(StreamReader reader, DateTime retrieveDate, string suffix)
		{
			string fileName = InitalizeFileName(retrieveDate, suffix);

			try
			{
				using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
				{
					while (!reader.EndOfStream)
					{
						char[] buffer = new char[1];
						reader.Read(buffer, 0, 1);
						writer.Write(buffer);
					}
				}
			}
			catch (Exception ex)
			{
				File.Delete(fileName);
				throw ex;
			}

			return fileName;
		}

		protected string WriteResultToFile(StreamReader reader, DateTime retrieveDate)
		{
			return WriteResultToFile(reader, retrieveDate, string.Empty);
		}

		protected string WriteResultToFile(string inputPath, DateTime retrieveDate, string suffix, string directory)
		{
			if (File.Exists(inputPath))
			{
				string fileName = InitalizeFileName(inputPath, retrieveDate, suffix,directory);
				
				try
				{
					Directory.Move(inputPath, fileName);
				}
				catch (IOException ex) // file already exist
				{
					Log.Write("Could move file, trying different name...", ex, LogMessageType.Warning);

					fileName += " DUPLICATE";
					Directory.Move(inputPath, fileName);
				}

				Log.Write("Successfully moved file " + fileName, LogMessageType.Information);
				return fileName;
			}
			else
			{
				return WriteResultToFile(inputPath, retrieveDate, suffix, false);
			}
		}

		protected string WriteResultToFile(string inputPath, DateTime retrieveDate)
		{
			return WriteResultToFile(inputPath, retrieveDate, string.Empty, string.Empty);
		}

		protected string WriteResultToFile(string inputPath, DateTime retrieveDate, string directory)
		{
			return WriteResultToFile(inputPath, retrieveDate, string.Empty, directory);
		}



		/// <summary>
		/// Write the data from the BackOffice to a file. 
		/// </summary>
		/// <param name="dataFromBO">Data from BackOffice to write to file.</param>
		protected string WriteResultToFile(DataTable dataTable, DateTime retrieveDate, string suffix)
		{
			string fileName = InitalizeFileName(retrieveDate, suffix);

			dataTable.WriteXml(fileName);

			// Write the data from  BackOffice to a file in currentDirectory. 
			//using (StreamWriter writer = new StreamWriter(fileName))
			//{
			//    writer.Write(dataSet.GetXml());
			//    writer.Close();				
			//}
			return fileName;
		}

	
		protected string WriteResultToFile(DataTable dataTable, DateTime retrieveDate)
		{
			return WriteResultToFile(dataTable, retrieveDate, string.Empty);
		}

		/*=========================*/

		#endregion

		public string PathEscape(string path)
		{
			string escapeChars = "[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]";
			return Regex.Replace(path, escapeChars, String.Empty);
		}


		// Obsolete
		protected string WriteResultToFile(DataSet dataSet, DateTime retrieveDate)
		{
			return WriteResultToFile(dataSet, retrieveDate, string.Empty);
		}

		// Obsolete
		///<summary>
		///Write the data from the BackOffice to a file. 
		///</summary>
		///<param name="dataFromBO">Data from BackOffice to write to file.</param>
		protected string WriteResultToFile(DataSet dataSet, DateTime retrieveDate, string suffix)
		{
		    string fileName = InitalizeFileName(retrieveDate, suffix);

		    dataSet.WriteXml(fileName);

		    // Write the data from  BackOffice to a file in currentDirectory. 
		    //using (StreamWriter writer = new StreamWriter(fileName))
		    //{
		    //    writer.Write(dataSet.GetXml());
		    //    writer.Close();				
		    //}
		    return fileName;
		}
	}
}
