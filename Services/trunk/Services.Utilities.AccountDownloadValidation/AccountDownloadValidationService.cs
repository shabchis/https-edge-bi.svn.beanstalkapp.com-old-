﻿using System;
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

namespace Services.Utilities.AccountDownloadValidation
{
	public class AccountDownloadValidationService : Service
    {
        SqlCommand _LogCmd ,_baseCmd,_setCmd;
		string SourceConn,conn;
		

        protected override void OnInit()
        {
			conn = DataManager.ConnectionString;
			SourceConn =  ConfigurationSettings.AppSettings["Easynet.Edge.Services.DataRetrieval.BaseService.SourceConnectionString"];
            if (String.IsNullOrEmpty(SourceConn))
				throw new System.Configuration.ConfigurationException("Missing configuration appSettings SourceConnectionString");
						
            DataManager.ConnectionString = SourceConn;
			_LogCmd = DataManager.CreateCommand("SELECT [Account_ID],[DayCode],[Service] FROM [Source].[dbo].[AccountsServicesLog] WHERE Status is 0");
			_setCmd = DataManager.CreateCommand("Update [Source].[dbo].[AccountsServicesLog] set [status] = 9");
			//_baseCmd.Parameters["@ACCOUNT_ID"].Value =7;

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
			}
			
			return ServiceOutcome.Success;
		}
	}
}
