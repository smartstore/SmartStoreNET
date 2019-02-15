using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;

namespace SmartStore.Services.Cms
{
    public partial class LinkResolver : ILinkResolver
    {
        private const string LINKRESOLVER_NAME_KEY = "SmartStore.linkresolver.name-{0}-{1}";
        private const string LINKRESOLVER_LINK_KEY = "SmartStore.linkresolver.link-{0}-{1}";

        protected readonly IRepository<Picture> _pictureRepository;
        protected readonly IUrlRecordService _urlRecordService;
        protected readonly ILanguageService _languageService;
        protected readonly IPictureService _pictureService;
        protected readonly ILocalizedEntityService _localizedEntityService;
        protected readonly IWorkContext _workContext;
        protected readonly ICacheManager _cache;
        protected readonly UrlHelper _urlHelper;

        public LinkResolver(
            IRepository<Picture> pictureRepository,
            IUrlRecordService urlRecordService,
            ILanguageService languageService,
            IPictureService pictureService,
            ILocalizedEntityService localizedEntityService,
            IWorkContext workContext,
            ICacheManager cache,
            UrlHelper urlHelper)
        {
            _pictureRepository = pictureRepository;
            _urlRecordService = urlRecordService;
            _languageService = languageService;
            _pictureService = pictureService;
            _localizedEntityService = localizedEntityService;
            _workContext = workContext;
            _cache = cache;
            _urlHelper = urlHelper;
        }

        protected virtual TokenizeResult Parse(string linkExpression)
        {
            var arr = linkExpression.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length != 2)
            {
                return null;
            }

            if (!Enum.TryParse(arr[0], true, out TokenizeType type))
            {
                return null;
            }

            switch (type)
            {
                case TokenizeType.Product:
                case TokenizeType.Category:
                case TokenizeType.Manufacturer:
                case TokenizeType.Topic:
                    return new TokenizeResult(type, arr[1].ToInt());
                case TokenizeType.Media:
                    // Picture.Id, more to come later.
                    return new TokenizeResult(type, arr[1].ToInt());

                case TokenizeType.Url:
                case TokenizeType.File:
                default:
                    return new TokenizeResult(type, arr[1]);
            }
        }

        public virtual TokenizeResult GetDisplayName(string linkExpression, int languageId = 0)
        {
            Guard.NotEmpty(linkExpression, nameof(linkExpression));

            if (languageId == 0)
            {
                languageId = _workContext.WorkingLanguage.Id;
            }

            var data = _cache.Get(LINKRESOLVER_NAME_KEY.FormatInvariant(linkExpression, languageId), () =>
            {
                var result = Parse(linkExpression);
                if (result != null)
                {
                    var entityName = result.Type.ToString();

                    switch (result.Type)
                    {
                        case TokenizeType.Product:
                        case TokenizeType.Category:
                        case TokenizeType.Manufacturer:
                            result.Result = _localizedEntityService.GetLocalizedValue(languageId, (int)result.Value, entityName, "Name");
                            break;
                        case TokenizeType.Topic:
                            result.Result = _localizedEntityService.GetLocalizedValue(languageId, (int)result.Value, entityName, "ShortTitle");
                            if (string.IsNullOrEmpty(result.Result))
                            {
                                result.Result = _localizedEntityService.GetLocalizedValue(languageId, (int)result.Value, entityName, "Title");
                            }
                            break;
                        case TokenizeType.Media:
                            var entityId = (int)result.Value;
                            result.Result = _pictureRepository.TableUntracked.Where(x => x.Id == entityId).Select(x => x.SeoFilename).FirstOrDefault();
                            break;
                        case TokenizeType.Url:
                            var url = result.Value.ToString();
                            if (url.EmptyNull().StartsWith("~"))
                            {
                                url = VirtualPathUtility.ToAbsolute(url);
                            }
                            result.Result = url;
                            break;
                        case TokenizeType.File:
                        default:
                            result.Result = result.Value.ToString();
                            break;
                    }
                }

                return result;
            });

            return data;
        }

        public virtual TokenizeResult GetLink(string linkExpression, int languageId = 0)
        {
            Guard.NotEmpty(linkExpression, nameof(linkExpression));

            if (languageId == 0)
            {
                languageId = _workContext.WorkingLanguage.Id;
            }

            var data = _cache.Get(LINKRESOLVER_LINK_KEY.FormatInvariant(linkExpression, languageId), () =>
            {
                var result = Parse(linkExpression);
                if (result != null)
                {
                    switch (result.Type)
                    {
                        case TokenizeType.Product:
                        case TokenizeType.Category:
                        case TokenizeType.Manufacturer:
                        case TokenizeType.Topic:
                            var entityName = result.Type.ToString();
                            // Perf: GetActiveSlug only fetches UrlRecord.Slug from database.
                            var slug = _urlRecordService.GetActiveSlug((int)result.Value, entityName, languageId);
                            if (!string.IsNullOrEmpty(slug))
                            {
                                result.Result = _urlHelper.RouteUrl(entityName, new { SeName = slug });
                            }
                            break;
                        case TokenizeType.Media:
                            result.Result = _pictureService.GetUrl((int)result.Value);
                            break;
                        case TokenizeType.Url:
                            var url = result.Value.ToString();
                            if (url.EmptyNull().StartsWith("~"))
                            {
                                url = VirtualPathUtility.ToAbsolute(url);
                            }
                            result.Result = url;
                            break;
                        case TokenizeType.File:
                        default:
                            result.Result = result.Value.ToString();
                            break;
                    }
                }

                return result;
            });

            return data;
        }
    }
}
