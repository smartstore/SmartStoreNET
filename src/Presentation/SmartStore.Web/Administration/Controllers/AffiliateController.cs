using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Affiliates;
using SmartStore.Core;
using SmartStore.Core.Domain.Affiliates;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Security;
using SmartStore.Services.Affiliates;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class AffiliateController : AdminControllerBase
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWebHelper _webHelper;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IAffiliateService _affiliateService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly CustomerSettings _customerSettings;

        #endregion

        #region Constructors

        public AffiliateController(ILocalizationService localizationService,
            IWorkContext workContext, IDateTimeHelper dateTimeHelper, IWebHelper webHelper,
            ICountryService countryService, IStateProvinceService stateProvinceService,
            IPriceFormatter priceFormatter, IAffiliateService affiliateService,
            ICustomerService customerService, IOrderService orderService,
            AdminAreaSettings adminAreaSettings, CustomerSettings customerSettings)
        {
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._dateTimeHelper = dateTimeHelper;
            this._webHelper = webHelper;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._priceFormatter = priceFormatter;
            this._affiliateService = affiliateService;
            this._customerService = customerService;
            this._orderService = orderService;
            this._adminAreaSettings = adminAreaSettings;
            this._customerSettings = customerSettings;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected void PrepareAffiliateModel(AffiliateModel model, Affiliate affiliate, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (affiliate != null)
            {
                model.Id = affiliate.Id;
                model.Url = _webHelper.ModifyQueryString(_webHelper.GetStoreLocation(false), "affiliateid=" + affiliate.Id, null);
                if (!excludeProperties)
                {
                    model.Active = affiliate.Active;
                    model.Address = affiliate.Address.ToModel();
                }
            }

            model.Address.FirstNameEnabled = true;
            model.Address.FirstNameRequired = true;
            model.Address.LastNameEnabled = true;
            model.Address.LastNameRequired = true;
            model.Address.EmailEnabled = true;
            model.Address.EmailRequired = true;
            model.Address.CompanyEnabled = true;
            model.Address.CountryEnabled = true;
            model.Address.StateProvinceEnabled = true;
            model.Address.CityEnabled = true;
            model.Address.CityRequired = true;
            model.Address.StreetAddressEnabled = true;
            model.Address.StreetAddressRequired = true;
            model.Address.StreetAddress2Enabled = true;
            model.Address.ZipPostalCodeEnabled = true;
            model.Address.ZipPostalCodeRequired = true;
            model.Address.PhoneEnabled = true;
            model.Address.PhoneRequired = true;
            model.Address.FaxEnabled = true;

            model.GridPageSize = _adminAreaSettings.GridPageSize;
            model.UsernamesEnabled = _customerSettings.CustomerLoginType != CustomerLoginType.Email;

            //address
            foreach (var c in _countryService.GetAllCountries(true))
            {
                model.Address.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (affiliate != null && c.Id == affiliate.Address.CountryId) });
            }

            var states = model.Address.CountryId.HasValue ? _stateProvinceService.GetStateProvincesByCountryId(model.Address.CountryId.Value, true).ToList() : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (affiliate != null && s.Id == affiliate.Address.StateProvinceId) });
            }
            else
            {
                model.Address.AvailableStates.Add(new SelectListItem() { Text = T("Admin.Address.OtherNonUS"), Value = "0" });
            }
        }

        #endregion

        #region Methods

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Promotion.Affiliate.Read)]
        public ActionResult List()
        {
            var gridModel = new GridModel<AffiliateModel>();
            return View(gridModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Promotion.Affiliate.Read)]
        public ActionResult List(GridCommand command)
        {
            var model = new GridModel<AffiliateModel>();

            var affiliates = _affiliateService.GetAllAffiliates(true);

            model.Data = affiliates.PagedForCommand(command).Select(x =>
            {
                var m = new AffiliateModel();
                PrepareAffiliateModel(m, x, false);
                return m;
            });

            model.Total = affiliates.Count;

            return new JsonResult
            {
                Data = model
            };
        }

        [Permission(Permissions.Promotion.Affiliate.Create)]
        public ActionResult Create()
        {
            var model = new AffiliateModel();
            PrepareAffiliateModel(model, null, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [FormValueRequired("save", "save-continue")]
        [Permission(Permissions.Promotion.Affiliate.Create)]
        public ActionResult Create(AffiliateModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var affiliate = new Affiliate();

                affiliate.Active = model.Active;
                affiliate.Address = model.Address.ToEntity();
                affiliate.Address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (affiliate.Address.CountryId == 0)
                    affiliate.Address.CountryId = null;
                if (affiliate.Address.StateProvinceId == 0)
                    affiliate.Address.StateProvinceId = null;
                _affiliateService.InsertAffiliate(affiliate);

                NotifySuccess(T("Admin.Affiliates.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = affiliate.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareAffiliateModel(model, null, true);
            return View(model);
        }


        [Permission(Permissions.Promotion.Affiliate.Read)]
        public ActionResult Edit(int id)
        {
            var affiliate = _affiliateService.GetAffiliateById(id);
            if (affiliate == null || affiliate.Deleted)
                //No affiliate found with the specified id
                return RedirectToAction("List");

            var model = new AffiliateModel();
            PrepareAffiliateModel(model, affiliate, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Promotion.Affiliate.Update)]
        public ActionResult Edit(AffiliateModel model, bool continueEditing)
        {
            var affiliate = _affiliateService.GetAffiliateById(model.Id);
            if (affiliate == null || affiliate.Deleted)
                //No affiliate found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                affiliate.Active = model.Active;
                affiliate.Address = model.Address.ToEntity(affiliate.Address);
                //some validation
                if (affiliate.Address.CountryId == 0)
                    affiliate.Address.CountryId = null;
                if (affiliate.Address.StateProvinceId == 0)
                    affiliate.Address.StateProvinceId = null;
                _affiliateService.UpdateAffiliate(affiliate);

                NotifySuccess(T("Admin.Affiliates.Updated"));
                return continueEditing ? RedirectToAction("Edit", affiliate.Id) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareAffiliateModel(model, affiliate, true);
            return View(model);
        }

        [HttpPost]
        [Permission(Permissions.Promotion.Affiliate.Delete)]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var affiliate = _affiliateService.GetAffiliateById(id);
            if (affiliate == null)
                //No affiliate found with the specified id
                return RedirectToAction("List");

            _affiliateService.DeleteAffiliate(affiliate);
            NotifySuccess(T("Admin.Affiliates.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Promotion.Affiliate.Read)]
        public ActionResult AffiliatedOrderList(int affiliateId, GridCommand command)
        {
            var model = new GridModel<AffiliateModel.AffiliatedOrderModel>();

            var affiliate = _affiliateService.GetAffiliateById(affiliateId);
            var orders = _orderService.GetAllOrders(affiliate.Id, command.Page - 1, command.PageSize);

            model.Data = orders.Select(order =>
            {
                var orderModel = new AffiliateModel.AffiliatedOrderModel();
                orderModel.Id = order.Id;
                orderModel.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
                orderModel.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext);
                orderModel.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext);
                orderModel.OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false);
                orderModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);
                return orderModel;
            });
            model.Total = orders.TotalCount;

            return new JsonResult
            {
                Data = model
            };
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Promotion.Affiliate.Read)]
        public ActionResult AffiliatedCustomerList(int affiliateId, GridCommand command)
        {
            var model = new GridModel<AffiliateModel.AffiliatedCustomerModel>();

            var q = new CustomerSearchQuery
            {
                AffiliateId = affiliateId,
                PageIndex = command.Page - 1,
                PageSize = command.PageSize
            };

            var customers = _customerService.SearchCustomers(q);

            model.Data = customers.Select(customer =>
            {
                var customerModel = new AffiliateModel.AffiliatedCustomerModel
                {
                    Id = customer.Id,
                    Email = customer.Email,
                    Username = customer.Username,
                    FullName = customer.GetFullName()
                };

                return customerModel;
            });
            model.Total = customers.TotalCount;

            return new JsonResult
            {
                Data = model
            };
        }

        #endregion
    }
}
