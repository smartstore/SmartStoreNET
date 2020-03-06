using System.Web.Mvc;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Security;
using SmartStore.Data.Utilities;
using SmartStore.Services.Media;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class PictureController : AdminControllerBase
    {
        private readonly IPictureService _pictureService;
		private readonly MediaSettings _mediaSettings;

		public PictureController(
			IPictureService pictureService,
			MediaSettings mediaSettings)
        {
            _pictureService = pictureService;
			_mediaSettings = mediaSettings;
        }

        [HttpPost]
        [Permission(Permissions.Media.Upload)]
        public ActionResult AsyncUpload(bool isTransient = false, bool validate = true, string album = null)
        {
			var postedFile = Request.ToPostedFileResult();
			if (postedFile == null)
			{
				return Json(new { success = false });
			}
            
            var picture = _pictureService.InsertPicture(
                postedFile.Buffer, 
                postedFile.ContentType, 
                postedFile.FileName,
                isTransient, 
                validate, 
                album);

            return Json(new
			{
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
