﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.ComponentModel;
using Easynet.Edge.Core.Configuration;
using System.Diagnostics.Eventing.Reader;
using Easynet.Edge.Core.Services;
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;

/*
 * The Utilities Namespace is used for various utility classes such as logging and other functions
 *
 */
namespace Easynet.Edge.Core.Utilities
{
	[Serializable]
	public class LoggingException : Exception
	{
		public LoggingException() { }
		public LoggingException(string message) : base(message) { }
		public LoggingException(string message, Exception inner) : base(message, inner) { }
		protected LoggingException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

	public enum LogMessageType
	{
		Error = 1,
		Warning = 2,
		Information = 3
	};

	internal class LogEntry
	{
		static string ConnectionString = AppSettings.GetAbsolute("Easynet.Edge.Services.DataRetrieval.BaseService.SourceConnectionString");

		public string MachineName = Environment.MachineName;
		public int ProcessID = Process.GetCurrentProcess().Id;
		public string Source = null;
		public LogMessageType MessageType = LogMessageType.Information;
		public long ServiceInstanceID = -1;
		public int AccountID = -1;
		public string Message = null;
		public bool IsException = false;
		public string ExceptionDetails = null;

		public void Save()
		{
            /*Fixed buy alon 30/3/2001- bug-when  data manager.currecnt.openconnection() is starting transaction and 
             * on the log.save you create transaction you get error message:"ther transaction is either not associete with the current connection
             or has been commited.
             the fix is to create the comand not using the datamatanger.createCommand so the transaction will not be associte withe the log connection*/
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = @"insert into Log
				(
					MachineName,
					ProcessID,
					Source,
					MessageType,
					ServiceInstanceID,
					AccountID,
					Message,
					IsException,
					ExceptionDetails
				)
				values
				(
					@MachineName,
					@ProcessID,
					@Source,
					@MessageType,
					@ServiceInstanceID,
					@AccountID,
					@Message,
					@IsException,
					@ExceptionDetails
				)";


            cmd.Parameters.AddWithValue("@MachineName", this.MachineName);
            cmd.Parameters.AddWithValue("@ProcessID", this.ProcessID);
            cmd.Parameters.AddWithValue("@Source", this.Source);
            cmd.Parameters.AddWithValue("@MessageType", this.MessageType);
            cmd.Parameters.AddWithValue("@ServiceInstanceID", this.ServiceInstanceID);
            cmd.Parameters.AddWithValue("@AccountID", this.AccountID);
            cmd.Parameters.AddWithValue("@Message", Null(this.Message));
            cmd.Parameters.AddWithValue("@IsException", this.IsException);
            cmd.Parameters.AddWithValue("@ExceptionDetails", Null(this.ExceptionDetails));
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                cmd.Connection = connection;
                cmd.ExecuteNonQuery();
            }
//            SqlCommand cmd = DataManager.CreateCommand(@"
//				insert into Log
//				(
//					MachineName,
//					ProcessID,
//					Source,
//					MessageType,
//					ServiceInstanceID,
//					AccountID,
//					Message,
//					IsException,
//					ExceptionDetails
//				)
//				values
//				(
//					@MachineName:NVarChar,
//					@ProcessID:Int,
//					@Source:NVarChar,
//					@MessageType:Int,
//					@ServiceInstanceID:BigInt,
//					@AccountID:Int,
//					@Message:NVarChar,
//					@IsException:Bit,
//					@ExceptionDetails:NVarChar
//				)
//			");

//            cmd.Parameters["@MachineName"].Value = this.MachineName;
//            cmd.Parameters["@ProcessID"].Value = this.ProcessID;
//            cmd.Parameters["@Source"].Value = this.Source;
//            cmd.Parameters["@MessageType"].Value = this.MessageType;
//            cmd.Parameters["@ServiceInstanceID"].Value = this.ServiceInstanceID;
//            cmd.Parameters["@AccountID"].Value = this.AccountID;
//            cmd.Parameters["@Message"].Value = Null(this.Message);
//            cmd.Parameters["@IsException"].Value = this.IsException;
//            cmd.Parameters["@ExceptionDetails"].Value = Null(this.ExceptionDetails);

//            using (SqlConnection connection = new SqlConnection(ConnectionString))
//            {
//                connection.Open();
//                cmd.Connection = connection;
//                cmd.ExecuteNonQuery();
//            }
		}

