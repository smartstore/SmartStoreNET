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
using SmartStore.Core.Events;
using SmartStore.Web.Framework.Security;
using SmartStore.Core.Data;

namespace SmartStore.Web.Controllers
{
	[SessionState(SessionStateBehavior.Disabled)]
	[OverrideAuthentication]
	[OverrideAuthorization]
	[OverrideResultFilters]
	//[OverrideActionFilters] // TBD: (mc) really?
	[OverrideExceptionFilters]
	public partial class MediaController : Controller
    {
		private readonly static bool _streamRemoteMedia = CommonHelper.GetAppSetting<bool>("sm:StreamRemoteMedia");

		private readonly IPictureService _pictureService;
		private readonly IImageProcessor _imageProcessor;
		private readonly IImageCache _imageCache;
		private readonly IUserAgent _userAgent;
		private readonly IEventPublisher _eventPublisher;
		private readonly IMediaFileSystem _mediaFileSystem;
		private readonly MediaSettings _mediaSettings;

		public MediaController(
			IPictureService pictureService,
			IImageProcessor imageProcessor,
			IImageCache imageCache,
			IUserAgent userAgent,
			IEventPublisher eventPublisher,
			IMediaFileSystem mediaFileSystem,
			MediaSettings mediaSettings)
        {
			_pictureService = pictureService;
			_imageProcessor = imageProcessor;
			_imageCache = imageCache;
			_userAgent = userAgent;
			_eventPublisher = eventPublisher;
			_mediaFileSystem = mediaFileSystem;
			_mediaSettings = mediaSettings;

			Logger = NullLogger.Instance;
        }

		public ILogger Logger { get; set; }

		public async Task<ActionResult> Image(int id /* pictureId*/, string name)
		{
			string nameWithoutExtension = null;
			string mime = null;
			string extension = null;
			Picture picture = null;

			if (name.HasValue())
			{
				// Requested file name was passed with the URL: fetch all required data without harassing DB.
				name.SplitToPair(out nameWithoutExtension, out extension, ".", true);
				mime = MimeTypes.MapNameToMimeType(name);
			}

			if (nameWithoutExtension.IsEmpty() || extension.IsEmpty())
			{
				// Missing or malformed Uri: get picture from DB and determine correct metadata by id
				picture = _pictureService.GetPictureById(id);
				if (picture == null) return NotFound(mime ?? "text/html");

				mime = picture.MimeType;
				nameWithoutExtension = picture.SeoFilename;
				extension = MimeTypes.MapMimeTypeToExtension(mime);
				name = String.Concat(nameWithoutExtension, ".", extension);
			}

			extension = extension.ToLower();

			var query = CreateImageQuery(mime, extension);
			var cachedImage = _imageCache.Get(id, nameWithoutExtension, extension, query);

			return await HandleImage(
				query, 
				cachedImage,
				nameWithoutExtension,
				mime,
				extension,
				getSourceBuffer);

			async Task<byte[]> getSourceBuffer(string prevMime)
			{
				byte[] source;

				if (picture != null)
				{
					source = await _pictureService.LoadPictureBinaryAsync(picture);
				}
				else
				{
					if (id == 0)
					{
						// This is most likely a request for a default placeholder image
						var mappedPath = CommonHelper.MapPath(Path.Combine(PictureService.FallbackImagesRootPath, name), false);
						if (!System.IO.File.Exists(mappedPath))
							return null;

						source = System.IO.File.ReadAllBytes(mappedPath);
					}
					else
					{
						// Get metadata from DB
						picture = _pictureService.GetPictureById(id);

						// Picture must exist
						if (picture == null)
							return null;

						// Picture's mime must match requested mime
						if (!picture.MimeType.IsCaseInsensitiveEqual(prevMime ?? mime))
							return null;

						// When Picture has SeoFileName, it must match requested name
						// When Picture has NO SeoFileName, requested name must match Id
						if (picture.SeoFilename.HasValue() && !picture.SeoFilename.IsCaseInsensitiveEqual(nameWithoutExtension))
							return null;
						else if (picture.SeoFilename.IsEmpty() && picture.Id.ToString(ImageCache.IdFormatString) != nameWithoutExtension)
							return null;

						source = await _pictureService.LoadPictureBinaryAsync(picture);
					}
				}

				return source;
			}
		}

