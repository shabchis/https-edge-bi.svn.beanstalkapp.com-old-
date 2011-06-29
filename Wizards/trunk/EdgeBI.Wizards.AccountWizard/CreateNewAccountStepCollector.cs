using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;
using Microsoft.AnalysisServices;
using Easynet.Edge.Core.Utilities;

namespace EdgeBI.Wizards.AccountWizard
{
    class CreateNewAccountStepCollector : StepCollectorService
    {
        protected override Dictionary<string, string> Validate(Dictionary<string, object> inputValues)
        {
            if (inputValues.ContainsKey(ApplicationIDKey))
                SetAccountWizardSettingsByApllicationID(Convert.ToInt32(inputValues[ApplicationIDKey]));
            Dictionary<string, string> errors = null;
            foreach (KeyValuePair<string, object> input in inputValues)
            {
                switch (input.Key)
                {
                    case "AccountSettings.ApplicationID":
                        {
                            if (input.Value.ToString() == "0")
                            {
                                if (errors == null)
                                    errors = new Dictionary<string, string>();
                                errors.Add(input.Key, string.Format("You must select application first", input.Key));
                                

                            }
                            break;

                        }
                    case "AccountSettings.BI_Scope_ID":
                    case "AccountSettings.BI_Scope_Name":
                   
                        {
                            if (input.Value == null || input.Value.ToString() == string.Empty)
                            {
                                if (errors == null)
                                    errors = new Dictionary<string, string>();
                                errors.Add(input.Key, string.Format("key {0} must be set",input.Key));

                            }
                            break;

                        }
                    case "AccountSettings.CreateAccount":
                        {
                            if (Convert.ToBoolean(input.Value) == false)
                            {
                                if (CheckScopeIDExsitence(inputValues["AccountSettings.BI_Scope_ID"]) == false)
                                {
                                    if (errors == null)
                                        errors = new Dictionary<string, string>();
                                    errors.Add(input.Key, string.Format("ScopeID: {0} Does not exist", inputValues["AccountSettings.BI_Scope_ID"]));
                                }

                            }
                            
                               
                           
                            break;
                        }
                    case "AccountSettings.UseExistingRole":
                        {
                            if (Convert.ToBoolean(input.Value) == true)
                            {
                                if (!IsRoleExist(inputValues["AccountSettings.RoleID"].ToString()))
                                {
                                    if (errors == null)
                                        errors = new Dictionary<string, string>();
                                    errors.Add(input.Key, string.Format("RoleID: {0} Does not exist", inputValues["AccountSettings.RoleID"]));
                                }
                            }
                            break;
                        }
                }
            }
            return errors;
        }
        private bool IsRoleExist(string RoleNameID)
        {
            bool exists = false;
            //Connect To analysisServer
            using (Server analysisServer = new Server())
            {


                try
                {

                    analysisServer.Connect(accountWizardSettings.Get("AnalysisServer.ConnectionString"));
                }
                catch (Exception ex)
                {
                    Log.Write("Unable to connect analysisServer", ex);
                    throw;
                }

                //Get the database
                Database analysisDatabase = analysisServer.Databases.GetByName(accountWizardSettings.Get("AnalysisServer.Database"));
                if (analysisDatabase.Roles.Contains(RoleNameID) || analysisDatabase.Roles.ContainsName(RoleNameID))
                {
                    exists = true;

                }

            }
            return exists;


        }

        private bool CheckScopeIDExsitence(object scopeID)
        {
            bool exists = false;
            using (SqlConnection sqlConnection = new SqlConnection(accountWizardSettings.Get("OLTP.Connection.string")))
            {
                sqlConnection.Open();
                using (SqlCommand sqlCommand=DataManager.CreateCommand("SELECT Count(Scope_ID) FROM User_GUI_Account WHERE Scope_ID=@scopeID:int"))
                {
                    sqlCommand.Parameters["@scopeID"].Value = Convert.ToInt32(scopeID);
                    sqlCommand.Connection = sqlConnection;
                    int rowCount =Convert.ToInt32( sqlCommand.ExecuteScalar());
                    if (rowCount>0)
                        exists=true;                    
                }
            }
            return exists;            
        }
        protected override void OnInit()
        {
            base.OnInit();
            this.StepName = /*Instance.Configuration.Options["StepNum"] + */Instance.Configuration.Name;
        }
        protected override void Prepare()
        {
            StepDescription = "Create New Account/Use current scope";
            ValidatedInput.Add(System_Field_Step_Description, StepDescription);
            base.Prepare();
        }
    }
}
