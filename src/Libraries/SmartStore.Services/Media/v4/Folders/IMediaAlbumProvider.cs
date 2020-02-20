using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public interface IMediaAlbumProvider
    {
        IEnumerable<MediaAlbum> GetAlbums();
        MediaAlbumDisplayHint GetDisplayHint(MediaAlbum album);
    }
}
