using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;


namespace Easynet.Edge.Wizards
{
	/// <summary>
	/// New wizard parmeters, return when runing the start function
	/// </summary>
	[DataContract(Name="WizardSession",Namespace="Easynet.Edge.Wizards")]
	public struct WizardSession
	{
		[DataMember]
		public int WizardID;
		[DataMember]
		public int SessionID;
		[DataMember]
		public StepConfiguration CurrentStep;
	}
	/// <summary>
	/// Describe the current step and optionaly the next step fields
	/// </summary>
	public class StepConfiguration
	{
		public string StepName;
		public Dictionary<string, object> MetaData;
	}

	/// <summary>
	/// The response returned when runing the collect function
	/// </summary>
	public class StepCollectResponse
	{
		public StepResult Result;
		public StepConfiguration NextStep { get; set; } // Null if Result = HasErrors OR if there are no more steps
		public Dictionary<string, string> Errors;
	}
	/// <summary>
	/// Request the passed as paramter on the collect function
	/// </summary>
	public class StepCollectRequest
	{
		public string StepName;
		public Dictionary<string, object> CollectedValues;
	}

	/// <summary>
	/// Result of specific request or respond
	/// </summary>
	public enum StepResult
	{
		Next,
		Done,
		HasErrors
	}
}