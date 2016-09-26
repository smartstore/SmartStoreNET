using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
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
		#region Fields

		private readonly static object _lock = new object();
		private static IList<Type> _shippingMethodFilterTypes = null;

		private readonly IRepository<ShippingMethod> _shippingMethodRepository;
        private readonly ILogger _logger;
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

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="shippingMethodRepository">Shipping method repository</param>
        /// <param name="logger">Logger</param>
        /// <param name="productAttributeParser">Product attribute parser</param>
		/// <param name="productService">Product service</param>
        /// <param name="checkoutAttributeParser">Checkout attribute parser</param>
		/// <param name="genericAttributeService">Generic attribute service</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="shippingSettings">Shipping settings</param>
        /// <param name="pluginFinder">Plugin finder</param>
        /// <param name="eventPublisher">Event published</param>
        /// <param name="shoppingCartSettings">Shopping cart settings</param>
		/// <param name="settingService">Setting service</param>
        public ShippingService(
            IRepository<ShippingMethod> shippingMethodRepository,
            ILogger logger,
            IProductAttributeParser productAttributeParser,
			IProductService productService,
            ICheckoutAttributeParser checkoutAttributeParser,
			IGenericAttributeService genericAttributeService,
            ShippingSettings shippingSettings,
            IEventPublisher eventPublisher,
            ShoppingCartSettings shoppingCartSettings,
			ISettingService settingService,
			IProviderManager providerManager,
			ITypeFinder typeFinder)
        {
            this._shippingMethodRepository = shippingMethodRepository;
            this._logger = logger;
            this._productAttributeParser = productAttributeParser;
			this._productService = productService;
            this._checkoutAttributeParser = checkoutAttributeParser;
			this._genericAttributeService = genericAttributeService;
            this._shippingSettings = shippingSettings;
            this._eventPublisher = eventPublisher;
            this._shoppingCartSettings = shoppingCartSettings;
			this._settingService = settingService;
			this._providerManager = providerManager;
			this._typeFinder = typeFinder;

			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		#endregion

		#region Methods

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

            //event notification
            _eventPublisher.EntityDeleted(shippingMethod);
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

		public virtual IList<ShippingMethod> GetAllShippingMethods(GetShippingOptionRequest request = null)
        {
			var query =
				from sm in _shippingMethodRepository.Table
				orderby sm.DisplayOrder
				select sm;

			var allMethods = query.ToList();

			if (request == null)
				return allMethods;

			IList<IShippingMethodFilter> allFilters = null;
			var filterRequest = new ShippingFilterRequest {	Option = request };

			var activeShippingMethods = allMethods.Where(s =>
			{
				// shipping method filtering
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

            //event notification
            _eventPublisher.EntityInserted(shippingMethod);
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

            //event notification
            _eventPublisher.EntityUpdated(shippingMethod);
        }

        #endregion

        #region Workflow

        /// <summary>
        /// Gets shopping cart item weight (of one item)
        /// </summary>
        /// <param name="shoppingCartItem">Shopping cart item</param>
        /// <returns>Shopping cart item weight</returns>
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

        /// <summary>
        /// Gets shopping cart item total weight
        /// </summary>
        /// <param name="shoppingCartItem">Shopping cart item</param>
        /// <returns>Shopping cart item weight</returns>
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

        /// <summary>
        /// Create shipment package from shopping cart
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="shippingAddress">Shipping address</param>
		/// <param name="storeId">Store identifier</param>
        /// <returns>Shipment package</returns>
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

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="shippingAddress">Shipping address</param>
        /// <param name="allowedShippingRateComputationMethodSystemName">Filter by shipping rate computation method identifier; null to load shipping options of all shipping rate computation methods</param>
		/// <param name="storeId">Load records allows only in specified store; pass 0 to load all records</param>
        /// <returns>Shipping options</returns>
		public virtual GetShippingOptionResponse GetShippingOptions(IList<OrganizedShoppingCartItem> cart, Address shippingAddress, 
			string allowedShippingRateComputationMethodSystemName = "", int storeId = 0)
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

                    //round
                    if (_shoppingCartSettings.RoundPricesDuringCalculation)
                        so2.Rate = Math.Round(so2.Rate, 2);

                    result.ShippingOptions.Add(so2);
                }

                //log errors
                if (!getShippingOptionResponse.Success)
                {
                    foreach (string error in getShippingOptionResponse.Errors)
                    {
                        result.AddError(error);
						_logger.Warn(string.Concat(srcm.Metadata.FriendlyName, ": ", error));
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

        #endregion
    }
}
