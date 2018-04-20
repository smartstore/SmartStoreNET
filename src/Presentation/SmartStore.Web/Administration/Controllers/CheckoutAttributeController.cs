using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Orders;
using SmartStore.Core;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Logging;
using SmartStore.Services;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public class CheckoutAttributeController : AdminControllerBase
    {
		#region Fields

		private readonly ICommonServices _services;
		private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly ICustomerActivityService _customerActivityService;
		private readonly IStoreMappingService _storeMappingService;

		private readonly AdminAreaSettings _adminAreaSettings;

		#endregion

		#region Constructors

		public CheckoutAttributeController(
			ICommonServices services,
			ICheckoutAttributeService checkoutAttributeService,
            ILanguageService languageService, 
			ILocalizedEntityService localizedEntityService,
            ITaxCategoryService taxCategoryService,
            ICustomerActivityService customerActivityService,
            IMeasureService measureService, 
			MeasureSettings measureSettings,
			IStoreMappingService storeMappingService,
			AdminAreaSettings adminAreaSettings)
        {
			_services = services;
			_checkoutAttributeService = checkoutAttributeService;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _taxCategoryService = taxCategoryService;
            _customerActivityService = customerActivityService;
            _measureService = measureService;
            _measureSettings = measureSettings;
			_storeMappingService = storeMappingService;
			_adminAreaSettings = adminAreaSettings;
        }

        #endregion
        
        #region Utilities

        [NonAction]
        public void UpdateAttributeLocales(CheckoutAttribute checkoutAttribute, CheckoutAttributeModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(checkoutAttribute,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(checkoutAttribute,
                                                               x => x.TextPrompt,
                                                               localized.TextPrompt,
                                                               localized.LanguageId);
            }
        }

        [NonAction]
        public void UpdateValueLocales(CheckoutAttributeValue checkoutAttributeValue, CheckoutAttributeValueModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(checkoutAttributeValue,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);
            }
        }

        [NonAction]
        private void PrepareCheckoutAttributeModel(CheckoutAttributeModel model, CheckoutAttribute checkoutAttribute, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var taxCategories = _taxCategoryService.GetAllTaxCategories();

			foreach (var tc in taxCategories)
			{
				model.AvailableTaxCategories.Add(new SelectListItem
				{
					Text = tc.Name,
					Value = tc.Id.ToString(),
					Selected = (checkoutAttribute != null && !excludeProperties && tc.Id == checkoutAttribute.TaxCategoryId)
				});
			}

			if (!excludeProperties)
			{
				model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(checkoutAttribute);
			}

			model.AvailableStores = _services.StoreService.GetAllStores().ToSelectListItems(model.SelectedStoreIds);
		}

        #endregion
        
        #region Checkout attributes

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

			var model = new CheckoutAttributeListModel
			{
				GridPageSize = _adminAreaSettings.GridPageSize
			};

			model.AvailableStores = _services.StoreService.GetAllStores().ToSelectListItems();

			return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command)
        {
			var model = new GridModel<CheckoutAttributeModel>();

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var query = _checkoutAttributeService.GetCheckoutAttributes(0, true);
				var pagedList = new PagedList<CheckoutAttribute>(query, command.Page - 1, command.PageSize);

				model.Data = pagedList.Select(x =>
				{
					var caModel = x.ToModel();
					caModel.AttributeControlTypeName = x.AttributeControlType.GetLocalizedEnum(_services.Localization, _services.WorkContext);
					return caModel;
				});

				model.Total = pagedList.TotalCount;
			}
			else
			{
				model.Data = Enumerable.Empty<CheckoutAttributeModel>();

				NotifyAccessDenied();
			}

			return new JsonResult
            {
                Data = model
            };
        }
        
        //create
        public ActionResult Create()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var model = new CheckoutAttributeModel();
			model.IsActive = true;

            //locales
            AddLocales(_languageService, model.Locales);
            PrepareCheckoutAttributeModel(model, null, true);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(CheckoutAttributeModel model, bool continueEditing)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var checkoutAttribute = model.ToEntity();
                _checkoutAttributeService.InsertCheckoutAttribute(checkoutAttribute);

                UpdateAttributeLocales(checkoutAttribute, model);

				_storeMappingService.SaveStoreMappings(checkoutAttribute, model.SelectedStoreIds);

				//activity log
				_customerActivityService.InsertActivity("AddNewCheckoutAttribute", _services.Localization.GetResource("ActivityLog.AddNewCheckoutAttribute"), checkoutAttribute.Name);

                NotifySuccess(_services.Localization.GetResource("Admin.Catalog.Attributes.CheckoutAttributes.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = checkoutAttribute.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareCheckoutAttributeModel(model, null, true);
            return View(model);
        }

        //edit
        public ActionResult Edit(int id)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(id);
            if (checkoutAttribute == null)
                return RedirectToAction("List");

            var model = checkoutAttribute.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = checkoutAttribute.GetLocalized(x => x.Name, languageId, false, false);
                locale.TextPrompt = checkoutAttribute.GetLocalized(x => x.TextPrompt, languageId, false, false);
            });
            PrepareCheckoutAttributeModel(model, checkoutAttribute, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(CheckoutAttributeModel model, bool continueEditing)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(model.Id);
            if (checkoutAttribute == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                checkoutAttribute = model.ToEntity(checkoutAttribute);
                _checkoutAttributeService.UpdateCheckoutAttribute(checkoutAttribute);

                UpdateAttributeLocales(checkoutAttribute, model);

				_storeMappingService.SaveStoreMappings(checkoutAttribute, model.SelectedStoreIds);

				//activity log
				_customerActivityService.InsertActivity("EditCheckoutAttribute", _services.Localization.GetResource("ActivityLog.EditCheckoutAttribute"), checkoutAttribute.Name);

                NotifySuccess(_services.Localization.GetResource("Admin.Catalog.Attributes.CheckoutAttributes.Updated"));
                return continueEditing ? RedirectToAction("Edit", checkoutAttribute.Id) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareCheckoutAttributeModel(model, checkoutAttribute, true);
            return View(model);
        }

        //delete
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(id);
            _checkoutAttributeService.DeleteCheckoutAttribute(checkoutAttribute);

            //activity log
            _customerActivityService.InsertActivity("DeleteCheckoutAttribute", _services.Localization.GetResource("ActivityLog.DeleteCheckoutAttribute"), checkoutAttribute.Name);

            NotifySuccess(_services.Localization.GetResource("Admin.Catalog.Attributes.CheckoutAttributes.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Checkout attribute values

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ValueList(int checkoutAttributeId, GridCommand command)
        {
			var model = new GridModel<CheckoutAttributeValueModel>();

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var values = _checkoutAttributeService.GetCheckoutAttributeValues(checkoutAttributeId);

				model.Data = values.Select(x => x.ToModel());

				model.Total = values.Count();
			}
			else
			{
				model.Data = Enumerable.Empty<CheckoutAttributeValueModel>();

				NotifyAccessDenied();
			}

			return new JsonResult
            {
                Data = model
            };
        }

        //create
        public ActionResult ValueCreatePopup(int checkoutAttributeId)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var model = new CheckoutAttributeValueModel();
            model.CheckoutAttributeId = checkoutAttributeId;
			model.PrimaryStoreCurrencyCode = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name;

            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost]
        public ActionResult ValueCreatePopup(string btnId, string formId, CheckoutAttributeValueModel model)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(model.CheckoutAttributeId);
            if (checkoutAttribute == null)
                return RedirectToAction("List");

			model.PrimaryStoreCurrencyCode = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name;

            if (ModelState.IsValid)
            {
                var sao = model.ToEntity();

                _checkoutAttributeService.InsertCheckoutAttributeValue(sao);
                UpdateValueLocales(sao, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public ActionResult ValueEditPopup(int id)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var cav = _checkoutAttributeService.GetCheckoutAttributeValueById(id);
            if (cav == null)
                return RedirectToAction("List");

            var model = cav.ToModel();
			model.PrimaryStoreCurrencyCode = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name;

            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = cav.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        public ActionResult ValueEditPopup(string btnId, string formId, CheckoutAttributeValueModel model)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var cav = _checkoutAttributeService.GetCheckoutAttributeValueById(model.Id);
            if (cav == null)
                return RedirectToAction("List");

			model.PrimaryStoreCurrencyCode = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name;

            if (ModelState.IsValid)
            {
                cav = model.ToEntity(cav);
                _checkoutAttributeService.UpdateCheckoutAttributeValue(cav);

                UpdateValueLocales(cav, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ValueDelete(int valueId, int checkoutAttributeId, GridCommand command)
        {
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var cav = _checkoutAttributeService.GetCheckoutAttributeValueById(valueId);

				_checkoutAttributeService.DeleteCheckoutAttributeValue(cav);
			}

            return ValueList(checkoutAttributeId, command);
        }

        #endregion
    }
}
