using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easynet.Edge.Wizards.AccountWizard
{
	public class Step1Collector : StepCollectorService
	{
		

		#region Protected mehods
		protected override Dictionary<string, string> Validate(Dictionary<string, object> inputValues)
		{
			Dictionary<string, string> errors = new Dictionary<string, string>();
			foreach (KeyValuePair<string, object> input in inputValues)
			{
				switch (input.Key)
				{
					case "1111":
						{
							//errors.Add(input.Key, "Error test");
							break;
						}
					case "2222":
						{
							//errors.Add(input.Key, "Error test");
							break;
						}
					default:
						break;
				}

				
			}
			return errors;
		}
		protected override void OnInit()
		{
			base.OnInit();
			this.StepName = Instance.Configuration.Name; 
			
			this.StepDescription = "Do bla bla bla step1";
		}
		protected override void Prepare()
		{
			StepDescription = "bla bla bla step 1";
			ValidatedInput.Add(System_Field_Step_Description, StepDescription);
			base.Prepare();
		}
		 
		#endregion



	}
}
