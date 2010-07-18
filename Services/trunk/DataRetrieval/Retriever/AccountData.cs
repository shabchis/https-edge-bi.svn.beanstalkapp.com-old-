using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easynet.Edge.Services.DataRetrieval.Retriever
{
	/// <summary>
	/// This struct contain all the parmeters that need to access Google Adwords,
	/// including Easynet token and the required account data.
	/// <author>Yaniv Kahana</author>
	/// <creation_date>21/07/2008</creation_date>
	/// </summary>
    public struct AccountData
    {
        public string UserAgent;
        public string Email;
        public string Password;
        public string ClientEmail;
        public string Token;
        public string AppToken;

        public AccountData(string UserAgent, string Email, string Password, string ClientEmail, string Token, string AppToken)
       {
            this.AppToken = AppToken;
            this.UserAgent = UserAgent;
            this.Email = Email;
            this.Password = Password;
            this.ClientEmail = ClientEmail;
            this.Token = Token;

        }
        public AccountData(AccountData copy)
        {
            this.AppToken = copy.AppToken;
            this.UserAgent = copy.UserAgent;
            this.Email = copy.Email;
            this.Password = copy.Password;
            this.ClientEmail = copy.ClientEmail;
            this.Token = copy.Token;
        }
    }
}
