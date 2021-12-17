using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Collections;
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
    [IEEE754Compatible]
    public class MediaFoldersController : WebApiEntityController<MediaFolder, IFolderService>
    {
        private readonly IMediaService _mediaService;

        public MediaFoldersController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        // GET /MediaFolders
        [WebApiQueryable]
        [WebApiAuthenticate]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        // GET /MediaFolders(123)
        [WebApiQueryable]
        [WebApiAuthenticate]
        public IHttpActionResult Get(int key)
        {
            FolderNodeInfo result = null;

            this.ProcessEntity(() =>
            {
                var node = Service.GetNodeById(key);
                result = Convert(node, false)?.FirstOrDefault();
            });

            return Ok(result);
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
            const string infoSetName = "FolderNodeInfos";
            configData.ModelBuilder.EntitySet<FolderNodeInfo>(infoSetName);

            var entityConfig = configData.ModelBuilder.EntityType<MediaFolder>();

            entityConfig.Collection
                .Action("FolderExists")
                .Returns<bool>()
                .Parameter<string>("Path");

            entityConfig.Collection
                .Action("CreateFolder")
                .ReturnsFromEntitySet<FolderNodeInfo>(infoSetName)
                .Parameter<string>("Path");

            entityConfig.Collection
                .Action("MoveFolder")
                .ReturnsFromEntitySet<FolderNodeInfo>(infoSetName)
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

            entityConfig.Collection
                .Function("GetRootNode")
                .ReturnsCollectionFromEntitySet<FolderNodeInfo>(infoSetName);

            entityConfig.Collection
                .Action("GetNodeByPath")
                .ReturnsCollectionFromEntitySet<FolderNodeInfo>(infoSetName)
                .Parameter<string>("Path");
        }

        /// POST /MediaFolders/FolderExists {"Path":"my-folder"}
        [HttpPost]
        [WebApiAuthenticate]
        public IHttpActionResult FolderExists(ODataActionParameters parameters)
        {
            var folderExists = false;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");
                folderExists = _mediaService.FolderExists(path);
            });

            return Ok(folderExists);
        }

        /// POST /MediaFolders/CheckUniqueFolderName {"Path":"content/my-folder"}
        [HttpPost]
        [WebApiAuthenticate]
        public IHttpActionResult CheckUniqueFolderName(ODataActionParameters parameters)
        {
            var result = new CheckUniquenessResult();

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");

                result.Result = Service.CheckUniqueFolderName(path, out string newPath);
                result.NewPath = newPath;
            });

            return Ok(result);
        }

        /// GET /MediaFolders/GetRootNode
        [HttpGet]
        [WebApiAuthenticate]
        public IHttpActionResult GetRootNode()
        {
            List<FolderNodeInfo> result = null;

            this.ProcessEntity(() =>
            {
                var root = Service.GetRootNode();
                result = Convert(root);
            });

            return Ok(result);
        }

        /// POST /MediaFolders/GetNodeByPath {"Path":"content/my-folder"}
        [HttpPost]
        [WebApiAuthenticate]
        public IHttpActionResult GetNodeByPath(ODataActionParameters parameters)
        {
            List<FolderNodeInfo> result = null;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");

                var node = Service.GetNodeByPath(path);
                result = Convert(node);
            });

            return Ok(result);
        }

        /// POST /MediaFolders/CreateFolder {"Path":"content/my-folder"}
        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Media.Update)]
        public IHttpActionResult CreateFolder(ODataActionParameters parameters)
        {
            FolderNodeInfo newFolder = null;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");

                var result = _mediaService.CreateFolder(path);
                newFolder = Convert(result.Node, false)?.FirstOrDefault();
            });

            return Created(newFolder);
        }

        /// POST /MediaFolders/MoveFolder {"Path":"content/my-folder", "DestinationPath":"content/my-renamed-folder"}
        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Media.Update)]
        public IHttpActionResult MoveFolder(ODataActionParameters parameters)
        {
            FolderNodeInfo movedFolder = null;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");
                var destinationPath = parameters.GetValueSafe<string>("DestinationPath");

                var result = _mediaService.MoveFolder(path, destinationPath);
                movedFolder = Convert(result.Node, false)?.FirstOrDefault();
            });

            return Ok(movedFolder);
        }

        /// POST /MediaFolders/CopyFolder {"Path":"content/my-folder", "DestinationPath":"content/my-new-folder"}
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

                var result = _mediaService.CopyFolder(path, destinationPath, duplicateEntryHandling);

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

        /// POST /MediaFolders/DeleteFolder {"Path":"content/my-folder"}
        [HttpPost]
        [WebApiAuthenticate(Permission = Permissions.Media.Delete)]
        public IHttpActionResult DeleteFolder(ODataActionParameters parameters)
        {
            MediaFolderDeleteResult opResult = null;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");
                var fileHandling = parameters.GetValueSafe("FileHandling", FileHandling.SoftDelete);

                var result = _mediaService.DeleteFolder(path, fileHandling);

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

        private List<FolderNodeInfo> Convert(TreeNode<MediaFolderNode> folderNode, bool addChildren = true)
        {
            if (folderNode == null)
            {
                return null;
            }

            var result = new List<FolderNodeInfo>();

            ConvertNode(folderNode);
            return result;

            void ConvertNode(TreeNode<MediaFolderNode> node)
            {
                var val = node.Value;

                var parent = new FolderNodeInfo
                {
                    Id = val.Id,
                    ParentId = val.ParentId,
                    AlbumName = val.AlbumName,
                    Name = val.Name,
                    IsAlbum = val.IsAlbum,
                    Path = val.Path,
                    Slug = val.Slug,
                    HasChildren = node.HasChildren,
                    Children = new List<FolderNodeInfo.FolderChildNodeInfo>()
                };

                if (node.HasChildren)
                {
                    foreach (var child in node.Children)
                    {
                        parent.Children.Add(new FolderNodeInfo.FolderChildNodeInfo
                        {
                            Id = child.Value.Id,
                            Name = child.Value.Name,
                            Path = child.Value.Path
                        });
                    }
                }

                result.Add(parent);

                if (addChildren && node.HasChildren)
                {
                    node.Children.Each(child => ConvertNode(child));
                }
            }
        }

        #endregion
    }
}
