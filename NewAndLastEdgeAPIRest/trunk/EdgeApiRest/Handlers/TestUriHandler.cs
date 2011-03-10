using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EdgeApiRest.Handlers.UriTemplate;

namespace EdgeApiRest.Handlers
{
	public class TestUriHandler: UriTemplateHandler
	{
		public override bool ShouldValidateSession
		{
			get { return false; }
		}

		[UriMapping(Method = "GET", Template = "users/{userID}")]
		public object Example(int userID)
		{
			return null;
		}

	}
}