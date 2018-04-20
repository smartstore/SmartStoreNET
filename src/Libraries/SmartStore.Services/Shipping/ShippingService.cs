using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Shipping
{
	public partial class ShippingService : IShippingService
    {
		private readonly static object _lock = new object();
		private static IList<Type> _shippingMethodFilterTypes = null;

		private readonly IRepository<ShippingMethod> _shippingMethodRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly IProductAttributeParser _productAttributeParser;
		private readonly IProductService _productService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
		private readonly IGenericAttributeService _genericAttributeService;
        private readonly ShippingSettings _shippingSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly ShoppingCartSettings _shoppingCartSettings;
		private readonly ISettingService _settingService;
		private readonly IProviderManager _providerManager;
		private readonly ITypeFinder _typeFinder;
		private readonly ICommonServices _services;

        public ShippingService(
            IRepository<ShippingMethod> shippingMethodRepository,
			IRepository<StoreMapping> storeMappingRepository,
			IProductAttributeParser productAttributeParser,
			IProductService productService,
            ICheckoutAttributeParser checkoutAttributeParser,
			IGenericAttributeService genericAttributeService,
            ShippingSettings shippingSettings,
            IEventPublisher eventPublisher,
            ShoppingCartSettings shoppingCartSettings,
			ISettingService settingService,
			IProviderManager providerManager,
			ITypeFinder typeFinder,
			ICommonServices services)
        {
            _shippingMethodRepository = shippingMethodRepository;
			_storeMappingRepository = storeMappingRepository;
            _productAttributeParser = productAttributeParser;
			_productService = productService;
            _checkoutAttributeParser = checkoutAttributeParser;
			_genericAttributeService = genericAttributeService;
            _shippingSettings = shippingSettings;
            _eventPublisher = eventPublisher;
            _shoppingCartSettings = shoppingCartSettings;
			_settingService = settingService;
			_providerManager = providerManager;
			_typeFinder = typeFinder;
			_services = services;

			T = NullLocalizer.Instance;
			Logger = NullLogger.Instance;
			QuerySettings = DbQuerySettings.Default;
		}

		public Localizer T { get; set; }
		public ILogger Logger { get; set; }
		public DbQuerySettings QuerySettings { get; set; }

		#region Shipping rate computation methods

		/// <summary>
		/// Load active shipping rate computation methods
		/// </summary>
		/// <param name="storeId">Load records allows only in specified store; pass 0 to load all records</param>
		/// <returns>Shipping rate computation methods</returns>
		public virtual IEnumerable<Provider<IShippingRateComputationMethod>> LoadActiveShippingRateComputationMethods(int storeId = 0)
        {
			var allMethods = LoadAllShippingRateComputationMethods(storeId);

			var activeMethods = allMethods
				.Where(p => p.Value.IsActive && _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Contains(p.Metadata.SystemName, StringComparer.InvariantCultureIgnoreCase));

			if (!activeMethods.Any())
			{
				var fallbackMethod = allMethods.FirstOrDefault(x => x.IsShippingRateComputationMethodActive(_shippingSettings));

				if (fallbackMethod == null)
					fallbackMethod = allMethods.FirstOrDefault();
				
				if (fallbackMethod != null)
				{
					_shippingSettings.ActiveShippingRateComputationMethodSystemNames.Clear();
					_shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add(fallbackMethod.Metadata.SystemName);
					_settingService.SaveSetting(_shippingSettings);

					return new Provider<IShippingRateComputationMethod>[] { fallbackMethod };
				}
				else
				{
					if (DataSettings.DatabaseIsInstalled())
						throw new SmartException(T("Shipping.OneActiveMethodProviderRequired"));
				}
			}

			return activeMethods;
        }

        /// <summary>
        /// Load shipping rate computation method by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found Shipping rate computation method</returns>
		public virtual Provider<IShippingRateComputationMethod> LoadShippingRateComputationMethodBySystemName(string systemName, int storeId = 0)
        {
			return _providerManager.GetProvider<IShippingRateComputationMethod>(systemName, storeId);
        }

        /// <summary>
        /// Load all shipping rate computation methods
        /// </summary>
		/// <param name="storeId">Load records allows only in specified store; pass 0 to load all records</param>
        /// <returns>Shipping rate computation methods</returns>
		public virtual IEnumerable<Provider<IShippingRateComputationMethod>> LoadAllShippingRateComputationMethods(int storeId = 0)
        {
			return _providerManager.GetAllProviders<IShippingRateComputationMethod>(storeId);
        }

        #endregion

        #region Shipping methods


        /// <summary>
        /// Deletes a shipping method
        /// </summary>
        /// <param name="shippingMethod">The shipping method</param>
        public virtual void DeleteShippingMethod(ShippingMethod shippingMethod)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException("shippingMethod");

            _shippingMethodRepository.Delete(shippingMethod);
        }

        /// <summary>
        /// Gets a shipping method
        /// </summary>
        /// <param name="shippingMethodId">The shipping method identifier</param>
        /// <returns>Shipping method</returns>
        public virtual ShippingMethod GetShippingMethodById(int shippingMethodId)
        {
            if (shippingMethodId == 0)
                return null;

            return _shippingMethodRepository.GetById(shippingMethodId);
        }

		public virtual IList<ShippingMethod> GetAllShippingMethods(GetShippingOptionRequest request = null, int storeId = 0)
        {
			var query =
				from sm in _shippingMethodRepository.Table
				select sm;

			if (!QuerySettings.IgnoreMultiStore && storeId > 0)
			{
				query = 
					from x in query
					join sm in _storeMappingRepository.TableUntracked
					on new { c1 = x.Id, c2 = "ShippingMethod" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into x_sm
					from sm in x_sm.DefaultIfEmpty()
					where !x.LimitedToStores || storeId == sm.StoreId
					select x;

				query = 
					from x in query
					group x by x.Id into grp
					orderby grp.Key
					select grp.FirstOrDefault();
			}

			var allMethods = query.OrderBy(x => x.DisplayOrder).ToList();

			if (request == null)
			{
				return allMethods;
			}

			IList<IShippingMethodFilter> allFilters = null;
			var filterRequest = new ShippingFilterRequest {	Option = request };

			var activeShippingMethods = allMethods.Where(s =>
			{
				// Shipping method filtering.
				if (allFilters == null)
					allFilters = GetAllShippingMethodFilters();

				filterRequest.ShippingMethod = s;

				if (allFilters.Any(x => x.IsExcluded(filterRequest)))
					return false;

				return true;
			});

			return activeShippingMethods.ToList();
        }

        /// <summary>
        /// Inserts a shipping method
        /// </summary>
        /// <param name="shippingMethod">Shipping method</param>
        public virtual void InsertShippingMethod(ShippingMethod shippingMethod)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException("shippingMethod");

            _shippingMethodRepository.Insert(shippingMethod);
        }

        /// <summary>
        /// Updates the shipping method
        /// </summary>
        /// <param name="shippingMethod">Shipping method</param>
        public virtual void UpdateShippingMethod(ShippingMethod shippingMethod)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException("shippingMethod");

            _shippingMethodRepository.Update(shippingMethod);
        }

        #endregion

        #region Workflow

		public virtual decimal GetShoppingCartItemWeight(OrganizedShoppingCartItem shoppingCartItem)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");

            var weight = decimal.Zero;

            if (shoppingCartItem.Item.Product != null)
            {
                var attributesTotalWeight = decimal.Zero;

                if (!String.IsNullOrEmpty(shoppingCartItem.Item.AttributesXml))
                {
                    var pvaValues = _productAttributeParser.ParseProductVariantAttributeValues(shoppingCartItem.Item.AttributesXml).ToList();

					foreach (var pvaValue in pvaValues)
					{
						if (pvaValue.ValueType == ProductVariantAttributeValueType.ProductLinkage)
						{
							var linkedProduct = _productService.GetProductById(pvaValue.LinkedProductId);
							if (linkedProduct != null && linkedProduct.IsShipEnabled)
								attributesTotalWeight += (linkedProduct.Weight * pvaValue.Quantity);
						}
						else
						{
							attributesTotalWeight += pvaValue.WeightAdjustment;
						}
					}
                }

                weight = shoppingCartItem.Item.Product.Weight + attributesTotalWeight;
            }
            return weight;
        }

		public virtual decimal GetShoppingCartItemTotalWeight(OrganizedShoppingCartItem shoppingCartItem)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");

            var totalWeight = GetShoppingCartItemWeight(shoppingCartItem) * shoppingCartItem.Item.Quantity;
            return totalWeight;
        }

		public virtual decimal GetShoppingCartTotalWeight(IList<OrganizedShoppingCartItem> cart, bool includeFreeShippingProducts = true)
        {
			var totalWeight = decimal.Zero;
			var customer = cart.GetCustomer();

			// shopping cart items
			foreach (var cartItem in cart)
			{
				var product = cartItem.Item.Product;
				if (product != null)
				{
					if (!includeFreeShippingProducts && product.IsFreeShipping)
					{
						// skip product
					}
					else
					{
						totalWeight += GetShoppingCartItemTotalWeight(cartItem);
					}
				}
			}

            // checkout attributes
            if (customer != null)
            {
				var checkoutAttributesXml = customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService);
				if (!String.IsNullOrEmpty(checkoutAttributesXml))
				{
					var caValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(checkoutAttributesXml);
					foreach (var caValue in caValues)
					{
						totalWeight += caValue.WeightAdjustment;
					}
				}
            }

            return totalWeight;
        }

		public virtual GetShippingOptionRequest CreateShippingOptionRequest(IList<OrganizedShoppingCartItem> cart, Address shippingAddress, int storeId)
        {
            var request = new GetShippingOptionRequest();
			request.StoreId = storeId;
            request.Customer = cart.GetCustomer();
            request.ShippingAddress = shippingAddress;
            request.CountryFrom = null;
            request.StateProvinceFrom = null;
            request.ZipPostalCodeFrom = string.Empty;

			request.Items = new List<OrganizedShoppingCartItem>();

			foreach (var sc in cart)
			{
				if (sc.Item.IsShipEnabled)
					request.Items.Add(sc);
			}

            return request;

        }

		public virtual GetShippingOptionResponse GetShippingOptions(
			IList<OrganizedShoppingCartItem> cart, 
			Address shippingAddress, 
			string allowedShippingRateComputationMethodSystemName = "", 
			int storeId = 0)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            var result = new GetShippingOptionResponse();
            
            //create a package
            var getShippingOptionRequest = CreateShippingOptionRequest(cart, shippingAddress, storeId);

            var shippingRateComputationMethods = LoadActiveShippingRateComputationMethods(storeId)
                .Where(srcm => 
                    String.IsNullOrWhiteSpace(allowedShippingRateComputationMethodSystemName) || 
                    allowedShippingRateComputationMethodSystemName.Equals(srcm.Metadata.SystemName, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            if (shippingRateComputationMethods.Count == 0)
                throw new SmartException(T("Shipping.CouldNotLoadMethod"));

            //get shipping options
            foreach (var srcm in shippingRateComputationMethods)
            {
                var getShippingOptionResponse = srcm.Value.GetShippingOptions(getShippingOptionRequest);
                foreach (var so2 in getShippingOptionResponse.ShippingOptions)
                {
                    //system name
                    so2.ShippingRateComputationMethodSystemName = srcm.Metadata.SystemName;
                    so2.Rate = so2.Rate.RoundIfEnabledFor(_services.WorkContext.WorkingCurrency);

                    result.ShippingOptions.Add(so2);
                }

                //log errors
                if (!getShippingOptionResponse.Success)
                {
					var hasItemsToShip = getShippingOptionRequest.Items != null && getShippingOptionRequest.Items.Count > 0;

					foreach (string error in getShippingOptionResponse.Errors)
                    {
                        result.AddError(error);
						if (hasItemsToShip)
						{
							Logger.Warn(error);
						}
                    }
                }
            }

            if (_shippingSettings.ReturnValidOptionsIfThereAreAny)
            {
                //return valid options if there are any (no matter of the errors returned by other shipping rate compuation methods).
                if (result.ShippingOptions.Count > 0 && result.Errors.Count > 0)
                    result.Errors.Clear();
            }

			//no shipping options loaded
			if (result.ShippingOptions.Count == 0 && result.Errors.Count == 0)
			{
				result.Errors.Add(T("Checkout.ShippingOptionCouldNotBeLoaded"));
			}
            
            return result;
        }

		public virtual IList<IShippingMethodFilter> GetAllShippingMethodFilters()
		{
			if (_shippingMethodFilterTypes == null)
			{
				lock (_lock)
				{
					if (_shippingMethodFilterTypes == null)
					{
						_shippingMethodFilterTypes = _typeFinder.FindClassesOfType<IShippingMethodFilter>(ignoreInactivePlugins: true)
							.ToList();
					}
				}
            }

			var shippingMethodFilters = _shippingMethodFilterTypes
				.Select(x => EngineContext.Current.ContainerManager.ResolveUnregistered(x) as IShippingMethodFilter)
				.ToList();

			return shippingMethodFilters;
        }

        #endregion
    }
}
