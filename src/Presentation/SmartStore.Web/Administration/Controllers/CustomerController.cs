using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.Admin.Models.Common;
using SmartStore.Admin.Models.Customers;
using SmartStore.Admin.Models.Dashboard;
using SmartStore.Admin.Models.ShoppingCart;
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
        private readonly IQueuedEmailService _queuedEmailService;
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
            IQueuedEmailService queuedEmailService,
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
            _queuedEmailService = queuedEmailService;
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
        public List<RegistredCustomersDate> GetRegisteredCustomersReport()
        {
            return _customerReportService.GetRegisteredCustomersDate();
        }

        [NonAction]
        protected IList<RegisteredCustomerReportLineModel> GetReportRegisteredCustomersModel()
        {
            var report = new List<RegisteredCustomerReportLineModel>();
            report.Add(new RegisteredCustomerReportLineModel()
            {
                Period = T("Admin.Customers.Reports.RegisteredCustomers.Fields.Period.7days"),
                Customers = _customerReportService.GetRegisteredCustomersReport(7)
            });
            report.Add(new RegisteredCustomerReportLineModel()
            {
                Period = T("Admin.Customers.Reports.RegisteredCustomers.Fields.Period.14days"),
                Customers = _customerReportService.GetRegisteredCustomersReport(14)
            });
            report.Add(new RegisteredCustomerReportLineModel()
            {
                Period = T("Admin.Customers.Reports.RegisteredCustomers.Fields.Period.month"),
                Customers = _customerReportService.GetRegisteredCustomersReport(30)
            });
            report.Add(new RegisteredCustomerReportLineModel()
            {
                Period = T("Admin.Customers.Reports.RegisteredCustomers.Fields.Period.year"),
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
            model.UsernamesEnabled = _customerSettings.CustomerLoginType != CustomerLoginType.Email;
            model.AllowUsersToChangeUsernames = _customerSettings.AllowUsersToChangeUsernames;
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
            model.TitleEnabled = _customerSettings.TitleEnabled;
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
            model.UsernamesEnabled = _customerSettings.CustomerLoginType != CustomerLoginType.Email;
            model.AllowUsersToChangeUsernames = _customerSettings.AllowUsersToChangeUsernames;
            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            model.Deleted = customer.Deleted;

            foreach (var tzi in Services.DateTimeHelper.GetSystemTimeZones())
            {
                model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = tzi.Id == model.TimeZoneId });
            }

            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            model.VatNumberStatusNote = ((VatNumberStatus)customer.VatNumberStatusId).GetLocalizedEnum(Services.Localization, Services.WorkContext);
            model.CreatedOn = Services.DateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc);
            model.LastActivityDate = Services.DateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc);
            model.LastIpAddress = model.LastIpAddress;
            model.LastVisitedPage = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastVisitedPage);

            // Form fields.
            model.TitleEnabled = _customerSettings.TitleEnabled;
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
                return String.Format(T("Admin.Customers.CanOnlyBeCustomerOrGuest"),
                    _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Guests).Name,
                    _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered).Name);
            }
            if (!isInGuestsRole && !isInRegisteredRole)
            {
                //return "Add the customer to 'Guests' or 'Registered' customer role";
                return String.Format(T("Admin.Customers.MustBeCustomerOrGuest"),
                    _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Guests).Name,
                    _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered).Name);
            }
            //no errors
            return "";
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
            var allRoles = _customerService.GetAllCustomerRoles(true);
            var registeredRoleId = allRoles.First(x => x.SystemName.IsCaseInsensitiveEqual(SystemCustomerRoleNames.Registered)).Id;

            var listModel = new CustomerListModel
            {
                UsernamesEnabled = _customerSettings.CustomerLoginType != CustomerLoginType.Email,
                DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled,
                CompanyEnabled = _customerSettings.CompanyEnabled,
                PhoneEnabled = _customerSettings.PhoneEnabled,
                ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled,
                SearchCustomerRoleIds = registeredRoleId.ToString()
            };

            listModel.AvailableCustomerRoles = allRoles
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                .ToList();

            var q = new CustomerSearchQuery
            {
                CustomerRoleIds = new int[] { registeredRoleId },
                PageSize = _adminAreaSettings.GridPageSize
            };

            var customers = _customerService.SearchCustomers(q);

            // Customer list.
            listModel.Customers = new GridModel<CustomerModel>
            {
                Data = customers.Select(PrepareCustomerModelForList),
                Total = customers.TotalCount
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
                CustomerRoleIds = model.SearchCustomerRoleIds.ToIntArray(),
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
        [Permission(Permissions.Customer.Create)]
        public ActionResult Create(CustomerModel model, bool continueEditing, FormCollection form)
        {
            // Validate customer roles.
            var allowManagingCustomerRoles = Services.Permissions.Authorize(Permissions.Customer.EditRole);
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            var newCustomerRoles = new List<CustomerRole>();

            if (model.SelectedCustomerRoleIds != null)
            {
                foreach (var customerRole in allCustomerRoles)
                {
                    if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                    {
                        newCustomerRoles.Add(customerRole);
                    }
                }
            }

            var customerRolesError = ValidateCustomerRoles(newCustomerRoles);
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
                        NotifyError("Common.CustomerNumberAlreadyExists");
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
                    if (allowManagingCustomerRoles)
                    {
                        newCustomerRoles.Each(x => _customerService.InsertCustomerRoleMapping(new CustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = x.Id }));
                    }

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
        [Permission(Permissions.Customer.Update)]
        public ActionResult Edit(CustomerModel model, bool continueEditing, FormCollection form)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null || customer.Deleted)
                return RedirectToAction("List");

            // Validate customer roles.
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            var allowManagingCustomerRoles = Services.Permissions.Authorize(Permissions.Customer.EditRole);

            if (allowManagingCustomerRoles)
            {
                var newCustomerRoles = new List<CustomerRole>();

                foreach (var customerRole in allCustomerRoles)
                {
                    if (model.SelectedCustomerRoleIds != null && model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                    {
                        newCustomerRoles.Add(customerRole);
                    }
                }

                var customerRolesError = ValidateCustomerRoles(newCustomerRoles);
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
                            NotifyError("Common.CustomerNumberAlreadyExists");
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

                            foreach (var role in allCustomerRoles)
                            {
                                if (model.SelectedCustomerRoleIds?.Contains(role.Id) ?? false)
                                {
                                    if (!existingMappings.ContainsKey(role.Id))
                                    {
                                        _customerService.InsertCustomerRoleMapping(new CustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = role.Id });
                                    }
                                }
                                else if (existingMappings.ContainsKey(role.Id))
                                {
                                    existingMappings[role.Id].Each(x => _customerService.DeleteCustomerRoleMapping(x));
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
        [Permission(Permissions.Customer.Impersonate)]
        public ActionResult Impersonate(int id)
        {
            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                return RedirectToAction("List");

            // ensure that a non-admin user cannot impersonate as an administrator
            // otherwise, that user can simply impersonate as an administrator and gain additional administrative privileges
            if (!Services.WorkContext.CurrentCustomer.IsAdmin() && customer.IsAdmin())
            {
                NotifyError("A non-admin user cannot impersonate as an administrator");
                return RedirectToAction("Edit", customer.Id);
            }

            _genericAttributeService.SaveAttribute<int?>(Services.WorkContext.CurrentCustomer,
                SystemCustomerAttributeNames.ImpersonatedCustomerId, customer.Id);

            return RedirectToAction("Index", "Home", new { area = "" });
        }

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
            model.Address.TitleEnabled = _addressSettings.TitleEnabled;
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
            var model = new GridModel<CustomerModel.OrderModel>();

            var orders = _orderService.SearchOrders(0, customerId, null, null, null, null, null, null, null, null, 0, int.MaxValue);

            model.Data = orders.PagedForCommand(command)
                .Select(order =>
                {
                    var store = Services.StoreService.GetStoreById(order.StoreId);
                    var orderModel = new CustomerModel.OrderModel()
                    {
                        Id = order.Id,
                        OrderStatus = order.OrderStatus.GetLocalizedEnum(Services.Localization, Services.WorkContext),
                        PaymentStatus = order.PaymentStatus.GetLocalizedEnum(Services.Localization, Services.WorkContext),
                        ShippingStatus = order.ShippingStatus.GetLocalizedEnum(Services.Localization, Services.WorkContext),
                        OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false),
                        StoreName = store != null ? store.Name : "".NaIfEmpty(),
                        CreatedOn = Services.DateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc),
                    };
                    return orderModel;
                });

            model.Total = orders.Count;

            return new JsonResult
            {
                Data = model
            };
        }

        #endregion

        #region Reports

        [NonAction]
        private List<TopCustomerReportLineModel> CreateCustomerReportLineModel(IList<TopCustomerReportLine> items)
        {
            var customers = _customerService.GetCustomersByIds(items.Distinct().Select(x => x.CustomerId).ToArray());
            return items.Select(x =>
             {
                 var m = new TopCustomerReportLineModel()
                 {
                     CustomerId = x.CustomerId,
                     OrderTotal = x.OrderTotal.ToString("C0"),
                     OrderCount = x.OrderCount.ToString("N0"),
                 };

                 var customer = customers.Where(y => y.Id == x.CustomerId).FirstOrDefault();
                 if (customer != null)
                 {
                     m.CustomerDisplayName = customer.FormatUserName() ?? customer.FindEmail();
                 }

                 return m;
             }).ToList();
        }

        [Permission(Permissions.Customer.Read, false)]
        public ActionResult TopCustomersDashboardReport()
        {
            var model = new TopCustomersDashboardReportModel
            {
                TopCustomersByAmount = CreateCustomerReportLineModel(_customerReportService.GetTopCustomersReport(null, null, null, null, null, 1, 7)),
                TopCustomersByQuantity = CreateCustomerReportLineModel(_customerReportService.GetTopCustomersReport(null, null, null, null, null, 2, 7))
            };

            return PartialView(model);
        }

        [ChildActionOnly]
        [Permission(Permissions.Customer.Read, false)]
        public ActionResult ReportRegisteredCustomers()
        {
            var model = GetReportRegisteredCustomersModel();
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
            PeriodState periodStatus;
            // Today (includes all but yesterday)
            if (dataPoint >= userTime.Date)
            {
                periodStatus = PeriodState.Today;
            }
            // Yesterday (includes all but today)
            else if (dataPoint >= userTime.AddDays(-1).Date)
            {
                periodStatus = PeriodState.Yesterday;
            }
            // Last 7 days (older than today and yesterday)
            else if (dataPoint >= userTime.AddDays(-6).Date)
            {
                periodStatus = PeriodState.Week;
            }
            // Last 28 days (older than last 7 days)
            else if (dataPoint >= userTime.AddDays(-27).Date)
            {
                periodStatus = PeriodState.Month;
            }
            // This year (older than last 28 days)
            else
            {
                periodStatus = PeriodState.Year;
            }

            if (periodStatus == PeriodState.Today)
            {
                reports[0].DataSets[0].Quantity[dataPoint.Hour]++;
                // Ignore yesterday if today
                reports[2].DataSets[0].Quantity[reports[2].DataSets[0].Quantity.Length - 1]++;
            }
            else if (periodStatus == PeriodState.Yesterday)
            {
                // Ignore today if yesterday
                reports[1].DataSets[0].Quantity[reports[1].DataSets[0].Quantity.Length - 1 - dataPoint.Hour]++;
                reports[2].DataSets[0].Quantity[reports[2].DataSets[0].Quantity.Length - 2]++;
            }
            else if (periodStatus == PeriodState.Week)
            {
                // Ignore today and yesterday
                var weekIndex = (userTime - dataPoint).Days;
                reports[2].DataSets[0].Quantity[reports[2].DataSets[0].Quantity.Length - weekIndex]++;
            }

            // Within last 28 days (older than last 7 days)
            if (periodStatus == PeriodState.Month)
            {
                // Ignore last 7 days
                var delta = (userTime - dataPoint).Days;
                var monthIndex = delta / 7 - (delta % 7 == 0 ? delta / 7 > 0 ? 1 : 0 : 0);
                reports[3].DataSets[0].Quantity[reports[3].DataSets[0].Amount.Length - monthIndex - 1]++;
            }
            else if (periodStatus != PeriodState.Year)
            {
                // Applies to last 7 days
                reports[3].DataSets[0].Quantity[reports[3].DataSets[0].Quantity.Length - 1]++;
            }

            // This year - need to check if still this year when period is not today or this year (0 || 4)
            if (periodStatus == PeriodState.Today || periodStatus == PeriodState.Year || dataPoint.Year == userTime.Year)
            {
                reports[4].DataSets[0].Quantity[dataPoint.Month - 1]++;
            }
        }

        /// <summary>
        /// Evaluates and displays customer registrations of this year as line chart
        /// </summary>
        /// <returns>Customers registrations chart</returns>
        public ActionResult RegisteredCustomersDashboardReport()
        {
            // Get customers of at least last 28 days (if year is younger)
            var beginningOfYear = new DateTime(DateTime.UtcNow.Year, 1, 1);
            var startDate = (DateTime.UtcNow.Date - beginningOfYear).Days < 28 ? DateTime.UtcNow.AddDays(-27).Date : beginningOfYear;
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

            var userTime = _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc);
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
                        model[i].Labels[j] = userTime.Date.AddHours(j).ToString("t");
                    }
                    // This year
                    else if (i == 4)
                    {
                        model[i].Labels[j] = new DateTime(userTime.Year, j + 1, 1).ToString("Y");
                    }
                    // Last 7 days
                    else if (i == 2)
                    {
                        model[i].Labels[j] = userTime.Date.AddDays(-6 + j).ToString("m");
                    }
                    // Last 28 days
                    else
                    {
                        model[i].Labels[j] = userTime.Date.AddDays(
                            -(7 * model[i].Labels.Length) + j * 7).ToString("m") + " - "
                            + userTime.Date.AddDays(-(7 * model[i].Labels.Length) + (j + 1) * 7 - (j != model[i].Labels.Length - 1 ? 1 : 0)).ToString("m");
                    }
                }
            }

            // Get registrations for corresponding period to calculate change in percentage 
            var sumBefore = new decimal[]
            {
                // Get registration count for day before
                model[1].TotalAmount,
                customerDates.Where( x =>
                    x >= DateTime.UtcNow.Date.AddDays(-2) && x < DateTime.UtcNow.Date.AddDays(-1)
                ).Count(),

                // Get registration count for week before
                customerDates.Where( x =>
                    x >= DateTime.UtcNow.Date.AddDays(-14) && x < DateTime.UtcNow.Date.AddDays(-7)
                ).Count(),

                // Get registration count for month before
                _customerReportService.GetCustomerRegistrations(beginningOfYear.AddDays(-56), DateTime.UtcNow.Date.AddDays(-28)),

                // Get registration count for year before
                _customerReportService.GetCustomerRegistrations(beginningOfYear.AddYears(-1), DateTime.UtcNow.AddYears(-1))
            };

            // Format percentage value
            for (int i = 0; i < model.Count; i++)
            {
                model[i].PercentageDelta = model[i].TotalAmount <= 0 ? 0
                    : sumBefore[i] <= 0 ? 100
                    : (int)Math.Round(model[i].TotalAmount / sumBefore[i] * 100 - 100);
            }

            return PartialView(model);
        }

        [Permission(Permissions.Customer.Read)]
        public ActionResult Reports()
        {
            var model = new CustomerReportsModel();

            //customers by number of orders
            model.TopCustomersByNumberOfOrders = new TopCustomersReportModel();
            model.TopCustomersByNumberOfOrders.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.TopCustomersByNumberOfOrders.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
            model.TopCustomersByNumberOfOrders.AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(false).ToList();

            //customers by order total
            model.TopCustomersByOrderTotal = new TopCustomersReportModel();
            model.TopCustomersByOrderTotal.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.TopCustomersByOrderTotal.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
            model.TopCustomersByOrderTotal.AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(false).ToList();

            return View(model);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Customer.Read)]
        public ActionResult ReportTopCustomersByOrderTotalList(GridCommand command, TopCustomersReportModel model)
        {
            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)Services.DateTimeHelper.ConvertToUtcTime(model.StartDate.Value, Services.DateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)Services.DateTimeHelper.ConvertToUtcTime(model.EndDate.Value, Services.DateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;
            ShippingStatus? shippingStatus = model.ShippingStatusId > 0 ? (ShippingStatus?)(model.ShippingStatusId) : null;

            var items = _customerReportService.GetTopCustomersReport(startDateValue, endDateValue, orderStatus, paymentStatus, shippingStatus, 1, 20);

            var gridModel = new GridModel<TopCustomerReportLineModel>
            {
                Data = CreateCustomerReportLineModel(items),
                Total = items.Count
            };

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Customer.Read)]
        public ActionResult ReportTopCustomersByNumberOfOrdersList(GridCommand command, TopCustomersReportModel model)
        {
            DateTime? startDateValue = (model.StartDate == null)
                ? null
                : (DateTime?)Services.DateTimeHelper.ConvertToUtcTime(model.StartDate.Value, Services.DateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null)
                ? null
                : (DateTime?)Services.DateTimeHelper.ConvertToUtcTime(model.EndDate.Value, Services.DateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;
            ShippingStatus? shippingStatus = model.ShippingStatusId > 0 ? (ShippingStatus?)(model.ShippingStatusId) : null;

            var items = _customerReportService.GetTopCustomersReport(startDateValue, endDateValue, orderStatus, paymentStatus, shippingStatus, 2, 20);

            var gridModel = new GridModel<TopCustomerReportLineModel>
            {
                Data = CreateCustomerReportLineModel(items),
                Total = items.Count
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

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Customer.Read)]
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