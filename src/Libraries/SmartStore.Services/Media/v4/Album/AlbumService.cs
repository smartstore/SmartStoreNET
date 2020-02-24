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

        private readonly IRepository<MediaAlbum> _albumRepository;
        private readonly IRepository<MediaFolder> _folderRepository;
        private readonly ICacheManager _cache;


        private readonly IEnumerable<Lazy<IAlbumProvider>> _albumProviders;
        private readonly IIndex<Type, IAlbumProvider> _albumProviderIndexer;

        private readonly static ConcurrentDictionary<string, AlbumProviderInfo> _albumProviderInfoCache = new ConcurrentDictionary<string, AlbumProviderInfo>();
        class AlbumProviderInfo
        {
            public int Id { get; set; } 
            public string Name { get; set; }
            public Type ProviderType { get; set; }
            public bool IsTrackDetector { get; set; }
            public AlbumDisplayHint DisplayHint { get; set; }
        }

        public AlbumService(
            IRepository<MediaAlbum> albumRepository,
            IRepository<MediaFolder> folderRepository,
            ICacheManager cache,
            IEnumerable<Lazy<IAlbumProvider>> albumProviders,
            IIndex<Type, IAlbumProvider> albumProvider)
        {
            _albumRepository = albumRepository;
            _folderRepository = folderRepository;
            _cache = cache;
            _albumProviders = albumProviders;
            _albumProviderIndexer = albumProvider;
        }

        #region Albums

        public T LoadAlbumProvider<T>() where T : IAlbumProvider
        {
            return (T)_albumProviderIndexer[typeof(T)];
        }

        public IAlbumProvider LoadAlbumProvider(string albumName)
        {
            Guard.NotEmpty(albumName, nameof(albumName));

            if (_albumProviderInfoCache.TryGetValue(albumName, out var info))
            {
                return _albumProviderIndexer[info.ProviderType];
            }

            return null;
        }

        public IAlbumProvider[] LoadAllAlbumProviders()
        {
            return _albumProviders.Select(x => x.Value).ToArray();
        }

        public void InstallAlbums(IEnumerable<IAlbumProvider> albumProviders)
        {
            Guard.NotNull(albumProviders, nameof(albumProviders));

            var dbAlbums = _albumRepository.Table.Select(x => new { x.Id, x.Name }).ToDictionary(x => x.Name);
            var hasChanges = false;

            using (var scope = new DbContextScope(_albumRepository.Context, 
                validateOnSave: false, 
                hooksEnabled: false, 
                autoCommit: true))
            {
                foreach (var provider in albumProviders)
                {
                    var albums = provider.GetAlbums().DistinctBy(x => x.Name).ToArray();

                    foreach (var album in albums)
                    {
                        var info = new AlbumProviderInfo 
                        { 
                            Name = album.Name,
                            ProviderType = provider.GetType(),
                            IsTrackDetector = provider is IMediaTrackDetector,
                            DisplayHint = provider.GetDisplayHint(album) ?? new AlbumDisplayHint()
                        };

                        if (dbAlbums.TryGetValue(album.Name, out var dbAlbum))
                        {
                            info.Id = dbAlbum.Id;
                        }
                        else
                        {
                            _albumRepository.Insert(album);
                            hasChanges = true;
                            info.Id = album.Id;
                        }

                        _albumProviderInfoCache.AddOrUpdate(album.Name, info, (key, val) => info);
                    }
                }
            }

            if (hasChanges)
            {
                ClearCache();
            }
        }

        public int GetAlbumIdByName(string name)
        {
            if (_albumProviderInfoCache.TryGetValue(name, out var info))
            {
                return info.Id;
            }
            
            return 0;
        }

        public void DeleteAlbum(string name)
        {
            Guard.NotEmpty(name, nameof(name));

            _albumProviderInfoCache.TryRemove(name, out _);

            // TODO
            throw new NotImplementedException();

            //ClearCache();
        }

        public IEnumerable<string> GetAlbumNames(bool withRelationDetectors = false)
        {
            if (!withRelationDetectors)
            {
                return _albumProviderInfoCache.Keys;
            }

            return _albumProviderInfoCache.Where(x => x.Value.IsTrackDetector).Select(x => x.Key).ToArray();
        }

        #endregion

        #region Folders

        public void DeleteFolder(MediaFolder folder)
        {
            // TODO
            throw new NotImplementedException();
        }

        #endregion

        #region Tree

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
                        CanTrackRelations = x.CanTrackRelations,
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

                        if (_albumProviderInfoCache.TryGetValue(album.Name, out var info))
                        {
                            var displayHint = info.DisplayHint;
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

        #endregion
    }
}
