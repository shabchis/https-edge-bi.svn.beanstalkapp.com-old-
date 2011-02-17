using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Activation;
using Easynet.Edge.Core.Data;
using System.Net;
using Easynet.Edge.Core.Utilities;
using System.Data.SqlClient;
using Microsoft.ServiceModel.Http;
using Microsoft.Http;
using System.IO;
using Newtonsoft.Json;
using System.Data;
using System.Text;
using Newtonsoft.Json.Linq;

/// <summary>
/// Summary description for AlonService
/// </summary>
/// 
namespace EdgeBI.FacebookTools.Services.Service
{
	[ServiceContract]
	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class FacebbookTools
	{
		[WebInvoke(Method="POST", UriTemplate = "test")]
		public void test(JObject j)
		{
			LifeTimeBudget l = new LifeTimeBudget();
			l.test(j);
		}


	}
}