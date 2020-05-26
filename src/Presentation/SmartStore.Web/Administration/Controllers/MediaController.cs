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
using System.Dynamic;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class MediaController : AdminControllerBase
    {
        private readonly IMediaService _mediaService;
        private readonly IMediaTypeResolver _mediaTypeResolver;
        private readonly MediaSettings _mediaSettings;

		public MediaController(
            IMediaService mediaService,
            IMediaTypeResolver mediaTypeResolver,
            MediaSettings mediaSettings)
        {
            _mediaService = mediaService;
            _mediaTypeResolver = mediaTypeResolver;
			_mediaSettings = mediaSettings;
        }

        [HttpPost]
        [Permission(Permissions.Media.Upload)]
        public async Task<ActionResult> Upload(string path, string[] acceptedMediaTypes = null, bool isTransient = false, DuplicateFileHandling duplicateFileHandling = DuplicateFileHandling.ThrowError)
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
                        // TODO: (mm) (mc) pass acceptedMediaTypes. It is always null at the moment.
                        var mediaType = _mediaTypeResolver.Resolve(Path.GetExtension(fileName), uploadedFile.ContentType);
                        if (!acceptedMediaTypes.Contains((string)mediaType))
                        {
                            throw new DeniedMediaTypeException(fileName, mediaType, acceptedMediaTypes);
                        }
                    }
                    
                    var mediaFile = await _mediaService.SaveFileAsync(filePath, uploadedFile.InputStream, isTransient, duplicateFileHandling);

                    result.Add(new 
                    {
                        success = true,
                        id = mediaFile.Id,
                        path = mediaFile.Path,
                        url = _mediaService.GetUrl(mediaFile, _mediaSettings.ProductThumbPictureSize, host: string.Empty)
                    });
                }
                catch (Exception ex)
                {
                    var dupe = (ex as DuplicateMediaFileException)?.File;

                    dynamic resultParams = new ExpandoObject();

                    resultParams.success = false;
                    resultParams.path = filePath;
                    resultParams.dupe = ex is DuplicateMediaFileException;
                    resultParams.message = ex.Message;

                    if (dupe != null)
                    {
                        _mediaService.CheckUniqueFileName(filePath, out string newPath);

                        resultParams.newPath = newPath;
                        resultParams.id = dupe.Id;
                        resultParams.url = _mediaService.GetUrl(dupe, _mediaSettings.ProductThumbPictureSize, host: string.Empty);
                        resultParams.date = dupe.CreatedOn.ToString();
                        resultParams.dimensions = dupe.Dimensions.Width + " x " + dupe.Dimensions.Height;
                        resultParams.size = dupe.Size;
                    }
                    
                    result.Add(resultParams);
                }
            }

            // TODO: (mm) (mc) display error notification for every failed file

            return Json(result.Count == 1 ? result[0] : result);
        }

        //[ChildActionOnly]
        [HttpPost]
        public ActionResult DupeFileHandlerDialog()
        {
            return PartialView();
        }

        public ActionResult MoveFsMedia()
		{
			var count = DataMigrator.MoveFsMedia(Services.DbContext);
			return Content("Moved and reorganized {0} media files.".FormatInvariant(count));
		}
    }
}
