using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Events;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Category template service
    /// </summary>
    public partial class CategoryTemplateService : ICategoryTemplateService
    {

        #region Fields

        private readonly IRepository<CategoryTemplate> _categoryTemplateRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

        #endregion
        
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="categoryTemplateRepository">Category template repository</param>
        /// <param name="eventPublisher">Event published</param>
        public CategoryTemplateService(ICacheManager cacheManager,
            IRepository<CategoryTemplate> categoryTemplateRepository, IEventPublisher eventPublisher)
        {
            _cacheManager = cacheManager;
            _categoryTemplateRepository = categoryTemplateRepository;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete category template
        /// </summary>
        /// <param name="categoryTemplate">Category template</param>
        public virtual void DeleteCategoryTemplate(CategoryTemplate categoryTemplate)
        {
            if (categoryTemplate == null)
                throw new ArgumentNullException("categoryTemplate");

            _categoryTemplateRepository.Delete(categoryTemplate);

            //event notification
            _eventPublisher.EntityDeleted(categoryTemplate);
        }

        /// <summary>
        /// Gets all category templates
        /// </summary>
        /// <returns>Category templates</returns>
        public virtual IList<CategoryTemplate> GetAllCategoryTemplates()
        {
            var query = from pt in _categoryTemplateRepository.Table
                        orderby pt.DisplayOrder
                        select pt;

            var templates = query.ToList();
            return templates;
        }
 
        /// <summary>
        /// Gets a category template
        /// </summary>
        /// <param name="categoryTemplateId">Category template identifier</param>
        /// <returns>Category template</returns>
        public virtual CategoryTemplate GetCategoryTemplateById(int categoryTemplateId)
        {
            if (categoryTemplateId == 0)
                return null;

            return _categoryTemplateRepository.GetById(categoryTemplateId);
        }

        /// <summary>
        /// Inserts category template
        /// </summary>
        /// <param name="categoryTemplate">Category template</param>
        public virtual void InsertCategoryTemplate(CategoryTemplate categoryTemplate)
        {
            if (categoryTemplate == null)
                throw new ArgumentNullException("categoryTemplate");

            _categoryTemplateRepository.Insert(categoryTemplate);

            //event notification
            _eventPublisher.EntityInserted(categoryTemplate);
        }

        /// <summary>
        /// Updates the category template
        /// </summary>
        /// <param name="categoryTemplate">Category template</param>
        public virtual void UpdateCategoryTemplate(CategoryTemplate categoryTemplate)
        {
            if (categoryTemplate == null)
                throw new ArgumentNullException("categoryTemplate");

            _categoryTemplateRepository.Update(categoryTemplate);

            //event notification
            _eventPublisher.EntityUpdated(categoryTemplate);
        }
        
        #endregion
    }
}
