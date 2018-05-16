using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Html;
using SmartStore.Core.Logging;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Models.Checkout;
using SmartStore.Web.Models.Common;

namespace SmartStore.Web.Controllers
{
    [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
    public partial class CheckoutController : PublicControllerBase
    {
		#region Fields

        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IShippingService _shippingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IOrderService _orderService;
        private readonly IWebHelper _webHelper;
        private readonly HttpContextBase _httpContext;
		private readonly ISettingService _settingService;
        private readonly OrderSettings _orderSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly AddressSettings _addressSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
		private readonly PluginMediator _pluginMediator;

        #endregion

		#region Constructors

		public CheckoutController(IWorkContext workContext, IStoreContext storeContext,
            IShoppingCartService shoppingCartService, ILocalizationService localizationService, 
            ITaxService taxService, ICurrencyService currencyService, 
            IPriceFormatter priceFormatter, IOrderProcessingService orderProcessingService,
            ICustomerService customerService,  IGenericAttributeService genericAttributeService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService, IShippingService shippingService,
			IPaymentService paymentService, 
			IOrderTotalCalculationService orderTotalCalculationService,
            IOrderService orderService, IWebHelper webHelper,
            HttpContextBase httpContext, IMobileDeviceHelper mobileDeviceHelper,
            OrderSettings orderSettings, 
            PaymentSettings paymentSettings, AddressSettings addressSettings,
            ShoppingCartSettings shoppingCartSettings, ShippingSettings shippingSettings,
			ISettingService settingService, PluginMediator pluginMediator)
        {
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._shoppingCartService = shoppingCartService;
            this._localizationService = localizationService;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            this._orderProcessingService = orderProcessingService;
            this._customerService = customerService;
            this._genericAttributeService = genericAttributeService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._shippingService = shippingService;
            this._paymentService = paymentService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._orderService = orderService;
            this._webHelper = webHelper;
            this._httpContext = httpContext;
			this._settingService = settingService;
            this._orderSettings = orderSettings;
            this._paymentSettings = paymentSettings;
            this._addressSettings = addressSettings;
            this._shippingSettings = shippingSettings;
            this._shoppingCartSettings = shoppingCartSettings;
			this._pluginMediator = pluginMediator;
        }

        #endregion

        #region Utilities

        [NonAction]
		protected bool IsPaymentWorkflowRequired(IList<OrganizedShoppingCartItem> cart, bool ignoreRewardPoints = false)
        {
            // Check whether order total equals zero
            decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart, ignoreRewardPoints);

            if (shoppingCartTotalBase.HasValue && shoppingCartTotalBase.Value == decimal.Zero)
                return false;

			if (_httpContext.GetCheckoutState().IsPaymentSelectionSkipped)
				return false;

            return true;
        }

        [NonAction]
        protected CheckoutBillingAddressModel PrepareBillingAddressModel(int? selectedCountryId = null)
        {
            var model = new CheckoutBillingAddressModel();
            //existing addresses
            var addresses = _workContext.CurrentCustomer.Addresses.Where(a => a.Country == null || a.Country.AllowsBilling).ToList();
            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                addressModel.PrepareModel(address, 
                    false, 
                    _addressSettings);
                model.ExistingAddresses.Add(addressModel);
            }

            //new address
            model.NewAddress.CountryId = selectedCountryId;
            model.NewAddress.PrepareModel(null,
                false,
                _addressSettings,
                _localizationService,
                _stateProvinceService,
                () => _countryService.GetAllCountriesForBilling());
            return model;
        }

        [NonAction]
        protected CheckoutShippingAddressModel PrepareShippingAddressModel(int? selectedCountryId = null)
        {
            var model = new CheckoutShippingAddressModel();
            //existing addresses
            var addresses = _workContext.CurrentCustomer.Addresses.Where(a => a.Country == null || a.Country.AllowsShipping).ToList();
            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                addressModel.PrepareModel(address,
                    false,
                    _addressSettings);
                model.ExistingAddresses.Add(addressModel);
            }

