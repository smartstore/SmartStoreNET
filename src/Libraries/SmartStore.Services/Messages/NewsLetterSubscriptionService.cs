using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Messages;

namespace SmartStore.Services.Messages
{
    public class NewsLetterSubscriptionService : INewsLetterSubscriptionService
    {
        private readonly IRepository<NewsLetterSubscription> _subscriptionRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly ICommonServices _services;

        public NewsLetterSubscriptionService(
            IRepository<NewsLetterSubscription> subscriptionRepository,
            IRepository<Customer> customerRepository,
            ICommonServices services)
        {
            _subscriptionRepository = subscriptionRepository;
            _customerRepository = customerRepository;
            _services = services;
        }

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
                            StoreId = storeId,
                            WorkingLanguageId = _services.WorkContext.WorkingLanguage.Id
                        };
                        InsertNewsLetterSubscription(newsletter);

                        _services.MessageFactory.SendNewsLetterSubscriptionActivationMessage(newsletter, _services.WorkContext.WorkingLanguage.Id);

                        result = true;
                    }
                }
            }
            return result;
        }

        public virtual NewsLetterSubscription GetNewsLetterSubscriptionById(int newsLetterSubscriptionId)
        {
            if (newsLetterSubscriptionId == 0) return null;

            var queuedEmail = _subscriptionRepository.GetById(newsLetterSubscriptionId);
            return queuedEmail;
        }

        public virtual NewsLetterSubscription GetNewsLetterSubscriptionByGuid(Guid newsLetterSubscriptionGuid)
        {
            if (newsLetterSubscriptionGuid == Guid.Empty) return null;

            var newsLetterSubscriptions = from nls in _subscriptionRepository.Table
                                          where nls.NewsLetterSubscriptionGuid == newsLetterSubscriptionGuid
                                          orderby nls.Id
                                          select nls;

            return newsLetterSubscriptions.FirstOrDefault();
        }

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

        public virtual IPagedList<NewsletterSubscriber> GetAllNewsLetterSubscriptions(
            string email,
            int pageIndex,
            int pageSize,
            bool showHidden = false,
            int[] storeIds = null,
            int[] customerRolesIds = null)
        {
            var customerQuery = _customerRepository.Table.Where(x => !x.Deleted);

            // Note, changing the shape makes eager loading for customer entity impossible here.
            var query =
                from ns in _subscriptionRepository.Table
                join c in customerQuery on ns.Email equals c.Email into customers
                from c in customers.DefaultIfEmpty()
                select new NewsletterSubscriber
                {
                    Subscription = ns,
                    Customer = c
                };

            if (email.HasValue())
            {
                query = query.Where(x => x.Subscription.Email.Contains(email));
            }

            if (!showHidden)
            {
                query = query.Where(x => x.Subscription.Active);
            }

            if (storeIds?.Any() ?? false)
            {
                query = query.Where(x => storeIds.Contains(x.Subscription.StoreId));
            }

            if (customerRolesIds?.Any() ?? false)
            {
                query = query.Where(x => x.Customer.CustomerRoleMappings
                    .Where(rm => rm.CustomerRole.Active)
                    .Select(rm => rm.CustomerRoleId)
                    .Intersect(customerRolesIds).Count() > 0);
            }

            query = query
                .OrderBy(x => x.Subscription.Email)
                .ThenBy(x => x.Subscription.StoreId);

            var result = new PagedList<NewsletterSubscriber>(query, pageIndex, pageSize);
            return result;
        }

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