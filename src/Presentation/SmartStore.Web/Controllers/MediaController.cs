using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
using SmartStore.Services.Media;
using SmartStore.Services.Common;
using SmartStore.Core.Logging;
using SmartStore.Utilities;
using System.IO;
using SmartStore.Core.Async;

namespace SmartStore.Web.Controllers
{
	[SessionState(SessionStateBehavior.Disabled)]
	[OverrideAuthentication, OverrideAuthorization, OverrideResultFilters] // TBD: (mc) should we also override action filters?
	public partial class MediaController : Controller
    {
		private readonly IPictureService _pictureService;
		private readonly IImageCache _imageCache;
		private readonly IUserAgent _userAgent;
		private readonly MediaSettings _mediaSettings;

		public MediaController(
			IPictureService pictureService,
			IImageCache imageCache,
			IUserAgent userAgent,
			MediaSettings mediaSettings)
        {
			_pictureService = pictureService;
			_imageCache = imageCache;
			_userAgent = userAgent;
			_mediaSettings = mediaSettings;

			Logger = NullLogger.Instance;
        }

		public ILogger Logger { get; set; }

		public async Task<ActionResult> Image(int id /* pictureId*/, string name)
		{
			string nameWithoutExtension = null;
			string mime = null;
			string prevMime = null;
			string extension = null;
			string etag;
			Picture picture = null;

			if (name.HasValue())
			{
				// Requested file name was passed with the URL: fetch all required data without harassing DB.
				name.SplitToPair(out nameWithoutExtension, out extension, ".");
				mime = MimeTypes.MapNameToMimeType(name);
			}

			if (nameWithoutExtension.IsEmpty() || extension.IsEmpty())
			{
				// Missing or malformed Uri: get picture from DB and determine correct metadata by id
				picture = _pictureService.GetPictureById(id);
				if (picture == null) return NotFound();

				mime = picture.MimeType;
				nameWithoutExtension = picture.SeoFilename;
				extension = MimeTypes.MapMimeTypeToExtension(mime);
				name = String.Concat(nameWithoutExtension, ".", extension);
			}

			var query = CreateImageQuery();
			var cachedImage = _imageCache.GetCachedImage(id, nameWithoutExtension, extension, query);

			if (extension != cachedImage.Extension)
			{
				// The query requests another format. 
				// Adjust extension and mime type fo proper ETag creation.
				extension = cachedImage.Extension;
				prevMime = mime;
				mime = MimeTypes.MapNameToMimeType(cachedImage.FileName);
			}

			if (cachedImage.Exists)
			{
				var ifNoneMatch = Request.Headers["If-None-Match"];
				if (ifNoneMatch.HasValue())
				{
					etag = GetFileETag(nameWithoutExtension, mime, cachedImage.LastModifiedUtc.Value);

					if (etag == ifNoneMatch)
					{
						// File hasn't changed, so return HTTP 304 without retrieving the data
						Response.StatusCode = 304;
						Response.StatusDescription = "Not Modified";

						// Explicitly set the Content-Length header so the client doesn't wait for
						// content but keeps the connection open for other requests
						Response.AddHeader("Content-Length", "0");

						SetCacheHeaders(Response.Cache, etag);

						return Content(null);
					}
				}
			}

			var isFaulted = false;

			try
			{
				if (!cachedImage.Exists)
				{
					// get the async (semaphore) locker specific to this key
					var keyLock = AsyncLock.Acquire("lock" + cachedImage.Path);

					// Lock concurrent requests to same resource
					using (await keyLock.LockAsync())
					{
						_imageCache.RefreshInfo(cachedImage);

						// File could have been processed by another request in the meantime, check again.
						if (!cachedImage.Exists)
						{
							byte[] source;

							if (picture == null)
							{
								if (id == 0)
								{
									// This is most likely a request for a default placeholder image
									var mappedPath = CommonHelper.MapPath(Path.Combine(PictureService.DefaultImagesRootPath, name), false);
									if (!System.IO.File.Exists(mappedPath))
										return NotFound();

									source = System.IO.File.ReadAllBytes(mappedPath);
								}
								else
								{
									// Get metadata from DB
									picture = _pictureService.GetPictureById(id);

									// Picture must exist
									if (picture == null)
										return NotFound();

									// Picture's mime must match requested mime
									if (!picture.MimeType.IsCaseInsensitiveEqual(prevMime ?? mime))
										return NotFound();

									// When Picture has SeoFileName, it must match requested name
									// When Picture has NO SeoFileName, requested name must match Id
									if (picture.SeoFilename.HasValue() && !picture.SeoFilename.IsCaseInsensitiveEqual(nameWithoutExtension))
										return NotFound();
									else if (picture.SeoFilename.IsEmpty() && picture.Id.ToString(ImageCache.IdFormatString) != nameWithoutExtension)
										return NotFound();

									source = await _pictureService.LoadPictureBinaryAsync(picture);
								}
							}
							else
							{
								source = await _pictureService.LoadPictureBinaryAsync(picture);
							}

							var buffer = await _imageCache.ProcessAndAddImageToCacheAsync(cachedImage, source, query);
							return File(buffer, mime);
						}
					}
				}
				else
				{			
					if (cachedImage.IsRemote)
					{
						// Redirect to existing remote file
						return Redirect(_imageCache.GetPublicUrl(cachedImage.Path));
					}
					else
					{
						// Open existing stream
						return File(cachedImage.File.OpenRead(), mime);
					}		
				}
			}
			catch (Exception ex)
			{
				isFaulted = true;
				Logger.ErrorFormat(ex, "Error processing media file '{0}'.", cachedImage.Path);
				return new HttpStatusCodeResult(500, ex.Message);
			}
			finally
			{
				if (!isFaulted)
				{
					var lastModifiedUtc = cachedImage.LastModifiedUtc.GetValueOrDefault();
					etag = GetFileETag(nameWithoutExtension, mime, lastModifiedUtc);

					Response.Cache.SetLastModified(lastModifiedUtc);
					SetCacheHeaders(Response.Cache, etag);
				}
			}
		}

