using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Topics;
using SmartStore.Data;

namespace SmartStore.Services.Cms
{
    public partial class LinkResolverInvalidator : DbSaveHook<BaseEntity>
    {
        private readonly ICommonServices _services;

        private static readonly HashSet<string> _toxicProps = new HashSet<string>
        {
            nameof(Topic.SystemName),
            nameof(Topic.IsPublished),
            nameof(Topic.SubjectToAcl),
            nameof(Topic.LimitedToStores),
            nameof(Topic.Title),
            nameof(Topic.ShortTitle),
            nameof(Topic.Intro),
            nameof(Product.Name),
            nameof(Product.Deleted),
            nameof(Product.MainPictureId),
            nameof(Category.Published),
            nameof(Category.MediaFileId),
            nameof(StoreMapping.StoreId),
            nameof(UrlRecord.IsActive),
            nameof(UrlRecord.Slug)
        };

        public LinkResolverInvalidator(ICommonServices services)
        {
            _services = services;
        }

        public override void OnBeforeSave(IHookedEntity entry)
        {
            var cache = _services.Cache;
            var e = entry.Entity;

            var evict = e is Topic || e is Product || e is Category || e is Manufacturer || e is StoreMapping || e is UrlRecord;
            if (!evict)
                throw new NotSupportedException(); // Perf

            if (evict && entry.InitialState == EntityState.Modified)
            {
                var modProps = entry.Entry.GetModifiedProperties(_services.DbContext);
                evict = modProps.Keys.Any(x => _toxicProps.Contains(x));
            }

            if (!evict)
                return;

            int? evictTopicId = null;

            if (e is Topic t)
            {
                cache.RemoveByPattern(BuildPatternKey("topic", t.Id));
                cache.RemoveByPattern(BuildPatternKey("topic", t.SystemName));
            }
            else if (e is Category || e is Product || e is Manufacturer)
            {
                cache.RemoveByPattern(BuildPatternKey(e.GetEntityName().ToLowerInvariant(), e.Id));
            }
            else if (e is UrlRecord ur)
            {
                cache.RemoveByPattern(BuildPatternKey(ur.EntityName.ToLowerInvariant(), ur.EntityId));
                evictTopicId = ur.EntityId;
            }
            else if (e is StoreMapping sm)
            {
                cache.RemoveByPattern(BuildPatternKey(sm.EntityName.ToLowerInvariant(), sm.EntityId));
                evictTopicId = sm.EntityId;
            }

            if (evictTopicId.HasValue)
            {
                var systemName = _services.DbContext.Set<Topic>().Where(x => x.Id == evictTopicId.Value).Select(x => x.SystemName).FirstOrDefault();
                if (systemName.HasValue())
                {
                    cache.RemoveByPattern(BuildPatternKey("topic", systemName));
                }
            }
        }

        private string BuildPatternKey(string entityName, object ident)
        {
            return LinkResolver.LINKRESOLVER_PATTERN_KEY.FormatInvariant(String.Concat(entityName, ":", ident));
        }
    }
}
