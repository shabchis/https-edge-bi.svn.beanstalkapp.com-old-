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
			
			

			Dictionary<string, object> collectedData = this.GetStepCollectedData(Instance.Configuration.Options["CollectorStep"]);
			Console.WriteLine("Test First executer, Get Field and values of current step");
			foreach (KeyValuePair<string, object> item in collectedData)
			{
				Console.WriteLine("Field: {0} | Value: {1}", item.Key, item.Value);
			}
			Console.WriteLine("Test saving executor datat");
			SaveExecutorData(new Dictionary<string, object>() {
				{ "Executor1111","Executor1111"},
				{ "Executor2222","Executor3333"}});
			Console.WriteLine("Test Get executor data");
			Dictionary<string,object> executorData= GetExecutorData("Step1Executor");
			foreach (KeyValuePair<string, object> item in executorData)
			{
				Console.WriteLine("Field: {0} | Value: {1}", item.Key, item.Value);
			}
			return base.DoWork();
		}
		
		
		
	}
}
