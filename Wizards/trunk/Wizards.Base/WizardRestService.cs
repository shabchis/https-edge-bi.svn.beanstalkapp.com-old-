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
using System.ServiceModel.Description;
using System.Net;


namespace Easynet.Edge.Wizards
{
	/// <summary>
	/// WizardSerivce class negotiate with the UI 
	/// </summary>
	[ServiceContract]
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class WizardRestService
	{
		#region Fields
		static Dictionary<int, ServiceInstance> StepInstances = new Dictionary<int, ServiceInstance>();
		static Dictionary<int, ServiceInstance> MainExecuter = new Dictionary<int, ServiceInstance>();
		#endregion


		#region Events
		void instance_OutcomeReported(object sender, EventArgs e)
		{


		}

		void instance_StateChanged(object sender, ServiceStateChangedEventArgs e)
		{
			ServiceInstance instance = (ServiceInstance)sender;

			if (e.StateAfter == ServiceState.Ready)
			{
				if (instance.Configuration.Name == "AccountWizardExecutors")
				{
					Console.WriteLine("Collect finish waiting for execute command");
					
				}
				else
				{
					Console.WriteLine("Starting {0}\n", instance.Configuration.Name);
					instance.Start();
				}

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
				int sessionID = int.Parse(e.RequestedService.ParentInstance.ParentInstance.Configuration.Options["SessionID"]);
				lock (StepInstances)
					StepInstances[sessionID] = e.RequestedService;

			}
			if (e.RequestedService.Configuration.Name=="AccountWizardExecutors")
			{
				int sessionID = int.Parse(e.RequestedService.ParentInstance.Configuration.Options["SessionID"]);
				lock (MainExecuter)
					MainExecuter[sessionID] = e.RequestedService;
				
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
																		    (WizardID,CurrentStepName)
																			Values (@WizardID:Int,0);SELECT @@IDENTITY"))
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
			return new StepCollectRequest()
			{
				StepName = "ONE",
				CollectedValues = new Dictionary<string, object>(){
				{ "1111","1111"},
				{ "2222","3333"}}
			};

		}
		/// <summary>
		/// Get the request Call the right class to collect , and return repond
		/// </summary>
		/// <param name="stepCollectRequest"></param>
		/// <returns>Step collect Respond</returns>
		[WebInvoke(Method = "POST", UriTemplate = "collect?sessionID={sessionID}", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml) ]
		public StepCollectResponse Collect(int sessionID, StepCollectRequest request)
		{
			
			StepCollectResponse response = null;
			try
			{
				
				// TODO: make sure request.StepName matches the step that is running

				using (ServiceClient<IStepCollector> client = new ServiceClient<IStepCollector>("Test", String.Format("net.tcp://localhost:3636/wizard/step/{0}", sessionID)))
				{



					response = client.Service.Collect(request.CollectedValues);
					



				}
			}
			catch (Exception ex)
			{
				//throw new Exception("bla bla bla");

				//throw new WebException("Service not started yet" + ex.Message, WebExceptionStatus.ConnectFailure);
				throw new WebFaultException<string>("Service not Started yet: " + ex.Message, HttpStatusCode.ServiceUnavailable);
			//	throw new WebFaultException<string>("bla bla bla", HttpStatusCode.Forbidden);
				//return new StepCollectResponse() { Errors = new Dictionary<string, string>() { { "Service is not started", ex.Message } }, Result = StepResult.HasErrors, NextStep = null };

			}
			return response;

		}

		/// <summary>
		/// Return a text with summary of what is going to happend on the execute
		/// </summary>
		/// <param name="sessionID"></param>
		/// <returns>string summary</returns>
		[WebInvoke(Method = "POST", UriTemplate = "summary?sessionID={sessionID}", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
		public string GetSummary(int sessionID)
		{
			StringBuilder summary = new StringBuilder();
			using (DataManager.Current.OpenConnection())
			{
				using (SqlCommand sqlCommand = DataManager.CreateCommand(@"SELECT StepName,Value 
																		FROM Wizards_Data_Per_WizardID_SessionID_Step_And_Field
																		WHERE SessionID=@SessionID:Int AND Field=N'StepDescription'"))
				{
					sqlCommand.Parameters["@SessionID"].Value = sessionID;
					using (SqlDataReader reader = sqlCommand.ExecuteReader())
					{
						while (reader.Read())
						{
							summary.AppendLine(string.Format("Step {0}:  {1}\n", reader.GetString(0), reader.GetString(1)));
						}
					}
				}

			}

			return summary.ToString();
		}

		/// <summary>
		/// After finished Collecting call all the executers and return the wizardstatus
		/// </summary>
		/// <param name="sessionID"></param>
		/// <returns>return error of success </returns>
		[WebInvoke(Method = "POST", UriTemplate = "execute?sessionID={sessionID}", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
		public WizardStatus Execute(int sessionID)
		{
			ServiceInstance executer = MainExecuter[sessionID];
			executer.Start();
			return WizardStatus.FinishedSuccessfuly;
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
