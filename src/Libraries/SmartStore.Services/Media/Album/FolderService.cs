using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using SmartStore.Collections;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public partial class FolderService : ScopedServiceBase, IFolderService
    {
        internal static TimeSpan FolderTreeCacheDuration = TimeSpan.FromHours(3);

        internal const string FolderTreeKey = "mediafolder:tree";

        private readonly IAlbumRegistry _albumRegistry;
        private readonly IRepository<MediaFolder> _folderRepo;
        private readonly ICacheManager _cache;

        public FolderService(
            IAlbumRegistry albumRegistry,
            IRepository<MediaFolder> folderRepo,
            ICacheManager cache)
        {
            _albumRegistry = albumRegistry;
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

            static void AddChildTreeNodes(TreeNode<MediaFolderNode> parentNode, int parentId, Multimap<int, MediaFolderNode> nodeMap)
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

        public bool CheckUniqueFolderName(string path, out string newName)
        {
            Guard.NotEmpty(path, nameof(path));

            // TODO: (mm) (mc) throw when path is not a folder path

            newName = null;

            var node = GetNodeByPath(path);
            if (node == null)
            {
                return false;
            }

            var sourceName = node.Value.Name;
            var names = new HashSet<string>(node.Parent.Children.Select(x => x.Value.Name), StringComparer.OrdinalIgnoreCase);

            int i = 1;
            while (true)
            {
                var test = sourceName + "-" + i;
                if (!names.Contains(test))
                {
                    // Found our gap
                    newName = test;
                    return true;
                }

                i++;
            }
        }

        protected override void OnClearCache()
        {
            _cache.Remove(FolderTreeKey);
        }

        #endregion

        #region Storage

        public MediaFolder GetFolderById(int id, bool withFiles = false)
        {
            if (id <= 0)
                return null;

            if (!withFiles)
            {
                return _folderRepo.GetById(id);
            }

            return _folderRepo.Table.Include(x => x.Files).FirstOrDefault(x => x.Id == id);
        }

        public void InsertFolder(MediaFolder folder)
        {
            Guard.NotNull(folder, nameof(folder));

            _folderRepo.Insert(folder);

            HasChanges = true;
            if (!IsInScope)
            {
                OnClearCache();
            }
        }

        public void DeleteFolder(MediaFolder folder)
        {
            Guard.NotNull(folder, nameof(folder));

            _folderRepo.Delete(folder);

            HasChanges = true;
            if (!IsInScope)
            {
                OnClearCache();
            }
        }

        public void UpdateFolder(MediaFolder folder)
        {
            Guard.NotNull(folder, nameof(folder));

            _folderRepo.Update(folder);

            HasChanges = true;
            if (!IsInScope)
            {
                OnClearCache();
            }
        }

        #endregion

        #region Utils

        protected internal static string NormalizePath(string path, bool forQuery = true)
        {
            path = path.Replace('\\', '/').Trim('/');
            return forQuery ? path.ToLower() : path;
        }

        #endregion
    }
}
