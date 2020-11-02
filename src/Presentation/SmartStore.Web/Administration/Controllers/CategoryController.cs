using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Rules;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Discounts;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class CategoryController : AdminControllerBase
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly ICategoryTemplateService _categoryTemplateService;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IDiscountService _discountService;
        private readonly IAclService _aclService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IRuleStorage _ruleStorage;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly Lazy<IGenericAttributeService> _genericAttributeService;
        private readonly Lazy<ITaskScheduler> _taskScheduler;
        private readonly Lazy<IScheduleTaskService> _scheduleTaskService;

        #endregion

        #region Constructors

        public CategoryController(
            ICategoryService categoryService,
            ICategoryTemplateService categoryTemplateService,
            IProductService productService,
            IUrlRecordService urlRecordService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IDiscountService discountService,
            IAclService aclService,
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            ICustomerActivityService customerActivityService,
            IRuleStorage ruleStorage,
            IDateTimeHelper dateTimeHelper,
            AdminAreaSettings adminAreaSettings,
            CatalogSettings catalogSettings,
            IEventPublisher eventPublisher,
            Lazy<IGenericAttributeService> genericAttributeService,
            Lazy<ITaskScheduler> taskScheduler,
            Lazy<IScheduleTaskService> scheduleTaskService)
        {
            _categoryService = categoryService;
            _categoryTemplateService = categoryTemplateService;
            _productService = productService;
            _urlRecordService = urlRecordService;
            _languageService = languageService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _discountService = discountService;
            _aclService = aclService;
            _storeService = storeService;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
            _customerActivityService = customerActivityService;
            _ruleStorage = ruleStorage;
            _dateTimeHelper = dateTimeHelper;
            _adminAreaSettings = adminAreaSettings;
            _catalogSettings = catalogSettings;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _taskScheduler = taskScheduler;
            _scheduleTaskService = scheduleTaskService;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected void UpdateLocales(Category category, CategoryModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(category, x => x.Name, localized.Name, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(category, x => x.FullName, localized.FullName, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(category, x => x.Description, localized.Description, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(category, x => x.BottomDescription, localized.BottomDescription, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(category, x => x.BadgeText, localized.BadgeText, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(category, x => x.MetaKeywords, localized.MetaKeywords, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(category, x => x.MetaDescription, localized.MetaDescription, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(category, x => x.MetaTitle, localized.MetaTitle, localized.LanguageId);

                //search engine name
                var seName = category.ValidateSeName(localized.SeName, localized.Name, false, localized.LanguageId);
                _urlRecordService.SaveSlug(category, seName, localized.LanguageId);
            }
        }

        [NonAction]
        protected void PrepareTemplatesModel(CategoryModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var templates = _categoryTemplateService.GetAllCategoryTemplates();
            foreach (var template in templates)
            {
                model.AvailableCategoryTemplates.Add(new SelectListItem()
                {
                    Text = template.Name,
                    Value = template.Id.ToString()
                });
            }
        }

        [NonAction]
        protected void PrepareCategoryModel(CategoryModel model, Category category)
        {
            Guard.NotNull(model, nameof(model));

            model.GridPageSize = _adminAreaSettings.GridPageSize;

            if (category != null)
            {
                model.CreatedOn = _dateTimeHelper.ConvertToUserTime(category.CreatedOnUtc, DateTimeKind.Utc);
                model.UpdatedOn = _dateTimeHelper.ConvertToUserTime(category.UpdatedOnUtc, DateTimeKind.Utc);
                model.SelectedDiscountIds = category.AppliedDiscounts.Select(d => d.Id).ToArray();
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(category);
                model.SelectedCustomerRoleIds = _aclService.GetCustomerRoleIdsWithAccessTo(category);
                model.SelectedRuleSetIds = category.RuleSets.Select(x => x.Id).ToArray();

                model.ShowRuleApplyButton = model.SelectedRuleSetIds.Any();
                if (!model.ShowRuleApplyButton)
                {
                    var productCategoriesQuery = _categoryService.GetProductCategoriesByCategoryId(category.Id, 0, int.MaxValue, true).SourceQuery;
                    model.ShowRuleApplyButton = productCategoriesQuery.Any(x => x.IsSystemMapping);
                }
            }

            model.AvailableDefaultViewModes.Add(
                new SelectListItem { Value = "grid", Text = T("Common.Grid"), Selected = model.DefaultViewMode.IsCaseInsensitiveEqual("grid") }
            );
            model.AvailableDefaultViewModes.Add(
                new SelectListItem { Value = "list", Text = T("Common.List"), Selected = model.DefaultViewMode.IsCaseInsensitiveEqual("list") }
            );

            // Add available badges.
            model.AvailableBadgeStyles.Add(new SelectListItem { Value = "0", Text = "Secondary", Selected = model.BadgeStyle == 0 });
            model.AvailableBadgeStyles.Add(new SelectListItem { Value = "1", Text = "Primary", Selected = model.BadgeStyle == 1 });
            model.AvailableBadgeStyles.Add(new SelectListItem { Value = "2", Text = "Success", Selected = model.BadgeStyle == 2 });
            model.AvailableBadgeStyles.Add(new SelectListItem { Value = "3", Text = "Info", Selected = model.BadgeStyle == 3 });
            model.AvailableBadgeStyles.Add(new SelectListItem { Value = "4", Text = "Warning", Selected = model.BadgeStyle == 4 });
            model.AvailableBadgeStyles.Add(new SelectListItem { Value = "5", Text = "Danger", Selected = model.BadgeStyle == 5 });
            model.AvailableBadgeStyles.Add(new SelectListItem { Value = "6", Text = "Light", Selected = model.BadgeStyle == 6 });
            model.AvailableBadgeStyles.Add(new SelectListItem { Value = "7", Text = "Dark", Selected = model.BadgeStyle == 7 });
        }

        #endregion

        #region List / tree

        public ActionResult Index()
        {
            var customerChoice = _genericAttributeService.Value.GetAttribute<string>("Customer", _workContext.CurrentCustomer.Id, "AdminCategoriesType");

            if (customerChoice != null && customerChoice.Equals("Tree"))
            {
                return RedirectToAction("Tree");
            }

            return RedirectToAction("List");
        }

        [Permission(Permissions.Catalog.Category.Read)]
        public ActionResult List()
        {
            var customerChoice = _genericAttributeService.Value.GetAttribute<string>("Customer", _workContext.CurrentCustomer.Id, "AdminCategoriesType");
            if (customerChoice == null || customerChoice.Equals("Tree"))
            {
                _genericAttributeService.Value.SaveAttribute(_workContext.CurrentCustomer, "AdminCategoriesType", "List");
            }

            var model = new CategoryListModel
            {
                IsSingleStoreMode = _storeService.IsSingleStoreMode(),
                GridPageSize = _adminAreaSettings.GridPageSize
            };

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Category.Read)]
        public ActionResult List(GridCommand command, CategoryListModel model)
        {
            var gridModel = new GridModel<CategoryModel>();

            var categories = _categoryService.GetAllCategories(model.SearchCategoryName, command.Page - 1, command.PageSize, true, model.SearchAlias, false, model.SearchStoreId);
            gridModel.Data = categories.Select(x =>
            {
                var categoryModel = x.ToModel();
                categoryModel.Breadcrumb = x.GetCategoryPath(
                    _categoryService,
                    languageId: _workContext.WorkingLanguage.Id,
                    aliasPattern: "<span class='badge badge-secondary'>{0}</span>").NaIfEmpty();
                return categoryModel;
            });

            gridModel.Total = categories.TotalCount;

            return new JsonResult
            {
                Data = gridModel
            };
        }

        // Ajax
        public ActionResult AllCategories(string label, string selectedIds)
        {
            var categoryTree = _categoryService.GetCategoryTree(includeHidden: true);
            var categories = categoryTree.Flatten(false);
            var selectedArr = selectedIds.ToIntArray();

            if (label.HasValue())
            {
                categories = (new[] { new Category { Name = label, Id = 0 } }).Concat(categories);

            }

            var query =
                from c in categories
                select new
                {
                    id = c.Id.ToString(),
                    text = c.GetCategoryPath(_categoryService, aliasPattern: "<span class='badge badge-secondary'>{0}</span>"),
                    selected = selectedArr.Contains(c.Id)
                };

            var mainList = query.ToList();

            var mruList = new TrimmedBuffer<string>(
                _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.MostRecentlyUsedCategories),
                _catalogSettings.MostRecentlyUsedCategoriesMaxSize)
                .Reverse()
                .Select(x =>
                {
                    var item = categoryTree.SelectNodeById(x.ToInt());
                    if (item != null)
                    {
                        return new
                        {
                            id = x,
                            text = _categoryService.GetCategoryPath(item, aliasPattern: "<span class='badge badge-secondary'>{0}</span>"),
                            selected = false
                        };
                    }

                    return null;
                })
                .Where(x => x != null)
                .ToList();

            object data = mainList;
            if (mruList.Count > 0)
            {
                data = new List<object>
                {
                    new Dictionary<string, object> { ["text"] = T("Common.Mru").Text, ["children"] = mruList },
                    new Dictionary<string, object> { ["text"] = T("Admin.Catalog.Categories").Text, ["children"] = mainList, ["main"] = true }
                };
            }

            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [Permission(Permissions.Catalog.Category.Read)]
        public ActionResult Tree()
        {
            var customerChoice = _genericAttributeService.Value.GetAttribute<string>("Customer", _workContext.CurrentCustomer.Id, "AdminCategoriesType");
            if (customerChoice == null || customerChoice.Equals("List"))
            {
                _genericAttributeService.Value.SaveAttribute(_workContext.CurrentCustomer, "AdminCategoriesType", "Tree");
            }

            var model = new CategoryTreeModel
            {
                IsSingleStoreMode = _storeService.IsSingleStoreMode(),
                CanEdit = Services.Permissions.Authorize(Permissions.Catalog.Category.Update)
            };

            return View(model);
        }

        // Ajax.
        [AcceptVerbs(HttpVerbs.Post)]
        [Permission(Permissions.Catalog.Category.Read)]
        public ActionResult TreeLoadChildren(TreeViewItem node, CategoryTreeModel model)
        {
            var parentId = !string.IsNullOrEmpty(node.Value) ? Convert.ToInt32(node.Value) : 0;
            var parentCategories = _categoryService.GetAllCategoriesByParentCategoryId(parentId, true);

            if (parentId == 0 && model.SearchStoreId != 0)
            {
                for (int i = parentCategories.Count - 1; i >= 0; --i)
                {
                    var category = parentCategories[i];
                    if (!category.LimitedToStores || (category.LimitedToStores && !_storeMappingService.GetStoresIdsWithAccess(category).Contains(model.SearchStoreId)))
                    {
                        parentCategories.Remove(category);
                    }
                }
            }

            var children = parentCategories.Select(x =>
            {
                var childCount = _categoryService.GetAllCategoriesByParentCategoryId(x.Id, true).Count;
                string text = childCount > 0 ? "{0} ({1})".FormatInvariant(x.Name, childCount) : x.Name;

                var item = new TreeViewItem
                {
                    Text = x.Alias.HasValue() ? "{0} <span class='badge badge-secondary'>{1}</span>".FormatCurrent(text, x.Alias) : text,
                    Encoded = x.Alias.IsEmpty(),
                    Value = x.Id.ToString(),
                    LoadOnDemand = childCount > 0,
                    Enabled = true,
                    ImageUrl = Url.Content(x.Published ? "~/Administration/Content/images/ico-content.png" : "~/Administration/Content/images/ico-content-o60.png"),
                    Url = Url.Action("Edit", "Category", new { id = x.Id })
                };

                return item;
            });

            return new JsonResult { Data = children };
        }

        // Ajax.
        [Permission(Permissions.Catalog.Category.Update)]
        public ActionResult TreeDrop(int item, int destinationitem, string position)
        {
            var categoryItem = _categoryService.GetCategoryById(item);
            var categoryDestinationItem = _categoryService.GetCategoryById(destinationitem);

            #region Re-calculate all display orders
            switch (position)
            {
                case "over":
                    categoryItem.ParentCategoryId = categoryDestinationItem.Id;
                    break;
                case "before":
                case "after":
                    categoryItem.ParentCategoryId = categoryDestinationItem.ParentCategoryId;
                    break;
            }
            //update display orders
            int tmp = 0;
            foreach (var c in _categoryService.GetAllCategoriesByParentCategoryId(categoryItem.ParentCategoryId, true))
            {
                c.DisplayOrder = tmp;
                tmp += 10;
                _categoryService.UpdateCategory(c);

                switch (position)
                {
                    case "before":
                        categoryItem.DisplayOrder = categoryDestinationItem.DisplayOrder - 5;
                        break;
                    case "after":
                        categoryItem.DisplayOrder = categoryDestinationItem.DisplayOrder + 5;
                        break;
                }
            }
            #endregion

            #region Simple Sort method (Obsolete, has issues)
            //switch (position)
            //{
            //    case "over":
            //        categoryItem.ParentCategoryId = categoryDestinationItem.Id;
            //        break;
            //    case "before":
            //        categoryItem.ParentCategoryId = categoryDestinationItem.ParentCategoryId;
            //        categoryItem.DisplayOrder = categoryDestinationItem.DisplayOrder - 1;
            //        break;
            //    case "after":
            //        categoryItem.ParentCategoryId = categoryDestinationItem.ParentCategoryId;
            //        categoryItem.DisplayOrder = categoryDestinationItem.DisplayOrder + 1;
            //        break;
            //}
            #endregion

            _categoryService.UpdateCategory(categoryItem);

            return Json(new { success = true });
        }

        #endregion

        #region Create / Edit / Delete

        [Permission(Permissions.Catalog.Category.Create)]
        public ActionResult Create()
        {
            var model = new CategoryModel();

            AddLocales(_languageService, model.Locales);

            PrepareTemplatesModel(model);
            PrepareCategoryModel(model, null);

            model.Published = true;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Category.Create)]
        public ActionResult Create(CategoryModel model, bool continueEditing, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var category = model.ToEntity();

                _categoryService.InsertCategory(category);

                model.SeName = category.ValidateSeName(model.SeName, category.Name, true);
                _urlRecordService.SaveSlug(category, model.SeName, 0);

                UpdateLocales(category, model);

                var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToCategories, null, true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    {
                        category.AppliedDiscounts.Add(discount);
                    }
                }

                if (model.SelectedRuleSetIds?.Any() ?? false)
                {
                    _ruleStorage.ApplyRuleSetMappings(category, model.SelectedRuleSetIds);
                }

                _categoryService.UpdateCategory(category);
                _categoryService.UpdateHasDiscountsApplied(category);

                SaveAclMappings(category, model.SelectedCustomerRoleIds);
                SaveStoreMappings(category, model.SelectedStoreIds);

                _eventPublisher.Publish(new ModelBoundEvent(model, category, form));

                _customerActivityService.InsertActivity("AddNewCategory", T("ActivityLog.AddNewCategory"), category.Name);

                NotifySuccess(T("Admin.Catalog.Categories.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = category.Id }) : RedirectToAction("Index");
            }

            // If we got this far, something failed, redisplay form.
            PrepareTemplatesModel(model);
            if (model.ParentCategoryId.HasValue)
            {
                var parentCategory = _categoryService.GetCategoryTree(model.ParentCategoryId.Value, true);
                if (parentCategory != null)
                {
                    model.ParentCategoryBreadcrumb = _categoryService.GetCategoryPath(parentCategory);
                }
                else
                {
                    model.ParentCategoryId = 0;
                }
            }

            PrepareCategoryModel(model, null);

            return View(model);
        }

        [Permission(Permissions.Catalog.Category.Read)]
        public ActionResult Edit(int id)
        {
            var category = _categoryService.GetCategoryById(id);
            if (category == null || category.Deleted)
            {
                return RedirectToAction("Index");
            }

            var model = category.ToModel();

            if (model.ParentCategoryId.HasValue)
            {
                var parentCategory = _categoryService.GetCategoryTree(model.ParentCategoryId.Value, true);
                if (parentCategory != null)
                {
                    model.ParentCategoryBreadcrumb = _categoryService.GetCategoryPath(parentCategory);
                }
                else
                {
                    model.ParentCategoryId = 0;
                }
            }

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = category.GetLocalized(x => x.Name, languageId, false, false);
                locale.FullName = category.GetLocalized(x => x.FullName, languageId, false, false);
                locale.Description = category.GetLocalized(x => x.Description, languageId, false, false);
                locale.BottomDescription = category.GetLocalized(x => x.BottomDescription, languageId, false, false);
                locale.BadgeText = category.GetLocalized(x => x.BadgeText, languageId, false, false);
                locale.MetaKeywords = category.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = category.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = category.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = category.GetSeName(languageId, false, false);
            });

            PrepareTemplatesModel(model);
            PrepareCategoryModel(model, category);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing"), FormValueRequired("save")]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Category.Update)]
        public ActionResult Edit(CategoryModel model, bool continueEditing, FormCollection form)
        {
            var category = _categoryService.GetCategoryById(model.Id);
            if (category == null || category.Deleted)
            {
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                category = model.ToEntity(category);
                category.ParentCategoryId = model.ParentCategoryId ?? 0;

                _categoryService.UpdateCategory(category);

                model.SeName = category.ValidateSeName(model.SeName, category.Name, true);
                _urlRecordService.SaveSlug(category, model.SeName, 0);

                UpdateLocales(category, model);

                var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToCategories, null, true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    {
                        if (category.AppliedDiscounts.Where(d => d.Id == discount.Id).Count() == 0)
                            category.AppliedDiscounts.Add(discount);
                    }
                    else
                    {
                        if (category.AppliedDiscounts.Where(d => d.Id == discount.Id).Count() > 0)
                            category.AppliedDiscounts.Remove(discount);
                    }
                }

                // Add\remove assigned rule sets.
                _ruleStorage.ApplyRuleSetMappings(category, model.SelectedRuleSetIds);

                _categoryService.UpdateCategory(category);

                _categoryService.UpdateHasDiscountsApplied(category);

                SaveAclMappings(category, model.SelectedCustomerRoleIds);
                SaveStoreMappings(category, model.SelectedStoreIds);

                _eventPublisher.Publish(new ModelBoundEvent(model, category, form));

                _customerActivityService.InsertActivity("EditCategory", _localizationService.GetResource("ActivityLog.EditCategory"), category.Name);

                NotifySuccess(T("Admin.Catalog.Categories.Updated"));
                return continueEditing ? RedirectToAction("Edit", category.Id) : RedirectToAction("Index");
            }

            // If we got this far, something failed, redisplay form.
            if (model.ParentCategoryId.HasValue)
            {
                var parentCategory = _categoryService.GetCategoryTree(model.ParentCategoryId.Value, true);
                if (parentCategory != null)
                {
                    model.ParentCategoryBreadcrumb = _categoryService.GetCategoryPath(parentCategory);
                }
                else
                {
                    model.ParentCategoryId = 0;
                }
            }

            PrepareTemplatesModel(model);
            PrepareCategoryModel(model, category);

            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        [ActionName("Edit"), FormValueRequired("inherit-acl-into-children")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Category.Update)]
        public ActionResult InheritAclIntoChildren(CategoryModel model)
        {
            _categoryService.InheritAclIntoChildren(model.Id, false, true, false);

            return RedirectToAction("Edit", "Category", new { id = model.Id });
        }

        [ValidateInput(false)]
        [HttpPost]
        [ActionName("Edit"), FormValueRequired("inherit-stores-into-children")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Category.Update)]
        public ActionResult InheritStoresIntoChildren(CategoryModel model)
        {
            _categoryService.InheritStoresIntoChildren(model.Id, false, true, false);

            return RedirectToAction("Edit", "Category", new { id = model.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Category.Delete)]
        public ActionResult Delete(int id, string deleteType)
        {
            var category = _categoryService.GetCategoryById(id);
            if (category == null)
                return RedirectToAction("Index");

            _categoryService.DeleteCategory(category, deleteType.IsCaseInsensitiveEqual("deletechilds"));

            _customerActivityService.InsertActivity("DeleteCategory", _localizationService.GetResource("ActivityLog.DeleteCategory"), category.Name);

            NotifySuccess(_localizationService.GetResource("Admin.Catalog.Categories.Deleted"));
            return RedirectToAction("Index");
        }

        #endregion

        #region Products

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Category.Read)]
        public ActionResult ProductList(GridCommand command, int categoryId)
        {
            var model = new GridModel<CategoryModel.CategoryProductModel>();

            var productCategories = _categoryService.GetProductCategoriesByCategoryId(categoryId, command.Page - 1, command.PageSize, true);

            var products = _productService.GetProductsByIds(productCategories.Select(x => x.ProductId).ToArray());

            model.Data = productCategories.Select(x =>
            {
                var productModel = new CategoryModel.CategoryProductModel
                {
                    Id = x.Id,
                    CategoryId = x.CategoryId,
                    ProductId = x.ProductId,
                    IsFeaturedProduct = x.IsFeaturedProduct,
                    DisplayOrder1 = x.DisplayOrder,
                    IsSystemMapping = x.IsSystemMapping
                };

                var product = products.FirstOrDefault(y => y.Id == x.ProductId);

                if (product != null)
                {
                    productModel.ProductName = product.Name;
                    productModel.Sku = product.Sku;
                    productModel.ProductTypeName = product.GetProductTypeLabel(_localizationService);
                    productModel.ProductTypeLabelHint = product.ProductTypeLabelHint;
                    productModel.Published = product.Published;
                }

                return productModel;
            });

            model.Total = productCategories.TotalCount;

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Category.EditProduct)]
        public ActionResult ProductUpdate(GridCommand command, CategoryModel.CategoryProductModel model)
        {
            var productCategory = _categoryService.GetProductCategoryById(model.Id);

            productCategory.IsFeaturedProduct = model.IsFeaturedProduct;
            productCategory.DisplayOrder = model.DisplayOrder1;

            _categoryService.UpdateProductCategory(productCategory);

            return ProductList(command, productCategory.CategoryId);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Category.EditProduct)]
        public ActionResult ProductDelete(int id, GridCommand command)
        {
            var productCategory = _categoryService.GetProductCategoryById(id);
            var categoryId = productCategory.CategoryId;

            _categoryService.DeleteProductCategory(productCategory);

            return ProductList(command, categoryId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Category.EditProduct)]
        public ActionResult ProductAdd(int categoryId, int[] selectedProductIds)
        {
            var products = _productService.GetProductsByIds(selectedProductIds);
            ProductCategory productCategory = null;
            var maxDisplayOrder = -1;

            foreach (var product in products)
            {
                var existingProductCategories = _categoryService.GetProductCategoriesByCategoryId(categoryId, 0, int.MaxValue, true);

                if (existingProductCategories.FindProductCategory(product.Id, categoryId) == null)
                {
                    if (maxDisplayOrder == -1 && (productCategory = existingProductCategories.OrderByDescending(x => x.DisplayOrder).FirstOrDefault()) != null)
                    {
                        maxDisplayOrder = productCategory.DisplayOrder;
                    }

                    _categoryService.InsertProductCategory(new ProductCategory
                    {
                        CategoryId = categoryId,
                        ProductId = product.Id,
                        IsFeaturedProduct = false,
                        DisplayOrder = ++maxDisplayOrder
                    });
                }
            }

            return new EmptyResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApplyRules(int id)
        {
            var category = _categoryService.GetCategoryById(id);
            if (category == null || category.Deleted)
            {
                return RedirectToAction("Index");
            }

            var task = _scheduleTaskService.Value.GetTaskByType<ProductRuleEvaluatorTask>();
            if (task != null)
            {
                _taskScheduler.Value.RunSingleTask(task.Id, new Dictionary<string, string>
                {
                    { "CategoryIds", category.Id.ToString() }
                });

                NotifyInfo(T("Admin.System.ScheduleTasks.RunNow.Progress"));
            }
            else
            {
                NotifyError(T("Admin.System.ScheduleTasks.TaskNotFound", nameof(ProductRuleEvaluatorTask)));
            }

            return RedirectToAction("Edit", new { id = category.Id });
        }

        #endregion
    }
}
