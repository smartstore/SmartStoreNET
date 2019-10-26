using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Discounts;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Discounts;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class DiscountController : AdminControllerBase
    {
        #region Fields

        private readonly IDiscountService _discountService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly PluginMediator _pluginMediator;
        private readonly ICommonServices _services;

        #endregion

        #region Constructors

        public DiscountController(
            IDiscountService discountService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductService productService,
            IDateTimeHelper dateTimeHelper,
            ICustomerActivityService customerActivityService,
            PluginMediator pluginMediator,
            ICommonServices services)
        {
            this._discountService = discountService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._dateTimeHelper = dateTimeHelper;
            this._customerActivityService = customerActivityService;
            this._pluginMediator = pluginMediator;
            this._services = services;
        }

        #endregion

        #region Utilities

        [NonAction]
        public string GetRequirementUrlInternal(IDiscountRequirementRule discountRequirementRule, Discount discount, int? discountRequirementId)
        {
            Guard.NotNull(discountRequirementRule, nameof(discountRequirementRule));
            Guard.NotNull(discount, nameof(discount));

            string url = string.Format("{0}{1}", _services.WebHelper.GetStoreLocation(), discountRequirementRule.GetConfigurationUrl(discount.Id, discountRequirementId));

            return url;
        }

        [NonAction]
        private void PrepareDiscountModel(DiscountModel model, Discount discount)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var language = _services.WorkContext.WorkingLanguage;

            model.PrimaryStoreCurrencyCode = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;
            model.AvailableDiscountRequirementRules.Add(
                new SelectListItem { Text = _services.Localization.GetResource("Admin.Promotions.Discounts.Requirements.DiscountRequirementType.Select"), Value = "" }
            );

            var discountRules = _discountService.LoadAllDiscountRequirementRules();
            foreach (var discountRule in discountRules)
            {
                model.AvailableDiscountRequirementRules.Add(new SelectListItem
                {
                    Text = _pluginMediator.GetLocalizedFriendlyName(discountRule.Metadata),
                    Value = discountRule.Metadata.SystemName
                });
            }

            if (discount != null)
            {
                // applied to categories
                model.AppliedToCategoryModels = discount.AppliedToCategories
                    .Where(x => x != null && !x.Deleted)
                    .Select(x => new DiscountModel.AppliedToCategoryModel { CategoryId = x.Id, Name = x.GetLocalized(y => y.Name, language) })
                    .ToList();

                // applied to manufacturers
                model.AppliedToManufacturerModels = discount.AppliedToManufacturers
                    .Where(x => x != null && !x.Deleted)
                    .Select(x => new DiscountModel.AppliedToManufacturerModel { ManufacturerId = x.Id, ManufacturerName = x.GetLocalized(y => y.Name, language) })
                    .ToList();

                // applied to products
                model.AppliedToProductModels = discount.AppliedToProducts
                    .Where(x => x != null && !x.Deleted)
                    .Select(x => new DiscountModel.AppliedToProductModel { ProductId = x.Id, ProductName = x.GetLocalized(y => y.Name, language) })
                    .ToList();

                // requirements
                foreach (var dr in discount.DiscountRequirements.OrderBy(dr => dr.Id))
                {
                    var drr = _discountService.LoadDiscountRequirementRuleBySystemName(dr.DiscountRequirementRuleSystemName);
                    if (drr != null)
                    {
                        model.DiscountRequirementMetaInfos.Add(new DiscountModel.DiscountRequirementMetaInfo
                        {
                            DiscountRequirementId = dr.Id,
                            RuleName = _pluginMediator.GetLocalizedFriendlyName(drr.Metadata),
                            ConfigurationUrl = GetRequirementUrlInternal(drr.Value, discount, dr.Id)
                        });
                    }
                }
            }
        }

        #endregion

        #region Discounts

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Promotion.Discount.Read)]
        public ActionResult List()
        {
            var discounts = _discountService.GetAllDiscounts(null, null, true);
            var gridModel = new GridModel<DiscountModel>
            {
                Data = discounts.Select(x =>
                {
                    var discountModel = x.ToModel();
                    discountModel.DiscountRequirementsCount = x.DiscountRequirements.Count;
                    return discountModel;
                }),
                Total = discounts.Count()
            };

            return View(gridModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Promotion.Discount.Read)]
        public ActionResult List(GridCommand command)
        {
            var model = new GridModel<DiscountModel>();

            var discounts = _discountService.GetAllDiscounts(null, null, true);

            model.Data = discounts.Select(x =>
            {
                var discountModel = x.ToModel();
                discountModel.DiscountRequirementsCount = x.DiscountRequirements.Count;
                return discountModel;
            });

            model.Total = discounts.Count();

            return new JsonResult
            {
                Data = model
            };
        }

        [Permission(Permissions.Promotion.Discount.Create)]
        public ActionResult Create()
        {
            var model = new DiscountModel();
            PrepareDiscountModel(model, null);
            //default values
            model.LimitationTimes = 1;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [Permission(Permissions.Promotion.Discount.Create)]
        public ActionResult Create(DiscountModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var discount = model.ToEntity();
                _discountService.InsertDiscount(discount);

                //activity log
                _customerActivityService.InsertActivity("AddNewDiscount", _services.Localization.GetResource("ActivityLog.AddNewDiscount"), discount.Name);

                NotifySuccess(_services.Localization.GetResource("Admin.Promotions.Discounts.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = discount.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareDiscountModel(model, null);
            return View(model);
        }

        [Permission(Permissions.Promotion.Discount.Read)]
        public ActionResult Edit(int id)
        {
            var discount = _discountService.GetDiscountById(id);
            if (discount == null)
                //No discount found with the specified id
                return RedirectToAction("List");

            var model = discount.ToModel();
            PrepareDiscountModel(model, discount);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [Permission(Permissions.Promotion.Discount.Update)]
        public ActionResult Edit(DiscountModel model, bool continueEditing)
        {
            var discount = _discountService.GetDiscountById(model.Id);
            if (discount == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var prevDiscountType = discount.DiscountType;
                discount = model.ToEntity(discount);
                _discountService.UpdateDiscount(discount);

                //clean up old references (if changed) and update "HasDiscountsApplied" properties
                if (prevDiscountType == DiscountType.AssignedToCategories && discount.DiscountType != DiscountType.AssignedToCategories)
                {
                    //applied to categories
                    var categories = discount.AppliedToCategories.ToList();
                    discount.AppliedToCategories.Clear();
                    _discountService.UpdateDiscount(discount);

                    categories.Each(x => _categoryService.UpdateHasDiscountsApplied(x));
                }

                if (prevDiscountType == DiscountType.AssignedToManufacturers && discount.DiscountType != DiscountType.AssignedToManufacturers)
                {
                    var manufacturers = discount.AppliedToManufacturers.ToList();
                    discount.AppliedToManufacturers.Clear();
                    _discountService.UpdateDiscount(discount);

                    manufacturers.Each(x => _manufacturerService.UpdateHasDiscountsApplied(x));
                }

                if (prevDiscountType == DiscountType.AssignedToSkus && discount.DiscountType != DiscountType.AssignedToSkus)
                {
                    //applied to products
                    var products = discount.AppliedToProducts.ToList();
                    discount.AppliedToProducts.Clear();
                    _discountService.UpdateDiscount(discount);

                    products.Each(x => _productService.UpdateHasDiscountsApplied(x));
                }

                //activity log
                _customerActivityService.InsertActivity("EditDiscount", _services.Localization.GetResource("ActivityLog.EditDiscount"), discount.Name);

                NotifySuccess(_services.Localization.GetResource("Admin.Promotions.Discounts.Updated"));
                return continueEditing ? RedirectToAction("Edit", discount.Id) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareDiscountModel(model, discount);
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [Permission(Permissions.Promotion.Discount.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var discount = _discountService.GetDiscountById(id);
            if (discount == null)
                return RedirectToAction("List");

            var categories = discount.AppliedToCategories.ToList();
            var manufacturers = discount.AppliedToManufacturers.ToList();
            var products = discount.AppliedToProducts.ToList();

            _discountService.DeleteDiscount(discount);

            //update "HasDiscountsApplied" properties
            categories.Each(x => _categoryService.UpdateHasDiscountsApplied(x));
            manufacturers.Each(x => _manufacturerService.UpdateHasDiscountsApplied(x));
            products.Each(x => _productService.UpdateHasDiscountsApplied(x));

            //activity log
            _customerActivityService.InsertActivity("DeleteDiscount", _services.Localization.GetResource("ActivityLog.DeleteDiscount"), discount.Name);

            NotifySuccess(_services.Localization.GetResource("Admin.Promotions.Discounts.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Discount requirements

        [AcceptVerbs(HttpVerbs.Get)]
        [Permission(Permissions.Promotion.Discount.Read)]
        public ActionResult GetDiscountRequirementConfigurationUrl(string systemName, int discountId, int? discountRequirementId)
        {
            if (string.IsNullOrEmpty(systemName))
                throw new ArgumentNullException("systemName");

            var discountRequirementRule = _discountService.LoadDiscountRequirementRuleBySystemName(systemName);
            if (discountRequirementRule == null)
                throw new ArgumentException("Discount requirement rule could not be loaded");

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            string url = GetRequirementUrlInternal(discountRequirementRule.Value, discount, discountRequirementId);
            return Json(new { url }, JsonRequestBehavior.AllowGet);
        }

        [Permission(Permissions.Promotion.Discount.Read)]
        public ActionResult GetDiscountRequirementMetaInfo(int discountRequirementId, int discountId)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            var discountRequirement = discount.DiscountRequirements.Where(dr => dr.Id == discountRequirementId).FirstOrDefault();
            if (discountRequirement == null)
                throw new ArgumentException("Discount requirement could not be loaded");

            var discountRequirementRule = _discountService.LoadDiscountRequirementRuleBySystemName(discountRequirement.DiscountRequirementRuleSystemName);
            if (discountRequirementRule == null)
                throw new ArgumentException("Discount requirement rule could not be loaded");

            string url = GetRequirementUrlInternal(discountRequirementRule.Value, discount, discountRequirementId);
            string ruleName = _pluginMediator.GetLocalizedFriendlyName(discountRequirementRule.Metadata);

            return Json(new { url, ruleName }, JsonRequestBehavior.AllowGet);
        }

        [Permission(Permissions.Promotion.Discount.Update)]
        public ActionResult DeleteDiscountRequirement(int discountRequirementId, int discountId)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            var discountRequirement = discount.DiscountRequirements.Where(dr => dr.Id == discountRequirementId).FirstOrDefault();
            if (discountRequirement == null)
                throw new ArgumentException("Discount requirement could not be loaded");

            _discountService.DeleteDiscountRequirement(discountRequirement);

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Discount usage history

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Promotion.Discount.Read)]
        public ActionResult UsageHistoryList(int discountId, GridCommand command)
        {
            var model = new GridModel<DiscountModel.DiscountUsageHistoryModel>();

            var discount = _discountService.GetDiscountById(discountId);

            var discountHistories = _discountService.GetAllDiscountUsageHistory(discount.Id, null, command.Page - 1, command.PageSize);

            model.Data = discountHistories.Select(x => new DiscountModel.DiscountUsageHistoryModel
            {
                Id = x.Id,
                DiscountId = x.DiscountId,
                OrderId = x.OrderId,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
            }).ToList();

            model.Total = discountHistories.TotalCount;

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Promotion.Discount.Update)]
        public ActionResult UsageHistoryDelete(int discountId, int id, GridCommand command)
        {
            var discountHistory = _discountService.GetDiscountUsageHistoryById(id);

            _discountService.DeleteDiscountUsageHistory(discountHistory);

            return UsageHistoryList(discountId, command);
        }

        #endregion
    }
}
