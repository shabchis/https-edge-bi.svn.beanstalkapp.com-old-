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
            if (inputValues.ContainsKey(ApplicationIDKey))
                SetAccountWizardSettingsByApllicationID(Convert.ToInt32(inputValues[ApplicationIDKey]));
			Dictionary<string, string> errors=null; 
            foreach (KeyValuePair<string, object> input in inputValues)
            {
                switch (input.Key)
                {
                    case "ActiveDirectory.UserName":
                        {
                            //Just example
                            if (input.Value.ToString().Length>20)
                            {
                                if (errors == null)
                                    errors = new Dictionary<string, string>();
                                
                                errors.Add(input.Key, "UserName is too long must be less then 20 char");
								
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
			this.StepName =/*Instance.Configuration.Options["StepNum"] + */ Instance.Configuration.Name; 
			
			
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
