using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public interface IAlbumProvider
    {
        IEnumerable<MediaAlbum> GetAlbums();
        AlbumDisplayHint GetDisplayHint(MediaAlbum album);
    }
}