using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;

namespace Easynet.Edge.Wizards
{
	#region Interfaces
	[ServiceContract]
	public interface IStepCollector
	{
		[OperationContract]
		[NetDataContract]
		Dictionary<string, string> Collect(Dictionary<string, object> input);
	}
	#endregion

	#region Classes
	/// <summary>
	/// Base class for wizard step collection.
	/// </summary>
	public abstract class StepCollectorService : Service, IStepCollector
	{
		#region Fields
		protected Dictionary<string, object> _validatedInput { get; private set; }
		protected ServiceHost _stepCollectorHost;
		protected int _step;

		protected WizardSession WizardSession
		{

			get
			{

				// TODO:
				//	provide access to the session data (collected data of all
				//	steps already completed during this session)

				int sessionID = Int32.Parse(Instance.ParentInstance.ParentInstance.Configuration.Options["SessionID"]);
				int wizardID = Int32.Parse(Instance.ParentInstance.ParentInstance.Configuration.Options["WizardID"]);
				return new WizardSession() { WizardID = wizardID, SessionID = sessionID, CurrentStep = new StepConfiguration() { Step = _step, MetaData = null } };
			}
		}
		#endregion

		#region Public Methods
		public Dictionary<string, string> Collect(Dictionary<string, object> inputValues)
		{
			// Get validation errors from override
			Dictionary<string, string> errors;
			try { errors = Validate(inputValues); }
			catch (Exception ex)
			{
				return new Dictionary<string, string>()
				{
					{"Unknown error", ex.ToString()}
				};
			}

			// Return validation errors if relevant, otherwise call Run
			if (errors != null && errors.Count > 0)
			{
				// Return errors
				return errors;
			}
			else
			{
				// Save the input for DoWork()
				_validatedInput = inputValues;
				Thread t = new Thread(new ThreadStart(this.Run));
				t.Start();

				return null;
			}
		}
		#endregion

		#region Protected Methods
		protected override void OnInit()
		{
			string sessionID = this.Instance.ParentInstance.ParentInstance.Configuration.Options["SessionID"];
			_stepCollectorHost = new ServiceHost(this);
			_stepCollectorHost.AddServiceEndpoint(typeof(IStepCollector), new NetTcpBinding("wizardStepBinding"), String.Format("net.tcp://localhost:3636/wizard/step/{0}", sessionID));
			_stepCollectorHost.Open();

		}
		protected override ServiceOutcome DoWork()
		{
			// We still haven't gotten a proper Collect()
			if (_validatedInput == null)
				return ServiceOutcome.Unspecified;

			// Now we can store the input in the db and report success
			Prepare();
			return ServiceOutcome.Success;
		}
		protected abstract Dictionary<string, string> Validate(Dictionary<string, object> inputValues);
		protected virtual void Prepare()
		{
			using (DataManager.Current.OpenConnection())
			{
				int sessionID = WizardSession.SessionID;
				int wizardID = WizardSession.WizardID;
				int step = WizardSession.CurrentStep.Step;
				foreach (KeyValuePair<string, object> input in _validatedInput)
				{
					using (SqlCommand sqlCommand = DataManager.CreateCommand(@"INSERT INTO Wizards_Data_Per_WizardID_SessionID_Step_And_Field 
																			(WizardID,SessionID,Step,Field,Value)
																			Values 
																			(@WizardID:Int,
																			@SessionID:Int,
																			@Step:Int,
																			@Field:NVarChar,
																			@Value:NVarChar)"))
					{
						sqlCommand.Parameters["@WizardID"].Value = wizardID;
						sqlCommand.Parameters["@SessionID"].Value = sessionID;
						sqlCommand.Parameters["@Step"].Value = step;
						sqlCommand.Parameters["@Field"].Value = input.Key;
						sqlCommand.Parameters["@Value"].Value = input.Value.ToString();
						sqlCommand.ExecuteNonQuery();

					}
				}
				using (SqlCommand sqlCommand = DataManager.CreateCommand(@"UPDATE Wizards_Sessions_Data 
																			SET 
																			CurrentStep=@CurrentStep:Int
																			WHERE SessionID=@SessionID:Int"))
				{
					sqlCommand.Parameters["@CurrentStep"].Value = step+1;
					sqlCommand.Parameters["@SessionID"].Value = sessionID;
					sqlCommand.ExecuteNonQuery();

				}
			}
		}
		#endregion

		protected override void OnEnded(ServiceOutcome outcome)
		{
			if (_stepCollectorHost != null && _stepCollectorHost.State == System.ServiceModel.CommunicationState.Opened)
				_stepCollectorHost.Close();
		}
	}
	#endregion
}


