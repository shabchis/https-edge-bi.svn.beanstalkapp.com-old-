using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Services;
using EdgeBI.Wizards.AccountWizard.CubeCreation;

namespace EdgeBI.Wizards.AccountWizard
{
	class ActiveDirectoryStepExecutor : StepExecuter
	{
		protected override ServiceOutcome DoWork()
		{


			
			//Log.Write(this.Instance.Configuration.Name, "Getting collected data from suitable collector", LogMessageType.Information);

			Dictionary<string, object> collectedData = this.GetStepCollectedData(Instance.Configuration.Options["CollectorStep"]);
			
			//Log.Write(this.Instance.Configuration.Name, "Creating user in active directory", LogMessageType.Information);

			CreatingUsers creatingUsers = new CreatingUsers();

			try
			{
				creatingUsers.AddNew(collectedData["UserName"].ToString(), collectedData["Password"].ToString(), collectedData["FullName"].ToString(), true);
			}
			catch (Exception ex)
			{
			//	Log.Write("User Can't be add!", ex,LogMessageType.Error);
				throw new Exception("User Can't be add!", ex);
				
				
			}
		//	Log.Write(this.Instance.Configuration.Name,string.Format("User {0} created in active directory",collectedData["UserName"].ToString()), LogMessageType.Information);
			
			return base.DoWork();
		}
		
		
		
	}
}
