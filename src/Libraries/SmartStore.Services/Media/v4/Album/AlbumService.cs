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
    public partial class AlbumService : IAlbumService
    {
        internal static TimeSpan FolderTreeCacheDuration = TimeSpan.FromHours(3);

        internal const string FolderTreeKey = "mediafolder:tree";

        private readonly IAlbumRegistry _albumRegistry;
        private readonly IRepository<MediaAlbum> _albumRepository;
        private readonly IRepository<MediaFolder> _folderRepository;
        private readonly ICacheManager _cache;

        public AlbumService(
            IAlbumRegistry albumRegistry,
            IRepository<MediaAlbum> albumRepository,
            IRepository<MediaFolder> folderRepository,
            ICacheManager cache,
            IEnumerable<Lazy<IAlbumProvider>> albumProviders,
            IIndex<Type, IAlbumProvider> albumProvider)
        {
            _albumRegistry = albumRegistry;
            _albumRepository = albumRepository;
            _folderRepository = folderRepository;
            _cache = cache;
        }

        public int GetAlbumIdByName(string albumName)
        {
            Guard.NotEmpty(albumName, nameof(albumName));
            return _albumRegistry.GetAlbumByName(albumName)?.Id ?? 0;
        }

        public void DeleteFolder(MediaFolder folder)
        {
            // TODO
            throw new NotImplementedException();
        }


        public void ClearCache()
        {
            _cache.Remove(FolderTreeKey);
        }

        public TreeNode<MediaFolderNode> FindAlbum(MediaFile mediaFile)
        {
            if (mediaFile?.FolderId == null)
                return null;
            
            var node = GetFolderTree(mediaFile.FolderId.Value);
            if (node != null)
            {
                return node.Closest(x => x.Value.IsAlbum);
            }

            return null;
        }

        public TreeNode<MediaFolderNode> GetFolderTree(int rootFolderId = 0)
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
                var curParent = new TreeNode<MediaFolderNode>(new MediaFolderNode { Name = "Root", Id = 0 });

                AddChildTreeNodes(curParent, 0, nodeMap);

                var root = curParent.Root;
                root.Traverse(x => 
                {
                    // Handle inheritable/chainable properties (AlbumName, Slug, CanTrackRelations, IncludePath)

                    if (x.IsLeaf)
                    {
                        // Start with deepest nodes
                        // TODO: ...
                    }
                });

                return root;
            }, FolderTreeCacheDuration);

            if (rootFolderId > 0)
            {
                root = root.SelectNodeById(rootFolderId);
            }

            return root;
        }

        private void AddChildTreeNodes(TreeNode<MediaFolderNode> parentNode, int parentId, Multimap<int, MediaFolderNode> nodeMap)
        {
            if (parentNode == null)
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
                var newNode = new TreeNode<MediaFolderNode>(node)
                {
                    Id = node.Id
                };

                parentNode.Append(newNode);

                AddChildTreeNodes(newNode, node.Id, nodeMap);
            }
        }
    }
}
