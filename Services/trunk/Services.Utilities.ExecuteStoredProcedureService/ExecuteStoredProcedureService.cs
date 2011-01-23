using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Data;
using System.Text.RegularExpressions;
using System.Reflection;
using Easynet.Edge.Core.Configuration;

namespace Easynet.Edge.Services.Utilities.ExecuteStoredProcedureService
{
	public class ExecuteStoredProcedureService : Service
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
			int timeout;
			if (Instance.Configuration.Options.TryGetValue("ConnectionTimeout", out timeoutStr) &&
				Int32.TryParse(timeoutStr, out timeout))
				DataManager.CommandTimeout = timeout;

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
					string dynamicParam = configVal.Substring(1, configVal.Length - 2).ToLower();

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
						PropertyInfo property = typeof(ServiceInstance).GetProperty(dynamicParamParts[0]);
						value = property.GetValue(targetInstance, null);
					}
				}

				param.Value = value;
			}

		}

		protected override ServiceOutcome DoWork()
		{
			using (DataManager.Current.OpenConnection())
			{
				DataManager.Current.AssociateCommands(_cmd);
				_cmd.ExecuteNonQuery();
			}

			return ServiceOutcome.Success;
		}
	}
}
