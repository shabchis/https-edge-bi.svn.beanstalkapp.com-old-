using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Data;

using System.Data.SqlClient;


namespace Easynet.Edge.Wizards
{
	/// <summary>
	/// WizardSerivce class negotiate with the UI 
	/// </summary>
	[ServiceContract]
	[ServiceBehavior(InstanceContextMode=InstanceContextMode.Single)]
	public class WizardRestService
	{
		#region Fields
		static Dictionary<int, ServiceInstance> _stepInstances=new Dictionary<int,ServiceInstance>();
		#endregion
		

		#region Events
		void instance_OutcomeReported(object sender, EventArgs e)
		{
			
		}

		void instance_StateChanged(object sender, ServiceStateChangedEventArgs e)
		{
			ServiceInstance instance = (ServiceInstance) sender;
			
			if (e.StateAfter == ServiceState.Ready)
			{
				Console.WriteLine("Starting {0}\n", instance.Configuration.Name);
				instance.Start();
				
			}
			else if (e.StateAfter == ServiceState.Ended)
			{
				Console.WriteLine("{0} has ended.", instance.Configuration.Name);
			}

		}

		void instance_ProgressReported(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}
		
		void instance_ChildServiceRequested(object sender, ServiceRequestedEventArgs e)
		{
			Console.WriteLine(e.RequestedService);
			ServiceInstance instance = e.RequestedService;
			instance.StateChanged += new EventHandler<ServiceStateChangedEventArgs>(instance_StateChanged);
			instance.OutcomeReported += new EventHandler(instance_OutcomeReported);
			instance.ChildServiceRequested += new EventHandler<ServiceRequestedEventArgs>(instance_ChildServiceRequested);
			instance.ProgressReported += new EventHandler(instance_ProgressReported);

			instance.Initialize();

			if (e.RequestedService.Configuration.Options["IsStep"] != null)
			{
				int sessionID =int.Parse(e.RequestedService.ParentInstance.ParentInstance.Configuration.Options["SessionID"]);
				lock (_stepInstances)
					_stepInstances[sessionID] = e.RequestedService;
			  
			}

			
		}
		#endregion


		#region Public Methods
		/// <summary>
		/// Get the wizrdID and return new session
		/// </summary>
		/// <param name="wizardID"></param>
		/// <returns>New SessionID</returns>
		[WebInvoke(Method = "GET", UriTemplate = "start?wizardID={wizardID}")]
		public WizardSession Start(int wizardID)
		{
			#region Datbase
			//Get the name of the specific wizard from database

			object wizardName = null;
			object sessionID = null;
			using (DataManager.Current.OpenConnection())
			{
				using (SqlCommand sqlCommand = DataManager.CreateCommand("SELECT WizardName FROM Wizards_Info WHERE WizardID=@WizardID:Int"))
				{
					sqlCommand.Parameters["@WizardID"].Value = wizardID;
					wizardName = sqlCommand.ExecuteScalar();
					if (wizardName == null)
						throw new Exception(string.Format("Error: Wizard with WizardID:{0} could not be found", wizardID));

				}


				//Open new session by insert values in to the wizards_sessions_data table

				using (SqlCommand sqlCommand = DataManager.CreateCommand(@"INSERT INTO Wizards_Sessions_Data 
																		    (WizardID,CurrentStep)
																			Values (@WizardID:Int,1);SELECT @@IDENTITY"))
				{
					sqlCommand.Parameters["@WizardID"].Value = wizardID;
					sessionID = sqlCommand.ExecuteScalar();
					if (sessionID == null)
						throw new Exception("Error: Session not created");
				}
			}
			#endregion

			//Load the Relevant services/Steps

			ServiceElement requestedWizardConfiguration = ServicesConfiguration.Services[wizardName.ToString()];

			ActiveServiceElement configurationToRun = new ActiveServiceElement(requestedWizardConfiguration);

			configurationToRun.Options["WizardID"] = wizardID.ToString();
			configurationToRun.Options["SessionID"] = sessionID.ToString();


			ServiceInstance instance = Service.CreateInstance(configurationToRun);

			instance.ChildServiceRequested += new EventHandler<ServiceRequestedEventArgs>(instance_ChildServiceRequested);
			instance.ProgressReported += new EventHandler(instance_ProgressReported);
			instance.StateChanged += new EventHandler<ServiceStateChangedEventArgs>(instance_StateChanged);
			instance.OutcomeReported += new EventHandler(instance_OutcomeReported);

			instance.Initialize();




			//Create WizardSession object to return

			WizardSession wizardSession = new WizardSession();
			//-------------Talk with Doron about it 
			//wizardSession.CurrentStep = new StepConfiguration() { StepName = "Welcome", MetaData = new Dictionary<string, object>() }; 
			//-------------Talk with Doron about it 
			wizardSession.SessionID = System.Convert.ToInt32(sessionID); //@@IDENTITY IS DECIMAL
			wizardSession.WizardID = wizardID;






			return wizardSession;




		}

		[WebInvoke(Method = "GET", UriTemplate = "continue?sessionID={sessionID}")]
		public WizardSession Continue(int sessionID)
		{
			throw new NotImplementedException();
		}
		[WebInvoke(Method = "GET", UriTemplate = "test")]
		public StepCollectRequest test()
		{
			return new StepCollectRequest() { Step=1,CollectedValues=new Dictionary<string,object>(){
				{ "1111","1111"},
				{ "2222","3333"}}};
				
		}
		/// <summary>
		/// Get the request Call the right class to collect , and return repond
		/// </summary>
		/// <param name="stepCollectRequest"></param>
		/// <returns>Step collect Respond</returns>
		[WebInvoke(Method = "POST", UriTemplate = "collect?sessionID={sessionID}", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
		public StepCollectResponse Collect(int sessionID, StepCollectRequest request)
		{
			StepCollectResponse response;
			using (ServiceClient<IStepCollector> client = new ServiceClient<IStepCollector>("Test" ,String.Format("net.tcp://localhost:3636/wizard/step/{0}", sessionID)))
			{
				
				Dictionary<string, string> errors = client.Service.Collect(request.CollectedValues);
				if (errors != null)
				{
					//Build StepCollectResponse with errors inside
					response = new StepCollectResponse() { Errors = errors, NextStep = new StepConfiguration() { MetaData = null, Step = request.Step }, Result = StepResult.HasErrors };
					
				}
				else
				{
					// Build response with Status NextStep
					 response = new StepCollectResponse() { Errors = null, Result = StepResult.Next, NextStep = new StepConfiguration() { MetaData = null, Step = 1 } };
				}
			}
			return response;

		}

		/// <summary>
		/// Return a text with summary of what is going to happend on the execute
		/// </summary>
		/// <param name="sessionID"></param>
		/// <returns>string summary</returns>
		[WebInvoke(Method = "GET", UriTemplate = "summary?sessionID={sessionID}")]
		public string GetSummary(int sessionID)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// After finished Collecting call all the executers and return the wizardstatus
		/// </summary>
		/// <param name="sessionID"></param>
		/// <returns>return error of success </returns>
		[WebInvoke(Method = "GET", UriTemplate = "execute?sessionID={sessionID}")]
		public WizardStatus Execute(int sessionID)
		{
			throw new System.NotImplementedException();
		}
		#endregion

	}

	#region Enums
	/// <summary>
	/// The wizard status
	/// </summary>
	public enum WizardStatus
	{
		Started,
		Collecting,
		ReadyToExecute,
		FinishedSuccessfuly,
		Error
	}
#endregion
}
