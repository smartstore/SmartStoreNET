using System;
using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public interface IMediaFolderService
    {
        T LoadAlbumProvider<T>() where T : IMediaAlbumProvider;
        IMediaAlbumProvider LoadAlbumProvider(string albumName);
        IMediaAlbumProvider[] LoadAllAlbumProviders();

        void InstallAlbums(IEnumerable<IMediaAlbumProvider> albumProviders);
        void DeleteAlbum(string name);
        IEnumerable<string> GetAlbumNames(bool withRelationDetectors = false);
        int GetAlbumIdByName(string name);

        void DeleteFolder(MediaFolder folder);

        TreeNode<MediaFolderNode> FindAlbum(MediaFile mediaFile);
        TreeNode<MediaFolderNode> GetFolderTree(int rootFolderId = 0);
        void ClearCache();
    }
}
