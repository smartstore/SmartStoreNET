using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Collections;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public static class IFolderServiceExtensions
    {
        public static TreeNode<MediaFolderNode> FindFolder(this IFolderService service, MediaFile mediaFile)
        {
            return service.GetNodeById(mediaFile?.FolderId ?? 0);
        }

        public static TreeNode<MediaFolderNode> FindAlbum(this IFolderService service, MediaFile mediaFile)
        {
            return FindFolder(service, mediaFile)?.Closest(x => x.Value.IsAlbum);
        }

        public static TreeNode<MediaFolderNode> FindAlbum(this IFolderService service, int folderId)
        {
            return service.GetNodeById(folderId)?.Closest(x => x.Value.IsAlbum);
        }

        public static bool AreInSameAlbum(this IFolderService service, MediaFile file1, MediaFile file2)
        {
            return FindAlbum(service, file1) == FindAlbum(service, file2);
        }

        public static bool AreInSameAlbum(this IFolderService service, int folderId1, int folderId2)
        {
            return FindAlbum(service, folderId1) == FindAlbum(service, folderId2);
        }

        public static IEnumerable<MediaFolderNode> GetFoldersFlattened(this IFolderService service, string path, bool includeSelf = true)
        {
            var node = service.GetNodeByPath(path);
            if (node == null)
            {
                return Enumerable.Empty<MediaFolderNode>();
            }

            return node.FlattenNodes(includeSelf).Select(x => x.Value);
        }

        public static IEnumerable<MediaFolderNode> GetFoldersFlattened(this IFolderService service, int folderId, bool includeSelf = true)
        {
            var node = service.GetNodeById(folderId);
            if (node == null)
            {
                return Enumerable.Empty<MediaFolderNode>();
            }

            return node.FlattenNodes(includeSelf).Select(x => x.Value);
        }
    }
}
