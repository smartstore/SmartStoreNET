using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;

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

            var oldSystemName = menu.SystemName;

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
            bool withItems = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = BuildMenuQuery(systemName, storeId, includeHidden, withItems);
            return new PagedList<MenuRecord>(query, pageIndex, pageSize);
        }

        public virtual MenuRecord GetMenuById(int id, bool withItems = false)
        {
            if (id == 0)
            {
                return null;
            }

            var query = _menuRepository.Table;

            if (withItems)
            {
                query = query.Include(x => x.Items);
            }

            return query.FirstOrDefault(x => x.Id == id);
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

        public virtual MenuItemRecord GetMenuItemById(int id, bool withMenu = false)
        {
            if (id == 0)
            {
                return null;
            }

            var query = _menuItemRepository.Table;

            if (withMenu)
            {
                query = query.Include(x => x.Menu.Items);
            }

            return query.FirstOrDefault(x => x.Id == id);
        }

        public virtual IList<MenuItemRecord> SortForTree(IEnumerable<MenuItemRecord> items, bool includeItemsWithoutExistingParent = true)
        {
            Guard.NotNull(items, nameof(items));

            var result = new List<MenuItemRecord>();

            var entities = items
                .OrderBy(x => x.ParentItemId)
                .ThenBy(x => x.DisplayOrder)
                .ThenBy(x => x.Title)
                .ToArray();

            SortChildItems(0);

            if (includeItemsWithoutExistingParent && result.Count != entities.Length)
            {
                foreach (var entity in entities)
                {
                    if (result.FirstOrDefault(x => x.Id == entity.Id) == null)
                    {
                        result.Add(entity);
                    }
                }
            }

            return result;

            void SortChildItems(int parentItemId)
            {
                var childItems = entities.Where(x => x.ParentItemId == parentItemId).ToArray();
                foreach (var item in childItems)
                {
                    result.Add(item);
                    SortChildItems(item.Id);
                }
            }
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

		protected virtual IQueryable<MenuRecord> BuildMenuQuery(string systemName, int storeId, bool includeHidden, bool withItems)
        {
            var applied = false;
            var entityName = nameof(MenuRecord);
            var query = _menuRepository.Table;

            if (withItems)
            {
                query = query.Include(x => x.Items);
            }

            query = query.Where(x => includeHidden || x.Published);

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

            if (applied)
            {
                query = 
                    from x in query
                    group x by x.Id into grp
                    orderby grp.Key
                    select grp.FirstOrDefault();
            }

            query = query.OrderBy(x => x.SystemName).ThenBy(x => x.Title);

            return query;
        }

		#endregion
	}
}
