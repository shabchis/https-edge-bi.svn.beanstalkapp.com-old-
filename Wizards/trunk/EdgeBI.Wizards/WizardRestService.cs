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
using System.Diagnostics;


namespace EdgeBI.Wizards
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
		static Dictionary<int, ServiceInstance> MainExecuters = new Dictionary<int, ServiceInstance>();
		static Dictionary<int, List<ServiceInstance>> ExecuterSteps =new Dictionary<int,List<ServiceInstance>>();
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
			//Debugger.Launch();

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
			//first step name
			ExecutionStepElement FirstStepCollector = GetFirstCollectorStep(requestedWizardConfiguration.ExecutionSteps);
			if (FirstStepCollector == null)
				throw new Exception("No step collector found");



			ActiveServiceElement configurationToRun = new ActiveServiceElement(requestedWizardConfiguration);


			//Initalize the main wizard service (account service...etc)

			ServiceInstance instance = Service.CreateInstance(configurationToRun);


			instance.ChildServiceRequested += new EventHandler<ServiceRequestedEventArgs>(instance_ChildServiceRequested);
			instance.ProgressReported += new EventHandler(instance_ProgressReported);
			instance.StateChanged += new EventHandler<ServiceStateChangedEventArgs>(instance_StateChanged);
			instance.OutcomeReported += new EventHandler(instance_OutcomeReported);


			instance.Initialize();
			object sessionID = null;
			//Open new session by insert values in to the wizards_sessions_data table
			using (DataManager.Current.OpenConnection())
			{





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

			instance.Configuration.Options["SessionID"] = sessionID.ToString();

			//Create WizardSession object to return

			WizardSession wizardSession = new WizardSession();
			wizardSession.SessionID = System.Convert.ToInt32(sessionID); //@@IDENTITY IS DECIMAL
			wizardSession.WizardID = wizardID;
			wizardSession.CurrentStep = new StepConfiguration() { StepName = FirstStepCollector.Name };


			return wizardSession;




		}
		/// <summary>
		/// Get the execution state
		/// </summary>
		/// <param name="sessionID"></param>
		/// <returns></returns>
		[WebInvoke(Method = "GET", UriTemplate = "GetExecutorState?sessionID={sessionID}")]
		public ServiceOutcome GetExecutorState(int sessionID)
		{
			return MainExecuters[sessionID].Outcome;

		}

		

		[WebInvoke(Method = "GET", UriTemplate = "continue?sessionID={sessionID}")]
		public WizardSession Continue(int sessionID)
		{
			throw new NotImplementedException();
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
				// make sure request.StepName matches StepInstances[sessionID].Configuration.Name(client is in the right step)
				if (request.StepName != StepInstances[sessionID].Configuration.Name)
				{
					throw new Exception("Step name is not matching current step!");
				}
				//connect to the  collector 
				using (ServiceClient<IStepCollector> client = new ServiceClient<IStepCollector>("Test", String.Format("net.tcp://localhost:3636/wizard/step/{0}", sessionID)))
				{



					tempResponse = client.Service.Collect(request.CollectedValues);

					if (tempResponse.Errors == null || tempResponse.Errors.Count == 0) //successfuly
					{


						//get next step detalis or if it   finished/
						ServiceInstance currentInstance = StepInstances[sessionID];
						
						// This will cause the collector service to end successfully
						currentInstance.Continue();

						ServiceInstance parentInstance = currentInstance.ParentInstance;
						ExecutionStepElement currentInstacnceElement = parentInstance.Configuration.ExecutionSteps[currentInstance.Configuration.Name];

						int currentStepIndex = parentInstance.Configuration.ExecutionSteps.IndexOf(currentInstacnceElement);
						if (parentInstance.Configuration.ExecutionSteps.Count > currentStepIndex + 1) //their is more steps
						{
							ExecutionStepElement nextStep = parentInstance.Configuration.ExecutionSteps[currentStepIndex + 1];
							//Update the database on the current (next step)
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
							//create succesful response with the collected details
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
					else //error
					{
						response = new StepCollectResponse() { Result = StepResult.HasErrors, Errors = tempResponse.Errors };
					}




				}
			}
			catch (Exception ex)
			{
				Log.Write("Service not Started yet: ", ex);
				throw new WebFaultException<string>("Service not Started yet: " + ex.Message, HttpStatusCode.ServiceUnavailable);

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
			ServiceInstance executer = MainExecuters[sessionID];
			executer.Start();

		}
		/// <summary>
		/// Get the execution progrees
		/// </summary>
		/// <param name="sessionID"></param>
		/// <returns></returns>
		[WebInvoke(Method = "GET", UriTemplate = "Progress?sessionID={sessionID}")]
		public ProgressState GetProgress(int sessionID)
		{
			int totalExecutionSteps;
			ProgressState progressState = new ProgressState();
			
			try
			{
				ServiceInstance serviceExecutorsContainer = ExecuterSteps[sessionID][0].ParentInstance;
				//Get the count of executors step from configuration
				totalExecutionSteps = serviceExecutorsContainer.Configuration.ExecutionSteps.Count;
				foreach (ServiceInstance executionStep in ExecuterSteps[sessionID])
				{
					if (executionStep.State!=ServiceState.Ended )
					{
						if (progressState.CurrentRuningStepsState == null)
							progressState.CurrentRuningStepsState = new Dictionary<string, float>();
						progressState.CurrentRuningStepsState.Add(executionStep.Configuration.Name, executionStep.Progress);
						
					}
					else 
					{
						if (progressState.CurrentRuningStepsState == null)
							progressState.CurrentRuningStepsState = new Dictionary<string, float>();
						progressState.CurrentRuningStepsState.Add(executionStep.Configuration.Name, (float)1);
					}					
				}
				//Find overall progress
				float existingStepsProgress = 0;
				foreach (float progress in progressState.CurrentRuningStepsState.Values)
				{
					existingStepsProgress += progress;
					
				}
				progressState.OverAllProgess = existingStepsProgress / totalExecutionSteps;


				
			}
			catch (Exception ex)
			{

				throw new Exception("Problem when trying to locate the service by session");
			}
			return progressState;
			

		

			


		}
		#endregion
		#region Private methods
		/// <summary>
		/// this function run again and again until find the collectorcontainar and gets the first step of it
		/// </summary>
		/// <param name="executionStepElementCollection"></param>
		/// <returns></returns>
		private ExecutionStepElement GetFirstCollectorStep(ExecutionStepElementCollection executionStepElementCollection)
		{

			if (executionStepElementCollection != null && executionStepElementCollection.Count > 0)
			{
				foreach (ExecutionStepElement executionStep in executionStepElementCollection)
				{
					if (executionStep.Options["WizardRole"] == "CollectorContainer")
					{
						if (executionStep.ServiceToUse.Element.ExecutionSteps.Count > 0)
						{

							return executionStep.ServiceToUse.Element.ExecutionSteps[0];
						}
						else
							return null;
					}
					else
					{
						return GetFirstCollectorStep(executionStep.ServiceToUse.Element.ExecutionSteps);

					}
				}
				return null; // will never be reached
			}
			else
				return null;

		}
		private ServiceInstance GetMainParent(ServiceInstance serviceInstance)
		{
			while (serviceInstance.ParentInstance!=null)
			{
				serviceInstance = GetMainParent(serviceInstance.ParentInstance);
			}
			return serviceInstance;
		}

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
				lock (MainExecuters)
					MainExecuters[sessionID] = e.RequestedService;
			}
			if (e.RequestedService.ParentInstance.Configuration.Options["WizardRole"] == "ExecutorContainer")
			{
				int sessionID = int.Parse(e.RequestedService.ParentInstance.ParentInstance.Configuration.Options["SessionID"]);
				lock (ExecuterSteps)
				{
					if (ExecuterSteps.ContainsKey(sessionID))
						ExecuterSteps[sessionID].Add(e.RequestedService);
					else
					{
						List<ServiceInstance> listOfServiceInstances = new List<ServiceInstance>();
						listOfServiceInstances.Add(e.RequestedService);
						ExecuterSteps.Add(sessionID, listOfServiceInstances);			

					}

						
					

				}




			}


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
