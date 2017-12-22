using System;
using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Core.Domain.Messages;

namespace SmartStore.Services.Messages
{
	public interface IMessageModelProvider
	{
		void AddGlobalModelParts(MessageContext messageContext);
		void AddModelPart(object part, MessageContext messageContext, string name = null);
		string ResolveModelName(object model);

		TreeNode<ModelTreeMember> BuildModelTree(TemplateModel model);
		TreeNode<ModelTreeMember> GetLastModelTree(string messageTemplateName);
		TreeNode<ModelTreeMember> GetLastModelTree(MessageTemplate template);
	}
}
