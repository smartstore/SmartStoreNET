using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Security;
using SmartStore.Services.Media;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.Configuration;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using SmartStore.WebApi.Models.OData.Media;

namespace SmartStore.WebApi.Controllers.OData
{
    /// <summary>
    /// Is intended to make methods of the IMediaService accessible. Direct access to the MediaFile entity is not intended.
    /// </summary>
    /// <remarks>
    /// Functions like GET /Media/FileExists(Path='content/my-file.jpg') would never work (404).
    /// That's why some endpoints are implemented as Actions (POST).
    /// </remarks>
    public class MediaController : WebApiEntityController<MediaFile, IMediaService>
    {
        public static MediaLoadFlags _defaultLoadFlags = MediaLoadFlags.AsNoTracking | MediaLoadFlags.WithTags | MediaLoadFlags.WithTracks | MediaLoadFlags.WithFolder;

        // GET /Media(123)
        [WebApiQueryable]
        [WebApiAuthenticate]
        public SingleResult<FileItemInfo> Get(int key)
        {
            var file = Service.GetFileById(key, _defaultLoadFlags);
            if (file == null)
            {
                throw Request.NotFoundException(WebApiGlobal.Error.EntityNotFound.FormatInvariant(key));
            }

            return SingleResult.Create(new[] { Convert(file) }.AsQueryable());
        }

        // GET /Media
        [WebApiQueryable]
        [WebApiAuthenticate]
        public IQueryable<FileItemInfo> Get(/*ODataQueryOptions<MediaFile> queryOptions*/)
        {
            throw new HttpResponseException(HttpStatusCode.NotImplemented);

            // TODO or not TODO :)
            //var maxTop = WebApiCachingControllingData.Data().MaxTop;
            //var top = Math.Min(this.GetQueryStringValue("$top", maxTop), maxTop);

            //var query = queryOptions.ApplyTo(GetEntitySet(), new ODataQuerySettings { PageSize = top }) as IQueryable<MediaFile>;
            //var files = query.ToList();
            //var result = files.Select(x => Convert(Service.ConvertMediaFile(x)));

            //return result.AsQueryable();
        }

        // GET /Media(123)/ThumbUrl
        [WebApiAuthenticate]
        public HttpResponseMessage GetProperty(int key, string propertyName)
        {
            Type propertyType = null;
            object propertyValue = null;

            this.ProcessEntity(() =>
            {
                var file = Service.GetFileById(key);
                if (file == null)
                {
                    throw Request.NotFoundException(WebApiGlobal.Error.EntityNotFound.FormatInvariant(key));
                }

                var item = Convert(file);

                var prop = FastProperty.GetProperty(item.GetType(), propertyName);
                if (prop == null)
                {
                    throw Request.BadRequestException(WebApiGlobal.Error.PropertyNotFound.FormatInvariant(propertyName.EmptyNull()));
                }

                propertyType = prop.Property.PropertyType;
                propertyValue = prop.GetValue(item);
            });

            if (propertyType == null)
            {
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }

            return Request.CreateResponse(HttpStatusCode.OK, propertyType, propertyValue);
        }

        public IHttpActionResult Post(MediaFile entity)
        {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
        }

        public IHttpActionResult Put(int key, MediaFile entity)
        {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
        }

        public IHttpActionResult Patch(int key, Delta<MediaFile> model)
        {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
        }

        [WebApiAuthenticate(Permission = Permissions.Media.Delete)]
        public IHttpActionResult Delete(int key)
        {
            this.ProcessEntity(() =>
            {
                var file = Service.GetFileById(key);
                if (file == null)
                {
                    throw Request.NotFoundException(WebApiGlobal.Error.EntityNotFound.FormatInvariant(key));
                }

                // Get options from query string. 
                // FromODataUri (404) and FromBody ("Can't bind multiple parameters") are not possible here.
                var permanent = this.GetQueryStringValue("permanent", false);
                var force = this.GetQueryStringValue("force", false);

                Service.DeleteFile(file.File, permanent, force);
            });

            return StatusCode(HttpStatusCode.NoContent);
        }

        #region Actions and functions