		public async Task<ActionResult> File(string path)
		{
			string name = null;
			string mime = null;

			if (path.IsEmpty())
			{
				return NotFound(null);
			}

			var tenantPrefix = DataSettings.Current.TenantName + "/";
			if (path.StartsWith(tenantPrefix))
			{
				// V3.0.x comapt: in previous versions the file path
				// contained the tenant name. Strip it out.
				path = path.Substring(tenantPrefix.Length);
			}

			name = Path.GetFileName(path);

			name.SplitToPair(out var nameWithoutExtension, out var extension, ".", true);
			mime = MimeTypes.MapNameToMimeType(name);

			if (nameWithoutExtension.IsEmpty() || extension.IsEmpty())
			{
				return NotFound(mime ?? "text/html");
			}

			extension = extension.ToLower();

			var file = _mediaFileSystem.GetFile(path);

			if (!file.Exists)
			{
				return NotFound(mime);
			}

			var query = CreateImageQuery(mime, extension);
			var isProcessableImage = query.NeedsProcessing(true) && _imageProcessor.IsSupportedImage(file.Name);
			if (isProcessableImage)
			{
				var cachedImage = _imageCache.Get(file, query);
				return await HandleImage(
					query,
					cachedImage,
					nameWithoutExtension,
					mime,
					extension,
					getSourceBuffer);
			}


			// It's no image... proceed with standard stuff...

			if (ETagMatches(nameWithoutExtension, mime, file.LastUpdated))
			{
				return Content(null);
			}

			var isFaulted = false;

			try
			{
				if (Request.HttpMethod == "HEAD")
				{
					return new HttpStatusCodeResult(200);
				}
				
				if (_mediaFileSystem.IsCloudStorage && !_streamRemoteMedia)
				{
					// Redirect to existing remote file
					Response.ContentType = mime;
					return Redirect(_mediaFileSystem.GetPublicUrl(path, true));
				}
				else
				{
					// Open existing stream
					return File(file.OpenRead(), mime);
				}
			}
			catch (Exception ex)
			{
				isFaulted = true;
				Logger.ErrorFormat(ex, "Error processing media file '{0}'.", path);
				return new HttpStatusCodeResult(500, ex.Message);
			}
			finally
			{
				if (!isFaulted)
				{
					FinalizeRequest(nameWithoutExtension, mime, file.LastUpdated);
				}
			}

			async Task<byte[]> getSourceBuffer(string prevMime)
			{
				return await file.OpenRead().ToByteArrayAsync();
			}
		}

		[NonAction]
		private async Task<ActionResult> HandleImage(
			ProcessImageQuery query,
			CachedImageResult cachedImage,
			string nameWithoutExtension,
			string mime,
			string extension,
			Func<string, Task<byte[]>> getSourceBuffer)
		{
			string prevMime = null;

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
				if (ETagMatches(nameWithoutExtension, mime, cachedImage.LastModifiedUtc.Value))
				{
					return Content(null);
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
							// Call inner function
							byte[] source = await getSourceBuffer(prevMime);
							if (source == null || source.Length == 0)
							{
								return NotFound(mime);
							}

							source = await ProcessAndPutToCacheAsync(cachedImage, source, query);

							return File(source, mime);
						}
					}
				}

				if (Request.HttpMethod == "HEAD")
				{
					return new HttpStatusCodeResult(200);
				}

