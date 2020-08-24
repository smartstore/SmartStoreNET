using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Microsoft.OData.Core.UriParser.Semantic;
using SmartStore.Collections;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Security;
using SmartStore.Services.Media;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.Caching;
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

        private readonly Lazy<IFolderService> _folderService;

        public MediaController(Lazy<IFolderService> folderService)
        {
            _folderService = folderService;
        }

        // GET /Media
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

        // GET /Media(123)
        [WebApiQueryable]
        [WebApiAuthenticate]
        public IHttpActionResult Get(int key)
        {
            var file = Service.GetFileById(key, _defaultLoadFlags);

            return Ok(Convert(file));
        }

        // GET /Media(123)/ThumbUrl
        [WebApiAuthenticate]
        public IHttpActionResult GetProperty(int key, string propertyName)
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
                return StatusCode(HttpStatusCode.NoContent);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK, propertyType, propertyValue);
            return ResponseMessage(response);
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
            var entityConfig = configData.ModelBuilder.EntityType<FileItemInfo>();

            #region Files

            entityConfig.Collection
                .Action("GetFileByPath")
                .ReturnsFromEntitySet<FileItemInfo>("Media")
                .Parameter<string>("Path");

            //entityConfig.Collection
            //    .Action("GetFileByName")
            //    .ReturnsFromEntitySet<FileItemInfo>("Media")
            //    .AddParameter<string>("FileName")
            //    .AddParameter<int>("FolderId");

            entityConfig.Collection
                .Function("GetFilesByIds")
                .ReturnsFromEntitySet<FileItemInfo>("Media")
                .CollectionParameter<int>("Ids");

            entityConfig.Collection
                .Action("SearchFiles")
                .ReturnsFromEntitySet<FileItemInfo>("Media")
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

            // Doesn't work:
            //var cfgr = configData.ModelBuilder.ComplexType<CountFilesGroupedResult>();
            //cfgr.Property(x => x.Total);
            //cfgr.Property(x => x.Trash);
            //cfgr.Property(x => x.Unassigned);
            //cfgr.Property(x => x.Transient);
            //cfgr.Property(x => x.Orphan);
            //cfgr.ComplexProperty(x => x.Filter);
            //cfgr.HasDynamicProperties(x => x.Folders);

            entityConfig
                .Action("MoveFile")
                .ReturnsFromEntitySet<FileItemInfo>("Media")
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

            entityConfig.Collection
                .Action("MoveFolder")
                .Returns<FolderItemInfo>()
                .AddParameter<string>("Path")
                .AddParameter<string>("DestinationPath");

            entityConfig.Collection
                .Action("CopyFolder")
                .Returns<MediaFolderOperationResult>()
                .AddParameter<string>("Path")
                .AddParameter<string>("DestinationPath")
                .AddParameter<DuplicateEntryHandling>("DuplicateEntryHandling", true);

            entityConfig.Collection
                .Action("DeleteFolder")
                .Returns<MediaFolderDeleteResult>()
                .AddParameter<string>("Path")
                .AddParameter<FileHandling>("FileHandling", true);

            entityConfig.Collection
                .Action("CheckUniqueFolderName")
                .Returns<CheckUniquenessResult>()
                .Parameter<string>("Path");

            #endregion
        }

        /// POST /Media/GetFileByPath {"Path":"content/my-file.jpg"}
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

        /// GET /Media/GetFilesByIds(Ids=[1,2,3])
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

            return Ok(files ??  new List<FileItemInfo>().AsQueryable());
        }

        /// POST /Media/SearchFiles {"Query":{"FolderId":7,"Extensions":["jpg"], ...}}
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

        /// POST /Media/FileExists {"Path":"content/my-file.jpg"}
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

        /// POST /Media/CheckUniqueFileName {"Path":"content/my-file.jpg"}
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

        /// POST /Media/CountFiles {"Query":{"FolderId":7,"Extensions":["jpg"], ...}}
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

        /// POST /Media/CountFilesGrouped {"Filter":{"Term":"my image","Extensions":["jpg"], ...}}
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

        /// POST /Media(123)/MoveFile {"DestinationFileName":"content/updated-file-name.jpg"}
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

        /// POST /Media(123)/CopyFile {"DestinationFileName":"content/new-file.jpg"}
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

        /// POST /Media(123)/DeleteFile {"Permanent":false}
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


        /// POST /Media/FolderExists {"Path":"my-folder"}
        [HttpPost]
        [WebApiAuthenticate]
        public IHttpActionResult FolderExists(ODataActionParameters parameters)
        {
            var folderExists = false;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");
                folderExists = Service.FolderExists(path);
            });

            return Ok(folderExists);
        }

        /// POST /Media/CheckUniqueFolderName {"Path":"content/my-folder"}
        [HttpPost]
        [WebApiAuthenticate]
        public IHttpActionResult CheckUniqueFolderName(ODataActionParameters parameters)
        {
            var result = new CheckUniquenessResult();

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");

                result.Result = _folderService.Value.CheckUniqueFolderName(path, out string newPath);
                result.NewPath = newPath;
            });

            return Ok(result);
        }

        /// POST /Media/CreateFolder {"Path":"content/my-folder"}
        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate]
        public IHttpActionResult CreateFolder(ODataActionParameters parameters)
        {
            FolderItemInfo newFolder = null;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");

                var result = Service.CreateFolder(path);
                newFolder = Convert(result);
            });

            return Created(newFolder);
        }

        /// POST /Media/MoveFolder {"Path":"content/my-folder", "DestinationPath":"content/my-renamed-folder"}
        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Media.Update)]
        public IHttpActionResult MoveFolder(ODataActionParameters parameters)
        {
            FolderItemInfo movedFolder = null;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");
                var destinationPath = parameters.GetValueSafe<string>("DestinationPath");

                var result = Service.MoveFolder(path, destinationPath);
                movedFolder = Convert(result);
            });

            return Ok(movedFolder);
        }

        /// POST /Media/CopyFolder {"Path":"content/my-folder", "DestinationPath":"content/my-new-folder"}
        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Media.Update)]
        public IHttpActionResult CopyFolder(ODataActionParameters parameters)
        {
            MediaFolderOperationResult opResult = null;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");
                var destinationPath = parameters.GetValueSafe<string>("DestinationPath");
                var duplicateEntryHandling = parameters.GetValueSafe("DuplicateEntryHandling", DuplicateEntryHandling.Skip);

                var result = Service.CopyFolder(path, destinationPath, duplicateEntryHandling);

                opResult = new MediaFolderOperationResult
                {
                    FolderId = result.Folder.Id,
                    //Folder = Convert(result.Folder)
                };

                opResult.DuplicateFiles = result.DuplicateFiles
                    .Select(x => new MediaFolderOperationResult.DuplicateFileInfo
                    {
                        SourceFileId = x.SourceFile.Id,
                        DestinationFileId = x.DestinationFile.Id,
                        //SourceFile = Convert(x.SourceFile),
                        //DestinationFile = Convert(x.DestinationFile),
                        UniquePath = x.UniquePath
                    })
                    .ToList();
            });

            return Ok(opResult);
        }

        /// POST /Media/DeleteFolder {"Path":"content/my-folder"}
        [HttpPost]
        [WebApiAuthenticate(Permission = Permissions.Media.Delete)]
        public IHttpActionResult DeleteFolder(ODataActionParameters parameters)
        {
            MediaFolderDeleteResult opResult = null;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");
                var fileHandling = parameters.GetValueSafe("FileHandling", FileHandling.SoftDelete);

                var result = Service.DeleteFolder(path, fileHandling);

                opResult = new MediaFolderDeleteResult
                {
                    DeletedFileNames = result.DeletedFileNames,
                    DeletedFolderIds = result.DeletedFolderIds
                };
            });

            return Ok(opResult);
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

        private FolderItemInfo Convert(MediaFolderInfo folder)
        {
            if (folder != null)
            {
                var item = MiniMapper.Map<MediaFolderInfo, FolderItemInfo>(folder, CultureInfo.InvariantCulture);
                return item;
            }

            return null;
        }

        private FolderNodeInfo Convert(TreeNode<MediaFolderNode> folderNode)
        {
            if (folderNode != null)
            {
                var result = ConvertNode(folderNode);
                return result;
            }

            return null;

            FolderNodeInfo ConvertNode(TreeNode<MediaFolderNode> node)
            {
                var val = node.Value;

                var item = new FolderNodeInfo
                {
                    Id = val.Id,
                    ParentId = val.ParentId,
                    AlbumName = val.AlbumName,
                    Name = val.Name,
                    IsAlbum = val.IsAlbum,
                    Path = val.Path,
                    Slug = val.Slug,
                    Children = new List<FolderNodeInfo>(),
                };

                foreach (var child in node.Children)
                {
                    item.Children.Add(ConvertNode(child));
                }

                return item;
            }
        }

        #endregion
    }
}
