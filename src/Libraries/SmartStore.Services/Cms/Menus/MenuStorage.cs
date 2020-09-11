using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;
using SmartStore.Data.Caching;

namespace SmartStore.Services.Cms
{
    public partial class MenuStorage : IMenuStorage
    {
        private const string MENU_ALLSYSTEMNAMES_CACHE_KEY = "MenuStorage:SystemNames";
        private const string MENU_USER_CACHE_KEY = "MenuStorage:Menus:User-{0}-{1}";
        private const string MENU_PATTERN_KEY = "MenuStorage:Menus:*";

        private readonly IRepository<MenuRecord> _menuRepository;
        private readonly IRepository<MenuItemRecord> _menuItemRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly ICommonServices _services;

        public MenuStorage(
            IRepository<MenuRecord> menuRepository,
            IRepository<MenuItemRecord> menuItemRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IRepository<AclRecord> aclRepository,
            ICommonServices services)
        {
            _menuRepository = menuRepository;
            _menuItemRepository = menuItemRepository;
            _storeMappingRepository = storeMappingRepository;
            _aclRepository = aclRepository;
            _services = services;

            QuerySettings = DbQuerySettings.Default;
        }

        public DbQuerySettings QuerySettings { get; set; }

        public virtual void InsertMenu(MenuRecord menu)
        {
            Guard.NotNull(menu, nameof(MenuRecord));

            menu.SystemName = menu.SystemName.ToValidPath();

            _menuRepository.Insert(menu);

            var systemNames = GetMenuSystemNames(false);
            if (systemNames != null && menu.Published)
            {
                systemNames.Add(menu.SystemName);
            }

            _services.Cache.RemoveByPattern(MENU_PATTERN_KEY);
        }

        public virtual void UpdateMenu(MenuRecord menu)
        {
            Guard.NotNull(menu, nameof(MenuRecord));

            menu.SystemName = menu.SystemName.ToValidPath();

            var modProps = _services.DbContext.GetModifiedProperties(menu);

            _menuRepository.Update(menu);

            var systemNames = GetMenuSystemNames(false);
            if (systemNames != null)
            {
                if (modProps.TryGetValue(nameof(menu.Published), out var original))
                {
                    if (original.Convert<bool>() == true)
                    {
                        systemNames.Remove(menu.SystemName);
                    }
                    else
                    {
                        systemNames.Add(menu.SystemName);
                    }
                }
                else if (modProps.TryGetValue(nameof(menu.SystemName), out original))
                {
                    systemNames.Remove((string)original);
                    systemNames.Add(menu.SystemName);
                }
            }

            _services.Cache.RemoveByPattern(MENU_PATTERN_KEY);
        }

        public virtual void DeleteMenu(MenuRecord menu)
        {
            if (menu == null)
                return;

            _menuRepository.Delete(menu);

            var systemNames = GetMenuSystemNames(false);
            if (systemNames != null)
            {
                systemNames.Remove(menu.SystemName);
            }

            _services.Cache.RemoveByPattern(MENU_PATTERN_KEY);
        }

        public virtual IEnumerable<string> GetAllMenuSystemNames()
        {
            return GetMenuSystemNames(true);
        }

        public virtual IPagedList<MenuRecord> GetAllMenus(
            string systemName = null,
            int storeId = 0,
            bool includeHidden = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = BuildMenuQuery(0, storeId, systemName, null, includeHidden);
            return new PagedList<MenuRecord>(query, pageIndex, pageSize);
        }

        public virtual IEnumerable<MenuInfo> GetUserMenuInfos(IEnumerable<CustomerRole> roles = null, int storeId = 0)
        {
            if (roles == null)
            {
                roles = _services.WorkContext.CurrentCustomer.CustomerRoleMappings.Select(x => x.CustomerRole);
            }

            if (storeId == 0)
            {
                storeId = _services.StoreContext.CurrentStore.Id;
            }

            var cacheKey = MENU_USER_CACHE_KEY.FormatInvariant(storeId, string.Join(",", roles.Where(x => x.Active).Select(x => x.Id)));

            var userMenusInfo = _services.Cache.Get(cacheKey, () =>
            {
                var query = BuildMenuQuery(0, storeId, null, false, false, true, true, true);

                var data = query.Select(x => new
                {
                    x.Id,
                    x.SystemName,
                    x.Template,
                    x.WidgetZone,
                    x.DisplayOrder
                })
                .ToList();

                var result = data.Select(x => new MenuInfo
                {
                    Id = x.Id,
                    SystemName = x.SystemName,
                    Template = x.Template,
                    DisplayOrder = x.DisplayOrder,
                    WidgetZones = x.WidgetZone.EmptyNull().Trim()
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(y => y.Trim())
                        .ToArray()
                })
                .ToList();

                return result;
            });

            return userMenusInfo;
        }

