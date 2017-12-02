using System;
using System.Collections.Generic;
using SmartStore.Collections;

namespace SmartStore.Services.Messages
{
	public interface IMessageModelProvider
	{
		void AddGlobalModelParts(MessageContext messageContext, IDictionary<string, object> model);
		void AddModelPart(object part, MessageContext messageContext, IDictionary<string, object> model, string name = null);
		TreeNode<ModelTreeMember> BuildModelTree(IDictionary<string, object> model);
	}
}
