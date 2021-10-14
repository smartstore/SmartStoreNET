using System;
using System.Linq;

using Autofac;

using QTRADO.WMAddOn.Domain;

using SmartStore;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Events;
using SmartStore.Services.Catalog;

namespace QTRADO.WMAddOn.Services
{
    public partial class WMAddOnService : IWMAddOnService
    {
        private readonly IRepository<Grossist> _repository;
        private readonly IDbContext _dbContext;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly IEventPublisher _eventPublisher;

        public WMAddOnService(
            IRepository<Grossist> repository,
            IDbContext dbContext,
            AdminAreaSettings adminAreaSettings,
            IEventPublisher eventPublisher,
            IComponentContext ctx)
        {
            _repository = repository;
            _dbContext = dbContext;
            _adminAreaSettings = adminAreaSettings;
            _eventPublisher = eventPublisher;
        }

        public Grossist GetWMAddOnRecord(int entityId, string entityName)
        {
            if (entityId == 0)
                return null;

            var record = new Grossist();

            var query =
                from x in _repository.Table
                    //where x.EntityId == entityId && x.EntityName == entityName
                select x;

            record = query.FirstOrDefault();

            return record;
        }

        public Grossist GetWMAddOnRecordById(int id)
        {
            if (id == 0)
                return null;

            var record = new Grossist();

            var query =
                from x in _repository.Table
                where x.Id == id
                select x;

            record = query.FirstOrDefault();

            return record;
        }

        public void InsertWMAddOnRecord(Grossist record)
        {
            Guard.NotNull(record, nameof(record));

            var utcNow = DateTime.UtcNow;
            record.CreatedOnUtc = utcNow;

            _repository.Insert(record);
        }

        public void UpdateWMAddOnRecord(Grossist record)
        {
            Guard.NotNull(record, nameof(record));

            var utcNow = DateTime.UtcNow;
            record.UpdatedOnUtc = utcNow;

            _repository.Update(record);
        }

        public void DeleteWMAddOnRecord(Grossist record)
        {
            Guard.NotNull(record, nameof(record));

            _repository.Delete(record);
        }
    }
}
