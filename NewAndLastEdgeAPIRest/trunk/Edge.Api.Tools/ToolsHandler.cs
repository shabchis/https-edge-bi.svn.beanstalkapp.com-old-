using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Edge.Objects;
using Edge.Api.Handlers.Template;
using System.Net;

namespace Edge.Api.Tools.Handlers
{
	public class ToolsHandler : TemplateHandler
	{
		[UriMapping(Method = "POST", Template = "tools/refund", BodyParameter = "refund")]
		public void AddRefund(Refund refund)
		{
			refund.AddRefund();			
		}
		[UriMapping(Method = "POST", Template = "tools/deleterefund", BodyParameter = "refund")] //tempurl
		public void DeleteRefund(Refund refund)
		{
			
				refund.DeleteRefund();	
		}
		[UriMapping(Method="GET",Template = "tools/refund")]
		public Refund GetRefund()
		{
			return new Refund() { AccountID = 7, ChannelID = 6, Month =DateTime.Now, RefundAmount = 90 };

		}
		public override bool ShouldValidateSession
		{
			get { return true; }
		}
		
	}
}
