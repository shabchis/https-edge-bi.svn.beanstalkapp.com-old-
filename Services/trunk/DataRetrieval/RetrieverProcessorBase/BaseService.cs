using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;

namespace Easynet.Edge.Services.DataRetrieval
{
	public class BaseService : Service
	{
		#region Consts
		/*=========================*/

		protected const string SearchType = "'Search Only'";
		protected const string ContentType = "'Content Only'";
		private const string DefualtTargetSubDir = @"Unknown\";
		private const string DefualtFileExtension = "txt";		
		protected const int SystemAccountID = -1;

		/*=========================*/
		#endregion    

		#region Fields
		/*=========================*/
		protected bool _debugMode = false;
		protected string _sourceConnectionString = string.Empty;
		protected int _accountID;
		private string _resultsRoot = string.Empty;
		private string _targetSubDir = string.Empty;
		private string _fileExtension = string.Empty;
		private string _errorFilePath = string.Empty;
		private bool _dailyFileName = true;
		protected DateTime _requiredDay = new DateTime();
		protected int _channelID;

		/*=========================*/
		#endregion      

		#region Access Methods
		/*=========================*/

		protected bool DailyFileName
		{
			get { return _dailyFileName; }
			set { _dailyFileName = value; }
		}

		protected string TargetSubDir
		{
			get { return _targetSubDir; }
			set { _targetSubDir = value; }
		}

		protected string ErrorFilePath
		{
			get { return _errorFilePath; }
			set { _errorFilePath = value; }
		}

		/*=========================*/
		#endregion

		#region Constructor
		/*=========================*/

		/// <summary>
		/// Constructor
		/// </summary>
		public BaseService()
		{
			// Load application settings.
			bool.TryParse(AppSettings.GetAbsolute("DebugMode"), out _debugMode);
			_sourceConnectionString = AppSettings.Get(this, "SourceConnectionString");

			_resultsRoot = FormatPath(AppSettings.Get(this, "ResultsRoot"));

		}

		protected override void OnInit()
		{
			_fileExtension = Instance.Configuration.Options["FileExtension"];
			if (_fileExtension == null)
			{
				_fileExtension = AppSettings.Get(this, "FileExtension", false);
				if (_fileExtension == null)
					_fileExtension = DefualtFileExtension; 
			}			

			_targetSubDir = Instance.Configuration.Options["TargetSubDirectory"];
			if (_targetSubDir == null)
			{
				_targetSubDir = AppSettings.Get( this, "TargetSubDirectory", false);

				if (_targetSubDir == null)			
					_targetSubDir = FormatPath(DefualtTargetSubDir);
				else				
					_targetSubDir = FormatPath(_targetSubDir);
				
			}
		}

		/*=========================*/
		#endregion

		#region InitalizeFileName
		/*=========================*/

		protected virtual string InitalizeFileName(DateTime retrieveDate, string suffix)
		{
			return InitalizeFileName(string.Empty, retrieveDate, suffix, string.Empty);
		}

		protected virtual string InitalizeFileName(string prefix, DateTime retrieveDate, string suffix)
		{
			return InitalizeFileName(prefix, retrieveDate, suffix, string.Empty);
		}

		protected virtual string InitalizeFileName(DateTime retrieveDate, string suffix, string directory)
		{
			return InitalizeFileName(string.Empty, retrieveDate, suffix, directory);
		}