        public virtual MenuRecord GetMenuById(int id)
        {
            if (id == 0)
            {
                return null;
            }

            return _menuRepository.GetByIdCached(id, "db.menurecord.id-" + id);
        }

        public virtual bool MenuExists(string systemName)
        {
            if (systemName.IsEmpty())
            {
                return false;
            }

            return GetMenuSystemNames(true).Contains(systemName);
        }

        #region Menu items

        public virtual void InsertMenuItem(MenuItemRecord item)
        {
            Guard.NotNull(item, nameof(MenuRecord));

            // Prevent inconsistent tree structure.
            if (item.ParentItemId != 0 && item.ParentItemId == item.Id)
            {
                item.ParentItemId = 0;
            }

            _menuItemRepository.Insert(item);

            _services.Cache.RemoveByPattern(MENU_PATTERN_KEY);
        }

        public virtual void UpdateMenuItem(MenuItemRecord item)
        {
            Guard.NotNull(item, nameof(MenuRecord));

            // Prevent inconsistent tree structure.
            if (item.ParentItemId != 0 && item.ParentItemId == item.Id)
            {
                item.ParentItemId = 0;
            }

            _menuItemRepository.Update(item);

            _services.Cache.RemoveByPattern(MENU_PATTERN_KEY);
        }

        public virtual void DeleteMenuItem(MenuItemRecord item, bool deleteChilds = true)
        {
            if (item == null)
            {
                return;
            }

            if (!deleteChilds)
            {
                _menuItemRepository.Delete(item);
            }
            else
            {
                var ids = new HashSet<int> { item.Id };
                GetChildIds(item.Id, ids);

                foreach (var chunk in ids.Slice(200))
                {
                    var items = _menuItemRepository.Table.Where(x => chunk.Contains(x.Id)).ToList();
                    _menuItemRepository.DeleteRange(items);
                }
            }

            _services.Cache.RemoveByPattern(MENU_PATTERN_KEY);

            void GetChildIds(int parentId, HashSet<int> ids)
            {
                var childIds = _menuItemRepository.TableUntracked
                    .Where(x => x.ParentItemId == parentId)
                    .Select(x => x.Id)
                    .ToList();

                if (childIds.Any())
                {
                    ids.AddRange(childIds);
                    childIds.Each(x => GetChildIds(x, ids));
                }
            }
        }

        public virtual MenuItemRecord GetMenuItemById(int id)
        {
            if (id == 0)
            {
                return null;
            }

            return _menuItemRepository.GetByIdCached(id, "db.menuitemrecord.id-" + id);
        }

        public virtual IList<MenuItemRecord> GetMenuItems(int menuId, int storeId = 0, bool includeHidden = false)
        {
            if (menuId == 0)
            {
                return new List<MenuItemRecord>();
            }

            var query = BuildMenuItemQuery(menuId, null, storeId, includeHidden);
            return query.ToList();
        }

        public virtual IList<MenuItemRecord> GetMenuItems(string systemName, int storeId = 0, bool includeHidden = false)
        {
            if (systemName.IsEmpty())
            {
                return new List<MenuItemRecord>();
            }

            var query = BuildMenuItemQuery(0, systemName, storeId, includeHidden);
            return query.ToList();
        }

        #endregion

        #region Utilities

        private ISet GetMenuSystemNames(bool ensureCreated)
        {
            if (ensureCreated || _services.Cache.Contains(MENU_ALLSYSTEMNAMES_CACHE_KEY))
            {
                return _services.Cache.GetHashSet(MENU_ALLSYSTEMNAMES_CACHE_KEY, () =>
                {
                    return _menuRepository.TableUntracked
                        .Where(x => x.Published)
                        .OrderByDescending(x => x.IsSystemMenu)
                        .ThenBy(x => x.Id)
                        .Select(x => x.SystemName)
                        .ToArray();
                });
            }

            return null;
        }

