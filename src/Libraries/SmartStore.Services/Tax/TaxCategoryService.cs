using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Events;
using SmartStore.Data.Caching;

namespace SmartStore.Services.Tax
{
    public partial class TaxCategoryService : ITaxCategoryService
    {
        private readonly IRepository<TaxCategory> _taxCategoryRepository;
        private readonly IEventPublisher _eventPublisher;

        public TaxCategoryService(
            IRepository<TaxCategory> taxCategoryRepository,
            IEventPublisher eventPublisher)
        {
            _taxCategoryRepository = taxCategoryRepository;
            _eventPublisher = eventPublisher;
        }

        public virtual void DeleteTaxCategory(TaxCategory taxCategory)
        {
            if (taxCategory == null)
                throw new ArgumentNullException("taxCategory");

            _taxCategoryRepository.Delete(taxCategory);
        }

        public virtual IList<TaxCategory> GetAllTaxCategories()
        {
			var query = from tc in _taxCategoryRepository.Table
						orderby tc.DisplayOrder
						select tc;

			var taxCategories = query.ToListCached("db.taxcategory.all");
			return taxCategories;
		}

        public virtual TaxCategory GetTaxCategoryById(int taxCategoryId)
        {
            if (taxCategoryId == 0)
                return null;

			return _taxCategoryRepository.GetByIdCached(taxCategoryId, "db.taxcategory.id-" + taxCategoryId);
		}

        public virtual void InsertTaxCategory(TaxCategory taxCategory)
        {
            if (taxCategory == null)
                throw new ArgumentNullException("taxCategory");

            _taxCategoryRepository.Insert(taxCategory);
        }

        public virtual void UpdateTaxCategory(TaxCategory taxCategory)
        {
            if (taxCategory == null)
                throw new ArgumentNullException("taxCategory");

            _taxCategoryRepository.Update(taxCategory);
        }
    }
}
