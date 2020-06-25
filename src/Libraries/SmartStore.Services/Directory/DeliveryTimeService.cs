using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Localization;
using SmartStore.Data.Caching;
using SmartStore.Data.Utilities;
using SmartStore.Services.Catalog;

namespace SmartStore.Services.Directory
{
    public partial class DeliveryTimeService : IDeliveryTimeService
    {
        private readonly IRepository<DeliveryTime> _deliveryTimeRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductVariantAttributeCombination> _attributeCombinationRepository;
		private readonly CatalogSettings _catalogSettings;

        public DeliveryTimeService(
            IRepository<DeliveryTime> deliveryTimeRepository,
            IRepository<Product> productRepository,
            IRepository<ProductVariantAttributeCombination> attributeCombinationRepository,
			CatalogSettings catalogSettings)
        {
            _deliveryTimeRepository = deliveryTimeRepository;
            _productRepository = productRepository;
            _attributeCombinationRepository = attributeCombinationRepository;
			_catalogSettings = catalogSettings;
		}

		public Localizer T { get; set; } = NullLocalizer.Instance;

        public virtual void DeleteDeliveryTime(DeliveryTime deliveryTime)
        {
            if (deliveryTime == null)
            {
                return;
            }

            // Remove associations to deleted products.
            using (var scope = new DbContextScope(_productRepository.Context, autoDetectChanges: false, validateOnSave: false, hooksEnabled: false, autoCommit: false))
            {
                var productsQuery = _productRepository.Table.Where(x => x.Deleted && x.DeliveryTimeId == deliveryTime.Id);
                var productsPager = new FastPager<Product>(productsQuery, 500);

                while (productsPager.ReadNextPage(out var products))
                {
                    if (products.Any())
                    {
                        products.Each(x => x.DeliveryTimeId = null);
                        scope.Commit();
                    }
                }

                var attributeCombinationQuery =
                    from ac in _attributeCombinationRepository.Table
                    join p in _productRepository.Table on ac.ProductId equals p.Id
                    where p.Deleted && ac.DeliveryTimeId == deliveryTime.Id
                    select ac;
                var attributeCombinationPager = new FastPager<ProductVariantAttributeCombination>(attributeCombinationQuery, 1000);

                while (attributeCombinationPager.ReadNextPage(out var attributeCombinations))
                {
                    if (attributeCombinations.Any())
                    {
                        attributeCombinations.Each(x => x.DeliveryTimeId = null);
                        scope.Commit();
                    }
                }
            }

            // Warn if there are associations to active products.
            if (IsAssociated(deliveryTime.Id))
            {
                throw new SmartException(T("Admin.Configuration.DeliveryTimes.CannotDeleteAssignedProducts"));
            }

            _deliveryTimeRepository.Delete(deliveryTime);
        }

        public virtual bool IsAssociated(int deliveryTimeId)
        {
            if (deliveryTimeId == 0)
                return false;

            var query = 
				from p in _productRepository.Table
				where p.DeliveryTimeId == deliveryTimeId || p.ProductVariantAttributeCombinations.Any(c => c.DeliveryTimeId == deliveryTimeId)
				select p.Id;

            return query.Count() > 0;
        }

        public virtual DeliveryTime GetDeliveryTimeById(int deliveryTimeId)
        {
            if (deliveryTimeId == 0)
            {
                if (_catalogSettings.ShowDefaultDeliveryTime)
                {
                    return GetDefaultDeliveryTime();
                }
                else
                {
                    return null;
                }
            }
            
            return  _deliveryTimeRepository.GetByIdCached(deliveryTimeId, "deliverytime-{0}".FormatInvariant(deliveryTimeId));
        }

        public virtual DeliveryTime GetDeliveryTime(Product product)
		{
            var deliveryTimeId = product.GetDeliveryTimeIdAccordingToStock(_catalogSettings);
            return GetDeliveryTimeById(deliveryTimeId ?? 0);
		}

        public virtual IList<DeliveryTime> GetAllDeliveryTimes()
        {
			var query = _deliveryTimeRepository.Table.OrderBy(c => c.DisplayOrder);
			var deliveryTimes = query.ToListCached("db.delivtimes.all");
			return deliveryTimes;
		}

        public virtual void InsertDeliveryTime(DeliveryTime deliveryTime)
        {
            Guard.NotNull(deliveryTime, nameof(deliveryTime));

            _deliveryTimeRepository.Insert(deliveryTime);
        }

        public virtual void UpdateDeliveryTime(DeliveryTime deliveryTime)
        {
            Guard.NotNull(deliveryTime, nameof(deliveryTime));

            _deliveryTimeRepository.Update(deliveryTime);
        }

        public virtual void SetToDefault(DeliveryTime deliveryTime)
        {
            Guard.NotNull(deliveryTime, nameof(deliveryTime));

            var deliveryTimes = GetAllDeliveryTimes();

            foreach(var time in deliveryTimes)
            {
                time.IsDefault = time.Equals(deliveryTime) ? true : false;
                _deliveryTimeRepository.Update(time);
            }
        }
        
        public virtual DeliveryTime GetDefaultDeliveryTime()
        {
            return _deliveryTimeRepository.Table.Where(x => x.IsDefault == true).FirstOrDefault();
        }
    }
}