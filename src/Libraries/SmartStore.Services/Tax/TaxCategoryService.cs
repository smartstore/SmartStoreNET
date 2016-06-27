using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Events;

namespace SmartStore.Services.Tax
{
    /// <summary>
    /// Tax category service
    /// </summary>
    public partial class TaxCategoryService : ITaxCategoryService
    {
        #region Constants
        private const string TAXCATEGORIES_ALL_KEY = "SmartStore.taxcategory.all";
        private const string TAXCATEGORIES_PATTERN_KEY = "SmartStore.taxcategory.";
        private const string TAXCATEGORIES_BY_ID_KEY = "SmartStore.taxcategory.id-{0}";
        #endregion

        #region Fields

        private readonly IRepository<TaxCategory> _taxCategoryRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRequestCache _requestCache;

        #endregion

        #region Ctor

        public TaxCategoryService(
			IRequestCache requestCache,
            IRepository<TaxCategory> taxCategoryRepository,
            IEventPublisher eventPublisher)
        {
            _requestCache = requestCache;
            _taxCategoryRepository = taxCategoryRepository;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a tax category
        /// </summary>
        /// <param name="taxCategory">Tax category</param>
        public virtual void DeleteTaxCategory(TaxCategory taxCategory)
        {
            if (taxCategory == null)
                throw new ArgumentNullException("taxCategory");

            _taxCategoryRepository.Delete(taxCategory);

            _requestCache.RemoveByPattern(TAXCATEGORIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(taxCategory);
        }

        /// <summary>
        /// Gets all tax categories
        /// </summary>
        /// <returns>Tax category collection</returns>
        public virtual IList<TaxCategory> GetAllTaxCategories()
        {
            string key = string.Format(TAXCATEGORIES_ALL_KEY);
            return _requestCache.Get(key, () =>
            {
                var query = from tc in _taxCategoryRepository.Table
                            orderby tc.DisplayOrder
                            select tc;
                var taxCategories = query.ToList();
                return taxCategories;
            });
        }

        /// <summary>
        /// Gets a tax category
        /// </summary>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <returns>Tax category</returns>
        public virtual TaxCategory GetTaxCategoryById(int taxCategoryId)
        {
            if (taxCategoryId == 0)
                return null;

            string key = string.Format(TAXCATEGORIES_BY_ID_KEY, taxCategoryId);
            return _requestCache.Get(key, () => 
            { 
                return _taxCategoryRepository.GetById(taxCategoryId); 
            });
        }

        /// <summary>
        /// Inserts a tax category
        /// </summary>
        /// <param name="taxCategory">Tax category</param>
        public virtual void InsertTaxCategory(TaxCategory taxCategory)
        {
            if (taxCategory == null)
                throw new ArgumentNullException("taxCategory");

            _taxCategoryRepository.Insert(taxCategory);

            _requestCache.RemoveByPattern(TAXCATEGORIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(taxCategory);
        }

        /// <summary>
        /// Updates the tax category
        /// </summary>
        /// <param name="taxCategory">Tax category</param>
        public virtual void UpdateTaxCategory(TaxCategory taxCategory)
        {
            if (taxCategory == null)
                throw new ArgumentNullException("taxCategory");

            _taxCategoryRepository.Update(taxCategory);

            _requestCache.RemoveByPattern(TAXCATEGORIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(taxCategory);
        }
        #endregion
    }
}
