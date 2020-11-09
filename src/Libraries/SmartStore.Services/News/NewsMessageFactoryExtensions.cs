using System;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.News;
using SmartStore.Services.Messages;

namespace SmartStore.Services.News
{
    public static class NewsMessageFactoryExtensions
    {
        /// <summary>
        /// Sends a news comment notification message to a store owner
        /// </summary>
        public static CreateMessageResult SendNewsCommentNotificationMessage(this IMessageFactory factory, NewsComment newsComment, int languageId = 0)
        {
            Guard.NotNull(newsComment, nameof(newsComment));
            return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.NewsCommentStoreOwner, languageId, customer: newsComment.Customer), true, newsComment);
        }
    }
}
