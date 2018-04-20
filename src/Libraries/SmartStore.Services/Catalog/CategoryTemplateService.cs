using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Events;

namespace SmartStore.Services.Catalog
{
    public partial class CategoryTemplateService : ICategoryTemplateService
    {
        private readonly IRepository<CategoryTemplate> _categoryTemplateRepository;
        private readonly IEventPublisher _eventPublisher;

		public CategoryTemplateService(IRepository<CategoryTemplate> categoryTemplateRepository, IEventPublisher eventPublisher)
        {
            _categoryTemplateRepository = categoryTemplateRepository;
            _eventPublisher = eventPublisher;
        }

        public virtual void DeleteCategoryTemplate(CategoryTemplate categoryTemplate)
        {
            if (categoryTemplate == null)
                throw new ArgumentNullException("categoryTemplate");

            _categoryTemplateRepository.Delete(categoryTemplate);
        }

        public virtual IList<CategoryTemplate> GetAllCategoryTemplates()
        {
            var query = from pt in _categoryTemplateRepository.Table
                        orderby pt.DisplayOrder
                        select pt;

            var templates = query.ToList();
            return templates;
        }
 
        public virtual CategoryTemplate GetCategoryTemplateById(int categoryTemplateId)
        {
            if (categoryTemplateId == 0)
                return null;

            return _categoryTemplateRepository.GetById(categoryTemplateId);
        }

        public virtual void InsertCategoryTemplate(CategoryTemplate categoryTemplate)
        {
            if (categoryTemplate == null)
                throw new ArgumentNullException("categoryTemplate");

            _categoryTemplateRepository.Insert(categoryTemplate);
        }

        public virtual void UpdateCategoryTemplate(CategoryTemplate categoryTemplate)
        {
            if (categoryTemplate == null)
                throw new ArgumentNullException("categoryTemplate");

            _categoryTemplateRepository.Update(categoryTemplate);
        }
    }
}
