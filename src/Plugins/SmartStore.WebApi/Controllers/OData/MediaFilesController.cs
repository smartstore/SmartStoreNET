using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Security;
using SmartStore.Services.Media;
using SmartStore.Utilities;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.Caching;
using SmartStore.Web.Framework.WebApi.Configuration;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using SmartStore.WebApi.Models.OData.Media;

namespace SmartStore.WebApi.Controllers.OData
{
    /// <remarks>
    /// Some endpoints are accessible via POST where you would expect GET.
    /// That's because a function like GET /MediaFiles/FileExists(Path='content/my-file.jpg') would never work (HTTP status 404).
    /// </remarks>
    [IEEE754Compatible]
    public class MediaFilesController : WebApiEntityController<MediaFile, IMediaService>
    {
        public static MediaLoadFlags _defaultLoadFlags = MediaLoadFlags.AsNoTracking | MediaLoadFlags.WithTags | MediaLoadFlags.WithTracks | MediaLoadFlags.WithFolder;

        // GET /MediaFiles
        [WebApiQueryable]
        [WebApiAuthenticate]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }
        /*public IHttpActionResult Get(ODataQueryOptions<MediaFile> queryOptions)
        {
            IQueryable<MediaFile> query = null;

            var hasClientPaging = Request?.RequestUri?.Query?.Contains("$top=") ?? false;
            if (!hasClientPaging)
            {
                var maxTop = WebApiCachingControllingData.Data().MaxTop;
                var top = Math.Min(this.GetQueryStringValue("$top", maxTop), maxTop);

                query = queryOptions.ApplyTo(GetEntitySet(), new ODataQuerySettings { PageSize = top }) as IQueryable<MediaFile>;
            }
            else
            {
                query = queryOptions.ApplyTo(GetEntitySet()) as IQueryable<MediaFile>;
            }

            var files = query.ToList();
            var result = files.Select(x => Convert(Service.ConvertMediaFile(x)));

            return Ok(result);
        }*/

        // GET /MediaFiles(123)
        [WebApiQueryable]
        [WebApiAuthenticate]
        public IHttpActionResult Get(int key)
        {
            var file = Service.GetFileById(key, _defaultLoadFlags);

            return Ok(Convert(file));
        }

        [WebApiAuthenticate]
        public IHttpActionResult GetProperty()
        {
            // Because of the return object, the method makes little sense here.
            return StatusCode(HttpStatusCode.NotImplemented);
        }

