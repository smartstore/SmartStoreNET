using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Orders;
using SmartStore.Core;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Services;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Stores;
using SmartStore.Services.Tax;
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
            Guard.NotNull(model, nameof(model));

            var taxCategories = _taxCategoryService.GetAllTaxCategories();

            foreach (var tc in taxCategories)
            {
                model.AvailableTaxCategories.Add(new SelectListItem
                {
                    Text = tc.Name,
                    Value = tc.Id.ToString(),
                    Selected = checkoutAttribute != null && !excludeProperties && tc.Id == checkoutAttribute.TaxCategoryId
                });
            }

            if (!excludeProperties)
            {
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(checkoutAttribute);
            }
        }

        #endregion

        #region Checkout attributes

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Cart.CheckoutAttribute.Read)]
        public ActionResult List()
        {
            var model = new CheckoutAttributeListModel
            {
                IsSingleStoreMode = Services.StoreService.IsSingleStoreMode(),
                GridPageSize = _adminAreaSettings.GridPageSize
            };

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cart.CheckoutAttribute.Read)]
        public ActionResult List(GridCommand command)
        {
            var model = new GridModel<CheckoutAttributeModel>();

            var query = _checkoutAttributeService.GetCheckoutAttributes(0, true);
            var pagedList = new PagedList<CheckoutAttribute>(query, command.Page - 1, command.PageSize);

            model.Data = pagedList.Select(x =>
            {
                var caModel = x.ToModel();
                caModel.AttributeControlTypeName = x.AttributeControlType.GetLocalizedEnum(_services.Localization, _services.WorkContext);
                return caModel;
            });

            model.Total = pagedList.TotalCount;

            return new JsonResult
            {
                Data = model
            };
        }

        [Permission(Permissions.Cart.CheckoutAttribute.Create)]
        public ActionResult Create()
        {
            var model = new CheckoutAttributeModel();
            model.IsActive = true;

            AddLocales(_languageService, model.Locales);
            PrepareCheckoutAttributeModel(model, null, true);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cart.CheckoutAttribute.Create)]
        public ActionResult Create(CheckoutAttributeModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var checkoutAttribute = model.ToEntity();
                _checkoutAttributeService.InsertCheckoutAttribute(checkoutAttribute);

                UpdateAttributeLocales(checkoutAttribute, model);
                SaveStoreMappings(checkoutAttribute, model.SelectedStoreIds);

                _customerActivityService.InsertActivity("AddNewCheckoutAttribute", T("ActivityLog.AddNewCheckoutAttribute"), checkoutAttribute.Name);

                NotifySuccess(T("Admin.Catalog.Attributes.CheckoutAttributes.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = checkoutAttribute.Id }) : RedirectToAction("List");
            }

            PrepareCheckoutAttributeModel(model, null, true);

            return View(model);
        }

        [Permission(Permissions.Cart.CheckoutAttribute.Read)]
        public ActionResult Edit(int id)
        {
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
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cart.CheckoutAttribute.Update)]
        public ActionResult Edit(CheckoutAttributeModel model, bool continueEditing)
        {
            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(model.Id);
            if (checkoutAttribute == null)
            {
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                checkoutAttribute = model.ToEntity(checkoutAttribute);
                _checkoutAttributeService.UpdateCheckoutAttribute(checkoutAttribute);

                UpdateAttributeLocales(checkoutAttribute, model);
                SaveStoreMappings(checkoutAttribute, model.SelectedStoreIds);

                _customerActivityService.InsertActivity("EditCheckoutAttribute", T("ActivityLog.EditCheckoutAttribute"), checkoutAttribute.Name);

                NotifySuccess(T("Admin.Catalog.Attributes.CheckoutAttributes.Updated"));
                return continueEditing ? RedirectToAction("Edit", checkoutAttribute.Id) : RedirectToAction("List");
            }

            PrepareCheckoutAttributeModel(model, checkoutAttribute, true);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cart.CheckoutAttribute.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(id);
            _checkoutAttributeService.DeleteCheckoutAttribute(checkoutAttribute);

            _customerActivityService.InsertActivity("DeleteCheckoutAttribute", T("ActivityLog.DeleteCheckoutAttribute"), checkoutAttribute.Name);

            NotifySuccess(T("Admin.Catalog.Attributes.CheckoutAttributes.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Checkout attribute values

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cart.CheckoutAttribute.Read)]
        public ActionResult ValueList(int checkoutAttributeId, GridCommand command)
        {
            var values = _checkoutAttributeService.GetCheckoutAttributeValues(checkoutAttributeId);

            var valuesModel = values
                .Select(x =>
                {
                    var m = x.ToModel();
                    m.NameString = Server.HtmlEncode(x.Color.IsEmpty() ? x.Name : $"{x.Name} - {x.Color}");

                    return m;
                })
                .ToList();

            var model = new GridModel<CheckoutAttributeValueModel>
            {
                Data = valuesModel,
                Total = values.Count()
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [Permission(Permissions.Cart.CheckoutAttribute.Update)]
        public ActionResult ValueCreatePopup(int checkoutAttributeId)
        {
            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(checkoutAttributeId);
            if (checkoutAttribute == null)
            {
                return RedirectToAction("List");
            }

            var model = new CheckoutAttributeValueModel();
            PrepareModel(model, checkoutAttribute);

            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cart.CheckoutAttribute.Update)]
        public ActionResult ValueCreatePopup(string btnId, string formId, CheckoutAttributeValueModel model)
        {
            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(model.CheckoutAttributeId);
            if (checkoutAttribute == null)
            {
                return RedirectToAction("List");
            }

            PrepareModel(model, checkoutAttribute);

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

            return View(model);
        }

        [Permission(Permissions.Cart.CheckoutAttribute.Update)]
        public ActionResult ValueEditPopup(int id)
        {
            var cav = _checkoutAttributeService.GetCheckoutAttributeValueById(id);
            if (cav == null)
            {
                return RedirectToAction("List");
            }

            var model = cav.ToModel();
            PrepareModel(model, cav.CheckoutAttribute);

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = cav.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cart.CheckoutAttribute.Update)]
        public ActionResult ValueEditPopup(string btnId, string formId, CheckoutAttributeValueModel model)
        {
            var cav = _checkoutAttributeService.GetCheckoutAttributeValueById(model.Id);
            if (cav == null)
            {
                return RedirectToAction("List");
            }

            PrepareModel(model, cav.CheckoutAttribute);

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

            return View(model);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cart.CheckoutAttribute.Update)]
        public ActionResult ValueDelete(int id, int checkoutAttributeId, GridCommand command)
        {
            var cav = _checkoutAttributeService.GetCheckoutAttributeValueById(id);

            _checkoutAttributeService.DeleteCheckoutAttributeValue(cav);

            return ValueList(checkoutAttributeId, command);
        }

        private void PrepareModel(CheckoutAttributeValueModel model, CheckoutAttribute attribute)
        {
            model.CheckoutAttributeId = attribute?.Id ?? 0;
            model.IsListTypeAttribute = attribute?.IsListTypeAttribute() ?? false;

            model.PrimaryStoreCurrencyCode = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId)?.GetLocalized(x => x.Name) ?? string.Empty;
        }

        #endregion
    }
}
