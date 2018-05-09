using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Events;
using SmartStore.Services.Messages;
using SmartStore.Services.Common;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Back in stock subscription service
    /// </summary>
    public partial class BackInStockSubscriptionService : IBackInStockSubscriptionService
    {
        #region Fields

        private readonly IRepository<BackInStockSubscription> _backInStockSubscriptionRepository;
        private readonly IWorkflowMessageService _workflowMessageService;
		private readonly IWorkContext _workContext;
        private readonly IEventPublisher _eventPublisher;

        #endregion
        
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="backInStockSubscriptionRepository">Back in stock subscription repository</param>
        /// <param name="workflowMessageService">Workflow message service</param>
		/// <param name="workContext">Work context</param>
        /// <param name="eventPublisher">Event publisher</param>
        public BackInStockSubscriptionService(IRepository<BackInStockSubscription> backInStockSubscriptionRepository,
            IWorkflowMessageService workflowMessageService,
			IWorkContext workContext,
            IEventPublisher eventPublisher)
        {
            this._backInStockSubscriptionRepository = backInStockSubscriptionRepository;
            this._workflowMessageService = workflowMessageService;
			this._workContext = workContext;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete a back in stock subscription
        /// </summary>
        /// <param name="subscription">Subscription</param>
        public virtual void DeleteSubscription(BackInStockSubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            _backInStockSubscriptionRepository.Delete(subscription);

            //event notification
            _eventPublisher.EntityDeleted(subscription);
        }

        /// <summary>
        /// Gets all subscriptions
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
		/// <param name="storeId">Store identifier; pass 0 to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Subscriptions</returns>
        public virtual IPagedList<BackInStockSubscription> GetAllSubscriptionsByCustomerId(int customerId, int storeId, int pageIndex, int pageSize)
        {
            var query = _backInStockSubscriptionRepository.Table;
            //customer
            query = query.Where(biss => biss.CustomerId == customerId);
			//store
			if (storeId > 0)
				query = query.Where(biss => biss.StoreId == storeId);
            //product
            query = query.Where(biss => !biss.Product.Deleted);
            query = query.OrderByDescending(biss => biss.CreatedOnUtc);

            return new PagedList<BackInStockSubscription>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all subscriptions
        /// </summary>
        /// <param name="productId">Product identifier</param>
		/// <param name="storeId">Store identifier; pass 0 to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Subscriptions</returns>
        public virtual IPagedList<BackInStockSubscription> GetAllSubscriptionsByProductId(int productId,
			int storeId, int pageIndex, int pageSize)
        {
            var query = _backInStockSubscriptionRepository.Table;
            //product
            query = query.Where(biss => biss.ProductId == productId);
			//store
			if (storeId > 0)
				query = query.Where(biss => biss.StoreId == storeId);
            //customer
            query = query.Where(biss => !biss.Customer.Deleted);
            query = query.OrderByDescending(biss => biss.CreatedOnUtc);
            return new PagedList<BackInStockSubscription>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all subscriptions
        /// </summary>
        /// <param name="customerId">Customer id</param>
        /// <param name="productId">Product identifier</param>
		/// <param name="storeId">Store identifier</param>
        /// <returns>Subscriptions</returns>
		public virtual BackInStockSubscription FindSubscription(int customerId, int productId, int storeId)
        {
			var query = 
				from biss in _backInStockSubscriptionRepository.Table
				orderby biss.CreatedOnUtc descending
				where biss.CustomerId == customerId &&	biss.ProductId == productId &&	biss.StoreId == storeId
				select biss;

            var subscription = query.FirstOrDefault();
            return subscription;
        }

        /// <summary>
        /// Gets a subscription
        /// </summary>
        /// <param name="subscriptionId">Subscription identifier</param>
        /// <returns>Subscription</returns>
        public virtual BackInStockSubscription GetSubscriptionById(int subscriptionId)
        {
            if (subscriptionId == 0)
                return null;

            var subscription = _backInStockSubscriptionRepository.GetById(subscriptionId);
            return subscription;
        }

        /// <summary>
        /// Inserts subscription
        /// </summary>
        /// <param name="subscription">Subscription</param>
        public virtual void InsertSubscription(BackInStockSubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            _backInStockSubscriptionRepository.Insert(subscription);

            //event notification
            _eventPublisher.EntityInserted(subscription);
        }

        /// <summary>
        /// Updates subscription
        /// </summary>
        /// <param name="subscription">Subscription</param>
        public virtual void UpdateSubscription(BackInStockSubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            _backInStockSubscriptionRepository.Update(subscription);

            //event notification
            _eventPublisher.EntityUpdated(subscription);
        }

        /// <summary>
        /// Send notification to subscribers
        /// </summary>
        /// <param name="product">The product</param>
        /// <returns>Number of sent email</returns>
        public virtual int SendNotificationsToSubscribers(Product product)
        {
			if (product == null)
				throw new ArgumentNullException("product");

            int result = 0;
			var subscriptions = GetAllSubscriptionsByProductId(product.Id, 0, 0, int.MaxValue);
            foreach (var subscription in subscriptions)
            {
                //ensure that customer is registered (simple and fast way)
				if (subscription.Customer.Email.IsEmail())
                {
					var customer = subscription.Customer;
					var customerLanguageId = customer.GetAttribute<int>(SystemCustomerAttributeNames.LanguageId, subscription.StoreId);
					_workflowMessageService.SendBackInStockNotification(subscription, customerLanguageId);
                    result++;
                }
            }
            for (int i = 0; i <= subscriptions.Count - 1; i++)
                DeleteSubscription(subscriptions[i]);
            return result;
        }
        
        #endregion
    }
}