        protected virtual IQueryable<MenuRecord> BuildMenuQuery(
            int id,
            int storeId,
            string systemName,
            bool? isSystemMenu,
            bool includeHidden,
            bool groupBy = true,
            bool sort = true,
            bool untracked = false)
        {
            var applied = false;
            var entityName = nameof(MenuRecord);
            var query = untracked ? _menuRepository.TableUntracked : _menuRepository.Table;

            query = query.Where(x => includeHidden || x.Published);

            if (id != 0)
            {
                query = query.Where(x => x.Id == id);
            }

            if (systemName.HasValue())
            {
                query = query.Where(x => x.SystemName == systemName);
            }

            if (isSystemMenu.HasValue)
            {
                query = query.Where(x => x.IsSystemMenu == isSystemMenu.Value);
            }

            if (storeId > 0 && !QuerySettings.IgnoreMultiStore)
            {
                query =
                    from x in query
                    join m in _storeMappingRepository.Table
                    on new { x1 = x.Id, x2 = entityName } equals new { x1 = m.EntityId, x2 = m.EntityName } into sm
                    from m in sm.DefaultIfEmpty()
                    where !x.LimitedToStores || storeId == m.StoreId
                    select x;

                applied = true;
            }

            if (!includeHidden && !QuerySettings.IgnoreAcl)
            {
                var allowedRoleIds = _services.WorkContext.CurrentCustomer.GetRoleIds();

                query =
                    from x in query
                    join a in _aclRepository.Table
                    on new { x1 = x.Id, x2 = entityName } equals new { x1 = a.EntityId, x2 = a.EntityName } into ac
                    from a in ac.DefaultIfEmpty()
                    where !x.SubjectToAcl || allowedRoleIds.Contains(a.CustomerRoleId)
                    select x;

                applied = true;
            }

            if (applied && groupBy)
            {
                query =
                    from x in query
                    group x by x.Id into grp
                    orderby grp.Key
                    select grp.FirstOrDefault();
            }

            if (sort)
            {
                query = query
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.SystemName)
                    .ThenBy(x => x.Title);
            }

            return query;
        }

        protected virtual IQueryable<MenuItemRecord> BuildMenuItemQuery(
            int menuId,
            string systemName,
            int storeId,
            bool includeHidden)
        {
            var applied = false;
            var entityName = nameof(MenuItemRecord);
            var singleMenu = menuId != 0 || (systemName.HasValue() && storeId != 0);
            var menuQuery = BuildMenuQuery(menuId, storeId, systemName, null, includeHidden, !singleMenu, !singleMenu);

            if (singleMenu)
            {
                menuQuery = menuQuery.Take(1);
            }

            var query =
                from m in menuQuery
                join mi in _menuItemRepository.Table on m.Id equals mi.MenuId
                where includeHidden || mi.Published
                orderby mi.ParentItemId, mi.DisplayOrder
                select mi;

            if (storeId > 0 && !QuerySettings.IgnoreMultiStore)
            {
                query =
                    from x in query
                    join m in _storeMappingRepository.Table
                    on new { x1 = x.Id, x2 = entityName } equals new { x1 = m.EntityId, x2 = m.EntityName } into sm
                    from m in sm.DefaultIfEmpty()
                    where !x.LimitedToStores || storeId == m.StoreId
                    select x;

                applied = true;
            }

            if (!includeHidden && !QuerySettings.IgnoreAcl)
            {
                var allowedRoleIds = _services.WorkContext.CurrentCustomer.GetRoleIds();

                query =
                    from x in query
                    join a in _aclRepository.Table
                    on new { x1 = x.Id, x2 = entityName } equals new { x1 = a.EntityId, x2 = a.EntityName } into ac
                    from a in ac.DefaultIfEmpty()
                    where !x.SubjectToAcl || allowedRoleIds.Contains(a.CustomerRoleId)
                    select x;

                applied = true;
            }

            if (applied)
            {
                query =
                    from x in query
                    group x by x.Id into grp
                    orderby grp.Key
                    select grp.FirstOrDefault();
            }

            return query;
        }

        #endregion
    }
}
