using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;
using SmartStore.Data.Caching;

namespace SmartStore.Services.Cms
{
    public partial class MenuService : IMenuService
    {
        private readonly IRepository<Menu> _menuRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly ICommonServices _services;

        public MenuService(
            IRepository<Menu> menuRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IRepository<AclRecord> aclRepository,
            ICommonServices services)
        {
            _menuRepository = menuRepository;
            _storeMappingRepository = storeMappingRepository;
            _aclRepository = aclRepository;
            _services = services;

            QuerySettings = DbQuerySettings.Default;
        }

        public DbQuerySettings QuerySettings { get; set; }

        public virtual void InsertMenu(Menu menu)
        {
            Guard.NotNull(menu, nameof(Menu));

            _menuRepository.Insert(menu);
        }

        public virtual void UpdateMenu(Menu menu)
        {
            Guard.NotNull(menu, nameof(Menu));

            _menuRepository.Update(menu);
        }

        public virtual void DeleteMenu(Menu menu)
        {
            if (menu != null)
            {
                _menuRepository.Delete(menu);
            }
        }

        public virtual IPagedList<Menu> GetAllMenus(
            string systemName = null,
            int storeId = 0,
            bool showHidden = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = BuildMenuQuery(systemName, storeId, showHidden);
            return new PagedList<Menu>(query, pageIndex, pageSize);
        }

        public virtual Menu GetMenuBySystemName(string systemName, int storeId = 0, bool checkPermission = true)
        {
            if (systemName.IsEmpty())
            {
                return null;
            }

            var query = BuildMenuQuery(systemName, storeId, !checkPermission);
            var rolesIdent = checkPermission
                ? "0"
                : _services.WorkContext.CurrentCustomer.GetRolesIdent();

            var result = query.FirstOrDefaultCached("db.manu.bysysname-{0}-{1}-{2}".FormatInvariant(systemName, storeId, rolesIdent));
            return result;
        }

        protected virtual IQueryable<Menu> BuildMenuQuery(string systemName, int storeId, bool showHidden)
        {
            var applied = false;
            var entityName = nameof(Menu);
            var query = _menuRepository.Table.Where(x => showHidden || x.Published);

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

            if (!showHidden && !QuerySettings.IgnoreAcl)
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
    }
}