				if (cachedImage.IsRemote && !_streamRemoteMedia)
				{
					// Redirect to existing remote file
					Response.ContentType = mime;
					return Redirect(_imageCache.GetPublicUrl(cachedImage.Path));
				}
				else
				{
					// Open existing stream
					return File(cachedImage.File.OpenRead(), mime);
				}
			}
			catch (Exception ex)
			{
				isFaulted = true;
				if (!(ex is ProcessImageException))
				{
					// ProcessImageException is logged already in ImageProcessor
					Logger.ErrorFormat(ex, "Error processing media file '{0}'.", cachedImage.Path);
				}
				return new HttpStatusCodeResult(500, ex.Message);
			}
			finally
			{
				if (!isFaulted)
				{
					FinalizeRequest(nameWithoutExtension, mime, cachedImage.LastModifiedUtc.GetValueOrDefault());
				}
			}
		}

		private bool ETagMatches(string nameWithoutExtension, string mime, DateTime lastModifiedUtc)
		{
			string etag;

			var ifNoneMatch = Request.Headers["If-None-Match"];
			if (ifNoneMatch.HasValue())
			{
				etag = GetFileETag(nameWithoutExtension, mime, lastModifiedUtc);

				if (etag == ifNoneMatch)
				{
					// File hasn't changed, so return HTTP 304 without retrieving the data
					Response.StatusCode = 304;
					Response.StatusDescription = "Not Modified";

					// Explicitly set the Content-Length header so the client doesn't wait for
					// content but keeps the connection open for other requests
					Response.AddHeader("Content-Length", "0");

					ApplyResponseHeaders();

					return true;
				}
			}

			return false;
		}

		private void FinalizeRequest(string nameWithoutExtension, string mime, DateTime lastModifiedUtc)
		{
			var etag = GetFileETag(nameWithoutExtension, mime, lastModifiedUtc);
			ApplyResponseHeaders(lastModifiedUtc);
			ApplyETagHeader(etag);
		}

		private async Task<byte[]> ProcessAndPutToCacheAsync(CachedImageResult cachedImage, byte[] buffer, ProcessImageQuery query)
		{
			if (!query.NeedsProcessing())
			{
				await _imageCache.PutAsync(cachedImage, buffer);
				return buffer;
			}
			else
			{
				var processQuery = new ProcessImageQuery(query)
				{
					Source = buffer,
					Format = query.Format ?? cachedImage.Extension,
					FileName = cachedImage.FileName,
					DisposeSource = true
				};

				using (var result = _imageProcessor.ProcessImage(processQuery))
				{
					var outBuffer = result.OutputStream.GetBuffer();
					await _imageCache.PutAsync(cachedImage, outBuffer);
					
					if (cachedImage.Extension != result.FileExtension)
					{
						cachedImage.Path = Path.ChangeExtension(cachedImage.Path, result.FileExtension);
						cachedImage.Extension = result.FileExtension;
					}

					Logger.DebugFormat($"Processed image '{cachedImage.FileName}' in {result.ProcessTimeMs} ms.", null);

					return outBuffer;
				}
			}
		}

		private ActionResult NotFound(string mime)
		{
			Response.ContentType = mime.NullEmpty() ?? "text/html";
			Response.StatusCode = 404;
			return Content("404: Not Found");
		}

		private void ApplyResponseHeaders(DateTime? lastModifiedUtc = null)
		{
			var cache = this.Response.Cache;

			cache.SetCacheability(System.Web.HttpCacheability.Public);
			cache.VaryByHeaders["Accept-Encoding"] = true;
			cache.SetExpires(DateTime.UtcNow.AddDays(7));
			cache.SetMaxAge(TimeSpan.FromDays(7));
			cache.SetRevalidation(HttpCacheRevalidation.AllCaches);

			if (lastModifiedUtc.HasValue)
			{
				cache.SetLastModified(lastModifiedUtc.Value);
			}
		}

		private void ApplyETagHeader(string etag)
		{
			this.Response.Cache.SetETag(etag);
		}

		private string GetFileETag(string seoName, string mime, DateTime lastModifiedUtc)
		{
			var timestamp = lastModifiedUtc.ToUnixTime().ToString();
			return "\"" + String.Concat(seoName, mime, timestamp).Hash(Encoding.UTF8) + "\"";
		}

		protected virtual ProcessImageQuery CreateImageQuery(string mimeType, string extension)
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

			_eventPublisher.Publish(new ImageQueryCreatedEvent(query, this.HttpContext, mimeType, extension));

			return query;
		}
    }
}
