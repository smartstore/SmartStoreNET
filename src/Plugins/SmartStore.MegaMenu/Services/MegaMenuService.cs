using Autofac;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Events;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.MegaMenu.Domain;
using SmartStore.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.MegaMenu.Services
{
    public class MegaMenuService : IMegaMenuService
    {
        /// <summary>
        /// Keys for caching
        /// </summary>
        /// <remarks>
        /// {0} : StoreId
        /// {1} : CustomerRoleId
        /// {2} : LanguageId
        /// </remarks>
        public const string MEGAMENU_MODELS_KEY = "megamenumodels:{0}-{1}-{2}";
        private const string MEGAMENU_MODELS_PATTERN_KEY = "megamenumodels:";

        private readonly IRepository<MegaMenuRecord> _repository;
        private readonly ICategoryService _categoryService;
        private readonly IDbContext _dbContext;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;

        public MegaMenuService(
            IRepository<MegaMenuRecord> repository,
            IDbContext dbContext,
            ICategoryService categoryService,
            ICacheManager cacheManager,
            IEventPublisher eventPublisher)
		{
            _repository = repository;
            _dbContext = dbContext;
            _categoryService = categoryService;
            _cacheManager = cacheManager;
            _eventPublisher = eventPublisher;

            T = NullLocalizer.Instance;
			Logger = NullLogger.Instance;
		}

		public Localizer T { get; set; }
		public ILogger Logger { get; set; }

        public MegaMenuRecord GetMegaMenuRecord(int categoryId)
        {
            if (categoryId == 0)
                return null;

            var record = new MegaMenuRecord();

            var query =
                from gp in _repository.Table
                where gp.CategoryId == categoryId
                select gp;

            record = query.FirstOrDefault();

            return record;
        }

        public IList<MegaMenuRecord> GetMegaMenuRecords(int[] categoryIds)
        {
            if (categoryIds == null || categoryIds.Length == 0)
                return null;

            var record = new MegaMenuRecord();

            var query =
                from x in _repository.Table
                where categoryIds.Contains(x.CategoryId)
                select x;

            return query.ToList();
        }

        public void InsertMegaMenuRecord(MegaMenuRecord record)
        {
            Guard.NotNull(record, nameof(record));

            InvalidateCacheItem();

            _repository.Insert(record);

            //event notification
            _eventPublisher.EntityInserted(record);
        }

        public void UpdateMegaMenuRecord(MegaMenuRecord record)
        {
            Guard.NotNull(record, nameof(record));

            try
            {
                InvalidateCacheItem();

                _repository.Update(record);

                //event notification
                _eventPublisher.EntityUpdated(record);
            }
            catch (Exception ex)
            {
                var exs = ex;
            }
        }

        public void DeleteMegaMenuRecord(MegaMenuRecord record)
        {
            Guard.NotNull(record, nameof(record));

            InvalidateCacheItem();

            _repository.Delete(record);

            //event notification
            _eventPublisher.EntityDeleted(record);
        }

        private void InvalidateCacheItem()
        {
            _cacheManager.RemoveByPattern(MEGAMENU_MODELS_PATTERN_KEY);
        }

        public string GetCacheKey(int storeID, string customerRoleIds, int languageId)
        {
            return MEGAMENU_MODELS_KEY.FormatWith(storeID.ToString(), customerRoleIds, languageId.ToString());
        }
    }
}
