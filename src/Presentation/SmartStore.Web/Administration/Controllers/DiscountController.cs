using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Discounts;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Logging;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Discounts;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
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
					.Where(x => x != null && !x.Deleted && !x.IsSystemProduct)
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

        public ActionResult List()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discounts = _discountService.GetAllDiscounts(null, null, true);
            var gridModel = new GridModel<DiscountModel>
            {
                Data = discounts.Select(x => x.ToModel()),
                Total = discounts.Count()
            };
            return View(gridModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command)
        {
			var model = new GridModel<DiscountModel>();

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageDiscounts))
			{
				var discounts = _discountService.GetAllDiscounts(null, null, true);

				model.Data = discounts.Select(x => x.ToModel()).ToList();
				model.Total = discounts.Count();
			}
			else
			{
				model.Data = Enumerable.Empty<DiscountModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = model
            };
        }
        
        public ActionResult Create()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var model = new DiscountModel();
            PrepareDiscountModel(model, null);
            //default values
            model.LimitationTimes = 1;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(DiscountModel model, bool continueEditing)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

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

        public ActionResult Edit(int id)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(id);
            if (discount == null)
                //No discount found with the specified id
                return RedirectToAction("List");

            var model = discount.ToModel();
            PrepareDiscountModel(model, discount);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(DiscountModel model, bool continueEditing)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

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
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

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
        public ActionResult GetDiscountRequirementConfigurationUrl(string systemName, int discountId, int? discountRequirementId)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            if (String.IsNullOrEmpty(systemName))
                throw new ArgumentNullException("systemName");
            
            var discountRequirementRule = _discountService.LoadDiscountRequirementRuleBySystemName(systemName);
            if (discountRequirementRule == null)
                throw new ArgumentException("Discount requirement rule could not be loaded");

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            string url = GetRequirementUrlInternal(discountRequirementRule.Value, discount, discountRequirementId);
            return Json(new { url = url }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDiscountRequirementMetaInfo(int discountRequirementId, int discountId)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

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

            return Json(new { url = url, ruleName = ruleName }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteDiscountRequirement(int discountRequirementId, int discountId)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

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
        public ActionResult UsageHistoryList(int discountId, GridCommand command)
        {
			var model = new GridModel<DiscountModel.DiscountUsageHistoryModel>();

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageDiscounts))
			{
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
			}
			else
			{
				model.Data = Enumerable.Empty<DiscountModel.DiscountUsageHistoryModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult UsageHistoryDelete(int discountId, int id, GridCommand command)
        {
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageDiscounts))
			{
				var discountHistory = _discountService.GetDiscountUsageHistoryById(id);

				_discountService.DeleteDiscountUsageHistory(discountHistory);
			}

            return UsageHistoryList(discountId, command);
        }

        #endregion
    }
}
