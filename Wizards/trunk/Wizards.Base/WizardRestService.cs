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
				if (instance.Configuration.Options["WizardRole"] == "ExecutorContainer")
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
			if (e.RequestedService.Configuration.Options["WizardRole"] == "ExecutorContainer")
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
		/// <param name="WizardName"></param>
		/// <returns>New SessionID</returns>
		[WebInvoke(Method = "GET", UriTemplate = "start?wizardName={WizardName}")]
		public WizardSession Start(string WizardName)
		{
			
			//load the service from configuration file 
			//CHANGED 1 (Service is taken from app.config- what to do if service not found?)
			int wizardID;
			ServiceElement requestedWizardConfiguration = ServicesConfiguration.Services[WizardName];
			if (requestedWizardConfiguration == null)
			{
				throw new Exception("Wizard cannot be found");
			}



			//Get service data
			wizardID = int.Parse(requestedWizardConfiguration.Options["WizardID"]); //to insert in database it's int future
			ExecutionStepElement FirstStepCollector = GetFirstCollectorStep(requestedWizardConfiguration.ExecutionSteps);
			ActiveServiceElement configurationToRun = new ActiveServiceElement(requestedWizardConfiguration);

			


			ServiceInstance instance = Service.CreateInstance(configurationToRun);


			instance.ChildServiceRequested += new EventHandler<ServiceRequestedEventArgs>(instance_ChildServiceRequested);
			instance.ProgressReported += new EventHandler(instance_ProgressReported);
			instance.StateChanged += new EventHandler<ServiceStateChangedEventArgs>(instance_StateChanged);
			instance.OutcomeReported += new EventHandler(instance_OutcomeReported);


			instance.Initialize();
			object sessionID = null;
			using (DataManager.Current.OpenConnection())
			{



				//Open new session by insert values in to the wizards_sessions_data table

				using (SqlCommand sqlCommand = DataManager.CreateCommand(@"INSERT INTO Wizards_Sessions_Data 
																		    (WizardID,CurrentStepName,ServiceInstanceID,WizardStatus)
																			Values (@WizardID:Int,@CurrentStepName:NvarChar,@ServiceInstanceID:BigInt,@WizardStatus:Int);SELECT @@IDENTITY"))
				{
					sqlCommand.Parameters["@WizardID"].Value = wizardID;
					sqlCommand.Parameters["@CurrentStepName"].Value = FirstStepCollector.Name;
					sqlCommand.Parameters["@ServiceInstanceID"].Value = instance.InstanceID;
					sqlCommand.Parameters["@WizardStatus"].Value = WizardStatus.Collect;

					sessionID = sqlCommand.ExecuteScalar();
					if (sessionID == null)
						throw new Exception("Error: Session not created");
				}
			}
		//	configurationToRun.Options["SessionID"] = sessionID.ToString();
			instance.Configuration.Options["SessionID"] = sessionID.ToString();

			//Start the service
			



			//Create WizardSession object to return

			WizardSession wizardSession = new WizardSession();		
			wizardSession.SessionID = System.Convert.ToInt32(sessionID); //@@IDENTITY IS DECIMAL
			wizardSession.WizardID = wizardID;
			wizardSession.CurrentStep = new StepConfiguration() { StepName = FirstStepCollector.Name };





			return wizardSession;




		}

		private ExecutionStepElement GetFirstCollectorStep(ExecutionStepElementCollection executionStepElementCollection)
		{

			if (executionStepElementCollection != null)
			{
				foreach (ExecutionStepElement executionStep in executionStepElementCollection)
				{


					if (executionStep.Options["WizardRole"] != null)
					{
						if (executionStep.Options["WizardRole"] == "CollectorContainer")
						{


							if (executionStep.ServiceToUse.Element.ExecutionSteps.Count > 0)
							{

								return executionStep.ServiceToUse.Element.ExecutionSteps[0];
							}
						}
						else
						{
							GetFirstCollectorStep(executionStep.ServiceToUse.Element.ExecutionSteps);

						}

					}
					else
					{
						GetFirstCollectorStep(executionStep.ServiceToUse.Element.ExecutionSteps);
					}



				}
				return null;
			}
			return null;

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
		[WebInvoke(Method = "POST", UriTemplate = "collect?sessionID={sessionID}", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
		public StepCollectResponse Collect(int sessionID, StepCollectRequest request)
		{

			StepCollectResponse response = null;
			StepCollectResponse tempResponse;
			try
			{

				// TODO: make sure request.StepName matches StepInstances[sessionID].Configuration.Name
				if (request.StepName != StepInstances[sessionID].Configuration.Name)
				{
					throw new Exception("Step name is not matching current step!");
				}
				using (ServiceClient<IStepCollector> client = new ServiceClient<IStepCollector>("Test", String.Format("net.tcp://localhost:3636/wizard/step/{0}", sessionID)))
				{



					tempResponse = client.Service.Collect(request.CollectedValues); //CHANGE  doron - the response is done in the main collect
					if (tempResponse.Errors == null || tempResponse.Errors.Count == 0) //successfuly
					{
						response = new StepCollectResponse();
						//get next step or this finished/
						ServiceInstance currentInstance = StepInstances[sessionID];
						ServiceInstance parentInstance = currentInstance.ParentInstance;
						ExecutionStepElement currentInstacnceElement = parentInstance.Configuration.ExecutionSteps[currentInstance.Configuration.Name];

						int currentStepIndex = parentInstance.Configuration.ExecutionSteps.IndexOf(currentInstacnceElement);
						if (parentInstance.Configuration.ExecutionSteps.Count > currentStepIndex + 1) //their is mor steps
						{
							ExecutionStepElement nextStep = parentInstance.Configuration.ExecutionSteps[currentStepIndex + 1];
							using (DataManager.Current.OpenConnection())
							{
								using (SqlCommand sqlCommand = DataManager.CreateCommand(@"UPDATE Wizards_Sessions_Data 
																			SET 
																			CurrentStepName=@CurrentStepName:NvarChar,
																			WizardStatus=@WizardStatus:Int
																			WHERE SessionID=@SessionID:Int 
																			AND ServiceInstanceID=@ServiceInstanceID:BigInt"))
								{
									sqlCommand.Parameters["@CurrentStepName"].Value = nextStep.Name;
									sqlCommand.Parameters["@SessionID"].Value = sessionID;
									sqlCommand.Parameters["@WizardStatus"].Value = WizardStatus.Collect;
									sqlCommand.Parameters["@ServiceInstanceID"].Value = parentInstance.ParentInstance.InstanceID;
									sqlCommand.ExecuteNonQuery();

								}
							}
							response = new StepCollectResponse() { Errors = null, Result = StepResult.Next, NextStep = new StepConfiguration() { StepName = nextStep.Name } };


						}
						else //the last step of the collectors
						{
							//Change Status and current Step
							using (DataManager.Current.OpenConnection())
							{
								using (SqlCommand sqlCommand = DataManager.CreateCommand(@"UPDATE Wizards_Sessions_Data 
																			SET 
																			CurrentStepName=@CurrentStepName:NvarChar,
																			WizardStatus=@WizardStatus:Int
																			WHERE SessionID=@SessionID:Int 
																			AND ServiceInstanceID=@ServiceInstanceID:BigInt"))
								{
									sqlCommand.Parameters["@CurrentStepName"].Value = string.Empty;
									sqlCommand.Parameters["@SessionID"].Value = sessionID;
									sqlCommand.Parameters["@WizardStatus"].Value = WizardStatus.Execute;
									sqlCommand.Parameters["@ServiceInstanceID"].Value = parentInstance.ParentInstance.InstanceID;
									sqlCommand.ExecuteNonQuery();

								} 
							}
							response = new StepCollectResponse() { Errors = null, Result = StepResult.Done };

						}

					}
					else
					{
						response = new StepCollectResponse() { Result = StepResult.HasErrors, Errors = tempResponse.Errors };
					}




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
		/// After finished Collecting call all the executers and return the WizardStatus
		/// </summary>
		/// <param name="sessionID"></param>
		/// <returns>return error of success </returns>
		[WebInvoke(Method = "POST", UriTemplate = "execute?sessionID={sessionID}", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
		public void Execute(int sessionID)
		{
			ServiceInstance executer = MainExecuter[sessionID];
			executer.Start();

		}
		#endregion

	}

	#region Enums
	/// <summary>
	/// CollectStatus
	/// </summary>
	public enum WizardStatus
	{
		/// <summary>
		/// Collecting
		/// </summary>
		Collect,
		/// <summary>
		/// Execute
		/// </summary>
		Execute
		
	}
	#endregion
}
