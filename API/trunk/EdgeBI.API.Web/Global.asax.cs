// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace JsonValueSample
{
    using System;
    using System.Web.Routing;

    using Microsoft.ServiceModel.Http;
	using EdgeBI.API.Web;
	using System.ServiceModel.Activation;
	using System.Collections.Generic;
	using Db4objects.Db4o;
	using Db4objects.Db4o.CS.Config;
	using Db4objects.Db4o.CS;
	using Db4objects.Db4o.Constraints;
	using System.IO;
	using Db4objects.Db4o.Config;

    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
			

			///*Load dll dynamicly in order to  allow publish only specific dlls.*/
			string path = Server.MapPath("~/test.db4o");


			IEmbeddedConfiguration serverConfig = Db4oEmbedded.NewConfiguration();
			serverConfig.Common.ObjectClass(typeof(module)).ObjectField("RoutePrefix").Indexed(true);
			serverConfig.Common.Add(new UniqueFieldValueConstraint(typeof(module), "RoutePrefix"));
			
			IObjectSet result;
			using (IObjectContainer db = Db4oEmbedded.OpenFile(serverConfig, path))
			{
				module m = new module() { AssemblyQualifiedName = "EdgeBI.API.Web.EdgeApiTools,EdgeBI.API.Web.EdgeApiTools", RoutePrefix = "Tools" };
				db.Store(m);
				try
				{
					db.Commit();

				}
				catch (Exception)
				{
					db.Rollback();

				}
				result = db.QueryByExample(typeof(module));
				db.Ext().Refresh(result, 0);
				foreach (module mm in result)
				{
					Type t = Type.GetType(m.AssemblyQualifiedName, false);
					if (t != null)
					{
						var route1 = new ServiceRoute(m.RoutePrefix, new EdgeApiServiceHostFactory(new EdgeApiServiceConfiguration()), t);
						RouteTable.Routes.Add(route1);
					}

				}
			}
		
			var route2 = new ServiceRoute("", new EdgeApiServiceHostFactory(new EdgeApiServiceConfiguration()), typeof(EdgeApiCore));
			RouteTable.Routes.Add(route2);
		   

        }
    }
	public class module
	{
		public string RoutePrefix;
		public string AssemblyQualifiedName;

	}
}