		protected virtual string InitalizeFileName(string prefix, DateTime retrieveDate, string suffix, string directory)
		{
			string outputDir;

			if (!String.IsNullOrEmpty(prefix))
			{
				string inputFileName = prefix.Substring(prefix.LastIndexOf("\\") + "\\".Length);
				prefix = inputFileName.Substring(0, inputFileName.LastIndexOf("."));
			}

			if (directory == string.Empty)
				outputDir = _targetSubDir;
			else
				outputDir = directory;

			// Daily or Montly time formats
			string format = _dailyFileName ?
				@"{0}{1}{2:yyyy}\{2:MM}\{2:dd}\" :
				@"{0}{1}{2:yyyy}\{2:MM}\"
				;

			// Format the directory path
			string currentDirectory = String.Format(format,
					_resultsRoot,
					outputDir,
					retrieveDate
					);

			if (!Directory.Exists(currentDirectory))
				Directory.CreateDirectory(currentDirectory);

			// {2:yyyyMMdd}@{2:hhmmssmm} time format that include second and miliseconds
			// Format the file name
			//string fileName = String.Format(@"{0}{5} {1:000} {2:yyyyMMdd}@{2:hhmm} {3}.{4}",
			string fileName = String.Format(@"{0}{1} {2:000} {3:yyyyMMdd}@{3:hhmm} ({4}) {5}.{6}",
				currentDirectory,
				prefix,
				_accountID,
				DateTime.Now,
				this.Instance.InstanceID,
				suffix,
				_fileExtension
				);

			// Remove the inavlid file char ':' - WHAT THE FUCK WHO DID THIS
			//fileName = fileName.Remove(fileName.LastIndexOf(':'), 1);
			return fileName;
		}

		/*=========================*/
		#endregion

		#region Delete Methods
		/*=========================*/

		/// <summary>
		/// Activate stored procedure Sp_Delete_Table_Days that delete 
		/// all the data in the DB for the required Day.
		/// </summary>
		protected void DeleteDayChannel(int dayCode, string tableName, int channelID, int accountID)
		{
			using (ConnectionKey key = DataManager.Current.OpenConnection())
			{
				// Initalize stored procedure Sp_Delete_Table_Days.
				SqlCommand deleteDayStoredProcedureCmd = DataManager.CreateCommand("Sp_Delete_Table_Days(@Table_Name:NVarChar, @Account_ID:NVarChar,@Day:NVarChar, @Channel_ID:NVarChar)", CommandType.StoredProcedure);
				deleteDayStoredProcedureCmd.CommandTimeout = 30000;

				// Init Paramters
				deleteDayStoredProcedureCmd.Parameters["@Table_Name"].Value = tableName;
				deleteDayStoredProcedureCmd.Parameters["@Account_ID"].Value = accountID;
				deleteDayStoredProcedureCmd.Parameters["@Day"].Value = dayCode.ToString();
				deleteDayStoredProcedureCmd.Parameters["@Channel_ID"].Value = channelID;

				try
				{
					// Activate stored procedure.
					deleteDayStoredProcedureCmd.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					throw new Exception("Error activate stored procedure Delete Table Days.", ex);
				}
			}
		}
		protected void DeleteDay(int dayCode, string tableName)
		{
			DeleteDayChannel(dayCode, tableName, _channelID, _accountID);
		}

        //========================================
        //WE SHOULD IMPLEMENT DELETEDAYANALYTICS
        //========================================

		/// <summary>
		/// Activate stored procedure Sp_Delete_Table_Days that delete 
		/// all the data in the DB for the required Day.
		/// </summary>
		protected void DeleteDay(int dayCode, string tableName, int accountID)
		{
			using (ConnectionKey key = DataManager.Current.OpenConnection())
			{
				// Initalize stored procedure Sp_Delete_Table_Days.
				SqlCommand deleteDayStoredProcedureCmd = DataManager.CreateCommand("Sp_Delete_Table_Days_no_channel(@Table_Name:NVarChar, @Account_ID:NVarChar,@Day:NVarChar)", CommandType.StoredProcedure);
				deleteDayStoredProcedureCmd.CommandTimeout = 30000;

				// Init Paramters
				deleteDayStoredProcedureCmd.Parameters["@Table_Name"].Value = tableName;
				deleteDayStoredProcedureCmd.Parameters["@Account_ID"].Value = accountID;
				deleteDayStoredProcedureCmd.Parameters["@Day"].Value = dayCode.ToString();

				try
				{
					deleteDayStoredProcedureCmd.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					throw new Exception("Error activate stored procedure Delete Table Days.", ex);
				}
			}
		}

