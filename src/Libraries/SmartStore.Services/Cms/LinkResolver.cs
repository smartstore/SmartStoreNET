using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Topics;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Cms
{
    public partial class LinkResolver : ILinkResolver
    {
        private const string LINKRESOLVER_KEY = "SmartStore.linkresolver-{0}-{1}";

        protected readonly ICommonServices _services;
        protected readonly IUrlRecordService _urlRecordService;
        protected readonly ILocalizedEntityService _localizedEntityService;
        protected readonly IAclService _aclService;
        protected readonly IStoreMappingService _storeMappingService;
        protected readonly UrlHelper _urlHelper;

        public LinkResolver(
            ICommonServices services,
            IUrlRecordService urlRecordService,
            ILocalizedEntityService localizedEntityService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            UrlHelper urlHelper)
        {
            _services = services;
            _urlRecordService = urlRecordService;
            _localizedEntityService = localizedEntityService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _urlHelper = urlHelper;

            QuerySettings = DbQuerySettings.Default;
        }

        public DbQuerySettings QuerySettings { get; set; }

        public virtual LinkResolverResult Resolve(string linkExpression, IEnumerable<CustomerRole> roles = null, int languageId = 0, int storeId = 0)
        {
            if (roles == null)
            {
                roles = _services.WorkContext.CurrentCustomer.CustomerRoles;
            }

            if (languageId == 0)
            {
                languageId = _services.WorkContext.WorkingLanguage.Id;
            }

            if (storeId == 0)
            {
                storeId = _services.StoreContext.CurrentStore.Id;
            }

            var data = _services.Cache.Get(LINKRESOLVER_KEY.FormatInvariant(linkExpression, languageId), () =>
            {
                var d = Parse(linkExpression);

                switch (d.Type)
                {
                    case LinkType.Product:
						GetEntityData<Product>(d, languageId, x => new ResolverEntitySummary
						{
							Name = x.Name,
							Published = x.Published,
							Deleted = x.Deleted,
							SubjectToAcl = x.SubjectToAcl,
							LimitedToStores = x.LimitedToStores
						});
						break;
					case LinkType.Category:
						GetEntityData<Category>(d, languageId, x => new ResolverEntitySummary
						{
							Name = x.Name,
							Published = x.Published,
							Deleted = x.Deleted,
							SubjectToAcl = x.SubjectToAcl,
							LimitedToStores = x.LimitedToStores
						});
						break;
					case LinkType.Manufacturer:
                        GetEntityData<Manufacturer>(d, languageId, x => new ResolverEntitySummary
                        {
                            Name = x.Name,
                            Published = x.Published,
                            Deleted = x.Deleted,
                            LimitedToStores = x.LimitedToStores
                        });
                        break;
                    case LinkType.Topic:
                        GetEntityData<Topic>(d, languageId, x => null);
                        break;
                    case LinkType.Url:
                        var url = d.Value.ToString();
                        if (url.EmptyNull().StartsWith("~"))
                        {
                            url = VirtualPathUtility.ToAbsolute(url);
                        }
                        d.Link = d.Label = url;
                        break;
                    case LinkType.File:
                    default:
                        d.Link = d.Label = d.Value.ToString();
                        break;
                }

                return d;
            });

            var result = new LinkResolverResult
            {
                Type = data.Type,
                Status = data.Status,
                Value = data.Value,
                Link = data.Link,
                Label = data.Label
            };

            // Check ACL and limited to stores.
            switch (data.Type)
            {
                case LinkType.Product:
                case LinkType.Category:
                case LinkType.Manufacturer:
                case LinkType.Topic:
                    var entityName = data.Type.ToString();
                    if (data.LimitedToStores && data.Status == LinkStatus.Ok && !QuerySettings.IgnoreMultiStore && !_storeMappingService.Authorize(entityName, data.Id, storeId))
                    {
                        result.Status = LinkStatus.NotFound;
                    }
                    else if (data.SubjectToAcl && data.Status == LinkStatus.Ok && !QuerySettings.IgnoreAcl && !_aclService.Authorize(entityName, data.Id, roles))
                    {
                        result.Status = LinkStatus.Forbidden;
                    }
                    break;
            }

            return result;
        }

        protected virtual LinkResolverData Parse(string linkExpression)
        {
            if (!string.IsNullOrWhiteSpace(linkExpression))
            {
                var index = linkExpression.IndexOf(':');

                if (index != -1 && Enum.TryParse(linkExpression.Substring(0, index), true, out LinkType type))
                {
                    var value = linkExpression.Substring(index + 1);

                    switch (type)
                    {
                        case LinkType.Product:
                        case LinkType.Category:
                        case LinkType.Manufacturer:
                        case LinkType.Topic:
                            var id = value.ToInt();
                            return new LinkResolverData { Type = type, Value = id != 0 ? (object)id : value };
                        case LinkType.Url:
                        case LinkType.File:
                        default:
                            return new LinkResolverData { Type = type, Value = value };
                    }
                }
            }

            return new LinkResolverData { Type = LinkType.Url, Value = linkExpression.EmptyNull() };
        }

        internal void GetEntityData<T>(LinkResolverData data, int languageId, Expression<Func<T, ResolverEntitySummary>> selector) where T : BaseEntity
        {
            ResolverEntitySummary summary = null;
            string systemName = null;

            if (data.Value is string)
            {
                data.Id = 0;
                systemName = (string)data.Value;
            }
            else
            {
                data.Id = (int)data.Value;
            }

            if (data.Type == LinkType.Topic)
            {
                var query = _services.DbContext.Set<Topic>()
                    .AsNoTracking()
                    .AsQueryable();

                query = string.IsNullOrEmpty(systemName)
                    ? query.Where(x => x.Id == data.Id)
                    : query.Where(x => x.SystemName == systemName);

                summary = query.Select(x => new ResolverEntitySummary
                {
                    Id = x.Id,
                    Name = x.SystemName,
                    Published = x.IsPublished,
                    SubjectToAcl = x.SubjectToAcl,
                    LimitedToStores = x.LimitedToStores
                })
                .FirstOrDefault();
            }
            else
            {
                summary = _services.DbContext.Set<T>()
                    .AsNoTracking()
                    .Where(x => x.Id == data.Id)
                    .Select(selector)
                    .FirstOrDefault();
            }

            if (summary != null)
            {
                var entityName = data.Type.ToString();

                data.Id = summary.Id != 0 ? summary.Id : data.Id;
                data.SubjectToAcl = summary.SubjectToAcl;
                data.LimitedToStores = summary.LimitedToStores;
                data.Status = summary.Deleted
                    ? LinkStatus.NotFound
                    : summary.Published ? LinkStatus.Ok : LinkStatus.Hidden;

                if (data.Type == LinkType.Topic)
                {
                    data.Label = _localizedEntityService.GetLocalizedValue(languageId, data.Id, entityName, "ShortTitle").NullEmpty() ??
                        _localizedEntityService.GetLocalizedValue(languageId, data.Id, entityName, "Title").NullEmpty() ??
                        summary.Name;
                }
                else
                {
                    data.Label = _localizedEntityService.GetLocalizedValue(languageId, data.Id, entityName, "Name").NullEmpty() ?? summary.Name;
                }

                var slug = _urlRecordService.GetActiveSlug(data.Id, entityName, languageId).NullEmpty() ?? _urlRecordService.GetActiveSlug(data.Id, entityName, 0);
                if (!string.IsNullOrEmpty(slug))
                {
                    data.Link = _urlHelper.RouteUrl(entityName, new { SeName = slug });
                }
            }
            else
            {
                data.Label = systemName;
                data.Status = LinkStatus.NotFound;
            }
        }
    }


    internal class ResolverEntitySummary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Deleted { get; set; }
        public bool Published { get; set; }
        public bool SubjectToAcl { get; set; }
        public bool LimitedToStores { get; set; }
    }
}
