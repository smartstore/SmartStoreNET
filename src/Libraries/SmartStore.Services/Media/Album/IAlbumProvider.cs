using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    /// <summary>
    /// Creates album entities and provides media manager UI infos about them.
    /// Implementation classes are instantiated once on app startup. Impl does NOT
    /// need to be registered in DI.
    /// </summary>
    public interface IAlbumProvider
    {
        /// <summary>
        /// Creates a list of album entities the provider is responsible for.
        /// A special bootstrapper checks whether an album already exists in the
        /// database (by <see cref="MediaFolder.Name"/> key). The entity is inserted
        /// to the database if it does not exist yet, or skipped otherwise.
        /// </summary>
        /// <returns>List of entities.</returns>
        IEnumerable<MediaAlbum> GetAlbums();

        /// <summary>
        /// Gets UI display info about an album.
        /// </summary>
        /// <param name="album">Album to get info for.</param>
        /// <returns>UI display hint object.</returns>
        AlbumDisplayHint GetDisplayHint(MediaAlbum album);
    }
}