		// TODO: maybe to move to base object
		protected void DeleteDayBO(int dayCode, string tableName)
		{
			using (ConnectionKey key = DataManager.Current.OpenConnection())
			{
				// Initalize stored procedure Sp_Delete_Table_Days.
                SqlCommand deleteDayStoredProcedureCmd = DataManager.CreateCommand("Sp_Delete_Table_Days_no_channel(@Table_Name:NVarChar, @Account_ID:NVarChar,@Day:NVarChar)", CommandType.StoredProcedure);
				deleteDayStoredProcedureCmd.CommandTimeout = 30000;

				// Init Paramters
				deleteDayStoredProcedureCmd.Parameters["@Table_Name"].Value = tableName;
				deleteDayStoredProcedureCmd.Parameters["@Account_ID"].Value = _accountID;
				deleteDayStoredProcedureCmd.Parameters["@Day"].Value = dayCode.ToString();

				try
				{
					deleteDayStoredProcedureCmd.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					throw new Exception("Error activate stored procedure Delete Table Days no channel.", ex);
				}
			}
		}

		/*=========================*/
		#endregion

		#region Resolve Methods
		/*=========================*/

		/// <summary>
		/// Check if we can convert the input string to int. 
		/// Used for converting fields from BO to insert command parameters.
		/// </summary>
		/// <param name="inputString">String to convert.</param>
		/// <returns>The number in the string or 0 if it can't be converted.</returns>
		protected int ResolveInt(string inputString)
		{
			int tempInt;
			if (!int.TryParse(inputString, out tempInt))
				tempInt = 0;

			return tempInt;
		}

		/// <summary>
		/// Check if we can convert the input string to int. 
		/// Used for converting fields from BO to insert command parameters.
		/// </summary>
		/// <param name="inputString">String to convert.</param>
		/// <returns>The number in the string or 0 if it can't be converted.</returns>
		protected long ResolveLong(string inputString)
		{
			long tempLong;
			if (!long.TryParse(inputString, out tempLong))
				tempLong = 0;

			return tempLong;
		}

		/// <summary>
		/// Check if we can convert the input string to int. 
		/// Used for converting fields from BO to insert command parameters.
		/// </summary>
		/// <param name="inputString">String to convert.</param>
		/// <returns>The number in the string or 0 if it can't be converted.</returns>
		protected double ResolveDouble(string inputString)
		{
			double tempDouble;
			if (!double.TryParse(inputString, out tempDouble))
				tempDouble = 0;

			return tempDouble;
		}

		/// <summary>
		/// If the attrbiute have null value and we return empty string 
		/// otherwise we return the value in string format.
		/// </summary>
		/// <param name="attributeName">Attribut to check if it has null value.</param>
		/// <returns>The value in string format.</returns>
		protected string ResolveString(object attribute)
		{
			if (attribute == null)
				return string.Empty;
			else
				return attribute.ToString();
		}

		/*=========================*/
		#endregion     

		#region Date Methods
		/*=========================*/

		// TODO: maybe move some of the day code Methods to core\Utilities\DayCode
		protected bool CheckRangeDate(ref ArrayList dates)
		{

			if ((Instance.ParentInstance != null) &&
				(Instance.ParentInstance.ActiveSchedulingRule != null) &&
				(!String.IsNullOrEmpty(Instance.ParentInstance.ActiveSchedulingRule.FromDate)) &&
				(!String.IsNullOrEmpty(Instance.ParentInstance.ActiveSchedulingRule.ToDate)))
			{
				string excatUnits = Instance.ParentInstance.ActiveSchedulingRule.ExcatUnits != null ? Instance.ParentInstance.ActiveSchedulingRule.ExcatUnits : string.Empty;
				dates = ScheduleConvertor.GetRangeDate(Instance.ParentInstance.ActiveSchedulingRule.FromDate.ToString(), Instance.ParentInstance.ActiveSchedulingRule.ToDate.ToString(), excatUnits);
				return true;
			}
			else if ((Instance.ParentInstance.ParentInstance != null) &&
					(Instance.ParentInstance.ParentInstance.ActiveSchedulingRule != null) &&
					(!String.IsNullOrEmpty(Instance.ParentInstance.ParentInstance.ActiveSchedulingRule.FromDate)) &&
					(!String.IsNullOrEmpty(Instance.ParentInstance.ParentInstance.ActiveSchedulingRule.ToDate)))
			{
				string excatUnits = Instance.ParentInstance.ParentInstance.ActiveSchedulingRule.ExcatUnits != null ? Instance.ParentInstance.ParentInstance.ActiveSchedulingRule.ExcatUnits : string.Empty;
				dates = ScheduleConvertor.GetRangeDate(Instance.ParentInstance.ParentInstance.ActiveSchedulingRule.FromDate.ToString(), Instance.ParentInstance.ParentInstance.ActiveSchedulingRule.ToDate.ToString(), excatUnits);
				return true;
			}
			else
				return false;
		}

