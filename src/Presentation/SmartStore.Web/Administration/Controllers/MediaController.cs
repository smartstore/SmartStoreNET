using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Admin.Controllers
{

	public partial class MediaController : AdminControllerBase
    {
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
			var postedFile = Request.ToPostedFileResult();
			if (postedFile == null)
			{
				return new UploadFileResult { Message = "No file uploaded" };
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
