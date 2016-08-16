using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Services.Media;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Admin.Controllers
{
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

		public ActionResult Picture(int id /* pictureId*/, int? size)
		{
			//return new HttpStatusCodeResult(System.Net.HttpStatusCode.NotModified);

			var picture = _pictureService.GetPictureById(id);

			if (picture == null)
				return HttpNotFound();

			var targetSize = size ?? 100;

			var settings = new NameValueCollection();
			settings["maxwidth"] = targetSize.ToString();
			settings["maxheight"] = targetSize.ToString();

			//return Redirect(_pictureService.GetPictureUrl(picture, size ?? 100, false));
			
			var cachedImage = _imageCache.GetCachedImage(picture, settings);

			if (!cachedImage.Exists)
			{
				// ensure thumbnail gets created
				_pictureService.GetPictureUrl(picture, targetSize, false);
			}

			// open the stream
			var stream = _imageCache.OpenCachedImage(cachedImage);

			Response.Cache.SetCacheability(System.Web.HttpCacheability.Public);
			Response.Cache.SetLastModified(DateTime.SpecifyKind(picture.UpdatedOnUtc, DateTimeKind.Utc));

			return File(stream, picture.MimeType);
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
