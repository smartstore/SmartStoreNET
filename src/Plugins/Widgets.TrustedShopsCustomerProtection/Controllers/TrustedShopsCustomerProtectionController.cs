using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection.Models;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Stores;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Settings;
using SmartStore.Web.Framework.UI;


namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection.Controllers
{
    
    public class TrustedShopsCustomerProtectionController : Controller
    {
		private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
		private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ITaxService _taxService;

        private readonly TrustedShopsCustomerProtection.com.trustedshops.qa.TSProtectionService _protectionServiceSandbox;
        private readonly TrustedShopsCustomerProtection.com.trustedshops.www.TSProtectionService _protectionServiceLive;

		public TrustedShopsCustomerProtectionController(IWorkContext workContext,
			IStoreContext storeContext, IStoreService storeService,
            ISettingService settingService, IProductService productService,
            ILocalizationService localizationService, IOrderService orderService,
            IPriceFormatter priceFormatter, ICurrencyService currencyService,
            IOrderTotalCalculationService orderTotalCalculationService, ITaxService taxService)
        {
			_workContext = workContext;
			_storeContext = storeContext;
			_storeService = storeService;
            _settingService = settingService;
            _localizationService = localizationService;
            _orderService = orderService;
            _productService = productService;
            _priceFormatter = priceFormatter;
            _currencyService = currencyService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _taxService = taxService;

            _protectionServiceSandbox = new TrustedShopsCustomerProtection.com.trustedshops.qa.TSProtectionService();
            _protectionServiceLive = new TrustedShopsCustomerProtection.com.trustedshops.www.TSProtectionService();
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var TrustedShopsCustomerProtectionSettings = _settingService.LoadSetting<TrustedShopsCustomerProtectionSettings>(storeScope);

            var model = new ConfigurationModel();
            model.TrustedShopsId = TrustedShopsCustomerProtectionSettings.TrustedShopsId;
            model.IsTestMode = TrustedShopsCustomerProtectionSettings.IsTestMode;
            model.UserName = TrustedShopsCustomerProtectionSettings.UserName;
            model.Password = TrustedShopsCustomerProtectionSettings.Password;
            model.ProtectionMode = TrustedShopsCustomerProtectionSettings.ProtectionMode;

            model.AvailableModes.Add(new SelectListItem() { Text = "Classic", Value = "Classic", Selected = TrustedShopsCustomerProtectionSettings.ProtectionMode == "Classic" });
            model.AvailableModes.Add(new SelectListItem() { Text = "Excellence", Value = "Excellence", Selected = TrustedShopsCustomerProtectionSettings.ProtectionMode == "Excellence" });

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			storeDependingSettingHelper.GetOverrideKeys(TrustedShopsCustomerProtectionSettings, model, storeScope, _settingService);

            return View("SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection.Views.TrustedShopsCustomerProtection.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        [FormValueRequired("save")]
		public ActionResult Configure(ConfigurationModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
                return Configure();

			//load settings for a chosen store scope
			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var TrustedShopsCustomerProtectionSettings = _settingService.LoadSetting<TrustedShopsCustomerProtectionSettings>(storeScope);

            var tsProtectionServiceSandbox = new TrustedShopsCustomerProtection.com.trustedshops.qa.TSProtectionService();
            var tsProtectionServiceLive = new TrustedShopsCustomerProtection.com.trustedshops.www.TSProtectionService();

            if (model.IsTestMode)
            {
                var certStatus = new TrustedShopsCustomerProtection.com.trustedshops.qa.CertificateStatus();
                certStatus = tsProtectionServiceSandbox.checkCertificate(model.TrustedShopsId);

                if (certStatus.stateEnum == "TEST")
                {
                    // inform user about successfull validation
                    this.AddNotificationMessage(NotifyType.Success, _localizationService.GetResource("Plugins.Widgets.TrustedShopsCustomerProtection.CheckIdSuccess"), true);

                    //save settings
                    TrustedShopsCustomerProtectionSettings.TrustedShopsId = model.TrustedShopsId;
                    TrustedShopsCustomerProtectionSettings.IsTestMode = model.IsTestMode;
                    TrustedShopsCustomerProtectionSettings.IsExcellenceMode = model.ProtectionMode == "Excellence";
                    TrustedShopsCustomerProtectionSettings.UserName = model.UserName;
                    TrustedShopsCustomerProtectionSettings.Password = model.Password;
                    TrustedShopsCustomerProtectionSettings.ProtectionMode = model.ProtectionMode;

					storeDependingSettingHelper.UpdateSettings(TrustedShopsCustomerProtectionSettings, form, storeScope, _settingService);
					_settingService.ClearCache();
                }
                else
                {
                    // inform user about validation error
                    this.AddNotificationMessage(NotifyType.Error, _localizationService.GetResource("Plugins.Widgets.TrustedShopsCustomerProtection.CheckIdError"), true);
                    model.TrustedShopsId = String.Empty;
                    model.IsTestMode = false;
                }
            }
            else
            {
                var certStatus = new TrustedShopsCustomerProtection.com.trustedshops.www.CertificateStatus();
                certStatus = tsProtectionServiceLive.checkCertificate(model.TrustedShopsId);

                if (certStatus.stateEnum == "PRODUCTION")
                {
                    // inform user about successfull validation
                    this.AddNotificationMessage(NotifyType.Success, _localizationService.GetResource("Plugins.Widgets.TrustedShopsCustomerProtection.CheckIdSuccess"), true);

                    //save settings
                    TrustedShopsCustomerProtectionSettings.TrustedShopsId = model.TrustedShopsId;
                    TrustedShopsCustomerProtectionSettings.IsTestMode = model.IsTestMode;
                    TrustedShopsCustomerProtectionSettings.IsExcellenceMode = model.ProtectionMode == "Excellence";
                    TrustedShopsCustomerProtectionSettings.UserName = model.UserName;
                    TrustedShopsCustomerProtectionSettings.Password = model.Password;
                    TrustedShopsCustomerProtectionSettings.ProtectionMode = model.ProtectionMode;

					storeDependingSettingHelper.UpdateSettings(TrustedShopsCustomerProtectionSettings, form, storeScope, _settingService);
					_settingService.ClearCache();
                }
                else
                {
                    // inform user about validation error
                    this.AddNotificationMessage(NotifyType.Error, _localizationService.GetResource("Plugins.Widgets.TrustedShopsCustomerProtection.CheckIdError"), true);
                    model.TrustedShopsId = String.Empty;
                    model.IsTestMode = false;
                }
            }

            return Configure();
        }

        [HttpPost, ActionName("Configure"), AdminAuthorize, ChildActionOnly]
        [FormValueRequired("check-login")]
        public ActionResult CheckLogin(ConfigurationModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var TrustedShopsCustomerProtectionSettings = _settingService.LoadSetting<TrustedShopsCustomerProtectionSettings>(storeScope);

            var tsProtectionServiceSandbox = new TrustedShopsCustomerProtection.com.trustedshops.qa.TSProtectionService();
            var tsProtectionServiceLive = new TrustedShopsCustomerProtection.com.trustedshops.www.TSProtectionService();

            long checkLoginResult;

            if (!String.IsNullOrEmpty(model.TrustedShopsId) && !String.IsNullOrEmpty(model.UserName) && !String.IsNullOrEmpty(model.Password))
            {
                if (model.IsTestMode)
                {
                    checkLoginResult = tsProtectionServiceSandbox.checkLogin(model.TrustedShopsId, model.UserName, model.Password);
                }
                else
                {
                    checkLoginResult = tsProtectionServiceLive.checkLogin(model.TrustedShopsId, model.UserName, model.Password);
                }

                if (checkLoginResult >= 0)
                {
                    // inform user about successfull validation
                    this.AddNotificationMessage(NotifyType.Success, _localizationService.GetResource("Plugins.Widgets.TrustedShopsCustomerProtection.CheckLoginSuccess"), true);

                    //save settings
                    TrustedShopsCustomerProtectionSettings.UserName = model.UserName;
                    TrustedShopsCustomerProtectionSettings.Password = model.Password;

                    storeDependingSettingHelper.UpdateSettings(TrustedShopsCustomerProtectionSettings, form, storeScope, _settingService);
                    _settingService.ClearCache();
                }
                else
                {
                    // inform user about validation error
                    this.AddNotificationMessage(NotifyType.Error, _localizationService.GetResource("Plugins.Widgets.TrustedShopsCustomerProtection.Error." + checkLoginResult), true);

                    //reset user id and password
                    //model.UserName = String.Empty;
                    //model.Password = String.Empty;
                }
            }
            else { 
                //TODO: inform the shop admin that he has to fill all the fields
                this.AddNotificationMessage(NotifyType.Error, _localizationService.GetResource("Plugins.Widgets.TrustedShopsCustomerProtection.Error.MissingParams"), true);
            }

            return Configure();
        }

        [HttpPost, ActionName("Configure"), AdminAuthorize, ChildActionOnly]
        [FormValueRequired("get-protection-items")]
        public ActionResult GetProtectionItems(ConfigurationModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
                return Configure();

            var tsProtectionServiceSandbox = new TrustedShopsCustomerProtection.com.trustedshops.qa.TSProtectionService();
            var tsProtectionServiceLive = new TrustedShopsCustomerProtection.com.trustedshops.www.TSProtectionService();

            //TODO: remove duplicate code
            if (model.IsTestMode)
            {
                var tsProtectionProducts = tsProtectionServiceSandbox.getProtectionItems(model.TrustedShopsId);

                foreach(var tsProduct in tsProtectionProducts) 
                {
                    var tsProductName = _localizationService.GetResource("Plugins.Widgets.TrustedShopsCustomerProtection.ProtectionMode.Excellence.Public.Name").FormatWith(
                        _priceFormatter.FormatPrice(tsProduct.protectedAmountDecimal, false, false)
                    );
                    
                    var tsProductDesc = _localizationService.GetResource("TrustedShopsProduct.Description").FormatWith(
                        tsProduct.protectionDurationInt, 
                        tsProduct.protectedAmountDecimal
                    );

                    var tsProductVariant = _productService.GetProductVariantBySku(tsProduct.tsProductID);

                    if (tsProductVariant == null)
                    {
                        //product
                        var product = new Product()
                        {
                            Name = tsProductName,
                            ShortDescription = tsProduct.tsProductID,
                            FullDescription = tsProductDesc,
                            AllowCustomerReviews = false,
                            Published = true,
                            CreatedOnUtc = DateTime.UtcNow,
                            UpdatedOnUtc = DateTime.UtcNow
                        };
                        _productService.InsertProduct(product);

                        //product variant
                        tsProductVariant = new ProductVariant();

                        //tsProductVariant.Name = tsProductName;
                        tsProductVariant.Sku = tsProduct.tsProductID;
                        tsProductVariant.Price = tsProduct.grossFee;
                        tsProductVariant.TaxCategoryId = 1;
                        tsProductVariant.Description = tsProductDesc;
                        tsProductVariant.Published = true;
                        tsProductVariant.CreatedOnUtc = DateTime.UtcNow;
                        tsProductVariant.UpdatedOnUtc = DateTime.UtcNow;
                        tsProductVariant.OrderMaximumQuantity = 1;
                        tsProductVariant.OrderMinimumQuantity = 1;
                        tsProductVariant.AdminComment = "TrustedShops-Product";

                        var tempname = product.Name;
                        var temp = product.Id;
                        tsProductVariant.ProductId = temp;

                        //insert variant
                        _productService.InsertProductVariant(tsProductVariant);
                    }
                    else {
                        tsProductVariant.Price = tsProduct.grossFee;
                        tsProductVariant.TaxCategoryId = 1;
                        //tsProductVariant.Name = tsProductName;
                        tsProductVariant.Product.Name = tsProductName;
                        tsProductVariant.Sku = tsProduct.tsProductID;
                        tsProductVariant.Description = tsProductDesc;
                        tsProductVariant.UpdatedOnUtc = DateTime.UtcNow;
                        tsProductVariant.OrderMaximumQuantity = 1;
                        tsProductVariant.OrderMinimumQuantity = 1;
                        tsProductVariant.AdminComment = "TrustedShops-Product";
                        //update variant
                        _productService.UpdateProductVariant(tsProductVariant);
                    }
                }
            }
            else
            {
                var tsProtectionProducts = tsProtectionServiceLive.getProtectionItems(model.TrustedShopsId);

                foreach (var tsProduct in tsProtectionProducts)
                {
                    var tsProductName = _localizationService.GetResource("Plugins.Widgets.TrustedShopsCustomerProtection.ProtectionMode.Excellence.Public.Name").FormatWith(
                        _priceFormatter.FormatPrice(tsProduct.protectedAmountDecimal, false, false)
                    );

                    var tsProductDesc = _localizationService.GetResource("TrustedShopsProduct.Description").FormatWith(
                        tsProduct.protectionDurationInt,
                        tsProduct.protectedAmountDecimal
                    );

                    var tsProductVariant = _productService.GetProductVariantBySku(tsProduct.tsProductID);

                    if (tsProductVariant == null)
                    {
                        //product
                        var product = new Product()
                        {
                            Name = tsProductName,
                            ShortDescription = tsProduct.tsProductID,
                            FullDescription = tsProductDesc,
                            AllowCustomerReviews = false,
                            Published = true,
                            CreatedOnUtc = DateTime.UtcNow,
                            UpdatedOnUtc = DateTime.UtcNow
                        };
                        _productService.InsertProduct(product);

                        //product variant
                        tsProductVariant = new ProductVariant();

                        //tsProductVariant.Name = tsProductName;
                        tsProductVariant.Sku = tsProduct.tsProductID;
                        tsProductVariant.Price = tsProduct.grossFee;
                        tsProductVariant.TaxCategoryId = 1;
                        tsProductVariant.Description = tsProductDesc;
                        tsProductVariant.Published = true;
                        tsProductVariant.CreatedOnUtc = DateTime.UtcNow;
                        tsProductVariant.UpdatedOnUtc = DateTime.UtcNow;
                        tsProductVariant.OrderMaximumQuantity = 1;
                        tsProductVariant.OrderMinimumQuantity = 1;
                        tsProductVariant.AdminComment = "TrustedShops-Product";

                        var tempname = product.Name;
                        var temp = product.Id;
                        tsProductVariant.ProductId = temp;

                        //insert variant
                        _productService.InsertProductVariant(tsProductVariant);
                    }
                    else
                    {
                        tsProductVariant.Price = tsProduct.grossFee;
                        tsProductVariant.TaxCategoryId = 1;
                        //tsProductVariant.Name = tsProductName;
                        tsProductVariant.Product.Name = tsProductName;
                        tsProductVariant.Sku = tsProduct.tsProductID;
                        tsProductVariant.Description = tsProductDesc;
                        tsProductVariant.UpdatedOnUtc = DateTime.UtcNow;
                        tsProductVariant.OrderMaximumQuantity = 1;
                        tsProductVariant.OrderMinimumQuantity = 1;
                        tsProductVariant.AdminComment = "TrustedShops-Product";
                        //update variant
                        _productService.UpdateProductVariant(tsProductVariant);
                    }
                }

            }

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone)
        {
			var TrustedShopsCustomerProtectionSettings = _settingService.LoadSetting<TrustedShopsCustomerProtectionSettings>(_storeContext.CurrentStore.Id);

            var model = new PublicInfoModel();
            model.TrustedShopsId = TrustedShopsCustomerProtectionSettings.TrustedShopsId;
            model.IsTestMode = TrustedShopsCustomerProtectionSettings.IsTestMode;

            var lastOrder = _orderService.SearchOrders(_storeContext.CurrentStore.Id, _workContext.CurrentCustomer.Id,
                null, null, null, null, null, _workContext.CurrentCustomer.BillingAddress.Email, null, null, 0, 1).FirstOrDefault();

            string paymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.SelectedPaymentMethod, _storeContext.CurrentStore.Id);
            
            //BEGIN: get subtotal
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                 .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                 .Where(sci => sci.StoreId == _storeContext.CurrentStore.Id)
                 .ToList();

            decimal subtotalBase = decimal.Zero;
            decimal orderSubTotalDiscountAmountBase = decimal.Zero;
            Discount orderSubTotalAppliedDiscount = null;
            decimal subTotalWithoutDiscountBase = decimal.Zero;
            decimal subTotalWithDiscountBase = decimal.Zero;
            _orderTotalCalculationService.GetShoppingCartSubTotal(cart, 
                out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscount, out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
            subtotalBase = subTotalWithoutDiscountBase;
            decimal subtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
            //END: get subtotal

            model.BuyerEmail = _workContext.CurrentCustomer.BillingAddress.Email;
            model.Amount = Math.Round(subtotal, 2).ToString().Replace(",", ".");
            model.Currency = _workContext.WorkingCurrency.CurrencyCode;
            model.PaymentType = TrustedShopsUtils.ConvertPaymentSystemNameToTrustedShopsCode(paymentMethodSystemName);
            model.CustomerId = _workContext.CurrentCustomer.Id.ToString();
            model.OrderId = (lastOrder.Id + 1).ToString();
            

            //get ts-products and insert them into select-Box
            string tsProductSku = TrustedShopsUtils.GetTrustedShopsProductSku(_workContext.WorkingCurrency.CurrencyCode, subtotal);
            var tsProduct = _productService.GetProductVariantBySku(tsProductSku);

            model.TrustedShopsProductId = Convert.ToString(tsProduct.Id);

            //calculate prices
            decimal taxRate = decimal.Zero;
            decimal finalPriceBase = _taxService.GetProductPrice(tsProduct, tsProduct.Price, out taxRate);

            model.ExcellenceName = tsProduct.Name + " (" + _priceFormatter.FormatPrice(finalPriceBase, true, true) + ")";
              
            model.ExcellenceDescription = _localizationService.GetResource("Plugins.Widgets.TrustedShopsCustomerProtection.ProtectionMode.Excellence.Public.Description").FormatWith(
                TrustedShopsCustomerProtectionSettings.TrustedShopsId
            );

            if ((TrustedShopsCustomerProtectionSettings.ProtectionMode == "Excellence") && 
                (widgetZone == "checkout_payment_method_bottom" || widgetZone == "op_checkout_payment_method_bottom"))
            {
                //show view on payment page
                return View("SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection.Views.TrustedShopsCustomerProtection.PublicInfoPayment", model);
            }
            else if ((TrustedShopsCustomerProtectionSettings.ProtectionMode == "Classic") && 
                (widgetZone == "op_checkout_confirm_bottom" || widgetZone == "order_summary_content_before")) 
            {
                //show view on confirmation page
                return View("SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection.Views.TrustedShopsCustomerProtection.PublicInfoCheckout", model);
            }

            return new EmptyResult();
        }

    }

}