using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EdgeBI.Wizards.AccountWizard
{
	class AddToSSISCollector : StepCollectorService
	{
		protected override Dictionary<string, string> Validate(Dictionary<string, object> inputValues)
		{
            if (inputValues.ContainsKey(ApplicationIDKey))
                SetAccountWizardSettingsByApllicationID(Convert.ToInt32(inputValues[ApplicationIDKey]));
			return null;
		}
		protected override void OnInit()
		{

			base.OnInit();
            this.StepName = /*Instance.Configuration.Options["StepNum"] + */ Instance.Configuration.Name;
		}
		protected override void Prepare()
		{
			StepDescription = "Add task to SSIS";
			ValidatedInput.Add(System_Field_Step_Description, StepDescription);
			base.Prepare();
		}
	}
}
