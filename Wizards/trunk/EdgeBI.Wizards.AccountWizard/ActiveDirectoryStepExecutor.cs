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

			
			
			Log.Write("Getting collected data from suitable collector", LogMessageType.Information);

			Dictionary<string, object> collectedData = this.GetStepCollectedData(Instance.Configuration.Options["CollectorStep"]);
			this.ReportProgress((float)0.5);
			
			Log.Write("Creating user in active directory", LogMessageType.Information);

			CreatingUsers creatingUsers = new CreatingUsers();


			try
			{
				creatingUsers.AddNew(collectedData["UserName"].ToString(), collectedData["Password"].ToString(), collectedData["FullName"].ToString(), true);
				this.ReportProgress((float)0.5);
			}
			catch (Exception ex)
			{
				Log.Write("User Can't be add!", ex, LogMessageType.Error);
				throw new Exception("User Can't be add!", ex);


			}
			finally
			{
				this.ReportProgress((float)1);

			}
			Log.Write(string.Format("User {0} created in active directory",collectedData["UserName"].ToString()), LogMessageType.Information);
			
			return base.DoWork();
		}
		
		
		
		
	}
}
