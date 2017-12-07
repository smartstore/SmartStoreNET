using System;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.Messages;

namespace SmartStore.Services.Catalog
{
	public static class CatalogMessageFactoryExtensions
	{
		/// <summary>
		/// Sends "email a friend" message
		/// </summary>
		public static CreateMessageResult SendShareProductMessage(this IMessageFactory factory, Customer customer, Product product,
			string fromEmail, string toEmail, string personalMessage, int languageId = 0)
		{
			Guard.NotNull(customer, nameof(customer));
			Guard.NotNull(product, nameof(product));

			var model = new
			{
				__Name = "Message",
				Body = personalMessage,
				From = fromEmail,
				To = toEmail
			};

			return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.ShareProduct, languageId), true, customer, product, model);
		}

		public static CreateMessageResult SendProductQuestionMessage(this IMessageFactory factory, Customer customer, Product product,
			string senderEmail, string senderName, string senderPhone, string question, int languageId = 0)
		{
			Guard.NotNull(customer, nameof(customer));
			Guard.NotNull(product, nameof(product));

			var model = new
			{
				__Name = "Message",
				Message = question,
				SenderEmail = senderEmail,
				SenderName = senderName,
				SenderPhone = senderPhone
			};

			return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.ProductQuestion, languageId), true, customer, product, model);
		}

		/// <summary>
		/// Sends a product review notification message to a store owner
		/// </summary>
		public static CreateMessageResult SendProductReviewNotificationMessage(this IMessageFactory factory, ProductReview productReview, int languageId = 0)
		{
			Guard.NotNull(productReview, nameof(productReview));
			return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.ProductReviewStoreOwner, languageId), true, productReview, productReview.Customer);
		}

		/// <summary>
		/// Sends a "quantity below" notification to a store owner
		/// </summary>
		public static CreateMessageResult SendQuantityBelowStoreOwnerNotification(this IMessageFactory factory, Product product, int languageId = 0)
		{
			Guard.NotNull(product, nameof(product));
			return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.QuantityBelowStoreOwner, languageId), true, product);
		}

		/// <summary>
		/// Sends a 'Back in stock' notification message to a customer
		/// </summary>
		public static CreateMessageResult SendBackInStockNotification(this IMessageFactory factory, BackInStockSubscription subscription, int languageId = 0)
		{
			Guard.NotNull(subscription, nameof(subscription));
			return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.BackInStockCustomer, languageId, subscription.StoreId), true, subscription, subscription.Customer);
		}
	}
}
