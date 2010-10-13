using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EdgeBI.Wizards.AccountWizard
{
	public class ActiveDirectoryStepCollector : StepCollectorService
	{
		

		#region Protected mehods
		protected override Dictionary<string, string> Validate(Dictionary<string, object> inputValues)
		{
			Dictionary<string, string> errors = new Dictionary<string, string>();
			foreach (KeyValuePair<string, object> input in inputValues)
			{
				switch (input.Key)
				{
					case "UserName":
						{
							//Just example
							if (input.Value.ToString().Length>10)
							{
								errors.Add(input.Key, "UserName is too long");
								
							}
							break;
						}
					case "Password":
						{
							//errors.Add(input.Key, "Error test");
							break;
						}
					case "FullName":
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
			this.StepName = Instance.Configuration.Name; 
			
			this.StepDescription = "Do bla bla bla step1";
		}
		protected override void Prepare()
		{
			StepDescription = "Create User on active directory";
			ValidatedInput.Add(System_Field_Step_Description, StepDescription);
			base.Prepare();
		}
		 
		#endregion



	}
}