		private ActionResult NotFound()
		{
			Response.StatusCode = 404;
			return Content("404: Not Found");
		}

		private void SetCacheHeaders(HttpCachePolicyBase cache, string etag)
		{
			cache.SetCacheability(System.Web.HttpCacheability.Public);
			cache.SetExpires(DateTime.Now.ToUniversalTime().AddDays(7));
			cache.SetMaxAge(TimeSpan.FromDays(7));
			cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
			cache.SetETag(etag);
			//cache.SetValidUntilExpires(false);
			//cache.VaryByParams["id"] = true;
			cache.VaryByParams["size"] = true;
			cache.VaryByHeaders["Accept-Encoding"] = true;
			//cache.SetLastModified(DateTime.SpecifyKind(picture.UpdatedOnUtc, DateTimeKind.Utc));
		}

		private string GetFileETag(string seoName, string mime, DateTime lastModifiedUtc)
		{
			var timestamp = lastModifiedUtc.ToUnixTime().ToString();
			return "\"" + String.Concat(seoName, mime, timestamp).Hash(Encoding.UTF8) + "\"";
		}

		private ProcessImageQuery CreateImageQuery()
		{
			var query = new ProcessImageQuery(null, Request.QueryString);

			if (query.MaxWidth == null && query.MaxHeight == null && query.Contains("size"))
			{
				int size = query["size"].Convert<int>();
				query.MaxWidth = size;
				query.MaxHeight = size;

				query.Remove("size");
			}

			if (query.Quality == null)
			{
				query.Quality = _mediaSettings.DefaultImageQuality;
			}

			//if (query.Format == null && _userAgent.UserAgent.SupportsWebP)
			//{
			//	query.Format = "webp";
			//}

			// TODO: (mc) Handle WebP format properly
			// TODO: (mc) Publish event ImageQueryCreated

			return query;
		}
    }
}
