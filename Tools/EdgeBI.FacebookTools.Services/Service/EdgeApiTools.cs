using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using EdgeBI.Objects;
using System.Net;
using Easynet.Edge.Core.Data;
using System.Data.SqlClient;

namespace EdgeBI.API.Web
{
	[ServiceContract]
	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class EdgeApiTools
	{
		[WebInvoke(Method="POST", UriTemplate = "refund")]
		public void AddRefund(Refund refund)
		{
			try
			{


				refund.AddRefund();
				
			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, ex.Message);
			}
		}
		[WebInvoke(Method = "POST", UriTemplate = "deleterefund")] //tempurl
		public void DeleteRefund(Refund refund)
		{
			try
			{
				refund.DeleteRefund();

			}
			catch (Exception ex)
			{

				ErrorMessageInterceptor.ThrowError(HttpStatusCode.Forbidden, ex.Message);
			}
		}
		[WebGet(UriTemplate = "refund")]
		public Refund GetRefund()
		{
			return new Refund() { AccountID=7,ChannelID=6,Month=new DateTime(1977,10,27),RefundAmount=90};

		}
	}
}