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
        private readonly AdminAreaSettings _adminAreaSettings;

        public MenuController(
            IMenuStorage menuStorage,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            ICustomerService customerService,
            AdminAreaSettings adminAreaSettings)
        {
            _menuStorage = menuStorage;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
            _customerService = customerService;
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
            PrepareModel(model, null, false);
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

            PrepareModel(model, null, false);

            return View(model);
        }

        public ActionResult Edit(int id)
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

            var model = MiniMapper.Map<MenuRecord, MenuRecordModel>(menu);

            PrepareModel(model, menu, false);

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

            var menu = _menuStorage.GetMenuById(model.Id);
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

            PrepareModel(model, menu, true);

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
            var model = new MenuRecordModel
            {
                Id = id,
                ItemTree = GetItemList(id)
            };

            return PartialView(model);
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
                ParentItemId = parentItemId
            };

            PrepareModel(model, null);
            AddLocales(_languageService, model.Locales);

            // Preset max display order to always insert item at the end.
            var item = menu.Items.Where(x => x.ParentItemId == parentItemId)
                .OrderByDescending(x => x.DisplayOrder)
                .FirstOrDefault();
            if (item != null)
            {
                model.DisplayOrder = item.DisplayOrder + 1;
            }

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

                _menuStorage.InsertMenuItem(item);
                itemModel.Id = item.Id;

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

            return View(itemModel);
        }

        public ActionResult EditItem(int id, string btnId)
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

            var model = MiniMapper.Map<MenuItemRecord, MenuItemRecordModel>(item);

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

            var item = _menuStorage.GetMenuItemById(itemModel.Id);
            if (item == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                MiniMapper.Map(itemModel, item);

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
                var model = GetItemList(menuId);
                return PartialView("ItemList", model);
            }

            return RedirectToAction("Edit", new { id = menuId });
        }

        #endregion

        #region Utilities

        private void PrepareModel(MenuRecordModel model, MenuRecord entity, bool excludeProperties)
        {
            if (!excludeProperties)
            {
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(entity);
                model.SelectedCustomerRoleIds = _aclService.GetCustomerRoleIdsWithAccessTo(entity);
            }

            model.AvailableStores = Services.StoreService.GetAllStores().ToSelectListItems(model.SelectedStoreIds);
            model.AvailableCustomerRoles = _customerService.GetAllCustomerRoles(true).ToSelectListItems(model.SelectedCustomerRoleIds);

            if (entity != null)
            {
                model.ItemTree = GetItemList(entity.Id);
            }
        }

        private void PrepareModel(MenuItemRecordModel model, MenuItemRecord entity)
        {
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

        private TreeNode<MenuItem> GetItemList(int id)
        {
            return new TreeNode<MenuItem>(new MenuItem { Text = "Root" }, new List<TreeNode<MenuItem>>
            {
                new TreeNode<MenuItem>(new MenuItem { Text = "Item 1", EntityId = 1 }),
                new TreeNode<MenuItem>(new MenuItem { Text = "Item 2", EntityId = 3 }),
                new TreeNode<MenuItem>(new MenuItem { Text = "Item 3", EntityId = 4 }, new List<TreeNode<MenuItem>>
                {
                    new TreeNode<MenuItem>(new MenuItem { Text = "Sub 1", EntityId = 7 }),
                    new TreeNode<MenuItem>(new MenuItem { Text = "Sub 2", EntityId = 8, Visible = false }),
                    new TreeNode<MenuItem>(new MenuItem { Text = "Sub 3", EntityId = 10 }, new List<TreeNode<MenuItem>>
                    {
                        new TreeNode<MenuItem>(new MenuItem { Text = "Sub 1", EntityId = 13 }),
                        new TreeNode<MenuItem>(new MenuItem { Text = "Sub 2", EntityId = 15 }),
                        new TreeNode<MenuItem>(new MenuItem { Text = "Sub 3", EntityId = 16 }, new List<TreeNode<MenuItem>>
                        {
                            new TreeNode<MenuItem>(new MenuItem { Text = "Sub 1", EntityId = 17 }),
                            new TreeNode<MenuItem>(new MenuItem { Text = "Sub 2", EntityId = 9 })
                        }),
                        new TreeNode<MenuItem>(new MenuItem { Text = "Sub 4", EntityId = 11 }),
                        new TreeNode<MenuItem>(new MenuItem { EntityId = 21, Attributes = new Dictionary<string, object> { { "IsDivider", true } } }),
                        new TreeNode<MenuItem>(new MenuItem { Text = "Sub 5", EntityId = 12 }),
                        new TreeNode<MenuItem>(new MenuItem { Text = "Sub 6", EntityId = 18 })
                    })
                }),
                new TreeNode<MenuItem>(new MenuItem { Text = "Item 4", EntityId = 19 }),
                new TreeNode<MenuItem>(new MenuItem { Text = "Item 5", EntityId = 20 })
            });
        }

        #endregion
    }
}