using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Media
{
    public static class IAlbumServiceExtensions
    {
        public static IEnumerable<MediaFolderNode> GetFoldersFlattened(this IAlbumService service, string albumName, bool includeAlbumNode = true)
        {
            return GetFoldersFlattened(service, service.GetAlbumIdByName(albumName), includeAlbumNode);
        }

        public static IEnumerable<MediaFolderNode> GetFoldersFlattened(this IAlbumService service, int albumId, bool includeAlbumNode = true)
        {
            var albumNode = service.GetFolderTree(albumId);
            if (albumNode == null)
            {
                return Enumerable.Empty<MediaFolderNode>();
            }

            return albumNode.FlattenNodes(includeAlbumNode).Select(x => x.Value);
        }
    }
}
