using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Services.Security;

namespace SmartStore.Admin.Controllers
{

	public partial class MediaController : AdminControllerBase
    {
		private static readonly Regex s_allowedImageTypes = new Regex(@"(.*?)\.(gif|jpg|jpeg|png|bmp|ico|svg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;

        public MediaController(IPermissionService permissionService, IWebHelper webHelper)
        {
            this._permissionService = permissionService;
            this._webHelper = webHelper;
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

		private UploadFileResult UploadImageInternal()
		{
			if (Request.Files.Count == 0)
			{
				return new UploadFileResult { Message = "No file uploaded" };
			}

			var uploadFile = Request.Files[0];
			if (uploadFile == null)
			{
				return new UploadFileResult { Message = "No file name provided" };
			}

			var fileName = Path.GetFileName(uploadFile.FileName);
			if (fileName.IsEmpty())
			{
				return new UploadFileResult { Message = "No file name provided" };
			}

			var directory = "~/Media/Uploaded/";
			var filePath = Path.Combine(_webHelper.MapPath(directory), fileName);

			if (!IsAllowedImageType(fileName))
			{
				return new UploadFileResult { Message = "Files with extension '{0}' cannot be uploaded".FormatInvariant(Path.GetExtension(filePath)) };
			}

			uploadFile.SaveAs(filePath);

			return new UploadFileResult
			{
				Success = true,
				Url = this.Url.Content(string.Format("{0}{1}", directory, fileName))
			};
		}

		[NonAction]
		protected virtual bool IsAllowedImageType(string path)
		{
			return s_allowedImageTypes.IsMatch(path);
		}

		public class UploadFileResult
		{
			public bool Success { get; set; }
			public string Url { get; set; }
			public string Message { get; set; }
		}

    }
}
