using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Services.DataRetrieval.GAdWordsReportServiceV13;

namespace Easynet.Edge.Services.DataRetrieval.Retriever
{
	/// <summary>
	/// This class warp ReportService class of google AdWords API.
	/// <author>Yaniv Kahana</author>
	/// <creation_date>21/07/2008</creation_date>
	/// </summary>
    class ReportServiceWrapper : ReportService
    {
        #region Public Methods  
        /*=========================*/

        /// <summary>
        /// Initalize the account services with the parameters of the account data.
        /// </summary>
        /// <param name="accessData"></param>
        public void Update(AccountData accessData)
        {
			useragentValue = new GAdWordsReportServiceV13.useragent();

            useragentValue.Text = new String[] { accessData.UserAgent };

            emailValue = new GAdWordsReportServiceV13.email();
            emailValue.Text = new String[] { accessData.Email };

            passwordValue = new GAdWordsReportServiceV13.password();
            passwordValue.Text = new String[] { accessData.Password };

            clientEmailValue = new GAdWordsReportServiceV13.clientEmail();
            clientEmailValue.Text = new String[] { accessData.ClientEmail };

            developerTokenValue = new GAdWordsReportServiceV13.developerToken();
            developerTokenValue.Text = new String[] { accessData.Token };

            applicationTokenValue = new GAdWordsReportServiceV13.applicationToken();
            applicationTokenValue.Text = new String[] { accessData.AppToken };
        }
        /*=========================*/
        #endregion  
    }
}
