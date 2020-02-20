using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Collections;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public interface IMediaFolderService
    {
        IMediaAlbumProvider[] LoadAlbumProviders();
        void InstallAlbums(IEnumerable<IMediaAlbumProvider> albumProviders);
        void DeleteAlbum(string name);
        void DeleteFolder(MediaFolder folder);

        TreeNode<MediaFolderNode> GetFolderTree(int rootNodeId = 0);
        void ClearCache();
    }
}
