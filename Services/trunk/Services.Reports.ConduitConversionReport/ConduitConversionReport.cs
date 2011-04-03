using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;
using System.Text.RegularExpressions;
using System.Reflection;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Utilities;

namespace Easynet.Edge.Services.Reports
{
	public class ConduitConversionReport : Service
	{
		SqlCommand _cmd;

		protected override void OnInit()
		{
			string sp = Instance.Configuration.Options["Procedure"];
			if (String.IsNullOrEmpty(sp))
				throw new ConfigurationException("Missing configuration option \"Procedure\".");

			// Check for a custom connection string
			string conn;
			if (Instance.Configuration.Options.TryGetValue("ConnectionString", out conn))
				DataManager.ConnectionString = conn;

			// Check for a custom timeout
			string timeoutStr;
			TimeSpan _cmdTimeOut;
			if (Instance.Configuration.Options.TryGetValue("ConnectionTimeout", out timeoutStr))
			{
				if (TimeSpan.TryParse(timeoutStr, out _cmdTimeOut))
					DataManager.CommandTimeout = (Int32)(_cmdTimeOut.TotalSeconds);
			}

			// Build the command
			_cmd = DataManager.CreateCommand(sp, System.Data.CommandType.StoredProcedure);
			foreach (SqlParameter param in _cmd.Parameters)
			{
				string name = param.ParameterName.Remove(0, 1); //fix by alon on 20/1/2001 remove the '@'
				string configVal;
				if (!Instance.Configuration.Options.TryGetValue("param." + name, out configVal)) //also remove the s from params
					continue;

				// Apply the configuration value, before we check if we need to parse it
				object value = configVal;

				// Dynamic Params
				if (configVal.StartsWith("{") && configVal.EndsWith("}"))
				{
					ServiceInstanceInfo targetInstance = Instance;
					string dynamicParam = configVal.Substring(1, configVal.Length - 2);

					// Go up levels ../../InstanceID
					int levelsUp = Regex.Matches(dynamicParam, @"\.\.\/").Count;
					for (int i = 0; i < levelsUp; i++)
						targetInstance = targetInstance.ParentInstance;

					// Split properties into parts (Configuration.Options.BlahBlah);
					dynamicParam = dynamicParam.Replace("../", string.Empty);
					string[] dynamicParamParts = dynamicParam.Split('.');

					// Get the matching property
					if (dynamicParamParts[0] == "Configuration" && dynamicParamParts.Length > 1)
					{
						if (dynamicParamParts[1] == "Options" && dynamicParamParts.Length > 2)
						{
							// Asked for an option
							value = targetInstance.Configuration.Options[dynamicParamParts[2]];
						}
						else
						{
							// Asked for some other configuration value
							PropertyInfo property = typeof(ServiceElement).GetProperty(dynamicParamParts[1]);
							value = property.GetValue(targetInstance.Configuration, null);
						}
					}
					else
					{
						// Asked for an instance value
						//Alon & Shay Bug fix = incorrect date format ( daycode )
						PropertyInfo property = typeof(ServiceInstanceInfo).GetProperty(dynamicParamParts[0]);
						if (!property.PropertyType.FullName.Equals("System.DateTime"))
						{
							value = property.GetValue(targetInstance, null);

						}
						else
						{
							value = ((DateTime)property.GetValue(targetInstance, null)).ToString("yyyyMMdd");
							// Log.Write("Delete Days - DayCode " + (string)value, LogMessageType.Information);
						}
					}
				}
				
				param.Value = value;
				
				Log.Write(string.Format("ExecuteStoredProcedureService Value{0} ", value), LogMessageType.QA);
			}

		}

		protected override ServiceOutcome DoWork()
		{
			using (DataManager.Current.OpenConnection())
			{
				Report _report = new Report();
				DataManager.Current.AssociateCommands(_cmd);
				//Log.Write(_cmd.ToString(), LogMessageType.Information);
				//_cmd.ExecuteNonQuery();
				using (SqlDataReader _reader = _cmd.ExecuteReader())
				{
					if (!_reader.IsClosed)
						while (_reader.Read())
						{
							_report.AddRow(_reader);
						}
				}

			}

			return ServiceOutcome.Success;
		}
	}
}
