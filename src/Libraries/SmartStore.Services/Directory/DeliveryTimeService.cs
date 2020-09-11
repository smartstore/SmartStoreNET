using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
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
        private readonly IRepository<DeliveryTime> _deliveryTimeRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductVariantAttributeCombination> _attributeCombinationRepository;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkContext _workContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly ShippingSettings _shippingSettings;

        public DeliveryTimeService(
            IRepository<DeliveryTime> deliveryTimeRepository,
            IRepository<Product> productRepository,
            IRepository<ProductVariantAttributeCombination> attributeCombinationRepository,
            IDateTimeHelper dateTimeHelper,
            IWorkContext workContext,
            CatalogSettings catalogSettings,
            ShippingSettings shippingSettings)
        {
            _deliveryTimeRepository = deliveryTimeRepository;
            _productRepository = productRepository;
            _attributeCombinationRepository = attributeCombinationRepository;
            _dateTimeHelper = dateTimeHelper;
            _workContext = workContext;
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

        public virtual string GetFormattedDate(
            DeliveryTime deliveryTime,
            Language language = null,
            bool prependLabel = true)
        {
            if (deliveryTime == null)
            {
                return null;
            }
            if (!deliveryTime.MinDays.HasValue && !deliveryTime.MaxDays.HasValue)
            {
                return null;
            }

            if (language == null)
            {
                language = _workContext.WorkingLanguage;
            }

            CultureInfo ci;
            string result = null;
            var daysToAdd = 0;
            var dateFormat = _shippingSettings.DeliveryTimesDateFormat.NullEmpty() ?? "M";

            // TODO: server's local time is inaccurate (server can be anywhere). Use time at shipping origin address instead (ShippingSettings.ShippingOriginAddressId)?
            var now = DateTime.Now;

            try
            {
                ci = new CultureInfo(language.LanguageCulture);
            }
            catch
            {
                ci = CultureInfo.CurrentCulture;
            }

            // now.Hour: 0-23. TodayDeliveryHour: 1-24.
            if (_shippingSettings.TodayShipmentHour.HasValue && now.Hour < _shippingSettings.TodayShipmentHour)
            {
                daysToAdd -= 1;
            }

            // Normalization. "Today" is not supported\allowed.
            var minDate = deliveryTime.MinDays.HasValue
                ? AddDays(now, Math.Max(deliveryTime.MinDays.Value + daysToAdd, 1))
                : (DateTime?)null;

            var maxDate = deliveryTime.MaxDays.HasValue
                ? AddDays(now, Math.Max(deliveryTime.MaxDays.Value + daysToAdd, 1))
                : (DateTime?)null;

            if (minDate.HasValue && maxDate.HasValue)
            {
                if (minDate == maxDate)
                {
                    if (IsTomorrow(minDate.Value))
                        result = T("Time.Tomorrow");
                    else
                        result = T("DeliveryTimes.Date.DeliveredOn", Format(minDate.Value));
                }
                else if (minDate < maxDate)
                {
                    result = T("DeliveryTimes.Date.Between",
                        IsTomorrow(minDate.Value) ? T("Time.Tomorrow").Text : Format(minDate.Value),
                        Format(maxDate.Value));
                }
            }
            else if (minDate.HasValue)
            {
                if (IsTomorrow(minDate.Value))
                    result = T("Time.Tomorrow");
                else
                    result = T("DeliveryTimes.Date.NotBefore", Format(minDate.Value));
            }
            else if (maxDate.HasValue)
            {
                if (IsTomorrow(maxDate.Value))
                    result = T("Time.Tomorrow");
                else
                    result = T("DeliveryTimes.Date.NotLaterThan", Format(maxDate.Value));
            }

            if (result != null && prependLabel)
            {
                result = T("DeliveryTimes.Date.DeliveryInfo", result);
            }

            return result;

            bool IsTomorrow(DateTime date)
            {
                return (date - now).TotalDays == 1;
            }

            string Format(DateTime date)
            {
                return _dateTimeHelper.ConvertToUserTime(date).ToString(dateFormat, ci);
            }
        }

        #region Utilities

        /// <see cref="https://stackoverflow.com/questions/1044688/addbusinessdays-and-getbusinessdays"/>
        /// <seealso cref="https://en.wikipedia.org/wiki/Workweek_and_weekend"/>
        private DateTime AddDays(DateTime date, int days)
        {
            Guard.NotNegative(days, nameof(days));

            if (days == 0)
            {
                return date;
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

        //private DateTime AddWorkweekDays(DateTime date, int days, DayOfWeek[] nonWorkweekDays)
        //{
        //    var sign = Math.Sign(days);
        //    var unsignedDays = Math.Abs(days);

        //    for (var i = 0; i < unsignedDays; ++i)
        //    {
        //        do
        //        {
        //            date = date.AddDays(sign);
        //        }
        //        while (nonWorkweekDays.Contains(date.DayOfWeek));
        //    }

        //    return date;
        //}

        #endregion
    }
}