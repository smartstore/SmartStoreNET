using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.Admin.Models.Common;
using SmartStore.Admin.Models.Customers;
using SmartStore.Admin.Models.Dashboard;
using SmartStore.Admin.Models.ShoppingCart;
using SmartStore.ComponentModel;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Security;
using SmartStore.Services;
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
using SmartStore.Services.Tax;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;
using static SmartStore.Services.Customers.CustomerReportService;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class CustomerController : AdminControllerBase
    {
        private readonly ICommonServices _services;
        private readonly ICustomerService _customerService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerReportService _customerReportService;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly TaxSettings _taxSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressService _addressService;
        private readonly CustomerSettings _customerSettings;
        private readonly ITaxService _taxService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderService _orderService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ForumSettings _forumSettings;
        private readonly IForumService _forumService;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly AddressSettings _addressSettings;
        private readonly PluginMediator _pluginMediator;
        private readonly IAffiliateService _affiliateService;
        private readonly Lazy<IGdprTool> _gdprTool;
        private readonly IDateTimeHelper _dateTimeHelper;

        public CustomerController(
            ICommonServices services,
            ICustomerService customerService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IGenericAttributeService genericAttributeService,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerReportService customerReportService,
            DateTimeSettings dateTimeSettings,
            TaxSettings taxSettings,
            RewardPointsSettings rewardPointsSettings,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IAddressService addressService,
            CustomerSettings customerSettings,
            ITaxService taxService,
            IPriceFormatter priceFormatter,
            IOrderService orderService,
            IPriceCalculationService priceCalculationService,
            AdminAreaSettings adminAreaSettings,
            IEmailAccountService emailAccountService,
            ForumSettings forumSettings,
            IForumService forumService,
            IOpenAuthenticationService openAuthenticationService,
            AddressSettings addressSettings,
            PluginMediator pluginMediator,
            IAffiliateService affiliateService,
            Lazy<IGdprTool> gdprTool,
            IDateTimeHelper dateTimeHelper)
        {
            _services = services;
            _customerService = customerService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _genericAttributeService = genericAttributeService;
            _customerRegistrationService = customerRegistrationService;
            _customerReportService = customerReportService;
            _dateTimeSettings = dateTimeSettings;
            _taxSettings = taxSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _addressService = addressService;
            _customerSettings = customerSettings;
            _taxService = taxService;
            _priceFormatter = priceFormatter;
            _orderService = orderService;
            _priceCalculationService = priceCalculationService;
            _adminAreaSettings = adminAreaSettings;
            _emailAccountService = emailAccountService;
            _forumSettings = forumSettings;
            _forumService = forumService;
            _openAuthenticationService = openAuthenticationService;
            _addressSettings = addressSettings;
            _pluginMediator = pluginMediator;
            _affiliateService = affiliateService;
            _gdprTool = gdprTool;
            _dateTimeHelper = dateTimeHelper;
        }

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
                    AuthMethodName = _pluginMediator.GetLocalizedFriendlyName(method.Metadata, Services.WorkContext.WorkingLanguage.Id)
                });
            }

            return result;
        }

        [NonAction]
        protected CustomerModel PrepareCustomerModelForList(Customer customer)
        {
            return new CustomerModel
            {
                Id = customer.Id,
                Email = customer.Email.HasValue() ? customer.Email : (customer.IsGuest() ? T("Admin.Customers.Guest").Text : "".NaIfEmpty()),
                Username = customer.Username,
                FullName = customer.GetFullName(),
                Company = customer.Company,
                CustomerNumber = customer.CustomerNumber,
                ZipPostalCode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode),
                Active = customer.Active,
                Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                CustomerRoleNames = GetCustomerRolesNames(customer.CustomerRoleMappings.Select(x => x.CustomerRole).ToList()),
                CreatedOn = Services.DateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc),
                LastActivityDate = Services.DateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc),
            };
        }

        protected virtual void PrepareCustomerModelForCreate(CustomerModel model)
        {
            string timeZoneId = model.TimeZoneId.HasValue() ? model.TimeZoneId : Services.DateTimeHelper.DefaultStoreTimeZone.Id;

            model.GridPageSize = _adminAreaSettings.GridPageSize;
            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            model.DisplayVatNumber = false;

            foreach (var tzi in Services.DateTimeHelper.GetSystemTimeZones())
            {
                model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = tzi.Id == timeZoneId });
            }

            if (model.SelectedCustomerRoleIds == null || model.SelectedCustomerRoleIds.Count() == 0)
            {
                var role = _customerService.GetCustomerRoleBySystemName("Registered");
                model.SelectedCustomerRoleIds = new int[] { role.Id };
            }

            model.AllowManagingCustomerRoles = Services.Permissions.Authorize(Permissions.Customer.EditRole);

            // Form fields
            MiniMapper.Map(_customerSettings, model);

            model.CustomerNumberEnabled = _customerSettings.CustomerNumberMethod != CustomerNumberMethod.Disabled;
            model.UsernamesEnabled = _customerSettings.CustomerLoginType != CustomerLoginType.Email;

            if (_customerSettings.CountryEnabled)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = T("Admin.Address.SelectCountry"), Value = "0" });

                foreach (var c in _countryService.GetAllCountries())
                {
                    model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = c.Id == model.CountryId });
                }

                if (_customerSettings.StateProvinceEnabled)
                {
                    // states
                    var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId).ToList();
                    if (states.Count > 0)
                    {
                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = s.Id == model.StateProvinceId });
                        }
                    }
                    else
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = T("Admin.Address.OtherNonUS"), Value = "0" });
                    }
                }
            }
        }

        protected virtual void PrepareCustomerModelForEdit(CustomerModel model, Customer customer)
        {
            model.GridPageSize = _adminAreaSettings.GridPageSize;
            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            model.Deleted = customer.Deleted;
            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            model.VatNumberStatusNote = ((VatNumberStatus)customer.VatNumberStatusId).GetLocalizedEnum(Services.Localization, Services.WorkContext);
            model.CreatedOn = Services.DateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc);
            model.LastActivityDate = Services.DateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc);
            model.LastIpAddress = customer.LastIpAddress;
            model.LastVisitedPage = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastVisitedPage);

            foreach (var tzi in Services.DateTimeHelper.GetSystemTimeZones())
            {
                model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = tzi.Id == model.TimeZoneId });
            }

            // Form fields.
            MiniMapper.Map(_customerSettings, model);

            model.CustomerNumberEnabled = _customerSettings.CustomerNumberMethod != CustomerNumberMethod.Disabled;
            model.UsernamesEnabled = _customerSettings.CustomerLoginType != CustomerLoginType.Email;

            // Countries and states.
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
                    // States
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

            model.AllowManagingCustomerRoles = Services.Permissions.Authorize(Permissions.Customer.EditRole);
            model.DisplayRewardPointsHistory = _rewardPointsSettings.Enabled;
            model.AddRewardPointsValue = 0;
            model.AssociatedExternalAuthRecords = GetAssociatedExternalAuthRecords(customer);
            model.PermissionTree = Services.Permissions.GetPermissionTree(customer, true);

            // Addresses.
            var addresses = customer.Addresses
                .OrderByDescending(a => a.CreatedOnUtc)
                .ThenByDescending(a => a.Id)
                .ToList();

            model.Addresses = addresses
                .Select(x => x.ToModel())
                .ToList();
        }

        private string ValidateCustomerRoles(int[] selectedCustomerRoleIds, List<int> allCustomerRoleIds, out List<CustomerRole> newCustomerRoles)
        {
            Guard.NotNull(allCustomerRoleIds, nameof(allCustomerRoleIds));

            newCustomerRoles = new List<CustomerRole>();

            var newCustomerRoleIds = new HashSet<int>();

            if (selectedCustomerRoleIds != null)
            {
                foreach (var roleId in allCustomerRoleIds)
                {
                    if (selectedCustomerRoleIds.Contains(roleId))
                    {
                        newCustomerRoleIds.Add(roleId);
                    }
                }
            }

            if (newCustomerRoleIds.Any())
            {
                var customerRolesQuery = _customerService.GetAllCustomerRoles(true).SourceQuery;
                newCustomerRoles = customerRolesQuery.Where(x => newCustomerRoleIds.Contains(x.Id)).ToList();
            }

            var isInGuestsRole = newCustomerRoles.FirstOrDefault(cr => cr.SystemName == SystemCustomerRoleNames.Guests) != null;
            var isInRegisteredRole = newCustomerRoles.FirstOrDefault(cr => cr.SystemName == SystemCustomerRoleNames.Registered) != null;

            // Ensure a customer is not added to both 'Guests' and 'Registered' customer roles.
            if (isInGuestsRole && isInRegisteredRole)
            {
                return string.Format(T("Admin.Customers.CanOnlyBeCustomerOrGuest"),
                    _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Guests).Name,
                    _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered).Name);
            }

            // Ensure that a customer is in at least one required role ('Guests' and 'Registered').
            if (!isInGuestsRole && !isInRegisteredRole)
            {
                return string.Format(T("Admin.Customers.MustBeCustomerOrGuest"),
                    _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Guests).Name,
                    _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered).Name);
            }

            return string.Empty;
        }

        #endregion

        #region Customers

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Customer.Read)]
        public ActionResult List()
        {
            // Load registered customers by default.
            var registeredRole = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered);

            var listModel = new CustomerListModel
            {
                GridPageSize = _adminAreaSettings.GridPageSize,
                UsernamesEnabled = _customerSettings.CustomerLoginType != CustomerLoginType.Email,
                DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled,
                CompanyEnabled = _customerSettings.CompanyEnabled,
                PhoneEnabled = _customerSettings.PhoneEnabled,
                ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled,
                SearchCustomerRoleIds = new int[] { registeredRole.Id }
            };

            return View(listModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Customer.Read)]
        public ActionResult CustomerList(GridCommand command, CustomerListModel model)
        {
            // We use own own binder for searchCustomerRoleIds property.
            var gridModel = new GridModel<CustomerModel>();

            var q = new CustomerSearchQuery
            {
                CustomerRoleIds = model.SearchCustomerRoleIds,
                Email = model.SearchEmail,
                Username = model.SearchUsername,
                SearchTerm = model.SearchTerm,
                DayOfBirth = model.SearchDayOfBirth.ToInt(),
                MonthOfBirth = model.SearchMonthOfBirth.ToInt(),
                Phone = model.SearchPhone,
                ZipPostalCode = model.SearchZipPostalCode,
                Deleted = false,
                Active = model.SearchActiveOnly,
                PageIndex = command.Page - 1,
                PageSize = command.PageSize
            };

            var customers = _customerService.SearchCustomers(q);

            gridModel.Data = customers.Select(PrepareCustomerModelForList);
            gridModel.Total = customers.TotalCount;

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Customer.Create)]
        public ActionResult Create()
        {
            var model = new CustomerModel();

            PrepareCustomerModelForCreate(model);

            //default value
            model.Active = true;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Customer.Create)]
        public ActionResult Create(CustomerModel model, bool continueEditing, FormCollection form)
        {
            // Validate customer roles.
            var allCustomerRoleIds = _customerService.GetAllCustomerRoles(true).SourceQuery.Select(x => x.Id).ToList();

            var customerRolesError = ValidateCustomerRoles(model.SelectedCustomerRoleIds, allCustomerRoleIds, out var newCustomerRoles);
            if (customerRolesError.HasValue())
            {
                ModelState.AddModelError("", customerRolesError);
            }

            if (ModelState.IsValid)
            {
                var utcNow = DateTime.UtcNow;
                var customer = new Customer
                {
                    CustomerGuid = Guid.NewGuid(),
                    Email = model.Email,
                    Username = model.Username,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    AdminComment = model.AdminComment,
                    IsTaxExempt = model.IsTaxExempt,
                    Active = model.Active,
                    CreatedOnUtc = utcNow,
                    LastActivityDateUtc = utcNow,
                };

                if (_customerSettings.TitleEnabled)
                    customer.Title = model.Title;
                if (_customerSettings.DateOfBirthEnabled)
                    customer.BirthDate = model.DateOfBirth;
                if (_customerSettings.CompanyEnabled)
                    customer.Company = model.Company;

                if (_customerSettings.CustomerNumberMethod == CustomerNumberMethod.AutomaticallySet && model.CustomerNumber.IsEmpty())
                {
                    customer.CustomerNumber = null;
                    // Let any NumberFormatter plugin handle this
                    Services.EventPublisher.Publish(new CustomerRegisteredEvent { Customer = customer });
                }
                else if (_customerSettings.CustomerNumberMethod == CustomerNumberMethod.Enabled && model.CustomerNumber.HasValue())
                {
                    var numberExists = _customerService.SearchCustomers(new CustomerSearchQuery { CustomerNumber = model.CustomerNumber }).SourceQuery.Any();
                    if (numberExists)
                    {
                        NotifyError(T("Common.CustomerNumberAlreadyExists"));
                    }
                    else
                    {
                        customer.CustomerNumber = model.CustomerNumber;
                    }
                }

                try
                {
                    _customerService.InsertCustomer(customer);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }

                // Form fields.
                if (ModelState.IsValid)
                {
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                        customer.TimeZoneId = model.TimeZoneId;
                    if (_customerSettings.GenderEnabled)
                        customer.Gender = model.Gender;
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

                    // Password.
                    if (model.Password.HasValue())
                    {
                        var changePassRequest = new ChangePasswordRequest(model.Email, false, _customerSettings.DefaultPasswordFormat, model.Password);
                        var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);
                        if (!changePassResult.Success)
                        {
                            foreach (var changePassError in changePassResult.Errors)
                            {
                                NotifyError(changePassError);
                            }
                        }
                    }

                    // Customer roles.
                    newCustomerRoles.Each(x => _customerService.InsertCustomerRoleMapping(new CustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = x.Id }));

                    Services.EventPublisher.Publish(new ModelBoundEvent(model, customer, form));
                    Services.CustomerActivity.InsertActivity("AddNewCustomer", T("ActivityLog.AddNewCustomer"), customer.Id);

                    NotifySuccess(T("Admin.Customers.Customers.Added"));
                    return continueEditing ? RedirectToAction("Edit", new { id = customer.Id }) : RedirectToAction("List");
                }
            }

            // If we got this far, something failed, redisplay form.
            PrepareCustomerModelForCreate(model);

            return View(model);
        }

        [Permission(Permissions.Customer.Read)]
        public ActionResult Edit(int id)
        {
            var customer = _customerService.GetCustomerById(id);
            if (customer == null /*|| customer.Deleted*/)
                return RedirectToAction("List");

            var model = new CustomerModel
            {
                Id = customer.Id,
                Email = customer.Email,
                Username = customer.Username,
                AdminComment = customer.AdminComment,
                IsTaxExempt = customer.IsTaxExempt,
                Active = customer.Active,
                TimeZoneId = customer.TimeZoneId,
                VatNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber),
                AffiliateId = customer.AffiliateId
            };

            if (customer.AffiliateId != 0)
            {
                var affiliate = _affiliateService.GetAffiliateById(customer.AffiliateId);
                if (affiliate != null && affiliate.Address != null)
                    model.AffiliateFullName = affiliate.Address.GetFullName();
            }

            // Form fields
            model.Title = customer.Title;
            model.FirstName = customer.FirstName;
            model.LastName = customer.LastName;
            model.DateOfBirth = customer.BirthDate;
            model.Company = customer.Company;
            model.CustomerNumber = customer.CustomerNumber;
            model.Gender = customer.Gender;
            model.ZipPostalCode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode);
            model.CountryId = customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId);
            model.StreetAddress = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress);
            model.StreetAddress2 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2);
            model.City = customer.GetAttribute<string>(SystemCustomerAttributeNames.City);
            model.StateProvinceId = customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId);
            model.Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
            model.Fax = customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax);

            model.SelectedCustomerRoleIds = customer.CustomerRoleMappings
                .Where(x => !x.IsSystemMapping)
                .Select(x => x.CustomerRoleId)
                .ToArray();

            PrepareCustomerModelForEdit(model, customer);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Customer.Update)]
        public ActionResult Edit(CustomerModel model, bool continueEditing, FormCollection form)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null || customer.Deleted)
            {
                return RedirectToAction("List");
            }

            if (customer.IsAdmin() && !Services.WorkContext.CurrentCustomer.IsAdmin())
            {
                NotifyAccessDenied();
                return RedirectToAction("Edit", new { customer.Id });
            }

            // Validate customer roles.
            var allowManagingCustomerRoles = Services.Permissions.Authorize(Permissions.Customer.EditRole);

            var allCustomerRoleIds = allowManagingCustomerRoles
                ? _customerService.GetAllCustomerRoles(true).SourceQuery.Select(x => x.Id).ToList()
                : new List<int>();

            if (allowManagingCustomerRoles)
            {
                var customerRolesError = ValidateCustomerRoles(model.SelectedCustomerRoleIds, allCustomerRoleIds, out _);
                if (customerRolesError.HasValue())
                {
                    ModelState.AddModelError("", customerRolesError);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    customer.AdminComment = model.AdminComment;
                    customer.IsTaxExempt = model.IsTaxExempt;
                    customer.Active = model.Active;
                    customer.FirstName = model.FirstName;
                    customer.LastName = model.LastName;

                    if (_customerSettings.TitleEnabled)
                        customer.Title = model.Title;
                    if (_customerSettings.DateOfBirthEnabled)
                        customer.BirthDate = model.DateOfBirth;
                    if (_customerSettings.CompanyEnabled)
                        customer.Company = model.Company;

                    // Customer number.
                    if (_customerSettings.CustomerNumberMethod != CustomerNumberMethod.Disabled)
                    {
                        var numberExists = _customerService.SearchCustomers(new CustomerSearchQuery { CustomerNumber = model.CustomerNumber }).SourceQuery.Any();
                        if (model.CustomerNumber != customer.CustomerNumber && numberExists)
                        {
                            NotifyError(T("Common.CustomerNumberAlreadyExists"));
                        }
                        else
                        {
                            customer.CustomerNumber = model.CustomerNumber;
                        }
                    }

                    if (model.Email.HasValue())
                    {
                        _customerRegistrationService.SetEmail(customer, model.Email);
                    }
                    else
                    {
                        customer.Email = model.Email;
                    }

                    if (_customerSettings.CustomerLoginType != CustomerLoginType.Email && _customerSettings.AllowUsersToChangeUsernames)
                    {
                        if (model.Username.HasValue())
                        {
                            _customerRegistrationService.SetUsername(customer, model.Username);
                        }
                        else
                        {
                            customer.Username = model.Username;
                        }
                    }

                    _customerService.UpdateCustomer(customer);

                    // VAT number.
                    if (_taxSettings.EuVatEnabled)
                    {
                        string prevVatNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);

                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumber, model.VatNumber);
                        // Set VAT number status.
                        if (model.VatNumber.HasValue())
                        {
                            if (!model.VatNumber.Equals(prevVatNumber, StringComparison.InvariantCultureIgnoreCase))
                            {
                                customer.VatNumberStatusId = (int)_taxService.GetVatNumberStatus(model.VatNumber);
                            }
                        }
                        else
                        {
                            customer.VatNumberStatusId = (int)VatNumberStatus.Empty;
                        }
                    }

                    // Form fields.
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                        customer.TimeZoneId = model.TimeZoneId;
                    if (_customerSettings.GenderEnabled)
                        customer.Gender = model.Gender;
                    if (_customerSettings.CountryEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, model.CountryId);
                    if (_customerSettings.ZipPostalCodeEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode, model.ZipPostalCode);
                    if (_customerSettings.StreetAddressEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress, model.StreetAddress);
                    if (_customerSettings.StreetAddress2Enabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2, model.StreetAddress2);
                    if (_customerSettings.CityEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvinceId, model.StateProvinceId);
                    if (_customerSettings.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
                    if (_customerSettings.FaxEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);

                    _customerService.UpdateCustomer(customer);

                    // Customer roles.
                    if (allowManagingCustomerRoles)
                    {
                        using (var scope = new DbContextScope(ctx: Services.DbContext, validateOnSave: false, autoDetectChanges: false, autoCommit: false))
                        {
                            var existingMappings = customer.CustomerRoleMappings
                                .Where(x => !x.IsSystemMapping)
                                .ToMultimap(x => x.CustomerRoleId, x => x);

                            foreach (var roleId in allCustomerRoleIds)
                            {
                                if (model.SelectedCustomerRoleIds?.Contains(roleId) ?? false)
                                {
                                    if (!existingMappings.ContainsKey(roleId))
                                    {
                                        _customerService.InsertCustomerRoleMapping(new CustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = roleId });
                                    }
                                }
                                else if (existingMappings.ContainsKey(roleId))
                                {
                                    existingMappings[roleId].Each(x => _customerService.DeleteCustomerRoleMapping(x));
                                }
                            }

                            scope.Commit();
                        }
                    }

                    Services.EventPublisher.Publish(new ModelBoundEvent(model, customer, form));
                    Services.CustomerActivity.InsertActivity("EditCustomer", T("ActivityLog.EditCustomer"), customer.Id);

                    NotifySuccess(T("Admin.Customers.Customers.Updated"));
                    return continueEditing ? RedirectToAction("Edit", customer.Id) : RedirectToAction("List");
                }
                catch (Exception ex)
                {
                    NotifyError(ex.Message, false);
                }
            }

            // If we got this far, something failed, redisplay form.
            PrepareCustomerModelForEdit(model, customer);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("changepassword")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Customer.Update)]
        public ActionResult ChangePassword(CustomerModel model)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var changePassRequest = new ChangePasswordRequest(model.Email, false, _customerSettings.DefaultPasswordFormat, model.Password);
                var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);

                if (changePassResult.Success)
                {
                    NotifySuccess(T("Admin.Customers.Customers.PasswordChanged"));
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
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Customer.Update)]
        public ActionResult MarkVatNumberAsValid(CustomerModel model)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            customer.VatNumberStatusId = (int)VatNumberStatus.Valid;

            _customerService.UpdateCustomer(customer);

            return RedirectToAction("Edit", customer.Id);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markVatNumberAsInvalid")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Customer.Update)]
        public ActionResult MarkVatNumberAsInvalid(CustomerModel model)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            customer.VatNumberStatusId = (int)VatNumberStatus.Invalid;

            _customerService.UpdateCustomer(customer);

            return RedirectToAction("Edit", customer.Id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Customer.Delete)]
        public ActionResult Delete(int id)
        {
            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                return RedirectToAction("List");

            try
            {
                _customerService.DeleteCustomer(customer);

                if (customer.Email.HasValue())
                {
                    foreach (var store in Services.StoreService.GetAllStores())
                    {
                        var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(customer.Email, store.Id);
                        if (subscription != null)
                        {
                            _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);
                        }
                    }
                }

                Services.CustomerActivity.InsertActivity("DeleteCustomer", T("ActivityLog.DeleteCustomer", customer.Id));

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
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Customer.Impersonate)]
        public ActionResult Impersonate(int id)
        {
            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                return RedirectToAction("List");

            // Ensure that a non-admin user cannot impersonate as an administrator
            // Otherwise, that user can simply impersonate as an administrator and gain additional administrative privileges
            if (!Services.WorkContext.CurrentCustomer.IsAdmin() && customer.IsAdmin())
            {
                NotifyAccessDenied();
                return RedirectToAction("Edit", customer.Id);
            }

            _genericAttributeService.SaveAttribute<int?>(Services.WorkContext.CurrentCustomer,
                SystemCustomerAttributeNames.ImpersonatedCustomerId, customer.Id);

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [ValidateAntiForgeryToken]
        [Permission(Permissions.System.Message.Send)]
        public ActionResult SendEmail(CustomerModel model)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            try
            {
                if (!customer.Email.HasValue() || !customer.Email.IsEmail())
                    throw new SmartException(T("Admin.Customers.Customers.SendEmail.EmailNotValid"));

                var emailAccount = _emailAccountService.GetDefaultEmailAccount();
                if (emailAccount == null)
                    throw new SmartException(T("Common.Error.NoEmailAccount"));

                var messageContext = MessageContext.Create("System.Generic", customer.GetAttribute<int>(SystemCustomerAttributeNames.LanguageId));

                var customModel = new NamedModelPart("Generic")
                {
                    ["ReplyTo"] = emailAccount.ToEmailAddress(),
                    ["Email"] = customer.Email,
                    ["Subject"] = model.SendEmail.Subject,
                    ["Body"] = model.SendEmail.Body
                };

                _services.MessageFactory.CreateMessage(messageContext, true, customer, _services.StoreContext.CurrentStore, customModel);

                NotifySuccess(T("Admin.Customers.Customers.SendEmail.Queued"));
            }
            catch (Exception exc)
            {
                NotifyError(exc.Message);
            }

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [ValidateAntiForgeryToken]
        [Permission(Permissions.Customer.SendPm)]
        public ActionResult SendPm(CustomerModel model)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            try
            {
                if (!_forumSettings.AllowPrivateMessages)
                    throw new SmartException(T("PrivateMessages.Disabled"));
                if (customer.IsGuest())
                    throw new SmartException(T("Common.MethodNotSupportedForGuests"));

                var privateMessage = new PrivateMessage
                {
                    StoreId = Services.StoreContext.CurrentStore.Id,
                    ToCustomerId = customer.Id,
                    FromCustomerId = Services.WorkContext.CurrentCustomer.Id,
                    Subject = model.SendPm.Subject,
                    Text = model.SendPm.Message,
                    IsDeletedByAuthor = false,
                    IsDeletedByRecipient = false,
                    IsRead = false,
                    CreatedOnUtc = DateTime.UtcNow
                };

                _forumService.InsertPrivateMessage(privateMessage);

                NotifySuccess(T("Admin.Customers.Customers.SendPM.Sent"));
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
        [Permission(Permissions.Customer.Read)]
        public ActionResult RewardPointsHistorySelect(int customerId)
        {
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
                    CreatedOn = Services.DateTimeHelper.ConvertToUserTime(rph.CreatedOnUtc, DateTimeKind.Utc)
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

        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [Permission(Permissions.Customer.Update)]
        public ActionResult RewardPointsHistoryAdd(int customerId, int addRewardPointsValue, string addRewardPointsMessage)
        {
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            customer.AddRewardPointsHistoryEntry(addRewardPointsValue, addRewardPointsMessage);
            _customerService.UpdateCustomer(customer);

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Addresses

        [Permission(Permissions.Customer.EditAddress)]
        public ActionResult AddressCreate(int customerId)
        {
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
            {
                return RedirectToAction("List");
            }

            var model = new CustomerAddressModel();
            PrepareAddressModel(model, customer, null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Customer.EditAddress)]
        public ActionResult AddressCreate(CustomerAddressModel model, bool continueEditing)
        {
            var customer = _customerService.GetCustomerById(model.CustomerId);
            if (customer == null)
            {
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity();
                address.CreatedOnUtc = DateTime.UtcNow;

                if (address.CountryId == 0)
                {
                    address.CountryId = null;
                }
                if (address.StateProvinceId == 0)
                {
                    address.StateProvinceId = null;
                }

                customer.Addresses.Add(address);
                _customerService.UpdateCustomer(customer);

                NotifySuccess(T("Admin.Customers.Customers.Addresses.Added"));

                return continueEditing
                    ? RedirectToAction("AddressEdit", new { addressId = address.Id, customerId = model.CustomerId })
                    : RedirectToAction("Edit", new { id = customer.Id });
            }

            model.CustomerId = customer.Id;

            model.Address.AvailableCountries = _countryService.GetAllCountries(true)
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = x.Id == model.Address?.CountryId })
                .ToList();

            var states = _stateProvinceService.GetStateProvincesByCountryId(model.Address?.CountryId ?? 0);
            if (states.Any())
            {
                model.Address.AvailableStates = states
                    .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = x.Id == model.Address?.StateProvinceId })
                    .ToList();
            }
            else
            {
                model.Address.AvailableStates.Add(new SelectListItem { Text = T("Admin.Address.OtherNonUS"), Value = "0" });
            }

            return View(model);
        }

        [Permission(Permissions.Customer.EditAddress)]
        public ActionResult AddressEdit(int addressId, int customerId)
        {
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
            {
                return RedirectToAction("List");
            }

            var address = _addressService.GetAddressById(addressId);
            if (address == null)
            {
                return RedirectToAction("Edit", new { id = customer.Id });
            }

            var model = new CustomerAddressModel();
            PrepareAddressModel(model, customer, address);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Customer.EditAddress)]
        public ActionResult AddressEdit(CustomerAddressModel model, bool continueEditing)
        {
            var customer = _customerService.GetCustomerById(model.CustomerId);
            if (customer == null)
            {
                return RedirectToAction("List");
            }

            var address = _addressService.GetAddressById(model.Address.Id);
            if (address == null)
            {
                return RedirectToAction("Edit", new { id = customer.Id });
            }

            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                _addressService.UpdateAddress(address);

                NotifySuccess(T("Admin.Customers.Customers.Addresses.Updated"));

                return continueEditing
                    ? RedirectToAction("AddressEdit", new { addressId = model.Address.Id, customerId = model.CustomerId })
                    : RedirectToAction("Edit", new { id = customer.Id });
            }

            PrepareAddressModel(model, customer, address);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Customer.EditAddress)]
        public ActionResult AddressDelete(int customerId, int addressId)
        {
            var customer = _customerService.GetCustomerById(customerId);
            var address = customer.Addresses.FirstOrDefault(x => x.Id == addressId);

            customer.RemoveAddress(address);
            _customerService.UpdateCustomer(customer);

            _addressService.DeleteAddress(address);

            return new JsonResult { Data = new { success = true } };
        }

        private void PrepareAddressModel(CustomerAddressModel model, Customer customer, Address address)
        {
            model.CustomerId = customer.Id;
            model.Username = customer.Username;
            model.Address = address?.ToModel() ?? new AddressModel();
            model.Address.FirstNameEnabled = true;
            model.Address.FirstNameRequired = true;
            model.Address.LastNameEnabled = true;
            model.Address.LastNameRequired = true;
            model.Address.EmailEnabled = true;
            model.Address.EmailRequired = true;

            MiniMapper.Map(_addressSettings, model.Address);

            model.Address.AvailableCountries = _countryService.GetAllCountries(true)
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = x.Id == address?.CountryId })
                .ToList();

            var states = _stateProvinceService.GetStateProvincesByCountryId(address?.CountryId ?? 0);
            if (states.Any())
            {
                model.Address.AvailableStates = states
                    .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = x.Id == address?.StateProvinceId })
                    .ToList();
            }
            else
            {
                model.Address.AvailableStates.Add(new SelectListItem { Text = T("Admin.Address.OtherNonUS"), Value = "0" });
            }
        }

        #endregion

        #region Orders

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Order.Read)]
        public ActionResult OrderList(int customerId, GridCommand command)
        {
            var allStores = Services.StoreService.GetAllStores().ToDictionary(x => x.Id);
            var orders = _orderService.SearchOrders(0, customerId, null, null, null, null, null, null, null, null, command.Page - 1, command.PageSize);

            var model = new GridModel<CustomerModel.OrderModel>
            {
                Total = orders.TotalCount
            };

            model.Data = orders.Select(order =>
            {
                allStores.TryGetValue(order.StoreId, out var store);

                var orderModel = new CustomerModel.OrderModel
                {
                    Id = order.Id,
                    OrderStatus = order.OrderStatus.GetLocalizedEnum(Services.Localization, Services.WorkContext),
                    PaymentStatus = order.PaymentStatus.GetLocalizedEnum(Services.Localization, Services.WorkContext),
                    ShippingStatus = order.ShippingStatus.GetLocalizedEnum(Services.Localization, Services.WorkContext),
                    OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false),
                    StoreName = store?.Name.NullEmpty() ?? "".NaIfEmpty(),
                    CreatedOn = Services.DateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc),
                };

                return orderModel;
            });

            return new JsonResult
            {
                Data = model
            };
        }

        #endregion

        #region Reports

        private List<TopCustomerReportLineModel> CreateCustomerReportLineModel(IList<TopCustomerReportLine> items)
        {
            var customerIds = items.Distinct().Select(x => x.CustomerId).ToArray();
            var customers = _customerService.GetCustomersByIds(customerIds).ToDictionarySafe(x => x.Id);
            var guestStr = T("Admin.Customers.Guest").Text;

            var model = items.Select(x =>
            {
                customers.TryGetValue(x.CustomerId, out var customer);

                var m = new TopCustomerReportLineModel
                {
                    OrderTotal = _priceFormatter.FormatPrice(x.OrderTotal, true, false),
                    OrderCount = x.OrderCount.ToString("N0"),
                    CustomerId = x.CustomerId,
                    CustomerNumber = customer?.CustomerNumber,
                    CustomerDisplayName = customer?.FindEmail() ?? customer?.FormatUserName(_customerSettings, T, false) ?? "".NaIfEmpty(),
                    Email = customer?.Email.NullEmpty() ?? (customer.IsGuest() ? guestStr : "".NaIfEmpty()),
                    Username = customer?.Username,
                    FullName = customer?.GetFullName(),
                    Active = customer?.Active ?? false,
                    LastActivityDate = Services.DateTimeHelper.ConvertToUserTime(customer?.LastActivityDateUtc ?? DateTime.MinValue, DateTimeKind.Utc)
                };

                return m;
            })
            .ToList();

            return model;
        }

        [Permission(Permissions.Customer.Read, false)]
        public ActionResult TopCustomersDashboardReport()
        {
            var pageSize = 7;
            var reportByQuantity = _customerReportService.GetTopCustomersReport(null, null, null, null, null, ReportSorting.ByQuantityDesc, 0, pageSize);
            var reportByAmount = _customerReportService.GetTopCustomersReport(null, null, null, null, null, ReportSorting.ByAmountDesc, 0, pageSize);

            var model = new TopCustomersDashboardReportModel
            {
                TopCustomersByQuantity = CreateCustomerReportLineModel(reportByQuantity),
                TopCustomersByAmount = CreateCustomerReportLineModel(reportByAmount)
            };

            return PartialView(model);
        }

        [ChildActionOnly]
        [Permission(Permissions.Customer.Read, false)]
        public ActionResult ReportRegisteredCustomers()
        {
            var model = new List<RegisteredCustomerReportLineModel>();

            model.Add(new RegisteredCustomerReportLineModel
            {
                Period = T("Admin.Customers.Reports.RegisteredCustomers.Fields.Period.7days"),
                Customers = _customerReportService.GetRegisteredCustomersReport(7)
            });
            model.Add(new RegisteredCustomerReportLineModel
            {
                Period = T("Admin.Customers.Reports.RegisteredCustomers.Fields.Period.14days"),
                Customers = _customerReportService.GetRegisteredCustomersReport(14)
            });
            model.Add(new RegisteredCustomerReportLineModel
            {
                Period = T("Admin.Customers.Reports.RegisteredCustomers.Fields.Period.month"),
                Customers = _customerReportService.GetRegisteredCustomersReport(30)
            });
            model.Add(new RegisteredCustomerReportLineModel
            {
                Period = T("Admin.Customers.Reports.RegisteredCustomers.Fields.Period.year"),
                Customers = _customerReportService.GetRegisteredCustomersReport(365)
            });

            return PartialView(model);
        }

        /// <summary>
        /// Sorts customer registration dataPoint into customer chart report accordingly to period 
        /// </summary>
        /// <param name="reports">Registrations chart report</param>
        /// <param name="dataPoint">Current registration data</param>
        [NonAction]
        public void SetCustomerReportData(List<DashboardChartReportModel> reports, DateTime dataPoint)
        {
            var userTime = _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc);

            // Today
            if (dataPoint >= userTime.Date)
            {
                reports[0].DataSets[0].Quantity[dataPoint.Hour]++;
            }
            // Yesterday
            else if (dataPoint >= userTime.AddDays(-1).Date)
            {
                var yesterday = reports[1].DataSets[0];
                yesterday.Quantity[dataPoint.Hour]++;
            }

            // Within last 7 days
            if (dataPoint >= userTime.AddDays(-6).Date)
            {
                var week = reports[2].DataSets[0];
                var weekIndex = (userTime.Date - dataPoint.Date).Days;
                week.Quantity[week.Quantity.Length - weekIndex - 1]++;
            }

            // Within last 28 days  
            if (dataPoint >= userTime.AddDays(-27).Date)
            {
                var month = reports[3].DataSets[0];
                var monthIndex = (userTime.Date - dataPoint.Date).Days / 7;
                month.Quantity[month.Quantity.Length - monthIndex - 1]++;
            }

            // Within this year
            if (dataPoint.Year == userTime.Year)
            {
                reports[4].DataSets[0].Quantity[dataPoint.Month - 1]++;
            }
        }

        /// <summary>
        /// Evaluates and displays customer registrations of this year as line chart
        /// </summary>
        /// <returns>Customers registrations chart</returns>
        [Permission(Permissions.Customer.Read, false)]
        public ActionResult RegisteredCustomersDashboardReport()
        {
            // Get customers of at least last 28 days (if year is younger)
            var utcNow = DateTime.UtcNow;
            var beginningOfYear = new DateTime(utcNow.Year, 1, 1);
            var startDate = (utcNow.Date - beginningOfYear).Days < 28 ? utcNow.AddDays(-27).Date : beginningOfYear;
            var searchQuery = new CustomerSearchQuery
            {
                RegistrationFromUtc = startDate,
                CustomerRoleIds = new int[] { 3 },
                PageSize = int.MaxValue
            };

            var customerDates = _customerService.SearchCustomers(searchQuery).SourceQuery.Select(x => x.CreatedOnUtc).ToList();
            var model = new List<DashboardChartReportModel>()
            {
                // Today = index 0
                new DashboardChartReportModel(1, 24),
                // Yesterday = index 1
                new DashboardChartReportModel(1, 24),
                // Last 7 days = index 2
                new DashboardChartReportModel(1, 7),
                // Last 28 days = index 3
                new DashboardChartReportModel(1, 4),
                // This year = index 4
                new DashboardChartReportModel(1, 12)
            };

            // Sort data for chart display
            foreach (var dataPoint in customerDates)
            {
                SetCustomerReportData(model, _dateTimeHelper.ConvertToUserTime(dataPoint, DateTimeKind.Utc));
            }

            var userTime = _dateTimeHelper.ConvertToUserTime(utcNow, DateTimeKind.Utc).Date;
            // Format and sum values, create labels for all dataPoints
            for (int i = 0; i < model.Count; i++)
            {
                foreach (var data in model[i].DataSets)
                {
                    for (int j = 0; j < data.Amount.Length; j++)
                    {
                        data.QuantityFormatted[j] = data.Quantity[j].ToString("N0");
                    }
                    data.TotalAmount = data.Quantity.Sum();
                    data.TotalAmountFormatted = data.TotalAmount.ToString("N0");
                }
                model[i].TotalAmount = model[i].DataSets.Sum(x => x.TotalAmount);
                model[i].TotalAmountFormatted = model[i].TotalAmount.ToString("N0");

                for (int j = 0; j < model[i].Labels.Length; j++)
                {
                    // Today & yesterday
                    if (i <= 1)
                    {
                        model[i].Labels[j] = userTime.AddHours(j).ToString("t") + " - "
                            + userTime.AddHours(j).AddMinutes(59).ToString("t");
                    }
                    // Last 7 days
                    else if (i == 2)
                    {
                        model[i].Labels[j] = userTime.AddDays(-6 + j).ToString("m");
                    }
                    // Last 28 days
                    else if (i == 3)
                    {
                        var fromDay = -(7 * model[i].Labels.Length);
                        var toDayOffset = j == model[i].Labels.Length - 1 ? 0 : 1;
                        model[i].Labels[j] = userTime.AddDays(fromDay + 7 * j).ToString("m") + " - "
                            + userTime.AddDays(fromDay + 7 * (j + 1) - toDayOffset).ToString("m");
                    }
                    // This year
                    else if (i == 4)
                    {
                        model[i].Labels[j] = new DateTime(userTime.Year, j + 1, 1).ToString("Y");
                    }
                }
            }

            // Get registrations for corresponding period to calculate change in percentage; TODO: only apply to similar time of day?
            var sumBefore = new decimal[]
            {
                // Get registration count for day before
                model[1].TotalAmount,

                // Get registration count for day before yesterday
                customerDates.Where( x =>
                    x >= utcNow.Date.AddDays(-2) && x < utcNow.Date.AddDays(-1)
                ).Count(),

                // Get registration count for week before
                customerDates.Where( x =>
                    x >= utcNow.Date.AddDays(-14) && x < utcNow.Date.AddDays(-7)
                ).Count(),

                // Get registration count for month before
                _customerReportService.GetCustomerRegistrations(beginningOfYear.AddDays(-56), utcNow.Date.AddDays(-28)),

                // Get registration count for year before
                _customerReportService.GetCustomerRegistrations(beginningOfYear.AddYears(-1), utcNow.AddYears(-1))
            };

            // Format percentage value
            for (int i = 0; i < model.Count; i++)
            {
                model[i].PercentageDelta = model[i].TotalAmount != 0 && sumBefore[i] != 0
                    ? (int)Math.Round(model[i].TotalAmount / sumBefore[i] * 100 - 100)
                    : 0;
            }

            return PartialView(model);
        }

        [Permission(Permissions.Customer.Read)]
        public ActionResult Reports()
        {
            var model = new CustomerReportsModel
            {
                GridPageSize = _adminAreaSettings.GridPageSize,
                UsernamesEnabled = _customerSettings.CustomerLoginType != CustomerLoginType.Email,
                TopCustomers = new TopCustomersReportModel
                {
                    AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList(),
                    AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList(),
                    AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(false).ToList()
                }
            };

            return View(model);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Customer.Read)]
        public ActionResult ReportTopCustomersList(GridCommand command, TopCustomersReportModel model)
        {
            DateTime? startDateValue = (model.StartDate == null)
                ? null
                : (DateTime?)Services.DateTimeHelper.ConvertToUtcTime(model.StartDate.Value, Services.DateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null)
                ? null
                : (DateTime?)Services.DateTimeHelper.ConvertToUtcTime(model.EndDate.Value, Services.DateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)model.OrderStatusId : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)model.PaymentStatusId : null;
            ShippingStatus? shippingStatus = model.ShippingStatusId > 0 ? (ShippingStatus?)model.ShippingStatusId : null;

            // Sorting.
            var sorting = ReportSorting.ByAmountDesc;

            if (command.SortDescriptors?.Any() ?? false)
            {
                var sort = command.SortDescriptors.First();
                if (sort.Member == nameof(TopCustomerReportLineModel.OrderCount))
                {
                    sorting = sort.SortDirection == ListSortDirection.Ascending
                        ? ReportSorting.ByQuantityAsc
                        : ReportSorting.ByQuantityDesc;
                }
                else if (sort.Member == nameof(TopCustomerReportLineModel.OrderTotal))
                {
                    sorting = sort.SortDirection == ListSortDirection.Ascending
                        ? ReportSorting.ByAmountAsc
                        : ReportSorting.ByAmountDesc;
                }
            }

            var items = _customerReportService.GetTopCustomersReport(
                startDateValue,
                endDateValue,
                orderStatus,
                paymentStatus,
                shippingStatus,
                sorting,
                command.Page - 1,
                command.PageSize);

            var gridModel = new GridModel<TopCustomerReportLineModel>
            {
                Total = items.TotalCount,
                Data = CreateCustomerReportLineModel(items)
            };

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [NonAction]
        protected void CalculateOrdersAmount(DashboardChartReportModel report, IList<RegistredCustomersDate> allCustomers, List<RegistredCustomersDate> customers, DateTime fromDate, DateTime toDate)
        {
            foreach (var item in report.DataSets)
            {
                item.TotalAmountFormatted = ((int)Math.Round(item.Amount.Sum())).ToString("N");
            }

            var totalAmount = customers.Sum(x => x.Count);
            report.TotalAmountFormatted = ((int)Math.Round((double)totalAmount)).ToString("N");
            var sumBefore = (int)Math.Round((double)allCustomers
                .Where(x => x.Date < toDate && x.Date >= fromDate)
                .Select(x => x)
                .Count()
                );

            report.PercentageDelta = sumBefore <= 0 ? 0 : (int)Math.Round(totalAmount / (double)sumBefore * 100 - 100);
        }

        #endregion

        #region Current shopping cart/ wishlist

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cart.Read)]
        public ActionResult GetCartList(int customerId, int cartTypeId)
        {
            var customer = _customerService.GetCustomerById(customerId);
            var cart = customer.GetCartItems((ShoppingCartType)cartTypeId);

            var gridModel = new GridModel<ShoppingCartItemModel>
            {
                Data = cart.Select(sci =>
                {
                    decimal taxRate;
                    var store = Services.StoreService.GetStoreById(sci.Item.StoreId);

                    var sciModel = new ShoppingCartItemModel
                    {
                        Id = sci.Item.Id,
                        Store = store != null ? store.Name : "".NaIfEmpty(),
                        ProductId = sci.Item.ProductId,
                        Quantity = sci.Item.Quantity,
                        ProductName = sci.Item.Product.Name,
                        ProductTypeName = sci.Item.Product.GetProductTypeLabel(Services.Localization),
                        ProductTypeLabelHint = sci.Item.Product.ProductTypeLabelHint,
                        UnitPrice = _priceFormatter.FormatPrice(_taxService.GetProductPrice(sci.Item.Product, _priceCalculationService.GetUnitPrice(sci, true), out taxRate)),
                        Total = _priceFormatter.FormatPrice(_taxService.GetProductPrice(sci.Item.Product, _priceCalculationService.GetSubTotal(sci, true), out taxRate)),
                        UpdatedOn = Services.DateTimeHelper.ConvertToUserTime(sci.Item.UpdatedOnUtc, DateTimeKind.Utc)
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
        [Permission(Permissions.Configuration.ActivityLog.Read)]
        public JsonResult ListActivityLog(GridCommand command, int customerId)
        {
            var activityLog = Services.CustomerActivity.GetAllActivities(null, null, customerId, 0, command.Page - 1, command.PageSize);

            var gridModel = new GridModel<CustomerModel.ActivityLogModel>
            {
                Data = activityLog.Select(x =>
                {
                    var m = new CustomerModel.ActivityLogModel
                    {
                        Id = x.Id,
                        ActivityLogTypeName = x.ActivityLogType.Name,
                        Comment = x.Comment,
                        CreatedOn = Services.DateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
                    };
                    return m;
                }),
                Total = activityLog.TotalCount
            };

            return new JsonResult { Data = gridModel };
        }

        #endregion

        #region GDPR

        [Permission(Permissions.Customer.Read)]
        public ActionResult Export(int id /* customerId */)
        {
            var customer = _customerService.GetCustomerById(id);
            if (customer == null || customer.Deleted)
                return HttpNotFound();

            var data = _gdprTool.Value.ExportCustomer(customer);
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);

            return File(Encoding.UTF8.GetBytes(json), "application/json", "customer-{0}.json".FormatInvariant(customer.Id));
        }

        #endregion
    }
}