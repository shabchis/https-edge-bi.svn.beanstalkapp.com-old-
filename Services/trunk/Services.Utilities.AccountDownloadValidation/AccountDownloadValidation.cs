using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Data;
using System.Text.RegularExpressions;
using System.Reflection;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Utilities;
using System.Configuration;

namespace Easynet.Edge.Services.Utilities
{
	public class AccountDownloadValidation : Service
	{
		SqlCommand _LogCmd, _baseCmd, _setCmd, sp;
		string SourceConn, conn;
		string application;


		protected override void OnInit()
		{
			conn = DataManager.ConnectionString;
			SourceConn = ConfigurationSettings.AppSettings["Easynet.Edge.Services.DataRetrieval.BaseService.SourceConnectionString"];
			if (String.IsNullOrEmpty(SourceConn))
				throw new System.Configuration.ConfigurationException("Missing configuration appSettings SourceConnectionString");

			application = ConfigurationSettings.AppSettings["Easynet.Edge.Services.AccountValidation.DataBase"];
			if (String.IsNullOrEmpty(SourceConn))
				throw new System.Configuration.ConfigurationException("AccountValidationDataBase AppSettings Key is missing");
			
			// Check for a custom timeout
			string timeoutStr;
			TimeSpan _cmdTimeOut;
			if (Instance.Configuration.Options.TryGetValue("ConnectionTimeout", out timeoutStr))
			{
				if (TimeSpan.TryParse(timeoutStr, out _cmdTimeOut))
					DataManager.CommandTimeout = (Int32)(_cmdTimeOut.TotalSeconds);
			}
			if (String.IsNullOrEmpty(timeoutStr))
				throw new System.Configuration.ConfigurationException("AccountValidationDataBase ConnectionTimeout is missing");


			_LogCmd = DataManager.CreateCommand("SELECT [Account_ID],[DayCode],[Service],[Application],[Status] FROM [Source].[dbo].[AccountsServicesLog] WHERE Status = @stat:int and [Application]=@application:Nvarchar  order by [Application],[Status],[Account_ID]");
			_setCmd = DataManager.CreateCommand("Update [Source].[dbo].[AccountsServicesLog] set [status] = @stat:int where [Account_ID]=@account_id:int "
												+ "and [DayCode] = @day_code:int and [Service] = @service:int and [Application]=@app:Nvarchar");
			sp = DataManager.CreateCommand("Validate_Account()", System.Data.CommandType.StoredProcedure);
			using (DataManager.Current.OpenConnection())
			{
				DataManager.Current.AssociateCommands(sp);
				sp.ExecuteNonQuery();
			}
			DataManager.ConnectionString = SourceConn;
		}

		protected override ServiceOutcome DoWork()
		{
			List<AccountEntity> _Failed = new List<AccountEntity>();
			List<AccountEntity> _Success = new List<AccountEntity>();

			
			using (DataManager.Current.OpenConnection())
			{
				DataManager.Current.AssociateCommands(_LogCmd);
				
				//Getting failure accounts
				_LogCmd.Parameters["@stat"].Value = 0;
				_LogCmd.Parameters["@application"].Value = application;
				using (SqlDataReader _reader = _LogCmd.ExecuteReader())
				{
					try
					{
						if (!_reader.IsClosed)
							while (_reader.Read())
							{
								_Failed.Add(new AccountEntity(_reader));
							}
					}
					catch (Exception e)
					{
						Log.Write("Failed to retrieve accounts from service log table [ _Failed Accounts ] ", e);
					}

					
				}
				//Getting successed accounts
				_LogCmd.Parameters["@stat"].Value = 1;
				_LogCmd.Parameters["@application"].Value = application;
				using (SqlDataReader _reader = _LogCmd.ExecuteReader())
				{
					try
					{
						if (!_reader.IsClosed)
							while (_reader.Read())
							{
								_Success.Add(new AccountEntity(_reader));
							}
					}
					catch (Exception e)
					{
						Log.Write("Failed to retrieve accounts from service log table [ _Success Accounts ] ", e);
					}
					
				}
			}
			#region SendingReportByEmail

			//Sending Report by Email.
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("The following accounts reported :");
			sb.AppendLine("<table border=\"1\">");
			//Table headers
			sb.AppendLine("<tr><th>DayCode</th><th>Account ID</th><th>Account_Name</th><th>Service</th><th>Application</th><th>Status</th></tr>");
			string _startTag = "<td>";
			string _endTag = "</td>";
			if (_Failed.Count > 0)
			{
				foreach (AccountEntity _account in _Failed)
				{
					sb.AppendLine("<tr style=\"color:red\">" + _startTag + _account.DayCode.ToString() + _endTag + _startTag + _account.Account_id.ToString() + _endTag + _startTag + _account.Account_Name  +_endTag + _startTag + _account.CahnnelType + _endTag + _startTag + _account.App + _endTag + _startTag + _account.Status + _endTag + "</tr>");
				}
				foreach (AccountEntity _account in _Success)
				{
					sb.AppendLine("<tr>" + _startTag + _account.DayCode.ToString() + _endTag + _startTag + _account.Account_id.ToString() + _endTag + _startTag + _account.Account_Name  +_endTag + _startTag + _account.CahnnelType + _endTag + _startTag + _account.App + _endTag + _startTag + _account.Status + _endTag + "</tr>");
				}
				sb.AppendLine("</table>");
				try
				{
					Smtp.Send("Accounts Validation Report [" + application + "]", true, sb.ToString(), true, null);
				}
				catch (Exception e)
				{
					Log.Write("Error while trying to send account validation report email", e);
				}
			

			#endregion //SendingReportByEmail

				#region Update_Report_Status_Table 

				SqlCommand _cmd = new SqlCommand();

				using (DataManager.Current.OpenConnection())
				{
					_cmd = _setCmd;

					foreach (AccountEntity _account in _Failed)
					{
						_cmd = _setCmd;
						DataManager.Current.AssociateCommands(_cmd);
						_cmd.Parameters["@account_id"].Value = _account.Account_id;
						_cmd.Parameters["@day_code"].Value = _account.DayCode;
						_cmd.Parameters["@service"].Value = _account.Channel;
						_cmd.Parameters["@app"].Value = _account.App;
						_cmd.Parameters["@stat"].Value = 9;
						_cmd.ExecuteNonQuery();
					}
					foreach (AccountEntity _account in _Success)
					{
						_cmd = _setCmd;
						DataManager.Current.AssociateCommands(_cmd);
						_cmd.Parameters["@account_id"].Value = _account.Account_id;
						_cmd.Parameters["@day_code"].Value = _account.DayCode;
						_cmd.Parameters["@service"].Value = _account.Channel;
						_cmd.Parameters["@app"].Value = _account.App;
						_cmd.Parameters["@stat"].Value = 10;
						_cmd.ExecuteNonQuery();
					}
				}
				#endregion Update_Report_Status_Table
			}
			return ServiceOutcome.Success;
		}
	}
}
