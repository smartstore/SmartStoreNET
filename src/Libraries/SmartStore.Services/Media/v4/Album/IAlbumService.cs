using System;
using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public interface IAlbumService
    {
        int GetAlbumIdByName(string albumName);
        void DeleteFolder(MediaFolder folder);

        TreeNode<MediaFolderNode> FindAlbum(MediaFile mediaFile);
        TreeNode<MediaFolderNode> GetFolderTree(int rootFolderId = 0);
        void ClearCache();
    }
}
