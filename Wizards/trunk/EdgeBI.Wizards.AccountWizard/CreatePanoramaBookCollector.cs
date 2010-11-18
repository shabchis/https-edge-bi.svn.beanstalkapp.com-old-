using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EdgeBI.Wizards.AccountWizard
{
    class CreatePanoramaBookCollector : StepCollectorService
    {
        protected override Dictionary<string, string> Validate(Dictionary<string, object> inputValues)
        {
            return null;

        }

        protected override void OnInit()
        {

            base.OnInit();
            this.StepName = /*Instance.Configuration.Options["StepNum"] + */ Instance.Configuration.Name;
        }
        protected override void Prepare()
        {
            StepDescription = "Create New Panorama Book";
            ValidatedInput.Add(System_Field_Step_Description, StepDescription);
            base.Prepare();
        }
    }
}
