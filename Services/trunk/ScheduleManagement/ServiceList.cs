using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;

namespace Easynet.Edge.Services.ScheduleManagement
{
	/// <summary>
	/// This class contain a list of services of for a certain server type
	/// (like RankerGoogleRetriever for example) the list is contain in a Dictionary
	/// that the key is start time and the object is a list with all the services that
	/// need to run in start time.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>01/07/2008</creation_date>
	public class ServiceList : List<ServiceInstance> 
	{
		#region Members
		/*=========================*/

		//private ServiceElement _config;
		private int _maxInstances;

		/*=========================*/
		#endregion

		#region Constructor
		/*=========================*/

		/// <summary>
		/// Construactor - create service list with the service Instance parameters.
		/// </summary>
		/// <param name="ServiceInstance">We create service list by this service parameters.</param>

		public ServiceList(ServiceElement serviceElement)
		{
			//_config = serviceElement;
			_maxInstances = serviceElement.MaxInstances;
		}
		
		/*=========================*/
		#endregion

		#region Access Methods
		/*=========================*/

		public int ServiceMaxSlots
		{
			get
			{
				return _maxInstances;
				//return _config.MaxInstances;
			}
		}

		/*=========================*/
		#endregion

	}
}
