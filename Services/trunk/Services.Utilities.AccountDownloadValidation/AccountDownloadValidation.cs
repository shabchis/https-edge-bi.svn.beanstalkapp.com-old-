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
        SqlCommand _LogCmd ,_baseCmd,_setCmd,sp;
		string SourceConn,conn;
		

        protected override void OnInit()
        {
			conn = DataManager.ConnectionString;
			SourceConn =  ConfigurationSettings.AppSettings["Easynet.Edge.Services.DataRetrieval.BaseService.SourceConnectionString"];
            if (String.IsNullOrEmpty(SourceConn))
				throw new System.Configuration.ConfigurationException("Missing configuration appSettings SourceConnectionString");


			_LogCmd = DataManager.CreateCommand("SELECT [Account_ID],[DayCode],[Service],[Application],[Status] FROM [Source].[dbo].[AccountsServicesLog] WHERE Status = @stat:int  order by [Application],[Status],[Account_ID]");
			_setCmd = DataManager.CreateCommand("Update [Source].[dbo].[AccountsServicesLog] set [status] = @stat where [Account_ID]=@account_id:int "
												+ "and [DayCode] = @day_code:int and [Service] = @service:int and [Application]=@app:Nvarchar");
			//_baseCmd.Parameters["@ACCOUNT_ID"].Value =7;
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
			//Getting all values to test 
			using (DataManager.Current.OpenConnection())
            {
                DataManager.Current.AssociateCommands(_LogCmd);
				_LogCmd.Parameters["@stat"].Value = 0;
               // Log.Write(_cmd.ToString(), LogMessageType.Information);
				using (SqlDataReader _reader = _LogCmd.ExecuteReader())
				{
					if (!_reader.IsClosed)
						while (_reader.Read())
						{
							_Failed.Add(new AccountEntity(_reader));
						}
				}
				_LogCmd.Parameters["@stat"].Value = 1;
				using (SqlDataReader _reader = _LogCmd.ExecuteReader())
				{
					if (!_reader.IsClosed)
						while (_reader.Read())
						{
							_Success.Add(new AccountEntity(_reader));
						}
				}
            }
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("The following accounts reported :");
			sb.AppendLine("<table>");
			//Table headers
			sb.AppendLine("<tr><th>DayCode</th><th>Account ID</th><th>Application</th><th>Status</th></tr>");
			string _startTag = "<td>";
			string _endTag = "</td>";
			if (_Failed.Count > 0)
			{
				foreach (AccountEntity _account in _Failed)
				{
					sb.AppendLine("<tr>" + _startTag + _account.DayCode.ToString() + _endTag + _startTag + _account.Account_id.ToString() + _endTag + _startTag + _account.CahnnelType + _endTag + _startTag + _account.App + _endTag + _startTag + _account.Status + _endTag + "</tr>");
				}
				foreach (AccountEntity _account in _Success)
				{
					sb.AppendLine("<tr>" + _startTag + _account.DayCode.ToString() + _endTag + _startTag + _account.Account_id.ToString() + _endTag + _startTag + _account.CahnnelType + _endTag + _startTag + _account.App + _endTag + _startTag + _account.Status + _endTag + "</tr>");
				}
				sb.AppendLine("</table>");

				Smtp.Send("Accounts Validation Report", true, sb.ToString(),true,null);
				
				//update status to reported
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
			}
			return ServiceOutcome.Success;
		}
	}
}
