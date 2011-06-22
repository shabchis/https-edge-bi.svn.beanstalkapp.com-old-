using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EdgeBI.Wizards.AccountWizard;
using Microsoft.AnalysisServices;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Utilities;
using System.DirectoryServices;

namespace EdgeBI.Wizards.AccountWizard
{
    class CreateRoleStepCollector : StepCollectorService
    {
        protected override Dictionary<string, string> Validate(Dictionary<string, object> inputValues)
        {
            if (inputValues.ContainsKey(ApplicationIDKey))
                SetAccountWizardSettingsByApllicationID(Convert.ToInt32(inputValues[ApplicationIDKey]));
            Dictionary<string, string> errors = null;
            if (!(bool)inputValues["AccountSettings.UseExistingRole"])
            {
                foreach (KeyValuePair<string, object> input in inputValues)
                {
                    try
                    {
                        switch (input.Key)
                        {
                            case "AccountSettings.RoleName":
                            case "AccountSettings.RoleID":
                                {
                                    if (!string.IsNullOrEmpty(input.Value.ToString()))
                                    {
                                        if (IsRoleExist(input.Value.ToString()))
                                        {
                                            if (errors == null)
                                                errors = new Dictionary<string, string>();
                                            errors.Add(input.Key, string.Format(@"Role with ID\Name: ""{0}"" already exists", input.Value));

                                        }
                                    }
                                    else
                                    {
                                        if (errors == null)
                                            errors = new Dictionary<string, string>();
                                        errors.Add(input.Key, @"Role ID\Name cannot be null or empty");
                                    }



                                    break;
                                }
                            case "AccountSettings.RoleMemberName":
                                {
                                    if (!string.IsNullOrEmpty(input.Value.ToString()))
                                    {
                                        //if (!IsExistActiveDirectoryUser(input.Value.ToString()))
                                        //{
                                        //    if (errors == null)
                                        //        errors = new Dictionary<string, string>();
                                        //    errors.Add(input.Key, string.Format(@"Role with ID\Name: ""{0}"" already exists"));
                                        //} 
                                    }
                                    else
                                    {
                                        if (errors == null)
                                            errors = new Dictionary<string, string>();
                                        errors.Add(input.Key, "Role can not be empty");

                                    }
                                    break;
                                }


                        }
                    }
                    catch (Exception ex)
                    {

                        errors.Add(input.Key, ex.Message);
                    }


                } 
            }
            return errors;
        }

        private bool IsExistActiveDirectoryUser(string ActiveDirectoryUserName)
        {
            string ldapPath = Encryptor.Decrypt(accountWizardSettings.Get("ActiveDirectoryStepExecutor.LDAP.Path"));
            string ldapUserName = Encryptor.Decrypt(accountWizardSettings.Get("ActiveDirectoryStepExecutor.LDAP.UserName"));
            string ldapPassword = Encryptor.Decrypt(accountWizardSettings.Get("ActiveDirectoryStepExecutor.LDAP.Passwrod"));


            DirectoryEntry obDirEntry = new DirectoryEntry(ldapPath, ldapUserName, ldapPassword);
            bool userFound = false;
            try
            {

                if (obDirEntry.Children.Find("CN=" + ActiveDirectoryUserName, "user") != null)
                    userFound = true;
            }
            catch (Exception)
            {
                userFound = false; //user not in the system
                //  MessageBox.Show("New User error: " + ex.ToString());
            }
            return userFound;
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
    }
}
