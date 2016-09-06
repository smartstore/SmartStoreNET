using System;
using System.Web.Mvc;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Media;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public class DownloadController : AdminControllerBase
    {
		private const string DOWNLOAD_TEMPLATE = "~/Administration/Views/Shared/EditorTemplates/Download.cshtml";
		
		private readonly IDownloadService _downloadService;

        public DownloadController(IDownloadService downloadService)
        {
            this._downloadService = downloadService;
        }

        public ActionResult DownloadFile(int downloadId)
        {
            var download = _downloadService.GetDownloadById(downloadId);
            if (download == null)
                return Content(T("Common.Download.NoDataAvailable"));

            if (download.UseDownloadUrl)
            {
                return new RedirectResult(download.DownloadUrl);
            }
            else
            {
				//use stored data
				var data = _downloadService.LoadDownloadBinary(download);

				if (data == null || data.LongLength == 0)
					return Content(T("Common.Download.NoDataAvailable"));

				var fileName = (download.Filename.HasValue() ? download.Filename : downloadId.ToString());
				var contentType = (download.ContentType.HasValue() ? download.ContentType : "application/octet-stream");

                return new FileContentResult(data, contentType)
				{
					FileDownloadName = fileName + download.Extension
				};
            }
        }

        [HttpPost]
        [ValidateInput(false)]
		public ActionResult SaveDownloadUrl(string downloadUrl, bool minimalMode = false, string fieldName = null)
        {
			var download = new Download
			{
				DownloadGuid = Guid.NewGuid(),
				UseDownloadUrl = true,
				DownloadUrl = downloadUrl,
				IsNew = true,
				IsTransient = true,
				UpdatedOnUtc = DateTime.UtcNow
			};

            _downloadService.InsertDownload(download, null);

			return Json(new
			{
				success = true,
				downloadId = download.Id,
				html = this.RenderPartialViewToString(DOWNLOAD_TEMPLATE, download.Id, new { minimalMode = minimalMode, fieldName = fieldName })
			}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
		public ActionResult AsyncUpload(bool minimalMode = false, string fieldName = null)
        {
			var postedFile = Request.ToPostedFileResult();
			if (postedFile == null)
			{
				throw new ArgumentException(T("Common.NoFileUploaded"));
			}

			var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = "",
				ContentType = postedFile.ContentType,
                // we store filename without extension for downloads
                Filename = postedFile.FileTitle,
                Extension = postedFile.FileExtension,
                IsNew = true,
				IsTransient = true,
				UpdatedOnUtc = DateTime.UtcNow
			};

            _downloadService.InsertDownload(download, postedFile.Buffer);

            return Json(new 
            { 
                success = true, 
				downloadId = download.Id,
				html = this.RenderPartialViewToString(DOWNLOAD_TEMPLATE, download.Id, new { minimalMode = minimalMode, fieldName = fieldName })
            });
        }

		[HttpPost]
		public ActionResult DeleteDownload(bool minimalMode = false, string fieldName = null)
		{
			// We don't actually delete here. We just return the editor in it's init state
			// so the download entity can be set to transient state and deleted later by a scheduled task.
			return Json(new
			{
				success = true,
				html = this.RenderPartialViewToString(DOWNLOAD_TEMPLATE, null, new { minimalMode = minimalMode, fieldName = fieldName }),
			});
		}
    }
}
