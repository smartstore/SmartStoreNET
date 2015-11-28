using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Orders;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Logging;
using SmartStore.Services;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Security;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class CheckoutAttributeController : AdminControllerBase
    {
        #region Fields

        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly ICustomerActivityService _customerActivityService;
		private readonly ICommonServices _services;

        #endregion

        #region Constructors

        public CheckoutAttributeController(ICheckoutAttributeService checkoutAttributeService,
            ILanguageService languageService, 
			ILocalizedEntityService localizedEntityService,
            ITaxCategoryService taxCategoryService,
            ICustomerActivityService customerActivityService,
            IMeasureService measureService, 
			MeasureSettings measureSettings,
			ICommonServices services)
        {
            this._checkoutAttributeService = checkoutAttributeService;
            this._languageService = languageService;
            this._localizedEntityService = localizedEntityService;
            this._taxCategoryService = taxCategoryService;
            this._customerActivityService = customerActivityService;
            this._measureService = measureService;
            this._measureSettings = measureSettings;
			this._services = services;
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

            //tax categories
            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            foreach (var tc in taxCategories)
                model.AvailableTaxCategories.Add(new SelectListItem() { Text = tc.Name, Value = tc.Id.ToString(), Selected = checkoutAttribute != null && !excludeProperties && tc.Id == checkoutAttribute.TaxCategoryId });
        }

        #endregion
        
        #region Checkout attributes

        //list
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes(true);
            var gridModel = new GridModel<CheckoutAttributeModel>
            {
                Data = checkoutAttributes.Select(x =>
                {
                    var caModel = x.ToModel();
                    caModel.AttributeControlTypeName = x.AttributeControlType.GetLocalizedEnum(_services.Localization, _services.WorkContext);
                    return caModel;
                }),
                Total = checkoutAttributes.Count()
            };
            return new JsonResult
            {
                Data = gridModel
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

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Create(CheckoutAttributeModel model, bool continueEditing)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var checkoutAttribute = model.ToEntity();
                _checkoutAttributeService.InsertCheckoutAttribute(checkoutAttribute);
                UpdateAttributeLocales(checkoutAttribute, model);

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
                //No checkout attribute found with the specified id
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

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Edit(CheckoutAttributeModel model, bool continueEditing)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(model.Id);
            if (checkoutAttribute == null)
                //No checkout attribute found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                checkoutAttribute = model.ToEntity(checkoutAttribute);
                _checkoutAttributeService.UpdateCheckoutAttribute(checkoutAttribute);

                UpdateAttributeLocales(checkoutAttribute, model);

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

        //list
        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ValueList(int checkoutAttributeId, GridCommand command)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var values = _checkoutAttributeService.GetCheckoutAttributeValues(checkoutAttributeId);
            var gridModel = new GridModel<CheckoutAttributeValueModel>
            {
                Data = values.Select(x => 
                    {
                        var model = x.ToModel();
                        //locales
                        //AddLocales(_languageService, model.Locales, (locale, languageId) =>
                        //{
                        //    locale.Name = x.GetLocalized(y => y.Name, languageId, false, false);
                        //});
                        return model;
                    }),
                Total = values.Count()
            };
            return new JsonResult
            {
                Data = gridModel
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
                //No checkout attribute found with the specified id
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
                //No checkout attribute value found with the specified id
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
                //No checkout attribute value found with the specified id
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

        //delete
        [GridAction(EnableCustomBinding = true)]
        public ActionResult ValueDelete(int valueId, int checkoutAttributeId, GridCommand command)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var cav = _checkoutAttributeService.GetCheckoutAttributeValueById(valueId);
            if (cav == null)
                throw new ArgumentException("No checkout attribute value found with the specified id");
            _checkoutAttributeService.DeleteCheckoutAttributeValue(cav);

            return ValueList(checkoutAttributeId, command);
        }


        #endregion
    }
}
