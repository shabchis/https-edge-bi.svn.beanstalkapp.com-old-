using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Services;
using EdgeBI.Wizards.AccountWizard;
using Easynet.Edge.Core.Configuration;
using Microsoft.AnalysisServices;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using Easynet.Edge.Core.Data;


namespace EdgeBI.Wizards.AccountWizard
{
	class CreateRoleStepExecutor : StepExecuter
	{

		protected override Easynet.Edge.Core.Services.ServiceOutcome DoWork()
		{
			
			Log.Write("Getting collected data from suitable collector", LogMessageType.Information);

			Dictionary<string, object> collectedData = this.GetCollectedData();
            if (collectedData.ContainsKey(ApplicationIDKey))
                SetAccountWizardSettingsByApllicationID(Convert.ToInt32(collectedData[ApplicationIDKey]));
			this.ReportProgress(0.1f);
			Log.Write("Creating role on analysis server", LogMessageType.Information);
            if (!(bool)collectedData["AccountSettings.UseExistingRole"])
            {
                CreateRole(collectedData);
            }
			this.ReportProgress(0.7f);
			Log.Write("Role Created", LogMessageType.Information);

			Log.Write("Update OLTP database", LogMessageType.Information);
			UpdateOltpDataBase(collectedData);
			this.ReportProgress(1);

			return base.DoWork();
		}

//     

		private void CreateRole(Dictionary<string, object> collectedData)
		{
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

				//Create new role
				Role newRole;
				try
				{
					newRole = analysisDatabase.Roles.Add(collectedData["AccountSettings.RoleName"].ToString(), collectedData["AccountSettings.RoleID"].ToString());
                    if (collectedData.ContainsKey("AccountSettings.RoleMemberName"))
                    {
                        newRole.Members.Add(new RoleMember(collectedData["AccountSettings.RoleMemberName"].ToString()));
                    }
                    else
                    {
                        Dictionary<string, object> executorData = GetExecutorData("ActiveDirectoryStepExecutor");
                        newRole.Members.Add(new RoleMember(executorData["ActiveDirectory.UserName"].ToString()));
                    }
					
					newRole.Update();
				}
				catch (Exception ex)
				{
					Log.Write("Error when adding a role", ex);
					throw new Exception("Error when adding a role", ex);
				}

			}
		}

		



	}

}
