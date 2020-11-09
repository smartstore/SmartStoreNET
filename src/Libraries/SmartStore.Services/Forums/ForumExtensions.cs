using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Html;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Customers;

namespace SmartStore.Services.Forums
{
    public static class ForumExtensions
    {
        /// <summary>
        /// Formats the forum post text
        /// </summary>
        /// <param name="post">Forum post</param>
        /// <returns>Formatted text</returns>
        public static string FormatPostText(this ForumPost post)
        {
            Guard.NotNull(post, nameof(post));

            var text = post.Text;
            if (text.IsEmpty())
            {
                return string.Empty;
            }

            text = HtmlUtils.ConvertPlainTextToHtml(text.HtmlEncode());

            if (EngineContext.Current.Resolve<ForumSettings>().ForumEditor == EditorType.BBCodeEditor)
            {
                text = BBCodeHelper.ToHtml(text);
            }

            return text;
        }

        /// <summary>
        /// Strips the topic subject
        /// </summary>
        /// <param name="topic">Forum topic</param>
        /// <returns>Formatted subject</returns>
        public static string StripTopicSubject(this ForumTopic topic)
        {
            Guard.NotNull(topic, nameof(topic));

            var subject = topic.Subject;
            if (subject.IsEmpty())
            {
                return subject;
            }

            var strippedTopicMaxLength = EngineContext.Current.Resolve<ForumSettings>().StrippedTopicMaxLength;
            if (strippedTopicMaxLength > 0 && subject.Length > strippedTopicMaxLength)
            {
                var index = subject.IndexOf(" ", strippedTopicMaxLength);
                if (index > 0)
                {
                    subject = subject.Substring(0, index);
                    subject += "…";
                }
            }

            return subject;
        }

        /// <summary>
        /// Formats the forum signature text
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>Formatted text</returns>
        public static string FormatForumSignatureText(this string text)
        {
            if (text.IsEmpty())
            {
                return string.Empty;
            }

            return HtmlUtils.ConvertPlainTextToHtml(text.HtmlEncode());
        }

        /// <summary>
        /// Formats the private message text
        /// </summary>
        /// <param name="message">Private message</param>
        /// <returns>Formatted text</returns>
        public static string FormatPrivateMessageText(this PrivateMessage message)
        {
            Guard.NotNull(message, nameof(message));

            var text = message.Text;
            if (text.IsEmpty())
            {
                return string.Empty;
            }

            text = HtmlUtils.ConvertPlainTextToHtml(text.HtmlEncode());
            return BBCodeHelper.ToHtml(text);
        }

        /// <summary>
        /// Get forum last topic
        /// </summary>
        /// <param name="forum">Forum</param>
        /// <param name="forumService">Forum service</param>
        /// <returns>Forum topic</returns>
        public static ForumTopic GetLastTopic(this Forum forum, IForumService forumService)
        {
            Guard.NotNull(forum, nameof(forum));
            Guard.NotNull(forumService, nameof(forumService));

            return forumService.GetTopicById(forum.LastTopicId);
        }

        /// <summary>
        /// Get forum last post
        /// </summary>
        /// <param name="forum">Forum</param>
        /// <param name="forumService">Forum service</param>
        /// <returns>Forum topic</returns>
        public static ForumPost GetLastPost(this Forum forum, IForumService forumService)
        {
            Guard.NotNull(forum, nameof(forum));
            Guard.NotNull(forumService, nameof(forumService));

            return forumService.GetPostById(forum.LastPostId);
        }

        /// <summary>
        /// Get forum last post customer
        /// </summary>
        /// <param name="forum">Forum</param>
        /// <param name="customerService">Customer service</param>
        /// <returns>Customer</returns>
        public static Customer GetLastPostCustomer(this Forum forum, ICustomerService customerService)
        {
            Guard.NotNull(forum, nameof(forum));
            Guard.NotNull(customerService, nameof(customerService));

            return customerService.GetCustomerById(forum.LastPostCustomerId);
        }

        /// <summary>
        /// Get first post
        /// </summary>
        /// <param name="topic">Forum topic</param>
        /// <param name="forumService">Forum service</param>
        /// <returns>Forum post</returns>
        public static ForumPost GetFirstPost(this ForumTopic topic, IForumService forumService)
        {
            Guard.NotNull(topic, nameof(topic));
            Guard.NotNull(forumService, nameof(forumService));

            var posts = forumService.GetAllPosts(topic.Id, 0, true, 0, 1);
            if (posts.Count > 0)
            {
                return posts[0];
            }

            return null;
        }

        /// <summary>
        /// Get last post
        /// </summary>
        /// <param name="topic">Forum topic</param>
        /// <param name="forumService">Forum service</param>
        /// <returns>Forum post</returns>
        public static ForumPost GetLastPost(this ForumTopic topic, IForumService forumService)
        {
            Guard.NotNull(topic, nameof(topic));
            Guard.NotNull(forumService, nameof(forumService));

            return forumService.GetPostById(topic.LastPostId);
        }

        /// <summary>
        /// Get forum last post customer
        /// </summary>
        /// <param name="topic">Forum topic</param>
        /// <param name="customerService">Customer service</param>
        /// <returns>Customer</returns>
        public static Customer GetLastPostCustomer(this ForumTopic topic, ICustomerService customerService)
        {
            Guard.NotNull(topic, nameof(topic));
            Guard.NotNull(customerService, nameof(customerService));

            return customerService.GetCustomerById(topic.LastPostCustomerId);
        }
    }
}
