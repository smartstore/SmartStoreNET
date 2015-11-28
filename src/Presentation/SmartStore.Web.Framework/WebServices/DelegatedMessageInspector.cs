using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace SmartStore.Web.Framework.WebServices
{
	/// <summary>
	/// Useful to trace incoming and outgoing xml of WCF services.
	/// </summary>
	public class DelegatedEndpointBehavior : IEndpointBehavior
	{
		private Action<DelegatedMessageState> _handler;

		public DelegatedEndpointBehavior(Action<DelegatedMessageState> handler)
		{
			_handler = handler;
		}

		public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			clientRuntime.MessageInspectors.Add(new DelegatedMessageInspector(_handler));
		}

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
		}

		public void Validate(ServiceEndpoint endpoint)
		{
		}
	}


	public class DelegatedMessageInspector : IClientMessageInspector
	{
		private Action<DelegatedMessageState> _handler;

		public DelegatedMessageInspector(Action<DelegatedMessageState> handler)
		{
			_handler = handler;
		}

		/// <summary>
		/// Implement this method to inspect/modify messages before they are sent to the service
		/// </summary>
		public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
		{
			_handler(new DelegatedMessageState()
			{
				Message = request,
				Type = DelegatedMessageState.MessageType.Request
			});

			return null;
		}

		/// <summary>
		/// Implement this method to inspect/modify messages after a message is received but prior to passing it back to the client 
		/// </summary>
		public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
		{
			_handler(new DelegatedMessageState()
			{
				Message = reply,
				Type = DelegatedMessageState.MessageType.Reply
			});
		}
	}


	public class DelegatedMessageState
	{
		public enum MessageType : int
		{
			None = 0,
			Request,
			Reply
		}

		public System.ServiceModel.Channels.Message Message { get; set; }

		public MessageType Type { get; set; }
	}
}
