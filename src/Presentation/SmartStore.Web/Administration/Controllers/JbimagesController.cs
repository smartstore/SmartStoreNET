using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Services.Security;

namespace SmartStore.Admin.Controllers
{

	public partial class JbimagesController : AdminControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;

        public JbimagesController(IPermissionService permissionService, IWebHelper webHelper)
        {
            this._permissionService = permissionService;
            this._webHelper = webHelper;
        }

        [HttpPost]
        public ActionResult Upload()
        {
            if (Request.Files.Count == 0)
                throw new Exception("No file uploaded");

            var uploadFile = Request.Files[0];
            if (uploadFile == null)
            {
                ViewData["ResultCode"] = "failed";
                ViewData["Result"] = "No file name provided";
                return View();
            }

            var fileName = Path.GetFileName(uploadFile.FileName);
            if (fileName.IsEmpty())
            {
                ViewData["ResultCode"] = "failed";
                ViewData["Result"] = "No file name provided";
                return View();
            }

            var directory = "~/Media/Uploaded/";
            var filePath = Path.Combine(_webHelper.MapPath(directory), fileName);

            var fileExtension = Path.GetExtension(filePath);
            if (!GetAllowedFileTypes().Contains(fileExtension))
            {
                ViewData["ResultCode"] = "failed";
                ViewData["Result"] = string.Format("Files with {0} extension cannot be uploaded", fileExtension);
                return View();
            }

            uploadFile.SaveAs(filePath);

            ViewData["ResultCode"] = "success";
            ViewData["Result"] = "success";
            ViewData["FileName"] = this.Url.Content(string.Format("{0}{1}", directory, fileName));
            return View();
        }

		[NonAction]
		protected IList<string> GetAllowedFileTypes()
		{
			return new List<string>() { ".gif", ".jpg", ".jpeg", ".png", ".bmp", ".ico", ".svg" };
		}
    }
}
