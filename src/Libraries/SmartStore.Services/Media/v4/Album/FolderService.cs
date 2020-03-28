using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Collections;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Services.Media
{
    public partial class FolderService : IFolderService
    {
        internal static TimeSpan FolderTreeCacheDuration = TimeSpan.FromHours(3);

        internal const string FolderTreeKey = "mediafolder:tree";

        private readonly IAlbumRegistry _albumRegistry;
        //private readonly IRepository<MediaAlbum> _albumRepo;
        private readonly IRepository<MediaFolder> _folderRepo;
        private readonly ICacheManager _cache;

        public FolderService(
            IAlbumRegistry albumRegistry,
            //IRepository<MediaAlbum> albumRepo,
            IRepository<MediaFolder> folderRepo,
            ICacheManager cache)
        {
            _albumRegistry = albumRegistry;
            //_albumRepo = albumRepo;
            _folderRepo = folderRepo;
            _cache = cache;
        }

        #region Cached nodes

        public TreeNode<MediaFolderNode> GetRootNode()
        {
            var cacheKey = FolderTreeKey;

            var root = _cache.Get(cacheKey, () => 
            {
                var query = from x in _folderRepo.TableUntracked
                            orderby x.ParentId, x.Name
                            select x;

                var unsortedNodes = query.ToList().Select(x => 
                {
                    var item = new MediaFolderNode
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ParentId = x.ParentId,
                        CanDetectTracks = x.CanDetectTracks,
                        FilesCount = x.FilesCount,
                        Slug = x.Slug
                    };

                    if (x is MediaAlbum album)
                    {
                        item.IsAlbum = true;
                        item.AlbumName = album.Name;
                        item.Path = album.Name;
                        item.ResKey = album.ResKey;
                        item.IncludePath = album.IncludePath;
                        item.Order = album.Order ?? 0;

                        var albumInfo = _albumRegistry.GetAlbumByName(album.Name);
                        if (albumInfo != null)
                        {
                            var displayHint = albumInfo.DisplayHint;
                            item.Color = displayHint.Color;
                            item.OverlayColor = displayHint.OverlayColor;
                            item.OverlayIcon = displayHint.OverlayIcon;
                        }
                    }

                    return item;
                });

                var nodeMap = unsortedNodes.ToMultimap(x => x.ParentId ?? 0, x => x);
                var rootNode = new TreeNode<MediaFolderNode>(new MediaFolderNode { Name = "Root", Id = 0 });

                AddChildTreeNodes(rootNode, 0, nodeMap);

                return rootNode;
            }, FolderTreeCacheDuration);

            return root;

            /*static*/ void AddChildTreeNodes(TreeNode<MediaFolderNode> parentNode, int parentId, Multimap<int, MediaFolderNode> nodeMap)
            {
                var parent = parentNode?.Value;
                if (parent == null)
                {
                    return;
                }

                var nodes = Enumerable.Empty<MediaFolderNode>();

                if (nodeMap.ContainsKey(parentId))
                {
                    nodes = parentId == 0
                        ? nodeMap[parentId].OrderBy(x => x.Order)
                        : nodeMap[parentId].OrderBy(x => x.Name);
                }

                foreach (var node in nodes)
                {
                    var newNode = new TreeNode<MediaFolderNode>(node);

                    // Inherit some props from parent node
                    if (!node.IsAlbum)
                    {
                        node.AlbumName = parent.AlbumName;
                        node.CanDetectTracks = parent.CanDetectTracks;
                        node.IncludePath = parent.IncludePath;
                        node.Path = (parent.Path + "/" + (node.Slug.NullEmpty() ?? node.Name)).Trim('/').ToLower();
                    }

                    // We gonna query nodes by path also, therefore we need 2 keys per node (FolderId and computed path)
                    newNode.Id = new object[] { node.Id, node.Path };

                    parentNode.Append(newNode);

                    AddChildTreeNodes(newNode, node.Id, nodeMap);
                }
            }
        }

        public TreeNode<MediaFolderNode> GetNodeById(int id)
        {
            if (id <= 0)
                return null;
            
            return GetRootNode().SelectNodeById(id);
        }

        public TreeNode<MediaFolderNode> GetNodeByPath(string path)
        {
            Guard.NotEmpty(path, nameof(path));
            return GetRootNode().SelectNodeById(NormalizePath(path));
        }

        public void ClearCache()
        {
            _cache.Remove(FolderTreeKey);
        }

        private string NormalizePath(string path)
        {
            return path.Replace('\\', '/').Trim('/').ToLower();
        }

        #endregion

        #region Storage

        public bool FolderExists(string path)
        {
            return GetNodeByPath(path) != null;
        }

        public MediaFolder GetFolderById(int id)
        {
            if (id <= 0)
                return null;

            return _folderRepo.GetById(id);
        }

        public MediaFolderInfo InsertFolder(MediaFolder folder)
        {
            Guard.NotNull(folder, nameof(folder));

            _folderRepo.Insert(folder);
            ClearCache();

            return new MediaFolderInfo(GetNodeById(folder.Id));
        }

        public void UpdateFolder(MediaFolder folder)
        {
            Guard.NotNull(folder, nameof(folder));

            _folderRepo.Update(folder);
            ClearCache();
        }

        public void DeleteFolder(MediaFolder folder)
        {
            Guard.NotNull(folder, nameof(folder));

            _folderRepo.Delete(folder);
            ClearCache();
        }

        public MediaFolderInfo MoveFolder(MediaFolder folder, string destinationPath)
        {
            Guard.NotNull(folder, nameof(folder));
            Guard.NotEmpty(destinationPath, nameof(destinationPath));

            var node = GetNodeById(folder.Id);
            if (node.Value.IsAlbum)
            {
                throw new NotSupportedException($"Moving or renaming root album folders is not supported. Folder: {node.Value.Name}.");
            }

            if (destinationPath.IndexOfAny(new[] { '/', '\\' }) == -1)
            {
                // Destination cannot be an album
                throw new ArgumentException("Invalid destination path specification '" + destinationPath + "'.");
            }

            // Destination must not exist
            if (FolderExists(destinationPath))
            {
                throw new ArgumentException("Folder '" + destinationPath + "' already exists.");
            }

            var destParent = NormalizePath(Path.GetDirectoryName(destinationPath));

            // Get destination parent
            var destParentNode = GetNodeByPath(destParent);
            if (destParentNode == null)
            {
                throw new MediaFolderNotFoundException(destinationPath);
            }

            // Cannot move outside source album
            if (!this.AreInSameAlbum(folder.Id, destParentNode.Value.Id))
            {
                throw new NotSameAlbumException(node.Value.Path, destParent);
            }

            if (destParentNode.IsDescendantOf(node))
            {
                throw new ArgumentException("Destination folder '" + destinationPath + "' is not allowed to be a descendant of source folder '" + node.Value.Path + "'.");
            }

            // Set new values
            folder.ParentId = destParentNode.Value.Id;
            folder.Name = Path.GetFileName(destinationPath);

            // Commit
            _folderRepo.Update(folder);

            // TODO: (mm) Clear image cache for all files contained within folder.

            ClearCache();

            return new MediaFolderInfo(GetNodeById(folder.Id));
        }

        #endregion
    }
}
