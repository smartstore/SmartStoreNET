using System;
using SmartStore.Core.Domain.Messages;

namespace SmartStore.Services.Messages
{
	public class CreateMessageResult
	{
		public QueuedEmail Email { get; set; }
		public TemplateModel Model { get; set; }
	}

	public interface IMessageFactory
	{
		CreateMessageResult CreateMessage(MessageContext messageContext, bool queue, params object[] modelParts);

		void QueueMessage(MessageContext messageContext, QueuedEmail queuedEmail);

		object[] GetTestModels(MessageContext messageContext);
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
