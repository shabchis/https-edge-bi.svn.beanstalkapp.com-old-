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
						
            
			_LogCmd = DataManager.CreateCommand("SELECT [Account_ID],[DayCode],[Service] FROM [Source].[dbo].[AccountsServicesLog] WHERE Status = 0");
			_setCmd = DataManager.CreateCommand("Update [Source].[dbo].[AccountsServicesLog] set [status] = 9 where [Account_ID]=@account_id:int "
												+ "and [DayCode] = @day_code:int and [Service] = @service:int");
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
			List<AccountEntity> _accounts = new List<AccountEntity>();
			//Getting all values to test 
			using (DataManager.Current.OpenConnection())
            {
                DataManager.Current.AssociateCommands(_LogCmd);
               // Log.Write(_cmd.ToString(), LogMessageType.Information);
				using (SqlDataReader _reader = _LogCmd.ExecuteReader())
				{
					if (!_reader.IsClosed)
						while (_reader.Read())
						{
							_accounts.Add(new AccountEntity(_reader));
						}
				}
            }
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("The following accounts reported failure :");
			sb.AppendLine("DayCode\t" + "Account ID\t" + "Channel");
			if (_accounts.Count > 0)
			{
				foreach (AccountEntity _account in _accounts)
				{
					sb.AppendLine(_account.DayCode.ToString() + "\t" + _account.Account_id.ToString() + "\t" + _account.CahnnelType);
				}
				Smtp.Send("Accounts Validataion Report", true, sb.ToString(), null);
				
				//set status  = 9 ( means data wasnt downloaded and reported )
				SqlCommand _cmd = new SqlCommand();
				
				using (DataManager.Current.OpenConnection())
				{
					_cmd = _setCmd; 
					
					foreach (AccountEntity _account in _accounts)
					{
						_cmd = _setCmd;
						DataManager.Current.AssociateCommands(_cmd);
						_cmd.Parameters["@account_id"].Value = _account.Account_id;
						_cmd.Parameters["@day_code"].Value = _account.DayCode;
						_cmd.Parameters["@service"].Value = _account.Channel;
						_cmd.ExecuteNonQuery();
					}
				}
			}
			return ServiceOutcome.Success;
		}
	}
}
