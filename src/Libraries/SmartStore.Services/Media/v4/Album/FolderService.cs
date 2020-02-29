using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
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
        private readonly IRepository<MediaAlbum> _albumRepository;
        private readonly IRepository<MediaFolder> _folderRepository;
        private readonly ICacheManager _cache;

        public FolderService(
            IAlbumRegistry albumRegistry,
            IRepository<MediaAlbum> albumRepository,
            IRepository<MediaFolder> folderRepository,
            ICacheManager cache,
            IEnumerable<Lazy<IAlbumProvider>> albumProviders)
        {
            _albumRegistry = albumRegistry;
            _albumRepository = albumRepository;
            _folderRepository = folderRepository;
            _cache = cache;
        }

        #region Album

        public int GetAlbumIdByName(string albumName)
        {
            Guard.NotEmpty(albumName, nameof(albumName));
            return _albumRegistry.GetAlbumByName(albumName)?.Id ?? 0;
        }

        public TreeNode<MediaFolderNode> FindAlbum(MediaFile mediaFile)
        {
            var node = GetNodeById(mediaFile?.FolderId ?? 0);
            if (node != null)
            {
                return node.Closest(x => x.Value.IsAlbum);
            }

            return null;
        }

        #endregion

        #region Cached nodes

        public TreeNode<MediaFolderNode> GetRootNode()
        {
            var cacheKey = FolderTreeKey;

            var root = _cache.Get(cacheKey, () => 
            {
                var query = from x in _folderRepository.TableUntracked
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

        public void ClearCache()
        {
            _cache.Remove(FolderTreeKey);
        }

        private string NormalizePath(string path)
        {
            return path.Trim('/', '\\').ToLower();
        }

        #endregion

        #region Storage

        public MediaFolder GetFolderById(int id)
        {
            if (id <= 0)
                return null;

            return _folderRepository.GetById(id);
        }

        public bool FolderExists(string path)
        {
            return GetNodeByPath(path) != null;
        }

        public void InsertFolder(MediaFolder folder)
        {
            Guard.NotNull(folder, nameof(folder));

            ClearCache();

            throw new NotImplementedException();
        }

        public void UpdateFolder(MediaFolder folder)
        {
            Guard.NotNull(folder, nameof(folder));

            ClearCache();

            throw new NotImplementedException();
        }

        public void DeleteFolder(MediaFolder folder)
        {
            Guard.NotNull(folder, nameof(folder));

            ClearCache();

            throw new NotImplementedException();
        }

        #endregion
    }
}
