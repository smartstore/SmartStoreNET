using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Events;

namespace SmartStore.Services.Messages
{
    public static class EventPublisherExtensions
    {
        /// <summary>
        /// Publishes the newsletter subscribe event.
        /// </summary>
        /// <param name="eventPublisher">The event publisher.</param>
        /// <param name="email">The email.</param>
        public static void PublishNewsletterSubscribe(this IEventPublisher eventPublisher, string email)
        {
            eventPublisher.Publish(new EmailSubscribedEvent(email));
        }

        /// <summary>
        /// Publishes the newsletter unsubscribe event.
        /// </summary>
        /// <param name="eventPublisher">The event publisher.</param>
        /// <param name="email">The email.</param>
        public static void PublishNewsletterUnsubscribe(this IEventPublisher eventPublisher, string email)
        {
            eventPublisher.Publish(new EmailUnsubscribedEvent(email));
        }

        public static void EntityTokensAdded<T, U>(this IEventPublisher eventPublisher, T entity, IList<U> tokens) where T : BaseEntity
        {
            eventPublisher.Publish(new EntityTokensAddedEvent<T, U>(entity, tokens));
        }

        public static void MessageTokensAdded<U>(this IEventPublisher eventPublisher, MessageTemplate message, IList<U> tokens)
        {
            eventPublisher.Publish(new MessageTokensAddedEvent<U>(message, tokens));
        }
    }
}