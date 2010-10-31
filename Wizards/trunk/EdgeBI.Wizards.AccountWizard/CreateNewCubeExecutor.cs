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
		
		private const string AccSettClientSpecific = "AccountSettings.Client Specific";
		private const string AccSettNewUser = "AccountSettings.New Users";
		private const string AccSettNewActiveUser = "AccountSettings.New Active Users";
		

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
			this.ReportProgress(0.9f);

			return base.DoWork();
		}
		private void CreateCube(Dictionary<string, object> collectedData)
		{
			//Connect To analysisServer
			using (Server analysisServer = new Server())
			{
				Dictionary<string,object> executorData=new Dictionary<string,object>();  //dictionary for save executors data for next steps

				analysisServer.Connect(AppSettings.Get(this, "AnalysisServer.ConnectionString"));

				//Get the database
				Database analysisDatabase = analysisServer.Databases.GetByName(AppSettings.Get(this, "AnalysisServer.Database"));

				//Create bo cube
				Cube boCubeTemplate = analysisDatabase.Cubes[AppSettings.Get(this, "Cube.Templates.BOTemplate")];


				Cube newBOCube = boCubeTemplate.Clone();



				////change cube name and id
				newBOCube.Name = string.Format("BO{0}", collectedData["AccountSettings.CubeName"].ToString());


				executorData.Add("AccountSettings.CubeName", newBOCube.Name); //for next step
				newBOCube.ID = string.Format("BO{0}", collectedData["AccountSettings.CubeID"].ToString());

				//change client specific measures
				foreach (MeasureGroup measureGroup in newBOCube.MeasureGroups)
				{

					foreach (KeyValuePair<string, object> input in collectedData)
					{
                        //NEW USERS
                        try
                        {
                            if (input.Key.ToUpper() == AccSettNewUser.ToUpper())
                            {
                                if (!executorData.ContainsKey(input.Key))
                                {
                                    executorData.Add(input.Key, input.Value);
                                }
                                Measure measure = measureGroup.Measures.Find(input.Key.Replace("AccountSettings.", string.Empty));
                                if (measure != null)
                                {
                                    if (input.Value.ToString() != " " && bool.Parse(collectedData["AccountSettings.Cpa1.OnlyCalC"].ToString()) == false)
                                    {
                                        if (measureGroup.Measures.FindByName(input.Value.ToString()) == null) //check if their is no measure with the same name
                                        {
                                            measure.Name = input.Value.ToString();
                                        }
                                    }


                                }

                            }
                            //NEW ACTIVE USERS
                            else if (input.Key.ToUpper() == AccSettNewActiveUser.ToUpper())
                            {
                                if (!executorData.ContainsKey(input.Key))
                                {
                                    executorData.Add(input.Key, input.Value);
                                }
                                Measure measure = measureGroup.Measures.Find(input.Key.Replace("AccountSettings.", string.Empty));
                                if (measure != null)
                                {
                                    if (input.Value.ToString() != " " && bool.Parse(collectedData["AccountSettings.Cpa2.OnlyCalC"].ToString()) == false)
                                    {
                                        if (measureGroup.Measures.FindByName(input.Value.ToString()) == null) //check if their is no measure with the same name
                                        {
                                            measure.Name = input.Value.ToString();
                                        }
                                    }


                                }

                            }
                            //MEASURES CLIENT SPECIFIC
                            else if (input.Key.StartsWith(AccSettClientSpecific, true, null))
                            {
                                if (!executorData.ContainsKey(input.Key))
                                {
                                    executorData.Add(input.Key, input.Value);
                                }
                               
                                Measure measure = measureGroup.Measures.Find(input.Key.Replace("AccountSettings.", string.Empty));
                                if (measure != null)
                                {
                                    if (measureGroup.Measures.FindByName(input.Value.ToString()) == null) //check if their is no measure with the same name
                                    {
                                        measure.Name = input.Value.ToString();
                                    }
                                }

                            }
                        }
                        catch (Exception EX)
                        {
                            
                            throw;
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
						foreach (KeyValuePair<string, object> input in collectedData)
						{
							if (input.Key.StartsWith(AccSettClientSpecific,true,null ))
							{			//TODO: CHECK THIS AGAIN					
								//command.Text = Regex.Replace(command.Text, input.Key.Replace("AccountSettings.", string.Empty), input.Value.ToString(), RegexOptions.IgnoreCase);
								string patern = string.Format(@"\b{0}\b","BO " + input.Key.Replace("AccountSettings.", string.Empty));
								command.Text = Regex.Replace(command.Text, patern, input.Value.ToString(), RegexOptions.IgnoreCase);
							}
                            else if (input.Key.ToUpper() == AccSettNewUser.ToUpper()) //cpa 1
                            {
                                string patern = string.Format(@"\b{0}\b", "Regs");
                                command.Text = Regex.Replace(command.Text, patern, input.Value.ToString(), RegexOptions.IgnoreCase);

                            }
                            else if (input.Key.ToUpper() == AccSettNewActiveUser.ToUpper())
                            {
                                string patern = string.Format(@"\b{0}\b", "Actives");
                                command.Text = Regex.Replace(command.Text, patern, input.Value.ToString(), RegexOptions.IgnoreCase);

                            }
                            else if(input.Key.StartsWith("AccountSettings.StringReplacment."))
                            {
                                string patern = string.Format(@"\b{0}\b", input.Key.Replace("AccountSettings.StringReplacment.",string.Empty));
                                command.Text = Regex.Replace(command.Text, patern, input.Value.ToString(), RegexOptions.IgnoreCase);

                            }
						}

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

				//check if content cube should be add too-------------------------------------
				//---------------------------------------------------------------------------

				if (bool.Parse(collectedData["AccountSettings.AddContentCube"].ToString()) == true)
				{
					//Add Content Cube
					//Create bo cube
					Cube ContentCubeTemplate = analysisDatabase.Cubes[AppSettings.Get(this, "Cube.Templates.ContentTemplate")];


					Cube newContentCube = ContentCubeTemplate.Clone();



					////change cube name and id
					newContentCube.Name = string.Format("Content{0}", collectedData["AccountSettings.CubeName"].ToString());
					newContentCube.ID = string.Format("Content{0}", collectedData["AccountSettings.CubeID"].ToString());

					//Replace the scope_id
					foreach (MeasureGroup measureGroup in newContentCube.MeasureGroups)
					{

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


					SaveExecutorData(executorData);


				}
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
							sqlCommand.Parameters["@Value"].Value = input.Value; ;
							sqlCommand.Parameters["@sys_creation_date"].Value = DateTime.Now;

							sqlCommand.ExecuteNonQuery();

						}
					}


				}
			}
		}



	}
}
