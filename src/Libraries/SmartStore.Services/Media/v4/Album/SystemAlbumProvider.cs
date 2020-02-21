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

namespace SmartStore.Services.Media
{
    public partial class SystemAlbumProvider : IAlbumProvider, IMediaRelationDetector
    {
        private readonly IDbContext _dbContext;
        
        public SystemAlbumProvider(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public const string Products = "product";
        public const string Categories = "category";
        public const string Brands = "brand";
        public const string Customers = "customer";
        public const string Blog = "blog";
        public const string News = "news";
        public const string Forums = "forum";
        public const string Downloads = "download";
        public const string Messages = "message";
        public const string Files = "file";

        #region Album Provider

        public IEnumerable<MediaAlbum> GetAlbums()
        {
            return new[]
            {
                new MediaAlbum
                {
                    Name = Products,
                    ResKey = "Admin.Catalog.Products",
                    CanTrackRelations = true,
                    Order = int.MinValue
                },
                new MediaAlbum
                {
                    Name = Categories,
                    ResKey = "Admin.Catalog.Categories",
                    CanTrackRelations = true,
                    Order = int.MinValue + 10
                },
                new MediaAlbum
                {
                    Name = Brands,
                    ResKey = "Manufacturers",
                    CanTrackRelations = true,
                    Order = int.MinValue + 20
                },
                new MediaAlbum
                {
                    Name = Customers,
                    ResKey = "Admin.Customers",
                    CanTrackRelations = true, // TBD
                    Order = int.MinValue + 30
                },
                new MediaAlbum
                {
                    Name = Blog,
                    ResKey = "Blog",
                    CanTrackRelations = true,
                    Order = int.MinValue + 40
                },
                new MediaAlbum
                {
                    Name = News,
                    ResKey = "News",
                    CanTrackRelations = true,
                    Order = int.MinValue + 50
                },
                new MediaAlbum
                {
                    Name = Forums,
                    ResKey = "Forum.Forum",
                    CanTrackRelations = false, // TBD
                    Order = int.MinValue + 60
                },
                new MediaAlbum
                {
                    Name = Downloads,
                    ResKey = "Common.Downloads",
                    CanTrackRelations = true,
                    Order = int.MinValue + 70
                },
                new MediaAlbum
                {
                    Name = Messages,
                    ResKey = "Admin.Media.Album.Message",
                    CanTrackRelations = true,
                    Order = int.MinValue + 80
                },
                new MediaAlbum
                {
                    Name = Files,
                    ResKey = "Admin.Media.Album.File",
                    CanTrackRelations = false,
                    // Slug = "uploaded", // TBD: hmmmm??
                    Order = int.MaxValue
                }
            };
        }

        public MediaAlbumDisplayHint GetDisplayHint(MediaAlbum album)
        {
            if (album.Name == Products)
            {
                return new MediaAlbumDisplayHint { OverlayIcon = "fa fa-cube" };
            }
            if (album.Name == Categories)
            {
                return new MediaAlbumDisplayHint { OverlayIcon = "fa fa-sitemap" };
            }
            if (album.Name == Brands)
            {
                return new MediaAlbumDisplayHint { OverlayIcon = "far fa-building" };
            }
            if (album.Name == Customers)
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
            if (album.Name == Forums)
            {
                return new MediaAlbumDisplayHint { OverlayIcon = "fa fa-users" };
            }
            if (album.Name == Downloads)
            {
                return new MediaAlbumDisplayHint { OverlayIcon = "fa fa-download" };
            }
            if (album.Name == Messages)
            {
                return new MediaAlbumDisplayHint { OverlayIcon = "fa fa-envelope" };
            }
            if (album.Name == Files)
            {
                // TODO: var(--success) should be system default.
                return new MediaAlbumDisplayHint { Color = "var(--success)" };
            }

            return null;
        }

        #endregion

        #region Relation Detector

        public IEnumerable<MediaRelation> DetectAllRelations(string albumName)
        {
            var ctx = _dbContext;
            var entityName = string.Empty;

            // TODO: Messages, Downloads, Forums (?), Store (?)

            // Products
            if (albumName == Products)
            {
                // Products
                {
                    var name = nameof(Product);
                    var p = new FastPager<ProductMediaFile>(ctx.Set<ProductMediaFile>().AsNoTracking());
                    while (p.ReadNextPage(x => new { x.Id, x.ProductId, x.MediaFileId }, x => x.Id, out var list))
                    {
                        foreach (var x in list)
                        {
                            yield return new MediaRelation { EntityId = x.ProductId, EntityName = name, MediaFileId = x.MediaFileId };
                        }
                    }
                }

                // ProductAttributeOption
                {
                    var name = nameof(ProductAttributeOption);
                    var p = new FastPager<ProductAttributeOption>(ctx.Set<ProductAttributeOption>().AsNoTracking().Where(x => x.MediaFileId > 0));
                    while (p.ReadNextPage(x => new { x.Id, x.MediaFileId }, x => x.Id, out var list))
                    {
                        foreach (var x in list)
                        {
                            yield return new MediaRelation { EntityId = x.Id, EntityName = name, MediaFileId = x.MediaFileId };
                        }
                    }
                }

                // ProductVariantAttributeValue
                {
                    var name = nameof(ProductVariantAttributeValue);
                    var p = new FastPager<ProductVariantAttributeValue>(ctx.Set<ProductVariantAttributeValue>().AsNoTracking().Where(x => x.MediaFileId > 0));
                    while (p.ReadNextPage(x => new { x.Id, x.MediaFileId }, x => x.Id, out var list))
                    {
                        foreach (var x in list)
                        {
                            yield return new MediaRelation { EntityId = x.Id, EntityName = name, MediaFileId = x.MediaFileId };
                        }
                    }
                }

                // SpecificationAttributeOption
                {
                    var name = nameof(SpecificationAttributeOption);
                    var p = new FastPager<SpecificationAttributeOption>(ctx.Set<SpecificationAttributeOption>().AsNoTracking().Where(x => x.MediaFileId > 0));
                    while (p.ReadNextPage(x => new { x.Id, x.MediaFileId }, x => x.Id, out var list))
                    {
                        foreach (var x in list)
                        {
                            yield return new MediaRelation { EntityId = x.Id, EntityName = name, MediaFileId = x.MediaFileId };
                        }
                    }
                }
            }

            // Categories
            if (albumName == Categories)
            {
                var name = nameof(Category);
                var p = new FastPager<Category>(ctx.Set<Category>().AsNoTracking().Where(x => x.MediaFileId.HasValue));
                while (p.ReadNextPage(x => new { x.Id, x.MediaFileId }, x => x.Id, out var list))
                {
                    foreach (var x in list)
                    {
                        yield return new MediaRelation { EntityId = x.Id, EntityName = name, MediaFileId = x.MediaFileId.Value };
                    }
                }
            }


            // Brands
            if (albumName == Brands)
            {
                var name = nameof(Manufacturer);
                var p = new FastPager<Manufacturer>(ctx.Set<Manufacturer>().AsNoTracking().Where(x => x.MediaFileId.HasValue));
                while (p.ReadNextPage(x => new { x.Id, x.MediaFileId }, x => x.Id, out var list))
                {
                    foreach (var x in list)
                    {
                        yield return new MediaRelation { EntityId = x.Id, EntityName = name, MediaFileId = x.MediaFileId.Value };
                    }
                }
            }

            // BlogPost
            if (albumName == Blog)
            {
                var name = nameof(BlogPost);
                var p = new FastPager<BlogPost>(ctx.Set<BlogPost>().AsNoTracking().Where(x => x.MediaFileId.HasValue || x.PreviewMediaFileId.HasValue));
                while (p.ReadNextPage(x => new { x.Id, x.MediaFileId, x.PreviewMediaFileId }, x => x.Id, out var list))
                {
                    foreach (var x in list)
                    {
                        if (x.MediaFileId.HasValue)
                            yield return new MediaRelation { EntityId = x.Id, EntityName = name, MediaFileId = x.MediaFileId.Value };
                        if (x.PreviewMediaFileId.HasValue)
                            yield return new MediaRelation { EntityId = x.Id, EntityName = name, MediaFileId = x.PreviewMediaFileId.Value };
                    }
                }
            }

            // NewsItem
            if (albumName == News)
            {
                var name = nameof(NewsItem);
                var p = new FastPager<NewsItem>(ctx.Set<NewsItem>().AsNoTracking().Where(x => x.MediaFileId.HasValue || x.PreviewMediaFileId.HasValue));
                while (p.ReadNextPage(x => new { x.Id, x.MediaFileId, x.PreviewMediaFileId }, x => x.Id, out var list))
                {
                    foreach (var x in list)
                    {
                        if (x.MediaFileId.HasValue)
                            yield return new MediaRelation { EntityId = x.Id, EntityName = name, MediaFileId = x.MediaFileId.Value };
                        if (x.PreviewMediaFileId.HasValue)
                            yield return new MediaRelation { EntityId = x.Id, EntityName = name, MediaFileId = x.PreviewMediaFileId.Value };
                    }
                }
            }

            // Customer
            if (albumName == Customers)
            {
                var name = nameof(Customers);

                // Avatars
                var p = new FastPager<GenericAttribute>(ctx.Set<GenericAttribute>().AsNoTracking()
                    .Where(x => x.KeyGroup == nameof(Customer) && x.Key == SystemCustomerAttributeNames.AvatarPictureId));
                while (p.ReadNextPage(x => new { x.Id, x.EntityId, x.Value }, x => x.Id, out var list))
                {
                    foreach (var x in list)
                    {
                        var id = x.Value.ToInt();
                        if (id > 0)
                        {
                            yield return new MediaRelation { EntityId = x.EntityId, EntityName = name, MediaFileId = id };
                        }
                    }
                }
            }

            // Downloads
            if (albumName == Downloads)
            {
                var name = nameof(Download);
                var p = new FastPager<Download>(ctx.Set<Download>().AsNoTracking().Where(x => x.MediaFileId.HasValue));
                while (p.ReadNextPage(x => new { x.Id, x.MediaFileId }, x => x.Id, out var list))
                {
                    foreach (var x in list)
                    {
                        yield return new MediaRelation { EntityId = x.Id, EntityName = name, MediaFileId = x.MediaFileId.Value };
                    }
                }
            }
        }

        #endregion
    }
}
