using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Topics;
using SmartStore.Services.Localization;
using SmartStore.Services.Seo;

namespace SmartStore.Services.Cms
{
    public partial class LinkResolver : ILinkResolver
    {
        private const string LINKRESOLVER_KEY = "SmartStore.linkresolver-{0}-{1}";

        protected readonly ICommonServices _services;
        protected readonly IUrlRecordService _urlRecordService;
        protected readonly ILocalizedEntityService _localizedEntityService;
        protected readonly UrlHelper _urlHelper;

        public LinkResolver(
            ICommonServices services,
            IUrlRecordService urlRecordService,
            ILocalizedEntityService localizedEntityService,
            UrlHelper urlHelper)
        {
            _services = services;
            _urlRecordService = urlRecordService;
            _localizedEntityService = localizedEntityService;
            _urlHelper = urlHelper;
        }

        public virtual LinkResolverResult Resolve(string linkExpression, int languageId = 0)
        {
            if (languageId == 0)
            {
                languageId = _services.WorkContext.WorkingLanguage.Id;
            }

            var data = _services.Cache.Get(LINKRESOLVER_KEY.FormatInvariant(linkExpression, languageId), () =>
            {
                var r = Parse(linkExpression);
                var entity = r.Type.ToString();

                switch (r.Type)
                {
                    case LinkType.Product:
                    case LinkType.Category:
                    case LinkType.Manufacturer:
                        r.Link = GetLink((int)r.Value, entity, languageId);
                        r.Label = _localizedEntityService.GetLocalizedValue(languageId, (int)r.Value, entity, "Name");

                        if (string.IsNullOrEmpty(r.Label))
                        {
                            if (r.Type == LinkType.Product)
							{
								r.Label = GetFromDatabase<Product>(x => x.Name, (int)r.Value);
							}  
                            else if (r.Type == LinkType.Category)
							{
								r.Label = GetFromDatabase<Category>(x => x.Name, (int)r.Value);
							} 
                            else
							{
								r.Label = GetFromDatabase<Manufacturer>(x => x.Name, (int)r.Value);
							}      
                        }
                        break;
                    case LinkType.Topic:
						var (id, systemName) = GetTopicData(r);

						r.Link = GetLink(id, entity, languageId);

                        r.Label = _localizedEntityService.GetLocalizedValue(languageId, id, entity, "ShortTitle").NullEmpty() ?? 
							_localizedEntityService.GetLocalizedValue(languageId, id, entity, "Title").NullEmpty() ?? 
							systemName ?? GetFromDatabase<Topic>(x => x.SystemName, id);

                        break;
                    case LinkType.Url:
                        var url = r.Value.ToString();
                        if (url.EmptyNull().StartsWith("~"))
                        {
                            url = VirtualPathUtility.ToAbsolute(url);
                        }
                        r.Link = r.Label = url;
                        break;
                    case LinkType.File:
                    default:
                        r.Link = r.Label = r.Value.ToString();
                        break;
                }

                return r;
            });

            return data;

			(int, string) GetTopicData(LinkResolverResult r)
			{
				if (r.Value is string systemName)
				{
					var id = _services.DbContext.Set<Topic>().Where(x => x.SystemName == systemName).Select(x => x.Id).FirstOrDefault();
					return (id, systemName);
				}
				else
				{
					return ((int)r.Value, null);
				}
			}
        }

        protected virtual LinkResolverResult Parse(string linkExpression)
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
                            if (id != 0)
                            {
                                return new LinkResolverResult { Type = type, Value = id };
                            }
                            else
                            {
                                // System name.
                                return new LinkResolverResult { Type = type, Value = value };
                            }
                        case LinkType.Url:
                        case LinkType.File:
                        default:
                            return new LinkResolverResult { Type = type, Value = value };
                    }
                }
            }

            return new LinkResolverResult { Type = LinkType.Url, Value = linkExpression.EmptyNull() };
        }

        protected virtual string GetLink(int id, string entity, int languageId)
        {
            if (id != 0)
            {
                var slug = _urlRecordService.GetActiveSlug(id, entity, languageId).NullEmpty() ?? _urlRecordService.GetActiveSlug(id, entity, 0);

                if (!string.IsNullOrEmpty(slug))
                {
                    return _urlHelper.RouteUrl(entity, new { SeName = slug });
                }
            }

            return null;
        }

        protected virtual string GetFromDatabase<T>(Expression<Func<T, string>> selector, int entityId) where T : BaseEntity
		{
			return _services.DbContext.Set<T>()
				.AsNoTracking()
				.Where(x => x.Id == entityId)
				.Select(selector)
				.FirstOrDefault()
				.EmptyNull();
		}
    }
}
