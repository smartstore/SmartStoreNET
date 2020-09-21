using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Localization;
using SmartStore.Data.Caching;
using SmartStore.Data.Utilities;
using SmartStore.Services.Catalog;
using SmartStore.Services.Helpers;

namespace SmartStore.Services.Directory
{
    public partial class DeliveryTimeService : IDeliveryTimeService
    {
        // Two letter ISO code to shortest month-day format pattern.
        private readonly static ConcurrentDictionary<string, string> _monthDayFormats = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly IRepository<DeliveryTime> _deliveryTimeRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductVariantAttributeCombination> _attributeCombinationRepository;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly CatalogSettings _catalogSettings;
        private readonly ShippingSettings _shippingSettings;

        public DeliveryTimeService(
            IRepository<DeliveryTime> deliveryTimeRepository,
            IRepository<Product> productRepository,
            IRepository<ProductVariantAttributeCombination> attributeCombinationRepository,
            IDateTimeHelper dateTimeHelper,
            CatalogSettings catalogSettings,
            ShippingSettings shippingSettings)
        {
            _deliveryTimeRepository = deliveryTimeRepository;
            _productRepository = productRepository;
            _attributeCombinationRepository = attributeCombinationRepository;
            _dateTimeHelper = dateTimeHelper;
            _catalogSettings = catalogSettings;
            _shippingSettings = shippingSettings;
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

            return _deliveryTimeRepository.GetByIdCached(deliveryTimeId, "deliverytime-{0}".FormatInvariant(deliveryTimeId));
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

            foreach (var time in deliveryTimes)
            {
                time.IsDefault = time.Equals(deliveryTime) ? true : false;
                _deliveryTimeRepository.Update(time);
            }
        }

        public virtual DeliveryTime GetDefaultDeliveryTime()
        {
            return _deliveryTimeRepository.Table.Where(x => x.IsDefault == true).FirstOrDefault();
        }

        public virtual (DateTime? minDate, DateTime? maxDate) GetDeliveryDate(DeliveryTime deliveryTime)
        {
            var currentDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _dateTimeHelper.DefaultStoreTimeZone);
            return GetDeliveryDate(deliveryTime, currentDate);
        }

        public virtual (DateTime? minDate, DateTime? maxDate) GetDeliveryDate(DeliveryTime deliveryTime, DateTime fromDate)
        {
            var minDate = deliveryTime?.MinDays != null
                ? AddDays(fromDate, deliveryTime.MinDays.Value)
                : (DateTime?)null;

            var maxDate = deliveryTime?.MaxDays != null
                ? AddDays(fromDate, deliveryTime.MaxDays.Value)
                : (DateTime?)null;

            return (minDate, maxDate);
        }

        public virtual string GetFormattedDeliveryDate(DeliveryTime deliveryTime, DateTime? fromDate = null, CultureInfo culture = null)
        {
            if (deliveryTime == null || (!deliveryTime.MinDays.HasValue && !deliveryTime.MaxDays.HasValue))
            {
                return null;
            }

            if (culture == null)
            {
                culture = Thread.CurrentThread.CurrentUICulture;
            }

            var currentDate = fromDate ?? TimeZoneInfo.ConvertTime(DateTime.UtcNow, _dateTimeHelper.DefaultStoreTimeZone);
            var (min, max) = GetDeliveryDate(deliveryTime, currentDate);

            if (min.HasValue)
            {
                min = _dateTimeHelper.ConvertToUserTime(min.Value);
            }
            if (max.HasValue)
            {
                max = _dateTimeHelper.ConvertToUserTime(max.Value);
            }

            // Convention: always separate weekday with comma and format month in shortest form.
            if (min.HasValue && max.HasValue)
            {
                if (min == max)
                {
                    return T("DeliveryTimes.Dates.DeliveryOn", Format(min.Value, "dddd, ", false, "DeliveryTimes.Dates.OnTomorrow"));
                }
                else if (min < max)
                {
                    return T("DeliveryTimes.Dates.Between", 
                        Format(min.Value, "ddd, ", min.Value.Month == max.Value.Month && min.Value.Year == max.Value.Year, null),
                        Format(max.Value, "ddd, ", false, null));
                }
            }
            else if (min.HasValue)
            {
                return T("DeliveryTimes.Dates.NotBefore", Format(min.Value, "dddd, "));
            }
            else if (max.HasValue)
            {
                return T("DeliveryTimes.Dates.Until", Format(max.Value, "dddd, "));
            }

            return null;

            string Format(DateTime date, string patternPrefix, bool noMonth = false, string tomorrowKey = "DeliveryTimes.Dates.Tomorrow")
            {
                // Offer some way to skip our formatting and to force a custom formatting.
                if (!string.IsNullOrEmpty(_shippingSettings.DeliveryTimesDateFormat))
                {
                    return date.ToString(_shippingSettings.DeliveryTimesDateFormat, culture) ?? "-";
                }

                if (tomorrowKey != null && (date - currentDate).TotalDays == 1)
                {
                    return T(tomorrowKey);
                }

                string patternSuffix = null;

                if (noMonth)
                {
                    // MonthDayPattern can contain non-interpreted text like "de", "mh" or even "'d'" (e.g. 21 de septiembre).
                    patternSuffix = _monthDayFormats.GetOrAdd(culture.TwoLetterISOLanguageName + "-nomonth", _ =>
                    {
                        var mdp = culture.DateTimeFormat.MonthDayPattern;
                        return mdp.Contains("d. ") || mdp.Contains("dd. ") ? "d." : "d";
                    });
                }
                else
                {
                    patternSuffix = _monthDayFormats.GetOrAdd(culture.TwoLetterISOLanguageName, _ =>
                    {
                        return culture.DateTimeFormat.MonthDayPattern.Replace("MMMM", "MMM").TrimSafe();
                    });
                }

                return date.ToString(patternPrefix + patternSuffix, culture) ?? "-";
            }
        }

        #region Utilities

        /// <see cref="https://stackoverflow.com/questions/1044688/addbusinessdays-and-getbusinessdays"/>
        /// <seealso cref="https://en.wikipedia.org/wiki/Workweek_and_weekend"/>
        protected virtual DateTime AddDays(DateTime date, int days)
        {
            Guard.NotNegative(days, nameof(days));

            // now.Hour: 0-23. TodayDeliveryHour: 1-24.
            if (_shippingSettings.TodayShipmentHour.HasValue && date.Hour < _shippingSettings.TodayShipmentHour)
            {
                if ((date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday) || !_shippingSettings.DeliveryOnWorkweekDaysOnly)
                {
                    days -= 1;
                }
            }

            // Normalization. Do not support today delivery.
            if (days < 1)
            {
                days = 1;
            }

            if (!_shippingSettings.DeliveryOnWorkweekDaysOnly)
            {
                return date.AddDays(days);
            }

            // Add days for non workweek days.
            if (date.DayOfWeek == DayOfWeek.Saturday)
            {
                date = date.AddDays(2);
                days -= 1;
            }
            else if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
                days -= 1;
            }

            date = date.AddDays(days / 5 * 7);
            int extraDays = days % 5;

            if ((int)date.DayOfWeek + extraDays > 5)
            {
                extraDays += 2;
            }

            return date.AddDays(extraDays);
        }

        #endregion
    }
}