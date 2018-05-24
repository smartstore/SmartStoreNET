using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Events;

namespace SmartStore.Services.Messages
{
	public class NewsLetterSubscriptionService : INewsLetterSubscriptionService
    {
        private readonly IRepository<NewsLetterSubscription> _subscriptionRepository;
		private readonly ICommonServices _services;

		public NewsLetterSubscriptionService(IRepository<NewsLetterSubscription> subscriptionRepository, ICommonServices services)
        {
            _subscriptionRepository = subscriptionRepository;
			_services = services;
		}

        /// <summary>
        /// Inserts a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        public void InsertNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
			Guard.NotNull(newsLetterSubscription, nameof(newsLetterSubscription));

			if (newsLetterSubscription.StoreId == 0)
			{
				throw new SmartException("News letter subscription must be assigned to a valid store.");
			}

            // Handle e-mail
            newsLetterSubscription.Email = EnsureSubscriberEmailOrThrow(newsLetterSubscription.Email);

            // Persist
            _subscriptionRepository.Insert(newsLetterSubscription);

            // Publish the subscription event 
            if (newsLetterSubscription.Active)
            {
                PublishSubscriptionEvent(newsLetterSubscription.Email, true, publishSubscriptionEvents);
            }
        }

        /// <summary>
        /// Updates a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        public void UpdateNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
			Guard.NotNull(newsLetterSubscription, nameof(newsLetterSubscription));

			if (newsLetterSubscription.StoreId == 0)
			{
				throw new SmartException("News letter subscription must be assigned to a valid store.");
			}

            // Handle e-mail
            newsLetterSubscription.Email = EnsureSubscriberEmailOrThrow(newsLetterSubscription.Email);

            // Get original subscription record
            var originalSubscription = _services.DbContext.LoadOriginalCopy(newsLetterSubscription);

            // Persist
            _subscriptionRepository.Update(newsLetterSubscription);

            // Publish the subscription event 
            if ((originalSubscription.Active == false && newsLetterSubscription.Active) ||
                (newsLetterSubscription.Active && (originalSubscription.Email != newsLetterSubscription.Email)))
            {
                // If the previous entry was false, but this one is true, publish a subscribe.
                PublishSubscriptionEvent(newsLetterSubscription.Email, true, publishSubscriptionEvents);
            }
            
            if ((originalSubscription.Active && newsLetterSubscription.Active) && 
                (originalSubscription.Email != newsLetterSubscription.Email))
            {
                // If the two emails are different publish an unsubscribe.
                PublishSubscriptionEvent(originalSubscription.Email, false, publishSubscriptionEvents);
            }

