using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Collections.ObjectModel;

/*
 * from http://wcf.codeplex.com/Thread/View.aspx?ThreadId=239005
 */
namespace EdgeBI.API.Web
{
	public abstract class MessageInterceptor
	{
		public abstract void ProcessResponse(ref System.ServiceModel.Channels.Message request, ref System.ServiceModel.Channels.Message response);
		public abstract void ProcessRequest(ref System.ServiceModel.Channels.Message request );
	}

	public class MessageInspector : IDispatchMessageInspector, IServiceBehavior
	{
		System.ServiceModel.Channels.Message _request;

		Collection<MessageInterceptor> _msgReqInterceptors = new Collection<MessageInterceptor>();
		Collection<MessageInterceptor> _msgResInterceptors = new Collection<MessageInterceptor>();

		public Collection<MessageInterceptor> RequestInterceptors { get { return _msgReqInterceptors; } }
		public Collection<MessageInterceptor> ResponseInterceptors { get { return _msgResInterceptors; } }

		#region IDispatchMessageInspector Members

		public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel, System.ServiceModel.InstanceContext instanceContext)
		{
			_request = request;

			
				if (_msgReqInterceptors != null)
				{

					foreach (var msgInterceptor in _msgReqInterceptors)
					{

						msgInterceptor.ProcessRequest(ref request);

					}
				}
				
			
			return null;

		}

		public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
		{

			
				if (_msgResInterceptors != null)
				{
					foreach (var msgInterceptor in _msgResInterceptors)
					{
						msgInterceptor.ProcessResponse(ref this._request, ref reply);
					}
				}
			
		}

		#endregion

		#region IServiceBehavior Members

		void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
		{
		}

		void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
		{
			foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
			{
				foreach (EndpointDispatcher endPointDispatcher in channelDispatcher.Endpoints)
				{
					endPointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
					
				}
			}
		}

		void IServiceBehavior.Validate(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
		{
		}

		#endregion
	}
}
