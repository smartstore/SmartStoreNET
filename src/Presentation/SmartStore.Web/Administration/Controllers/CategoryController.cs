using System;
using System.Linq;
using System.Collections.Generic;
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
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Discounts;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework;
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
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService; 
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IDiscountService _discountService;
        private readonly IPermissionService _permissionService;
        private readonly IAclService _aclService;
		private readonly IStoreService _storeService;
		private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerActivityService _customerActivityService;
		private readonly IDateTimeHelper _dateTimeHelper;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly CatalogSettings _catalogSettings;
		private readonly IEventPublisher _eventPublisher;

		#endregion

		#region Constructors

		public CategoryController(ICategoryService categoryService, ICategoryTemplateService categoryTemplateService,
            IManufacturerService manufacturerService, IProductService productService, 
            ICustomerService customerService,
            IUrlRecordService urlRecordService, IPictureService pictureService, ILanguageService languageService,
            ILocalizationService localizationService, ILocalizedEntityService localizedEntityService,
            IDiscountService discountService, IPermissionService permissionService,
			IAclService aclService, IStoreService storeService, IStoreMappingService storeMappingService,
            IWorkContext workContext,
            ICustomerActivityService customerActivityService,
			IDateTimeHelper dateTimeHelper,
			AdminAreaSettings adminAreaSettings,
            CatalogSettings catalogSettings,
            IEventPublisher eventPublisher)
        {
            this._categoryService = categoryService;
            this._categoryTemplateService = categoryTemplateService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._customerService = customerService;
            this._urlRecordService = urlRecordService;
            this._pictureService = pictureService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._localizedEntityService = localizedEntityService;
            this._discountService = discountService;
            this._permissionService = permissionService;
            this._aclService = aclService;
			this._storeService = storeService;
			this._storeMappingService = storeMappingService;
            this._workContext = workContext;
            this._customerActivityService = customerActivityService;
			this._dateTimeHelper = dateTimeHelper;
            this._adminAreaSettings = adminAreaSettings;
            this._catalogSettings = catalogSettings;
			this._eventPublisher = eventPublisher;
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
        protected void UpdatePictureSeoNames(Category category)
        {
            var picture = _pictureService.GetPictureById(category.PictureId.GetValueOrDefault());
            if (picture != null)
                _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(category.Name));
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
        protected void PrepareCategoryModel(CategoryModel model, Category category, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

			model.GridPageSize = _adminAreaSettings.GridPageSize;

            var discounts = _discountService.GetAllDiscounts(DiscountType.AssignedToCategories, null, true);
            model.AvailableDiscounts = discounts.ToList();

            if (!excludeProperties)
            {
                model.SelectedDiscountIds = category.AppliedDiscounts.Select(d => d.Id).ToArray();
            }

			if (category != null)
			{
				model.CreatedOn = _dateTimeHelper.ConvertToUserTime(category.CreatedOnUtc, DateTimeKind.Utc);
				model.UpdatedOn = _dateTimeHelper.ConvertToUserTime(category.UpdatedOnUtc, DateTimeKind.Utc);
			}

            model.AvailableDefaultViewModes.Add(
                new SelectListItem { Value = "grid", Text = _localizationService.GetResource("Common.Grid"), Selected = model.DefaultViewMode.IsCaseInsensitiveEqual("grid") }
            );
            model.AvailableDefaultViewModes.Add(
                new SelectListItem { Value = "list", Text = _localizationService.GetResource("Common.List"), Selected = model.DefaultViewMode.IsCaseInsensitiveEqual("list") }
            );

			// add available badges
			model.AvailableBadgeStyles.Add(new SelectListItem { Value = "0", Text = "Secondary", Selected = model.BadgeStyle == 0 });
            model.AvailableBadgeStyles.Add(new SelectListItem { Value = "1", Text = "Primary", Selected = model.BadgeStyle == 1 });
            model.AvailableBadgeStyles.Add(new SelectListItem { Value = "2", Text = "Success", Selected = model.BadgeStyle == 2 });
            model.AvailableBadgeStyles.Add(new SelectListItem { Value = "3", Text = "Info", Selected = model.BadgeStyle == 3 });
            model.AvailableBadgeStyles.Add(new SelectListItem { Value = "4", Text = "Warning", Selected = model.BadgeStyle == 4 });
            model.AvailableBadgeStyles.Add(new SelectListItem { Value = "5", Text = "Danger", Selected = model.BadgeStyle == 5 });
			model.AvailableBadgeStyles.Add(new SelectListItem { Value = "6", Text = "Light", Selected = model.BadgeStyle == 6 });
			model.AvailableBadgeStyles.Add(new SelectListItem { Value = "7", Text = "Dark", Selected = model.BadgeStyle == 7});
		}

        [NonAction]
        private void PrepareAclModel(CategoryModel model, Category category, bool excludeProperties)
        {
			Guard.NotNull(model, nameof(model));

            if (!excludeProperties)
            {
                if (category != null)
                {
                    model.SelectedCustomerRoleIds = _aclService.GetCustomerRoleIdsWithAccess(category);
                }
                else
                {
                    model.SelectedCustomerRoleIds = new int[0];
                }
            }

			model.AvailableCustomerRoles = _customerService.GetAllCustomerRoles(true).ToSelectListItems(model.SelectedCustomerRoleIds);
		}

        [NonAction]
        protected void SaveCategoryAcl(Category category, CategoryModel model)
        {
            var existingAclRecords = _aclService.GetAclRecords(category);
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds != null && model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Where(acl => acl.CustomerRoleId == customerRole.Id).Count() == 0)
                        _aclService.InsertAclRecord(category, customerRole.Id);
                }
                else
                {
                    //removed role
                    var aclRecordToDelete = existingAclRecords.Where(acl => acl.CustomerRoleId == customerRole.Id).FirstOrDefault();
                    if (aclRecordToDelete != null)
                        _aclService.DeleteAclRecord(aclRecordToDelete);
                }
            }
        }

		[NonAction]
		private void PrepareStoresMappingModel(CategoryModel model, Category category, bool excludeProperties)
		{
			Guard.NotNull(model, nameof(model));

			if (!excludeProperties)
			{
				model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(category);
			}

			model.AvailableStores = _storeService.GetAllStores().ToSelectListItems(model.SelectedStoreIds);
		}

        #endregion

        #region List / tree

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

			var allStores = _storeService.GetAllStores();
			var model = new CategoryListModel
			{
				GridPageSize = _adminAreaSettings.GridPageSize
			};

			foreach (var store in allStores)
			{
				model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });
			}

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command, CategoryListModel model)
        {
			var gridModel = new GridModel<CategoryModel>();
			
			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var categories = _categoryService.GetAllCategories(model.SearchCategoryName, command.Page - 1, command.PageSize, true, model.SearchAlias, false, model.SearchStoreId);
				gridModel.Data = categories.Select(x =>
				{
					var categoryModel = x.ToModel();
					categoryModel.Breadcrumb = x.GetCategoryPath(
						_categoryService, 
						languageId: _workContext.WorkingLanguage.Id, 
						aliasPattern: "<span class='badge badge-secondary'>{0}</span>");
					return categoryModel;
				});

				gridModel.Total = categories.TotalCount;
			}
			else
			{
				gridModel.Data = Enumerable.Empty<CategoryModel>();

				NotifyAccessDenied();
			}

			return new JsonResult
            {
                Data = gridModel
            };
        }

		// Ajax
		public ActionResult AllCategories(string label, int selectedId)
        {
			var categoryTree = _categoryService.GetCategoryTree(includeHidden: true);
			var categories = categoryTree.Flatten(false);

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
					selected = c.Id == selectedId
				};

			var mainList = query.ToList();

			var mruList = new MostRecentlyUsedList<string>(
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

        public ActionResult Tree()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

			var allStores = _storeService.GetAllStores();
			var model = new CategoryTreeModel();

			foreach (var store in allStores)
			{
				model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });
			}

			return View(model);
        }

        // Ajax
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult TreeLoadChildren(TreeViewItem node, CategoryTreeModel model)
        {
            var parentId = !string.IsNullOrEmpty(node.Value) ? Convert.ToInt32(node.Value) : 0;
			var urlHelper = new UrlHelper(this.ControllerContext.RequestContext);

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
				string text = (childCount > 0 ? "{0} ({1})".FormatInvariant(x.Name, childCount) : x.Name);

				var item = new TreeViewItem
				{
					Text = x.Alias.HasValue() ? "{0} <span class='badge badge-secondary'>{1}</span>".FormatCurrent(text, x.Alias) : text,
					Encoded = x.Alias.IsEmpty(),
					Value = x.Id.ToString(),
					LoadOnDemand = (childCount > 0),
					Enabled = true,
					ImageUrl = Url.Content(x.Published ? "~/Administration/Content/images/ico-content.png" : "~/Administration/Content/images/ico-content-o60.png"),
					Url = urlHelper.Action("Edit", "Category", new { id = x.Id })
				};

                return item;
            });

            return new JsonResult { Data = children };
        }

        //ajax
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

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var model = new CategoryModel();

			AddLocales(_languageService, model.Locales);

            PrepareTemplatesModel(model);
            PrepareCategoryModel(model, null, true);
			PrepareAclModel(model, null, false);
			PrepareStoresMappingModel(model, null, false);

            model.Published = true;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
		[ValidateInput(false)]
        public ActionResult Create(CategoryModel model, bool continueEditing, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var category = model.ToEntity();

				MediaHelper.UpdatePictureTransientStateFor(category, c => c.PictureId);

                _categoryService.InsertCategory(category);
                
				//search engine name
                model.SeName = category.ValidateSeName(model.SeName, category.Name, true);
                _urlRecordService.SaveSlug(category, model.SeName, 0);
                
				//locales
                UpdateLocales(category, model);
                
				//discounts
                var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToCategories, null, true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                        category.AppliedDiscounts.Add(discount);
                }
                _categoryService.UpdateCategory(category);

                //update "HasDiscountsApplied" property
                _categoryService.UpdateHasDiscountsApplied(category);

                //update picture seo file name
                UpdatePictureSeoNames(category);

                //ACL (customer roles)
                SaveCategoryAcl(category, model);

				//Stores
				_storeMappingService.SaveStoreMappings<Category>(category, model.SelectedStoreIds);

				_eventPublisher.Publish(new ModelBoundEvent(model, category, form));

                //activity log
                _customerActivityService.InsertActivity("AddNewCategory", _localizationService.GetResource("ActivityLog.AddNewCategory"), category.Name);

                NotifySuccess(_localizationService.GetResource("Admin.Catalog.Categories.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = category.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //templates
            PrepareTemplatesModel(model);
            //parent categories
            if (model.ParentCategoryId.HasValue)
            {
                var parentCategory = _categoryService.GetCategoryTree(model.ParentCategoryId.Value, true);
                if (parentCategory != null)
                    model.ParentCategoryBreadcrumb = _categoryService.GetCategoryPath(parentCategory);
                else
                    model.ParentCategoryId = 0;
            }

            PrepareCategoryModel(model, null, true);
            //ACL
            PrepareAclModel(model, null, true);
			//Stores
			PrepareStoresMappingModel(model, null, true);
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var category = _categoryService.GetCategoryById(id);
            if (category == null || category.Deleted)
                return RedirectToAction("List");

            var model = category.ToModel();

			//parent categories
			if (model.ParentCategoryId.HasValue)
            {
                var parentCategory = _categoryService.GetCategoryTree(model.ParentCategoryId.Value, true);

                if (parentCategory != null)
                    model.ParentCategoryBreadcrumb = _categoryService.GetCategoryPath(parentCategory);
                else
                    model.ParentCategoryId = 0;
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
            PrepareCategoryModel(model, category, false);

            PrepareAclModel(model, category, false);

			PrepareStoresMappingModel(model, category, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
		[ValidateInput(false)]
        public ActionResult Edit(CategoryModel model, bool continueEditing, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var category = _categoryService.GetCategoryById(model.Id);
            if (category == null || category.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                category = model.ToEntity(category);
				category.ParentCategoryId = model.ParentCategoryId ?? 0;

				MediaHelper.UpdatePictureTransientStateFor(category, c => c.PictureId);

                _categoryService.UpdateCategory(category);

                //search engine name
                model.SeName = category.ValidateSeName(model.SeName, category.Name, true);
                _urlRecordService.SaveSlug(category, model.SeName, 0);

                //locales
                UpdateLocales(category, model);

                //discounts
                var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToCategories, null, true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    {
                        //new role
                        if (category.AppliedDiscounts.Where(d => d.Id == discount.Id).Count() == 0)
                            category.AppliedDiscounts.Add(discount);
                    }
                    else
                    {
                        //removed role
                        if (category.AppliedDiscounts.Where(d => d.Id == discount.Id).Count() > 0)
                            category.AppliedDiscounts.Remove(discount);
                    }
                }
                _categoryService.UpdateCategory(category);

                //update "HasDiscountsApplied" property
                _categoryService.UpdateHasDiscountsApplied(category);

                //update picture seo file name
                UpdatePictureSeoNames(category);

                //ACL
                SaveCategoryAcl(category, model);

				//Stores
				_storeMappingService.SaveStoreMappings<Category>(category, model.SelectedStoreIds);

				_eventPublisher.Publish(new ModelBoundEvent(model, category, form));

                //activity log
                _customerActivityService.InsertActivity("EditCategory", _localizationService.GetResource("ActivityLog.EditCategory"), category.Name);

                NotifySuccess(_localizationService.GetResource("Admin.Catalog.Categories.Updated"));
                return continueEditing ? RedirectToAction("Edit", category.Id) : RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
            //parent categories
            if (model.ParentCategoryId.HasValue)
            {
                var parentCategory = _categoryService.GetCategoryTree(model.ParentCategoryId.Value, true);
                if (parentCategory != null)
                    model.ParentCategoryBreadcrumb = _categoryService.GetCategoryPath(parentCategory);
                else
                    model.ParentCategoryId = 0;
            }
            //templates
            PrepareTemplatesModel(model);
            PrepareCategoryModel(model, category, true);
            //ACL
            PrepareAclModel(model, category, true);
			//Stores
			PrepareStoresMappingModel(model, category, true);

            return View(model);
        }

        [ValidateInput(false)]
        public ActionResult InheritAclIntoChildren(int categoryId)
        {
            _categoryService.InheritAclIntoChildren(categoryId, false, true, false);

            return RedirectToAction("Edit", "Category", new { id = categoryId });
        }

        [ValidateInput(false)]
        public ActionResult InheritStoresIntoChildren(int categoryId)
        {
            _categoryService.InheritStoresIntoChildren(categoryId, false, true, false);

            return RedirectToAction("Edit", "Category", new { id = categoryId });
        }

        [HttpPost]
		public ActionResult Delete(int id, string deleteType)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();
			
            var category = _categoryService.GetCategoryById(id);
            if (category == null)
                return RedirectToAction("List");

			_categoryService.DeleteCategory(category, deleteType.IsCaseInsensitiveEqual("deletechilds"));

            _customerActivityService.InsertActivity("DeleteCategory", _localizationService.GetResource("ActivityLog.DeleteCategory"), category.Name);

            NotifySuccess(_localizationService.GetResource("Admin.Catalog.Categories.Deleted"));
            return RedirectToAction("List");
        }


        #endregion

        #region Products

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductList(GridCommand command, int categoryId)
        {
			var model = new GridModel<CategoryModel.CategoryProductModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
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
						DisplayOrder1 = x.DisplayOrder
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
			}
			else
			{
				model.Data = Enumerable.Empty<CategoryModel.CategoryProductModel>();

				NotifyAccessDenied();
			}

			return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductUpdate(GridCommand command, CategoryModel.CategoryProductModel model)
        {
			var productCategory = _categoryService.GetProductCategoryById(model.Id);

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				productCategory.IsFeaturedProduct = model.IsFeaturedProduct;
				productCategory.DisplayOrder = model.DisplayOrder1;

				_categoryService.UpdateProductCategory(productCategory);
			}

            return ProductList(command, productCategory.CategoryId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductDelete(int id, GridCommand command)
        {
			var productCategory = _categoryService.GetProductCategoryById(id);
			var categoryId = productCategory.CategoryId;

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				_categoryService.DeleteProductCategory(productCategory);
			}

            return ProductList(command, categoryId);
        }

		[HttpPost]
		public ActionResult ProductAdd(int categoryId, int[] selectedProductIds)
		{
			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
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
			}
			else
			{
				NotifyAccessDenied();
			}

			return new EmptyResult();
		}

		#endregion
	}
}
