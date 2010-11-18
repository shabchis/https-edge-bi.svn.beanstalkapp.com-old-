using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Utilities;
using Microsoft.AnalysisServices;
using Easynet.Edge.Core.Configuration;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;

namespace EdgeBI.Wizards.AccountWizard
{
    class CreateNewCubeExecutor : StepExecuter
    {

        private const string C_AccSettClientSpecific = "AccountSettings.Client Specific";
        private const string C_AccSettNewUser = "AccountSettings.New Users";
        private const string C_AccSettNewActiveUser = "AccountSettings.New Active Users";
        private const string C_AccSettStringReplacemnet = "AccountSettings.StringReplacment.";


        protected override Easynet.Edge.Core.Services.ServiceOutcome DoWork()
        {

            Log.Write("Getting collected data from suitable collector", LogMessageType.Information);

            Dictionary<string, object> collectedData = this.GetStepCollectedData(Instance.Configuration.Options["CollectorStep"]);
            this.ReportProgress(0.1f);

            Log.Write("Creating Cube/s on analysis server", LogMessageType.Information);
            CreateCube(collectedData);
            this.ReportProgress(0.7f);
            Log.Write("Cube Created", LogMessageType.Information);

            Log.Write("Update OLTP datbase", LogMessageType.Information);
            UpdateOltpDataBASE(collectedData);
            this.ReportProgress(1);

            return base.DoWork();
        }
        private void CreateCube(Dictionary<string, object> collectedData)
        {
            //Connect To analysisServer
            using (Server analysisServer = new Server())
            {
                Dictionary<string, object> executorData = new Dictionary<string, object>();  //dictionary for save executors data for next steps

                analysisServer.Connect(AppSettings.Get(this, "AnalysisServer.ConnectionString"));

                //Get the database
                Database analysisDatabase = analysisServer.Databases.GetByName(AppSettings.Get(this, "AnalysisServer.Database"));

                #region CreateBoCube
                //Create bo cube
                Cube boCubeTemplate = analysisDatabase.Cubes[AppSettings.Get(this, "Cube.Templates.BOTemplate")];


                Cube newBOCube = boCubeTemplate.Clone();



                ////change cube name and id
                newBOCube.Name = AppSettings.GetAbsolute("EdgeBI.Wizards.StepExecuter.Cube.BO.Name.Perfix") + collectedData["AccountSettings.CubeName"].ToString();


                executorData.Add("AccountSettings.CubeName", collectedData["AccountSettings.CubeName"].ToString()); //for next step
                newBOCube.ID = AppSettings.GetAbsolute("EdgeBI.Wizards.StepExecuter.Cube.BO.Name.Perfix") + collectedData["AccountSettings.CubeID"].ToString();

                //change client specific measures
                foreach (MeasureGroup measureGroup in newBOCube.MeasureGroups)
                {

                    foreach (KeyValuePair<string, object> input in collectedData)
                    {
                        //NEW USERS - 
                        if (input.Value is Replacment)
                        {
                            Replacment replacement = (Replacment)input.Value;

                            if (input.Key.ToUpper() == C_AccSettNewUser.ToUpper())
                            {



                                Measure measure = measureGroup.Measures.Find(replacement.ReplaceFrom);
                                if (measure != null)
                                {                                                                     //perfix of new useres /regs
                                    if (!replacement.CalcMembersOnly)
                                    {
                                        if (measureGroup.Measures.FindByName(replacement.ReplaceTo) == null) //check if their is no measure with the same name
                                        {
                                            measure.Name = replacement.ReplaceTo;
                                        }
                                    }


                                }


                            }
                            //NEW ACTIVE USERS
                            else if (input.Key.ToUpper() == C_AccSettNewActiveUser.ToUpper())
                            {

                                Measure measure = measureGroup.Measures.Find(replacement.ReplaceFrom);
                                if (measure != null)
                                {                                                                  //key of new active useres /actives
                                    if (!replacement.CalcMembersOnly)
                                    {
                                        if (measureGroup.Measures.FindByName(replacement.ReplaceTo) == null) //check if their is no measure with the same name
                                        {
                                            measure.Name = replacement.ReplaceTo;
                                        }
                                    }


                                }

                            }
                            //MEASURES CLIENT SPECIFIC
                            else if (input.Key.StartsWith(C_AccSettClientSpecific, true, null))
                            {

                                if (!replacement.CalcMembersOnly)
                                {
                                    Measure measure = measureGroup.Measures.Find(replacement.ReplaceFrom);
                                    if (measure != null)
                                    {
                                        if (measureGroup.Measures.FindByName(replacement.ReplaceTo) == null) //check if their is no measure with the same name
                                        {
                                            measure.Name = replacement.ReplaceTo;
                                        }
                                    }
                                }

                            }
                            else if (input.Key.StartsWith(C_AccSettStringReplacemnet))// string replacment (measure -google values)
                            {
                                if (!replacement.CalcMembersOnly)
                                {

                                    Measure measure = measureGroup.Measures.Find(replacement.ReplaceFrom);
                                    if (measure != null)
                                    {
                                        if (measureGroup.Measures.FindByName(replacement.ReplaceTo) == null) //check if their is no measure with the same name
                                        {
                                            measure.Name = replacement.ReplaceTo;
                                        }
                                    }
                                }

                            }
                            //Copy All Data to the next step Panorama cube
                            if (!executorData.ContainsKey(input.Key))
                            {
                                executorData.Add(input.Key, input.Value);
                            }

                        }

                    }

                    //Change scope_id
                    foreach (Partition partition in measureGroup.Partitions)
                    {
                        QueryBinding queryBinding = partition.Source as QueryBinding;

                        if (queryBinding != null)
                        {
                            //since the "scopee_id" must be on the end of the query according to amit
                            //get last index of scope_id
                            int indexScope_id = queryBinding.QueryDefinition.LastIndexOf("scope_id", StringComparison.OrdinalIgnoreCase);
                            int spacesUntillEndOfQuery = queryBinding.QueryDefinition.Length - indexScope_id;
                            //remove all scope_id
                            queryBinding.QueryDefinition = queryBinding.QueryDefinition.Remove(indexScope_id, spacesUntillEndOfQuery);
                            //insert new scope_id
                            queryBinding.QueryDefinition = queryBinding.QueryDefinition.Insert(queryBinding.QueryDefinition.Length, string.Format(" Scope_ID={0}", collectedData["AccountSettings.Scope_ID"].ToString()));
                            // 
                        }
                    }


                }

                //change client specific measures/STRING REPLACEMENT/ in calculated members

                foreach (MdxScript script in newBOCube.MdxScripts)
                {
                    foreach (Command command in script.Commands)
                    {
                        CalculatedMembersCollection CalculatedMembersCollection = new CalculatedMembersCollection(command.Text);
                        foreach (KeyValuePair<string, object> input in collectedData)
                        {
                            if (input.Value is Replacment)
                            {
                                Replacment replacment = (Replacment)input.Value;
                                if (input.Key.StartsWith(C_AccSettClientSpecific, true, null))
                                {
                                    //Client specifixc x (measureres but this time by name + bo)
                                    CalculatedMembersCollection.ReplaceCalculatedMembersStrings("BO " + replacment.ReplaceFrom, replacment.ReplaceTo);

                                }
                                else if (input.Key.ToUpper() == C_AccSettNewUser.ToUpper()) //cpa 1
                                {

                                    CalculatedMembersCollection.ReplaceCalculatedMembersStrings("Regs", replacment.ReplaceTo);

                                }
                                else if (input.Key.ToUpper() == C_AccSettNewActiveUser.ToUpper()) //cpa 2
                                {

                                    CalculatedMembersCollection.ReplaceCalculatedMembersStrings("Actives", replacment.ReplaceTo);

                                }
                                else if (input.Key.StartsWith(C_AccSettStringReplacemnet)) //string replacement
                                {
                                    CalculatedMembersCollection.ReplaceCalculatedMembersStrings(replacment.ReplaceFrom, replacment.ReplaceTo);

                                }
                            }
                        }
                        command.Text = CalculatedMembersCollection.GetText();

                    }
                }


                //Add last step created role (for this test some existing  role role 8 which not exist in the template) 

                //Get the roleID from last step collector
                Dictionary<string, object> roleData = GetStepCollectedData("CreateRoleStepCollector");

                CubePermission cubePermission = new CubePermission(roleData["AccountSettings.RoleID"].ToString(), AppSettings.Get(this, "Cube.CubePermission.ID"), AppSettings.Get(this, "Cube.CubePermission.Name"));
                cubePermission.Read = ReadAccess.Allowed;
                newBOCube.CubePermissions.Add(cubePermission);




                ///Add the new BO cube
                try
                {

                    int result = analysisDatabase.Cubes.Add(newBOCube);
                    newBOCube.Update(UpdateOptions.ExpandFull, UpdateMode.Create);
                }
                catch (Exception ex)
                {

                    Log.Write("Error adding BO cube", ex);
                }
                #endregion
                #region ContentCube
                //check if content cube should be add too-------------------------------------
                //---------------------------------------------------------------------------
                executorData.Add("AccountSettings.AddContentCube", collectedData["AccountSettings.AddContentCube"]);
                executorData.Add("AccountSettings.ProcessCubes", collectedData["AccountSettings.ProcessCubes"]);
                if (bool.Parse(collectedData["AccountSettings.AddContentCube"].ToString()) == true)
                {
                    //Add Content Cube
                    //Create bo cube
                    Cube ContentCubeTemplate = analysisDatabase.Cubes[AppSettings.Get(this, "Cube.Templates.ContentTemplate")];


                    Cube newContentCube = ContentCubeTemplate.Clone();



                    ////change cube name and id
                    newContentCube.Name = AppSettings.GetAbsolute("EdgeBI.Wizards.StepExecuter.Cube.Content.Name.Perfix") + collectedData["AccountSettings.CubeName"].ToString();
                    newContentCube.ID = AppSettings.GetAbsolute("EdgeBI.Wizards.StepExecuter.Cube.Content.Name.Perfix") + collectedData["AccountSettings.CubeID"].ToString();
                    //Get google values for replacments;
                    string[] googleConversions = AppSettings.GetAbsolute("EdgeBI.Wizards.GoogleConversions").Split(',');

                    foreach (MeasureGroup measureGroup in newContentCube.MeasureGroups)
                    {
                        //Replace Google Value
                        foreach (KeyValuePair<string, object> input in collectedData)
                        {
                            if (input.Value is Replacment)
                            {
                                Replacment replacement = (Replacment)input.Value;
                                if (!replacement.CalcMembersOnly)
                                {
                                    foreach (string googleConversion in googleConversions)
                                    {
                                        if (replacement.ReplaceFrom == googleConversion)
                                        {
                                            //Chek with amit about calcmembers only
                                            Measure measure = measureGroup.Measures.Find(replacement.ReplaceFrom);
                                            if (measure != null)
                                            {
                                                if (measureGroup.Measures.FindByName(replacement.ReplaceTo) == null) //check if their is no measure with the same name
                                                {
                                                    measure.Name = AppSettings.GetAbsolute("EdgeBI.Wizards.StepExecuter.Cube.Content.Name.Perfix") + " " + replacement.ReplaceTo;
                                                }
                                            }

                                        }
                                    } 
                                }

                            }

                        }
                        //Replace the scope_id
                        foreach (Partition partition in measureGroup.Partitions)
                        {
                            QueryBinding queryBinding = partition.Source as QueryBinding;

                            if (queryBinding != null)
                            {
                                //since the "scopee_id" must be on the end of the query according to amit
                                //get last index of scope_id
                                int indexScope_id = queryBinding.QueryDefinition.LastIndexOf("scope_id", StringComparison.OrdinalIgnoreCase);
                                int spacesUntillEndOfQuery = queryBinding.QueryDefinition.Length - indexScope_id;
                                //remove all scope_id
                                queryBinding.QueryDefinition = queryBinding.QueryDefinition.Remove(indexScope_id, spacesUntillEndOfQuery);
                                //insert new scope_id
                                queryBinding.QueryDefinition = queryBinding.QueryDefinition.Insert(queryBinding.QueryDefinition.Length, string.Format(" Scope_ID={0}", collectedData["AccountSettings.Scope_ID"].ToString()));

                            }
                        }
                    }
                    //Replace google conversion on calculated members
                    foreach (MdxScript script in newBOCube.MdxScripts)
                    {

                        foreach (Command command in script.Commands)
                        {
                            CalculatedMembersCollection CalculatedMembersCollection = new CalculatedMembersCollection(command.Text);
                            foreach (KeyValuePair<string, object> input in collectedData)
                            {
                                if (input.Value is Replacment)
                                {
                                    Replacment replacment = (Replacment)input.Value;
                                    foreach (string googleConversion in googleConversions)
                                    {
                                        if (replacment.ReplaceFrom == googleConversion)
                                        {
                                            CalculatedMembersCollection.ReplaceCalculatedMembersStrings(replacment.ReplaceFrom, replacment.ReplaceTo);
                                        }
                                    }

                                }
                            }
                            command.Text = CalculatedMembersCollection.GetText();
                        }


                    }

                    //add last step role (cube permission
                    cubePermission = new CubePermission(roleData["AccountSettings.RoleID"].ToString(), AppSettings.Get(this, "Cube.CubePermission.ID"), AppSettings.Get(this, "Cube.CubePermission.Name"));
                    cubePermission.Read = ReadAccess.Allowed;
                    newContentCube.CubePermissions.Add(cubePermission);


                    ///Add the new Content cube
                    try
                    {

                        int result = analysisDatabase.Cubes.Add(newContentCube);
                        newContentCube.Update(UpdateOptions.ExpandFull, UpdateMode.Create);
                    }
                    catch (Exception ex)
                    {

                        Log.Write("Error adding BO cube", ex);
                    }





                }
                #endregion
                SaveExecutorData(executorData);
            }
        }
        private void UpdateOltpDataBASE(Dictionary<string, object> collectedData)
        {
            using (SqlConnection sqlConnection = new SqlConnection(AppSettings.Get(this, "OLTP.Connection.string")))
            {
                sqlConnection.Open();
                foreach (KeyValuePair<string, object> input in collectedData)
                {
                    if (input.Key.StartsWith("AccountSettings"))
                    {


                        using (SqlCommand sqlCommand = DataManager.CreateCommand(@"INSERT INTO User_Gui_AccountSettings
																			(ScopeID,AccountID,Name,Value,sys_creation_date)
																			Values
																			(@ScopeID:Int,
																			 @AccountID:NVarchar,
																			 @Name:NVarchar,
																			 @Value:NVarchar,
																			 @sys_creation_date:DateTime)"))
                        {
                            sqlCommand.Connection = sqlConnection;
                            sqlCommand.Parameters["@ScopeID"].Value = 3861; //TODO: TEMPORARLY WILL COME FROM OTHER COLLECTOR (GENERAL COLLECTOR-ASK DORON)
                            sqlCommand.Parameters["@AccountID"].Value = DBNull.Value; //TODO: CHECK THIS FOR NOW IT'S NULL
                            sqlCommand.Parameters["@Name"].Value = input.Key;
                            if (input.Value is Replacment)
                            {
                                Replacment replacement = (Replacment)input.Value;
                                sqlCommand.Parameters["@Value"].Value = replacement.ReplaceTo;

                            }
                            else
                            {
                                sqlCommand.Parameters["@Value"].Value = input.Value;

                            }
                            sqlCommand.Parameters["@sys_creation_date"].Value = DateTime.Now;

                            sqlCommand.ExecuteNonQuery();

                        }
                    }


                }
            }
        }



    }
}
