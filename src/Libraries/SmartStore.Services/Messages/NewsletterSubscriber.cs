using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Messages;

namespace SmartStore.Services.Messages
{
    /// <summary>
    /// Represents a newsletter subscriber and associated customer.
    /// </summary>
    public class NewsletterSubscriber
    {
        /// <summary>
        /// Newsletter subscription.
        /// </summary>
        public NewsLetterSubscription Subscription { get; set; }

        /// <summary>
        /// The customer associated with the newsletter subscription. Can be <c>null</c>.
        /// </summary>
        public Customer Customer { get; set; }
    }
}
