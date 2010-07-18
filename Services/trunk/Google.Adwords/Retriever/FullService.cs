using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Services.Google.Adwords.GAdWordsReportServiceV13;
using Easynet.Edge.Services.DataRetrieval;

namespace Easynet.Edge.Services.Google.Adwords
{
	/// <summary>
	/// This class contain report service and account service and give an 
	/// abstrcation for there functions.
	/// <author>Yaniv Kahana</author>
	/// <creation_date>21/07/2008</creation_date>
	/// </summary>
    public class FullService
    {
        #region Members
        /*=========================*/
		private ReportServiceWrapper _reportService = null;
		private AccountServiceWrapper _accountService = null;
		private AccountData _accessAccount;
        /*=========================*/
        #endregion

		#region Constructor
		/*=========================*/
		public FullService(AccountData accessAccount)
		{
			_reportService = new ReportServiceWrapper();
			_accountService = new AccountServiceWrapper();
			_accessAccount = new AccountData(accessAccount);
		}
		/*=========================*/
		#endregion

        #region Access Methods
        /*=========================*/
		public AccountData AccessAccount
        {
            get
            {
				return _accessAccount;
            }
        }
        /*=========================*/
        #endregion
   
        #region public Methods
        /*=========================*/

        public void Update()
        {
			_reportService.Update(_accessAccount);
			_accountService.Update(_accessAccount);
        }

        public void Abort()
        {
            _reportService.Abort();
            _accountService.Abort();
        }
        
        public string GetDescriptiveName()
        {
            return _accountService.getAccountInfo().descriptiveName;
        }

		/// <summary>
		/// Validate the job and schedule his report.
		/// </summary>
		/// <param name="job">The job to shcedule.</param>
		/// <returns>ID of the job, Id = 0 means the job is not valid</returns>
		public long ScheduleReportJob(GAdWordsReportServiceV13.ReportJob job, int accountID)
        {
            try
            {
                _reportService.validateReportJob(job);
				//_reportService.
                return _reportService.scheduleReportJob(job);				
            }
            catch (Exception ex)
            {
				Log.Write("Google AdWords report is invaild.", ex);
				return 0;
            }
        }

        /// <summary>
        /// Create the report of the job and get his url.
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns>The url of created report.</returns>
        public string GetReportDownloadUrl(long jobID, int accountID)
        {
			int failureTimes = 0;
			ReportJobStatus status = ReportJobStatus.Pending;

			// Try to get the report till we get get a status of Completed or Failed.
			// make break of 10 seconds between each try.
            while ((status != ReportJobStatus.Completed) && (status != ReportJobStatus.Failed))
            {
                try
                {
                    status = _reportService.getReportJobStatus(jobID);					
                }
                catch (Exception ex)
				{
					if (failureTimes < 3)
					{
						++failureTimes;
					}
					else
					{
						Log.Write("Can't get report status from Google AdWords.", ex);
						return "failed";
					}
				}
				System.Threading.Thread.Sleep(3000); // 3 seconds
            }
			if (status == ReportJobStatus.Failed)
			{
				Log.Write("Can't get report from Google AdWords for accountID ", LogMessageType.Error);
				return "failed";
			}

			try
			{
				return _reportService.getReportDownloadUrl(jobID);
			}
			catch (Exception ex)
			{
				Log.Write("Can't get report from Google AdWords.", ex);
				return "failed";
			}
        }
        /*=========================*/
        #endregion
    }
}
