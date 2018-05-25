using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Events;

namespace SmartStore.Services.Customers
{
    public partial class CustomerContentService : ICustomerContentService
    {
        private readonly IRepository<CustomerContent> _contentRepository;
        private readonly IEventPublisher _eventPublisher;

        public CustomerContentService(IRepository<CustomerContent> contentRepository, IEventPublisher eventPublisher)
        {
            _contentRepository = contentRepository;
            _eventPublisher = eventPublisher;
        }

        public virtual void DeleteCustomerContent(CustomerContent content)
        {
			Guard.NotNull(content, nameof(content));

            _contentRepository.Delete(content);
        }

        public virtual IList<CustomerContent> GetAllCustomerContent(int customerId, bool? approved)
        {
            var query = from c in _contentRepository.Table
                        orderby c.CreatedOnUtc descending
                        where !approved.HasValue || c.IsApproved == approved &&
                        (customerId == 0 || c.CustomerId == customerId)
                        select c;

            var content = query.ToList();
            return content;
        }

        public virtual IList<T> GetAllCustomerContent<T>(int customerId, bool? approved, DateTime? fromUtc = null, DateTime? toUtc = null) where T : CustomerContent
        {
            var query = _contentRepository.Table;

            if (approved.HasValue)
                query = query.Where(c => c.IsApproved == approved);
            if (customerId > 0)
                query = query.Where(c => c.CustomerId == customerId);
            if (fromUtc.HasValue)
                query = query.Where(c => fromUtc.Value <= c.CreatedOnUtc);
            if (toUtc.HasValue)
                query = query.Where(c => toUtc.Value >= c.CreatedOnUtc);

            query = query.OrderByDescending(c => c.CreatedOnUtc);

            var content = query.OfType<T>().ToList();
            return content;
        }

        public virtual CustomerContent GetCustomerContentById(int contentId)
        {
            if (contentId == 0)
                return null;

            return _contentRepository.GetById(contentId);
                                          
        }

        public virtual void InsertCustomerContent(CustomerContent content)
        {
			Guard.NotNull(content, nameof(content));

			_contentRepository.Insert(content);
        }

        public virtual void UpdateCustomerContent(CustomerContent content)
        {
			Guard.NotNull(content, nameof(content));

			_contentRepository.Update(content);
        }
    }
}