        public static void Init(WebApiConfigurationBroadcaster configData)
        {
            var entityConfig = configData.ModelBuilder.EntityType<FileItemInfo>();

            #region Files

            entityConfig.Collection
                .Action("GetFileByPath")
                .ReturnsFromEntitySet<FileItemInfo>("Media")
                .Parameter<string>("Path");

            //var getFileByName = entityConfig.Collection
            //    .Action("GetFileByName")
            //    .ReturnsFromEntitySet<FileItemInfo>("Media");
            //getFileByName.Parameter<string>("FileName");
            //getFileByName.Parameter<int>("FolderId");

            entityConfig.Collection
                .Function("GetFilesByIds")
                .ReturnsFromEntitySet<FileItemInfo>("Media")
                .CollectionParameter<int>("Ids");

            entityConfig.Collection
                .Action("FileExists")
                .Returns<bool>()
                .Parameter<string>("Path");

            entityConfig.Collection
                .Action("CheckUniqueFileName")
                .Returns<CheckUniqueFileNameResult>()
                .Parameter<string>("Path");

            entityConfig.Collection
                .Action("CountFiles")
                .Returns<int>()
                .Parameter<MediaSearchQuery>("Query");

            entityConfig.Collection
                .Action("CountFilesGrouped")
                .Returns<CountFilesGroupedResult>()
                .Parameter<MediaFilesFilter>("Filter");

            // Doesn't work:
            //var cfgr = configData.ModelBuilder.ComplexType<CountFilesGroupedResult>();
            //cfgr.Property(x => x.Total);
            //cfgr.Property(x => x.Trash);
            //cfgr.Property(x => x.Unassigned);
            //cfgr.Property(x => x.Transient);
            //cfgr.Property(x => x.Orphan);
            //cfgr.ComplexProperty(x => x.Filter);
            //cfgr.HasDynamicProperties(x => x.Folders);

            var moveFile = entityConfig
                .Action("MoveFile")
                .ReturnsFromEntitySet<FileItemInfo>("Media");

            moveFile.Parameter<string>("DestinationFileName");
            var dph1 = moveFile.Parameter<DuplicateFileHandling>("DuplicateFileHandling");
            dph1.OptionalParameter = true;

            var copyFile = entityConfig
                .Action("CopyFile")
                .ReturnsFromEntitySet<FileItemInfo>("Media");

            copyFile.Parameter<string>("DestinationFileName");
            var dph2 = copyFile.Parameter<DuplicateFileHandling>("DuplicateFileHandling");
            dph2.OptionalParameter = true;

            #endregion

            #region Folders

            entityConfig.Collection
                .Action("FolderExists")
                .Returns<bool>()
                .Parameter<string>("Path");

            entityConfig.Collection
                .Action("CreateFolder")
                .Returns<FolderItemInfo>()
                .Parameter<string>("Path");

            #endregion
        }

        /// POST /Media/GetFileByPath {"Path":"content/my-file.jpg"}
        [HttpPost]
        [WebApiAuthenticate]
        public FileItemInfo GetFileByPath(ODataActionParameters parameters)
        {
            FileItemInfo file = null;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");
                var mediaFile = Service.GetFileByPath(path, _defaultLoadFlags);

                if (mediaFile == null)
                {
                    throw Request.NotFoundException($"The file with the path '{path ?? string.Empty}' does not exist.");
                }

                file = Convert(mediaFile);
            });

            return file;
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

        /// GET /Media/GetFilesByIds(Ids=[1,2,3])
        [HttpGet, WebApiQueryable]
        [WebApiAuthenticate]
        public IQueryable<FileItemInfo> GetFilesByIds([FromODataUri] int[] ids)
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

