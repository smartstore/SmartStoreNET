using System;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.Messages;

namespace SmartStore.Services.Blogs
{
    public static class BlogMessageFactoryExtensions
    {
        /// <summary>
        /// Sends a blog comment notification message to a store owner
        /// </summary>
        public static CreateMessageResult SendBlogCommentNotificationMessage(this IMessageFactory factory, BlogComment blogComment, int languageId = 0)
        {
            Guard.NotNull(blogComment, nameof(blogComment));
            return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.BlogCommentStoreOwner, languageId, customer: blogComment.Customer), true, blogComment);
        }
    }
}
