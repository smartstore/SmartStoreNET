using System.Web.Mvc;
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

        public PictureController(IPictureService pictureService,
             IPermissionService permissionService)
        {
            this._pictureService = pictureService;
            this._permissionService = permissionService;
        }

        [HttpPost]
        public ActionResult AsyncUpload(bool isTransient = false, bool validate = true)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.UploadPictures))
                return Json(new { success = false, error = T("Admin.AccessDenied.Description") });

			var postedFile = Request.ToPostedFileResult();

			var picture = _pictureService.InsertPicture(postedFile.Buffer, postedFile.ContentType, null, true, isTransient, validate);

            return Json(
                new { 
                    success = true, 
                    pictureId = picture.Id,
                    imageUrl = _pictureService.GetPictureUrl(picture, 100) 
                });
        }
    }
}
