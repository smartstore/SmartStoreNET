using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using SmartStore.Core;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Configuration;

namespace SmartStore.Services.Media
{
    public partial class MediaUrlGenerator : IMediaUrlGenerator
    {
		private readonly ISettingService _settingService;
		private readonly HttpContextBase _httpContext;

        private readonly string _host;
        private readonly string _appPath;

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
			_settingService = settingService;
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
		}

		public static string FallbackImagesRootPath
		{
			get { return "~/" + _fallbackImagesRootPath; }
		}

		protected virtual string GetFallbackImageFileName(FallbackPictureType defaultPictureType = FallbackPictureType.Entity)
		{
			string defaultImageFileName;

			switch (defaultPictureType)
			{
				case FallbackPictureType.Entity:
					defaultImageFileName = _settingService.GetSettingByKey("Media.DefaultImageName", "default-image.png");
					break;
				default:
					defaultImageFileName = _settingService.GetSettingByKey("Media.DefaultImageName", "default-image.png");
					break;
			}

			return defaultImageFileName;
		}

		public virtual string GenerateUrl(
			MediaFileInfo file, 
			ProcessImageQuery imageQuery,
			string host = null)
		{
			// TODO: (mm) DoFallback
			
			// Build virtual path with pattern "media/{id}/{album}/{dir}/{NameWithExt}"
			var path = _processedImagesRootPath + file.Id.ToString(CultureInfo.InvariantCulture) + "/" + file.Path;

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
