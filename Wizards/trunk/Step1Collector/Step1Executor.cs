using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easynet.Edge.Wizards.AccountWizard
{
	class Step1Executor : StepExecuter
	{
		protected override Core.Services.ServiceOutcome DoWork()
		{
			Console.WriteLine("Update Current Step Name");
			UpdateCurrentStepNameAndStatus("Step1Executer");
			
			Dictionary<string, object> test = this.GetStepCollectedData(Instance.Configuration.Options["CollectorStep"], SessionID);
			Console.WriteLine("Test First executer, Get Field and values of current step");
			foreach (KeyValuePair<string,object> item in test)
			{
				Console.WriteLine("Field: {0} | Value: {1}", item.Key, item.Value);
			}
			UpdateCurrentStepNameAndStatus("Wizard Finished!");

			return base.DoWork();
		}
		
		
		
	}
}
