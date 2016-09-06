using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using SmartStore.Core;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
using SmartStore.Services.Media;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Admin.Controllers
{
	[SessionState(SessionStateBehavior.Disabled)]
	public partial class MediaController : AdminControllerBase
    {
		private readonly IPictureService _pictureService;
		private readonly IImageCache _imageCache;
		private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;

        public MediaController(
			IPictureService pictureService,
			IImageCache imageCache,
			IPermissionService permissionService, 
			IWebHelper webHelper)
        {
			_pictureService = pictureService;
			_imageCache = imageCache;
			_permissionService = permissionService;
            _webHelper = webHelper;
        }

        [HttpPost]
        public ActionResult UploadImage()
        {
			var result = this.UploadImageInternal();

			if (result.Success)
			{
				ViewData["ResultCode"] = "success";
				ViewData["Result"] = "success";
				ViewData["FileName"] = result.Url;
			}
			else 
			{
				ViewData["ResultCode"] = "failed";
				ViewData["Result"] = result.Message;
			}
			
            return View();
        }

		[HttpPost]
		public ActionResult UploadImageAjax()
		{
			var result = this.UploadImageInternal();
			return Json(result);
		}

		public async Task<ActionResult> Picture(int id /* pictureId*/, int? size)
		{
			var picture = _pictureService.GetPictureById(id);

			if (picture == null)
				return HttpNotFound();

			var name = picture.SeoFilename;
			var mime = picture.MimeType;
			var timestamp = picture.UpdatedOnUtc.ToUnixTime().ToString();
			var etag = GetFileETag(name, mime, timestamp);
			var targetSize = size ?? 100;

			CachedImageResult cachedImage = null;
			
			var ifNoneMatch = Request.Headers["If-None-Match"];

			if (ifNoneMatch.HasValue() && etag == ifNoneMatch)
			{
				cachedImage = GetCachedImage(picture, targetSize);

				// the file could have been deleted in the meantime. Check for it.
				if (cachedImage.Exists)
				{
					// File hasn't changed, so return HTTP 304 without retrieving the data
					Response.StatusCode = 304;
					Response.StatusDescription = "Not Modified";

					// Explicitly set the Content-Length header so the client doesn't wait for
					// content but keeps the connection open for other requests
					Response.AddHeader("Content-Length", "0");

					HandleCaching(Response.Cache, etag);

					return Content(null);
				}
			}

			var extension = MimeTypes.MapMimeTypeToExtension(mime);
			
			cachedImage = cachedImage ?? GetCachedImage(picture, targetSize);

			HandleCaching(Response.Cache, etag);

			if (!cachedImage.Exists)
			{
				// create and return result
				var buffer = await _imageCache.ProcessAndAddImageToCacheAsync(cachedImage, await _pictureService.LoadPictureBinaryAsync(picture), targetSize);
				return File(buffer, mime);
			}
			else
			{
				// open existing stream
				var stream = _imageCache.OpenCachedImage(cachedImage);
				return File(stream, mime);
			}
		}

		private void HandleCaching(HttpCachePolicyBase cache, string etag)
		{
			cache.SetCacheability(System.Web.HttpCacheability.Public);
			//cache.SetExpires(DateTime.Now.AddDays(7));
			cache.SetMaxAge(TimeSpan.FromDays(7));
			cache.SetETag(etag);
			//cache.SetValidUntilExpires(false);
			cache.VaryByParams["id"] = true;
			cache.VaryByParams["size"] = true;
			//cache.SetLastModified(DateTime.SpecifyKind(picture.UpdatedOnUtc, DateTimeKind.Utc));
		}

		private string GetFileETag(string name, string mimeType, string timestamp)
		{
			return "\"" + String.Concat(name, mimeType, timestamp).Hash(Encoding.UTF8) + "\"";
		}

		private CachedImageResult GetCachedImage(Picture picture, int size)
		{
			var settings = new NameValueCollection();
			settings["maxwidth"] = size.ToString();
			settings["maxheight"] = size.ToString();

			return _imageCache.GetCachedImage(picture, settings);
		}

		private UploadFileResult UploadImageInternal()
		{
			var postedFile = Request.ToPostedFileResult();
			if (postedFile == null)
			{
				return new UploadFileResult { Message = T("Common.NoFileUploaded") };
			}

			if (postedFile.FileName.IsEmpty())
			{
				return new UploadFileResult { Message = "No file name provided" };
			}

			var directory = "~/Media/Uploaded/";
			var filePath = Path.Combine(_webHelper.MapPath(directory), postedFile.FileName);

			if (!!postedFile.IsImage)
			{
				return new UploadFileResult { Message = "Files with extension '{0}' cannot be uploaded".FormatInvariant(postedFile.FileExtension) };
			}

			postedFile.File.SaveAs(filePath);

			return new UploadFileResult
			{
				Success = true,
				Url = this.Url.Content(string.Format("{0}{1}", directory, postedFile.FileName))
			};
		}

		public class UploadFileResult
		{
			public bool Success { get; set; }
			public string Url { get; set; }
			public string Message { get; set; }
		}

    }
}
