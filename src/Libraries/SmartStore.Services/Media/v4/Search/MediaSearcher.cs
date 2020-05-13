using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using SmartStore.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Infrastructure.DependencyManagement;
using System.Linq.Expressions;

namespace SmartStore.Services.Media
{
    public class MediaSearcher : IMediaSearcher
    {
        private readonly IRepository<MediaFile> _fileRepo;
        private readonly Work<IFolderService> _folderService;

        public MediaSearcher(
            IRepository<MediaFile> fileRepo, 
            Work<IFolderService> folderService)
        {
            _fileRepo = fileRepo;
            _folderService = folderService;
        }

        public virtual IPagedList<MediaFile> SearchFiles(MediaSearchQuery query, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking)
        {
            var q = PrepareQuery(query, flags);
            return new PagedList<MediaFile>(q, query.PageIndex, query.PageSize);
        }

        public virtual IQueryable<MediaFile> PrepareQuery(MediaSearchQuery query, MediaLoadFlags flags)
        {
            Guard.NotNull(query, nameof(query));

            var q = PrepareFilterQuery(query);
            bool? shouldIncludeDeleted = false;

            // Folder
            if (query.FolderId > 0)
            {
                if (query.DeepSearch)
                {
                    var folderIds = _folderService.Value.GetNodesFlattened(query.FolderId.Value, true).Select(x => x.Id).ToArray();
                    q = q.Where(x => x.FolderId != null && folderIds.Contains(x.FolderId.Value));
                }
                else
                {
                    q = q.Where(x => x.FolderId == query.FolderId);
                }
            }
            else if (query.FolderId < 0)
            {
                // Special folders
                if (query.FolderId == (int)SpecialMediaFolder.AllFiles)
                {
                    shouldIncludeDeleted = null;
                }
                else if (query.FolderId == (int)SpecialMediaFolder.Trash)
                {
                    shouldIncludeDeleted = true;
                }
                else if (query.FolderId == (int)SpecialMediaFolder.Orphans)
                {
                    // Get ids of untrackable folders, 'cause no orphan check can be made for them.
                    var untrackableFolderIds = _folderService.Value.GetRootNode()
                        .SelectNodes(x => !x.Value.CanDetectTracks)
                        .Select(x => x.Value.Id)
                        .ToArray();

                    q = q.Where(x => x.FolderId > 0 && !untrackableFolderIds.Contains(x.FolderId.Value) && !x.Tracks.Any());
                }
                else if (query.FolderId == (int)SpecialMediaFolder.TransientFiles)
                {
                    q = q.Where(x => x.IsTransient);
                }
                else if (query.FolderId == (int)SpecialMediaFolder.UnassignedFiles)
                {
                    q = q.Where(x => x.FolderId == null);
                }
            }

            if (query.Deleted == null && shouldIncludeDeleted.HasValue)
            {
                q = q.Where(x => x.Deleted == shouldIncludeDeleted.Value);
            }

            // Sorting
            var ordering = query.SortBy;
            if (ordering.HasValue())
            {
                if (query.SortDescending) ordering += " descending";
                q = q.OrderBy(ordering);
            }

            return ApplyLoadFlags(q, flags);
        }

