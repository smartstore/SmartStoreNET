using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Events;

namespace SmartStore.Services.Catalog
{
    public partial class ManufacturerTemplateService : IManufacturerTemplateService
    {
        private readonly IRepository<ManufacturerTemplate> _manufacturerTemplateRepository;
        private readonly IEventPublisher _eventPublisher;

        public ManufacturerTemplateService(IRepository<ManufacturerTemplate> manufacturerTemplateRepository, IEventPublisher eventPublisher)
        {
            _manufacturerTemplateRepository = manufacturerTemplateRepository;
            _eventPublisher = eventPublisher;
        }

        public virtual void DeleteManufacturerTemplate(ManufacturerTemplate manufacturerTemplate)
        {
            if (manufacturerTemplate == null)
                throw new ArgumentNullException("manufacturerTemplate");

            _manufacturerTemplateRepository.Delete(manufacturerTemplate);
        }

        public virtual IList<ManufacturerTemplate> GetAllManufacturerTemplates()
        {
            var query = from pt in _manufacturerTemplateRepository.Table
                        orderby pt.DisplayOrder
                        select pt;

            var templates = query.ToList();
            return templates;
        }
 
        public virtual ManufacturerTemplate GetManufacturerTemplateById(int manufacturerTemplateId)
        {
            if (manufacturerTemplateId == 0)
                return null;

            return _manufacturerTemplateRepository.GetById(manufacturerTemplateId);
        }

        public virtual void InsertManufacturerTemplate(ManufacturerTemplate manufacturerTemplate)
        {
            if (manufacturerTemplate == null)
                throw new ArgumentNullException("manufacturerTemplate");

            _manufacturerTemplateRepository.Insert(manufacturerTemplate);
        }

        public virtual void UpdateManufacturerTemplate(ManufacturerTemplate manufacturerTemplate)
        {
            if (manufacturerTemplate == null)
                throw new ArgumentNullException("manufacturerTemplate");

            _manufacturerTemplateRepository.Update(manufacturerTemplate);

        }
    }
}
