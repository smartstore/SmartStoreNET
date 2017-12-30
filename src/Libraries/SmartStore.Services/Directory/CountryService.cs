using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Templating;
using SmartStore.Services.Messages;
using System.Globalization;

namespace SmartStore.Services.Directory
{
    public partial class CountryService : ICountryService
    {
        private const string COUNTRIES_ALL_KEY = "SmartStore.country.all-{0}";
        private const string COUNTRIES_BILLING_KEY = "SmartStore.country.billing-{0}";
        private const string COUNTRIES_SHIPPING_KEY = "SmartStore.country.shipping-{0}";
        private const string COUNTRIES_PATTERN_KEY = "SmartStore.country.*";

		private readonly ICommonServices _services;
		private readonly IRepository<Country> _countryRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly ITemplateEngine _templateEngine;
		private readonly IMessageModelProvider _messageModelProvider;

		public CountryService(
			ICommonServices services,
            IRepository<Country> countryRepository,
			IRepository<StoreMapping> storeMappingRepository,
			ITemplateEngine templateEngine,
			IMessageModelProvider messageModelProvider)
        {
            _countryRepository = countryRepository;
			_services = services;
			_storeMappingRepository = storeMappingRepository;
			_templateEngine = templateEngine;
			_messageModelProvider = messageModelProvider;
        }

		public DbQuerySettings QuerySettings { get; set; }

        public virtual void DeleteCountry(Country country)
        {
			Guard.NotNull(country, nameof(country));

			_countryRepository.Delete(country);
			
            _services.RequestCache.RemoveByPattern(COUNTRIES_PATTERN_KEY);
        }

        public virtual IList<Country> GetAllCountries(bool showHidden = false)
        {
            string key = string.Format(COUNTRIES_ALL_KEY, showHidden);
            return _services.RequestCache.Get(key, () =>
            {
				var query = _countryRepository.Table;

				if (!showHidden)
					query = query.Where(c => c.Published);

				query = query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name);

				if (!showHidden && !QuerySettings.IgnoreMultiStore)
				{
					var currentStoreId = _services.StoreContext.CurrentStore.Id;
					query = from c in query
							join sc in _storeMappingRepository.Table
							on new { c1 = c.Id, c2 = "Country" } equals new { c1 = sc.EntityId, c2 = sc.EntityName } into c_sm
							from sc in c_sm.DefaultIfEmpty()
							where !c.LimitedToStores || currentStoreId == sc.StoreId
							select c;

					query = from c in query
							group c by c.Id into cGroup
							orderby cGroup.Key
							select cGroup.FirstOrDefault();

					query = query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name);
				}

                var countries = query.ToList();
                return countries;
            });
        }

        public virtual IList<Country> GetAllCountriesForBilling(bool showHidden = false)
        {
            string key = string.Format(COUNTRIES_BILLING_KEY, showHidden);
            return _services.RequestCache.Get(key, () =>
            {
				var allCountries = GetAllCountries(showHidden);

				var countries = allCountries.Where(x => x.AllowsBilling).ToList();
                return countries;
            });
        }

        public virtual IList<Country> GetAllCountriesForShipping(bool showHidden = false)
        {
            string key = string.Format(COUNTRIES_SHIPPING_KEY, showHidden);
            return _services.RequestCache.Get(key, () =>
            {			
				var allCountries = GetAllCountries(showHidden);

                var countries = allCountries.Where(x => x.AllowsShipping).ToList();
                return countries;
            });
        }

        public virtual Country GetCountryById(int countryId)
        {
            if (countryId == 0)
                return null;

            return _countryRepository.GetById(countryId);
        }

		public virtual Country GetCountryByTwoOrThreeLetterIsoCode(string letterIsoCode)
		{
			if (letterIsoCode.HasValue())
			{
				if (letterIsoCode.Length == 2)
					return GetCountryByTwoLetterIsoCode(letterIsoCode);
				else if (letterIsoCode.Length == 3)
					return GetCountryByThreeLetterIsoCode(letterIsoCode);
			}
			return null;
		}

        public virtual Country GetCountryByTwoLetterIsoCode(string twoLetterIsoCode)
        {
			if (twoLetterIsoCode.IsEmpty())
				return null;

            var query = from c in _countryRepository.Table
                        where c.TwoLetterIsoCode == twoLetterIsoCode
                        select c;

            var country = query.FirstOrDefault();
            return country;
        }

        public virtual Country GetCountryByThreeLetterIsoCode(string threeLetterIsoCode)
        {
			if (threeLetterIsoCode.IsEmpty())
				return null;

            var query = from c in _countryRepository.Table
                        where c.ThreeLetterIsoCode == threeLetterIsoCode
                        select c;

            var country = query.FirstOrDefault();
            return country;
        }

        public virtual void InsertCountry(Country country)
        {
			Guard.NotNull(country, nameof(country));

			_countryRepository.Insert(country);

			_services.RequestCache.RemoveByPattern(COUNTRIES_PATTERN_KEY);
        }

        public virtual void UpdateCountry(Country country)
        {
			Guard.NotNull(country, nameof(country));

            _countryRepository.Update(country);

			_services.RequestCache.RemoveByPattern(COUNTRIES_PATTERN_KEY);
        }

		public virtual string FormatAddress(Address address, bool newLineToBr = false)
		{
			Guard.NotNull(address, nameof(address));

			var template = address.Country?.AddressFormat.NullEmpty() ?? Address.DefaultAddressFormat;

			var messageContext = new MessageContext
			{
				Language = _services.WorkContext.WorkingLanguage,
				Store = _services.StoreContext.CurrentStore,
				Model = new TemplateModel()
			};

			_messageModelProvider.AddModelPart(address, messageContext, "Address");
			var model = messageContext.Model["Address"];
			var result = _templateEngine.Render(template, model, messageContext.FormatProvider).Compact(true);

			if (newLineToBr)
			{
				result = Core.Html.HtmlUtils.ConvertPlainTextToHtml(result);
			}		

			return result;
		}

		public virtual string FormatAddress(object address, Country country = null, IFormatProvider formatProvider = null)
		{
			Guard.NotNull(address, nameof(address));

			var template = country?.AddressFormat.NullEmpty() ?? Address.DefaultAddressFormat;
			var result = _templateEngine.Render(template, address, formatProvider ?? CultureInfo.CurrentCulture).Compact(true);

			return result;
		}
	}
}