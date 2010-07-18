using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Services.Google.Adwords.GAdWordsAccountServiceV13;

namespace Easynet.Edge.Services.Google.Adwords
{
	/// <summary>
	/// This class warp AccountService class of google AdWords API.
	/// <author>Yaniv Kahana</author>
	/// <creation_date>21/07/2008</creation_date>
	/// </summary>
    public class AccountServiceWrapper : AccountService
    {
        #region Public Methods
        /*=========================*/

        /// <summary>
        /// Initalize the account services with the parameters of the account data.
        /// </summary>
        /// <param name="accessData"></param>
        public void Update(AccountData accessData)
        {
            useragentValue = new GAdWordsAccountServiceV13.useragent();
            useragentValue.Text = new String[] { accessData.UserAgent };

            emailValue = new GAdWordsAccountServiceV13.email();
            emailValue.Text = new String[] { accessData.Email };

            passwordValue = new GAdWordsAccountServiceV13.password();
            passwordValue.Text = new String[] { accessData.Password };

            clientEmailValue = new GAdWordsAccountServiceV13.clientEmail();
            clientEmailValue.Text = new String[] { accessData.ClientEmail };

            developerTokenValue = new GAdWordsAccountServiceV13.developerToken();
            developerTokenValue.Text = new String[] { accessData.Token };

            applicationTokenValue = new GAdWordsAccountServiceV13.applicationToken();
            applicationTokenValue.Text = new String[] { accessData.AppToken };
        }
        /*=========================*/
         #endregion  
    }

}
