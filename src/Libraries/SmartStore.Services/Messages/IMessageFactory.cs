using System;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Email;

namespace SmartStore.Services.Messages
{
	public class CreateMessageResult
	{
		public QueuedEmail Email { get; set; }
		public TemplateModel Model { get; set; }
		public MessageContext MessageContext { get; set; }
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
		/// Sends the "ContactUs" message to the store owner
		/// </summary>
		public static CreateMessageResult SendContactUsMessage(this IMessageFactory factory, Customer customer,
			string senderEmail, string senderName, string subject, string message, EmailAddress senderEmailAddress, int languageId = 0)
		{
			var model = new NamedModelPart("Message")
			{
				["Subject"] = subject.NullEmpty(),
				["Message"] = message.NullEmpty(),
				["SenderEmail"] = senderEmail.NullEmpty(),
				["SenderName"] = senderName.NullEmpty()
			};

			var messageContext = MessageContext.Create(MessageTemplateNames.SystemContactUs, languageId, customer: customer);
			if (senderEmailAddress != null)
			{
				messageContext.SenderEmailAddress = senderEmailAddress;
			}

			return factory.CreateMessage(messageContext, true, model);
		}

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