            //new address
            model.NewAddress.CountryId = selectedCountryId;
            model.NewAddress.PrepareModel(null,
                false,
                _addressSettings,
                _localizationService,
                _stateProvinceService,
                () => _countryService.GetAllCountriesForShipping());
            return model;
        }

		[NonAction]
		protected CheckoutShippingMethodModel PrepareShippingMethodModel(IList<OrganizedShoppingCartItem> cart)
		{
			var model = new CheckoutShippingMethodModel();

			var store = _storeContext.CurrentStore;
			var customer = _workContext.CurrentCustomer;
			var getShippingOptionResponse = _shippingService.GetShippingOptions(cart, customer.ShippingAddress, "", store.Id);

			if (getShippingOptionResponse.Success)
			{
				// Performance optimization. cache returned shipping options.
				// We'll use them later (after a customer has selected an option).
				_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.OfferedShippingOptions, getShippingOptionResponse.ShippingOptions, store.Id);

				var shippingMethods = _shippingService.GetAllShippingMethods(null, store.Id);

				foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
				{
					var soModel = new CheckoutShippingMethodModel.ShippingMethodModel
					{
						ShippingMethodId = shippingOption.ShippingMethodId,
						Name = shippingOption.Name,
						Description = shippingOption.Description,
						ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName,
					};

					var srcmProvider = _shippingService.LoadShippingRateComputationMethodBySystemName(shippingOption.ShippingRateComputationMethodSystemName);
					if (srcmProvider != null)
					{
						soModel.BrandUrl = _pluginMediator.GetBrandImageUrl(srcmProvider.Metadata);
					}

					// Adjust rate.
					Discount appliedDiscount = null;
					var shippingTotal = _orderTotalCalculationService.AdjustShippingRate(shippingOption.Rate, cart, shippingOption, shippingMethods, out appliedDiscount);
					decimal rateBase = _taxService.GetShippingPrice(shippingTotal, customer);
					decimal rate = _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
					soModel.FeeRaw = rate;
					soModel.Fee = _priceFormatter.FormatShippingPrice(rate, true);

					model.ShippingMethods.Add(soModel);
				}

				// Find a selected (previously) shipping method.
				var selectedShippingOption = customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, store.Id);
				if (selectedShippingOption != null)
				{
					var shippingOptionToSelect = model.ShippingMethods
						.ToList()
						.Find(so => !String.IsNullOrEmpty(so.Name) && so.Name.Equals(selectedShippingOption.Name, StringComparison.InvariantCultureIgnoreCase) &&
						!String.IsNullOrEmpty(so.ShippingRateComputationMethodSystemName) &&
						so.ShippingRateComputationMethodSystemName.Equals(selectedShippingOption.ShippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase));

					if (shippingOptionToSelect != null)
					{
						shippingOptionToSelect.Selected = true;
					}
				}

				// If no option has been selected, let's do it for the first one.
				if (model.ShippingMethods.Where(so => so.Selected).FirstOrDefault() == null)
				{
					var shippingOptionToSelect = model.ShippingMethods.FirstOrDefault();
					if (shippingOptionToSelect != null)
					{
						shippingOptionToSelect.Selected = true;
					}
				}
			}
			else
			{
				foreach (var error in getShippingOptionResponse.Errors)
				{
					model.Warnings.Add(error);
				}
			}

			return model;
		}

        [NonAction]
		protected CheckoutPaymentMethodModel PreparePaymentMethodModel(IList<OrganizedShoppingCartItem> cart)
        {
			var store = _storeContext.CurrentStore;
			var customer = _workContext.CurrentCustomer;
			var model = new CheckoutPaymentMethodModel();

            // Was shipping skipped.
            var shippingOptions = _shippingService.GetShippingOptions(cart, customer.ShippingAddress, "", store.Id).ShippingOptions;

            if (!cart.RequiresShipping() || (shippingOptions.Count <= 1 && _shippingSettings.SkipShippingIfSingleOption))
            {
                model.SkippedSelectShipping = true;
            }

			var paymentTypes = new PaymentMethodType[] { PaymentMethodType.Standard, PaymentMethodType.Redirection, PaymentMethodType.StandardAndRedirection };

            var boundPaymentMethods = _paymentService
				.LoadActivePaymentMethods(customer, cart, store.Id, paymentTypes)
                .ToList();

			var allPaymentMethods = _paymentService.GetAllPaymentMethods(store.Id);

            foreach (var pm in boundPaymentMethods)
            {
				if (cart.IsRecurring() && pm.Value.RecurringPaymentType == RecurringPaymentType.NotSupported)
                    continue;

				var paymentMethod = allPaymentMethods.FirstOrDefault(x => x.PaymentMethodSystemName.IsCaseInsensitiveEqual(pm.Metadata.SystemName));
                
                var pmModel = new CheckoutPaymentMethodModel.PaymentMethodModel
                {
					Name = _pluginMediator.GetLocalizedFriendlyName(pm.Metadata),
					Description = _pluginMediator.GetLocalizedDescription(pm.Metadata),
                    PaymentMethodSystemName = pm.Metadata.SystemName,
					PaymentInfoRoute = pm.Value.GetPaymentInfoRoute(),
					RequiresInteraction = pm.Value.RequiresInteraction
                };

				if (paymentMethod != null)
				{
					pmModel.FullDescription = paymentMethod.GetLocalized(x => x.FullDescription, _workContext.WorkingLanguage);
				}
				
				pmModel.BrandUrl = _pluginMediator.GetBrandImageUrl(pm.Metadata);

                // Payment method additional fee.
				decimal paymentMethodAdditionalFee = _paymentService.GetAdditionalHandlingFee(cart, pm.Metadata.SystemName);
                decimal rateBase = _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, customer);
                decimal rate = _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
                
				if (rate != decimal.Zero)
					pmModel.Fee = _priceFormatter.FormatPaymentMethodAdditionalFee(rate, true);

                model.PaymentMethods.Add(pmModel);
            }

			// Find a selected (previously) payment method.
			var selected = false;
			var selectedPaymentMethodSystemName = customer.GetAttribute<string>(SystemCustomerAttributeNames.SelectedPaymentMethod, _genericAttributeService, store.Id);
			if (selectedPaymentMethodSystemName.HasValue())
            {
                var paymentMethodToSelect = model.PaymentMethods.Find(pm => pm.PaymentMethodSystemName.IsCaseInsensitiveEqual(selectedPaymentMethodSystemName));
				if (paymentMethodToSelect != null)
				{
					paymentMethodToSelect.Selected = true;
					selected = true;
				}
            }

            // If no option has been selected, let's do it for the first one.
			if (!selected)
            {
                var paymentMethodToSelect = model.PaymentMethods.FirstOrDefault();
                if (paymentMethodToSelect != null)
                    paymentMethodToSelect.Selected = true;
            }

            return model;
        }

        [NonAction]
		protected CheckoutConfirmModel PrepareConfirmOrderModel(IList<OrganizedShoppingCartItem> cart)
        {
            var model = new CheckoutConfirmModel();

            //min order amount validation
            bool minOrderTotalAmountOk = _orderProcessingService.ValidateMinOrderTotalAmount(cart);
            if (!minOrderTotalAmountOk)
            {
                decimal minOrderTotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderTotalAmount, _workContext.WorkingCurrency);
                model.MinOrderTotalWarning = string.Format(_localizationService.GetResource("Checkout.MinOrderTotalAmount"), _priceFormatter.FormatPrice(minOrderTotalAmount, true, false));
            }

            model.TermsOfServiceEnabled = _orderSettings.TermsOfServiceEnabled;
			model.ShowEsdRevocationWaiverBox = _shoppingCartSettings.ShowEsdRevocationWaiverBox;
			model.BypassPaymentMethodInfo = _paymentSettings.BypassPaymentMethodInfo;
			model.NewsLetterSubscription = _shoppingCartSettings.NewsLetterSubscription;
			model.ThirdPartyEmailHandOver = _shoppingCartSettings.ThirdPartyEmailHandOver;

			if (_shoppingCartSettings.ThirdPartyEmailHandOver != CheckoutThirdPartyEmailHandOver.None)
			{
				model.ThirdPartyEmailHandOverLabel = _shoppingCartSettings.GetLocalized(x => x.ThirdPartyEmailHandOverLabel, _workContext.WorkingLanguage, true, false);

				if (model.ThirdPartyEmailHandOverLabel.IsEmpty())
					model.ThirdPartyEmailHandOverLabel = T("Admin.Configuration.Settings.ShoppingCart.ThirdPartyEmailHandOverLabel.Default");
			}

			return model;
        }

        [NonAction]
        protected bool IsMinimumOrderPlacementIntervalValid(Customer customer)
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (_orderSettings.MinimumOrderPlacementInterval == 0)
                return true;

			var lastOrder = _orderService.SearchOrders(_storeContext.CurrentStore.Id, _workContext.CurrentCustomer.Id,
				 null, null, null, null, null, null, null, null, 0, 1)
                .FirstOrDefault();
			if (lastOrder == null)
                return true;

			var interval = DateTime.UtcNow - lastOrder.CreatedOnUtc;
            return interval.TotalSeconds > _orderSettings.MinimumOrderPlacementInterval;
        }

		private bool IsValidPaymentForm(IPaymentMethod paymentMethod, FormCollection form)
		{
			var paymentControllerType = paymentMethod.GetControllerType();
			var paymentController = DependencyResolver.Current.GetService(paymentControllerType) as PaymentControllerBase;
			var warnings = paymentController.ValidatePaymentForm(form);
				
			foreach (var warning in warnings)
			{
				ModelState.AddModelError("", warning);
			}

			if (ModelState.IsValid)
			{
				var paymentInfo = paymentController.GetPaymentInfo(form);
				_httpContext.Session["OrderPaymentInfo"] = paymentInfo;

				_httpContext.GetCheckoutState().PaymentSummary = paymentController.GetPaymentSummary(form);

				return true;
			}

			return false;
		}

        #endregion

        #region Methods (multistep checkout)

        public ActionResult Index()
        {
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //reset checkout data
            _customerService.ResetCheckoutData(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id);

            //validation (cart)
			var checkoutAttributesXml = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService);
			var scWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, true);
            if (scWarnings.Count > 0)
                return RedirectToRoute("ShoppingCart");

            //validation (each shopping cart item)
            foreach (var sci in cart)
            {
                var sciWarnings = _shoppingCartService.GetShoppingCartItemWarnings(_workContext.CurrentCustomer,
                    sci.Item.ShoppingCartType,
                    sci.Item.Product,
					sci.Item.StoreId,
                    sci.Item.AttributesXml,
                    sci.Item.CustomerEnteredPrice,
                    sci.Item.Quantity,
                    false, childItems: sci.ChildItems);
                if (sciWarnings.Count > 0)
                    return RedirectToRoute("ShoppingCart");
            }

            return RedirectToAction("BillingAddress");
        }


        public ActionResult BillingAddress()
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //model
            var model = PrepareBillingAddressModel();
            return View(model);
        }
        public ActionResult SelectBillingAddress(int addressId)
        {
            var address = _workContext.CurrentCustomer.Addresses.Where(a => a.Id == addressId).FirstOrDefault();
            if (address == null)
				return RedirectToAction("BillingAddress");

            _workContext.CurrentCustomer.BillingAddress = address;
            _customerService.UpdateCustomer(_workContext.CurrentCustomer);

			return RedirectToAction("ShippingAddress");
        }
        [HttpPost, ActionName("BillingAddress")]
        [FormValueRequired("nextstep")]
        public ActionResult NewBillingAddress(CheckoutBillingAddressModel model)
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                var address = model.NewAddress.ToEntity();
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;
                _workContext.CurrentCustomer.Addresses.Add(address);
                _workContext.CurrentCustomer.BillingAddress = address;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);

				return RedirectToAction("ShippingAddress");
            }


            //If we got this far, something failed, redisplay form
            model = PrepareBillingAddressModel(model.NewAddress.CountryId);
            return View(model);
        }

        public ActionResult ShippingAddress()
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            if (!cart.RequiresShipping())
            {
                _workContext.CurrentCustomer.ShippingAddress = null;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                return RedirectToAction("ShippingMethod");
            }

            //model
            var model = PrepareShippingAddressModel();
            return View(model);
        }

        public ActionResult SelectShippingAddress(int addressId)
        {
            var address = _workContext.CurrentCustomer.Addresses.Where(a => a.Id == addressId).FirstOrDefault();
            if (address == null)
				return RedirectToAction("ShippingAddress");

            _workContext.CurrentCustomer.ShippingAddress = address;
            _customerService.UpdateCustomer(_workContext.CurrentCustomer);

			return RedirectToAction("ShippingMethod");
        }

        [HttpPost, ActionName("ShippingAddress")]
        [FormValueRequired("nextstep")]
        public ActionResult NewShippingAddress(CheckoutShippingAddressModel model)
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            if (!cart.RequiresShipping())
            {
                _workContext.CurrentCustomer.ShippingAddress = null;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
				return RedirectToAction("ShippingMethod");
            }

            if (ModelState.IsValid)
            {
                var address = model.NewAddress.ToEntity();
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;
                _workContext.CurrentCustomer.Addresses.Add(address);
                _workContext.CurrentCustomer.ShippingAddress = address;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);

				return RedirectToAction("ShippingMethod");
            }


            //If we got this far, something failed, redisplay form
            model = PrepareShippingAddressModel(model.NewAddress.CountryId);
            return View(model);
        }
        
        public ActionResult ShippingMethod()
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            if (!cart.RequiresShipping())
            {
				_genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, null, _storeContext.CurrentStore.Id);

                return RedirectToAction("PaymentMethod");
            }
                        
            var shippingOptions = _shippingService.GetShippingOptions(cart, _workContext.CurrentCustomer.ShippingAddress, "", _storeContext.CurrentStore.Id).ShippingOptions;

            var checkoutState = _httpContext.GetCheckoutState();
            if (checkoutState.CustomProperties.ContainsKey("HasOnlyOneActiveShippingMethod"))
                checkoutState.CustomProperties["HasOnlyOneActiveShippingMethod"] = shippingOptions.Count == 1;
            else
                checkoutState.CustomProperties.Add("HasOnlyOneActiveShippingMethod", shippingOptions.Count == 1);
            
            if (shippingOptions.Count <= 1 && _shippingSettings.SkipShippingIfSingleOption)
            {
                _genericAttributeService.SaveAttribute<ShippingOption>(
                    _workContext.CurrentCustomer, 
                    SystemCustomerAttributeNames.SelectedShippingOption, 
                    shippingOptions.FirstOrDefault(), 
                    _storeContext.CurrentStore.Id);

				var referrer = Services.WebHelper.GetUrlReferrer();
				if (referrer.EndsWith("/PaymentMethod") || referrer.EndsWith("/Confirm"))
				{
					return RedirectToAction("ShippingAddress");
				}

				return RedirectToAction("PaymentMethod");
            }

            //model
            var model = PrepareShippingMethodModel(cart);
            return View(model);
        }

        [HttpPost, ActionName("ShippingMethod")]
        [FormValueRequired("nextstep")]
        [ValidateInput(false)]
        public ActionResult SelectShippingMethod(string shippingoption)
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            if (!cart.RequiresShipping())
            {
				_genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer,
					 SystemCustomerAttributeNames.SelectedShippingOption, null, _storeContext.CurrentStore.Id);
				return RedirectToAction("PaymentMethod");
            }

            //parse selected method 
            if (String.IsNullOrEmpty(shippingoption))
                return ShippingMethod();
            
			var splittedOption = shippingoption.Split(new string[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
            if (splittedOption.Length != 2)
                return ShippingMethod();

            string selectedName = splittedOption[0];
            string shippingRateComputationMethodSystemName = splittedOption[1];
            
            //find it
            //performance optimization. try cache first
			var shippingOptions = _workContext.CurrentCustomer.GetAttribute<List<ShippingOption>>(SystemCustomerAttributeNames.OfferedShippingOptions, _storeContext.CurrentStore.Id);
            if (shippingOptions == null || shippingOptions.Count == 0)
            {
                //not found? let's load them using shipping service
                shippingOptions = _shippingService
					.GetShippingOptions(cart, _workContext.CurrentCustomer.ShippingAddress, shippingRateComputationMethodSystemName, _storeContext.CurrentStore.Id)
                    .ShippingOptions
                    .ToList();
            }
            else
            {
                //loaded cached results. let's filter result by a chosen shipping rate computation method
                shippingOptions = shippingOptions.Where(so => so.ShippingRateComputationMethodSystemName.Equals(shippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
            }

            var shippingOption = shippingOptions.Find(so => !String.IsNullOrEmpty(so.Name) && so.Name.Equals(selectedName, StringComparison.InvariantCultureIgnoreCase));

            if (shippingOption == null)
                return ShippingMethod();

            //save
			_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, shippingOption, _storeContext.CurrentStore.Id);

			return RedirectToAction("PaymentMethod");
        }
        
        
        public ActionResult PaymentMethod()
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

			// Check whether payment workflow is required. We ignore reward points during cart total calculation.
			decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart, true);
			var isPaymentWorkflowRequired = !(shoppingCartTotalBase.HasValue && shoppingCartTotalBase.Value == decimal.Zero);

			var model = PreparePaymentMethodModel(cart);
			var onlyOnePassiveMethod = model.PaymentMethods.Count == 1 && !model.PaymentMethods[0].RequiresInteraction;

            var checkoutState = _httpContext.GetCheckoutState();
            if (checkoutState.CustomProperties.ContainsKey("HasOnlyOneActivePaymentMethod"))
                checkoutState.CustomProperties["HasOnlyOneActivePaymentMethod"] = model.PaymentMethods.Count == 1;
            else
                checkoutState.CustomProperties.Add("HasOnlyOneActivePaymentMethod", model.PaymentMethods.Count == 1);
            
            if (!isPaymentWorkflowRequired || (_paymentSettings.BypassPaymentMethodSelectionIfOnlyOne && onlyOnePassiveMethod))
            {
                // If there's nothing to pay for OR if we have only one passive payment method and reward points are disabled
				// or the current customer doesn't have any reward points so customer doesn't have to choose a payment method.

				_genericAttributeService.SaveAttribute<string>(
					_workContext.CurrentCustomer,
					SystemCustomerAttributeNames.SelectedPaymentMethod,
					!model.PaymentMethods.Any() ? null : model.PaymentMethods[0].PaymentMethodSystemName,
					_storeContext.CurrentStore.Id);

                checkoutState.IsPaymentSelectionSkipped = true;

				var referrer = Services.WebHelper.GetUrlReferrer();
				if (referrer.EndsWith("/Confirm"))
				{
					return RedirectToAction("ShippingMethod");
				}

				return RedirectToAction("Confirm");
            }

            checkoutState.IsPaymentSelectionSkipped = false;

            return View(model);
        }

        [HttpPost, ActionName("PaymentMethod")]
        [FormValueRequired("nextstep")]
        [ValidateInput(false)]
        public ActionResult SelectPaymentMethod(string paymentmethod, CheckoutPaymentMethodModel model, FormCollection form)
        {
			// validation
			var storeId = _storeContext.CurrentStore.Id;
			var customer = _workContext.CurrentCustomer;
			var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, storeId);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if ((customer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            // payment method 
            if (String.IsNullOrEmpty(paymentmethod))
                return PaymentMethod();

			var paymentMethodProvider = _paymentService.LoadPaymentMethodBySystemName(paymentmethod, true, storeId);
			if (paymentMethodProvider == null)
                return PaymentMethod();

            // save
			_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, paymentmethod, storeId);

			// validate info
			if (!IsValidPaymentForm(paymentMethodProvider.Value, form))
				return PaymentMethod();

			// save payment data so that the user must not re-enter it
			form.CopyTo(_httpContext.GetCheckoutState().PaymentData, true);

			return RedirectToAction("Confirm");
        }

		[HttpPost]
		public ActionResult PaymentInfoAjax(string paymentMethodSystemName)
		{
			if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
				return Content("");

			if (paymentMethodSystemName.IsEmpty())
				return new HttpStatusCodeResult(404);

			var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(paymentMethodSystemName);
			if (paymentMethod == null)
				return new HttpStatusCodeResult(404);

			var infoRoute = paymentMethod.Value.GetPaymentInfoRoute();

			if (infoRoute == null)
				return Content("");

			return RedirectToAction(infoRoute.Action, infoRoute.Controller, infoRoute.RouteValues);
		}

        public ActionResult Confirm()
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            var model = PrepareConfirmOrderModel(cart);

			//if (TempData["ConfirmOrderWarnings"] != null)
			//	model.Warnings.AddRange(TempData["ConfirmOrderWarnings"] as IList<string>);

            return View(model);
        }
        [HttpPost, ActionName("Confirm")]
        [ValidateInput(false)]
        public ActionResult ConfirmOrder(FormCollection form)
        {
			//validation
			var storeId = _storeContext.CurrentStore.Id;
			var customer = _workContext.CurrentCustomer;
			var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, storeId);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if ((customer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            var model = new CheckoutConfirmModel();
			PlaceOrderResult placeOrderResult = null;
			PostProcessPaymentRequest postProcessPaymentRequest = null;

			try
            {
                var processPaymentRequest = _httpContext.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
                if (processPaymentRequest == null)
                {
                    //Check whether payment workflow is required
					if (IsPaymentWorkflowRequired(cart))
						return RedirectToAction("PaymentMethod");

					processPaymentRequest = new ProcessPaymentRequest();
                }
                
                //prevent 2 orders being placed within an X seconds time frame
                if (!IsMinimumOrderPlacementIntervalValid(customer))
                    throw new Exception(T("Checkout.MinOrderPlacementInterval"));

                //place order
				processPaymentRequest.StoreId = storeId;
                processPaymentRequest.CustomerId = customer.Id;
				processPaymentRequest.PaymentMethodSystemName = customer.GetAttribute<string>(SystemCustomerAttributeNames.SelectedPaymentMethod, _genericAttributeService, storeId);

                var placeOrderExtraData = new Dictionary<string, string>();
                placeOrderExtraData["CustomerComment"] = form["customercommenthidden"];
				placeOrderExtraData["SubscribeToNewsLetter"] = form["SubscribeToNewsLetter"];
				placeOrderExtraData["AcceptThirdPartyEmailHandOver"] = form["AcceptThirdPartyEmailHandOver"];

				placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest, placeOrderExtraData);

                if (!placeOrderResult.Success)
                {
					model.Warnings.AddRange(placeOrderResult.Errors.Select(x => HtmlUtils.ConvertPlainTextToHtml(x)));
                }
            }
            catch (Exception exception)
            {
				Logger.Warn(exception, exception.Message);

				if (!model.Warnings.Any(x => x == exception.Message))
				{
					model.Warnings.Add(exception.Message);
				}
            }

			if (placeOrderResult == null || !placeOrderResult.Success || model.Warnings.Any())
			{
				return View(model);
			}

			try
			{
				postProcessPaymentRequest = new PostProcessPaymentRequest
				{
					Order = placeOrderResult.PlacedOrder
				};

				_paymentService.PostProcessPayment(postProcessPaymentRequest);
			}
			catch (Exception exception)
			{
				NotifyError(exception);
			}
			finally
			{
				_httpContext.Session["OrderPaymentInfo"] = null;
				_httpContext.RemoveCheckoutState();
			}

			if (postProcessPaymentRequest != null && postProcessPaymentRequest.RedirectUrl.HasValue())
			{
				return Redirect(postProcessPaymentRequest.RedirectUrl);
			}

			return RedirectToAction("Completed");
		}


        public ActionResult Completed()
        {
            //validation
            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //model
            var model = new CheckoutCompletedModel();

			var order = _orderService.SearchOrders(_storeContext.CurrentStore.Id, _workContext.CurrentCustomer.Id,
				null, null, null, null, null, null, null, null, 0, 1).FirstOrDefault();

			if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
			{
				return HttpNotFound();
			}

			//disable "order completed" page?
			if (_orderSettings.DisableOrderCompletedPage)
			{
				return RedirectToAction("Details", "Order", new { id = order.Id });
			}

			model.OrderId = order.Id;
            model.OrderNumber = order.GetOrderNumber();

            return View(model);
        }
        
        [ChildActionOnly]
        public ActionResult CheckoutProgress(CheckoutProgressStep step)
        {
            var model = new CheckoutProgressModel() {CheckoutProgressStep = step};

            var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);
            var shippingOptions = _shippingService.GetShippingOptions(cart, _workContext.CurrentCustomer.ShippingAddress, "", _storeContext.CurrentStore.Id).ShippingOptions;

            if (shippingOptions.Count <= 1 && _shippingSettings.SkipShippingIfSingleOption)
            {
                model.DisplayShippingOptions = false;
            }

            return PartialView(model);
        }
        #endregion

    }
}
