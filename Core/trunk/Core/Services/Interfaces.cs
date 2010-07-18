﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Utilities;

namespace Easynet.Edge.Core.Services
{
	/// <summary>
	/// Contract of server.
	/// </summary>
	[ServiceContract(
		SessionMode=SessionMode.Required,
		CallbackContract=typeof(IServiceSubscriber))]
	internal interface IServiceEngine: IDisposable
	{
		[OperationContract(IsOneWay=true)]
		void Abort();

		[OperationContract(IsOneWay=true)]
		void Run();

		[OperationContract(IsOneWay=true)]
		void Subscribe();

		[OperationContract(IsOneWay=true)]
		void Unsubscribe();

		[OperationContract(IsOneWay=true)]
		void ChildServiceOutcomeReported(int stepNumber, ServiceOutcome outcome);
	
		[OperationContract(IsOneWay=true)]
		void ChildServiceStateChanged(int stepNumber, ServiceState state);
	}

	/// <summary>
	/// Contract of client.
	/// </summary>
	internal interface IServiceSubscriber
	{
		[OperationContract(IsOneWay=true)]
		void StateChanged(ServiceState state);

		[OperationContract(IsOneWay=true)]
		void OutcomeReported(ServiceOutcome outcome);

		[OperationContract(IsOneWay=true)]
		void ChildServiceRequested(int stepNumber, int attemptNumber);
		
		[OperationContract(IsOneWay=true)]
		void ProgressReported(float progress);
	}

	public interface IServiceInstance
	{
		int AccountID { get; }
		long InstanceID { get; }
		string ServiceUrl { get; }
		ServicePriority Priority { get; }
		ServiceInstanceInfo ParentInstance { get; }
		ActiveServiceElement Configuration { get; }
		SchedulingRuleElement ActiveSchedulingRule { get; }
		DateTime TimeStarted { get; }
		DateTime TimeScheduled { get; }
	}
}
