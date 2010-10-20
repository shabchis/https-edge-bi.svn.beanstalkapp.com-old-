﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EdgeBI.Wizards.AccountWizard
{
	class CreateNewCubeCollector : StepCollectorService
	{
		protected override Dictionary<string, string> Validate(Dictionary<string, object> inputValues)
		{
			return null;
		}
		protected override void OnInit()
		{

			base.OnInit();
			this.StepName = Instance.Configuration.Name; 
		}
		protected override void Prepare()
		{
			StepDescription = "Create New Cube";
			ValidatedInput.Add(System_Field_Step_Description, StepDescription);
			base.Prepare();
		}
	}
}