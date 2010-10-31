using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EdgeBI.Wizards.AccountWizard;

namespace EdgeBI.Wizards.AccountWizard
{
	class CreateRoleStepCollector : StepCollectorService
	{
		protected override Dictionary<string, string> Validate(Dictionary<string, object> inputValues)
		{
			Dictionary<string, string> errors=null;
			foreach (KeyValuePair<string, object> input in inputValues)
			{
				switch (input.Key)
				{
					case "RoleName":
						{
							break;
						}
					case "RoleID":
						{
							break;
						}
					case "RoleMemberName":
						{
							break;
						}
					
						
				}


			}
			return errors;
		}
		protected override void OnInit()
		{
			base.OnInit();
            this.StepName = /*Instance.Configuration.Options["StepNum"] + */Instance.Configuration.Name; 
		}
		protected override void Prepare()
		{
			StepDescription = "Create New Role";
			ValidatedInput.Add(System_Field_Step_Description, StepDescription);
			base.Prepare();
		}
	}
}
