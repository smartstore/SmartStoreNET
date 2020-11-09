using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Data.Utilities;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Services.Media
{
    public partial class SystemAlbumProvider : IAlbumProvider, IMediaTrackDetector
    {
        private readonly IDbContext _dbContext;

        public SystemAlbumProvider(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public const string Catalog = "catalog";
        public const string Content = "content";
        public const string Downloads = "download";
        public const string Messages = "message";
        public const string Customers = "customer";
        public const string Files = "file";

        #region Album Provider

        public IEnumerable<MediaAlbum> GetAlbums()
        {
            return new[]
            {
                new MediaAlbum
                {
                    Name = Catalog,
                    ResKey = "Admin.Catalog",
                    CanDetectTracks = true,
                    Order = int.MinValue
                },
                new MediaAlbum
                {
                    Name = Content,
                    ResKey = "Admin.Media.Album.Content",
                    CanDetectTracks = true,
                    Order = int.MinValue + 10
                },
                new MediaAlbum
                {
                    Name = Downloads,
                    ResKey = "Common.Downloads",
                    CanDetectTracks = true,
                    Order = int.MinValue + 30
                },
                new MediaAlbum
                {
                    Name = Messages,
                    ResKey = "Admin.Media.Album.Message",
                    CanDetectTracks = true,
                    Order = int.MinValue + 40
                },
                new MediaAlbum
                {
                    Name = Customers,
                    ResKey = "Admin.Customers",
                    CanDetectTracks = true, // TBD
                    Order = int.MinValue + 50
                },
                new MediaAlbum
                {
                    Name = Files,
                    ResKey = "Admin.Media.Album.File",
                    CanDetectTracks = false,
                    IncludePath = true,
                    Order = int.MaxValue
                }
            };
        }

        public AlbumDisplayHint GetDisplayHint(MediaAlbum album)
        {

            if (album.Name == Catalog)
            {
                return new AlbumDisplayHint { OverlayIcon = "fa fa-cube" };
            }
            if (album.Name == Content)
            {
                return new AlbumDisplayHint { OverlayIcon = "fa fa-sitemap" };
            }
            if (album.Name == Downloads)
            {
                return new AlbumDisplayHint { OverlayIcon = "fa fa-download" };
            }
            if (album.Name == Messages)
            {
                return new AlbumDisplayHint { OverlayIcon = "fa fa-envelope" };
            }
            if (album.Name == Customers)
            {
                return new AlbumDisplayHint { OverlayIcon = "fa fa-user" };
            }
            if (album.Name == Files)
            {
                // TODO: var(--success) should be system default.
                return new AlbumDisplayHint { Color = "var(--success)" };
            }

            return null;
        }

        #endregion

        #region Tracking

        public void ConfigureTracks(string albumName, TrackedMediaPropertyTable table)
        {
            if (albumName == Catalog)
            {
                table.Register<ProductMediaFile>(x => x.MediaFileId);
                table.Register<ProductAttributeOption>(x => x.MediaFileId);
                table.Register<ProductVariantAttributeValue>(x => x.MediaFileId);
                table.Register<CheckoutAttributeValue>(x => x.MediaFileId);
                table.Register<SpecificationAttributeOption>(x => x.MediaFileId);
                table.Register<Category>(x => x.MediaFileId);
                table.Register<Manufacturer>(x => x.MediaFileId);
            }
            else if (albumName == Content)
            {
                table.Register<BlogPost>(x => x.MediaFileId);
                table.Register<BlogPost>(x => x.PreviewMediaFileId);
                table.Register<NewsItem>(x => x.MediaFileId);
                table.Register<NewsItem>(x => x.PreviewMediaFileId);
                table.Register<Store>(x => x.LogoMediaFileId);
                table.Register<Store>(x => x.FavIconMediaFileId);
                table.Register<Store>(x => x.PngIconMediaFileId);
                table.Register<Store>(x => x.AppleTouchIconMediaFileId);
                table.Register<Store>(x => x.MsTileImageMediaFileId);
            }
            else if (albumName == Downloads)
            {
                table.Register<Download>(x => x.MediaFileId);
            }
            else if (albumName == Messages)
            {
                // TODO: (mm) (mc) These props are localizable
                table.Register<MessageTemplate>(x => x.Attachment1FileId);
                table.Register<MessageTemplate>(x => x.Attachment2FileId);
                table.Register<MessageTemplate>(x => x.Attachment3FileId);
            }
        }

        public IEnumerable<MediaTrack> DetectAllTracks(string albumName)
        {
            var ctx = _dbContext;
            var entityName = string.Empty;

            // Products
            if (albumName == Catalog)
            {
                // Products
                {
                    var name = nameof(ProductMediaFile);
                    var p = new FastPager<ProductMediaFile>(ctx.Set<ProductMediaFile>().AsNoTracking(), 5000);
                    while (p.ReadNextPage(x => new { x.Id, x.ProductId, x.MediaFileId }, x => x.Id, out var list))
                    {
                        foreach (var x in list)
                        {
                            yield return new MediaTrack { EntityId = x.Id, EntityName = name, MediaFileId = x.MediaFileId, Property = nameof(x.MediaFileId) };
                        }
                        list.Clear();
                    }
                }

                GC.Collect();

                // ProductAttributeOption
                {
                    var name = nameof(ProductAttributeOption);
                    var p = new FastPager<ProductAttributeOption>(ctx.Set<ProductAttributeOption>().AsNoTracking().Where(x => x.MediaFileId > 0), 5000);
                    while (p.ReadNextPage(x => new { x.Id, x.MediaFileId }, x => x.Id, out var list))
                    {
                        foreach (var x in list)
                        {
                            yield return new MediaTrack { EntityId = x.Id, EntityName = name, MediaFileId = x.MediaFileId, Property = nameof(x.MediaFileId) };
                        }
                        list.Clear();
                    }
                }

                // ProductVariantAttributeValue
                {
                    var name = nameof(ProductVariantAttributeValue);
                    var p = new FastPager<ProductVariantAttributeValue>(ctx.Set<ProductVariantAttributeValue>().AsNoTracking().Where(x => x.MediaFileId > 0), 5000);
                    while (p.ReadNextPage(x => new { x.Id, x.MediaFileId }, x => x.Id, out var list))
                    {
                        foreach (var x in list)
                        {
                            yield return new MediaTrack { EntityId = x.Id, EntityName = name, MediaFileId = x.MediaFileId, Property = nameof(x.MediaFileId) };
                        }
                        list.Clear();
                    }
                }

                // CheckoutAttributeValue
                {
                    var name = nameof(CheckoutAttributeValue);
                    var p = new FastPager<CheckoutAttributeValue>(ctx.Set<CheckoutAttributeValue>().AsNoTracking().Where(x => x.MediaFileId.HasValue), 5000);
                    while (p.ReadNextPage(x => new { x.Id, x.MediaFileId }, x => x.Id, out var list))
                    {
                        foreach (var x in list)
                        {
                            yield return new MediaTrack { EntityId = x.Id, EntityName = name, MediaFileId = x.MediaFileId.Value, Property = nameof(x.MediaFileId) };
                        }
                        list.Clear();
                    }
                }

                GC.Collect();

                // SpecificationAttributeOption
                {
                    var name = nameof(SpecificationAttributeOption);
                    var p = new FastPager<SpecificationAttributeOption>(ctx.Set<SpecificationAttributeOption>().AsNoTracking().Where(x => x.MediaFileId > 0), 5000);
                    while (p.ReadNextPage(x => new { x.Id, x.MediaFileId }, x => x.Id, out var list))
                    {
                        foreach (var x in list)
                        {
                            yield return new MediaTrack { EntityId = x.Id, EntityName = name, MediaFileId = x.MediaFileId, Property = nameof(x.MediaFileId) };
                        }
                        list.Clear();
                    }
                }

                // Categories
                {
                    var name = nameof(Category);
                    var p = new FastPager<Category>(ctx.Set<Category>().AsNoTracking().Where(x => x.MediaFileId.HasValue), 5000);
                    while (p.ReadNextPage(x => new { x.Id, x.MediaFileId }, x => x.Id, out var list))
                    {
                        foreach (var x in list)
                        {
                            yield return new MediaTrack { EntityId = x.Id, EntityName = name, MediaFileId = x.MediaFileId.Value, Property = nameof(x.MediaFileId) };
                        }
                        list.Clear();
                    }
                }

                GC.Collect();

                // Brands
                {
                    var name = nameof(Manufacturer);
                    var p = new FastPager<Manufacturer>(ctx.Set<Manufacturer>().AsNoTracking().Where(x => x.MediaFileId.HasValue), 5000);
                    while (p.ReadNextPage(x => new { x.Id, x.MediaFileId }, x => x.Id, out var list))
                    {
                        foreach (var x in list)
                        {
                            yield return new MediaTrack { EntityId = x.Id, EntityName = name, MediaFileId = x.MediaFileId.Value, Property = nameof(x.MediaFileId) };
                        }
                        list.Clear();
                    }
                }

                yield break;
            }

            // Content
            if (albumName == Content)
            {
                // Blog
                {
                    var name = nameof(BlogPost);
                    var p = new FastPager<BlogPost>(ctx.Set<BlogPost>().AsNoTracking().Where(x => x.MediaFileId.HasValue || x.PreviewMediaFileId.HasValue));
                    while (p.ReadNextPage(x => new { x.Id, x.MediaFileId, x.PreviewMediaFileId }, x => x.Id, out var list))
                    {
                        foreach (var x in list)
                        {
                            if (x.MediaFileId.HasValue)
                                yield return new MediaTrack { EntityId = x.Id, EntityName = name, MediaFileId = x.MediaFileId.Value, Property = nameof(x.MediaFileId) };
                            if (x.PreviewMediaFileId.HasValue)
                                yield return new MediaTrack { EntityId = x.Id, EntityName = name, MediaFileId = x.PreviewMediaFileId.Value, Property = nameof(x.PreviewMediaFileId) };
                        }
                        list.Clear();
                    }
                }

                // NewsItem
                {
                    var name = nameof(NewsItem);
                    var p = new FastPager<NewsItem>(ctx.Set<NewsItem>().AsNoTracking().Where(x => x.MediaFileId.HasValue || x.PreviewMediaFileId.HasValue));
                    while (p.ReadNextPage(x => new { x.Id, x.MediaFileId, x.PreviewMediaFileId }, x => x.Id, out var list))
                    {
                        foreach (var x in list)
                        {
                            if (x.MediaFileId.HasValue)
                                yield return new MediaTrack { EntityId = x.Id, EntityName = name, MediaFileId = x.MediaFileId.Value, Property = nameof(x.MediaFileId) };
                            if (x.PreviewMediaFileId.HasValue)
                                yield return new MediaTrack { EntityId = x.Id, EntityName = name, MediaFileId = x.PreviewMediaFileId.Value, Property = nameof(x.PreviewMediaFileId) };
                        }
                        list.Clear();
                    }
                }

                // Store
                {
                    var name = nameof(Store);
                    var p = new FastPager<Store>(ctx.Set<Store>().AsNoTracking().Where(x => x.LogoMediaFileId > 0));
                    while (p.ReadNextPage(x => new { x.Id, x.LogoMediaFileId }, x => x.Id, out var list))
                    {
                        foreach (var x in list)
                        {
                            yield return new MediaTrack { EntityId = x.Id, EntityName = name, MediaFileId = x.LogoMediaFileId, Property = nameof(x.LogoMediaFileId) };
                        }
                        list.Clear();
                    }
                }

                yield break;
            }

            // Downloads
            if (albumName == Downloads)
            {
                var name = nameof(Download);
                var p = new FastPager<Download>(ctx.Set<Download>().AsNoTracking().Where(x => x.MediaFileId.HasValue), 5000);
                while (p.ReadNextPage(x => new { x.Id, x.MediaFileId }, x => x.Id, out var list))
                {
                    foreach (var x in list)
                    {
                        yield return new MediaTrack { EntityId = x.Id, EntityName = name, MediaFileId = x.MediaFileId.Value, Property = nameof(x.MediaFileId) };
                    }
                    list.Clear();
                }

                yield break;
            }

            // Messages
            if (albumName == Messages)
            {
                var name = nameof(MessageTemplate);
                var p = new FastPager<MessageTemplate>(ctx.Set<MessageTemplate>().AsNoTracking());
                while (p.ReadNextPage(x => new { x.Id, x.Attachment1FileId, x.Attachment2FileId, x.Attachment3FileId }, x => x.Id, out var list))
                {
                    foreach (var x in list)
                    {
                        if (x.Attachment1FileId.HasValue)
                            yield return new MediaTrack { EntityId = x.Id, EntityName = name, MediaFileId = x.Attachment1FileId.Value, Property = nameof(x.Attachment1FileId) };
                        if (x.Attachment2FileId.HasValue)
                            yield return new MediaTrack { EntityId = x.Id, EntityName = name, MediaFileId = x.Attachment2FileId.Value, Property = nameof(x.Attachment2FileId) };
                        if (x.Attachment3FileId.HasValue)
                            yield return new MediaTrack { EntityId = x.Id, EntityName = name, MediaFileId = x.Attachment3FileId.Value, Property = nameof(x.Attachment3FileId) };
                    }
                    list.Clear();
                }

                yield break;
            }

            // Customer
            if (albumName == Customers)
            {
                var name = nameof(Customer);
                var key = SystemCustomerAttributeNames.AvatarPictureId;

                // Avatars
                var p = new FastPager<GenericAttribute>(ctx.Set<GenericAttribute>().AsNoTracking()
                    .Where(x => x.KeyGroup == nameof(Customer) && x.Key == key));
                while (p.ReadNextPage(x => new { x.Id, x.EntityId, x.Value }, x => x.Id, out var list))
                {
                    foreach (var x in list)
                    {
                        var id = x.Value.ToInt();
                        if (id > 0)
                        {
                            yield return new MediaTrack { EntityId = x.EntityId, EntityName = name, MediaFileId = id, Property = key };
                        }
                    }
                    list.Clear();
                }

                yield break;
            }
        }

        #endregion
    }
}
