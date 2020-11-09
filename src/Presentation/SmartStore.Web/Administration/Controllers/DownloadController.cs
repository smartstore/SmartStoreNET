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
        private readonly IMediaService _mediaService;
        private readonly MediaSettings _mediaSettings;

        public DownloadController(
            IDownloadService downloadService,
            IMediaService mediaService,
            MediaSettings mediaSettings)
        {
            _downloadService = downloadService;
            _mediaService = mediaService;
            _mediaSettings = mediaSettings;
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
        [ValidateAntiForgeryToken]
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
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Media.Download.Create)]
        public ActionResult CreateDownloadFromMediaFile(int mediaFileId, int entityId = 0, string entityName = "")
        {
            var download = new Download
            {
                EntityId = entityId,
                EntityName = entityName,
                MediaFileId = mediaFileId,
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                IsTransient = true,
                UpdatedOnUtc = DateTime.UtcNow
            };

            _downloadService.InsertDownload(download);

            return Json(new
            {
                success = true,
                downloadId = download.Id,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Media.Download.Create)]
        public ActionResult AsyncUpload(string clientCtrlId, bool minimalMode = false, string fieldName = null, int entityId = 0, string entityName = "")
        {
            var postedFile = Request.ToPostedFileResult();
            if (postedFile == null)
            {
                throw new ArgumentException(T("Common.NoFileUploaded"));
            }

            var path = _mediaService.CombinePaths(SystemAlbumProvider.Downloads, postedFile.FileName);
            var file = _mediaService.SaveFile(path, postedFile.Stream, dupeFileHandling: DuplicateFileHandling.Rename);

            return Json(new
            {
                success = true,
                clientCtrlId,
                id = file.Id,
                name = file.Name,
                type = file.MediaType,
                thumbUrl = _mediaService.GetUrl(file.Id, _mediaSettings.ProductThumbPictureSize, host: string.Empty)
            });
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
