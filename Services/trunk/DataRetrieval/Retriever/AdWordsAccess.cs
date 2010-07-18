using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easynet.Edge.Services.Google.Adwords.Retriever
{
	/// <summary>
	/// This class used to gather all AdWords parameters:
	/// 'AccessEmail' - Access MCC for this client
	/// 'AccessPassword' - Access MCC for this client
	/// 'Email' - client email
	/// <author>Yaniv Kahana</author>
	/// <creation_date>21/07/2008</creation_date>
	/// </summary>
	public class AdWordsAccess
	{
		#region Members
		/*=========================*/

		private string _accessEmail;
		private string _accessPassword;
		private string _email;

		/*=========================*/
		#endregion

		#region Constructor
		/*=========================*/

		public AdWordsAccess(string accessEmail, string accessPassword, string email)
		{
			_accessEmail = accessEmail;
			_accessPassword = accessPassword;
			_email = email;
		}

		/*=========================*/
		#endregion

		#region Access Methods
		/*=========================*/

		public string AccessEmail
		{
			get { return _accessEmail; }
			set { _accessEmail = value; }
		}

		public string AccessPassword
		{
			get { return _accessPassword; }
			set { _accessPassword = value; }
		}

		public string Email
		{
			get { return _email; }
			set { _email = value; }
		}

		/*=========================*/
		#endregion
	}
}
