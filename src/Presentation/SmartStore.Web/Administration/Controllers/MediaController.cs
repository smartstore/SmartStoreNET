using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
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
    public class MediaController : AdminControllerBase
    {
        private readonly IPictureService _pictureService;
        private readonly IMediaService _mediaService;
        private readonly IMediaTypeResolver _mediaTypeResolver;
        private readonly MediaSettings _mediaSettings;

		public MediaController(
			IPictureService pictureService,
            IMediaService mediaService,
            IMediaTypeResolver mediaTypeResolver,
            MediaSettings mediaSettings)
        {
            _pictureService = pictureService;
            _mediaService = mediaService;
            _mediaTypeResolver = mediaTypeResolver;
			_mediaSettings = mediaSettings;
        }

        [HttpPost]
        [Permission(Permissions.Media.Upload)]
        public async Task<ActionResult> Upload(string path, string[] acceptedMediaTypes = null, bool isTransient = false)
        {
            var len = Request.Files.Count;
            var result = new List<object>(len);

            for (var i = 0; i < len; ++i)
            {
                var uploadedFile = Request.Files[i];
                var fileName = uploadedFile.FileName;
                var filePath = _mediaService.CombinePaths(path, fileName);

                try
                {
                    if (acceptedMediaTypes != null)
                    {
                        // TODO: (mm) pass acceptedMediaTypes. It is always null at the moment.
                        var mediaType = _mediaTypeResolver.Resolve(Path.GetExtension(fileName), uploadedFile.ContentType);
                        if (!acceptedMediaTypes.Contains((string)mediaType))
                        {
                            throw new DeniedMediaTypeException(fileName, mediaType, acceptedMediaTypes);
                        }
                    }
                    
                    var mediaFile = await _mediaService.SaveFileAsync(filePath, uploadedFile.InputStream, isTransient);

                    result.Add(new 
                    {
                        success = true,
                        fileId = mediaFile.Id,
                        path = mediaFile.Path,
                        url = _mediaService.GetUrl(mediaFile, _mediaSettings.ProductThumbPictureSize, host: string.Empty)
                    });
                }
                catch (Exception ex)
                {
                    result.Add(new
                    {
                        success = false,
                        path = filePath,
                        dupe = ex is DuplicateMediaFileException,
                        message = ex.Message
                    });
                }
            }

            // TODO: (mm) display error notification for every failed file

            return Json(result.Count == 1 ? result[0] : result);
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
                fileId = picture.Id,
                url = _pictureService.GetUrl(picture, _mediaSettings.ProductThumbPictureSize, host: string.Empty) 
            });
        }

		public ActionResult MoveFsMedia()
		{
			var count = DataMigrator.MoveFsMedia(Services.DbContext);
			return Content("Moved and reorganized {0} media files.".FormatInvariant(count));
		}
    }
}
