using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Easynet.Edge.Wizards
{

	public struct WizardSession
	{
		public int WizardID;
		public int SessionID;

		public StepConfiguration CurrentStep;
	}

	public class StepConfiguration
	{
		public int Step;
		public Dictionary<string, object> MetaData;
	}

	public class StepCollectResponse
	{
		public StepResult Result;
		public StepConfiguration NextStep { get; set; } // Null if Result = HasErrors OR if there are no more steps
		public Dictionary<string, string> Errors;
	}

	public class StepCollectRequest
	{
		public int Step;
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