        public IHttpActionResult Post()
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }

        public IHttpActionResult Put()
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }

        public IHttpActionResult Patch()
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }

        public IHttpActionResult Delete()
        {
            // Insufficient endpoint. Parameters required but ODataActionParameters not possible here.
            // Query string parameters less good because not part of the EDM.
            // So use action method "DeleteFile" instead to trigger the corresponding service method.

            return StatusCode(HttpStatusCode.Forbidden);
        }

        #region Actions and functions

        public static void Init(WebApiConfigurationBroadcaster configData)
        {
            const string infoSetName = "FileItemInfos";
            configData.ModelBuilder.EntitySet<FileItemInfo>(infoSetName);

            var entityConfig = configData.ModelBuilder.EntityType<MediaFile>();

            entityConfig.Collection
                .Action("GetFileByPath")
                .ReturnsFromEntitySet<FileItemInfo>(infoSetName)
                .Parameter<string>("Path");

            entityConfig.Collection
                .Function("GetFilesByIds")
                .ReturnsFromEntitySet<FileItemInfo>(infoSetName)
                .CollectionParameter<int>("Ids");

            entityConfig.Collection
                .Function("Download")
                .Returns<StreamContent>()
                .Parameter<int>("Id");

            entityConfig.Collection
                .Action("SearchFiles")
                .ReturnsFromEntitySet<FileItemInfo>(infoSetName)
                .Parameter<MediaSearchQuery>("Query");

            entityConfig.Collection
                .Action("FileExists")
                .Returns<bool>()
                .Parameter<string>("Path");

            entityConfig.Collection
                .Action("CheckUniqueFileName")
                .Returns<CheckUniquenessResult>()
                .Parameter<string>("Path");

            entityConfig.Collection
                .Action("CountFiles")
                .Returns<int>()
                .Parameter<MediaSearchQuery>("Query");

            entityConfig.Collection
                .Action("CountFilesGrouped")
                .Returns<MediaCountResult>()
                .Parameter<MediaFilesFilter>("Filter");

            entityConfig
                .Action("MoveFile")
                .ReturnsFromEntitySet<FileItemInfo>(infoSetName)
                .AddParameter<string>("DestinationFileName")
                .AddParameter<DuplicateFileHandling>("DuplicateFileHandling", true);

            entityConfig
                .Action("CopyFile")
                .Returns<MediaFileOperationResult>()
                .AddParameter<string>("DestinationFileName")
                .AddParameter<DuplicateFileHandling>("DuplicateFileHandling", true);

            entityConfig
                .Action("DeleteFile")
                .AddParameter<bool>("Permanent")
                .AddParameter<bool>("Force", true);

            entityConfig.Collection
                .Action("SaveFile")
                .ReturnsFromEntitySet<FileItemInfo>(infoSetName);
        }

        /// POST /MediaFiles/GetFileByPath {"Path":"content/my-file.jpg"}
        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate]
        public IHttpActionResult GetFileByPath(ODataActionParameters parameters)
        {
            FileItemInfo file = null;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");
                var result = Service.GetFileByPath(path, _defaultLoadFlags);
                file = Convert(result);
            });

            return Ok(file);
        }

        // POST /Media/GetFileByName {"FolderId":2, "FileName":"my-file.jpg"}
        //[HttpPost, WebApiAuthenticate]
        //public MediaFileInfo GetFileByName(ODataActionParameters parameters)
        //{
        //    MediaFileInfo file = null;

        //    this.ProcessEntity(() =>
        //    {
        //        var folderId = parameters.GetValueSafe<int>("FolderId");
        //        var fileName = parameters.GetValueSafe<string>("FileName");

        //        file = _mediaService.GetFileByName(folderId, fileName);
        //        if (file == null)
        //        {
        //            throw this.ExceptionNotFound($"The file with the folder ID {folderId} and file name '{fileName ?? string.Empty}' does not exist.");
        //        }
        //    });

        //    return file;
        //}

        /// GET /MediaFiles/GetFilesByIds(Ids=[1,2,3])
        [HttpGet, WebApiQueryable]
        [WebApiAuthenticate]
        public IHttpActionResult GetFilesByIds([FromODataUri] int[] ids)
        {
            IQueryable<FileItemInfo> files = null;

            this.ProcessEntity(() =>
            {
                if (ids?.Any() ?? false)
                {
                    var mediaFiles = Service.GetFilesByIds(ids.ToArray(), _defaultLoadFlags);

                    files = mediaFiles.Select(x => Convert(x)).AsQueryable();
                }
            });

            return Ok(files ?? new List<FileItemInfo>().AsQueryable());
        }

        /// GET /MediaFiles/Download(Id=123)
        [HttpGet]
        [WebApiAuthenticate]
        public IHttpActionResult Download(int id)
        {
            var file = Service.GetFileById(id);
            if (file == null)
            {
                return NotFound();
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StreamContent(file.OpenRead());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.MimeType);

            if (file.Name.HasValue() &&
                ContentDispositionHeaderValue.TryParse($"inline; filename=\"{PathHelper.SanitizeFileName(file.Name)}\"", out var contentDisposition))
            {
                response.Content.Headers.ContentDisposition = contentDisposition;
            }

            return ResponseMessage(response);
        }

        /// POST /MediaFiles/SearchFiles {"Query":{"FolderId":7,"Extensions":["jpg"], ...}}
        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate]
        public async Task<IHttpActionResult> SearchFiles(ODataActionParameters parameters)
        {
            MediaSearchResult result = null;

            await this.ProcessEntityAsync(async () =>
            {
                var maxTop = WebApiCachingControllingData.Data().MaxTop;
                var query = parameters.GetValueSafe<MediaSearchQuery>("Query") ?? new MediaSearchQuery { PageSize = maxTop };

                query.PageSize = Math.Min(query.PageSize, maxTop);

                result = await Service.SearchFilesAsync(query, _defaultLoadFlags);
            });

            return Ok(result.Select(x => Convert(x)).AsQueryable());
        }

        /// POST /MediaFiles/FileExists {"Path":"content/my-file.jpg"}
        [HttpPost]
        [WebApiAuthenticate]
        public IHttpActionResult FileExists(ODataActionParameters parameters)
        {
            var fileExists = false;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");
                fileExists = Service.FileExists(path);
            });

            return Ok(fileExists);
        }

        /// POST /MediaFiles/CheckUniqueFileName {"Path":"content/my-file.jpg"}
        [HttpPost]
        [WebApiAuthenticate]
        public IHttpActionResult CheckUniqueFileName(ODataActionParameters parameters)
        {
            var result = new CheckUniquenessResult();

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");

                result.Result = Service.CheckUniqueFileName(path, out string newPath);
                result.NewPath = newPath;
            });

            return Ok(result);
        }

        /// POST /MediaFiles/CountFiles {"Query":{"FolderId":7,"Extensions":["jpg"], ...}}
        [HttpPost]
        [WebApiAuthenticate]
        public async Task<IHttpActionResult> CountFiles(ODataActionParameters parameters)
        {
            var count = 0;

            await this.ProcessEntityAsync(async () =>
            {
                var query = parameters.GetValueSafe<MediaSearchQuery>("Query");
                count = await Service.CountFilesAsync(query ?? new MediaSearchQuery());
            });

            return Ok(count);
        }

        /// POST /MediaFiles/CountFilesGrouped {"Filter":{"Term":"my image","Extensions":["jpg"], ...}}
        [HttpPost]
        [WebApiAuthenticate]
        public IHttpActionResult CountFilesGrouped(ODataActionParameters parameters)
        {
            MediaCountResult result = null;

            this.ProcessEntity(() =>
            {
                var query = parameters.GetValueSafe<MediaFilesFilter>("Filter");
                var res = Service.CountFilesGrouped(query ?? new MediaFilesFilter());

                result = new MediaCountResult
                {
                    Total = res.Total,
                    Trash = res.Trash,
                    Unassigned = res.Unassigned,
                    Transient = res.Transient,
                    Orphan = res.Orphan
                };

                result.Folders = res.Folders
                    .Select(x => new MediaCountResult.FolderCount
                    {
                        FolderId = x.Key,
                        Count = x.Value
                    })
                    .ToList();
            });

            return Ok(result);
        }

        /// POST /MediaFiles(123)/MoveFile {"DestinationFileName":"content/updated-file-name.jpg"}
        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Media.Update)]
        public IHttpActionResult MoveFile(int key, ODataActionParameters parameters)
        {
            FileItemInfo movedFile = null;

            this.ProcessEntity(() =>
            {
                var file = Service.GetFileById(key);
                if (file == null)
                {
                    throw Request.NotFoundException(WebApiGlobal.Error.EntityNotFound.FormatInvariant(key));
                }

                var destinationFileName = parameters.GetValueSafe<string>("DestinationFileName");
                var duplicateFileHandling = parameters.GetValueSafe("DuplicateFileHandling", DuplicateFileHandling.ThrowError);

                var result = Service.MoveFile(file.File, destinationFileName, duplicateFileHandling);
                movedFile = Convert(result);
            });

            return Ok(movedFile);
        }

        /// POST /MediaFiles(123)/CopyFile {"DestinationFileName":"content/new-file.jpg"}
        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Media.Update)]
        public IHttpActionResult CopyFile(int key, ODataActionParameters parameters)
        {
            MediaFileOperationResult opResult = null;

            this.ProcessEntity(() =>
            {
                var file = Service.GetFileById(key);
                if (file == null)
                {
                    throw Request.NotFoundException(WebApiGlobal.Error.EntityNotFound.FormatInvariant(key));
                }

                var destinationFileName = parameters.GetValueSafe<string>("DestinationFileName");
                var duplicateFileHandling = parameters.GetValueSafe("DuplicateFileHandling", DuplicateFileHandling.ThrowError);

                var result = Service.CopyFile(file, destinationFileName, duplicateFileHandling);

                opResult = new MediaFileOperationResult
                {
                    DestinationFileId = result.DestinationFile.Id,
                    //DestinationFile = Convert(result.DestinationFile),
                    IsDuplicate = result.IsDuplicate,
                    UniquePath = result.UniquePath
                };
            });

            return Ok(opResult);
        }

        /// POST /MediaFiles(123)/DeleteFile {"Permanent":false}
        [HttpPost]
        [WebApiAuthenticate(Permission = Permissions.Media.Delete)]
        public IHttpActionResult DeleteFile(int key, ODataActionParameters parameters)
        {
            this.ProcessEntity(() =>
            {
                var file = Service.GetFileById(key);
                if (file == null)
                {
                    throw Request.NotFoundException(WebApiGlobal.Error.EntityNotFound.FormatInvariant(key));
                }

                var permanent = parameters.GetValueSafe<bool>("Permanent");
                var force = parameters.GetValueSafe("Force", false);

                Service.DeleteFile(file.File, permanent, force);
            });

            return StatusCode(HttpStatusCode.NoContent);
        }

        /// POST /MediaFiles/SaveFile <multipart data of a single file>
        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Media.Upload)]
        public async Task<IHttpActionResult> SaveFile()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return StatusCode(HttpStatusCode.UnsupportedMediaType);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            FileItemInfo savedFile = null;
            var provider = new MultipartMemoryStreamProvider();

            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            // Get file content.
            var contents = provider.Contents?.Where(x => x.IsFileContent())?.ToList();

            if ((contents?.Count ?? 0) == 0)
            {
                return BadRequest("Missing multipart file data.");
            }
            if (contents.Count > 1)
            {
                return BadRequest("Send one file per request, not multiple.");
            }

            var content = contents.First();
            var cd = content.Headers?.ContentDisposition;

            if (!(cd?.Parameters?.Any() ?? false))
            {
                return BadRequest("Missing file parameters in content-disposition header.");
            }

            await this.ProcessEntityAsync(async () =>
            {
                var path = cd.Parameters.FirstOrDefault(x => x.Name == "Path")?.Value.ToUnquoted();
                var isTransient = cd.Parameters.FirstOrDefault(x => x.Name == "IsTransient")?.Value?.ToUnquoted()?.ToBool(true) ?? true;

                var rawDuplicateFileHandling = cd.Parameters.FirstOrDefault(x => x.Name == "DuplicateFileHandling")?.Value?.ToUnquoted();
                Enum.TryParse<DuplicateFileHandling>(rawDuplicateFileHandling.EmptyNull(), out var duplicateFileHandling);

                using (var stream = await content.ReadAsStreamAsync())
                {
                    var result = await Service.SaveFileAsync(path, stream, isTransient, duplicateFileHandling);
                    savedFile = Convert(result);
                }
            });

            return Ok(savedFile);
        }

        #endregion

        #region Utilities

        private FileItemInfo Convert(MediaFileInfo file)
        {
            if (file != null)
            {
                var item = MiniMapper.Map<MediaFileInfo, FileItemInfo>(file, CultureInfo.InvariantCulture);
                return item;
            }

            return null;
        }

        #endregion
    }
}
