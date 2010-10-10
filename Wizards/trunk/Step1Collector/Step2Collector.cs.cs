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

			StepDescription = "bla bla bla step 2";
			ValidatedInput.Add(System_Field_Step_Description, StepDescription);
			base.Prepare();

		}
		protected override void OnInit()
		{
			base.OnInit();
			this.StepName = Instance.Configuration.Name;
			this.StepDescription = "Do bla bla bla step2";
		}
		#endregion
	}
}