		protected DateTime GetDateFromFilePath(string xmlPath)
		{
			int day;
			int month;
			int year;

			// Load a specific xml file from the configuration
			if (!String.IsNullOrEmpty(GetConfigurationOptionsField("FileDate")))
				return _requiredDay;

			if (!GetIntValue(ref xmlPath, out day, 2))
				return new DateTime();

			if (!GetIntValue(ref xmlPath, out month, 2))
				return new DateTime();

			if (!GetIntValue(ref xmlPath, out year, 4))
				return new DateTime();

			return new DateTime(year, month, day);
		}

		/// <summary>
		/// Check if we need to get manual date.
		/// </summary>
		protected void CheckManualDate()
		{
			//ASAF: use get config fuction.
			// Check date field
			if (Instance.ParentInstance != null && Instance.ParentInstance.Configuration.Options["Date"] != null)
			{
				InitalizeReportDate(Instance.ParentInstance.Configuration.Options["Date"].ToString(), ref _requiredDay);
			}
			else if (Instance.ParentInstance.ParentInstance != null
				&& Instance.ParentInstance.ParentInstance.Configuration.Options["Date"] != null)
			{
				InitalizeReportDate(Instance.ParentInstance.ParentInstance.Configuration.Options["Date"].ToString(), ref _requiredDay);
			}
			// Check lastDay field
			else if (
						(Instance.Configuration.Options["LastDay"] != null &&
						 Convert.ToBoolean(Instance.Configuration.Options["LastDay"])
						)
						||
						(Instance.ParentInstance != null &&
						 Instance.ParentInstance.Configuration.Options["LastDay"] != null &&
						 Convert.ToBoolean(Instance.ParentInstance.Configuration.Options["LastDay"])
						)
					)
			{
				Log.Write("Fetch last day data.", LogMessageType.Information);
				_requiredDay = DateTime.Today.AddDays(-1);
			}

			if (_requiredDay < DateTime.Today.AddYears(-10))
				throw new Exception(string.Format("Wrong Date {0}.", _requiredDay.ToString()));
		}

		protected void InitalizeReportDate(string manualDate, ref DateTime retrievedDay)
		{
			Console.WriteLine("{0} service run for date {1}",
				ShortHeader(),
				manualDate);

			Log.Write(string.Format("Fetch data for date {0}.", manualDate), LogMessageType.Information);
			StringToDate(manualDate, out retrievedDay);
		}

		/// <summary>
		/// DayCode is an int that repreent a date in the format YYYYMMDD.
		/// </summary>
		/// <param name="dateToConvert">DateTime to convert to int date.</param>
		/// <returns>date in format YYYYMMDD.</returns>
		protected int GetDayCode(DateTime dateToConvert)
		{
			return dateToConvert.Year * 10000 + dateToConvert.Month * 100 + dateToConvert.Day;
		}

		/// <summary>
		/// Create Daycode for last month (summary report) in the format YYYYMM00.
		/// </summary>
		/// <returns>date in format YYYYMM00.</returns>
		protected int DayCodeSummary()
		{
			return DateTime.Now.Year * 10000 + DateTime.Now.AddMonths(-1).Month * 100;
		}

