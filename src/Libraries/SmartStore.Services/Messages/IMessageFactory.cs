using System;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Email;
using SmartStore.Templating;

namespace SmartStore.Services.Messages
{
    /// <summary>
    /// Contains the result data of a <see cref="IMessageFactory.CreateMessage(MessageContext, bool, object[])"/> call
    /// </summary>
    public class CreateMessageResult
    {
        /// <summary>
        /// The queued email instance which can be saved to the database
        /// </summary>
        public QueuedEmail Email { get; set; }

        /// <summary>
        /// The final model which contains all global and template specific model parts.
        /// </summary>
        public TemplateModel Model { get; set; }

        /// <summary>
        /// The message context used to create the message.
        /// </summary>
        public MessageContext MessageContext { get; set; }
    }

    /// <summary>
    /// Creates and optionally queues email messages
    /// </summary>
    public interface IMessageFactory
    {
        /// <summary>
        /// Creates an email message
        /// </summary>
        /// <param name="messageContext">Contains all data required for creating a message</param>
        /// <param name="queue">If <c>true</c>, the created email message will automatically be queued for sending (saved in database as a <see cref="QueuedEmail"/>)</param>
        /// <param name="modelParts">
        ///	All model objects that are necessary to render the template (in no particular order).
        ///	The passed object instances will be converted to special types which the underlying <see cref="ITemplateEngine"/> can handle.
        ///	<see cref="IMessageModelProvider"/> is responsible for the conversion. See also <seealso cref="IMessageModelProvider.AddModelPart(object, MessageContext, string)"/>.
        /// </param>
        /// <returns>Contains the message creation result.</returns>
        CreateMessageResult CreateMessage(MessageContext messageContext, bool queue, params object[] modelParts);

        /// <summary>
        /// Queues a message created by <see cref="IMessageFactory.CreateMessage(MessageContext, bool, object[])"/>
        /// </summary>
        /// <param name="messageContext">The message context used to create the message.</param>
        /// <param name="queuedEmail">The instance of <see cref="QueuedEmail"/> to queue, e.g. obtained from <see cref="CreateMessageResult.Email"/></param>
        void QueueMessage(MessageContext messageContext, QueuedEmail queuedEmail);

        /// <summary>
        /// Gets an array of suitable test model parts during preview mode. The message template defines
        /// which model part types are required (a comma-separated type list in <see cref="MessageTemplate.ModelTypes"/>).
        /// The framework tries to load a random entity for each defined type from the database. If the table does not contain any records,
        /// <see cref="ITemplateEngine.CreateTestModelFor(Core.BaseEntity, string)"/> gets called internally to obtain a test model wrapper with sample data.
        /// </summary>
        /// <param name="messageContext">The message context used to create the message.</param>
        /// <returns>An array of model parts which can be passed to <see cref="IMessageFactory.CreateMessage(MessageContext, bool, object[])"/></returns>
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
                ["SenderName"] = senderName.HasValue() ? senderName.NullEmpty() : senderEmail.NullEmpty()
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
