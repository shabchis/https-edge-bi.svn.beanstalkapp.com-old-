using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;

namespace Easynet.Edge.Wizards
{

	#region Interfaces
	/// <summary>
	/// Step Collector Interface
	/// </summary>
	[ServiceContract]
	public interface IStepCollector
	{
		[OperationContract]
		[NetDataContract]
		StepCollectResponse Collect(Dictionary<string, object> input);
	}
	#endregion

	#region Classes
	/// <summary>
	/// Base class for wizard step collection.
	/// </summary>
	public abstract class StepCollectorService : Service, IStepCollector
	{
		#region consts
		protected const string System_Field_Step_Description = "StepDescription";
		#endregion

		#region Fields
		protected Dictionary<string, object> ValidatedInput { get; set; }
		protected ServiceHost StepCollectorHost;
		protected string StepName;
		protected string StepDescription;
		/// <summary>
		/// Return the wizard session data
		/// </summary>
		protected WizardSession WizardSession
		{

			get
			{



				int sessionID = Int32.Parse(Instance.ParentInstance.ParentInstance.Configuration.Options["SessionID"]);
				int wizardID = Int32.Parse(Instance.ParentInstance.ParentInstance.Configuration.Options["WizardID"]);
				return new WizardSession() { WizardID = wizardID, SessionID = sessionID, CurrentStep = new StepConfiguration() { StepName = StepName, MetaData = null } };
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Main function collect data
		/// </summary>
		/// <param name="inputValues">keys and values to collect</param>
		/// <returns>step collect response</returns>
		public StepCollectResponse Collect(Dictionary<string, object> inputValues)
		{
			// Get validation errors from override
			Dictionary<string, string> errors;
			try { errors = Validate(inputValues); }
			catch (Exception ex)
			{
				Log.Write("Error while validating", ex);
				return new StepCollectResponse()
				{
					NextStep = new StepConfiguration() { StepName = Instance.Configuration.Name, MetaData = null },
					Errors = new Dictionary<string, string>()
				{
					{"Unknown error", ex.ToString()}
				},
					Result = StepResult.HasErrors
				};
			}

			// Return validation errors if relevant, otherwise call Run
			if (errors != null && errors.Count > 0)
			{
				// Return errors
				return new StepCollectResponse()
				{
					NextStep = new StepConfiguration() { StepName = Instance.Configuration.Name, MetaData = null },
					Errors = errors,
					Result = StepResult.HasErrors
				};
			}
			else
			{
				// Save the input for DoWork()
				ValidatedInput = inputValues;

				// GUIDLINES: Only lightweight stscoperations because the client is waiting for a response from this function

				try
				{
					Prepare();
				}
				catch (Exception ex)
				{
					Log.Write("Error while Peparing", ex);
					return new StepCollectResponse()
					{
						NextStep = new StepConfiguration() { StepName = Instance.Configuration.Name, MetaData = null },
						Errors = new Dictionary<string, string>()
				{
					{"Error while Peparing", ex.ToString()}
				},
						Result = StepResult.HasErrors
					};
				}

				Thread t = new Thread(new ThreadStart(this.Run));
				t.Start();
				if (Instance.Configuration.Options["LastStep"] == "true")
				{
					return new StepCollectResponse() { Errors = null, NextStep = new StepConfiguration() { MetaData = null, StepName = Instance.Configuration.Name }, Result = StepResult.Done };
				}
				else
				{
					return new StepCollectResponse() { Errors = null, NextStep = new StepConfiguration() { MetaData = null, StepName = Instance.Configuration.Name }, Result = StepResult.Next };
				}
			}
		}
		#endregion

		#region Protected Methods
		/// <summary>
		/// Start new wcf session
		/// </summary>
		protected override void OnInit()
		{
			string sessionID = WizardSession.SessionID.ToString();
			StepCollectorHost = new ServiceHost(this, new Uri(String.Format("net.tcp://localhost:3636/wizard/step/{0}", sessionID)));




			StepCollectorHost.Open();

		}
		protected override ServiceOutcome DoWork()
		{
			// We still haven't gotten a proper Collect()
			if (ValidatedInput == null)
				return ServiceOutcome.Unspecified;

			// Now we can store the input in the db and report success
			return ServiceOutcome.Success;
		}
		protected abstract Dictionary<string, string> Validate(Dictionary<string, object> inputValues);
		protected virtual void Prepare()
		{
			using (DataManager.Current.OpenConnection())
			{
				int sessionID = WizardSession.SessionID;
				int wizardID = WizardSession.WizardID;

				foreach (KeyValuePair<string, object> input in ValidatedInput)
				{
					using (SqlCommand sqlCommand = DataManager.CreateCommand(@"INSERT INTO Wizards_Data_Per_WizardID_SessionID_Step_And_Field 
																			(WizardID,SessionID,ServiceInstanceID,StepName,Field,Value)
																			Values 
																			(@WizardID:Int,
																			@SessionID:Int,
																			@ServiceInstanceID:BigInt,
																			@StepName:NvarChar,
																			@Field:NVarChar,
																			@Value:NVarChar)"))
					{
						sqlCommand.Parameters["@WizardID"].Value = wizardID;
						sqlCommand.Parameters["@SessionID"].Value = sessionID;
						sqlCommand.Parameters["@ServiceInstanceID"].Value = Instance.ParentInstance.ParentInstance.InstanceID;
						sqlCommand.Parameters["@StepName"].Value = StepName;
						sqlCommand.Parameters["@Field"].Value = input.Key;
						sqlCommand.Parameters["@Value"].Value = input.Value.ToString();
						sqlCommand.ExecuteNonQuery();

					}
				}

			}
		}
		protected override void OnEnded(ServiceOutcome outcome)
		{
			if (StepCollectorHost != null && StepCollectorHost.State == System.ServiceModel.CommunicationState.Opened)
				StepCollectorHost.Close();
		}
		#endregion

		/// <summary>
		/// unload the wcf
		/// </summary>
		/// <param name="outcome"></param>

	}
	#endregion
}


