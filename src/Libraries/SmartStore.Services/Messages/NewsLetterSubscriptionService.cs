using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Messages;
using SmartStore.Data;
using SmartStore.Core.Events;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Messages
{

    public class NewsLetterSubscriptionService : INewsLetterSubscriptionService
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IDbContext _context;
        private readonly IRepository<NewsLetterSubscription> _subscriptionRepository;
		private readonly IStoreService _storeService;

        public NewsLetterSubscriptionService(IDbContext context,
			IRepository<NewsLetterSubscription> subscriptionRepository,
			IEventPublisher eventPublisher,
			IStoreService storeService)
        {
            _context = context;
            _subscriptionRepository = subscriptionRepository;
            _eventPublisher = eventPublisher;
			_storeService = storeService;
        }

        public ImportResult ImportSubscribers(Stream stream)
        {
            Guard.ArgumentNotNull(() => stream);

            var result = new ImportResult();
            var toAdd = new List<NewsLetterSubscription>();
            var toUpdate = new List<NewsLetterSubscription>();

			using (var scope = new DbContextScope(ctx: _context, autoDetectChanges: false, proxyCreation: false, validateOnSave: false, autoCommit: false))
			{
				using (var reader = new StreamReader(stream))
				{
					while (!reader.EndOfStream)
					{
						string line = reader.ReadLine();
						if (line.IsEmpty())
						{
							continue;
						}
						string[] tmp = line.Split(',');

						var email = "";
						bool isActive = true;
						int storeId = 0;

						// parse
						if (tmp.Length == 1)
						{
							// "email" only
							email = tmp[0].Trim();
						}
						else if (tmp.Length == 2)
						{
							// "email" and "active" fields specified
							email = tmp[0].Trim();
							isActive = Boolean.Parse(tmp[1].Trim());
						}
						else if (tmp.Length == 3)
						{
							email = tmp[0].Trim();
							isActive = Boolean.Parse(tmp[1].Trim());
							storeId = int.Parse(tmp[2].Trim());
						}
						else
						{
							throw new SmartException("Wrong file format (expected comma separated entries 'Email' and optionally 'IsActive')");
						}

						result.TotalRecords++;

						if (email.Length > 255)
						{
							result.AddWarning("The emal address '{0}' exceeds the maximun allowed length of 255.".FormatInvariant(email));
							continue;
						}

						if (!email.IsEmail())
						{
							result.AddWarning("'{0}' is not a valid email address.".FormatInvariant(email));
							continue;
						}

						if (storeId == 0)
						{
							storeId = _storeService.GetAllStores().First().Id;
						}

						// import
						var subscription = (from nls in _subscriptionRepository.Table
											where nls.Email == email && nls.StoreId == storeId
											orderby nls.Id
											select nls).FirstOrDefault();

						if (subscription != null)
						{
							subscription.Active = isActive;

							toUpdate.Add(subscription);
							result.ModifiedRecords++;
						}
						else
						{
							subscription = new NewsLetterSubscription
							{
								Active = isActive,
								CreatedOnUtc = DateTime.UtcNow,
								Email = email,
								NewsLetterSubscriptionGuid = Guid.NewGuid(),
								StoreId = storeId
							};

							toAdd.Add(subscription);
							result.NewRecords++;
						}
					}
				}

				// insert new subscribers
				_subscriptionRepository.AutoCommitEnabled = true;
				_subscriptionRepository.InsertRange(toAdd, 500);
				toAdd.Clear();

				// update modified subscribers
				_subscriptionRepository.AutoCommitEnabled = null;
				toUpdate.Each(x =>
				{
					_subscriptionRepository.Update(x);
				});
				_subscriptionRepository.Context.SaveChanges();
				toUpdate.Clear();
			}

            return result;
        }

        /// <summary>
        /// Inserts a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        public void InsertNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
            if (newsLetterSubscription == null)
            {
                throw new ArgumentNullException("newsLetterSubscription");
            }

			if (newsLetterSubscription.StoreId == 0)
			{
				throw new SmartException("News letter subscription must be assigned to a valid store.");
			}

            //Handle e-mail
            newsLetterSubscription.Email = EnsureSubscriberEmailOrThrow(newsLetterSubscription.Email);

            //Persist
            _subscriptionRepository.Insert(newsLetterSubscription);

            //Publish the subscription event 
            if (newsLetterSubscription.Active)
            {
                PublishSubscriptionEvent(newsLetterSubscription.Email, true, publishSubscriptionEvents);
            }

            //Publish event
            _eventPublisher.EntityInserted(newsLetterSubscription);
        }

        /// <summary>
        /// Updates a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        public void UpdateNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
            if (newsLetterSubscription == null)
            {
                throw new ArgumentNullException("newsLetterSubscription");
            }

			if (newsLetterSubscription.StoreId == 0)
			{
				throw new SmartException("News letter subscription must be assigned to a valid store.");
			}

            //Handle e-mail
            newsLetterSubscription.Email = EnsureSubscriberEmailOrThrow(newsLetterSubscription.Email);

            //Get original subscription record
            var originalSubscription = _context.LoadOriginalCopy(newsLetterSubscription);

            //Persist
            _subscriptionRepository.Update(newsLetterSubscription);

            //Publish the subscription event 
            if ((originalSubscription.Active == false && newsLetterSubscription.Active) ||
                (newsLetterSubscription.Active && (originalSubscription.Email != newsLetterSubscription.Email)))
            {
                //If the previous entry was false, but this one is true, publish a subscribe.
                PublishSubscriptionEvent(newsLetterSubscription.Email, true, publishSubscriptionEvents);
            }
            
            if ((originalSubscription.Active && newsLetterSubscription.Active) && 
                (originalSubscription.Email != newsLetterSubscription.Email))
            {
                //If the two emails are different publish an unsubscribe.
                PublishSubscriptionEvent(originalSubscription.Email, false, publishSubscriptionEvents);
            }

            if ((originalSubscription.Active && !newsLetterSubscription.Active))
            {
                //If the previous entry was true, but this one is false
                PublishSubscriptionEvent(originalSubscription.Email, false, publishSubscriptionEvents);
            }

            //Publish event
            _eventPublisher.EntityUpdated(newsLetterSubscription);
        }

        /// <summary>
        /// Deletes a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        public virtual void DeleteNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
            if (newsLetterSubscription == null) throw new ArgumentNullException("newsLetterSubscription");

            _subscriptionRepository.Delete(newsLetterSubscription);

            //Publish the unsubscribe event 
            PublishSubscriptionEvent(newsLetterSubscription.Email, false, publishSubscriptionEvents);

            //event notification
            _eventPublisher.EntityDeleted(newsLetterSubscription);
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
                    _eventPublisher.PublishNewsletterSubscribe(email);
                }
                else
                {
                    _eventPublisher.PublishNewsletterUnsubscribe(email);
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