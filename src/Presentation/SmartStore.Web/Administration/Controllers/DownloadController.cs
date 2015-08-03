using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Media;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class DownloadController : AdminControllerBase
    {
        private readonly IDownloadService _downloadService;

        public DownloadController(IDownloadService downloadService)
        {
            this._downloadService = downloadService;
        }

        public ActionResult DownloadFile(int downloadId)
        {
            var download = _downloadService.GetDownloadById(downloadId);
            if (download == null)
                return Content("No download record found with the specified id");

            if (download.UseDownloadUrl)
            {
                return new RedirectResult(download.DownloadUrl);
            }
            else
            {
                //use stored data
                if (download.DownloadBinary == null)
                    return Content(string.Format("Download data is not available any more. Download ID={0}", downloadId));

                string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : downloadId.ToString();
                string contentType = !String.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : "application/octet-stream";
                return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
            }

        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveDownloadUrl(string downloadUrl)
        {
			var download = new Download
			{
				DownloadGuid = Guid.NewGuid(),
				UseDownloadUrl = true,
				DownloadUrl = downloadUrl,
				IsNew = true,
				IsTransient = true
			};
            _downloadService.InsertDownload(download);

            return Json(new { downloadId = download.Id }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AsyncUpload()
        {
			var postedFile = Request.ToPostedFileResult();
			if (postedFile == null)
			{
				throw new ArgumentException("No file uploaded");
			}

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = "",
                DownloadBinary = postedFile.Buffer,
                ContentType = postedFile.ContentType,
                //we store filename without extension for downloads
                Filename = postedFile.FileTitle,
                Extension = postedFile.FileExtension,
                IsNew = true,
				IsTransient = true
            };
            _downloadService.InsertDownload(download);

            return Json(new 
            { 
                success = true, 
                downloadId = download.Id,
                fileName = download.Filename.Truncate(50, "...") + download.Extension,
                downloadUrl = Url.Action("DownloadFile", new { downloadId = download.Id }) 
            });
        }

		[HttpPost]
		public ActionResult DeleteDownload(int downloadId)
		{
			var download = _downloadService.GetDownloadById(downloadId);
			if (download == null)
			{
				NotifyError("No download record found with the specified id");
				return Json(new { success = false });
			}

			_downloadService.DeleteDownload(download);
			return Json(new { success = true });
		}
    }
}
