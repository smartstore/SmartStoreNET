using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Security;
using SmartStore.Data.Utilities;
using SmartStore.Services.Media;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Newtonsoft.Json.Linq;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class MediaController : AdminControllerBase
    {
        private readonly IMediaService _mediaService;
        private readonly IMediaTypeResolver _mediaTypeResolver;
        private readonly MediaSettings _mediaSettings;
        private readonly MediaExceptionFactory _exceptionFactory;

        public MediaController(
            IMediaService mediaService,
            IMediaTypeResolver mediaTypeResolver,
            MediaSettings mediaSettings,
            MediaExceptionFactory exceptionFactory)
        {
            _mediaService = mediaService;
            _mediaTypeResolver = mediaTypeResolver;
			_mediaSettings = mediaSettings;
            _exceptionFactory = exceptionFactory;
        }

        [HttpPost]
        [Permission(Permissions.Media.Upload)]
        [MaxMediaFileSize]
        public async Task<ActionResult> Upload(string path, string[] typeFilter = null, bool isTransient = false, DuplicateFileHandling duplicateFileHandling = DuplicateFileHandling.ThrowError)
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
                    // Check if media type or file extension is allowed.
                    var extension = Path.GetExtension(fileName).TrimStart('.').ToLower();
                    if (typeFilter != null)
                    {
                        var mediaTypeExtensions = _mediaTypeResolver.ParseTypeFilter(typeFilter);
                        
                        if (!mediaTypeExtensions.Contains(extension))
                        {
                            throw _exceptionFactory.DeniedMediaType(fileName, extension, typeFilter);
                        }
                    }
                    else
                    {
                        // Check if extension is allowed by media settings.
                        if (!_mediaSettings.DocumentTypes.Contains(extension) &&
                            !_mediaSettings.ImageTypes.Contains(extension) &&
                            !_mediaSettings.VideoTypes.Contains(extension) &&
                            !_mediaSettings.AudioTypes.Contains(extension) &&
                            !_mediaSettings.TextTypes.Contains(extension))      // TODO: BinaryTypes
                        {
                            throw _exceptionFactory.DeniedMediaType(fileName, extension);
                        }
                    }
                    
                    var mediaFile = await _mediaService.SaveFileAsync(filePath, uploadedFile.InputStream, isTransient, duplicateFileHandling);

                    dynamic o = JObject.FromObject(mediaFile);
                    o.success = true;
                    o.url = _mediaService.GetUrl(mediaFile, _mediaSettings.ProductThumbPictureSize, host: string.Empty);
                    o.createdOn = mediaFile.CreatedOn.ToString();
                    o.lastUpdated = mediaFile.LastUpdated.ToString();

                    result.Add(o);
                }
                catch (Exception ex)
                {
                    if (ex is DeniedMediaTypeException)
                        throw;

                    var dupe = (ex as DuplicateMediaFileException)?.File;

                    dynamic o = dupe != null ? JObject.FromObject(dupe) : new JObject();
                    o.success = false;
                    o.dupe = ex is DuplicateMediaFileException;
                    o.message = ex.Message;             // TODO: rename to errMessage

                    if (dupe != null)
                    {
                        _mediaService.CheckUniqueFileName(filePath, out string newPath);
                        
                        o.newPath = newPath;
                        o.url = _mediaService.GetUrl(dupe, _mediaSettings.ProductThumbPictureSize, host: string.Empty);
                        o.createdOn = dupe.CreatedOn.ToString();
                        o.lastUpdated = dupe.LastUpdated.ToString();
                        o.dimensions = dupe.Dimensions.Width + " x " + dupe.Dimensions.Height;
                    }

                    result.Add(o);
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
