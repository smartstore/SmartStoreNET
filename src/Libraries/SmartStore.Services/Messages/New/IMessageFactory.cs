using System;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;

namespace SmartStore.Services.Messages
{
	public class CreateMessageResult
	{
		public QueuedEmail Email { get; set; }
		public dynamic Model { get; set; }
	}

	public interface IMessageFactory
	{
		CreateMessageResult CreateMessage(MessageContext messageContext, bool queue, params object[] modelParts);

		void QueueMessage(MessageContext messageContext, QueuedEmail queuedEmail, dynamic model);

		IEnumerable<BaseEntity> GetTestEntities(MessageContext messageContext);
	}

	public static class IMessageFactoryExtensions
	{
		/// <summary>
		/// Sends a newsletter subscription activation message
		/// </summary>
		public static CreateMessageResult SendNewsLetterSubscriptionActivationMessage(this IMessageFactory factory, NewsLetterSubscription subscription, int languageId = 0)
		{
			Guard.NotNull(subscription, nameof(subscription));
			return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.NewsLetterSubscriptionActivation, languageId), true, subscription);
		}

		/// <summary>
		/// Sends a newsletter subscription deactivation message
		/// </summary>
		public static CreateMessageResult SendNewsLetterSubscriptionDeactivationMessage(this IMessageFactory factory, NewsLetterSubscription subscription, int languageId = 0)
		{
			Guard.NotNull(subscription, nameof(subscription));
			return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.NewsLetterSubscriptionDeactivation, languageId), true, subscription);
		}
	}
}