		/// <summary>
		/// Date format YYYYMMDD
		/// </summary>
		/// <param name="date"></param>
		/// <param name="returnDate"></param>
		/// <returns></returns>
		protected void StringToDate(string date, out DateTime returnDate)
		{
			int year = 0;
			int month = 0;
			int day = 0;
			bool result = false;

			// Fetch Year
			result = int.TryParse(date.Substring(0, 4), out year);

			if (!result)
			{
				returnDate = new DateTime();
				throw new Exception(string.Format("Can't convert year from date string: {0}.", date));
			}

			// Fetch Month
			result = int.TryParse(date.Substring(4, 2), out month);

			if (!result)
			{
				returnDate = new DateTime();
				throw new Exception(string.Format("Can't convert month from date string: {0}.", date));
			}

			// Fetch Day
			result = int.TryParse(date.Substring(6, 2), out day);
			if (!result)
			{
				returnDate = new DateTime();
				throw new Exception(string.Format("Can't convert Day from date string: {0}.", date));
			}

			try
			{
				returnDate = new DateTime(year, month, day);
			}
			catch (ArgumentOutOfRangeException ex)
			{
				returnDate = new DateTime();
				throw new ArgumentOutOfRangeException(string.Format("Can't convert {0} to date, Exception message: {1}.", date, ex.Message));
			}
			catch (Exception ex)
			{
				returnDate = new DateTime();
				throw new ArgumentOutOfRangeException(string.Format("Can't convert {0} to date, Exception message: {1}.", date, ex.Message));
			}
		}

		/*=========================*/
		#endregion

		#region Protected Methods
		/*=========================*/

		/// <summary>
		/// Get from the table User_GUI_Account in easynet_OLTP DB
		/// the account ID by the account Name.
		/// </summary>
		/// <param name="accountName">Account name to find his ID.</param>
		/// <param name="accountID">Account ID of the Account name.</param>
		/// <returns> True for success, False for failure.</returns>
		protected virtual bool GetAccountID(string accountName, out int accountID, string accountFieldName)
		{
			accountID = SystemAccountID;
			// Initalize select command to account ID by account name.
			SqlCommand selectCommand = DataManager.CreateCommand(@"
				select Account_ID
					from User_GUI_Account
					where " + accountFieldName + " = @accountName:NVarChar",
				CommandType.Text);

			using (DataManager.Current.OpenConnection())
			{
				// Initalize connetion.
				DataManager.Current.AssociateCommands(selectCommand);

				// Initalize select command parameters.
				selectCommand.Parameters["@accountName"].Value = accountName;

				try
				{
					// Execute select command.
					accountID = (int)selectCommand.ExecuteScalar();
				}
				catch (Exception ex)
				{
					Log.Write("Error get report data from table in Source DB.", ex);
					return false;
				}
			}
			return true;
		}

         


		protected bool InitalizeAccountID()
		{
			if (Instance.AccountID > SystemAccountID)
				_accountID = Instance.AccountID;
			else if (!string.IsNullOrEmpty(GetConfigurationOptionsField("AccountID")))
			{
				if (!Int32.TryParse(GetConfigurationOptionsField("AccountID"), out _accountID))
				{
					Log.Write(string.Format("Wrong AccountID {0}", GetConfigurationOptionsField("AccountID")), LogMessageType.Error);
					return false;
				}
			}
			else
				_accountID = SystemAccountID;

			return true;
		}

		protected string ShortHeader()
		{

			string output = string.Format("{0} {1} - ",
				DateTime.Now.ToString("dd/MM"),
				DateTime.Now.ToShortTimeString());

			return output;
		}

		protected static string GetCleanDomainName(string siteUrl)
		{
			if (siteUrl.StartsWith("http://"))
				siteUrl = siteUrl.Remove(0, "http://".Length);
			else if (siteUrl.StartsWith("https://"))
				siteUrl = siteUrl.Remove(0, "https://".Length);
			
			// Remove everthing afte the first "/"
			if (siteUrl.IndexOf("/") > 0)
				siteUrl = siteUrl.Remove(siteUrl.IndexOf("/"));
			else if (siteUrl.IndexOf("...") > 0)
				siteUrl = siteUrl.Remove(siteUrl.IndexOf("..."));

			return siteUrl;
		}

