using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Web.SessionState;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Events;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;
using SmartStore.Services.Common;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;
using SmartStore.Utilities;
using SmartStore.Utilities.Threading;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Seo;
using SmartStore.Web.Framework.Localization;

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

		private readonly Lazy<SeoSettings> _seoSettings;
		private readonly Lazy<IXmlSitemapGenerator> _sitemapGenerator;

		public MediaController(
			IPictureService pictureService,
			IImageProcessor imageProcessor,
			IImageCache imageCache,
			IUserAgent userAgent,
			IEventPublisher eventPublisher,
			IMediaFileSystem mediaFileSystem,
			MediaSettings mediaSettings,
			Lazy<SeoSettings> seoSettings,
			Lazy<IXmlSitemapGenerator> sitemapGenerator)
        {
			_pictureService = pictureService;
			_imageProcessor = imageProcessor;
			_imageCache = imageCache;
			_userAgent = userAgent;
			_eventPublisher = eventPublisher;
			_mediaFileSystem = mediaFileSystem;
			_mediaSettings = mediaSettings;
			_seoSettings = seoSettings;
			_sitemapGenerator = sitemapGenerator;

			Logger = NullLogger.Instance;
        }

		public ILogger Logger { get; set; }

		#region XML sitemap

		[RewriteUrl(SslRequirement.No)]
		[LanguageSeoCode(Order = 1)]
		[SetWorkingCulture(Order = 2)]
		public async Task<ActionResult> XmlSitemap(int? index = null)
		{
			if (!_seoSettings.Value.XmlSitemapEnabled)
				return HttpNotFound();

			try
			{
				var partition = await _sitemapGenerator.Value.GetSitemapPartAsync(index ?? 0);
				return new FileStreamResult(partition.Stream, "text/xml");
			}
			catch (IndexOutOfRangeException)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Sitemap index is out of range.");
			}
			catch (Exception ex)
			{
				return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		#endregion

		public async Task<ActionResult> Image(int id /* pictureId*/, string name)
		{
			string nameWithoutExtension = null;
			string mime = null;
			string extension = null;
			MediaFile picture = null;

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
				nameWithoutExtension = picture.Name;
				extension = MimeTypes.MapMimeTypeToExtension(mime);
				name = String.Concat(nameWithoutExtension, ".", extension);
			}
			
			extension = extension.ToLower();

			var query = CreateImageQuery(mime, extension);
			var cachedImage = _imageCache.Get(id, nameWithoutExtension, extension, query);

			return await HandleImageAsync(
				query,
				cachedImage,
				nameWithoutExtension,
				mime,
				extension,
				getSourceBufferAsync);

			async Task<byte[]> getSourceBufferAsync(string prevMime)
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
						if (picture.Name.HasValue() && !picture.Name.IsCaseInsensitiveEqual(name))
							return null;
						//else if (picture.Name.IsEmpty() && picture.Id.ToString(ImageCache.IdFormatString) != nameWithoutExtension)
						//	return null;

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
				// V3.0.x compat: in previous versions the file path
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
			var isProcessableImage = query.NeedsProcessing(true) && _imageProcessor.IsSupportedImage(file.Extension);
			if (isProcessableImage)
			{
				var cachedImage = _imageCache.Get(file, query);
				return await HandleImageAsync(
					query,
					cachedImage,
					nameWithoutExtension,
					mime,
					extension,
					getSourceBufferAsync);
			}


			// It's no image... proceed with standard stuff...

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
				return new CachedFileResult(file, mime);
			}

			async Task<byte[]> getSourceBufferAsync(string prevMime)
			{
				return await file.OpenRead().ToByteArrayAsync();
			}
		}

		[NonAction]
		private async Task<ActionResult> HandleImageAsync(
			ProcessImageQuery query,
			CachedImage cachedImage,
			string nameWithoutExtension,
			string mime,
			string extension,
			Func<string, Task<byte[]>> getSourceBufferAsync)
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

			try
			{
				if (!cachedImage.Exists)
				{
					// Lock concurrent requests to same resource
					using (await KeyedLock.LockAsync("MediaController.HandleImage." + cachedImage.Path))
					{
						_imageCache.RefreshInfo(cachedImage);

						// File could have been processed by another request in the meantime, check again.
						if (!cachedImage.Exists)
						{
							// Call inner function
							byte[] source = await getSourceBufferAsync(prevMime);
							if (source == null || source.Length == 0)
							{
								return NotFound(mime);
							}

							source = await ProcessAndPutToCacheAsync(cachedImage, source, query);
							return new CachedFileResult(mime, cachedImage.LastModifiedUtc.GetValueOrDefault(), () => source, source.LongLength);
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
					return new CachedFileResult(cachedImage.File, mime);
				}
			}
			catch (Exception ex)
			{
				if (!(ex is ProcessImageException))
				{
					// ProcessImageException is logged already in ImageProcessor
					Logger.ErrorFormat(ex, "Error processing media file '{0}'.", cachedImage.Path);
				}
				return new HttpStatusCodeResult(500, ex.Message);
			}
		}

		private async Task<byte[]> ProcessAndPutToCacheAsync(CachedImage cachedImage, byte[] buffer, ProcessImageQuery query)
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
					var outBuffer = result.OutputStream.ToArray();

					await _imageCache.PutAsync(cachedImage, outBuffer);

					if (cachedImage.Extension != result.FileExtension)
					{
						// jpg <> jpeg
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

		protected virtual ProcessImageQuery CreateImageQuery(string mimeType, string extension)
		{
            if (extension == "svg")
            {
                return new ProcessImageQuery { Format = "svg" };
            }

            var qs = Request.QueryString;

			// TODO: (mc) implement "raw" image handling later
			//if (qs.GetValues(null).Contains("raw", StringComparer.OrdinalIgnoreCase) || qs["raw"] != null)
			//{
			//	return null;
			//}

			var query = new ProcessImageQuery(null, qs);
			
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