            if ((originalSubscription.Active && !newsLetterSubscription.Active))
            {
                // If the previous entry was true, but this one is false
                PublishSubscriptionEvent(originalSubscription.Email, false, publishSubscriptionEvents);
            }
        }

        /// <summary>
        /// Deletes a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        public virtual void DeleteNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
			Guard.NotNull(newsLetterSubscription, nameof(newsLetterSubscription));

            _subscriptionRepository.Delete(newsLetterSubscription);

            //Publish the unsubscribe event 
            PublishSubscriptionEvent(newsLetterSubscription.Email, false, publishSubscriptionEvents);
        }

		public virtual bool? AddNewsLetterSubscriptionFor(bool add, string email, int storeId)
		{
			bool? result = null;

			if (email.IsEmail())
			{
				var newsletter = GetNewsLetterSubscriptionByEmail(email, storeId);
				if (newsletter != null)
				{
					if (add)
					{
						if (!newsletter.Active)
						{
							_services.MessageFactory.SendNewsLetterSubscriptionActivationMessage(newsletter, _services.WorkContext.WorkingLanguage.Id);
						}
						UpdateNewsLetterSubscription(newsletter);
						result = true;
					}
					else
					{
						DeleteNewsLetterSubscription(newsletter);
						result = false;
					}
				}
				else
				{
					if (add)
					{
						newsletter = new NewsLetterSubscription
						{
							NewsLetterSubscriptionGuid = Guid.NewGuid(),
							Email = email,
							Active = false,
							CreatedOnUtc = DateTime.UtcNow,
							StoreId = storeId
						};
						InsertNewsLetterSubscription(newsletter);

						_services.MessageFactory.SendNewsLetterSubscriptionActivationMessage(newsletter, _services.WorkContext.WorkingLanguage.Id);

						result = true;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Gets a newsletter subscription by newsletter subscription identifier
		/// </summary>
		/// <param name="newsLetterSubscriptionId">The newsletter subscription identifier</param>
		/// <returns>NewsLetter subscription</returns>
		public virtual NewsLetterSubscription GetNewsLetterSubscriptionById(int newsLetterSubscriptionId)
        {
            if (newsLetterSubscriptionId == 0) return null;

            var queuedEmail = _subscriptionRepository.GetById(newsLetterSubscriptionId);
            return queuedEmail;
        }

        /// <summary>
        /// Gets a newsletter subscription by newsletter subscription GUID
        /// </summary>
        /// <param name="newsLetterSubscriptionGuid">The newsletter subscription GUID</param>
        /// <returns>NewsLetter subscription</returns>
        public virtual NewsLetterSubscription GetNewsLetterSubscriptionByGuid(Guid newsLetterSubscriptionGuid)
        {
            if (newsLetterSubscriptionGuid == Guid.Empty) return null;

            var newsLetterSubscriptions = from nls in _subscriptionRepository.Table
                                          where nls.NewsLetterSubscriptionGuid == newsLetterSubscriptionGuid
                                          orderby nls.Id
                                          select nls;

            return newsLetterSubscriptions.FirstOrDefault();
        }

        /// <summary>
        /// Gets a newsletter subscription by email
        /// </summary>
        /// <param name="email">The newsletter subscription email</param>
		/// <param name="storeId">The store identifier</param>
        /// <returns>NewsLetter subscription</returns>
        public virtual NewsLetterSubscription GetNewsLetterSubscriptionByEmail(string email, int storeId = 0)
        {
			if (!email.IsEmail())
				return null;

            email = email.Trim();

			var query = _subscriptionRepository.Table
				.Where(x => x.Email == email);

			if (storeId > 0)
				query = query.Where(x => x.StoreId == storeId);

			var subscription = query.OrderBy(x => x.Id).FirstOrDefault();

			return subscription;
        }

        /// <summary>
        /// Gets the newsletter subscription list
        /// </summary>
        /// <param name="email">Email to search or string. Empty to load all records.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
		/// <param name="showHidden">A value indicating whether the not active subscriptions should be loaded</param>
		/// <param name="storeId">The store identifier</param>
        /// <returns>NewsLetterSubscription entity list</returns>
        public virtual IPagedList<NewsLetterSubscription> GetAllNewsLetterSubscriptions(string email, int pageIndex, int pageSize, bool showHidden = false, int storeId = 0)
        {
            var query = _subscriptionRepository.Table;

			if (!String.IsNullOrEmpty(email))
			{
				query = query.Where(nls => nls.Email.Contains(email));
			}
            
			if (!showHidden)
            {
                query = query.Where(nls => nls.Active);
            }

			if (storeId > 0)
			{
				query = query.Where(x => x.StoreId == storeId);
			}

            query = query.OrderBy(nls => nls.Email).ThenBy(x => x.StoreId);

            var newsletterSubscriptions = new PagedList<NewsLetterSubscription>(query, pageIndex, pageSize);
            return newsletterSubscriptions;
        }

        /// <summary>
        /// Publishes the subscription event.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="isSubscribe">if set to <c>true</c> [is subscribe].</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        private void PublishSubscriptionEvent(string email, bool isSubscribe, bool publishSubscriptionEvents)
        {
            if (publishSubscriptionEvents)
            {
				if (isSubscribe)
                {
					_services.EventPublisher.Publish(new EmailSubscribedEvent(email));
				}
                else
                {
					_services.EventPublisher.Publish(new EmailUnsubscribedEvent(email));
				}
            }
        }

		/// <summary>
		/// Ensures the subscriber email or throw.
		/// </summary>
		/// <param name="email">The email.</param>
		/// <returns></returns>
		private static string EnsureSubscriberEmailOrThrow(string email)
		{
			string output = email.EmptyNull().Trim().Truncate(255);

			if (!output.IsEmail())
			{
				throw Error.ArgumentOutOfRange("email", "Email is not valid", email);
			}

			return output;
		}
    }
}