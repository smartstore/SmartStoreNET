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
        /// <summary>
        /// Gets a folder entity by path from the database, e.g. "catalog/subfolder1/subfolder2".
        /// The first token always refers to an album. This method bypasses the cache.
        /// </summary>
        public static MediaFolder GetFolderByPath(this IFolderService service, string path, bool withFiles = false)
        {
            return service.GetFolderById(service.GetNodeByPath(path)?.Value?.Id ?? 0, withFiles);
        }

        /// <summary>
        /// Finds the folder node for a given <see cref="MediaFile"/> object.
        /// </summary>
        /// <returns>The found folder node or <c>null</c>.</returns>
        public static TreeNode<MediaFolderNode> FindNode(this IFolderService service, MediaFile mediaFile)
        {
            return service.GetNodeById(mediaFile?.FolderId ?? 0);
        }

        /// <summary>
        /// Finds the root album node for a given <see cref="MediaFile"/> object.
        /// </summary>
        /// <returns>The found album node or <c>null</c>.</returns>
        public static TreeNode<MediaFolderNode> FindAlbum(this IFolderService service, MediaFile mediaFile)
        {
            return FindNode(service, mediaFile)?.Closest(x => x.Value.IsAlbum);
        }

        /// <summary>
        /// Finds the root album node for a given folder id.
        /// </summary>
        /// <returns>The found album node or <c>null</c>.</returns>
        public static TreeNode<MediaFolderNode> FindAlbum(this IFolderService service, int folderId)
        {
            return service.GetNodeById(folderId)?.Closest(x => x.Value.IsAlbum);
        }

        /// <summary>
        /// Checks whether all passed files are contained in the same album.
        /// </summary>
        public static bool AreInSameAlbum(this IFolderService service, params MediaFile[] files)
        {
            return files.Select(x => FindAlbum(service, x)).Distinct().Count() <= 1;
        }

        /// <summary>
        /// Checks whether all passed folder ids are children of the same album.
        /// </summary>
        public static bool AreInSameAlbum(this IFolderService service, params int[] folderIds)
        {
            return folderIds.Select(x => FindAlbum(service, x)).Distinct().Count() <= 1;
        }

        public static IEnumerable<MediaFolderNode> GetNodesFlattened(this IFolderService service, string path, bool includeSelf = true)
        {
            var node = service.GetNodeByPath(path);
            if (node == null)
            {
                return Enumerable.Empty<MediaFolderNode>();
            }

            return node.FlattenNodes(includeSelf).Select(x => x.Value);
        }

        public static IEnumerable<MediaFolderNode> GetNodesFlattened(this IFolderService service, int folderId, bool includeSelf = true)
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
