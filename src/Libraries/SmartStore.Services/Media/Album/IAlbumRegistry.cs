using System;
using System.Collections.Generic;

namespace SmartStore.Services.Media
{
    public interface IAlbumRegistry
    {
        IReadOnlyCollection<AlbumInfo> GetAllAlbums();
        IEnumerable<string> GetAlbumNames(bool withTrackDetectors = false);
        AlbumInfo GetAlbumByName(string name);
        AlbumInfo GetAlbumById(int id);

        /// <summary>
        /// Deletes album and all containing files
        /// </summary>
        /// <param name="albumName">Name of album to delete</param>
        void UninstallAlbum(string albumName);

        /// <summary>
        /// Deletes album but keeps containing files
        /// </summary>
        /// <param name="albumName">Name of album to delete</param>
        /// <param name="moveFilesToAlbum">Name of album to move files to</param>
        void DeleteAlbum(string albumName, string moveFilesToAlbum);
        void ClearCache();
    }
}