		protected bool GetIntValue(ref string xmlPath, out int intValue, int intLength)
		{			
			xmlPath = xmlPath.Substring(0, xmlPath.LastIndexOf(@"\"));
			string temp = xmlPath.Substring(xmlPath.LastIndexOf(@"\") + 1, intLength);
			try
			{
				Int32.TryParse(temp, out intValue);
			}
			catch (Exception ex)
			{
				Log.Write(string.Format("Can't convert xml file path {0} to date", xmlPath), ex);
				intValue = 0;
				return false;
			}

			return true;
		}

		protected string FormatPath(string path)
		{
			if
			(
				path.Length > 0 &&
				path[path.Length - 1] != Path.DirectorySeparatorChar &&
				path[path.Length - 1] != Path.AltDirectorySeparatorChar
			)
			{
				path += Path.DirectorySeparatorChar;
			}

			return path;
		}
		
		protected void WriteErrorMesageToFile(string filePath, string row, Exception ex, string defaultErrorSubDirPath)
		{
			// Create the file if it's not exist.
			if (_errorFilePath == string.Empty)
			{
				string errorDir;

				try
				{
					errorDir = FormatPath(AppSettings.Get(this, "ErrorSubDirectory"));
				}
				catch
				{
					errorDir = defaultErrorSubDirPath;
				}

				_errorFilePath = InitalizeFileName(filePath, DateTime.Now, string.Empty, errorDir);
			}

			
			// Add the line to error file.
			using (StreamWriter textWriter = new StreamWriter(_errorFilePath, true, Encoding.Unicode))
			{
				textWriter.WriteLine(row + string.Format("\t Exception message: {0}, Exception Type: {1}", ex.Message.ToString(), ex.GetType().FullName));
			}
		}

		/// <summary>
		/// Get from source DB the path of the report xml file for by account ID
		/// and service parent ID.
		/// </summary>
		/// <returns>Path of report xml file.</returns>
		protected virtual bool GetReportPath(ref string xmlPath, ref DateTime RetrieverDate, string serviceType)
		{
			SqlDataReader selectResults = null;

			// Initalize select command to get report xml file path.
			SqlCommand selectCommand = DataManager.CreateCommand(@"
				select RetrieveDate, Path
				from RetrievedFiles
				where AccountID = @accountID:Int and ServiceType = @serviceType:NVarChar
						  ParentInstanceID = @parentRunID:Int");

			using (IDisposable mc = new SqlConnection(_sourceConnectionString))
			{
				// Initalize connetion.
				SqlConnection manualConnection = (SqlConnection)mc;
				manualConnection.Open();
				selectCommand.Connection = manualConnection;

				// Initalize select command parameters.
				selectCommand.Parameters["@accountID"].Value = _accountID;
				selectCommand.Parameters["@serviceType"].Value = serviceType;
				// WCF: here too
				if (Instance.ParentInstance != null)
					selectCommand.Parameters["@parentRunID"].Value = Instance.ParentInstance.InstanceID;
				else
				{
					Log.Write(string.Format("There isn't service parent object for the service {0}", Instance), LogMessageType.Error);
					return false;
				}

				try
				{
					// Execute select command.
					selectResults = selectCommand.ExecuteReader();
				}
				catch (Exception ex)
				{
					Log.Write("Error get report data from RetrievedFiles table in Source DB.", ex);
					return false;
				}

				// Check if we get data from the DB.
				if (!selectResults.HasRows)
				{
					Log.Write(string.Format("Error get report data from RetrievedFiles table in Source DB for account ID {0}.", _accountID.ToString()), LogMessageType.Error);
					return false;
				}

				// Get the report data from the DB.
				selectResults.Read();

				try
				{
					xmlPath = selectResults["Path"].ToString();
					RetrieverDate = (DateTime)selectResults["RetrieveDate"];

					// Check if file exist.
					if (!File.Exists(xmlPath))
					{
						Log.Write(string.Format("File {0} doesn't exist account ID {1}.", xmlPath, _accountID.ToString()), LogMessageType.Error);
						return false;
					}
				}
				catch (Exception ex)
				{
					Log.Write("Error get ReportFileName from ReportFileName table in source DB.", ex);
					return false;
				}
				return true;
			}
		}

		/// <summary>
		/// Get from source DB the path of the report xml file for by account ID
		/// and service parent ID.
		/// </summary>
		/// <returns>Path of report xml file.</returns>
		protected virtual void GetReportPath(ArrayList filesPaths, string serviceType)
		{
			SqlDataReader selectResults = null;

			// Initalize select command to get report xml file path.
//            SqlCommand selectCommand = DataManager.CreateCommand(@"
//				select Path
//				from RetrievedFiles
//				where AccountID = @accountID:Int and ServiceType = @serviceType:NVarChar
//						  and ParentInstanceID = @parentRunID:Int");

			SqlCommand selectCommand = DataManager.CreateCommand(@"
				select Path
				from RetrievedFiles
				where AccountID = @accountID:Int 
						  and ParentInstanceID = @parentRunID:Int");


			using (IDisposable mc = new SqlConnection(_sourceConnectionString))
			{
				// Initalize connetion.
				SqlConnection manualConnection = (SqlConnection)mc;
				manualConnection.Open();
				selectCommand.Connection = manualConnection;

				// Initalize select command parameters.
				selectCommand.Parameters["@accountID"].Value = _accountID;
				//selectCommand.Parameters["@ServiceType"].Value = serviceType;

				if (Instance.ParentInstance != null)
					selectCommand.Parameters["@parentRunID"].Value = Instance.ParentInstance.InstanceID;
				else
					throw new Exception(string.Format("There isn't service parent object for the service {0}", Instance.InstanceID.ToString()));

				try
				{
					// Execute select command.
					selectResults = selectCommand.ExecuteReader();
				}
				catch (Exception ex)
				{
					throw new Exception(String.Format("Error get report data from Adwords_API_Retriever table in Source DB. Exception message: {0}.", ex.Message));
				}

				// Check if we get data from the DB.
				if (!selectResults.HasRows)
					throw new Exception("There is no reteiver file in table 'RetrievedFiles'");

				// Get the report data from the DB.
				//selectResults.Read();
				string xmlPath = string.Empty;

				try
				{
					while (selectResults.Read())
					{						
						xmlPath = selectResults["Path"].ToString();
						// Check if file exist.
						if (!File.Exists(xmlPath))
							Log.Write(string.Format("Xml report file {0} doesn't exist.", xmlPath), LogMessageType.Error);
						filesPaths.Add(xmlPath);
					}
					selectResults.Close();
				}
				catch (Exception ex)
				{
					throw new Exception(String.Format("Error get ReportFileName from ReportFileName table in source DB. Exception message: {0}.", ex.Message));
				}
			}
		}

		/// <summary>
		/// Fetch OptionName from configuration of the service.
		/// </summary>
		/// <param name="OptionName">the option name of the field we want to fetch.</param>
		/// <returns>the value of the OptionName field.</returns>
		protected string GetConfigurationOptionsField(string OptionName)
		{
			string fieldsMapping = string.Empty;
			// Check if the field exist in the service Configuration.
			if (Instance.Configuration.Options[OptionName] == null &&
				(Instance.ParentInstance == null ||
				Instance.ParentInstance.Configuration.Options[OptionName] == null))
			{
				return fieldsMapping;
				//throw new Exception(string.Format("There isn't option value {0} for the service {1}.", OptionName, Instance.Configuration.Name));
			}

			//  Fetch the field value.
			if (Instance.Configuration.Options[OptionName] == null)
				fieldsMapping = Instance.ParentInstance.Configuration.Options[OptionName].ToString();
			else
				fieldsMapping = Instance.Configuration.Options[OptionName].ToString();

			return fieldsMapping;
		}

		/*=========================*/
		#endregion
	}
}