		object Null(object obj)
		{
			if (obj == null)
				return DBNull.Value;
			else
				return obj;
		}
	}



    /// <summary>
    /// A class which writes events into the windows event viewer.
    /// </summary>
    public class Log
    {        
		//public static string LogName = AppSettings.Get(typeof(Log), "LogName");
		//private EventLog _log = new EventLog(LogName);
		private string _source;
		private IServiceInstance _instance;

		/*
		static Log()
		{
			bool check;
			if (bool.TryParse(AppSettings.Get(typeof(Log), "AllowNoOverwrite", false), out check) && check)
				return;

			EventLog log = new EventLog(LogName);

			// See if the log source name exists and check the OverflowAction status.
			// If the log is not decalred as OverwriteAsNeeded we throw a LoggingException.
			if (EventLog.SourceExists(LogName) && log.OverflowAction != OverflowAction.OverwriteAsNeeded)
				throw new LoggingException(
					"Event log requires the \"overwrite as needed\" setting. " +
					"Configure this via the Windows Event Viewer. " +
					"If you want more contol over event log settings," +
					"set Easynet.Edge.Core.Utilities.Log.AllowNoOverwrite " +
					"to true in the application settings.");
		}
		 */

		internal Log(IServiceInstance instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");
			
			_instance = instance;

			//_log.Source = instance.Configuration.Name;
			_source = instance.Configuration.Name;
		}

		internal Log(string source)
		{
			if (String.IsNullOrEmpty(source))
				throw new ArgumentNullException("source");

			//_log.Source = source;
			_source = source;
		}

		internal void InternalWrite(string message, Exception ex, LogMessageType messageType)
		{
			LogEntry entry = new LogEntry();
			entry.Source = _source;
			entry.MessageType = messageType;
			entry.Message = message;

			if (_instance != null)
			{
				entry.AccountID = _instance.AccountID;
				entry.ServiceInstanceID = _instance.InstanceID;
			}

			if (ex != null)
			{
				entry.IsException = true;
				entry.ExceptionDetails = ex.ToString();
			}

			entry.Save();
		}

		public static void Write(string message, Exception ex, LogMessageType messageType)
		{
			if (Service.Current == null)
				throw new InvalidOperationException("Source parameter must be specified when writing to the log outside of a service context.");

			Service.Current.Log.InternalWrite(message, ex, messageType);
		}

		public static void Write(string message, LogMessageType messageType)
		{
			Write(message, (Exception) null, messageType);
		}

		public static void Write(string message, Exception ex)
		{
			Write(message, ex, LogMessageType.Error);
		}
		
		public static void Write(string source, string message, Exception ex, LogMessageType messageType)
		{
			// Use source-less version when source is null
			if (String.IsNullOrEmpty(source))
				Write(message, ex, messageType);

			if (Service.Current != null)
				throw new InvalidOperationException("Cannot specify source when writing to the log within a service context.");

			Log log = new Log(source);
			log.InternalWrite(message, ex, messageType);
		}

		public static void Write(string source, string message, LogMessageType messageType)
		{
			Write(source, message, null, messageType);
		}

		public static void Write(string source, string message, Exception ex)
		{
			Write(source, message, ex, LogMessageType.Error);
		}

		
		/*
		static EventLogEntryType GetEventEntryType(LogMessageType type)
		{
			if (type == LogMessageType.Error)
				return EventLogEntryType.Error;

			if (type == LogMessageType.Information)
				return EventLogEntryType.Information;

			if (type == LogMessageType.Warning)
				return EventLogEntryType.Warning;

			return EventLogEntryType.Information;
		}
		*/

	}

	//[AttributeUsage(AttributeTargets.Class, Inherited=true, AllowMultiple=false)]
	//public sealed class LoggingSourceAttribute: Attribute
	//{
	//    string _name = null;
	//    public string Name
	//    {
	//        get { return _name; }
	//        set { _name = value; }
	//    }
	//}
}
