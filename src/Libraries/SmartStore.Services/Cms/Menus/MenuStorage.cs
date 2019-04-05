using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;
using SmartStore.Data.Caching;

namespace SmartStore.Services.Cms
{
    public partial class MenuStorage : IMenuStorage
    {
        private const string MENU_SYSTEMNAME_CACHE_KEY = "MenuStorage:SystemNames";

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
                if (modProps.TryGetValue(nameof(menu.Published), out var original) && original.Convert<bool>() == true)
                {
                    systemNames.Remove(menu.SystemName);
                }
                else if (modProps.TryGetValue(nameof(menu.SystemName), out original))
                {
                    systemNames.Remove((string)original);
                    systemNames.Add(menu.SystemName);
                }
            }
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
        }

        public virtual IPagedList<MenuRecord> GetAllMenus(
            string systemName = null,
            int storeId = 0,
            bool includeHidden = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = BuildMenuQuery(0, systemName, storeId, includeHidden);
            return new PagedList<MenuRecord>(query, pageIndex, pageSize);
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
                return;
            }

            var ids = new HashSet<int> { item.Id };
            GetChildIds(item.Id);

            foreach (var chunk in ids.Slice(200))
            {
                var items = _menuItemRepository.Table.Where(x => chunk.Contains(x.Id)).ToList();

                _menuItemRepository.DeleteRange(items);
            }

            void GetChildIds(int parentId)
            {
                var childIds = _menuItemRepository.TableUntracked
                    .Where(x => x.ParentItemId == parentId)
                    .Select(x => x.Id)
                    .ToList();

                if (childIds.Any())
                {
                    ids.AddRange(childIds);

                    childIds.Each(x => GetChildIds(x));
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

        private ISet GetMenuSystemNames(bool create)
		{
			if (create || _services.Cache.Contains(MENU_SYSTEMNAME_CACHE_KEY))
			{
				return _services.Cache.GetHashSet(MENU_SYSTEMNAME_CACHE_KEY, () =>
				{
					return _menuRepository.TableUntracked
						.Where(x => x.Published)
						.Select(x => x.SystemName)
						.ToArray();
				});
			}

			return null;
		}

        protected virtual IQueryable<MenuRecord> BuildMenuQuery(
            int id,
            string systemName,
            int storeId,
            bool includeHidden,
            bool groupBy = true,
            bool sort = true)
        {
            var applied = false;
            var entityName = nameof(MenuRecord);
            var query = _menuRepository.Table.Where(x => includeHidden || x.Published);

            if (id != 0)
            {
                query = query.Where(x => x.Id == id);
            }

            if (systemName.HasValue())
            {
                query = query.Where(x => x.SystemName == systemName);
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
                var allowedRoleIds = _services.WorkContext.CurrentCustomer.CustomerRoles.Where(x => x.Active).Select(x => x.Id).ToList();

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
                query = query.OrderBy(x => x.SystemName).ThenBy(x => x.Title);
            }

            return query;
        }

        protected virtual IQueryable<MenuItemRecord> BuildMenuItemQuery(
            int menuId,
            string systemName,
            int storeId,
            bool includeHidden)
        {
            var singleMenu = menuId != 0 || (systemName.HasValue() && storeId != 0);
            var menuQuery = BuildMenuQuery(menuId, systemName, storeId, includeHidden, !singleMenu, !singleMenu);

            if (singleMenu)
            {
                menuQuery = menuQuery.Take(1);
            }

            var query =
                from m in menuQuery
                join mi in _menuItemRepository.Table on m.Id equals mi.MenuId
                orderby mi.ParentItemId, mi.DisplayOrder
                select mi;

            return query;
        }

        #endregion
    }
}
