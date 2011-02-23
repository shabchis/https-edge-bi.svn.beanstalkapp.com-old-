using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.ServiceModel.Http;
using System.Collections.ObjectModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using JsonValueSample;
using System.ServiceModel.Dispatcher;

namespace EdgeBI.FacebookTools.Services.Service
{
	public class EdgeApiServiceHost : WebHttpServiceHost
	{
		public EdgeApiServiceHost(Type serviceType, HostConfiguration configuration, params Uri[] baseAddresses)
			: base(serviceType, configuration, baseAddresses)
		{
		}

		protected override void OnOpening()
		{
			base.OnOpening();

			MessageInspector inspector = new MessageInspector();
			inspector.RequestInterceptors.Add(new SessionValidationInterceptor());
			inspector.ResponseInterceptors.Add(new ErrorMessageInterceptor());

			this.Description.Behaviors.Insert(0, inspector);
		}
	}

	public class EdgeApiServiceHostFactory : WebHttpServiceHostFactory
	{
		HostConfiguration _hostConfiguration;
		public EdgeApiServiceHostFactory(HostConfiguration hostConfiguration = null) : base(hostConfiguration)
		{
			_hostConfiguration = hostConfiguration;
		}

		protected override System.ServiceModel.ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
		{
			EdgeApiServiceHost host = new EdgeApiServiceHost(serviceType, _hostConfiguration, baseAddresses);
			return host;
		}

	}

	public class EdgeApiServiceConfiguration : HostConfiguration
	{

		public override void RegisterRequestProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors, MediaTypeProcessorMode mode)
		{
			processors.Add(new JsonNetProcessor(operation, mode));			
			
		}

		public override void RegisterResponseProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors, MediaTypeProcessorMode mode)
		{
			processors.Add(new JsonNetProcessor(operation, mode));
			processors.Add(new TabDelimited(operation, mode));
			
		}
	}
	
	
}