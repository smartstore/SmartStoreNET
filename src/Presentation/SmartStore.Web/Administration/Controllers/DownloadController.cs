using System;
using System.Web.Mvc;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Security;
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
            _downloadService = downloadService;
        }

        [Permission(Permissions.Media.Download.Read)]
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

				var fileName = download.MediaFile.Name;
				var contentType = download.MediaFile.MimeType;

                return new FileContentResult(data, contentType)
				{
					FileDownloadName = fileName
				};
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        [Permission(Permissions.Media.Download.Create)]
        public ActionResult SaveDownloadUrl(string downloadUrl, bool minimalMode = false, string fieldName = null, int entityId = 0, string entityName = "")
        {
			var download = new Download
			{
                EntityId = entityId,
                EntityName = entityName,
				DownloadGuid = Guid.NewGuid(),
				UseDownloadUrl = true,
				DownloadUrl = downloadUrl,
				IsTransient = true,
				UpdatedOnUtc = DateTime.UtcNow
			};

            _downloadService.InsertDownload(download);

			return Json(new
			{
				success = true,
				downloadId = download.Id,
				html = this.RenderPartialViewToString(DOWNLOAD_TEMPLATE, download.Id, new { minimalMode = minimalMode, fieldName = fieldName })
			}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Permission(Permissions.Media.Download.Create)]
        public ActionResult AsyncUpload(bool minimalMode = false, string fieldName = null, int entityId = 0, string entityName = "")
        {
			var postedFile = Request.ToPostedFileResult();
			if (postedFile == null)
			{
				throw new ArgumentException(T("Common.NoFileUploaded"));
			}

            // TODO: (mm) migrate
            var download = new Download
            {
                EntityId = entityId,
                EntityName = entityName,
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = string.Empty,
				UpdatedOnUtc = DateTime.UtcNow
            };

            _downloadService.InsertDownload(download, postedFile.Buffer, postedFile.FileName, postedFile.ContentType);

            return Json(new 
            { 
                success = true, 
				downloadId = download.Id,
				html = this.RenderPartialViewToString(DOWNLOAD_TEMPLATE, download.Id, new { minimalMode, fieldName, entityId, entityName })
            });
        }

        [HttpPost]
        [ValidateInput(false)]
        [Permission(Permissions.Media.Download.Update)]
        public ActionResult AddChangelog(int downloadId, string changelogText)
        {
            var success = false;

            var download = _downloadService.GetDownloadById(downloadId);
            if (download != null)
            {
                download.Changelog = changelogText;
                _downloadService.UpdateDownload(download);
                success = true;
            }
            
            return Json(new { success });
        }

        [HttpPost]
        [Permission(Permissions.Media.Download.Read)]
        public ActionResult GetChangelogText(int downloadId)
        {
            var success = false;
            var changeLogText = string.Empty;

            var download = _downloadService.GetDownloadById(downloadId);
            if (download != null)
            {
                changeLogText = download.Changelog;
                success = true;
            }

            return Json(new
            {
                success,
                changelog = changeLogText
            });
        }

        [HttpPost]
        [Permission(Permissions.Media.Download.Delete)]
        public ActionResult DeleteDownload(bool minimalMode = false, string fieldName = null)
		{
			// We don't actually delete here. We just return the editor in it's init state.
			// So the download entity can be set to transient state and deleted later by a scheduled task.
			return Json(new
			{
				success = true,
				html = this.RenderPartialViewToString(DOWNLOAD_TEMPLATE, null, new { minimalMode, fieldName }),
			});
		}
    }
}
