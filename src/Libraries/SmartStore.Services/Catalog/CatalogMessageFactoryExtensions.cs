using System;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.Messages;
using SmartStore.Services.Common;

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

            var model = new NamedModelPart("Message")
            {
                ["Body"] = personalMessage.NullEmpty(),
                ["From"] = fromEmail.NullEmpty(),
                ["To"] = toEmail.NullEmpty()
            };

            return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.ShareProduct, languageId, customer: customer), true, product, model);
        }

		public static CreateMessageResult SendProductQuestionMessage(this IMessageFactory factory, Customer customer, Product product,
			string senderEmail, string senderName, string senderPhone, string question, 
            string attributes, string productUrl, bool isQuoteRequest, int languageId = 0)
		{
			Guard.NotNull(customer, nameof(customer));
			Guard.NotNull(product, nameof(product));

			var model = new NamedModelPart("Message")
			{
				["ProductUrl"] = productUrl.NullEmpty(),
				["IsQuoteRequest"] = isQuoteRequest,
				["ProductAttributes"] = attributes.NullEmpty(),
				["Message"] = question.NullEmpty(),
				["SenderEmail"] = senderEmail.NullEmpty(),
				["SenderName"] = senderName.NullEmpty(),
				["SenderPhone"] = senderPhone.NullEmpty()
			};

            return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.ProductQuestion, languageId, customer: customer), true, product, model);
        }

        /// <summary>
        /// Sends a product review notification message to a store owner
        /// </summary>
        public static CreateMessageResult SendProductReviewNotificationMessage(this IMessageFactory factory, ProductReview productReview, int languageId = 0)
        {
            Guard.NotNull(productReview, nameof(productReview));
            return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.ProductReviewStoreOwner, languageId, customer: productReview.Customer), true, productReview, productReview.Product);
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
        public static CreateMessageResult SendBackInStockNotification(this IMessageFactory factory, BackInStockSubscription subscription)
        {
            Guard.NotNull(subscription, nameof(subscription));

            var customer = subscription.Customer;
            var languageId = customer.GetAttribute<int>(SystemCustomerAttributeNames.LanguageId);

            return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.BackInStockCustomer, languageId, subscription.StoreId, customer), true, subscription.Product);
        }
    }
}
