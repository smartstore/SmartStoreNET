using System;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.Messages;

namespace SmartStore.Services.Customers
{
    public static class CustomerMessageFactoryExtensions
    {
        /// <summary>
        /// Sends 'New customer' notification message to a store owner
        /// </summary>
        public static CreateMessageResult SendCustomerRegisteredNotificationMessage(this IMessageFactory factory, Customer customer, int languageId = 0)
        {
            Guard.NotNull(customer, nameof(customer));
            return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.CustomerRegistered, languageId, customer: customer), true);
        }

        /// <summary>
        /// Sends a welcome message to a customer
        /// </summary>
        public static CreateMessageResult SendCustomerWelcomeMessage(this IMessageFactory factory, Customer customer, int languageId = 0)
        {
            Guard.NotNull(customer, nameof(customer));
            return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.CustomerWelcome, languageId, customer: customer), true);
        }

        /// <summary>
        /// Sends an email validation message to a customer
        /// </summary>
        public static CreateMessageResult SendCustomerEmailValidationMessage(this IMessageFactory factory, Customer customer, int languageId = 0)
        {
            Guard.NotNull(customer, nameof(customer));
            return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.CustomerEmailValidation, languageId, customer: customer), true);
        }

        /// <summary>
        /// Sends password recovery message to a customer
        /// </summary>
        public static CreateMessageResult SendCustomerPasswordRecoveryMessage(this IMessageFactory factory, Customer customer, int languageId = 0)
        {
            Guard.NotNull(customer, nameof(customer));
            return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.CustomerPasswordRecovery, languageId, customer: customer), true);
        }

        /// <summary>
        /// Sends wishlist "email a friend" message
        /// </summary>
        public static CreateMessageResult SendShareWishlistMessage(this IMessageFactory factory, Customer customer,
            string fromEmail, string toEmail, string personalMessage, int languageId = 0)
        {
            Guard.NotNull(customer, nameof(customer));

            var model = new NamedModelPart("Wishlist")
            {
                ["PersonalMessage"] = personalMessage,
                ["From"] = fromEmail,
                ["To"] = toEmail
            };

            return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.ShareWishlist, languageId, customer: customer), true, model);
        }

        /// <summary>
        /// Sends a "new VAT sumitted" notification to a store owner
        /// </summary>
        public static CreateMessageResult SendNewVatSubmittedStoreOwnerNotification(this IMessageFactory factory, Customer customer, string vatName, string vatAddress, int languageId = 0)
        {
            Guard.NotNull(customer, nameof(customer));

            var model = new NamedModelPart("VatValidationResult")
            {
                ["Name"] = vatName,
                ["Address"] = vatAddress
            };

            return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.NewVatSubmittedStoreOwner, languageId, customer: customer), true, model);
        }
    }
}
