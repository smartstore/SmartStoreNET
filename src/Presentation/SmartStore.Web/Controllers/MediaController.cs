using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Web.SessionState;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Seo;
using SmartStore.Web.Framework.Localization;
using System.Web.Routing;
using SmartStore.Core.Security;
using System.Collections.Generic;
using SmartStore.Core.IO;
using SmartStore.Core;

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
		//private readonly static bool _streamRemoteMedia = CommonHelper.GetAppSetting<bool>("sm:StreamRemoteMedia");

		private readonly IMediaService _mediaService;
		private readonly IFolderService _folderService;
		private readonly IPermissionService _permissionService;
		private readonly IEventPublisher _eventPublisher;
		private readonly IWorkContext _workContext;
		private readonly MediaSettings _mediaSettings;
		private readonly MediaHelper _mediaHelper;

		private readonly Lazy<IEnumerable<IMediaHandler>> _mediaHandlers;
		private readonly Lazy<SeoSettings> _seoSettings;
		private readonly Lazy<IXmlSitemapGenerator> _sitemapGenerator;

		public MediaController(
			IMediaService mediaService,
			IFolderService folderService,
			IPermissionService permissionService,
			IEventPublisher eventPublisher,
			IWorkContext workContext,
			MediaSettings mediaSettings,
			MediaHelper mediaHelper,
			Lazy<IEnumerable<IMediaHandler>> mediaHandlers,
			Lazy<SeoSettings> seoSettings,
			Lazy<IXmlSitemapGenerator> sitemapGenerator)
        {
			_mediaService = mediaService;
			_folderService = folderService;
			_permissionService = permissionService;
			_eventPublisher = eventPublisher;
			_workContext = workContext;
			_mediaSettings = mediaSettings;
			_mediaHelper = mediaHelper;
			_mediaHandlers = mediaHandlers;
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

			var mediaFile = _mediaService.GetFileByPath(path, MediaLoadFlags.AsNoTracking);
			if (mediaFile == null)
			{
				return NotFound(null);
			}

			var routeValues = new RouteValueDictionary(RouteData.Values)
			{
				["id"] = mediaFile.Id,
				["path"] = path
			};

			return RedirectToActionPermanent("File", routeValues);
		}

		[AcceptVerbs("GET", "HEAD")]
		public async Task<ActionResult> File(int id /* mediaFileId */, string path)
		{
			MediaFileInfo mediaFile = null;
			MediaPathData pathData = null;

			if (id == 0)
			{
				// This is most likely a request for a default placeholder image
				pathData = new MediaPathData(path);
			}
			else if (!_mediaHelper.TokenizePath(path, out pathData))
			{
				// Missing or malformed Uri: get file metadata from DB by id, but only when current user has media manage rights
				if (!_permissionService.Authorize(Permissions.Media.Update))
				{
					return NotFound(null);
				}
				
				mediaFile = _mediaService.GetFileById(id, MediaLoadFlags.AsNoTracking);
				if (mediaFile == null || mediaFile.FolderId == null || mediaFile.Deleted)
				{
					return NotFound(mediaFile?.MimeType);
				}

				pathData = new MediaPathData(_folderService.GetNodeById(mediaFile.FolderId.Value), mediaFile.Name)
				{
					Extension = mediaFile.Extension,
					MimeType = mediaFile.MimeType
				};
			}

			var handlerContext = new MediaHandlerContext
			{
				HttpContext = HttpContext,
				CurrentCustomer = _workContext.CurrentCustomer,
				PermissionService = _permissionService,
				MediaFileId = id,
				RawPath = path,
				MediaService = _mediaService,
				PathData = pathData,
				SourceFile = mediaFile,
				ImageQuery = CreateImageQuery(pathData.MimeType, pathData.Extension)
			};

			var handlers = _mediaHandlers.Value.OrderBy(x => x.Order).ToArray();

			IMediaHandler currentHandler;
			for (var i = 0; i < handlers.Length; i++)
			{
				currentHandler = handlers[i];

				// Execute handler
				await currentHandler.ExecuteAsync(handlerContext);

				if (handlerContext.Exception != null)
				{
					var isThumbExtractFail = handlerContext.Exception is ExtractThumbnailException;
					var statusCode = isThumbExtractFail ? HttpStatusCode.NoContent : HttpStatusCode.InternalServerError;
					var statusMessage = isThumbExtractFail ? handlerContext.Exception.InnerException?.Message.EmptyNull() : handlerContext.Exception.Message;

					return new HttpStatusCodeResult(statusCode, statusMessage);	
				}

				if (handlerContext.Executed || handlerContext.ResultFile != null)
				{
					// Get out if the handler produced a result file or has been executed in any way
					break;
				}
			}

			var responseFile = handlerContext.ResultFile ?? handlerContext.SourceFile;
			if (responseFile == null || !responseFile.Exists)
			{
				return NotFound(pathData.MimeType);
			}

			if (handlerContext.ResultStream != null)
			{
				// A result stream instance is given when the file has just been processed by a media handler during this request.
				// In this case there is no need to open the stream from storage again.
				return new CachedFileResult(MimeTypes.MapNameToMimeType(responseFile.Name), responseFile.LastUpdated, () => handlerContext.ResultStream, handlerContext.ResultStream.Length);
			}
			else
			{
				return new CachedFileResult(responseFile, pathData.MimeType);
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
			if (query.Quality == null)
			{
				query.Quality = _mediaSettings.DefaultImageQuality;
			}

			_eventPublisher.Publish(new ImageQueryCreatedEvent(query, this.HttpContext, mimeType, extension));

			return query;
		}
	}
}
