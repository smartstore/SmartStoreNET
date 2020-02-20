using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public partial class SystemMediaAlbumProvider : IMediaAlbumProvider
    {
        public const string Product = "product";
        public const string Category = "category";
        public const string Brand = "brand";
        public const string Customer = "customer";
        public const string Blog = "blog";
        public const string News = "news";
        public const string Forum = "forum";
        public const string Download = "download";
        public const string Message = "message";
        public const string File = "file";

        public IEnumerable<MediaAlbum> GetAlbums()
        {
            return new[]
            {
                new MediaAlbum
                {
                    Name = Product,
                    ResKey = "Admin.Media.Album.Product",
                    CanTrackRelations = true,
                    Order = int.MinValue
                },
                new MediaAlbum
                {
                    Name = Category,
                    ResKey = "Admin.Media.Album.Category",
                    CanTrackRelations = true,
                    Order = int.MinValue + 10
                },
                new MediaAlbum
                {
                    Name = Brand,
                    ResKey = "Admin.Media.Album.Brand",
                    CanTrackRelations = true,
                    Order = int.MinValue + 20
                },
                new MediaAlbum
                {
                    Name = Customer,
                    ResKey = "Admin.Media.Album.Customer",
                    CanTrackRelations = true, // TBD
                    Order = int.MinValue + 30
                },
                new MediaAlbum
                {
                    Name = Blog,
                    ResKey = "Admin.Media.Album.Blog",
                    CanTrackRelations = true,
                    Order = int.MinValue + 40
                },
                new MediaAlbum
                {
                    Name = News,
                    ResKey = "Admin.Media.Album.News",
                    CanTrackRelations = true,
                    Order = int.MinValue + 50
                },
                new MediaAlbum
                {
                    Name = Forum,
                    ResKey = "Admin.Media.Album.Forum",
                    CanTrackRelations = false, // TBD
                    Order = int.MinValue + 60
                },
                new MediaAlbum
                {
                    Name = Download,
                    ResKey = "Admin.Media.Album.Download",
                    CanTrackRelations = true,
                    Order = int.MinValue + 70
                },
                new MediaAlbum
                {
                    Name = Message,
                    ResKey = "Admin.Media.Album.Message",
                    CanTrackRelations = true,
                    Order = int.MinValue + 80
                },
                new MediaAlbum
                {
                    Name = File,
                    ResKey = "Admin.Media.Album.File",
                    CanTrackRelations = false,
                    // Slug = "uploaded", // TBD: hmmmm??
                    Order = int.MaxValue
                }
            };
        }

        public MediaAlbumDisplayHint GetDisplayHint(MediaAlbum album)
        {
            if (album.Name == Product)
            {
                return new MediaAlbumDisplayHint { OverlayIcon = "fa fa-cube" };
            }
            if (album.Name == Category)
            {
                return new MediaAlbumDisplayHint { OverlayIcon = "fa fa-sitemap" };
            }
            if (album.Name == Brand)
            {
                return new MediaAlbumDisplayHint { OverlayIcon = "far fa-building" };
            }
            if (album.Name == Customer)
            {
                return new MediaAlbumDisplayHint { OverlayIcon = "fa fa-user" };
            }
            if (album.Name == Blog)
            {
                return new MediaAlbumDisplayHint { OverlayIcon = "fa fa-blog" };
            }
            if (album.Name == News)
            {
                return new MediaAlbumDisplayHint { OverlayIcon = "fa fa-newspaper" };
            }
            if (album.Name == Forum)
            {
                return new MediaAlbumDisplayHint { OverlayIcon = "fa fa-users" };
            }
            if (album.Name == Download)
            {
                return new MediaAlbumDisplayHint { OverlayIcon = "fa fa-download" };
            }
            if (album.Name == Message)
            {
                return new MediaAlbumDisplayHint { OverlayIcon = "fa fa-envelope" };
            }
            if (album.Name == File)
            {
                // TODO: var(--success) should be system default.
                return new MediaAlbumDisplayHint { Color = "var(--success)" };
            }

            return null;
        }
    }
}
