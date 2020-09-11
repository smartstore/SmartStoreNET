using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Services.Customers
{
    public partial class CustomerContentService : ICustomerContentService
    {
        private readonly IRepository<CustomerContent> _contentRepository;

        public CustomerContentService(IRepository<CustomerContent> contentRepository)
        {
            _contentRepository = contentRepository;
        }

        public virtual void DeleteCustomerContent(CustomerContent content)
        {
            Guard.NotNull(content, nameof(content));

            _contentRepository.Delete(content);
        }

        public virtual IPagedList<CustomerContent> GetAllCustomerContent(
            int customerId,
            bool? approved,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _contentRepository.Table;

            if (approved.HasValue)
                query = query.Where(c => c.IsApproved == approved);
            if (customerId > 0)
                query = query.Where(c => c.CustomerId == customerId);

            var content = new PagedList<CustomerContent>(query, pageIndex, pageSize);
            return content;
        }

        public virtual IPagedList<T> GetAllCustomerContent<T>(
            int customerId,
            bool? approved,
            DateTime? fromUtc = null,
            DateTime? toUtc = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue) where T : CustomerContent
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

            var content = new PagedList<T>(query.OfType<T>(), pageIndex, pageSize);
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
