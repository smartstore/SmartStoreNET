using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Logging;
using SmartStore.Services;
using SmartStore.Services.Authentication;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Forums;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Seo;
using SmartStore.Services.Tax;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Models.Customer;
using SmartStore.Core.Events;

namespace SmartStore.Web.Controllers
{
	public partial class CustomerController : PublicControllerBase
    {
        #region Fields

        private readonly ICommonServices _services;
        private readonly IAuthenticationService _authenticationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly TaxSettings _taxSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ITaxService _taxService;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly AddressSettings _addressSettings;
        private readonly ForumSettings _forumSettings;
        private readonly OrderSettings _orderSettings;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly ICurrencyService _currencyService;
        private readonly IPaymentService _paymentService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPictureService _pictureService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IForumService _forumService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly IDownloadService _downloadService;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerActivityService _customerActivityService;
		private readonly ProductUrlHelper _productUrlHelper;
		private readonly MediaSettings _mediaSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
		private readonly PluginMediator _pluginMediator;
		private readonly IEventPublisher _eventPublisher;

		#endregion

		#region Ctor

		public CustomerController(
            ICommonServices services,
            IAuthenticationService authenticationService,
            IDateTimeHelper dateTimeHelper,
            DateTimeSettings dateTimeSettings, TaxSettings taxSettings,
            ILocalizationService localizationService,
			IWorkContext workContext, IStoreContext storeContext,
			ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ICustomerRegistrationService customerRegistrationService,
            ITaxService taxService, RewardPointsSettings rewardPointsSettings,
            CustomerSettings customerSettings,AddressSettings addressSettings, ForumSettings forumSettings,
            OrderSettings orderSettings, IAddressService addressService,
            ICountryService countryService, IStateProvinceService stateProvinceService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IOrderProcessingService orderProcessingService, IOrderService orderService,
            ICurrencyService currencyService,
            IPaymentService paymentService,
            IPriceFormatter priceFormatter,
            IPictureService pictureService, INewsLetterSubscriptionService newsLetterSubscriptionService,
            IForumService forumService, IShoppingCartService shoppingCartService,
            IOpenAuthenticationService openAuthenticationService, 
            IBackInStockSubscriptionService backInStockSubscriptionService, 
            IDownloadService downloadService, IWebHelper webHelper,
            ICustomerActivityService customerActivityService, 
			ProductUrlHelper productUrlHelper,
			MediaSettings mediaSettings,
            LocalizationSettings localizationSettings,
            CaptchaSettings captchaSettings, ExternalAuthenticationSettings externalAuthenticationSettings,
			PluginMediator pluginMediator, IEventPublisher eventPublisher)
        {
            _services = services;
            _authenticationService = authenticationService;
            _dateTimeHelper = dateTimeHelper;
            _dateTimeSettings = dateTimeSettings;
            _taxSettings = taxSettings;
            _localizationService = localizationService;
            _workContext = workContext;
			_storeContext = storeContext;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _customerRegistrationService = customerRegistrationService;
            _taxService = taxService;
            _rewardPointsSettings = rewardPointsSettings;
            _customerSettings = customerSettings;
            _addressSettings = addressSettings;
            _forumSettings = forumSettings;
            _orderSettings = orderSettings;
            _addressService = addressService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _orderProcessingService = orderProcessingService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _orderService = orderService;
            _currencyService = currencyService;
            _paymentService = paymentService;
            _priceFormatter = priceFormatter;
            _pictureService = pictureService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _forumService = forumService;
            _shoppingCartService = shoppingCartService;
            _openAuthenticationService = openAuthenticationService;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _downloadService = downloadService;
            _webHelper = webHelper;
            _customerActivityService = customerActivityService;
			_productUrlHelper = productUrlHelper;
			_mediaSettings = mediaSettings;
            _localizationSettings = localizationSettings;
            _captchaSettings = captchaSettings;
            _externalAuthenticationSettings = externalAuthenticationSettings;
			_pluginMediator = pluginMediator;
			_eventPublisher = eventPublisher;
		}

        #endregion

        #region Utilities

        [NonAction]
        protected bool IsCurrentUserRegistered()
        {
            return _workContext.CurrentCustomer.IsRegistered();
        }

        [NonAction]
        protected int GetUnreadPrivateMessages()
        {
            var result = 0;
            var customer = _services.WorkContext.CurrentCustomer;
            if (_forumSettings.AllowPrivateMessages && !customer.IsGuest())
            {
                var privateMessages = _forumService.GetAllPrivateMessages(_services.StoreContext.CurrentStore.Id, 0, customer.Id, false, null, false, 0, 1);
                if (privateMessages.TotalCount > 0)
                {
                    result = privateMessages.TotalCount;
                }
            }

            return result;
        }

        [NonAction]
        protected void TryAssociateAccountWithExternalAccount(Customer customer)
        {
            var parameters = ExternalAuthorizerHelper.RetrieveParametersFromRoundTrip(true);
            if (parameters == null)
                return;

            if (_openAuthenticationService.AccountExists(parameters))
                return;

            _openAuthenticationService.AssociateExternalAccountWithUser(customer, parameters);
        }

        [NonAction]
        protected bool UsernameIsValid(string username)
        {
            var result = true;

            if (String.IsNullOrEmpty(username))
            {
                return false;
            }

            return result;
        }

