using System.Web.Mvc;
using SmartStore.Core.Domain.Media;
using SmartStore.Data.Utilities;
using SmartStore.Services.Media;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public class PictureController : AdminControllerBase
    {
        private readonly IPictureService _pictureService;
        private readonly IPermissionService _permissionService;
		private readonly MediaSettings _mediaSettings;

		public PictureController(
			IPictureService pictureService,
            IPermissionService permissionService,
			MediaSettings mediaSettings)
        {
            _pictureService = pictureService;
            _permissionService = permissionService;
			_mediaSettings = mediaSettings;
        }

        [HttpPost]
        public ActionResult AsyncUpload(bool isTransient = false, bool validate = true)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.UploadPictures))
                return Json(new { success = false, error = T("Admin.AccessDenied.Description") });

			var postedFile = Request.ToPostedFileResult();
			if (postedFile == null)
			{
				return Json(new { success = false });
			}

			var picture = _pictureService.InsertPicture(postedFile.Buffer, postedFile.ContentType, null, true, isTransient, validate);
            return Json(
                new { 
                    success = true, 
                    pictureId = picture.Id,
                    imageUrl = _pictureService.GetUrl(picture, _mediaSettings.ProductThumbPictureSize, host: "") 
                });
        }

		public ActionResult MoveFsMedia()
		{
			var count = DataMigrator.MoveFsMedia(Services.DbContext);
			return Content("Moved and reorganized {0} media files.".FormatInvariant(count));
		}
    }
}
