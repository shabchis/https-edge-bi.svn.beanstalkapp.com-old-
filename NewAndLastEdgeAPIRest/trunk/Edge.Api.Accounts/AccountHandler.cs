using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Edge.Api.Handlers.Template;
using Edge.Objects;
using System.Net;

namespace Edge.Api.Accounts.Handlers
{
	public class AccountHandler : TemplateHandler
	{
		#region Accounts

		[UriMapping(Template = "accounts/{accountID}")]
		public Account GetAccount(string accountID)
		{
			List<Account> acc = null;
			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
				int? accId = int.Parse(accountID);
				acc = Account.GetAccount(accId, true, currentUser);
				if (acc.Count == 0)
					throw new HttpStatusException(HttpStatusCode.NotFound, String.Format("No account with permission found for user {0}", currentUser));
			}
			catch (Exception ex)
			{

				throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
			}
			return acc[0];
		}

		[UriMapping(Template = "Accounts")]
		public List<Account> GetAccount()
		{
			List<Account> acc = null;
			try
			{
				int currentUser;
				currentUser = System.Convert.ToInt32(CurrentContext.Request.Headers["edge-user-id"]);
				acc = Account.GetAccount(null, true, currentUser);
				if (acc.Count == 0)
					throw new HttpStatusException(HttpStatusCode.NotFound, String.Format("No account with permission found for user {0}", currentUser));
			}
			catch (Exception ex)
			{

				throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
			}
			return acc;
		}
		#endregion

		public override bool ShouldValidateSession
		{
			get { return true; }
		}
	}
}
