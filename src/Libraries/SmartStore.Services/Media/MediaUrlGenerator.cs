using System.Globalization;
using System.Web;
using System.Web.Hosting;
using SmartStore.Core;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Configuration;
using SmartStore.Services.Media.Imaging;

namespace SmartStore.Services.Media
{
    public partial class MediaUrlGenerator : IMediaUrlGenerator
    {
        private readonly HttpContextBase _httpContext;

        private readonly string _host;
        private readonly string _appPath;
        private readonly string _fallbackImageFileName;

        private static readonly string _processedImagesRootPath;
        private static readonly string _fallbackImagesRootPath;

        static MediaUrlGenerator()
        {
            _processedImagesRootPath = MediaFileSystem.GetMediaPublicPath();
            _fallbackImagesRootPath = "content/images/";
        }

        public MediaUrlGenerator(
            ISettingService settingService,
            MediaSettings mediaSettings,
            IStoreContext storeContext,
            HttpContextBase httpContext)
        {
            _httpContext = httpContext;

            string appPath = "/";

            if (HostingEnvironment.IsHosted)
            {
                appPath = HostingEnvironment.ApplicationVirtualPath.EmptyNull();

                var cdn = storeContext.CurrentStore.ContentDeliveryNetwork;
                if (cdn.HasValue() && !_httpContext.IsDebuggingEnabled && !_httpContext.Request.IsLocal)
                {
                    _host = cdn;
                }
                else if (mediaSettings.AutoGenerateAbsoluteUrls)
                {
                    var uri = httpContext.Request.Url;
                    _host = "//{0}{1}".FormatInvariant(uri.Authority, appPath);
                }
                else
                {
                    _host = appPath;
                }
            }

            _host = _host.EmptyNull().EnsureEndsWith("/");
            _appPath = appPath.EnsureEndsWith("/");
            _fallbackImageFileName = settingService.GetSettingByKey("Media.DefaultImageName", "default-image.png");
        }

        public static string FallbackImagesRootPath => _fallbackImagesRootPath;

        public virtual string GenerateUrl(
            MediaFileInfo file,
            ProcessImageQuery imageQuery,
            string host = null,
            bool doFallback = true)
        {
            string path;

            // Build virtual path with pattern "media/{id}/{album}/{dir}/{NameWithExt}"
            if (file?.Path != null)
            {
                path = _processedImagesRootPath + file.Id.ToString(CultureInfo.InvariantCulture) + "/" + file.Path;
            }
            else if (doFallback)
            {
                path = _processedImagesRootPath + "0/" + _fallbackImageFileName;
            }
            else
            {
                return null;
            }

            if (host == null)
            {
                host = _host;
            }
            else if (host == string.Empty)
            {
                host = _appPath;
            }
            else
            {
                host = host.EnsureEndsWith("/");
            }

            var url = host;

            // Strip leading "/", the host/apppath has this already
            if (path[0] == '/')
            {
                path = path.Substring(1);
            }

            // Append media path
            url += path;

            // Append query
            var query = imageQuery?.ToString(false);
            if (query != null && query.Length > 0)
            {
                url += query;
            }

            return url;
        }
    }
}
