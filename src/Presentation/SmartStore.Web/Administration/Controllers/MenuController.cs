using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Menus;
using SmartStore.ComponentModel;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Security;
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
        private readonly IDictionary<string, Lazy<IMenuItemProvider, MenuItemProviderMetadata>> _menuItemProviders;
        private readonly AdminAreaSettings _adminAreaSettings;

        public MenuController(
            IMenuStorage menuStorage,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            IEnumerable<Lazy<IMenuItemProvider,
            MenuItemProviderMetadata>> menuItemProviders,
            AdminAreaSettings adminAreaSettings)
        {
            _menuStorage = menuStorage;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
            _menuItemProviders = menuItemProviders.ToDictionarySafe(x => x.Metadata.ProviderName, x => x);
            _adminAreaSettings = adminAreaSettings;
        }

        #region Menu

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Cms.Menu.Read)]
        public ActionResult List()
        {
            var model = new MenuRecordListModel
            {
                GridPageSize = _adminAreaSettings.GridPageSize,
                AvailableStores = Services.StoreService.GetAllStores().ToSelectListItems()
            };

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.Menu.Read)]
        public ActionResult List(GridCommand command, MenuRecordListModel model)
        {
            var gridModel = new GridModel<MenuRecordModel>();

            var menus = _menuStorage.GetAllMenus(model.SystemName, model.StoreId, true, command.Page - 1, command.PageSize);

            gridModel.Data = menus.Select(x =>
            {
                var itemModel = new MenuRecordModel();
                MiniMapper.Map(x, itemModel);

                return itemModel;
            });

            gridModel.Total = menus.TotalCount;

            return new JsonResult
            {
                MaxJsonLength = int.MaxValue,
                Data = gridModel
            };
        }

        [Permission(Permissions.Cms.Menu.Create)]
        public ActionResult Create()
        {
            var model = new MenuRecordModel();
            PrepareModel(model, null);
            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost, ValidateInput(false), ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Menu.Create)]
        public ActionResult Create(MenuRecordModel model, bool continueEditing, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var menu = MiniMapper.Map<MenuRecordModel, MenuRecord>(model);
                menu.WidgetZone = string.Join(",", model.WidgetZone ?? new string[0]).NullEmpty();

                _menuStorage.InsertMenu(menu);

                SaveStoreMappings(menu, model.SelectedStoreIds);
                SaveAclMappings(menu, model.SelectedCustomerRoleIds);
                UpdateLocales(menu, model);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, menu, form));

                NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));
                return continueEditing ? RedirectToAction("Edit", new { id = menu.Id }) : RedirectToAction("List");
            }

            PrepareModel(model, null);

            return View(model);
        }

        [Permission(Permissions.Cms.Menu.Read)]
        public ActionResult Edit(int id)
        {
            var menu = _menuStorage.GetMenuById(id);
            if (menu == null)
            {
                return HttpNotFound();
            }

            var model = MiniMapper.Map<MenuRecord, MenuRecordModel>(menu);
            model.WidgetZone = menu.WidgetZone.SplitSafe(",");

            PrepareModel(model, menu);
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Title = menu.GetLocalized(x => x.Title, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ValidateInput(false), ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Menu.Update)]
        public ActionResult Edit(MenuRecordModel model, bool continueEditing, FormCollection form)
        {
            var menu = _menuStorage.GetMenuById(model.Id);
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                MiniMapper.Map(model, menu);
                menu.WidgetZone = string.Join(",", model.WidgetZone ?? new string[0]).NullEmpty();

                _menuStorage.UpdateMenu(menu);

                SaveStoreMappings(menu, model.SelectedStoreIds);
                SaveAclMappings(menu, model.SelectedCustomerRoleIds);
                UpdateLocales(menu, model);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, menu, form));

                NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));
                return continueEditing ? RedirectToAction("Edit", menu.Id) : RedirectToAction("List");
            }

            PrepareModel(model, menu);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Menu.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
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

        #endregion

        #region Menu items

        // Ajax.
        [Permission(Permissions.Cms.Menu.Read)]
        public ActionResult ItemList(int id)
        {
            var model = new MenuRecordModel { Id = id };
            PrepareModel(model, null);

            return PartialView(model);
        }

        // Do not use model binding because of input validation.
        [Permission(Permissions.Cms.Menu.Update)]
        public ActionResult CreateItem(string providerName, int menuId, int parentItemId)
        {
            var menu = _menuStorage.GetMenuById(menuId);
            if (menu == null)
            {
                return HttpNotFound();
            }

            var model = new MenuItemRecordModel
            {
                ProviderName = providerName,
                MenuId = menuId,
                ParentItemId = parentItemId,
                Published = true
            };

            PrepareModel(model, null);
            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        // Do not name parameter "model" because of property of same name.
        [HttpPost, ValidateInput(false), ParameterBasedOnFormName("save-item-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Menu.Update)]
        public ActionResult CreateItem(MenuItemRecordModel itemModel, bool continueEditing, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                itemModel.ParentItemId = itemModel.ParentItemId ?? 0;
                var item = MiniMapper.Map<MenuItemRecordModel, MenuItemRecord>(itemModel);
                item.PermissionNames = string.Join(",", itemModel.PermissionNames ?? new string[0]).NullEmpty();

                _menuStorage.InsertMenuItem(item);

                SaveStoreMappings(item, itemModel.SelectedStoreIds);
                SaveAclMappings(item, itemModel.SelectedCustomerRoleIds);
                UpdateLocales(item, itemModel);

                Services.EventPublisher.Publish(new ModelBoundEvent(itemModel, item, form));
                NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

                if (continueEditing)
                {
                    return RedirectToAction("EditItem", new { id = item.Id });
                }

                return RedirectToAction("Edit", new { id = item.MenuId });
            }

            PrepareModel(itemModel, null);

            return View(itemModel);
        }

        [Permission(Permissions.Cms.Menu.Read)]
        public ActionResult EditItem(int id)
        {
            var item = _menuStorage.GetMenuItemById(id);
            if (item == null)
            {
                return HttpNotFound();
            }

            var model = MiniMapper.Map<MenuItemRecord, MenuItemRecordModel>(item);
            model.ParentItemId = item.ParentItemId == 0 ? (int?)null : item.ParentItemId;
            model.PermissionNames = item.PermissionNames.SplitSafe(",");

            PrepareModel(model, item);
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Title = item.GetLocalized(x => x.Title, languageId, false, false);
                locale.ShortDescription = item.GetLocalized(x => x.ShortDescription, languageId, false, false);
            });

            return View(model);
        }

        // Do not name parameter "model" because of property of same name.
        [HttpPost, ValidateInput(false), ParameterBasedOnFormName("save-item-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Menu.Update)]
        public ActionResult EditItem(MenuItemRecordModel itemModel, bool continueEditing, FormCollection form)
        {
            var item = _menuStorage.GetMenuItemById(itemModel.Id);
            if (item == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                itemModel.ParentItemId = itemModel.ParentItemId ?? 0;
                MiniMapper.Map(itemModel, item);
                item.PermissionNames = string.Join(",", itemModel.PermissionNames ?? new string[0]).NullEmpty();

                _menuStorage.UpdateMenuItem(item);

                SaveStoreMappings(item, itemModel.SelectedStoreIds);
                SaveAclMappings(item, itemModel.SelectedCustomerRoleIds);
                UpdateLocales(item, itemModel);

                Services.EventPublisher.Publish(new ModelBoundEvent(itemModel, item, form));
                NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

                if (continueEditing)
                {
                    return RedirectToAction("EditItem", new { id = item.Id });
                }

                return RedirectToAction("Edit", new { id = item.MenuId });
            }

            PrepareModel(itemModel, item);

            return View(itemModel);
        }

        // Ajax.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Menu.Update)]
        public ActionResult MoveItem(int menuId, int sourceId, string direction)
        {
            if (menuId == 0 || sourceId == 0 || direction.IsEmpty())
            {
                return new EmptyResult();
            }

            using (var scope = new DbContextScope(ctx: Services.DbContext, autoCommit: false))
            {
                var allItems = _menuStorage.GetMenuItems(menuId, 0, true).ToDictionary(x => x.Id, x => x);
                var sourceItem = allItems[sourceId];

                var siblings = allItems.Select(x => x.Value)
                    .Where(x => x.ParentItemId == sourceItem.ParentItemId)
                    .OrderBy(x => x.DisplayOrder)
                    .ToList();

                var index = siblings.IndexOf(sourceItem) + (direction == "up" ? -1 : 1);
                if (index >= 0 && index < siblings.Count)
                {
                    var targetItem = siblings[index];

                    // Ensure unique display order starting from 1.
                    var count = 0;
                    siblings.Each(x => x.DisplayOrder = ++count);

                    // Swap display order of source and target item.
                    var tmp = sourceItem.DisplayOrder;
                    sourceItem.DisplayOrder = targetItem.DisplayOrder;
                    targetItem.DisplayOrder = tmp;

                    scope.Commit();
                }
            }

            return RedirectToAction("ItemList", new { id = menuId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Menu.Delete)]
        public ActionResult DeleteItem(int id)
        {
            var isAjax = Request.IsAjaxRequest();

            var item = _menuStorage.GetMenuItemById(id);
            if (item == null)
            {
                if (isAjax)
                {
                    return new EmptyResult();
                }

                return HttpNotFound();
            }

            var menuId = item.MenuId;
            _menuStorage.DeleteMenuItem(item);

            NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));

            return RedirectToAction(isAjax ? "ItemList" : "Edit", new { id = menuId });
        }

        #endregion

        #region Utilities

        private void PrepareModel(MenuRecordModel model, MenuRecord entity)
        {
            var templateNames = new string[] { "LinkList", "ListGroup", "Dropdown", "Navbar" };

            if (entity != null && ModelState.IsValid)
            {
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(entity);
                model.SelectedCustomerRoleIds = _aclService.GetCustomerRoleIdsWithAccessTo(entity);

                model.IsCustomTemplate = entity.Template.HasValue() && !templateNames.Contains(entity.Template);
            }

            model.Locales = new List<MenuRecordLocalizedModel>();

            model.AllTemplates = templateNames
                .Select(x => new SelectListItem { Text = x, Value = x, Selected = x.IsCaseInsensitiveEqual(entity?.Template) })
                .ToList();

            model.AllProviders = _menuItemProviders.Values
                .Select(x => new SelectListItem
                {
                    Text = T("Providers.MenuItems.FriendlyName." + x.Metadata.ProviderName),
                    Value = x.Metadata.ProviderName
                })
                .ToList();

            var entities = _menuStorage.GetMenuItems(model.Id, 0, true);
            model.ItemTree = entities.GetTree("EditMenu", _menuItemProviders);
        }

        private void PrepareModel(MenuItemRecordModel model, MenuItemRecord entity)
        {
            Lazy<IMenuItemProvider, MenuItemProviderMetadata> provider = null;
            var entities = _menuStorage.GetMenuItems(model.MenuId, 0, true).ToDictionary(x => x.Id, x => x);

            model.Locales = new List<MenuItemRecordLocalizedModel>();
            model.AllItems = new List<SelectListItem>();

            if (entity != null && ModelState.IsValid)
            {
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(entity);
                model.SelectedCustomerRoleIds = _aclService.GetCustomerRoleIdsWithAccessTo(entity);
            }

            if (_menuItemProviders.TryGetValue(model.ProviderName, out provider))
            {
                model.ProviderAppendsMultipleItems = provider.Metadata.AppendsMultipleItems;
            }

            // Preset max display order to always insert item at the end.
            if (entity == null && entities.Any())
            {
                var item = entities
                    .Select(x => x.Value)
                    .Where(x => x.ParentItemId == model.ParentItemId)
                    .OrderByDescending(x => x.DisplayOrder)
                    .FirstOrDefault();

                model.DisplayOrder = (item?.DisplayOrder ?? 0) + 1;
            }

            // Create list for selecting parent item.
            var tree = entities.Values.GetTree("EditMenu", _menuItemProviders);

            tree.Traverse(x =>
            {
                if (entity != null && entity.Id == x.Value.EntityId)
                {
                    // Ignore. Element cannot be parent itself.
                    model.TitlePlaceholder = x.Value.Text;
                }
                else if (entities.TryGetValue(x.Value.EntityId, out var record) &&
                    _menuItemProviders.TryGetValue(record.ProviderName, out provider) &&
                    provider.Metadata.AppendsMultipleItems)
                {
                    // Ignore. Element cannot have child nodes.
                }
                else if (!x.Value.IsGroupHeader)
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
        }

        private void UpdateLocales(MenuRecord entity, MenuRecordModel model)
        {
            if (model.Locales != null)
            {
                foreach (var localized in model.Locales)
                {
                    _localizedEntityService.SaveLocalizedValue(entity, x => x.Title, localized.Title, localized.LanguageId);
                }
            }
        }

        private void UpdateLocales(MenuItemRecord entity, MenuItemRecordModel model)
        {
            if (model.Locales != null)
            {
                foreach (var localized in model.Locales)
                {
                    _localizedEntityService.SaveLocalizedValue(entity, x => x.Title, localized.Title, localized.LanguageId);
                    _localizedEntityService.SaveLocalizedValue(entity, x => x.ShortDescription, localized.ShortDescription, localized.LanguageId);
                }
            }
        }

        #endregion
    }
}