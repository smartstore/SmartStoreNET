using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;

namespace SmartStore.Services.Directory
{
    /// <summary>
    /// Country service
    /// </summary>
    public partial class CountryService : ICountryService
    {
        #region Constants
        private const string COUNTRIES_ALL_KEY = "SmartStore.country.all-{0}";
        private const string COUNTRIES_BILLING_KEY = "SmartStore.country.billing-{0}";
        private const string COUNTRIES_SHIPPING_KEY = "SmartStore.country.shipping-{0}";
        private const string COUNTRIES_PATTERN_KEY = "SmartStore.country.";
        #endregion
        
        #region Fields
        
        private readonly IRepository<Country> _countryRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;
		private readonly IStoreContext _storeContext;
		private readonly IRepository<StoreMapping> _storeMappingRepository;

        #endregion

        #region Ctor

        public CountryService(ICacheManager cacheManager,
            IRepository<Country> countryRepository,
            IEventPublisher eventPublisher,
			IStoreContext storeContext,
			IRepository<StoreMapping> storeMappingRepository)
        {
            _cacheManager = cacheManager;
            _countryRepository = countryRepository;
            _eventPublisher = eventPublisher;
			_storeContext = storeContext;
			_storeMappingRepository = storeMappingRepository;
        }

		public DbQuerySettings QuerySettings { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a country
        /// </summary>
        /// <param name="country">Country</param>
        public virtual void DeleteCountry(Country country)
        {
            if (country == null)
                throw new ArgumentNullException("country");

            _countryRepository.Delete(country);

            _cacheManager.RemoveByPattern(COUNTRIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(country);
        }

        /// <summary>
        /// Gets all countries
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Country collection</returns>
        public virtual IList<Country> GetAllCountries(bool showHidden = false)
        {
            string key = string.Format(COUNTRIES_ALL_KEY, showHidden);
            return _cacheManager.Get(key, () =>
            {
				var query = _countryRepository.Table;

				if (!showHidden)
					query = query.Where(c => c.Published);

				query = query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name);

				if (!showHidden && !QuerySettings.IgnoreMultiStore)
				{
					var currentStoreId = _storeContext.CurrentStore.Id;
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

        /// <summary>
        /// Gets all countries that allow billing
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Country collection</returns>
        public virtual IList<Country> GetAllCountriesForBilling(bool showHidden = false)
        {
            string key = string.Format(COUNTRIES_BILLING_KEY, showHidden);
            return _cacheManager.Get(key, () =>
            {
				var allCountries = GetAllCountries(showHidden);

				var countries = allCountries.Where(x => x.AllowsBilling).ToList();
                return countries;
            });
        }

        /// <summary>
        /// Gets all countries that allow shipping
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Country collection</returns>
        public virtual IList<Country> GetAllCountriesForShipping(bool showHidden = false)
        {
            string key = string.Format(COUNTRIES_SHIPPING_KEY, showHidden);
            return _cacheManager.Get(key, () =>
            {			
				var allCountries = GetAllCountries(showHidden);

                var countries = allCountries.Where(x => x.AllowsShipping).ToList();
                return countries;
            });
        }

        /// <summary>
        /// Gets a country 
        /// </summary>
        /// <param name="countryId">Country identifier</param>
        /// <returns>Country</returns>
        public virtual Country GetCountryById(int countryId)
        {
            if (countryId == 0)
                return null;

            return _countryRepository.GetById(countryId);
        }

		/// <summary>
		/// Gets a country by two or three letter ISO code
		/// </summary>
		/// <param name="letterIsoCode">Country two or three letter ISO code</param>
		/// <returns>Country</returns>
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

        /// <summary>
        /// Gets a country by two letter ISO code
        /// </summary>
        /// <param name="twoLetterIsoCode">Country two letter ISO code</param>
        /// <returns>Country</returns>
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

        /// <summary>
        /// Gets a country by three letter ISO code
        /// </summary>
        /// <param name="threeLetterIsoCode">Country three letter ISO code</param>
        /// <returns>Country</returns>
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

        /// <summary>
        /// Inserts a country
        /// </summary>
        /// <param name="country">Country</param>
        public virtual void InsertCountry(Country country)
        {
            if (country == null)
                throw new ArgumentNullException("country");

            _countryRepository.Insert(country);

            _cacheManager.RemoveByPattern(COUNTRIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(country);
        }

        /// <summary>
        /// Updates the country
        /// </summary>
        /// <param name="country">Country</param>
        public virtual void UpdateCountry(Country country)
        {
            if (country == null)
                throw new ArgumentNullException("country");

            _countryRepository.Update(country);

            _cacheManager.RemoveByPattern(COUNTRIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(country);
        }

        #endregion
    }
}