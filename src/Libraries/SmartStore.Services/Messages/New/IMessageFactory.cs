using System;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Messages;

namespace SmartStore.Services.Messages
{
	public interface IMessageFactory
	{
		(QueuedEmail Email, dynamic Model) CreateMessage(MessageContext messageContext, bool queue, params object[] modelParts);

		void QueueMessage(QueuedEmail queuedEmail, MessageContext messageContext, dynamic model);

		IEnumerable<BaseEntity> GetTestEntities(MessageContext messageContext);
	}

	public static class IMessageFactoryExtensions
	{
		public static (QueuedEmail Email, dynamic Model) QueueCustomerWelcomeMessage(this IMessageFactory factory, MessageContext messageContext, Customer customer)
		{
			Guard.NotNull(customer, nameof(customer));

			return factory.CreateMessage(messageContext, true, customer);
		}

		public static (QueuedEmail Email, dynamic Model) QueueShareProductMessage(this IMessageFactory factory, MessageContext messageContext, 
			Customer customer,
			Product product,
			string senderEmail, string recipientEmail, string personalMessage)
		{
			Guard.NotNull(customer, nameof(customer));
			Guard.NotNull(product, nameof(product));

			var model = new { __Name = "EmailAFriend", PersonalMessage = personalMessage, Email = recipientEmail };

			return factory.CreateMessage(messageContext, true, customer, product, model);
		}
	}
}