        [NonAction]
        protected void PrepareCustomerInfoModel(CustomerInfoModel model, Customer customer, bool excludeProperties)
        {
			Guard.NotNull(model, nameof(model));
			Guard.NotNull(customer, nameof(customer));

            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;

			foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
			{
				model.AvailableTimeZones.Add(new SelectListItem
				{
					Text = tzi.DisplayName,
					Value = tzi.Id,
					Selected = (excludeProperties ? tzi.Id == model.TimeZoneId : tzi.Id == _dateTimeHelper.CurrentTimeZone.Id)
				});
			}

            if (!excludeProperties)
            {
				var dateOfBirth = customer.BirthDate;
				var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(customer.Email, _storeContext.CurrentStore.Id);

				model.VatNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);
                model.Title = customer.Title;
                model.FirstName = customer.FirstName;
                model.LastName = customer.LastName;
                model.Gender = customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender);
                if (dateOfBirth.HasValue)
                {
                    model.DateOfBirthDay = dateOfBirth.Value.Day;
                    model.DateOfBirthMonth = dateOfBirth.Value.Month;
                    model.DateOfBirthYear = dateOfBirth.Value.Year;
                }
                model.Company = customer.Company;
                model.StreetAddress = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress);
                model.StreetAddress2 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2);
                model.ZipPostalCode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode);
                model.City = customer.GetAttribute<string>(SystemCustomerAttributeNames.City);
                model.CountryId = customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId);
                model.StateProvinceId = customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId);
                model.Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                model.Fax = customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax);
                model.CustomerNumber = customer.CustomerNumber;
                model.Newsletter = newsletter != null && newsletter.Active;
                model.Signature = customer.GetAttribute<string>(SystemCustomerAttributeNames.Signature);
                model.Email = customer.Email;
                model.Username = customer.Username;
            }
            else
            {
				if (_customerSettings.UsernamesEnabled && !_customerSettings.AllowUsersToChangeUsernames)
				{
					model.Username = customer.Username;
				}
            }

            // Countries and states.
            if (_customerSettings.CountryEnabled)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = T("Address.SelectCountry"), Value = "0" });
                foreach (var c in _countryService.GetAllCountries())
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.GetLocalized(x => x.Name),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (_customerSettings.StateProvinceEnabled)
                {
                    var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId).ToList();
					if (states.Any())
					{
						foreach (var s in states)
						{
							model.AvailableStates.Add(new SelectListItem
							{
								Text = s.GetLocalized(x => x.Name),
								Value = s.Id.ToString(),
								Selected = (s.Id == model.StateProvinceId)
							});
						}
					}
					else
					{
						model.AvailableStates.Add(new SelectListItem { Text = T("Address.OtherNonUS"), Value = "0" });
					}
                }
            }

            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
			model.VatNumberStatusNote = ((VatNumberStatus)customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId))
				 .GetLocalizedEnum(_localizationService, _workContext);
            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.TitleEnabled = _customerSettings.TitleEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.CompanyRequired = _customerSettings.CompanyRequired;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _customerSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _customerSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _customerSettings.ZipPostalCodeRequired;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.CityRequired = _customerSettings.CityRequired;
            model.CountryEnabled = _customerSettings.CountryEnabled;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.PhoneRequired = _customerSettings.PhoneRequired;
            model.FaxEnabled = _customerSettings.FaxEnabled;
            model.FaxRequired = _customerSettings.FaxRequired;
            model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.AllowUsersToChangeUsernames = _customerSettings.AllowUsersToChangeUsernames;
            model.CheckUsernameAvailabilityEnabled = _customerSettings.CheckUsernameAvailabilityEnabled;
            model.SignatureEnabled = _forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled;
            model.DisplayCustomerNumber = _customerSettings.CustomerNumberMethod != CustomerNumberMethod.Disabled 
				&& _customerSettings.CustomerNumberVisibility != CustomerNumberVisibility.None;

            if (_customerSettings.CustomerNumberMethod != CustomerNumberMethod.Disabled
                && (_customerSettings.CustomerNumberVisibility == CustomerNumberVisibility.Editable 
                || (_customerSettings.CustomerNumberVisibility == CustomerNumberVisibility.EditableIfEmpty && model.CustomerNumber.IsEmpty())))
            {
                model.CustomerNumberEnabled = true;
            }
            else
            {
                model.CustomerNumberEnabled = false;
            }

            // External authentication.
            foreach (var ear in _openAuthenticationService.GetExternalIdentifiersFor(customer))
            {
                var authMethod = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName(ear.ProviderSystemName);
                if (authMethod == null || !authMethod.IsMethodActive(_externalAuthenticationSettings))
                    continue;

                model.AssociatedExternalAuthRecords.Add(new CustomerInfoModel.AssociatedExternalAuthModel
                {
                    Id = ear.Id,
                    Email = ear.Email,
                    ExternalIdentifier = ear.ExternalIdentifier,
                    AuthMethodName = _pluginMediator.GetLocalizedFriendlyName(authMethod.Metadata, _workContext.WorkingLanguage.Id)
                });
            }
        }
        
        [NonAction]
        protected CustomerOrderListModel PrepareCustomerOrderListModel(Customer customer, int pageIndex)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var roundingAmount = decimal.Zero;
            var storeScope = _orderSettings.DisplayOrdersOfAllStores ? 0 : _storeContext.CurrentStore.Id;
            var model = new CustomerOrderListModel();

			var orders = _orderService.SearchOrders(storeScope, customer.Id, null, null, null, null, null, null, null, null, pageIndex, _orderSettings.OrderListPageSize);

			var orderModels = orders
				.Select(x =>
				{
					var orderModel = new CustomerOrderListModel.OrderDetailsModel
					{
						Id = x.Id,
						OrderNumber = x.GetOrderNumber(),
						CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
						OrderStatus = x.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
						IsReturnRequestAllowed = _orderProcessingService.IsReturnRequestAllowed(x)
					};

                    var orderTotal = x.GetOrderTotalInCustomerCurrency(_currencyService, _paymentService, out roundingAmount);
                    orderModel.OrderTotal = _priceFormatter.FormatPrice(orderTotal, true, x.CustomerCurrencyCode, false, _workContext.WorkingLanguage);

					return orderModel;
				})
				.ToList();

			model.Orders = new PagedList<CustomerOrderListModel.OrderDetailsModel>(orderModels, orders.PageIndex, orders.PageSize, orders.TotalCount);


			var recurringPayments = _orderService.SearchRecurringPayments(_storeContext.CurrentStore.Id, customer.Id, 0, null);

            foreach (var recurringPayment in recurringPayments)
            {
                var recurringPaymentModel = new CustomerOrderListModel.RecurringOrderModel
                {
                    Id = recurringPayment.Id,
                    StartDate = _dateTimeHelper.ConvertToUserTime(recurringPayment.StartDateUtc, DateTimeKind.Utc).ToString(),
                    CycleInfo = string.Format("{0} {1}", recurringPayment.CycleLength, recurringPayment.CyclePeriod.GetLocalizedEnum(_localizationService, _workContext)),
                    NextPayment = recurringPayment.NextPaymentDate.HasValue ? _dateTimeHelper.ConvertToUserTime(recurringPayment.NextPaymentDate.Value, DateTimeKind.Utc).ToString() : "",
                    TotalCycles = recurringPayment.TotalCycles,
                    CyclesRemaining = recurringPayment.CyclesRemaining,
                    InitialOrderId = recurringPayment.InitialOrder.Id,
                    CanCancel = _orderProcessingService.CanCancelRecurringPayment(customer, recurringPayment),
                };

                model.RecurringOrders.Add(recurringPaymentModel);
            }

            return model;
        }

        #endregion

        #region Login / logout / register
        
        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult Login(bool? checkoutAsGuest)
        {
            var model = new LoginModel();
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.CheckoutAsGuest = checkoutAsGuest ?? false;
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage;
            
            if (_customerSettings.PrefillLoginUsername.HasValue())
            {
                if (model.UsernamesEnabled)
                    model.Username = _customerSettings.PrefillLoginUsername;
                else
                    model.Email = _customerSettings.PrefillLoginUsername;
                
            }
            if (_customerSettings.PrefillLoginPwd.HasValue())
            {
                model.Password = _customerSettings.PrefillLoginPwd;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateCaptcha]
        public ActionResult Login(LoginModel model, string returnUrl, bool captchaValid)
        {
            // Validate CAPTCHA.
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptcha"));
            }

            if (ModelState.IsValid)
            {
                if (_customerSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }

                if (_customerRegistrationService.ValidateCustomer(_customerSettings.UsernamesEnabled ? model.Username : model.Email, model.Password))
                {
                    var customer = _customerSettings.UsernamesEnabled 
						? _customerService.GetCustomerByUsername(model.Username) 
						: _customerService.GetCustomerByEmail(model.Email);

                    _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, customer);

                    _authenticationService.SignIn(customer, model.RememberMe);

                    _customerActivityService.InsertActivity("PublicStore.Login", _localizationService.GetResource("ActivityLog.PublicStore.Login"), customer);

					_eventPublisher.Publish(new CustomerLogedInEvent { Customer = customer });

					// Redirect home where redirect to referrer would be confusing.
					if (returnUrl.IsEmpty() || returnUrl.Contains(@"/login?") || returnUrl.Contains(@"/passwordrecoveryconfirm"))
					{
						return RedirectToRoute("HomePage");
					}

					return RedirectToReferrer(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials"));
                }
            }

            // If we got this far, something failed, redisplay form.
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage;

            return View(model);
        }

        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
		[GdprConsent]
		public ActionResult Register()
        {
			//check whether registration is allowed
			if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
			{
				return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });
			}

            var model = new RegisterModel();
            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            model.VatRequired = _taxSettings.VatRequired;
            //form fields
            model.GenderEnabled = _customerSettings.GenderEnabled;
			model.FirstNameRequired = _customerSettings.FirstNameRequired;
			model.LastNameRequired = _customerSettings.LastNameRequired;
			model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.CompanyRequired = _customerSettings.CompanyRequired;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _customerSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _customerSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _customerSettings.ZipPostalCodeRequired;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.CityRequired = _customerSettings.CityRequired;
            model.CountryEnabled = _customerSettings.CountryEnabled;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.PhoneRequired = _customerSettings.PhoneRequired;
            model.FaxEnabled = _customerSettings.FaxEnabled;
            model.FaxRequired = _customerSettings.FaxRequired;
            model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.CheckUsernameAvailabilityEnabled = _customerSettings.CheckUsernameAvailabilityEnabled;
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage;

			foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
			{
				model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (tzi.Id == _dateTimeHelper.DefaultStoreTimeZone.Id) });
			}

			if (_customerSettings.CountryEnabled)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
                foreach (var c in _countryService.GetAllCountries())
                {
                    model.AvailableCountries.Add(new SelectListItem { Text = c.GetLocalized(x => x.Name), Value = c.Id.ToString() });
                }
                
                if (_customerSettings.StateProvinceEnabled)
                {
                    var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId).ToList();
					if (states.Count > 0)
					{
						foreach (var s in states)
						{
							model.AvailableStates.Add(new SelectListItem { Text = s.GetLocalized(x => x.Name), Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
						}
					}
					else
					{
						model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "0" });
					}
                }
            }

            return View(model);
        }

        [HttpPost]
		[GdprConsent]
		[ValidateAntiForgeryToken, ValidateCaptcha, ValidateHoneypot]
		public ActionResult Register(RegisterModel model, string returnUrl, bool captchaValid)
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });
            
            if (_workContext.CurrentCustomer.IsRegistered())
            {
                // Already registered customer. 
                _authenticationService.SignOut();
                
                // Save a new record
                _workContext.CurrentCustomer = null;
            }

            var customer = _workContext.CurrentCustomer;

            // validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptcha"));
            }
            
            if (ModelState.IsValid)
            {
                if (_customerSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }

                bool isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
                var registrationRequest = new CustomerRegistrationRequest(customer, model.Email,
                    _customerSettings.UsernamesEnabled ? model.Username : model.Email, model.Password, _customerSettings.DefaultPasswordFormat, isApproved);
                var registrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);

                if (registrationResult.Success)
                {
                    // properties
					if (_dateTimeSettings.AllowCustomersToSetTimeZone)
					{
						_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId, model.TimeZoneId);
					}

                    // VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
						_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumber, model.VatNumber);

						var vatNumberStatus = _taxService.GetVatNumberStatus(model.VatNumber, out var vatName, out var vatAddress);
						_genericAttributeService.SaveAttribute(customer,
							SystemCustomerAttributeNames.VatNumberStatusId,
							(int)vatNumberStatus);
						// send VAT number admin notification
						if (!String.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
							Services.MessageFactory.SendNewVatSubmittedStoreOwnerNotification(customer, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                    }

					// form fields
					customer.FirstName = model.FirstName;
					customer.LastName = model.LastName;

					if (_customerSettings.CompanyEnabled)
						customer.Company = model.Company;

					if (_customerSettings.DateOfBirthEnabled)
					{
						try
						{
							customer.BirthDate = new DateTime(model.DateOfBirthYear.Value, model.DateOfBirthMonth.Value, model.DateOfBirthDay.Value);
						}
						catch { }
					}

					if (_customerSettings.CustomerNumberMethod == CustomerNumberMethod.AutomaticallySet && customer.CustomerNumber.IsEmpty())
						customer.CustomerNumber = customer.Id.Convert<string>();

					if (_customerSettings.GenderEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
                    if (_customerSettings.StreetAddressEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress, model.StreetAddress);
                    if (_customerSettings.StreetAddress2Enabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2, model.StreetAddress2);
                    if (_customerSettings.ZipPostalCodeEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode, model.ZipPostalCode);
                    if (_customerSettings.CityEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
                    if (_customerSettings.CountryEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, model.CountryId);
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvinceId, model.StateProvinceId);
                    if (_customerSettings.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
                    if (_customerSettings.FaxEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);

                    // newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        // save newsletter value
                        var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(model.Email, _storeContext.CurrentStore.Id);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                newsletter.Active = true;
                                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                            }
                            //else
                            //{
                                //When registering, not checking the newsletter check box should not take an existing email address off of the subscription list.
                                //_newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                            //}
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = model.Email,
                                    Active = true,
                                    CreatedOnUtc = DateTime.UtcNow,
									StoreId = _storeContext.CurrentStore.Id,
                                    WorkingLanguageId = _services.WorkContext.WorkingLanguage.Id
                                });
                            }
                        }
                    }

                    // Login customer now
                    if (isApproved)
                        _authenticationService.SignIn(customer, true);

                    // Associated with external account (if possible)
                    TryAssociateAccountWithExternalAccount(customer);
                    
                    // Insert default address (if possible)
                    var defaultAddress = new Address
                    {
						Title = customer.Title,
						FirstName = customer.FirstName,
                        LastName = customer.LastName,
                        Email = customer.Email,
                        Company = customer.Company,
                        CountryId = customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId) > 0 ? 
                            (int?)customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId) : null,
                        StateProvinceId = customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId) > 0 ?
                            (int?)customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId) : null,
                        City = customer.GetAttribute<string>(SystemCustomerAttributeNames.City),
                        Address1 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress),
                        Address2 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2),
                        ZipPostalCode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode),
                        PhoneNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                        FaxNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax),
                        CreatedOnUtc = customer.CreatedOnUtc
                    };

                    if (_addressService.IsAddressValid(defaultAddress))
                    {
                        // Some validation
                        if (defaultAddress.CountryId == 0)
                            defaultAddress.CountryId = null;
                        if (defaultAddress.StateProvinceId == 0)
                            defaultAddress.StateProvinceId = null;
                        // Set default address
                        customer.Addresses.Add(defaultAddress);
                        customer.BillingAddress = defaultAddress;
                        customer.ShippingAddress = defaultAddress;    
                    }

					_customerService.UpdateCustomer(customer);

					// Notifications
					if (_customerSettings.NotifyNewCustomerRegistration)
                        Services.MessageFactory.SendCustomerRegisteredNotificationMessage(customer, _localizationSettings.DefaultAdminLanguageId);
                    
                    switch (_customerSettings.UserRegistrationType)
                    {
                        case UserRegistrationType.EmailValidation:
                            {
                                // email validation message
                                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AccountActivationToken, Guid.NewGuid().ToString());
								Services.MessageFactory.SendCustomerEmailValidationMessage(customer, _workContext.WorkingLanguage.Id);

                                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.EmailValidation });
                            }
                        case UserRegistrationType.AdminApproval:
                            {
                                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.AdminApproval });
                            }
                        case UserRegistrationType.Standard:
                            {
								// send customer welcome message
								Services.MessageFactory.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);

                                var redirectUrl = Url.RouteUrl("RegisterResult", new { resultId = (int)UserRegistrationType.Standard });
                                if (!String.IsNullOrEmpty(returnUrl))
                                    redirectUrl = _webHelper.ModifyQueryString(redirectUrl, "returnurl=" + HttpUtility.UrlEncode(returnUrl), null);
                                return Redirect(redirectUrl);
                            }
                        default:
                            {
                                return RedirectToRoute("HomePage");
                            }
                    }
                }
                else
                {
                    foreach (var error in registrationResult.Errors)
                        ModelState.AddModelError("", error);
                }
            }

            //If we got this far, something failed, redisplay form
            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                model.AvailableTimeZones.Add(new SelectListItem() { Text = tzi.DisplayName, Value = tzi.Id, Selected = (tzi.Id == _dateTimeHelper.DefaultStoreTimeZone.Id) });
            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            model.VatRequired = _taxSettings.VatRequired;
            //form fields
            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
			model.FirstNameRequired = _customerSettings.FirstNameRequired;
			model.LastNameRequired = _customerSettings.LastNameRequired;
			model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.CompanyRequired = _customerSettings.CompanyRequired;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _customerSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _customerSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _customerSettings.ZipPostalCodeRequired;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.CityRequired = _customerSettings.CityRequired;
            model.CountryEnabled = _customerSettings.CountryEnabled;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.PhoneRequired = _customerSettings.PhoneRequired;
            model.FaxEnabled = _customerSettings.FaxEnabled;
            model.FaxRequired = _customerSettings.FaxRequired;
            model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.CheckUsernameAvailabilityEnabled = _customerSettings.CheckUsernameAvailabilityEnabled;
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage;

            if (_customerSettings.CountryEnabled)
            {
                model.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
                foreach (var c in _countryService.GetAllCountries())
                {
                    model.AvailableCountries.Add(new SelectListItem() { Text = c.GetLocalized(x => x.Name), Value = c.Id.ToString(), Selected = (c.Id == model.CountryId) });
                }


                if (_customerSettings.StateProvinceEnabled)
                {
                    //states
                    var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId).ToList();
                    if (states.Count > 0)
                    {
                        foreach (var s in states)
                            model.AvailableStates.Add(new SelectListItem() { Text = s.GetLocalized(x => x.Name), Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                    }
                    else
                        model.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "0" });

                }
            }

            return View(model);
        }

        public ActionResult RegisterResult(int resultId)
        {
            var resultText = "";
            switch ((UserRegistrationType)resultId)
            {
                case UserRegistrationType.Disabled:
                    resultText = _localizationService.GetResource("Account.Register.Result.Disabled");
                    break;
                case UserRegistrationType.Standard:
                    resultText = _localizationService.GetResource("Account.Register.Result.Standard");
                    break;
                case UserRegistrationType.AdminApproval:
                    resultText = _localizationService.GetResource("Account.Register.Result.AdminApproval");
                    break;
                case UserRegistrationType.EmailValidation:
                    resultText = _localizationService.GetResource("Account.Register.Result.EmailValidation");
                    break;
                default:
                    break;
            }

            var model = new RegisterResultModel { Result = resultText };
            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CheckUsernameAvailability(string username)
        {
            var usernameAvailable = false;
            var statusText = _localizationService.GetResource("Account.CheckUsernameAvailability.NotAvailable");

            if (_customerSettings.UsernamesEnabled && username != null)
            {
                username = username.Trim();

                if (UsernameIsValid(username))
                {
                    if (_workContext.CurrentCustomer != null && 
                        _workContext.CurrentCustomer.Username != null && 
                        _workContext.CurrentCustomer.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                    {
                        statusText = _localizationService.GetResource("Account.CheckUsernameAvailability.CurrentUsername");
                    }
                    else
                    {
                        var customer = _customerService.GetCustomerByUsername(username);
                        if (customer == null)
                        {
                            statusText = _localizationService.GetResource("Account.CheckUsernameAvailability.Available");
                            usernameAvailable = true;
                        }
                    }
                }
            }

            return Json(new { Available = usernameAvailable, Text = statusText });
        }
        
        public ActionResult Logout()
        {
            //external authentication
            ExternalAuthorizerHelper.RemoveParameters();

            if (_workContext.OriginalCustomerIfImpersonated != null)
            {
                //logout impersonated customer
                _genericAttributeService.SaveAttribute<int?>(_workContext.OriginalCustomerIfImpersonated,
                    SystemCustomerAttributeNames.ImpersonatedCustomerId, null);
                //redirect back to customer details page (admin area)
                return this.RedirectToAction("Edit", "Customer", new { id = _workContext.CurrentCustomer.Id, area = "Admin" });

            }
            else
            {
                //standard logout 

                //activity log
                _customerActivityService.InsertActivity("PublicStore.Logout", _localizationService.GetResource("ActivityLog.PublicStore.Logout"));

                _authenticationService.SignOut();
                return RedirectToRoute("HomePage");
            }

        }

        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult AccountActivation(string token, string email)
        {
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
            {
                NotifyError(T("Account.AccountActivation.InvalidEmailOrToken"));
                return RedirectToRoute("HomePage");
            }

            var cToken = customer.GetAttribute<string>(SystemCustomerAttributeNames.AccountActivationToken);
            if (cToken.IsEmpty() || !cToken.Equals(token, StringComparison.InvariantCultureIgnoreCase))
            {
                NotifyError(T("Account.AccountActivation.InvalidEmailOrToken"));
                return RedirectToRoute("HomePage");
            }
			
            // Activate user account.
            customer.Active = true;
            _customerService.UpdateCustomer(customer);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AccountActivationToken, "");

			// Send welcome message.
			Services.MessageFactory.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);
            
            var model = new AccountActivationModel();
            model.Result = T("Account.AccountActivation.Activated");

            return View(model);
        }

        #endregion

		[ChildActionOnly]
		public ActionResult MyAccountMenu(string selectedItem = null)
		{
			var customer = _workContext.CurrentCustomer;
			var menu = new List<MenuItem>();

			// Info
			menu.Add(new MenuItem
			{
				Id = "info",
				Text = T("Account.CustomerInfo"),
				Icon = "user-o",
				Url = Url.Action("Info"),
			});

			// Addresses
			menu.Add(new MenuItem
			{
				Id = "addresses",
				Text = T("Account.CustomerAddresses"),
				Icon = "address-book-o",
				Url = Url.Action("Addresses"),
			});

			// Orders
			menu.Add(new MenuItem
			{
				Id = "orders",
				Text = T("Account.CustomerOrders"),
				Icon = "file-text",
				Url = Url.Action("Orders"),
			});

			// Return requests
			if (_orderSettings.ReturnRequestsEnabled && _orderService.SearchReturnRequests(_storeContext.CurrentStore.Id, customer.Id, 0, null, 0, 1).TotalCount > 0)
			{
				menu.Add(new MenuItem
				{
					Id = "returnrequests",
					Text = T("Account.CustomerReturnRequests"),
					Icon = "truck",
					Url = Url.Action("ReturnRequests"),
				});
			}

			// Downloadable products
			if (!_customerSettings.HideDownloadableProductsTab)
			{
				menu.Add(new MenuItem
				{
					Id = "downloads",
					Text = T("Account.DownloadableProducts"),
					Icon = "download",
					Url = Url.Action("DownloadableProducts"),
				});
			}

			// BackInStock subscriptions
			if (!_customerSettings.HideBackInStockSubscriptionsTab)
			{
				menu.Add(new MenuItem
				{
					Id = "backinstock",
					Text = T("Account.BackInStockSubscriptions"),
					Icon = "bullhorn",
					Url = Url.Action("BackInStockSubscriptions"),
				});
			}

			// Reward points
			if (_rewardPointsSettings.Enabled)
			{
				menu.Add(new MenuItem
				{
					Id = "rewardpoints",
					Text = T("Account.RewardPoints"),
					Icon = "certificate",
					Url = Url.Action("RewardPoints"),
				});
			}

			// Change password
			menu.Add(new MenuItem
			{
				Id = "changepassword",
				Text = T("Account.ChangePassword"),
				Icon = "unlock-alt",
				Url = Url.Action("ChangePassword"),
			});

			// Avatar
			if (_customerSettings.AllowCustomersToUploadAvatars)
			{
				menu.Add(new MenuItem
				{
					Id = "avatar",
					Text = T("Account.Avatar"),
					Icon = "user-circle",
					Url = Url.Action("Avatar"),
				});
			}

			// Forum subscriptions
			if (_forumSettings.ForumsEnabled && _forumSettings.AllowCustomersToManageSubscriptions)
			{
				menu.Add(new MenuItem
				{
					Id = "forumsubscriptions",
					Text = T("Account.ForumSubscriptions"),
					Icon = "bell",
					Url = Url.Action("ForumSubscriptions"),
				});
			}

			// Private messages
			var numUnreadMessages = GetUnreadPrivateMessages();
			if (_forumSettings.AllowPrivateMessages)
			{
				menu.Add(new MenuItem
				{
					Id = "privatemessages",
					Text = T("PrivateMessages.Inbox"),
					Icon = "envelope-o",
					Url = Url.RouteUrl("PrivateMessages", new { tab = "inbox" }),
					BadgeText = numUnreadMessages > 0 ? numUnreadMessages.ToString() : null,
					BadgeStyle = BadgeStyle.Warning
				});
			}

			var model = new MyAccountMenuModel
			{
				Root = new TreeNode<MenuItem>(new MenuItem { Id = "root" }, menu),
				SelectedItemToken = selectedItem
			};

			// Event for plugins
			var evt = new MenuCreatedEvent("MyAccount", model.Root, model.SelectedItemToken);
			_services.EventPublisher.Publish(evt);
			model.SelectedItemToken = evt.SelectedItemToken;

			return PartialView(model);
		}

        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult Info()
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            var model = new CustomerInfoModel();
            PrepareCustomerInfoModel(model, customer, false);
			
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Info(CustomerInfoModel model)
        {
            if (!IsCurrentUserRegistered())
            {
                return new HttpUnauthorizedResult();
            }

            var customer = _workContext.CurrentCustomer;

            if (model.Email.IsEmpty())
			{
				ModelState.AddModelError("", "Email is not provided.");
			}               
            if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames && model.Username.IsEmpty())
            {
				ModelState.AddModelError("", "Username is not provided.");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    customer.FirstName = model.FirstName;
                    customer.LastName = model.LastName;

                    // Username.
                    if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames)
                    {
                        if (!customer.Username.Equals(model.Username.Trim(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Change username.
                            _customerRegistrationService.SetUsername(customer, model.Username.Trim());
                            // Re-authenticate.
                            _authenticationService.SignIn(customer, true);
                        }
                    }

                    // Email.
                    if (!customer.Email.Equals(model.Email.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Change email.
                        _customerRegistrationService.SetEmail(customer, model.Email.Trim());
                        // Re-authenticate (if usernames are disabled).
                        if (!_customerSettings.UsernamesEnabled)
                        {
                            _authenticationService.SignIn(customer, true);
                        }
                    }

                    // VAT number.
                    if (_taxSettings.EuVatEnabled)
                    {
						var prevVatNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);
						_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumber, model.VatNumber);

						if (prevVatNumber != model.VatNumber)
						{
							var vatNumberStatus = _taxService.GetVatNumberStatus(model.VatNumber, out var vatName, out var vatAddress);
							_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumberStatusId, (int)vatNumberStatus);

                            // Send VAT number admin notification.
                            if (model.VatNumber.HasValue() && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                            {
                                Services.MessageFactory.SendNewVatSubmittedStoreOwnerNotification(customer, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                            }
						}
                    }

					// Customer number.
					if (_customerSettings.CustomerNumberMethod != CustomerNumberMethod.Disabled)
					{
						var numberExists = _customerService.SearchCustomers(new CustomerSearchQuery { CustomerNumber = model.CustomerNumber }).SourceQuery.Any();
						if (model.CustomerNumber != customer.CustomerNumber && numberExists)
						{
							NotifyError("Common.CustomerNumberAlreadyExists");
						}
						else
						{
							customer.CustomerNumber = model.CustomerNumber;
						}
					}

                    if (_customerSettings.DateOfBirthEnabled)
                    {
                        try
                        {
                            if (model.DateOfBirthYear.HasValue && model.DateOfBirthMonth.HasValue && model.DateOfBirthDay.HasValue)
                            {
                                customer.BirthDate = new DateTime(model.DateOfBirthYear.Value, model.DateOfBirthMonth.Value, model.DateOfBirthDay.Value);
                            }
                            else
                            {
                                customer.BirthDate = null;
                            }
						}
                        catch { }
                    }

                    if (_customerSettings.CompanyEnabled)
                    {
                        customer.Company = model.Company;
                    }
                    if (_customerSettings.TitleEnabled)
                    {
                        customer.Title = model.Title;
                    }
                    if (_customerSettings.GenderEnabled)
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
                    }
                    if (_customerSettings.StreetAddressEnabled)
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress, model.StreetAddress);
                    }
                    if (_customerSettings.StreetAddress2Enabled)
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2, model.StreetAddress2);
                    }
                    if (_customerSettings.ZipPostalCodeEnabled)
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode, model.ZipPostalCode);
                    }
                    if (_customerSettings.CityEnabled)
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
                    }
                    if (_customerSettings.CountryEnabled)
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, model.CountryId);
                    }
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvinceId, model.StateProvinceId);
                    }
                    if (_customerSettings.PhoneEnabled)
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
                    }
                    if (_customerSettings.FaxEnabled)
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);
                    }
                    if (_customerSettings.NewsletterEnabled)
                    {
						_newsLetterSubscriptionService.AddNewsLetterSubscriptionFor(model.Newsletter, customer.Email, _storeContext.CurrentStore.Id);
                    }
					if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
					{
						_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Signature, model.Signature);
					}
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId, model.TimeZoneId);
                    }

                    _customerService.UpdateCustomer(customer);

                    return RedirectToAction("Info");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            // If we got this far, something failed, redisplay form.
            PrepareCustomerInfoModel(model, customer, true);
            return View(model);
        }
    
        #region Addresses

        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult Addresses()
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            var model = new CustomerAddressListModel();
            foreach (var address in customer.Addresses)
            {
                var addressModel = new AddressModel();
                addressModel.PrepareModel(address, false, _addressSettings, _localizationService,
                    _stateProvinceService, () => _countryService.GetAllCountries());
                model.Addresses.Add(addressModel);
            }
            return View(model);
        }

        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult AddressDelete(int id)
        {
			if (id < 1)
				return HttpNotFound();
			
			if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            //find address (ensure that it belongs to the current customer)
            var address = customer.Addresses.Where(a => a.Id == id).FirstOrDefault();
            if (address != null)
            {
                customer.RemoveAddress(address);
                _customerService.UpdateCustomer(customer);
                //now delete the address record
                _addressService.DeleteAddress(address);
            }

			return RedirectToAction("Addresses");
        }

        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult AddressAdd()
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            var model = new CustomerAddressEditModel();
            model.Address.PrepareModel(null, false, _addressSettings, _localizationService, _stateProvinceService, () => _countryService.GetAllCountries());
            model.Address.Email = customer?.Email;

            return View(model);
        }

        [HttpPost]
        public ActionResult AddressAdd(CustomerAddressEditModel model)
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;


            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity();
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;
                customer.Addresses.Add(address);
                _customerService.UpdateCustomer(customer);

				return RedirectToAction("Addresses");
            }


            // If we got this far, something failed, redisplay form
            model.Address.PrepareModel(null, true, _addressSettings, _localizationService, _stateProvinceService, () => _countryService.GetAllCountries());

            return View(model);
        }

        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult AddressEdit(int id)
        {
			if (id < 1)
				return HttpNotFound();
			
			if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;
            //find address (ensure that it belongs to the current customer)
            var address = customer.Addresses.Where(a => a.Id == id).FirstOrDefault();
            if (address == null)
                //address is not found
				return RedirectToAction("Addresses");

            var model = new CustomerAddressEditModel();
            model.Address.PrepareModel(address, false, _addressSettings, _localizationService,  _stateProvinceService, () => _countryService.GetAllCountries());

            return View(model);
        }

        [HttpPost]
        public ActionResult AddressEdit(CustomerAddressEditModel model, int id)
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;
            //find address (ensure that it belongs to the current customer)
            var address = customer.Addresses.Where(a => a.Id == id).FirstOrDefault();
            if (address == null)
                //address is not found
				return RedirectToAction("Addresses");

            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                _addressService.UpdateAddress(address);
				return RedirectToAction("Addresses");
            }

            // If we got this far, something failed, redisplay form
            model.Address.PrepareModel(address, true, _addressSettings, _localizationService, _stateProvinceService, () => _countryService.GetAllCountries());
            return View(model);
        }
           
        #endregion

        #region Orders

        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult Orders(int? page)
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var model = PrepareCustomerOrderListModel(_workContext.CurrentCustomer, Math.Max((page ?? 0) - 1, 0));

            return View(model);
        }

        [HttpPost, ActionName("Orders")]
        [FormValueRequired(FormValueRequirement.StartsWith, "cancelRecurringPayment")]
        public ActionResult CancelRecurringPayment(FormCollection form)
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            //get recurring payment identifier
            int recurringPaymentId = 0;
			foreach (var formValue in form.AllKeys)
			{
				if (formValue.StartsWith("cancelRecurringPayment", StringComparison.InvariantCultureIgnoreCase))
				{
					recurringPaymentId = Convert.ToInt32(formValue.Substring("cancelRecurringPayment".Length));
				}
			}

            var recurringPayment = _orderService.GetRecurringPaymentById(recurringPaymentId);
            if (recurringPayment == null)
            {
                return RedirectToAction("Orders");
            }

            var customer = _workContext.CurrentCustomer;
            if (_orderProcessingService.CanCancelRecurringPayment(customer, recurringPayment))
            {
                var errors = _orderProcessingService.CancelRecurringPayment(recurringPayment);

                var model = PrepareCustomerOrderListModel(customer, 0);
                model.CancelRecurringPaymentErrors = errors;

                return View(model);
            }

			return RedirectToAction("Orders");
		}

        #endregion

        #region Return request

        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult ReturnRequests()
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

			var model = new CustomerReturnRequestsModel();
			var customer = _workContext.CurrentCustomer;		
			var returnRequests = _orderService.SearchReturnRequests(_storeContext.CurrentStore.Id, customer.Id, 0, null, 0, int.MaxValue);

            foreach (var returnRequest in returnRequests)
            {
                var orderItem = _orderService.GetOrderItemById(returnRequest.OrderItemId);
                if (orderItem != null)
                {
                    var itemModel = new CustomerReturnRequestsModel.ReturnRequestModel
                    {
                        Id = returnRequest.Id,
                        ReturnRequestStatus = returnRequest.ReturnRequestStatus.GetLocalizedEnum(_localizationService, _workContext),
                        ProductId = orderItem.Product.Id,
						ProductName = orderItem.Product.GetLocalized(x => x.Name),
                        ProductSeName = orderItem.Product.GetSeName(),
                        Quantity = returnRequest.Quantity,
                        ReturnAction = returnRequest.RequestedAction,
                        ReturnReason = returnRequest.ReasonForReturn,
                        Comments = returnRequest.CustomerComments,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(returnRequest.CreatedOnUtc, DateTimeKind.Utc)
                    };

					itemModel.ProductUrl = _productUrlHelper.GetProductUrl(itemModel.ProductSeName, orderItem);

					model.Items.Add(itemModel);
                }
            }

            return View(model);
        }

        #endregion

        #region Downloable products

        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult DownloadableProducts()
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();
            
            var customer = _workContext.CurrentCustomer;

            var model = new CustomerDownloadableProductsModel();

            var items = _orderService.GetAllOrderItems(null, customer.Id, null, null, null, null, null, true);

            foreach (var item in items)
            {
                var itemModel = new CustomerDownloadableProductsModel.DownloadableProductsModel
                {
                    OrderItemGuid = item.OrderItemGuid,
                    OrderId = item.OrderId,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(item.Order.CreatedOnUtc, DateTimeKind.Utc),
					ProductName = item.Product.GetLocalized(x => x.Name),
                    ProductSeName = item.Product.GetSeName(),
                    ProductAttributes = item.AttributeDescription,
					ProductId = item.ProductId
                };

				itemModel.ProductUrl = _productUrlHelper.GetProductUrl(item.ProductId, itemModel.ProductSeName, item.AttributesXml);

                model.Items.Add(itemModel);
                
                itemModel.IsDownloadAllowed = _downloadService.IsDownloadAllowed(item);

                if (itemModel.IsDownloadAllowed)
                {
                    itemModel.DownloadVersions = _downloadService.GetDownloadsFor(item.Product)
                        .Select(x => new DownloadVersion
                        {
                            FileVersion = x.FileVersion,
                            FileName = string.Concat(x.Filename, x.Extension),
                            DownloadGuid = x.DownloadGuid,
                            Changelog = x.Changelog,
                            DownloadId = x.Id
                        })
                        .ToList();
                }

                if (_downloadService.IsLicenseDownloadAllowed(item))
                    itemModel.LicenseId = item.LicenseDownloadId ?? 0;
            }
            
            return View(model);
        }

        public ActionResult UserAgreement(Guid id /* orderItemId */, string fileVersion = "")
        {
			if (id == Guid.Empty)
				return HttpNotFound();

			var orderItem = _orderService.GetOrderItemByGuid(id);
            if (orderItem == null)
            {
                NotifyError(_services.Localization.GetResource("Customer.UserAgreement.OrderItemNotFound"));
                return RedirectToRoute("HomePage");
            }
			
            var product = orderItem.Product;
            if (product == null || !product.HasUserAgreement)
            {
                NotifyError(_services.Localization.GetResource("Customer.UserAgreement.ProductNotFound"));
                return RedirectToRoute("HomePage");
            }
			
            var model = new UserAgreementModel();
            model.UserAgreementText = product.UserAgreementText;
			model.OrderItemGuid = id;
            model.FileVersion = fileVersion;
            
            return View(model);
        }

        #endregion

        #region Reward points

        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult RewardPoints()
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            if (!_rewardPointsSettings.Enabled)
				return RedirectToAction("Info");

            var customer = _workContext.CurrentCustomer;

            var model = new CustomerRewardPointsModel();
            foreach (var rph in customer.RewardPointsHistory.OrderByDescending(rph => rph.CreatedOnUtc).ThenByDescending(rph => rph.Id))
            {
                model.RewardPoints.Add(new CustomerRewardPointsModel.RewardPointsHistoryModel()
                {
                    Points = rph.Points,
                    PointsBalance = rph.PointsBalance,
                    Message = rph.Message,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(rph.CreatedOnUtc, DateTimeKind.Utc)
                });
            }
            int rewardPointsBalance = customer.GetRewardPointsBalance();
            decimal rewardPointsAmountBase = _orderTotalCalculationService.ConvertRewardPointsToAmount(rewardPointsBalance);
            decimal rewardPointsAmount =_currencyService.ConvertFromPrimaryStoreCurrency(rewardPointsAmountBase,_workContext.WorkingCurrency);
            model.RewardPointsBalance = string.Format(_localizationService.GetResource("RewardPoints.CurrentBalance"), rewardPointsBalance, _priceFormatter.FormatPrice(rewardPointsAmount, true, false));
            
            return View(model);
        }

        #endregion

        #region Change password

        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult ChangePassword()
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var model = new ChangePasswordModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            if (ModelState.IsValid)
            {
                var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                    true, _customerSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
                var changePasswordResult = _customerRegistrationService.ChangePassword(changePasswordRequest);
                if (changePasswordResult.Success)
                {
                    model.Result = _localizationService.GetResource("Account.ChangePassword.Success");
                    return View(model);
                }
                else
                {
                    foreach (var error in changePasswordResult.Errors)
                        ModelState.AddModelError("", error);
                }
            }


            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Avatar

        [RequireHttpsByConfig(SslRequirement.Yes)]
        public ActionResult Avatar()
        {
            if (!IsCurrentUserRegistered())
            {
                return new HttpUnauthorizedResult();
            }

            if (!_customerSettings.AllowCustomersToUploadAvatars)
            {
                return RedirectToAction("Info");
            }

			var model = new CustomerAvatarEditModel();
            model.Avatar = _workContext.CurrentCustomer.ToAvatarModel(_genericAttributeService, _pictureService, _customerSettings, _mediaSettings, Url, null, true);
            model.MaxFileSize = Prettifier.BytesToString(_customerSettings.AvatarMaximumSizeBytes);

            return View(model);
        }

		[HttpPost]
		public ActionResult UploadAvatar()
		{
			var success = false;
			string avatarUrl = null;

			if (IsCurrentUserRegistered() && _customerSettings.AllowCustomersToUploadAvatars)
			{
				var customer = _workContext.CurrentCustomer;
				var uploadedFile = Request.Files["uploadedFile-file"].ToPostedFileResult();

				if (uploadedFile != null && uploadedFile.FileName.HasValue())
				{
					if (uploadedFile.Size > _customerSettings.AvatarMaximumSizeBytes)
					{
						throw new SmartException(T("Account.Avatar.MaximumUploadedFileSize", Prettifier.BytesToString(_customerSettings.AvatarMaximumSizeBytes)));
					}

					var customerAvatar = _pictureService.GetPictureById(customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId));
					if (customerAvatar != null)
					{
						// Remove from cache.
						_pictureService.DeletePicture(customerAvatar);
					}

					customerAvatar = _pictureService.InsertPicture(uploadedFile.Buffer, uploadedFile.ContentType, null, true, false);

					_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AvatarPictureId, customerAvatar.Id);

					avatarUrl = _pictureService.GetUrl(customerAvatar.Id, _mediaSettings.AvatarPictureSize, false);
					success = avatarUrl.HasValue();
				}
			}

			return Json(new { success, avatarUrl });
		}

		[HttpPost]
		public ActionResult RemoveAvatar()
		{
            var customer = _workContext.CurrentCustomer;

            if (IsCurrentUserRegistered() && _customerSettings.AllowCustomersToUploadAvatars)
			{
				var customerAvatar = _pictureService.GetPictureById(customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId));
				if (customerAvatar != null)
				{
					_pictureService.DeletePicture(customerAvatar);
				}

				_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AvatarPictureId, 0);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AvatarColor, (string)null);
			}

            return RedirectToAction("Avatar");
		}

        #endregion

        #region Password recovery

        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult PasswordRecovery()
        {
            var model = new PasswordRecoveryModel();
            return View(model);
        }

        [HttpPost, ActionName("PasswordRecovery")]
        [FormValueRequired("send-email")]
        public ActionResult PasswordRecoverySend(PasswordRecoveryModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = _customerService.GetCustomerByEmail(model.Email);
                if (customer != null && customer.Active && !customer.Deleted)
                {
                    var passwordRecoveryToken = Guid.NewGuid();
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryToken, passwordRecoveryToken.ToString());
					Services.MessageFactory.SendCustomerPasswordRecoveryMessage(customer, _workContext.WorkingLanguage.Id);

                    model.ResultMessage = _localizationService.GetResource("Account.PasswordRecovery.EmailHasBeenSent");
                    model.ResultState = PasswordRecoveryResultState.Success;
                }
                else
                {
                    model.ResultMessage = _localizationService.GetResource("Account.PasswordRecovery.EmailNotFound");
                    model.ResultState = PasswordRecoveryResultState.Error;
                }
                
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }


        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult PasswordRecoveryConfirm(string token, string email)
        {
            var customer = _customerService.GetCustomerByEmail(email);
			customer = Services.WorkContext.CurrentCustomer;

            if (customer == null)
            {
                NotifyError(T("Account.PasswordRecoveryConfirm.InvalidEmailOrToken"));
            }
            
            var model = new PasswordRecoveryConfirmModel();
            return View(model);
        }

        [HttpPost, ActionName("PasswordRecoveryConfirm")]
        [FormValueRequired("set-password")]
        public ActionResult PasswordRecoveryConfirmPOST(string token, string email, PasswordRecoveryConfirmModel model)
        {
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
            {
                NotifyError(T("Account.PasswordRecoveryConfirm.InvalidEmailOrToken"));
                return PasswordRecoveryConfirm(token, email);
            }

            var cPrt = customer.GetAttribute<string>(SystemCustomerAttributeNames.PasswordRecoveryToken);
            if (cPrt.IsEmpty() || !cPrt.Equals(token, StringComparison.InvariantCultureIgnoreCase))
            {
				NotifyError(T("Account.PasswordRecoveryConfirm.InvalidEmailOrToken"));
				return PasswordRecoveryConfirm(token, email);
			}
			
            if (ModelState.IsValid)
            {
                var response = _customerRegistrationService.ChangePassword(new ChangePasswordRequest(email,
                    false, _customerSettings.DefaultPasswordFormat, model.NewPassword));
                if (response.Success)
                {
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryToken, "");

                    model.SuccessfullyChanged = true;
                    model.Result = T("Account.PasswordRecovery.PasswordHasBeenChanged");
                }
                else
                {
                    model.Result = response.Errors.FirstOrDefault();
                }

                return View(model);
            }

            // If we got this far, something failed, redisplay form.
            return View(model);
        }

        #endregion

        #region Forum subscriptions

        public ActionResult ForumSubscriptions(int? page)
        {
            if (!_forumSettings.AllowCustomersToManageSubscriptions)
            {
				return RedirectToAction("Info");
            }

            int pageIndex = 0;
            if (page > 0)
            {
                pageIndex = page.Value - 1;
            }

            var customer = _workContext.CurrentCustomer;

            var pageSize = _forumSettings.ForumSubscriptionsPageSize;

            var list = _forumService.GetAllSubscriptions(customer.Id, 0, 0, pageIndex, pageSize);

            var model = new CustomerForumSubscriptionsModel(list);

            foreach (var forumSubscription in list)
            {
                var forumTopicId = forumSubscription.TopicId;
                var forumId = forumSubscription.ForumId;
                bool topicSubscription = false;
				string title = String.Empty;
                var slug = string.Empty;

                if (forumTopicId > 0)
                {
                    topicSubscription = true;
                    var forumTopic = _forumService.GetTopicById(forumTopicId);
                    if (forumTopic != null)
                    {
                        title = forumTopic.Subject;
                        slug = forumTopic.GetSeName();
                    }
                }
                else
                {
                    var forum = _forumService.GetForumById(forumId);
                    if (forum != null)
                    {
                        title = forum.GetLocalized(x => x.Name);
                        slug = forum.GetSeName();
                    }
                }

                model.ForumSubscriptions.Add(new ForumSubscriptionModel
                {
                    Id = forumSubscription.Id,
                    ForumTopicId = forumTopicId,
                    ForumId = forumSubscription.ForumId,
                    TopicSubscription = topicSubscription,
                    Title = title,
                    Slug = slug,
                });
            }

            return View(model);
        }

        [HttpPost, ActionName("ForumSubscriptions")]
        public ActionResult ForumSubscriptionsPOST(FormCollection formCollection)
        {
            foreach (var key in formCollection.AllKeys)
            {
                var value = formCollection[key];

                if (value.Equals("on") && key.StartsWith("fs", StringComparison.InvariantCultureIgnoreCase))
                {
                    var id = key.Replace("fs", "").Trim();

                    if (Int32.TryParse(id, out var forumSubscriptionId))
                    {
                        var forumSubscription = _forumService.GetSubscriptionById(forumSubscriptionId);
                        if (forumSubscription != null && forumSubscription.CustomerId == _workContext.CurrentCustomer.Id)
                        {
                            _forumService.DeleteSubscription(forumSubscription);
                        }
                    }
                }
            }

            return RedirectToAction("ForumSubscriptions");
        }

        public ActionResult DeleteForumSubscription(int id)
        {
			if (id < 1)
				return HttpNotFound();

			var forumSubscription = _forumService.GetSubscriptionById(id);
            if (forumSubscription != null && forumSubscription.CustomerId == _workContext.CurrentCustomer.Id)
            {
                _forumService.DeleteSubscription(forumSubscription);
            }

			return RedirectToAction("ForumSubscriptions");
        }

        #endregion

        #region Back in stock  subscriptions

        public ActionResult BackInStockSubscriptions(int? page)
        {
            if (_customerSettings.HideBackInStockSubscriptionsTab)
            {
				return RedirectToAction("Info");
            }

            int pageIndex = 0;
            if (page > 0)
            {
                pageIndex = page.Value - 1;
            }

            var customer = _workContext.CurrentCustomer;
            var pageSize = 10;
			var list = _backInStockSubscriptionService.GetAllSubscriptionsByCustomerId(customer.Id, _storeContext.CurrentStore.Id, pageIndex, pageSize);

            var model = new CustomerBackInStockSubscriptionsModel(list);

            foreach (var subscription in list)
            {
                var product = subscription.Product;

                if (product != null)
                {
                    var subscriptionModel = new BackInStockSubscriptionModel()
                    {
                        Id = subscription.Id,
                        ProductId = product.Id,
						ProductName = product.GetLocalized(x => x.Name),
                        SeName = product.GetSeName(),
                    };
                    model.Subscriptions.Add(subscriptionModel);
                }
            }

            return View(model);
        }

        [HttpPost, ActionName("BackInStockSubscriptions")]
        public ActionResult BackInStockSubscriptionsPOST(FormCollection formCollection)
        {
            foreach (var key in formCollection.AllKeys)
            {
                var value = formCollection[key];

                if (value.Equals("on") && key.StartsWith("biss", StringComparison.InvariantCultureIgnoreCase))
                {
                    var id = key.Replace("biss", "").Trim();

                    if (Int32.TryParse(id, out var subscriptionId))
                    {
                        var subscription = _backInStockSubscriptionService.GetSubscriptionById(subscriptionId);
                        if (subscription != null && subscription.CustomerId == _workContext.CurrentCustomer.Id)
                        {
                            _backInStockSubscriptionService.DeleteSubscription(subscription);
                        }
                    }
                }
            }

            return RedirectToAction("BackInStockSubscriptions");
        }

        public ActionResult DeleteBackInStockSubscription(int id /* subscriptionId */)
        {
            var subscription = _backInStockSubscriptionService.GetSubscriptionById(id);
            if (subscription != null && subscription.CustomerId == _workContext.CurrentCustomer.Id)
            {
                _backInStockSubscriptionService.DeleteSubscription(subscription);
            }

			return RedirectToAction("BackInStockSubscriptions");
        }

        #endregion
    }
}
