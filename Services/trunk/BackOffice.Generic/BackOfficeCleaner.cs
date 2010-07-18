using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Services.DataRetrieval;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;
using System.Data;

namespace Easynet.Edge.Services.BackOffice.Generic
{
	/// <summary>
	/// This class delete all the data in backoffice table on a chosen day.
	/// This service should be activated before we add new data from backoffice.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>27/08/2008</creation_date>
	
	public class BackOfficeCleanerService :BaseService
	{
		#region Consts
		/*=========================*/

		private const string BackOfficeTable = "BackOffice_Client_Gateway";

		/*=========================*/
		#endregion	

	

		#region Service Override Methods
		/*=========================*/

		/// <summary>
		/// Main entry point of the service.
		/// </summary>
		/// <returns>True for success. False for failure</returns>
		protected override ServiceOutcome DoWork()
		{			
			CheckManualDate();

			// Delete old data from Today.
			DeleteDayBO(GetDayCode(_requiredDay), BackOfficeTable);

			return ServiceOutcome.Success;
		}

		/*=========================*/
		#endregion   
	}
}
