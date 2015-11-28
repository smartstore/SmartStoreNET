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
    /// Manufacturer template service
    /// </summary>
    public partial class ManufacturerTemplateService : IManufacturerTemplateService
    {

        #region Fields

        private readonly IRepository<ManufacturerTemplate> _manufacturerTemplateRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

        #endregion
        
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="manufacturerTemplateRepository">Manufacturer template repository</param>
        /// <param name="eventPublisher">Event published</param>
        public ManufacturerTemplateService(ICacheManager cacheManager,
            IRepository<ManufacturerTemplate> manufacturerTemplateRepository,
            IEventPublisher eventPublisher)
        {
            _cacheManager = cacheManager;
            _manufacturerTemplateRepository = manufacturerTemplateRepository;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete manufacturer template
        /// </summary>
        /// <param name="manufacturerTemplate">Manufacturer template</param>
        public virtual void DeleteManufacturerTemplate(ManufacturerTemplate manufacturerTemplate)
        {
            if (manufacturerTemplate == null)
                throw new ArgumentNullException("manufacturerTemplate");

            _manufacturerTemplateRepository.Delete(manufacturerTemplate);

            //event notification
            _eventPublisher.EntityDeleted(manufacturerTemplate);
        }

        /// <summary>
        /// Gets all manufacturer templates
        /// </summary>
        /// <returns>Manufacturer templates</returns>
        public virtual IList<ManufacturerTemplate> GetAllManufacturerTemplates()
        {
            var query = from pt in _manufacturerTemplateRepository.Table
                        orderby pt.DisplayOrder
                        select pt;

            var templates = query.ToList();
            return templates;
        }
 
        /// <summary>
        /// Gets a manufacturer template
        /// </summary>
        /// <param name="manufacturerTemplateId">Manufacturer template identifier</param>
        /// <returns>Manufacturer template</returns>
        public virtual ManufacturerTemplate GetManufacturerTemplateById(int manufacturerTemplateId)
        {
            if (manufacturerTemplateId == 0)
                return null;

            return _manufacturerTemplateRepository.GetById(manufacturerTemplateId);
        }

        /// <summary>
        /// Inserts manufacturer template
        /// </summary>
        /// <param name="manufacturerTemplate">Manufacturer template</param>
        public virtual void InsertManufacturerTemplate(ManufacturerTemplate manufacturerTemplate)
        {
            if (manufacturerTemplate == null)
                throw new ArgumentNullException("manufacturerTemplate");

            _manufacturerTemplateRepository.Insert(manufacturerTemplate);

            //event notification
            _eventPublisher.EntityInserted(manufacturerTemplate);
        }

        /// <summary>
        /// Updates the manufacturer template
        /// </summary>
        /// <param name="manufacturerTemplate">Manufacturer template</param>
        public virtual void UpdateManufacturerTemplate(ManufacturerTemplate manufacturerTemplate)
        {
            if (manufacturerTemplate == null)
                throw new ArgumentNullException("manufacturerTemplate");

            _manufacturerTemplateRepository.Update(manufacturerTemplate);

            //event notification
            _eventPublisher.EntityUpdated(manufacturerTemplate);
        }
        
        #endregion
    }
}