        public virtual IQueryable<MediaFile> PrepareFilterQuery(MediaFilesFilter filter)
        {
            Guard.NotNull(filter, nameof(filter));

            var q = _fileRepo.Table;

            // Deleted
            if (filter.Deleted != null)
            {
                q = q.Where(x => x.Deleted == filter.Deleted.Value);
            }

            // Hidden
            if (filter.Hidden != null)
            {
                q = q.Where(x => x.Hidden == filter.Hidden.Value);
            }

            // MimeType
            if (filter.MimeTypes != null && filter.MimeTypes.Length > 0)
            {
                q = q.Where(x => filter.MimeTypes.Contains(x.MimeType));
            }

            // Extension
            if (filter.Extensions != null && filter.Extensions.Length > 0)
            {
                q = q.Where(x => filter.Extensions.Contains(x.Extension));
            }

            // MediaType
            if (filter.MediaTypes != null && filter.MediaTypes.Length > 0)
            {
                q = q.Where(x => filter.MediaTypes.Contains(x.MediaType));
            }

            // Tags
            if (filter.Tags != null && filter.Tags.Length > 0)
            {
                q = q.Where(x => x.Tags.Any(t => filter.Tags.Contains(t.Id)));
            }

            // Extensions
            if (filter.Dimensions != null && filter.Dimensions.Length > 0)
            {
                var predicates = new List<Expression<Func<MediaFile, bool>>>(5);

                foreach (var dim in filter.Dimensions.OrderBy(x => x).Distinct()) 
                {
                    if (dim == ImageDimension.VerySmall)
                        predicates.Add(x => x.PixelSize > 0 && x.PixelSize <= 50000);
                    else if (dim == ImageDimension.Small)
                        predicates.Add(x => x.PixelSize > 50000 && x.PixelSize <= 250000);
                    else if (dim == ImageDimension.Medium)
                        predicates.Add(x => x.PixelSize > 250000 && x.PixelSize <= 1000000);
                    else if (dim == ImageDimension.Large)
                        predicates.Add(x => x.PixelSize > 1000000 && x.PixelSize <= 2000000);
                    else if (dim == ImageDimension.VeryLarge)
                        predicates.Add(x => x.PixelSize > 2000000);
                }

                if (predicates.Any())
                {
                    var predicate = PredicateBuilder.New(predicates.First());
                    for (var i = 1; i < predicates.Count; i++)
                    {
                        predicate = PredicateBuilder.Or(predicate, predicates[i]);
                    }
                    q = q.Where(predicate);
                }
            }

            // Term
            if (filter.Term.HasValue() && filter.Term != "*")
            {
                // Convert file pattern to SQL 'LIKE' expression
                q = ApplySearchTerm(q, filter.Term, filter.IncludeAltForTerm, filter.ExactMatch);
            }

            return q;
        }

        public virtual IQueryable<MediaFile> ApplyLoadFlags(IQueryable<MediaFile> query, MediaLoadFlags flags)
        {
            if (flags == MediaLoadFlags.None)
            {
                return query;
            }

            if (flags.HasFlag(MediaLoadFlags.AsNoTracking))
            {
                query = query.AsNoTracking();
            }

            if (flags.HasFlag(MediaLoadFlags.WithBlob))
            {
                query = query.Include(x => x.MediaStorage);
            }

            if (flags.HasFlag(MediaLoadFlags.WithFolder))
            {
                query = query.Include(x => x.Folder);
            }

            if (flags.HasFlag(MediaLoadFlags.WithTags))
            {
                query = query.Include(x => x.Tags);
            }

            if (flags.HasFlag(MediaLoadFlags.WithTracks))
            {
                query = query.Include(x => x.Tracks);
            }

            return query;
        }


        private IQueryable<MediaFile> ApplySearchTerm(IQueryable<MediaFile> query, string term, bool includeAlt, bool exactMatch)
        {
            var hasAnyCharToken = term.IndexOf('*') > -1;
            var hasSingleCharToken = term.IndexOf('?') > -1;
            var hasAnyWildcard = hasAnyCharToken || hasSingleCharToken;

            if (!hasAnyWildcard)
            {
                return !includeAlt
                    ? (exactMatch ? query.Where(x => x.Name == term) : query.Where(x => x.Name.Contains(term)))
                    : (exactMatch ? query.Where(x => x.Name == term || x.Alt == term) : query.Where(x => x.Name.Contains(term) || x.Alt.Contains(term)));
            }
            else
            {
                // Convert file pattern to SQL LIKE expression:
                // my*new_file-?.png > my%new/_file-_.png

                var hasUnderscore = term.IndexOf('_') > -1;

                if (hasUnderscore)
                {
                    term = term.Replace("_", "/_");
                }
                if (hasAnyCharToken)
                {
                    term = term.Replace('*', '%');
                }
                if (hasSingleCharToken)
                {
                    term = term.Replace('?', '_');
                }

                return !includeAlt
                    ? query.Where(x => DbFunctions.Like(x.Name, term, "/"))
                    : query.Where(x => DbFunctions.Like(x.Name, term, "/") || DbFunctions.Like(x.Alt, term, "/"));
            }
        }
    }
}