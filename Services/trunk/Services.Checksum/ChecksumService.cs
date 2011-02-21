using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using Services.Checksum.Objects;
using Easynet.Edge.Core.Data;
using EdgeBI.Data.Readers;
using System.Data.SqlClient;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Services.Checksum.Configuration;

namespace Services.Checksum
{
	public class ChecksumService: Service
	{
		protected override ServiceOutcome DoWork()
		{
			// ----------------------
			// PHASE HANDLER

			string phase = Instance.Configuration.Options["Phase"];
			if (String.IsNullOrEmpty(phase))
				throw new ConfigurationException(
					"'Phase' option was not passed to the service.");

			if (!Instance.Configuration.ExtendedElements.ContainsKey("Phases"))
				throw new ConfigurationException(
					"No phases defined in configuration, cannot run checksum.");

			PhaseElementCollection phasesList = (PhaseElementCollection)Instance.Configuration.ExtendedElements["Phases"];
			PhaseElement phaseConfig = phasesList[phase];
			if (phaseConfig == null)
				throw new ConfigurationException(String.Format(
					"Specified phase '{0}' is not defined in the configuration.", phase));

			Type handlerType = Type.GetType(phaseConfig.HandlerType, false);
			if (handlerType == null)
				throw new ConfigurationException(String.Format(
					"Handler type for phase '{0}' was not found.", phase));

			// Create the phase handler
			PhaseHandler handler;
			try { handler = (PhaseHandler) Activator.CreateInstance(handlerType); }
			catch (Exception ex)
			{
				throw new Exception(
					"Failed to create phase handler.", ex);
			}
			handler.Instance = this.Instance;

			// ----------------------
			// TEST

			
			// Run all processing inside a single connection
			using (DataManager.Current.OpenConnection())
			{
				// Get or create a new test
				Test test;

				string raw_testID = Instance.Configuration.Options["TestID"];	
				if (!String.IsNullOrEmpty(raw_testID))
				{
					// Resume test
					Exception ex = ConfigurationException(String.Format(
						"Specified test ID {0} does not exist.", raw_testID));

					int testID;
					try { testID = Int32.Parse(raw_testID); }
					catch { throw ex; }

					test = Test.GetByID(testID);
					if (test == null)
						throw ex;
				}
				else
				{
					// New test
					string testTypeName = Instance.Configuration.Options["TestType"];

					TestType testType = TestType.GetByName(testTypeName);
					if (testType == null)
						throw new ConfigurationException(String.Format(
							"Specified test type '{0}' does not exist.", testTypeName));

					test = new Test(testType);
				}

				// Init handler and add any metadata
				handler.Test = test;

				try { handler.Init(); }
				catch (Exception ex) { throw new Exception(
					"Failed to initialize the phase handler.", ex); }
				
				// Save the test to get a valid test ID
				try { test.Save(); }
				catch (Exception ex) { throw new Exception(
					"Failed to update the Test metadata.", ex);

				// Allow the handler to prepare any data it needs for processing
				try { handler.Prepare(); }
				catch (Exception ex) { throw new Exception(
					"Error while preparing the execution phase.", ex);
			}

			return ServiceOutcome.Failure;
		}


	}
}
