using System;
using System.Collections.Generic;

namespace SmartStore.Services.Messages
{
	public interface IMessageModelProvider
	{
		void AddGlobalModelParts(MessageContext messageContext, IDictionary<string, object> model);
		void AddModelPart(object part, MessageContext messageContext, IDictionary<string, object> model, string name = null);
	}
}
