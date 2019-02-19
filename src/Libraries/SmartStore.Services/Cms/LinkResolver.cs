using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Topics;
using SmartStore.Services.Localization;
using SmartStore.Services.Seo;

namespace SmartStore.Services.Cms
{
    public partial class LinkResolver : ILinkResolver
    {
        private const string LINKRESOLVER_NAME_KEY = "SmartStore.linkresolver.name-{0}-{1}";
        private const string LINKRESOLVER_LINK_KEY = "SmartStore.linkresolver.link-{0}-{1}";

        protected readonly ICommonServices _services;
        protected readonly IUrlRecordService _urlRecordService;
        protected readonly ILanguageService _languageService;
        protected readonly ILocalizedEntityService _localizedEntityService;
        protected readonly UrlHelper _urlHelper;

        public LinkResolver(
            ICommonServices services,
            IUrlRecordService urlRecordService,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            UrlHelper urlHelper)
        {
            _services = services;
            _urlRecordService = urlRecordService;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _urlHelper = urlHelper;
        }

        public virtual LinkResolverResult GetDisplayName(string linkExpression, int languageId = 0)
        {
            if (languageId == 0)
            {
                languageId = _services.WorkContext.WorkingLanguage.Id;
            }

            var data = _services.Cache.Get(LINKRESOLVER_NAME_KEY.FormatInvariant(linkExpression, languageId), () =>
            {
                var r = Parse(linkExpression);
                var entityName = r.Type.ToString();

                switch (r.Type)
                {
                    case LinkType.Product:
                    case LinkType.Category:
                    case LinkType.Manufacturer:
                        r.Result = _localizedEntityService.GetLocalizedValue(languageId, (int)r.Value, entityName, "Name");
                        if (string.IsNullOrEmpty(r.Result))
                        {
                            if (r.Type == LinkType.Product)
                                r.Result = GetFromDatabase<Product>(x => x.Name, (int)r.Value);
                            else if (r.Type == LinkType.Category)
                                r.Result = GetFromDatabase<Category>(x => x.Name, (int)r.Value);
                            else
                                r.Result = GetFromDatabase<Manufacturer>(x => x.Name, (int)r.Value);
                        }
                        break;                        
                    case LinkType.Topic:
                        r.Result = _localizedEntityService.GetLocalizedValue(languageId, (int)r.Value, entityName, "ShortTitle");
                        if (string.IsNullOrEmpty(r.Result))
                        {
                            r.Result = _localizedEntityService.GetLocalizedValue(languageId, (int)r.Value, entityName, "Title");
                        }
                        if (string.IsNullOrEmpty(r.Result))
                        {
                            r.Result = GetFromDatabase<Topic>(x => x.SystemName, (int)r.Value);
                        }
                        break;
                    case LinkType.Url:
                        var url = r.Value.ToString();
                        if (url.EmptyNull().StartsWith("~"))
                        {
                            url = VirtualPathUtility.ToAbsolute(url);
                        }
                        r.Result = url;
                        break;
                    case LinkType.File:
                    default:
                        r.Result = r.Value.ToString();
                        break;
                }

                return r;
            });

            return data;
        }

        public virtual LinkResolverResult GetLink(string linkExpression, int languageId = 0)
        {
            if (languageId == 0)
            {
                languageId = _services.WorkContext.WorkingLanguage.Id;
            }

            var data = _services.Cache.Get(LINKRESOLVER_LINK_KEY.FormatInvariant(linkExpression, languageId), () =>
            {
                var result = Parse(linkExpression);

                switch (result.Type)
                {
                    case LinkType.Product:
                    case LinkType.Category:
                    case LinkType.Manufacturer:
                    case LinkType.Topic:
                        var entityName = result.Type.ToString();
                        // Perf: GetActiveSlug only fetches UrlRecord.Slug from database.
                        var slug = _urlRecordService.GetActiveSlug((int)result.Value, entityName, languageId);
                        if (string.IsNullOrEmpty(slug))
                        {
                            slug = _urlRecordService.GetActiveSlug((int)result.Value, entityName, 0);
                        }
                        if (!string.IsNullOrEmpty(slug))
                        {
                            result.Result = _urlHelper.RouteUrl(entityName, new { SeName = slug });
                        }
                        break;
                    case LinkType.Url:
                        var url = result.Value.ToString();
                        if (url.EmptyNull().StartsWith("~"))
                        {
                            url = VirtualPathUtility.ToAbsolute(url);
                        }
                        result.Result = url;
                        break;
                    case LinkType.File:
                    default:
                        result.Result = result.Value.ToString();
                        break;
                }

                return result;
            });

            return data;
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
								return new LinkResolverResult(type, id);
							}
							break;
						case LinkType.Url:
						case LinkType.File:
						default:
							return new LinkResolverResult(type, value);
					}
				}
			}

			// Fallback to default.
			return new LinkResolverResult(LinkType.Url, linkExpression.EmptyNull());
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
