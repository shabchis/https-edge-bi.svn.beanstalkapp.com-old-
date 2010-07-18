using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Data;

namespace Easynet.Edge.Services.ScheduleManagement
{
	#region Comparers
	/*=========================*/

	/// <summary>
	///  We sort the services by TimeScheduled value from minimum to maximum.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>01/07/2008</creation_date>
	//    public class TimeScheduledComparer :System.Collections.Generic.IComparer<ServiceInstance>
	public class TimeScheduledComparer : IComparer<ServiceInstance>
	{
		#region Public Methods

		/// <summary>
		/// Compare the TimeScheduled of the 2 services. first compare if the 
		/// service is ready and than check his schedule time. 
		/// Ready services will be first.
		/// </summary>
		/// <param name="x">service instance 1 to compare</param>
		/// <param name="y">service instance 2 to compare</param>
		/// <returns>return -1 if x is lower than y and 1 if x higher than y</returns>
		public int Compare(ServiceInstance x, ServiceInstance y)
		{
			// Check if the services are ready.

			// X is Uninitialized and Y isn't Uninitialized
			if ((x.State == ServiceState.Uninitialized) &&
				(y.State != ServiceState.Uninitialized))
			{
				return 1;
			}
			// X isn't Uninitialized and Y is Uninitialized
			else if ((x.State != ServiceState.Uninitialized) &&
					(y.State == ServiceState.Uninitialized))
			{
				return -1;
			}			
			if (x.TimeScheduled > y.TimeScheduled)
			{
				return 1;
			}
			else if (x.TimeScheduled < y.TimeScheduled)
			{
				return -1;
			}

			else return CompareMaxDeviation(x, y);
		}

		/// <summary>
		/// Compare the MaxDeviation of the 2 services.
		/// </summary>
		/// <param name="x">service instance 1 to compare</param>
		/// <param name="y">service instance 2 to compare</param>
		/// <returns>return -1 if x is lower than y and 1 if x higher than y</returns>
		private int CompareMaxDeviation(ServiceInstance x, ServiceInstance y)
		{
			// x's ActiveSchedulingRule is null
			if ((x.ActiveSchedulingRule == null) &&
			   (y.ActiveSchedulingRule != null))
			{
				return 1;
			}

			// y's ActiveSchedulingRule is null
			if ((x.ActiveSchedulingRule != null) &&
			   (y.ActiveSchedulingRule == null))
			{
				return -1;
			}

			// x's and y's ActiveSchedulingRule is null
			if ((x.ActiveSchedulingRule == null) &&
			   (y.ActiveSchedulingRule == null))
			{
				return 1;
			}

			if (x.ActiveSchedulingRule.MaxDeviation > y.ActiveSchedulingRule.MaxDeviation)
			{
				return 1;
			}
			else if (x.ActiveSchedulingRule.MaxDeviation < y.ActiveSchedulingRule.MaxDeviation)
			{
				return -1;
			}

			// Both service instances have the same MaxDeviation.
			// Remark - make sure that we get random results (sometimes  x be first 
			// and sometimes y) if it dont happen we need to develop it.
			return 0;
		}

		#endregion
	
	}


	public class ServiceStateComparer : IComparer<ServiceInstance>
	{
		#region Public Methods

		/// <summary>
		/// Compare the TimeScheduled of the 2 services. first compare if the 
		/// service is ready and than check his schedule time. 
		/// Ready services will be last.
		/// </summary>
		/// <param name="x">service instance 1 to compare</param>
		/// <param name="y">service instance 2 to compare</param>
		/// <returns>return -1 if x is lower than y and 1 if x higher than y</returns>
		public int Compare(ServiceInstance x, ServiceInstance y)
		{
			// Check if the services are ready.

			// X is Uninitialized and Y isn't Uninitialized
			if ((x.State == ServiceState.Uninitialized) &&
				(y.State != ServiceState.Uninitialized))
			{
				return 1;
			}
			// X isn't Uninitialized and Y is Uninitialized
			else if ((x.State != ServiceState.Uninitialized) &&
					(y.State == ServiceState.Uninitialized))
			{
				return -1;
			}			
			else return 0;
		}



		#endregion

	}


	/*=========================*/
	#endregion
}
