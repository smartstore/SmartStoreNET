using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Media
{
    public static class IFolderServiceExtensions
    {
        public static IEnumerable<MediaFolderNode> GetFoldersFlattened(this IFolderService service, string albumName, bool includeAlbumNode = true)
        {
            return GetFoldersFlattened(service, service.GetAlbumIdByName(albumName), includeAlbumNode);
        }

        public static IEnumerable<MediaFolderNode> GetFoldersFlattened(this IFolderService service, int albumId, bool includeAlbumNode = true)
        {
            var albumNode = service.GetNodeById(albumId);
            if (albumNode == null || !albumNode.Value.IsAlbum)
            {
                return Enumerable.Empty<MediaFolderNode>();
            }

            return albumNode.FlattenNodes(includeAlbumNode).Select(x => x.Value);
        }
    }
}
