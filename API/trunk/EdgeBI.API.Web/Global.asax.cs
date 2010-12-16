// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace JsonValueSample
{
    using System;
    using System.Web.Routing;

    using Microsoft.ServiceModel.Http;
	using EdgeBI.API.Web;

    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
			RouteTable.Routes.AddServiceRoute<EdgeBIAPIService>("", new MyResourceConfiguration());
        }
    }
}