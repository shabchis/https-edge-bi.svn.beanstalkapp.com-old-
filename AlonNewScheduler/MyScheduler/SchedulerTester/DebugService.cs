using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;
using System.Threading;

namespace SchedulerTester
{
	public class DebugService:Service
	{
		protected override ServiceOutcome DoWork()
		{
			try
			{
				string serviceName = Instance.Configuration.Name;
				int accountID = Instance.AccountID;
				using (DataManager.Current.OpenConnection())
				{
					SqlCommand sqlCommand = DataManager.CreateCommand(@"SELECT [Value]
																  FROM [testdb].[dbo].[ServiceConfigExecutionTimes] 
																  WHERE [ConfigName]=@ConfigName:NvarChar AND 
																  [ProfileID]=@ProfileID:Int");
					sqlCommand.Parameters["@ConfigName"].Value = serviceName;
					sqlCommand.Parameters["@ProfileID"].Value = accountID;
					TimeSpan timeOut = new TimeSpan(0, 0, 0, Convert.ToInt32(sqlCommand.ExecuteScalar()));
					Thread.Sleep(timeOut);

				}



				// TODO: test returning failure for failure repeat
				return ServiceOutcome.Success;
			}
			catch (Exception ex)
			{
				throw ex;
				
			}
		}
	}
}
