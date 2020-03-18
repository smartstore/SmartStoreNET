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
using System.Web.Routing;

namespace SmartStore.Web.Controllers
{
	[SessionState(SessionStateBehavior.Disabled)]
	[OverrideAuthentication]
	[OverrideAuthorization]
	[OverrideResultFilters]
	//[OverrideActionFilters] // TBD: (mc) really?
	[OverrideExceptionFilters]
	public partial class Media4Controller : Controller
    {
		private readonly static bool _streamRemoteMedia = CommonHelper.GetAppSetting<bool>("sm:StreamRemoteMedia");

		private readonly IMediaService _mediaService;
		private readonly IFolderService _folderService;
		private readonly IImageProcessor _imageProcessor;
		private readonly IImageCache _imageCache;
		private readonly IUserAgent _userAgent;
		private readonly IEventPublisher _eventPublisher;
		private readonly IMediaFileSystem _mediaFileSystem;
		private readonly MediaSettings _mediaSettings;
		private readonly MediaHelper _mediaHelper;

		private readonly Lazy<SeoSettings> _seoSettings;
		private readonly Lazy<IXmlSitemapGenerator> _sitemapGenerator;

		public Media4Controller(
			IMediaService mediaService,
			IFolderService folderService,
			IImageProcessor imageProcessor,
			IImageCache imageCache,
			IUserAgent userAgent,
			IEventPublisher eventPublisher,
			IMediaFileSystem mediaFileSystem,
			MediaSettings mediaSettings,
			MediaHelper mediaHelper,
			Lazy<SeoSettings> seoSettings,
			Lazy<IXmlSitemapGenerator> sitemapGenerator)
        {
			_mediaService = mediaService;
			_folderService = folderService;
			_imageProcessor = imageProcessor;
			_imageCache = imageCache;
			_userAgent = userAgent;
			_eventPublisher = eventPublisher;
			_mediaFileSystem = mediaFileSystem;
			_mediaSettings = mediaSettings;
			_mediaHelper = mediaHelper;
			_seoSettings = seoSettings;
			_sitemapGenerator = sitemapGenerator;
        }

		public ILogger Logger { get; set; } = NullLogger.Instance;

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

		/// <summary>
		/// Redirect legacy URL "/uploaded/some/file.png" to "/file/1234/some/file.png"
		/// </summary>
		public ActionResult Uploaded(string path)
		{
			path = SystemAlbumProvider.Files + "/" + path;

			var mediaFile = _mediaService.GetFileByPath(path);
			if (mediaFile == null)
			{
				return NotFound(null);
			}

			var routeValues = new RouteValueDictionary(RouteData.Values);
			routeValues["id"] = mediaFile.Id;
			routeValues["path"] = path;

			return RedirectToActionPermanent("File", routeValues);
		}

		public async Task<ActionResult> File(int id /* mediaFileId */, string path)
		{
			if (!_mediaHelper.TokenizePath(path, out var pathData))
			{
				return NotFound(null);
			}

			var query = CreateImageQuery(pathData.MimeType, pathData.Extension);
			var isProcessableImage = query.NeedsProcessing(true) && _imageProcessor.IsSupportedImage(pathData.Extension);
			if (isProcessableImage)
			{
				var cachedImage = _imageCache.Get4(id, pathData, query);
				return await HandleImageAsync(
					query,
					cachedImage,
					pathData,
					getSourceStream);
			}

			// It's no image... proceed with standard stuff...
			
			if (Request.HttpMethod == "HEAD")
			{
				return new HttpStatusCodeResult(200);
			}

			return null;

			Stream getSourceStream(string prevMime)
			{
				var mediaFile = _mediaService.GetFileById(id);
				return mediaFile.OpenRead();
			}
		}

		[NonAction]
		private async Task<ActionResult> HandleImageAsync(
			ProcessImageQuery query,
			CachedImage cachedImage,
			MediaPathData pathData,
			Func<string, Stream> getSourceStream)
		{
			string prevMime = null;

			if (pathData.Extension != cachedImage.Extension)
			{
				// The query requests another format. 
				// Adjust extension and mime type fo proper ETag creation.
				pathData.Extension = cachedImage.Extension;
				prevMime = pathData.MimeType;
				pathData.MimeType = MimeTypes.MapNameToMimeType(cachedImage.FileName);
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
							var stream = getSourceStream(prevMime);
							if (stream == null || stream.Length == 0)
							{
								return NotFound(pathData.MimeType);
							}

							stream = await ProcessAndPutToCacheAsync(cachedImage, stream, query);
							return new CachedFileResult(pathData.MimeType, cachedImage.LastModifiedUtc.GetValueOrDefault(), () => stream, stream.Length);
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
					Response.ContentType = pathData.MimeType;
					return Redirect(_imageCache.GetPublicUrl(cachedImage.Path));
				}
				else
				{
					// Open existing stream
					return new CachedFileResult(cachedImage.File, pathData.MimeType);
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

		private async Task<Stream> ProcessAndPutToCacheAsync(CachedImage cachedImage, Stream stream, ProcessImageQuery query)
		{
			if (!query.NeedsProcessing())
			{
				await _imageCache.Put4Async(cachedImage, stream);
				return stream;
			}
			else
			{
				var processQuery = new ProcessImageQuery(query)
				{
					Source = stream,
					Format = query.Format ?? cachedImage.Extension,
					FileName = cachedImage.FileName,
					DisposeSource = true
				};

				using (var result = _imageProcessor.ProcessImage(processQuery))
				{
					await _imageCache.Put4Async(cachedImage, result.OutputStream);

					if (cachedImage.Extension != result.FileExtension)
					{
						// jpg <> jpeg
						cachedImage.Path = Path.ChangeExtension(cachedImage.Path, result.FileExtension);
						cachedImage.Extension = result.FileExtension;
					}

					Logger.DebugFormat($"Processed image '{cachedImage.FileName}' in {result.ProcessTimeMs} ms.", null);

					return result.OutputStream;
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
