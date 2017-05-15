using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SmartStore.Admin.Models.Common;
using SmartStore.Admin.Models.Customers;
using SmartStore.Admin.Models.ShoppingCart;
using SmartStore.Core;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Services.Affiliates;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Forums;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public partial class CustomerController : AdminControllerBase
    {
        #region Fields

        private readonly ICustomerService _customerService;
		private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerReportService _customerReportService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly TaxSettings _taxSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressService _addressService;
        private readonly CustomerSettings _customerSettings;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderService _orderService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPermissionService _permissionService;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ForumSettings _forumSettings;
        private readonly IForumService _forumService;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly AddressSettings _addressSettings;
		private readonly IStoreService _storeService;
		private readonly IEventPublisher _eventPublisher;
		private readonly PluginMediator _pluginMediator;
		private readonly IAffiliateService _affiliateService;

		#endregion

		#region Constructors

		public CustomerController(ICustomerService customerService,
			INewsLetterSubscriptionService newsLetterSubscriptionService,
            IGenericAttributeService genericAttributeService,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerReportService customerReportService, IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService, DateTimeSettings dateTimeSettings,
            TaxSettings taxSettings, RewardPointsSettings rewardPointsSettings,
            ICountryService countryService, IStateProvinceService stateProvinceService, 
            IAddressService addressService,
            CustomerSettings customerSettings, ITaxService taxService,
			IWorkContext workContext, IStoreContext storeContext, 
			IPriceFormatter priceFormatter,
            IOrderService orderService,
            ICustomerActivityService customerActivityService,
            IPriceCalculationService priceCalculationService,
            IPermissionService permissionService, AdminAreaSettings adminAreaSettings,
            IQueuedEmailService queuedEmailService, EmailAccountSettings emailAccountSettings,
            IEmailAccountService emailAccountService, ForumSettings forumSettings,
            IForumService forumService, IOpenAuthenticationService openAuthenticationService,
			AddressSettings addressSettings, IStoreService storeService,
			IEventPublisher eventPublisher,
			PluginMediator pluginMediator,
			IAffiliateService affiliateService)
		{
            this._customerService = customerService;
			this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._genericAttributeService = genericAttributeService;
            this._customerRegistrationService = customerRegistrationService;
            this._customerReportService = customerReportService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._dateTimeSettings = dateTimeSettings;
            this._taxSettings = taxSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._addressService = addressService;
            this._customerSettings = customerSettings;
            this._taxService = taxService;
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._priceFormatter = priceFormatter;
            this._orderService = orderService;
            this._customerActivityService = customerActivityService;
            this._priceCalculationService = priceCalculationService;
            this._permissionService = permissionService;
            this._adminAreaSettings = adminAreaSettings;
            this._queuedEmailService = queuedEmailService;
            this._emailAccountSettings = emailAccountSettings;
            this._emailAccountService = emailAccountService;
            this._forumSettings = forumSettings;
            this._forumService = forumService;
            this._openAuthenticationService = openAuthenticationService;
            this._addressSettings = addressSettings;
			this._storeService = storeService;
			this._eventPublisher = eventPublisher;
			this._pluginMediator = pluginMediator;
			this._affiliateService = affiliateService;
		}

        #endregion

        #region Utilities

        [NonAction]
        protected string GetCustomerRolesNames(IList<CustomerRole> customerRoles, string separator = ",")
        {
            var sb = new StringBuilder();
            for (int i = 0; i < customerRoles.Count; i++)
            {
                sb.Append(customerRoles[i].Name);
                if (i != customerRoles.Count - 1)
                {
                    sb.Append(separator);
                    sb.Append(" ");
                }
            }
            return sb.ToString();
        }

        [NonAction]
        protected IList<RegisteredCustomerReportLineModel> GetReportRegisteredCustomersModel()
        {
            var report = new List<RegisteredCustomerReportLineModel>();
            report.Add(new RegisteredCustomerReportLineModel()
            {
                Period = _localizationService.GetResource("Admin.Customers.Reports.RegisteredCustomers.Fields.Period.7days"),
                Customers = _customerReportService.GetRegisteredCustomersReport(7)
            });

            report.Add(new RegisteredCustomerReportLineModel()
            {
                Period = _localizationService.GetResource("Admin.Customers.Reports.RegisteredCustomers.Fields.Period.14days"),
                Customers = _customerReportService.GetRegisteredCustomersReport(14)
            });
            report.Add(new RegisteredCustomerReportLineModel()
            {
                Period = _localizationService.GetResource("Admin.Customers.Reports.RegisteredCustomers.Fields.Period.month"),
                Customers = _customerReportService.GetRegisteredCustomersReport(30)
            });
            report.Add(new RegisteredCustomerReportLineModel()
            {
                Period = _localizationService.GetResource("Admin.Customers.Reports.RegisteredCustomers.Fields.Period.year"),
                Customers = _customerReportService.GetRegisteredCustomersReport(365)
            });

            return report;
        }

        [NonAction]
        protected IList<CustomerModel.AssociatedExternalAuthModel> GetAssociatedExternalAuthRecords(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var result = new List<CustomerModel.AssociatedExternalAuthModel>();
            foreach (var record in _openAuthenticationService.GetExternalIdentifiersFor(customer))
            {
                var method = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName(record.ProviderSystemName);
                if (method == null)
                    continue;

                result.Add(new CustomerModel.AssociatedExternalAuthModel()
                {
                    Id = record.Id,
                    Email = record.Email,
                    ExternalIdentifier = record.ExternalIdentifier,
					AuthMethodName = _pluginMediator.GetLocalizedFriendlyName(method.Metadata, _workContext.WorkingLanguage.Id)
                });
            }

            return result;
        }

        [NonAction]
        protected CustomerModel PrepareCustomerModelForList(Customer customer)
        {
            return new CustomerModel()
            {
                Id = customer.Id,
                Email = !String.IsNullOrEmpty(customer.Email) ? customer.Email : (customer.IsGuest() ? _localizationService.GetResource("Admin.Customers.Guest") : "".NaIfEmpty()),
                Username = customer.Username,
                FullName = customer.GetFullName(),
                Company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company),
                CustomerNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.CustomerNumber),
                Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                ZipPostalCode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode),
                CustomerRoleNames = GetCustomerRolesNames(customer.CustomerRoles.ToList()),
                Active = customer.Active,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc),
                LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc),
            };
        }

		private void PrepareCustomerModelForCreate(CustomerModel model)
		{
			string timeZoneId = (model.TimeZoneId.HasValue() ? model.TimeZoneId : _dateTimeHelper.DefaultStoreTimeZone.Id);

			model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
			model.AllowUsersToChangeUsernames = _customerSettings.AllowUsersToChangeUsernames;
			model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;

			foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
			{
				model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (tzi.Id == timeZoneId) });
			}

			model.DisplayVatNumber = false;
			model.AvailableCustomerRoles = _customerService
				.GetAllCustomerRoles(true)
				.Select(cr => cr.ToModel())
				.ToList();

			if (model.SelectedCustomerRoleIds == null || model.SelectedCustomerRoleIds.Count() == 0)
			{
                model.SelectedCustomerRoleIds = new int[] { 3 };      // 3 = CustomerRole > registered 
            }

			model.AllowManagingCustomerRoles = _permissionService.Authorize(StandardPermissionProvider.ManageCustomerRoles);
			//form fields
			model.GenderEnabled = _customerSettings.GenderEnabled;
			model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
			model.CompanyEnabled = _customerSettings.CompanyEnabled;
			model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
			model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
			model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
			model.CityEnabled = _customerSettings.CityEnabled;
			model.CountryEnabled = _customerSettings.CountryEnabled;
			model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
			model.PhoneEnabled = _customerSettings.PhoneEnabled;
			model.FaxEnabled = _customerSettings.FaxEnabled;
            model.CustomerNumberEnabled = _customerSettings.CustomerNumberMethod != CustomerNumberMethod.Disabled;
            
			if (_customerSettings.CountryEnabled)
			{
				model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });

				foreach (var c in _countryService.GetAllCountries())
				{
					model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.CountryId) });
				}

				if (_customerSettings.StateProvinceEnabled)
				{
					//states
					var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId).ToList();
					if (states.Count > 0)
					{
						foreach (var s in states)
						{
							model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
						}
					}
					else
					{
						model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });
					}
				}
			}
		}

        [NonAction]
        protected string ValidateCustomerRoles(IList<CustomerRole> customerRoles)
        {
            if (customerRoles == null)
                throw new ArgumentNullException("customerRoles");

            //ensure a customer is not added to both 'Guests' and 'Registered' customer roles
            //ensure that a customer is in at least one required role ('Guests' and 'Registered')
			bool isInGuestsRole = customerRoles.FirstOrDefault(cr => cr.SystemName == SystemCustomerRoleNames.Guests) != null;
			bool isInRegisteredRole = customerRoles.FirstOrDefault(cr => cr.SystemName == SystemCustomerRoleNames.Registered) != null;
            if (isInGuestsRole && isInRegisteredRole)
            {
                //return "The customer cannot be in both 'Guests' and 'Registered' customer roles";
				return String.Format(_localizationService.GetResource("Admin.Customers.CanOnlyBeCustomerOrGuest"),
                    _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Guests).Name,
                    _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered).Name);
            }
            if (!isInGuestsRole && !isInRegisteredRole)
            {
                //return "Add the customer to 'Guests' or 'Registered' customer role";
				return String.Format(_localizationService.GetResource("Admin.Customers.MustBeCustomerOrGuest"),
                    _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Guests).Name,
                    _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered).Name);
            }
            //no errors
            return "";
        }

        #endregion

        #region Customers

        //ajax
        public ActionResult AllCustomerRoles(string label, int selectedId)
        {
            var customerRoles = _customerService.GetAllCustomerRoles(true);
            if (label.HasValue())
            {
                customerRoles.Insert(0, new CustomerRole { Name = label, Id = 0 });
            }

            var list = from c in customerRoles
                       select new
                       {
                           id = c.Id.ToString(),
                           text = c.Name,
                           selected = c.Id == selectedId
                       };

            return new JsonResult { Data = list.ToList(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //load registered customers by default
            var defaultRoleIds = new[] {_customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered).Id};
            var listModel = new CustomerListModel()
            {
                UsernamesEnabled = _customerSettings.UsernamesEnabled,
                DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled,
                CompanyEnabled = _customerSettings.CompanyEnabled,
                PhoneEnabled = _customerSettings.PhoneEnabled,
                ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled,
                AvailableCustomerRoles = _customerService.GetAllCustomerRoles(true).Select(cr => cr.ToModel()).ToList(),
                SearchCustomerRoleIds = defaultRoleIds,
            };
            var customers = _customerService.GetAllCustomers(null, null, defaultRoleIds, null,
                null, null, null, 0, 0, null, null, null,
                false, null, 0, _adminAreaSettings.GridPageSize);
            //customer list
            listModel.Customers = new GridModel<CustomerModel>
            {
                Data = customers.Select(PrepareCustomerModelForList),
                Total = customers.TotalCount
            };
            return View(listModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult CustomerList(GridCommand command, CustomerListModel model, [ModelBinderAttribute(typeof(CommaSeparatedModelBinder))] int[] searchCustomerRoleIds)
        {
			// we use own own binder for searchCustomerRoleIds property 
			var gridModel = new GridModel<CustomerModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
			{
				var searchDayOfBirth = 0;
				var searchMonthOfBirth = 0;

				if (!String.IsNullOrWhiteSpace(model.SearchDayOfBirth))
					searchDayOfBirth = Convert.ToInt32(model.SearchDayOfBirth);

				if (!String.IsNullOrWhiteSpace(model.SearchMonthOfBirth))
					searchMonthOfBirth = Convert.ToInt32(model.SearchMonthOfBirth);

				var customers = _customerService.GetAllCustomers(null, null,
					searchCustomerRoleIds, model.SearchEmail, model.SearchUsername,
					model.SearchFirstName, model.SearchLastName,
					searchDayOfBirth, searchMonthOfBirth,
					model.SearchCompany, model.SearchPhone, model.SearchZipPostalCode,
					false, null, command.Page - 1, command.PageSize);

				gridModel.Data = customers.Select(PrepareCustomerModelForList);
				gridModel.Total = customers.TotalCount;
			}
			else
			{
				gridModel.Data = Enumerable.Empty<CustomerModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = gridModel
            };
        }
        
        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var model = new CustomerModel();

			PrepareCustomerModelForCreate(model);

            //default value
            model.Active = true;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
		[ValidateInput(false)]
        public ActionResult Create(CustomerModel model, bool continueEditing, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            if (!String.IsNullOrWhiteSpace(model.Email))
            {
                var cust2 = _customerService.GetCustomerByEmail(model.Email);
                if (cust2 != null)
                    ModelState.AddModelError("", "Email is already registered");
            }
            if (!String.IsNullOrWhiteSpace(model.Username) & _customerSettings.UsernamesEnabled)
            {
                var cust2 = _customerService.GetCustomerByEmail(model.Username);
                if (cust2 != null)
                    ModelState.AddModelError("", "Username is already registered");
            }

            //validate customer roles
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            var newCustomerRoles = new List<CustomerRole>();

			if (model.SelectedCustomerRoleIds != null)
			{
				foreach (var customerRole in allCustomerRoles)
				{
					if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
						newCustomerRoles.Add(customerRole);
				}
			}

            var customerRolesError = ValidateCustomerRoles(newCustomerRoles);

            if (customerRolesError.HasValue())
                ModelState.AddModelError("", customerRolesError);

            bool allowManagingCustomerRoles = _permissionService.Authorize(StandardPermissionProvider.ManageCustomerRoles);
            
            if (ModelState.IsValid)
            {
                var customer = new Customer()
                {
                    CustomerGuid = Guid.NewGuid(),
                    Email = model.Email,
                    Username = model.Username,
                    AdminComment = model.AdminComment,
                    IsTaxExempt = model.IsTaxExempt,
                    Active = model.Active,
                    CreatedOnUtc = DateTime.UtcNow,
                    LastActivityDateUtc = DateTime.UtcNow,
                };
                _customerService.InsertCustomer(customer);
                
                //form fields
				if (_dateTimeSettings.AllowCustomersToSetTimeZone)
					_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId, model.TimeZoneId);
                if (_customerSettings.GenderEnabled)
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
                if (_customerSettings.DateOfBirthEnabled)
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, model.DateOfBirth);
                if (_customerSettings.CompanyEnabled)
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Company, model.Company);
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
                if (_customerSettings.CustomerNumberMethod == CustomerNumberMethod.AutomaticallySet && String.IsNullOrEmpty(model.CustomerNumber))
                {
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CustomerNumber, customer.Id);
                    _eventPublisher.Publish(new CustomerRegisteredEvent { Customer = customer });
                }
                else
                {
                    var customerNumbers = _genericAttributeService.GetAttributes(SystemCustomerAttributeNames.CustomerNumber, "customer");

                    if (customerNumbers.Where(x => x.Value == model.CustomerNumber).Any())
                    {
                        this.NotifyError("Common.CustomerNumberAlreadyExists");
                    }
                    else
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CustomerNumber, model.CustomerNumber);
                    }
                }
                    

                //password
                if (!String.IsNullOrWhiteSpace(model.Password))
                {
                    var changePassRequest = new ChangePasswordRequest(model.Email, false, _customerSettings.DefaultPasswordFormat, model.Password);
                    var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);
                    if (!changePassResult.Success)
                    {
                        foreach (var changePassError in changePassResult.Errors)
							NotifyError(changePassError);
                    }
                }

                //customer roles
                if (allowManagingCustomerRoles)
                {
                    foreach (var customerRole in newCustomerRoles)
                        customer.CustomerRoles.Add(customerRole);
                    _customerService.UpdateCustomer(customer);
                }

				_eventPublisher.Publish(new ModelBoundEvent(model, customer, form));

                //activity log
                _customerActivityService.InsertActivity("AddNewCustomer", _localizationService.GetResource("ActivityLog.AddNewCustomer"), customer.Id);

                NotifySuccess(_localizationService.GetResource("Admin.Customers.Customers.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = customer.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
			PrepareCustomerModelForCreate(model);

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(id);
            if (customer == null || customer.Deleted)
                return RedirectToAction("List");

            var model = new CustomerModel();
            model.Id = customer.Id;
            model.Email = customer.Email;
            model.Username = customer.Username;
            model.AdminComment = customer.AdminComment;
            model.IsTaxExempt = customer.IsTaxExempt;
            model.Active = customer.Active;
            model.TimeZoneId = customer.GetAttribute<string>(SystemCustomerAttributeNames.TimeZoneId);
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.AllowUsersToChangeUsernames = _customerSettings.AllowUsersToChangeUsernames;
            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
			
			foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
			{
				model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (tzi.Id == model.TimeZoneId) });
			}

            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
			model.VatNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);
			
			model.VatNumberStatusNote = ((VatNumberStatus)customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId))
				.GetLocalizedEnum(_localizationService, _workContext);

			model.CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc);
            model.LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc);
            model.LastIpAddress = customer.LastIpAddress;
            model.LastVisitedPage = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastVisitedPage);
			model.AffiliateId = customer.AffiliateId;

			if (customer.AffiliateId != 0)
			{
				var affiliate = _affiliateService.GetAffiliateById(customer.AffiliateId);
				if (affiliate != null && affiliate.Address != null)
					model.AffiliateFullName = affiliate.Address.GetFullName();
			}
            
            //form fields
            model.FirstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
            model.LastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
            model.Gender = customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender);
            model.DateOfBirth = customer.GetAttribute<DateTime?>(SystemCustomerAttributeNames.DateOfBirth);
            model.Company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company);
            model.CustomerNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.CustomerNumber);
            model.StreetAddress = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress);
            model.StreetAddress2 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2);
            model.ZipPostalCode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode);
            model.City = customer.GetAttribute<string>(SystemCustomerAttributeNames.City);
            model.CountryId = customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId);
            model.StateProvinceId = customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId);
            model.Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
            model.Fax = customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax);

            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.CustomerNumberEnabled = _customerSettings.CustomerNumberMethod != CustomerNumberMethod.Disabled;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.CountryEnabled = _customerSettings.CountryEnabled;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.FaxEnabled = _customerSettings.FaxEnabled;

            //countries and states
            if (_customerSettings.CountryEnabled)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = T("Admin.Address.SelectCountry"), Value = "0" });
                foreach (var c in _countryService.GetAllCountries())
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (_customerSettings.StateProvinceEnabled)
                {
                    //states
                    var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId).ToList();
					if (states.Count > 0)
					{
						foreach (var s in states)
						{
							model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
						}
					}
					else
					{
						model.AvailableStates.Add(new SelectListItem { Text = T("Admin.Address.OtherNonUS"), Value = "0" });
					}
                }
            }

            //customer roles
            model.AvailableCustomerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Select(cr => cr.ToModel())
                .ToList();

            model.SelectedCustomerRoleIds = customer.CustomerRoles.Select(cr => cr.Id).ToArray();
            model.AllowManagingCustomerRoles = _permissionService.Authorize(StandardPermissionProvider.ManageCustomerRoles);

            //reward points gistory
            model.DisplayRewardPointsHistory = _rewardPointsSettings.Enabled;
            model.AddRewardPointsValue = 0;

            //external authentication records
            model.AssociatedExternalAuthRecords = GetAssociatedExternalAuthRecords(customer);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
		[ValidateInput(false)]
        public ActionResult Edit(CustomerModel model, bool continueEditing, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null || customer.Deleted)
                return RedirectToAction("List");

            //validate customer roles
			var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
			var allowManagingCustomerRoles = _permissionService.Authorize(StandardPermissionProvider.ManageCustomerRoles);

			if (allowManagingCustomerRoles)
			{
				var newCustomerRoles = new List<CustomerRole>();

				foreach (var customerRole in allCustomerRoles)
				{
					if (model.SelectedCustomerRoleIds != null && model.SelectedCustomerRoleIds.Contains(customerRole.Id))
						newCustomerRoles.Add(customerRole);
				}

				var customerRolesError = ValidateCustomerRoles(newCustomerRoles);
				if (customerRolesError.HasValue())
					ModelState.AddModelError("", customerRolesError);
			}
            
            if (ModelState.IsValid)
            {
                try
                {
                    customer.AdminComment = model.AdminComment;
                    customer.IsTaxExempt = model.IsTaxExempt;
                    customer.Active = model.Active;

                    //email
                    if (!String.IsNullOrWhiteSpace(model.Email))
                    {
                        _customerRegistrationService.SetEmail(customer, model.Email);
                    }
                    else
                    {
                        customer.Email = model.Email;
                    }

                    //username
                    if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames)
                    {
                        if (!String.IsNullOrWhiteSpace(model.Username))
                        {
                            _customerRegistrationService.SetUsername(customer, model.Username);
                        }
                        else
                        {
                            customer.Username = model.Username;
                        }
                    }

                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
						string prevVatNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);

						_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumber, model.VatNumber);
						//set VAT number status
						if (!String.IsNullOrEmpty(model.VatNumber))
						{
							if (!model.VatNumber.Equals(prevVatNumber, StringComparison.InvariantCultureIgnoreCase))
							{
								_genericAttributeService.SaveAttribute(customer,
									SystemCustomerAttributeNames.VatNumberStatusId,
									(int)_taxService.GetVatNumberStatus(model.VatNumber));
							}
						}
						else
						{
							_genericAttributeService.SaveAttribute(customer,
								SystemCustomerAttributeNames.VatNumberStatusId,
								(int)VatNumberStatus.Empty);
						}
                    }

					_customerService.UpdateCustomer(customer);

                    //form fields
					if (_dateTimeSettings.AllowCustomersToSetTimeZone)
						_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId, model.TimeZoneId);
                    if (_customerSettings.GenderEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);

                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);

                    if (_customerSettings.DateOfBirthEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, model.DateOfBirth);
                    if (_customerSettings.CompanyEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Company, model.Company);
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

                    //customer number
                    if (_customerSettings.CustomerNumberMethod != CustomerNumberMethod.Disabled)
                    {
                        var customerNumbers = _genericAttributeService.GetAttributes(SystemCustomerAttributeNames.CustomerNumber, "customer");
                        var currentCustomerNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.CustomerNumber);

                        if (model.CustomerNumber != currentCustomerNumber && customerNumbers.Where(x => x.Value == model.CustomerNumber).Any())
                        {
                            this.NotifyError("Common.CustomerNumberAlreadyExists");
                        }
                        else
                        {
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CustomerNumber, model.CustomerNumber);
                        }
                    }

                    //customer roles
                    if (allowManagingCustomerRoles)
                    {
                        foreach (var customerRole in allCustomerRoles)
                        {
                            if (model.SelectedCustomerRoleIds != null && model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                            {
                                //new role
                                if (customer.CustomerRoles.Where(cr => cr.Id == customerRole.Id).Count() == 0)
                                    customer.CustomerRoles.Add(customerRole);
                            }
                            else
                            {
                                //removed role
                                if (customer.CustomerRoles.Where(cr => cr.Id == customerRole.Id).Count() > 0)
                                    customer.CustomerRoles.Remove(customerRole);
                            }
                        }
                        _customerService.UpdateCustomer(customer);
                    }

					_eventPublisher.Publish(new ModelBoundEvent(model, customer, form));

                    //activity log
                    _customerActivityService.InsertActivity("EditCustomer", _localizationService.GetResource("ActivityLog.EditCustomer"), customer.Id);

                    NotifySuccess(_localizationService.GetResource("Admin.Customers.Customers.Updated"));

                    return continueEditing ? RedirectToAction("Edit", customer.Id) : RedirectToAction("List");
                }
                catch (Exception exc)
                {
					NotifyError(exc.Message, false);
                }
            }


            //If we got this far, something failed, redisplay form
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.AllowUsersToChangeUsernames = _customerSettings.AllowUsersToChangeUsernames;
            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;

			foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
			{
				model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (tzi.Id == model.TimeZoneId) });
			}

            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
			model.VatNumberStatusNote = ((VatNumberStatus)customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId))
				 .GetLocalizedEnum(_localizationService, _workContext);

            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc);
            model.LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc);
            model.LastIpAddress = model.LastIpAddress;
            model.LastVisitedPage = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastVisitedPage);

            //form fields
            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.CustomerNumberEnabled = _customerSettings.CustomerNumberMethod != CustomerNumberMethod.Disabled;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.CountryEnabled = _customerSettings.CountryEnabled;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.FaxEnabled = _customerSettings.FaxEnabled;

            //countries and states
            if (_customerSettings.CountryEnabled)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = T("Admin.Address.SelectCountry"), Value = "0" });
                foreach (var c in _countryService.GetAllCountries())
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (_customerSettings.StateProvinceEnabled)
                {
                    //states
                    var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId).ToList();
					if (states.Count > 0)
					{
						foreach (var s in states)
						{
							model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
						}
					}
					else
					{
						model.AvailableStates.Add(new SelectListItem { Text = T("Admin.Address.OtherNonUS"), Value = "0" });
					}
                }
            }

            //customer roles
            model.AvailableCustomerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Select(cr => cr.ToModel())
                .ToList();

            model.AllowManagingCustomerRoles = allowManagingCustomerRoles;
            
			//reward points gistory
            model.DisplayRewardPointsHistory = _rewardPointsSettings.Enabled;
            model.AddRewardPointsValue = 0;
            
			//external authentication records
            model.AssociatedExternalAuthRecords = GetAssociatedExternalAuthRecords(customer);

            return View(model);
        }
        
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("changepassword")]
        public ActionResult ChangePassword(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var changePassRequest = new ChangePasswordRequest(model.Email, false, _customerSettings.DefaultPasswordFormat, model.Password);
                var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);

				if (changePassResult.Success)
				{
					NotifySuccess(_localizationService.GetResource("Admin.Customers.Customers.PasswordChanged"));
				}
				else
				{
					foreach (var error in changePassResult.Errors)
						NotifyError(error);
				}
            }

            return RedirectToAction("Edit", customer.Id);
        }
        
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markVatNumberAsValid")]
        public ActionResult MarkVatNumberAsValid(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

			_genericAttributeService.SaveAttribute(customer,
				SystemCustomerAttributeNames.VatNumberStatusId,
				(int)VatNumberStatus.Valid);

            return RedirectToAction("Edit", customer.Id);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markVatNumberAsInvalid")]
        public ActionResult MarkVatNumberAsInvalid(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

			_genericAttributeService.SaveAttribute(customer,
				SystemCustomerAttributeNames.VatNumberStatusId,
				(int)VatNumberStatus.Invalid);
            
            return RedirectToAction("Edit", customer.Id);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                return RedirectToAction("List");

            try
            {
                _customerService.DeleteCustomer(customer);

				if (customer.Email.HasValue())
				{
					foreach (var store in _storeService.GetAllStores())
					{
						var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(customer.Email, store.Id);
						if (subscription != null)
						{
							_newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);
						}
					}
				}

                _customerActivityService.InsertActivity("DeleteCustomer", T("ActivityLog.DeleteCustomer", customer.Id));

                NotifySuccess(T("Admin.Customers.Customers.Deleted"));

                return RedirectToAction("List");
            }
            catch (Exception exception)
            {
				NotifyError(exception.Message);
                return RedirectToAction("Edit", new { id = customer.Id });
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("impersonate")]
        public ActionResult Impersonate(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AllowCustomerImpersonation))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                return RedirectToAction("List");

			// ensure that a non-admin user cannot impersonate as an administrator
			// otherwise, that user can simply impersonate as an administrator and gain additional administrative privileges
			if (!_workContext.CurrentCustomer.IsAdmin() && customer.IsAdmin())
			{
				NotifyError("A non-admin user cannot impersonate as an administrator");
				return RedirectToAction("Edit", customer.Id);
			}

            _genericAttributeService.SaveAttribute<int?>(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.ImpersonatedCustomerId, customer.Id);

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        public ActionResult SendEmail(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            try
            {
                if (String.IsNullOrWhiteSpace(customer.Email))
                    throw new SmartException("Customer email is empty");
				if (!customer.Email.IsEmail())
                    throw new SmartException("Customer email is not valid");
                if (String.IsNullOrWhiteSpace(model.SendEmail.Subject))
                    throw new SmartException("Email subject is empty");
                if (String.IsNullOrWhiteSpace(model.SendEmail.Body))
                    throw new SmartException("Email body is empty");

				var emailAccount = _emailAccountService.GetDefaultEmailAccount();
                if (emailAccount == null)
                    throw new SmartException(T("Common.Error.NoEmailAccount"));

                var email = new QueuedEmail
                {
                    EmailAccountId = emailAccount.Id,
                    FromName = emailAccount.DisplayName,
                    From = emailAccount.Email,
                    ToName = customer.GetFullName(),
                    To = customer.Email,
                    Subject = model.SendEmail.Subject,
                    Body = model.SendEmail.Body,
                    CreatedOnUtc = DateTime.UtcNow,
                };

				_queuedEmailService.InsertQueuedEmail(email);

                NotifySuccess(T("Admin.Customers.Customers.SendEmail.Queued"));
            }
            catch (Exception exc)
            {
				NotifyError(exc.Message);
            }

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        public ActionResult SendPm(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            try
            {
                if (!_forumSettings.AllowPrivateMessages)
                    throw new SmartException(T("PrivateMessages.Disabled"));
                if (customer.IsGuest())
                    throw new SmartException(T("Common.MethodNotSupportedForGuests"));
                if (String.IsNullOrWhiteSpace(model.SendPm.Subject))
                    throw new SmartException(T("Admin.Customers.Customers.SendPM.Subject.Hint"));
                if (String.IsNullOrWhiteSpace(model.SendPm.Message))
                    throw new SmartException(T("Admin.Customers.Customers.SendPM.Message.Hint"));


                var privateMessage = new PrivateMessage
                {
					StoreId = _storeContext.CurrentStore.Id,
                    ToCustomerId = customer.Id,
                    FromCustomerId = _workContext.CurrentCustomer.Id,
                    Subject = model.SendPm.Subject,
                    Text = model.SendPm.Message,
                    IsDeletedByAuthor = false,
                    IsDeletedByRecipient = false,
                    IsRead = false,
                    CreatedOnUtc = DateTime.UtcNow
                };

                _forumService.InsertPrivateMessage(privateMessage);
                NotifySuccess(_localizationService.GetResource("Admin.Customers.Customers.SendPM.Sent"));
            }
            catch (Exception exc)
            {
				NotifyError(exc.Message);
            }

            return RedirectToAction("Edit", new { id = customer.Id });
        }
        
        #endregion
        
        #region Reward points history

        [GridAction]
        public ActionResult RewardPointsHistorySelect(int customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id");

            var model = new List<CustomerModel.RewardPointsHistoryModel>();
            foreach (var rph in customer.RewardPointsHistory.OrderByDescending(rph => rph.CreatedOnUtc).ThenByDescending(rph => rph.Id))
            {
                model.Add(new CustomerModel.RewardPointsHistoryModel()
                    {
                        Points = rph.Points,
                        PointsBalance = rph.PointsBalance,
                        Message = rph.Message,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(rph.CreatedOnUtc, DateTimeKind.Utc)
                    });
            } 
            var gridModel = new GridModel<CustomerModel.RewardPointsHistoryModel>
            {
                Data = model,
                Total = model.Count
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [ValidateInput(false)]
        public ActionResult RewardPointsHistoryAdd(int customerId, int addRewardPointsValue, string addRewardPointsMessage)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            customer.AddRewardPointsHistoryEntry(addRewardPointsValue, addRewardPointsMessage);
            _customerService.UpdateCustomer(customer);

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }
        
        #endregion
        
        #region Addresses

        [GridAction]
        public ActionResult AddressesSelect(int customerId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id", "customerId");

            var addresses = customer.Addresses.OrderByDescending(a => a.CreatedOnUtc).ThenByDescending(a => a.Id).ToList();
            var gridModel = new GridModel<AddressModel>
            {
                Data = addresses.Select(x =>
                {
                    var model = x.ToModel();
                    var addressHtmlSb = new StringBuilder("<div>");
                    if (_addressSettings.CompanyEnabled && !String.IsNullOrEmpty(model.Company))
                        addressHtmlSb.AppendFormat("{0}<br />", Server.HtmlEncode(model.Company));
                    if (_addressSettings.StreetAddressEnabled && !String.IsNullOrEmpty(model.Address1))
                        addressHtmlSb.AppendFormat("{0}<br />", Server.HtmlEncode(model.Address1));
                    if (_addressSettings.StreetAddress2Enabled && !String.IsNullOrEmpty(model.Address2))
                        addressHtmlSb.AppendFormat("{0}<br />", Server.HtmlEncode(model.Address2));
                    if (_addressSettings.CityEnabled && !String.IsNullOrEmpty(model.City))
                        addressHtmlSb.AppendFormat("{0},", Server.HtmlEncode(model.City));
                    if (_addressSettings.StateProvinceEnabled && !String.IsNullOrEmpty(model.StateProvinceName))
                        addressHtmlSb.AppendFormat("{0},", Server.HtmlEncode(model.StateProvinceName));
                    if (_addressSettings.ZipPostalCodeEnabled && !String.IsNullOrEmpty(model.ZipPostalCode))
                        addressHtmlSb.AppendFormat("{0}<br />", Server.HtmlEncode(model.ZipPostalCode));
                    if (_addressSettings.CountryEnabled && !String.IsNullOrEmpty(model.CountryName))
                        addressHtmlSb.AppendFormat("{0}", Server.HtmlEncode(model.CountryName));
                    addressHtmlSb.Append("</div>");
                    model.AddressHtml = addressHtmlSb.ToString();
                    return model;
                }),
                Total = addresses.Count
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction]
        public ActionResult AddressDelete(int customerId, int addressId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id", "customerId");

            var address = customer.Addresses.Where(a => a.Id == addressId).FirstOrDefault();
            customer.RemoveAddress(address);
            _customerService.UpdateCustomer(customer);
            //now delete the address record
            _addressService.DeleteAddress(address);

            return AddressesSelect(customerId, command);
        }
        
        public ActionResult AddressCreate(int customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                return RedirectToAction("List");

            var model = new CustomerAddressModel();
            model.Address = new AddressModel();
            model.CustomerId = customerId;
            model.Address.FirstNameEnabled = true;
            model.Address.FirstNameRequired = true;
            model.Address.LastNameEnabled = true;
            model.Address.LastNameRequired = true;
            model.Address.EmailEnabled = true;
            model.Address.EmailRequired = true;
            model.Address.ValidateEmailAddress = _addressSettings.ValidateEmailAddress;
            model.Address.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.Address.CompanyRequired = _addressSettings.CompanyRequired;
            model.Address.CountryEnabled = _addressSettings.CountryEnabled;
            model.Address.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.Address.CityEnabled = _addressSettings.CityEnabled;
            model.Address.CityRequired = _addressSettings.CityRequired;
            model.Address.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.Address.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.Address.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.Address.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.Address.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.Address.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.Address.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.Address.PhoneRequired = _addressSettings.PhoneRequired;
            model.Address.FaxEnabled = _addressSettings.FaxEnabled;
            model.Address.FaxRequired = _addressSettings.FaxRequired;
            //countries
            foreach (var c in _countryService.GetAllCountries(true))
                model.Address.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString() });
            model.Address.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public ActionResult AddressCreate(CustomerAddressModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.CustomerId);
            if (customer == null)
                return RedirectToAction("List");

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

                NotifySuccess(_localizationService.GetResource("Admin.Customers.Customers.Addresses.Added"));
                return RedirectToAction("AddressEdit", new { addressId = address.Id, customerId = model.CustomerId });
            }

            //If we got this far, something failed, redisplay form
            model.CustomerId = customer.Id;
            //countries
            foreach (var c in _countryService.GetAllCountries(true))
            {
                model.Address.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.Address.CountryId) });
            }
            //states
            var states = model.Address.CountryId.HasValue ? _stateProvinceService.GetStateProvincesByCountryId(model.Address.CountryId.Value, true).ToList() : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                {
                    model.Address.AvailableStates.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.Address.StateProvinceId) });
                }
            }
            else
            {
                model.Address.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });
            }
            return View(model);
        }

        public ActionResult AddressEdit(int addressId, int customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                return RedirectToAction("List");

            var address = _addressService.GetAddressById(addressId);
            if (address == null)
                return RedirectToAction("Edit", new { id = customer.Id });

            var model = new CustomerAddressModel();
            model.CustomerId = customerId;
            model.Address = address.ToModel();
            model.Address.FirstNameEnabled = true;
            model.Address.FirstNameRequired = true;
            model.Address.LastNameEnabled = true;
            model.Address.LastNameRequired = true;
            model.Address.EmailEnabled = true;
            model.Address.EmailRequired = true;
            model.Address.ValidateEmailAddress = _addressSettings.ValidateEmailAddress;
            model.Address.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.Address.CompanyRequired = _addressSettings.CompanyRequired;
            model.Address.CountryEnabled = _addressSettings.CountryEnabled;
            model.Address.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.Address.CityEnabled = _addressSettings.CityEnabled;
            model.Address.CityRequired = _addressSettings.CityRequired;
            model.Address.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.Address.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.Address.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.Address.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.Address.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.Address.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.Address.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.Address.PhoneRequired = _addressSettings.PhoneRequired;
            model.Address.FaxEnabled = _addressSettings.FaxEnabled;
            model.Address.FaxRequired = _addressSettings.FaxRequired;
            //countries
            foreach (var c in _countryService.GetAllCountries(true))
                model.Address.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == address.CountryId) });
            //states
            var states = address.Country != null ? _stateProvinceService.GetStateProvincesByCountryId(address.Country.Id, true).ToList() : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public ActionResult AddressEdit(CustomerAddressModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.CustomerId);
            if (customer == null)
                return RedirectToAction("List");

            var address = _addressService.GetAddressById(model.Address.Id);
            if (address == null)
                return RedirectToAction("Edit", new { id = customer.Id });

            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                _addressService.UpdateAddress(address);

                NotifySuccess(_localizationService.GetResource("Admin.Customers.Customers.Addresses.Updated"));
                return RedirectToAction("AddressEdit", new { addressId = model.Address.Id, customerId = model.CustomerId });
            }

            //If we got this far, something failed, redisplay form
            model.CustomerId = customer.Id;
            model.Address = address.ToModel();
            model.Address.FirstNameEnabled = true;
            model.Address.FirstNameRequired = true;
            model.Address.LastNameEnabled = true;
            model.Address.LastNameRequired = true;
            model.Address.EmailEnabled = true;
            model.Address.EmailRequired = true;
            model.Address.ValidateEmailAddress = _addressSettings.ValidateEmailAddress;
            model.Address.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.Address.CompanyRequired = _addressSettings.CompanyRequired;
            model.Address.CountryEnabled = _addressSettings.CountryEnabled;
            model.Address.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.Address.CityEnabled = _addressSettings.CityEnabled;
            model.Address.CityRequired = _addressSettings.CityRequired;
            model.Address.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.Address.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.Address.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.Address.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.Address.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.Address.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.Address.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.Address.PhoneRequired = _addressSettings.PhoneRequired;
            model.Address.FaxEnabled = _addressSettings.FaxEnabled;
            model.Address.FaxRequired = _addressSettings.FaxRequired;
            //countries
            foreach (var c in _countryService.GetAllCountries(true))
                model.Address.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == address.CountryId) });
            //states
            var states = address.Country != null ? _stateProvinceService.GetStateProvincesByCountryId(address.Country.Id, true).ToList() : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });

            return View(model);
        }

        #endregion

        #region Orders
        
        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult OrderList(int customerId, GridCommand command)
        {
			var model = new GridModel<CustomerModel.OrderModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
			{
				var orders = _orderService.SearchOrders(0, customerId, null, null, null, null, null, null, null, null, 0, int.MaxValue);

				model.Data = orders.PagedForCommand(command)
					.Select(order =>
					{
						var store = _storeService.GetStoreById(order.StoreId);
						var orderModel = new CustomerModel.OrderModel()
						{
							Id = order.Id,
							OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
							PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
							ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext),
							OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false),
							StoreName = store != null ? store.Name : "".NaIfEmpty(),
							CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc),
						};
						return orderModel;
					});

				model.Total = orders.Count;
			}
			else
			{
				model.Data = Enumerable.Empty<CustomerModel.OrderModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = model
            };
        }
        
        #endregion

        #region Reports

        public ActionResult Reports()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var model = new CustomerReportsModel();

            //customers by number of orders
            model.BestCustomersByNumberOfOrders = new BestCustomersReportModel();
            model.BestCustomersByNumberOfOrders.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.BestCustomersByNumberOfOrders.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
            model.BestCustomersByNumberOfOrders.AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(false).ToList();

            //customers by order total
            model.BestCustomersByOrderTotal = new BestCustomersReportModel();
            model.BestCustomersByOrderTotal.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.BestCustomersByOrderTotal.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
            model.BestCustomersByOrderTotal.AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(false).ToList();
            
            return View(model);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ReportBestCustomersByOrderTotalList(GridCommand command, BestCustomersReportModel model)
        {
            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;
            ShippingStatus? shippingStatus = model.ShippingStatusId > 0 ? (ShippingStatus?)(model.ShippingStatusId) : null;

            var items = _customerReportService.GetBestCustomersReport(startDateValue, endDateValue, orderStatus, paymentStatus, shippingStatus, 1);

            var gridModel = new GridModel<BestCustomerReportLineModel>
            {
                Data = items.Select(x =>
                {
                    var m = new BestCustomerReportLineModel()
                    {
                        CustomerId = x.CustomerId,
                        OrderTotal = _priceFormatter.FormatPrice(x.OrderTotal, true, false),
                        OrderCount = x.OrderCount,
                    };

                    var customer = _customerService.GetCustomerById(x.CustomerId);
                    if (customer != null)
                    {
                        m.CustomerName = customer.IsGuest() ? T("Admin.Customers.Guest").Text : customer.Email;
                    }

                    return m;
                }),
                Total = items.Count
            };

            return new JsonResult
            {
                Data = gridModel
            };
        }

		[GridAction(EnableCustomBinding = true)]
        public ActionResult ReportBestCustomersByNumberOfOrdersList(GridCommand command, BestCustomersReportModel model)
        {
            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;
            ShippingStatus? shippingStatus = model.ShippingStatusId > 0 ? (ShippingStatus?)(model.ShippingStatusId) : null;

            var items = _customerReportService.GetBestCustomersReport(startDateValue, endDateValue, orderStatus, paymentStatus, shippingStatus, 2);

            var gridModel = new GridModel<BestCustomerReportLineModel>
            {
                Data = items.Select(x =>
                {
                    var m = new BestCustomerReportLineModel
                    {
                        CustomerId = x.CustomerId,
                        OrderTotal = _priceFormatter.FormatPrice(x.OrderTotal, true, false),
                        OrderCount = x.OrderCount,
                    };

                    var customer = _customerService.GetCustomerById(x.CustomerId);
                    if (customer != null)
                    {
                        m.CustomerName = customer.IsGuest() ? T("Admin.Customers.Guest").Text : customer.Email;
                    }

                    return m;
                }),
                Total = items.Count
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [ChildActionOnly]
        public ActionResult ReportRegisteredCustomers()
        {
            var model = GetReportRegisteredCustomersModel();
            return PartialView(model);
        }

		[GridAction(EnableCustomBinding = true)]
        public ActionResult ReportRegisteredCustomersList(GridCommand command)
        {
            var model = GetReportRegisteredCustomersModel();

			var gridModel = new GridModel<RegisteredCustomerReportLineModel>
            {
                Data = model,
                Total = model.Count
            };

            return new JsonResult
            {
                Data = gridModel
            };
        }
        
        #endregion

        #region Current shopping cart/ wishlist

        [GridAction(EnableCustomBinding = true)]
        public ActionResult GetCartList(int customerId, int cartTypeId)
        {
            var customer = _customerService.GetCustomerById(customerId);
            var cart = customer.GetCartItems((ShoppingCartType)cartTypeId);

            var gridModel = new GridModel<ShoppingCartItemModel>
            {
                Data = cart.Select(sci =>
                {
                    decimal taxRate;
					var store = _storeService.GetStoreById(sci.Item.StoreId); 

                    var sciModel = new ShoppingCartItemModel
                    {
                        Id = sci.Item.Id,
						Store = store != null ? store.Name : "".NaIfEmpty(),
						ProductId = sci.Item.ProductId,
                        Quantity = sci.Item.Quantity,
						ProductName = sci.Item.Product.Name,
						ProductTypeName = sci.Item.Product.GetProductTypeLabel(_localizationService),
						ProductTypeLabelHint = sci.Item.Product.ProductTypeLabelHint,
                        UnitPrice = _priceFormatter.FormatPrice(_taxService.GetProductPrice(sci.Item.Product, _priceCalculationService.GetUnitPrice(sci, true), out taxRate)),
                        Total = _priceFormatter.FormatPrice(_taxService.GetProductPrice(sci.Item.Product, _priceCalculationService.GetSubTotal(sci, true), out taxRate)),
                        UpdatedOn = _dateTimeHelper.ConvertToUserTime(sci.Item.UpdatedOnUtc, DateTimeKind.Utc)
                    };
                    return sciModel;
                }),
                Total = cart.Count
            };

            return new JsonResult
            {
                Data = gridModel
            };
        }

        #endregion

        #region Activity log

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public JsonResult ListActivityLog(GridCommand command, int customerId)
        {
            var activityLog = _customerActivityService.GetAllActivities(null, null, customerId, 0, command.Page - 1, command.PageSize);

            var gridModel = new GridModel<CustomerModel.ActivityLogModel>
            {
                Data = activityLog.Select(x =>
                {
                    var m = new CustomerModel.ActivityLogModel
                    {
                        Id = x.Id,
                        ActivityLogTypeName = x.ActivityLogType.Name,
                        Comment = x.Comment,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
                    };
                    return m;
                }),
                Total = activityLog.TotalCount
            };

            return new JsonResult { Data = gridModel };
        }

		#endregion
    }
}
