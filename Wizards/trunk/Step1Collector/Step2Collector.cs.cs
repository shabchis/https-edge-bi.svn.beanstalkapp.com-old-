using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easynet.Edge.Wizards.AccountWizard
{
	public class Step2Collector : StepCollectorService
	{

		#region Protected Methods
		protected override Dictionary<string, string> Validate(Dictionary<string, object> inputValues)
		{
			return null;
		}
		protected override void Prepare()
		{
			Console.WriteLine("test with no errors next step");
		}
		protected override void OnInit()
		{
			base.OnInit();
			_step = 2;
		}
		#endregion
	}
}
