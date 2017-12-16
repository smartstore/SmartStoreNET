using System;
using System.Collections.Generic;
using SmartStore.Collections;

namespace SmartStore.Services.Messages
{
	public interface IMessageModelProvider
	{
		void AddGlobalModelParts(MessageContext messageContext);
		void AddModelPart(object part, MessageContext messageContext, string name = null);
		TreeNode<ModelTreeMember> BuildModelTree(TemplateModel model);
		string ResolveModelName(object model);
	}
}
