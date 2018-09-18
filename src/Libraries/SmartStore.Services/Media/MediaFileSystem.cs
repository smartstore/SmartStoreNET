using System;
using SmartStore.Core.Data;
using SmartStore.Core.IO;
using SmartStore.Utilities;

namespace SmartStore.Services.Media
{
	public interface IMediaFileSystem : IFileSystem
	{
	}

	public class MediaFileSystem : LocalFileSystem, IMediaFileSystem
	{
		private static string _mediaPublicPath;

		public MediaFileSystem()
			: base(GetMediaBasePath(), "~/" + GetMediaPublicPath())
		{
			this.TryCreateFolder("Storage");
			this.TryCreateFolder("Thumbs");
			this.TryCreateFolder("Uploaded");
			this.TryCreateFolder("QueuedEmailAttachment");
			this.TryCreateFolder("Downloads");
		}

		public static string GetMediaBasePath()
		{
			var path = CommonHelper.GetAppSetting<string>("sm:MediaStoragePath")?.Trim().NullEmpty();
			if (path == null)
			{
				path = "/App_Data/Tenants/" + DataSettings.Current.TenantName + "/Media";
			}

			return path;
		}

		public static string GetMediaPublicPath()
		{
			if (_mediaPublicPath == null)
			{
				var path = CommonHelper.GetAppSetting<string>("sm:MediaPublicPath")?.Trim().NullEmpty() ?? "media";

				if (path.IsWebUrl())
				{
					throw new NotSupportedException("Fully qualified URLs are not supported for the 'sm:MediaPublicPath' setting.");
				}

				_mediaPublicPath = path.TrimStart('~', '/').Replace('\\', '/').ToLower().EnsureEndsWith("/");
			}

			
			return _mediaPublicPath;
		}
	}
}
