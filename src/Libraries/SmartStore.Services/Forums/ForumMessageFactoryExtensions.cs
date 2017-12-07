using System;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.Messages;

namespace SmartStore.Services.Forums
{
	public static class ForumMessageFactoryExtensions
	{
		/// <summary>
		/// Sends a forum subscription message to a customer
		/// </summary>
		public static CreateMessageResult SendNewForumTopicMessage(this IMessageFactory factory, Customer customer, ForumTopic forumTopic, int languageId = 0)
		{
			Guard.NotNull(customer, nameof(customer));
			Guard.NotNull(forumTopic, nameof(forumTopic));

			return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.NewForumTopic, languageId), true, forumTopic, forumTopic.Forum, customer);
		}

		/// <summary>
		/// Sends a forum subscription message to a customer
		/// </summary>
		public static CreateMessageResult SendNewForumPostMessage(this IMessageFactory factory, Customer customer, ForumPost forumPost, int languageId = 0)
		{
			Guard.NotNull(customer, nameof(customer));
			Guard.NotNull(forumPost, nameof(forumPost));

			return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.NewForumPost, languageId), true, forumPost, forumPost.ForumTopic, forumPost.ForumTopic.Forum, customer);
		}

		/// <summary>
		/// Sends a private message notification
		/// </summary>
		public static CreateMessageResult SendPrivateMessageNotification(this IMessageFactory factory, Customer customer, PrivateMessage privateMessage, int languageId = 0)
		{
			Guard.NotNull(customer, nameof(customer));
			Guard.NotNull(privateMessage, nameof(privateMessage));

			return factory.CreateMessage(MessageContext.Create(MessageTemplateNames.NewPrivateMessage, languageId, privateMessage.StoreId), true, privateMessage, customer);
		}
	}
}
