using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Menus;
using SmartStore.Collections;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Cms;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.UI;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class MenuController : AdminControllerBase
    {
        private readonly IMenuStorage _menuStorage;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;
        private readonly ICustomerService _customerService;
        private readonly IEnumerable<Lazy<IMenuItemProvider, MenuItemMetadata>> _menuItemProviders;
        private readonly AdminAreaSettings _adminAreaSettings;

        public MenuController(
            IMenuStorage menuStorage,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            ICustomerService customerService,
            IEnumerable<Lazy<IMenuItemProvider, MenuItemMetadata>> menuItemProviders,
            AdminAreaSettings adminAreaSettings)
        {
            _menuStorage = menuStorage;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
            _customerService = customerService;
            _menuItemProviders = menuItemProviders;
            _adminAreaSettings = adminAreaSettings;
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMenus))
            {
                return AccessDeniedView();
            }

            var model = new MenuRecordListModel
            {
                GridPageSize = _adminAreaSettings.GridPageSize,
                AvailableStores = Services.StoreService.GetAllStores().ToSelectListItems()
            };

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command, MenuRecordListModel model)
        {
            var gridModel = new GridModel<MenuRecordModel>();

            if (Services.Permissions.Authorize(StandardPermissionProvider.ManageMenus))
            {
                var items = _menuStorage.GetAllMenus(model.SystemName, model.StoreId, true, false, command.Page - 1, command.PageSize);

                gridModel.Data = items.Select(x =>
                {
                    var itemModel = new MenuRecordModel();
                    MiniMapper.Map(x, itemModel);

                    return itemModel;
                });

                gridModel.Total = items.TotalCount;
            }
            else
            {
                gridModel.Data = Enumerable.Empty<MenuRecordModel>();
                NotifyAccessDenied();
            }

            return new JsonResult
            {
                MaxJsonLength = int.MaxValue,
                Data = gridModel
            };
        }

        public ActionResult Create()
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMenus))
            {
                return AccessDeniedView();
            }

            var model = new MenuRecordModel();
            PrepareModel(model, null);
            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(MenuRecordModel model, bool continueEditing, FormCollection form)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMenus))
            {
                return AccessDeniedView();
            }

            if (ModelState.IsValid)
            {
                var menu = MiniMapper.Map<MenuRecordModel, MenuRecord>(model);

                _menuStorage.InsertMenu(menu);

                SaveStoreMappings(menu, model);
                SaveAclMappings(menu, model);
                UpdateLocales(menu, model);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, menu, form));

                NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));
                return continueEditing ? RedirectToAction("Edit", new { id = menu.Id }) : RedirectToAction("List");
            }

            PrepareModel(model, null);

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMenus))
            {
                return AccessDeniedView();
            }

            var menu = _menuStorage.GetMenuById(id, true);
            if (menu == null)
            {
                return HttpNotFound();
            }

            var model = MiniMapper.Map<MenuRecord, MenuRecordModel>(menu);

            PrepareModel(model, menu);

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Title = menu.GetLocalized(x => x.Title, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(MenuRecordModel model, bool continueEditing, FormCollection form)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMenus))
            {
                return AccessDeniedView();
            }

            var menu = _menuStorage.GetMenuById(model.Id, true);
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                MiniMapper.Map(model, menu);

                _menuStorage.UpdateMenu(menu);

                SaveStoreMappings(menu, model);
                SaveAclMappings(menu, model);
                UpdateLocales(menu, model);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, menu, form));

                NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));
                return continueEditing ? RedirectToAction("Edit", menu.Id) : RedirectToAction("List");
            }

            PrepareModel(model, menu);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMenus))
            {
                return AccessDeniedView();
            }

            var menu = _menuStorage.GetMenuById(id);
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (menu.IsSystemMenu)
            {
                NotifyError(T("Admin.ContentManagement.Menus.CannotBeDeleted"));
                return RedirectToAction("Edit", new { id = menu.Id });
            }

            _menuStorage.DeleteMenu(menu);

            NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));
            return RedirectToAction("List");
        }

        #region Menu items

        // Ajax.
        public ActionResult ItemList(int id)
        {
            var menu = _menuStorage.GetMenuById(id, true);
            if (menu == null)
            {
                NotifyError(T("Admin.Common.ResourceNotFound"));
                return new EmptyResult();
            }

            var model = new MenuRecordModel
            {
                Id = id
            };

            model.ItemTree = GetItemTree(menu);

            return PartialView(model);
        }

        // Ajax.
        public ActionResult LinkInputEditor(int id, string provider)
        {
            var html = string.Empty;
            var model = string.Empty;

            var item = _menuStorage.GetMenuItemById(id);
            if (item != null && item.SystemName.IsCaseInsensitiveEqual(provider))
            {
                model = item.Model;
            }

            if (provider.HasValue() && (provider == "entity" || provider == "route"))
            {
                var dataDic = new ViewDataDictionary { TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "Model" } };
                var viewName = $"EditorTemplates/MenuItem.{provider}";
                html = this.RenderPartialViewToString(viewName, model, dataDic);
            }

            if (html.IsEmpty())
            {
                html = "<input type='hidden' name='Model' id='Model' value='{0}' />".FormatInvariant(model.HtmlEncode());
            }

            return Json(new { success = true, html });
        }

        // Do not use model binding because of input validation.
        public ActionResult CreateItem(int menuId, int parentItemId, string btnId)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMenus))
            {
                return AccessDeniedView();
            }

            var menu = _menuStorage.GetMenuById(menuId, true);
            if (menu == null)
            {
                return HttpNotFound();
            }

            var model = new MenuItemRecordModel
            {
                MenuId = menuId,
                ParentItemId = parentItemId,
                Published = true
            };

            PrepareModel(model, null);
            AddLocales(_languageService, model.Locales);

            ViewBag.BtnId = btnId;

            return View(model);
        }

        // Do not name parameter "model" because of property of same name.
        [HttpPost, ParameterBasedOnFormName("save-item-continue", "continueEditing")]
        public ActionResult CreateItem(MenuItemRecordModel itemModel, string btnId, bool continueEditing)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMenus))
            {
                return AccessDeniedView();
            }

            if (ModelState.IsValid)
            {
                var item = MiniMapper.Map<MenuItemRecordModel, MenuItemRecord>(itemModel);
                item.ParentItemId = itemModel.ParentItemId ?? 0;

                _menuStorage.InsertMenuItem(item);
                itemModel.Id = item.Id;

                UpdateLocales(item, itemModel);

                ViewBag.btnId = btnId;
                ViewBag.RefreshPage = true;
                ViewBag.CloseWindow = !continueEditing;

                if (continueEditing)
                {
                    PrepareModel(itemModel, null);
                    NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));
                }
            }
            else
            {
                PrepareModel(itemModel, null);
            }

            return View(itemModel);
        }

        public ActionResult EditItem(int id, string btnId)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMenus))
            {
                return AccessDeniedView();
            }

            var item = _menuStorage.GetMenuItemById(id, true);
            if (item == null)
            {
                return HttpNotFound();
            }

            var model = MiniMapper.Map<MenuItemRecord, MenuItemRecordModel>(item);
            model.ParentItemId = item.ParentItemId == 0 ? (int?)null : item.ParentItemId;

            PrepareModel(model, item);

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Title = item.GetLocalized(x => x.Title, languageId, false, false);
                locale.ShortDescription = item.GetLocalized(x => x.ShortDescription, languageId, false, false);
            });

            ViewBag.BtnId = btnId;

            return View(model);
        }

        // Do not name parameter "model" because of property of same name.
        [HttpPost, ParameterBasedOnFormName("save-item-continue", "continueEditing")]
        public ActionResult EditItem(MenuItemRecordModel itemModel, string btnId, bool continueEditing)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMenus))
            {
                return AccessDeniedView();
            }

            var item = _menuStorage.GetMenuItemById(itemModel.Id, continueEditing);
            if (item == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                MiniMapper.Map(itemModel, item);
                item.ParentItemId = itemModel.ParentItemId ?? 0;

                _menuStorage.UpdateMenuItem(item);

                UpdateLocales(item, itemModel);

                ViewBag.btnId = btnId;
                ViewBag.RefreshPage = true;
                ViewBag.CloseWindow = !continueEditing;

                if (continueEditing)
                {
                    PrepareModel(itemModel, item);
                    NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));
                }
            }
            else
            {
                PrepareModel(itemModel, item);
            }

            return View(itemModel);
        }

        [HttpPost]
        public ActionResult DeleteItem(int id)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMenus))
            {
                return AccessDeniedView();
            }

            var item = _menuStorage.GetMenuItemById(id);
            if (item == null)
            {
                return HttpNotFound();
            }

            var menuId = item.MenuId;
            _menuStorage.DeleteMenuItem(item);

            NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));

            if (Request.IsAjaxRequest())
            {
                return RedirectToAction("ItemList", new { id = menuId });
            }

            return RedirectToAction("Edit", new { id = menuId });
        }

        #endregion

        #region Utilities

        private void PrepareModel(MenuRecordModel model, MenuRecord entity)
        {
            if (entity != null)
            {
                if (ModelState.IsValid)
                {
                    model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(entity);
                    model.SelectedCustomerRoleIds = _aclService.GetCustomerRoleIdsWithAccessTo(entity);
                }

                model.ItemTree = GetItemTree(entity);
            }

            model.AvailableStores = Services.StoreService.GetAllStores().ToSelectListItems(model.SelectedStoreIds);
            model.AvailableCustomerRoles = _customerService.GetAllCustomerRoles(true).ToSelectListItems(model.SelectedCustomerRoleIds);
        }

        private void PrepareModel(MenuItemRecordModel model, MenuItemRecord entity)
        {
            var menu = entity != null
                ? entity.Menu
                : _menuStorage.GetMenuById(model.MenuId, true);

            // Preset max display order to always insert item at the end.
            if (entity == null)
            {
                var item = menu.Items.Where(x => x.ParentItemId == model.ParentItemId)
                    .OrderByDescending(x => x.DisplayOrder)
                    .FirstOrDefault();

                model.DisplayOrder = (item?.DisplayOrder ?? 0) + 1;
            }

            // Create list for selecting parent item.
            var itemTree = GetItemTree(menu);
            itemTree.Traverse(x =>
            {
                if (entity != null && entity.Id == x.Value.EntityId)
                {
                    // Ignore. Element cannot be parent itself.
                }
                else if (x.Value.Text.HasValue())
                {
                    var path = string.Join(" » ", x.Trail.Skip(1).Select(y => y.Value.Text));
                    model.AllItems.Add(new SelectListItem
                    {
                        Text = path,
                        Value = x.Value.EntityId.ToString(),
                        Selected = entity != null && entity.ParentItemId == x.Value.EntityId
                    });
                }
            });

            // Create list of available item providers.
            model.AllProviders = _menuItemProviders
                .Select(x => new SelectListItem
                {
                    Text = T("Admin.ContentManagement.Menus.Provider." + x.Metadata.SystemName),
                    Value = x.Metadata.SystemName,
                    Selected = entity != null ? x.Metadata.SystemName.IsCaseInsensitiveEqual(entity.SystemName) : false
                })
                .ToList();
        }

        private void UpdateLocales(MenuRecord entity, MenuRecordModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(entity, x => x.Title, localized.Title, localized.LanguageId);
            }
        }

        private void UpdateLocales(MenuItemRecord entity, MenuItemRecordModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(entity, x => x.Title, localized.Title, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(entity, x => x.ShortDescription, localized.ShortDescription, localized.LanguageId);
            }
        }

        private TreeNode<MenuItem> GetItemTree(MenuRecord menu)
        {
            var resources = new Dictionary<string, string>
            {
                { "divider", T("Admin.ContentManagement.Menus.Item.IsDivider") },
                { "route", T("Admin.ContentManagement.Menus.Provider.Route") },
                { "catalog", T("Admin.ContentManagement.Menus.Provider.Catalog") },
                { "product", T("Common.Entity.Product") },
                { "category", T("Common.Entity.Category") },
                { "manufacturer", T("Common.Entity.Manufacturer") },
                { "topic", T("Common.Entity.Topic") },
                { "file", T("Common.File") },
                { "url", T("Common.Url") }
            };

            var entities = _menuStorage.SortForTree(menu.Items);
            var root = new TreeNode<MenuItem>(new MenuItem { Text = menu.GetLocalized(x => x.Title) });
            var parent = root;
            MenuItemRecord prevItem = null;

            foreach (var entity in entities)
            {
                // Get parent.
                if (prevItem != null)
                {
                    if (entity.ParentItemId != parent.Value.EntityId)
                    {
                        if (entity.ParentItemId == prevItem.Id)
                        {
                            // Level +1.
                            parent = parent.LastChild;
                        }
                        else
                        {
                            // Level -x.
                            while (!parent.IsRoot)
                            {
                                if (parent.Value.EntityId == entity.ParentItemId)
                                {
                                    break;
                                }
                                parent = parent.Parent;
                            }
                        }
                    }
                }

                var item = new MenuItem
                {
                    EntityId = entity.Id,
                    Text = entity.GetLocalized(x => x.Title),
                    Visible = entity.Published
                };

                #region Icon & title

                if (entity.IsDivider)
                {
                    item.BadgeText = resources["divider"];
                    item.Icon = "fas fa-minus";
                }
                else if (entity.SystemName.IsCaseInsensitiveEqual("route"))
                {
                    item.BadgeText = resources["route"];
                    item.Icon = "fas fa-directions";
                }
                else if (entity.SystemName.IsCaseInsensitiveEqual("catalog"))
                {
                    item.BadgeText = resources["catalog"];
                    item.Icon = "fa fa-sitemap";
                }
                else if (entity.SystemName.IsCaseInsensitiveEqual("entity") && entity.Model.HasValue())
                {
                    if (entity.Model.StartsWith("product:"))
                    {
                        item.BadgeText = resources["product"];
                        item.Icon = "fa fa-cube";
                    }
                    else if (entity.Model.StartsWith("category:"))
                    {
                        item.BadgeText = resources["category"];
                        item.Icon = "fa fa-sitemap";
                    }
                    else if (entity.Model.StartsWith("manufacturer:"))
                    {
                        item.BadgeText = resources["manufacturer"];
                        item.Icon = "far fa-building";
                    }
                    else if (entity.Model.StartsWith("topic:"))
                    {
                        item.BadgeText = resources["topic"];
                        item.Icon = "far fa-file";
                    }
                    else if (entity.Model.StartsWith("file:"))
                    {
                        item.BadgeText = resources["file"];
                        item.Icon = "far fa-folder-open";
                    }
                    else
                    {
                        item.BadgeText = resources["url"];
                        item.Icon = "fa fa-link";
                    }
                }

                #endregion

                parent.Append(item);
                prevItem = entity;
            }

            return root;
        }

        #endregion
    }
}