            return files ??  new List<FileItemInfo>().AsQueryable();
        }

        /// POST /Media/FileExists {"Path":"content/my-file.jpg"}
        [HttpPost]
        [WebApiAuthenticate]
        public bool FileExists(ODataActionParameters parameters)
        {
            var fileExists = false;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");
                fileExists = Service.FileExists(path);
            });

            return fileExists;
        }

        /// POST /Media/CheckUniqueFileName {"Path":"content/my-file.jpg"}
        [HttpPost]
        [WebApiAuthenticate]
        public CheckUniqueFileNameResult CheckUniqueFileName(ODataActionParameters parameters)
        {
            var result = new CheckUniqueFileNameResult();

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");

                result.Result = Service.CheckUniqueFileName(path, out string newPath);
                result.NewPath = newPath;
            });

            return result;
        }

        /// POST /Media/CountFiles {"Query":{"FolderId":7,"Extensions":["jpg"], ...}}
        [HttpPost]
        [WebApiAuthenticate]
        public async Task<int> CountFiles(ODataActionParameters parameters)
        {
            var count = 0;

            await this.ProcessEntityAsync(async () =>
            {
                var query = parameters.GetValueSafe<MediaSearchQuery>("Query");
                count = await Service.CountFilesAsync(query ?? new MediaSearchQuery());
            });

            return count;
        }

        /// POST /Media/CountFilesGrouped {"Filter":{"Term":"my image","Extensions":["jpg"], ...}}
        [HttpPost]
        [WebApiAuthenticate]
        public CountFilesGroupedResult CountFilesGrouped(ODataActionParameters parameters)
        {
            CountFilesGroupedResult result = null;

            this.ProcessEntity(() =>
            {
                var query = parameters.GetValueSafe<MediaFilesFilter>("Filter");
                var res = Service.CountFilesGrouped(query ?? new MediaFilesFilter());

                result = new CountFilesGroupedResult
                {
                    Total = res.Total,
                    Trash = res.Trash,
                    Unassigned = res.Unassigned,
                    Transient = res.Unassigned,
                    Orphan = res.Orphan,
                    Filter = res.Filter
                };

                result.Folders = res.Folders
                    .Select(x => new CountFilesGroupedResult.FolderCount
                    {
                        FolderId = x.Key,
                        Count = x.Value
                    })
                    .ToList();
            });

            return result;
        }

        /// POST /Media(123)/MoveFile {"DestinationFileName":"content/updated-file-name.jpg"}
        [HttpPost]
        [WebApiAuthenticate(Permission = Permissions.Media.Update)]
        public FileItemInfo MoveFile(int key, ODataActionParameters parameters)
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

            return movedFile;
        }

        /// POST /Media(123)/CopyFile {"DestinationFileName":"content/new-file.jpg"}
        [HttpPost]
        [WebApiAuthenticate(Permission = Permissions.Media.Update)]
        public FileItemInfo CopyFile(int key, ODataActionParameters parameters)
        {
            FileItemInfo fileCopy = null;

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
                fileCopy = Convert(result.DestinationFile);
            });

            return fileCopy;
        }


        /// POST /Media/FolderExists {"Path":"my-folder"}
        [HttpPost]
        [WebApiAuthenticate]
        public bool FolderExists(ODataActionParameters parameters)
        {
            var folderExists = false;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");
                folderExists = Service.FolderExists(path);
            });

            return folderExists;
        }

        /// POST /Media/CreateFolder {"Path":"content/my-folder"}
        [HttpPost]
        [WebApiAuthenticate]
        public HttpResponseMessage CreateFolder(ODataActionParameters parameters)
        {
            FolderItemInfo newFolder = null;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");

                var result = Service.CreateFolder(path);
                newFolder = Convert(result);
            });

            return Request.CreateResponse(HttpStatusCode.Created, newFolder);
        }


        #endregion

        #region Utilities

        private FileItemInfo Convert(MediaFileInfo file)
        {
            var item = MiniMapper.Map<MediaFileInfo, FileItemInfo>(file, CultureInfo.InvariantCulture);
            return item;
        }

        private FolderItemInfo Convert(MediaFolderInfo folder)
        {
            var item = MiniMapper.Map<MediaFolderInfo, FolderItemInfo>(folder, CultureInfo.InvariantCulture);
            return item;
        }

        #endregion
    }


    //   public class PicturesController : WebApiEntityController<MediaFile, IMediaService>
    //{
    //       protected override IQueryable<MediaFile> GetEntitySet()
    //       {
    //           var query =
    //               from x in Repository.Table
    //               where !x.Deleted && !x.Hidden
    //               select x;

    //           return query;
    //       }

    //	[WebApiQueryable]
    //       public SingleResult<MediaFile> GetPicture(int key)
    //	{
    //		return GetSingleResult(key);
    //	}

    //	[WebApiQueryable]
    //       public IQueryable<ProductMediaFile> GetProductPictures(int key)
    //	{
    //		return GetRelatedCollection(key, x => x.ProductMediaFiles);
    //	}
    //}
}
