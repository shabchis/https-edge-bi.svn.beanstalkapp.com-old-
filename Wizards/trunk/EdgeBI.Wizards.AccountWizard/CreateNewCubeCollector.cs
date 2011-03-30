using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AnalysisServices;
using Easynet.Edge.Core.Configuration;
using System.IO;
using Easynet.Edge.Core.Utilities;

namespace EdgeBI.Wizards.AccountWizard
{
    class CreateNewCubeCollector : StepCollectorService
    {
        protected override Dictionary<string, string> Validate(Dictionary<string, object> inputValues)
        {

            if (inputValues.ContainsKey(ApplicationIDKey))
                SetAccountWizardSettingsByApllicationID(Convert.ToInt32(inputValues[ApplicationIDKey]));
            Dictionary<string, string> errors = null;
            Dictionary<string, string> measures = new Dictionary<string, string>();
            Dictionary<string, Replacment> allReplaces = new Dictionary<string, Replacment>();
            string pattern;

            foreach (KeyValuePair<string, object> input in inputValues)
            {
                try
                {
                    switch (input.Key)
                    {
                        case "AccountSettings.CubeName": //same as cube id
                            {

                                pattern = @"\W";
                                if (Regex.IsMatch(input.Value.ToString(), pattern))
                                {
                                    if (errors == null)
                                        errors = new Dictionary<string, string>();
                                    errors.Add(input.Key, string.Format(@"The ScopeName\CubeName\CubeID: ""{0}"" contains non alphnumeric charechter", input.Value.ToString()));


                                }


                                break;
                            }
                        case "AccountSettings.CreateMeasure":
                            {
                                if (Convert.ToBoolean(input.Value) == true)
                                {
                                    if (!inputValues.ContainsKey("AccountSettings.MeasureAccountID") || inputValues["AccountSettings.MeasureAccountID"] == null || inputValues["AccountSettings.MeasureAccountID"].ToString() == string.Empty)
                                    {
                                        if (errors == null)
                                            errors = new Dictionary<string, string>();
                                        errors.Add(input.Key, "If Create measure is checked then measure account ID must be supplied!");

                                    }

                                }
                                break;
                            }
                        case "AccountSettings.BI_Scope_ID":
                            {

                                int i = 0;
                                if (!int.TryParse(input.Value.ToString(), out i))
                                {
                                    if (errors == null)
                                        errors = new Dictionary<string, string>();
                                    errors.Add(input.Key, string.Format(@"Scope Id must be int", input.Value.ToString()));
                                }
                                break;
                            }
                        case "AccountSettings.AddContentCube":
                        case "AccountSettings.ProcessCubes":
                        case "AccountSettings.CubeID":
                        case "AccountSettings.BI_Scope_Name":
                        case "AccountSettings.MeasureAccountID":                       
                        case ApplicationIDKey:
                            {

                                break;
                            }
                        case "AccountSettings.TargetValue1":
                        case "AccountSettings.TargetValue2":
                            {
                                double temp;
                                if (!inputValues.ContainsKey("AccountSettings.MeasureAccountID") || string.IsNullOrEmpty( inputValues["AccountSettings.MeasureAccountID"].ToString()))
                                {
                                    if (errors == null)
                                        errors = new Dictionary<string, string>();
                                    errors.Add(input.Key, "if you typed target cpa you must type an account ID");
                                }
                                if (!double.TryParse(input.Value.ToString(), out temp))
                                {
                                    if (!string.IsNullOrEmpty(input.Value.ToString()))
                                    {
                                        if (errors == null)
                                            errors = new Dictionary<string, string>();
                                        errors.Add(input.Key, "AccountSettings.TargetValue1 CPA MUST BE NUMERIC!");
                                    }
                                }

                                break;
                            }
                        default: //all replacements
                            {

                                Replacment replacment = (Replacment)input.Value;

                                if (replacment.CalcMembersOnly == false)
                                {
                                    if (measures.ContainsKey(replacment.ReplaceTo))
                                    {
                                        if (errors == null)
                                            errors = new Dictionary<string, string>();
                                        errors.Add(input.Key, string.Format("You are trying to change two measure name to the same name:{0}", replacment.ReplaceTo));
                                    }
                                    else
                                        measures.Add(replacment.ReplaceTo, replacment.ReplaceTo);




                                    if (IsMeasureNameExists(replacment.ReplaceTo))
                                    {
                                        if (errors == null)
                                            errors = new Dictionary<string, string>();
                                        errors.Add(input.Key, string.Format(" Measure Name or calc name: {0} already exists in the cube", replacment.ReplaceTo));
                                    }
                                }
                               



                                allReplaces.Add(replacment.ReplaceFrom, replacment);

                                break;
                            }

                    }
                }
                catch (Exception ex)
                {

                    if (errors == null)
                        errors = new Dictionary<string, string>();
                    errors.Add(input.Key, ex.Message);
                }




            }

            foreach (string key in allReplaces.Keys)
            {

                string replaceValue = allReplaces[key].ReplaceTo;
                int sameValueCounted = 0;
                foreach (Replacment val in allReplaces.Values)
                {

                    if (key == val.ReplaceTo) //trying to changed value to a changed value
                    {
                        if (errors == null)
                            errors = new Dictionary<string, string>();
                        errors.Add(key, string.Format("You have already changed the key {0} to {1} , you can not used it again.", key, replaceValue));

                    }
                    if (replaceValue == val.ReplaceTo)
                    {

                        if (val.CalcMembersOnly == false)
                        {
                            sameValueCounted += 1;
                        }

                    }

                }
                if (sameValueCounted > 1)
                {
                    if (errors == null)
                        errors = new Dictionary<string, string>();
                    errors.Add(key, string.Format("Yo can't change two object to the same name , value: {0}", key, replaceValue));

                }

            }
            //Check SSIS PATHS
            if (!File.Exists(accountWizardSettings.Get("SSIS.TemplateAllBoPackagePath")))
            {

                if (errors == null)
                    errors = new Dictionary<string, string>();
                errors.Add("SSIS.TemplateAllBoPackagePath", "Path not exists");
            }
            if (!File.Exists(accountWizardSettings.Get("SSIS.TemplateAllContentPackagePath")))
            {
                if (errors == null)
                    errors = new Dictionary<string, string>();
                errors.Add("SSIS.TemplateAllContentPackagePath", "Path not exists");
            }
            if (!File.Exists(accountWizardSettings.Get("SSIS.TemplateBoSpecific")))
            {
                if (errors == null)
                    errors = new Dictionary<string, string>();
                errors.Add("SSIS.TemplateBoSpecific", "Path not exists");
            }
            if (!Directory.Exists(accountWizardSettings.Get("SSIS.SSISNewTaskPath")))
            {
                if (errors == null)
                    errors = new Dictionary<string, string>();
                errors.Add("SSIS.AllBoPackageBackupPath", "Path not exists");
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
            StepDescription = "Create New Cube";
            ValidatedInput.Add(System_Field_Step_Description, StepDescription);
            base.Prepare();
        }
        private bool IsMeasureNameExists(string replaceTo)
        {
            bool exists = false;
            using (Server analysisServer = new Server())
            {
                analysisServer.Connect(accountWizardSettings.Get("AnalysisServer.ConnectionString"));
                Database analysisDatabase = analysisServer.Databases.GetByName(accountWizardSettings.Get("AnalysisServer.Database"));

                Cube boCubeTemplate = analysisDatabase.Cubes[accountWizardSettings.Get("Cube.Templates.BOTemplate")];
                foreach (MeasureGroup measureGroup in boCubeTemplate.MeasureGroups)
                {
                    Measure measure = measureGroup.Measures.FindByName(replaceTo);
                    if (measure != null)
                        exists = true;

                }
            }
            return exists;

        }
    }
}
