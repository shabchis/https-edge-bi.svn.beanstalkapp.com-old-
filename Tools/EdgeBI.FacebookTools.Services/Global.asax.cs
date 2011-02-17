// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace JsonValueSample
{
    using System;
    using System.Web.Routing;

    using Microsoft.ServiceModel.Http;	
	using System.ServiceModel.Activation;
	using System.Collections.Generic;	
	using System.IO;
	using EdgeBI.FacebookTools.Services.Service;
	

    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {



			var route = new ServiceRoute("", new EdgeApiServiceHostFactory(new EdgeApiServiceConfiguration()), typeof(FacebbookTools));
			RouteTable.Routes.Add(route);
		   

        }
    }
	public class module
	{
		public string RoutePrefix;
		public string AssemblyQualifiedName;

	}
}