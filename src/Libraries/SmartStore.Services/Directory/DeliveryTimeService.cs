using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Localization;
using SmartStore.Data.Caching;
using SmartStore.Data.Utilities;
using SmartStore.Services.Catalog;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Directory
{
    public partial class DeliveryTimeService : IDeliveryTimeService
    {
        private readonly IRepository<DeliveryTime> _deliveryTimeRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductVariantAttributeCombination> _attributeCombinationRepository;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly CatalogSettings _catalogSettings;

        public DeliveryTimeService(
            IRepository<DeliveryTime> deliveryTimeRepository,
            IRepository<Product> productRepository,
            IRepository<ProductVariantAttributeCombination> attributeCombinationRepository,
            IDateTimeHelper dateTimeHelper,
            CatalogSettings catalogSettings)
        {
            _deliveryTimeRepository = deliveryTimeRepository;
            _productRepository = productRepository;
            _attributeCombinationRepository = attributeCombinationRepository;
            _dateTimeHelper = dateTimeHelper;
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

        public virtual string FormatDeliveryTime(
            DeliveryTime deliveryTime,
            Language language,
            string dateFormat = "M",
            string delimiter = " ")
        {
            Guard.NotNull(language, nameof(language));
            Guard.NotEmpty(dateFormat, nameof(dateFormat));
            Guard.NotNull(delimiter, nameof(delimiter));

            if (deliveryTime == null)
            {
                return null;
            }

            string result = null;
            CultureInfo ci;

            try
            {
                ci = new CultureInfo(language.LanguageCulture);
            }
            catch
            {
                ci = CultureInfo.CurrentCulture;
            }


            switch (_catalogSettings.DeliveryTimesPresentation)
            {
                case DeliveryTimesPresentation.LabelAndDate:
                    result = deliveryTime.GetLocalized(x => x.Name, language).Value.Grow(GetFormattedDate(), delimiter);
                    break;
                case DeliveryTimesPresentation.LabelOnly:
                    result = deliveryTime.GetLocalized(x => x.Name, language);
                    break;
                case DeliveryTimesPresentation.DateOnly:
                default:
                    result = GetFormattedDate();
                    break;
            }

            // Fallback.
            if (string.IsNullOrEmpty(result))
            {
                result = deliveryTime.GetLocalized(x => x.Name, language);
            }

            return result;

            string GetFormattedDate()
            {
                var minDays = deliveryTime.MinDays ?? 0;
                var maxDays = deliveryTime.MaxDays ?? 0;

                var dtMin = minDays > 0
                    ? _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow.AddDays(minDays))
                    : (DateTime?)null;

                var dtMax = maxDays > 0
                    ? _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow.AddDays(maxDays))
                    : (DateTime?)null;

                if (dtMin.HasValue && dtMax.HasValue)
                {
                    if (minDays == maxDays)
                    {
                        return T("DeliveryTimes.Date.DeliveredOn", dtMin.Value.ToString(dateFormat, ci));
                    }
                    else if (minDays < maxDays)
                    {
                        return T("DeliveryTimes.Date.Between", dtMin.Value.ToString(dateFormat), dtMax.Value.ToString(dateFormat));
                    }
                }
                else if (dtMin.HasValue)
                {
                    return T("DeliveryTimes.Date.EarliestOn", dtMin.Value.ToString(dateFormat, ci));
                }
                else if (dtMax.HasValue)
                {
                    return T("DeliveryTimes.Date.LatestOn", dtMax.Value.ToString(dateFormat, ci));
                }

                return null;
            }
        }
    }
}