using System;
using System.Collections.Generic;
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
    public partial class MediaFolderService : IMediaFolderService
    {
        internal static TimeSpan FolderTreeCacheDuration = TimeSpan.FromHours(3);

        internal const string FolderTreeKey = "mediafolder:tree";

        private readonly IRepository<MediaAlbum> _albumRepository;
        private readonly IRepository<MediaFolder> _folderRepository;
        private readonly ITypeFinder _typeFinder;
        private readonly ICacheManager _cache;

        private IMediaAlbumProvider[] _albumProviders;

        public MediaFolderService(
            IRepository<MediaAlbum> albumRepository,
            IRepository<MediaFolder> folderRepository,
            ITypeFinder typeFinder,
            ICacheManager cache)
        {
            _albumRepository = albumRepository;
            _folderRepository = folderRepository;
            _typeFinder = typeFinder;
            _cache = cache;
        }

        public IMediaAlbumProvider[] LoadAlbumProviders()
        {
            if (_albumProviders == null)
            {
                _albumProviders = _typeFinder
                    .FindClassesOfType<IMediaAlbumProvider>(ignoreInactivePlugins: true)
                    .Select(x => Activator.CreateInstance(x))
                    .Cast<IMediaAlbumProvider>()
                    .ToArray();
            }
            
            return _albumProviders;
        }

        public void InstallAlbums(IEnumerable<IMediaAlbumProvider> albumProviders)
        {
            Guard.NotNull(albumProviders, nameof(albumProviders));

            var albums = albumProviders.SelectMany(x => x.GetAlbums()).DistinctBy(x => x.Name);
            var dbAlbumNames = new HashSet<string>(_albumRepository.Table.Select(x => x.Name).ToList(), StringComparer.OrdinalIgnoreCase);
            var hasChanges = false;

            using (var scope = new DbContextScope(_albumRepository.Context, validateOnSave: false, hooksEnabled: false, autoCommit: false))
            {
                foreach (var album in albums)
                {
                    if (!dbAlbumNames.Contains(album.Name))
                    {
                        _albumRepository.Insert(album);
                        hasChanges = true;
                    }
                }

                scope.Commit();
            }

            if (hasChanges)
            {
                ClearCache();
            }
        }

        public void DeleteAlbum(string name)
        {
            throw new NotImplementedException();
        }

        public void DeleteFolder(MediaFolder folder)
        {
            throw new NotImplementedException();
        }

        #region Tree

        public void ClearCache()
        {
            _cache.Remove(FolderTreeKey);
        }

        public TreeNode<MediaFolderNode> GetFolderTree(int rootFolderId = 0)
        {
            var cacheKey = FolderTreeKey;

            var root = _cache.Get(cacheKey, () => 
            {
                var providers = LoadAlbumProviders();

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
                        item.ResKey = album.ResKey;
                        item.IncludePath = album.IncludePath;
                        item.Order = album.Order ?? 0;

                        var displayHint = providers.Select(x => x.GetDisplayHint(album)).FirstOrDefault();
                        if (displayHint != null)
                        {
                            item.Color = displayHint.Color;
                            item.OverlayColor = displayHint.OverlayColor;
                            item.OverlayIcon = displayHint.OverlayIcon;
                        }
                    }

                    return item;
                });

                var nodeMap = unsortedNodes.ToMultimap(x => x.ParentId ?? 0, x => x);
                var curParent = new TreeNode<MediaFolderNode>(new MediaFolderNode { Name = "Root" });

                AddChildTreeNodes(curParent, 0, nodeMap);

                curParent.Root.Traverse(x => 
                {
                    // Handle inheritable/chainable properties (Slug, CanTrackRelations, IncludePath)

                    if (x.IsLeaf)
                    {
                        // Start with deepest nodes
                        // TODO: ...
                    }
                });

                return curParent.Root;
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
