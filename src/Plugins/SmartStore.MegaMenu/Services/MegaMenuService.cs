using Autofac;
using SmartStore.Core.Data;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.MegaMenu.Domain;
using SmartStore.Services.Catalog;
using System;
using System.Linq;

namespace SmartStore.MegaMenu.Services
{
    public class MegaMenuService : IMegaMenuService
    {
        private readonly IRepository<MegaMenuRecord> _cbRepository;
        private readonly ICategoryService _categoryService;
        private readonly IDbContext _dbContext;

        public MegaMenuService(
            IRepository<MegaMenuRecord> cbRepository,
            IDbContext dbContext,
            ICategoryService categoryService)
		{
            _cbRepository = cbRepository;
            _dbContext = dbContext;
            _categoryService = categoryService;

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
                from gp in _cbRepository.Table
                where gp.CategoryId == categoryId
                select gp;

            record = query.FirstOrDefault();

            return record ?? new MegaMenuRecord();
        }

        public void InsertMegaMenuRecord(MegaMenuRecord record)
        {
            if (record == null)
                throw new ArgumentNullException("MegaMenuRecord");

            _cbRepository.Insert(record);
        }

        public void UpdateMegaMenuRecord(MegaMenuRecord record)
        {
            if (record == null)
                throw new ArgumentNullException("MegaMenuRecord");

            try
            {
                _cbRepository.Update(record);
            }
            catch (Exception ex)
            {
                var exs = ex;
            }
        }

        public void DeleteMegaMenuRecord(MegaMenuRecord record)
        {
            if (record == null)
                throw new ArgumentNullException("MegaMenuRecord");

            _cbRepository.Delete(record);
        }
    }
}
