using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Infrastructure.DependencyManagement;

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

            var q = _fileRepo.TableUntracked;

            // Deleted
            if (query.Deleted != null)
            {
                q = q.Where(x => x.Deleted == query.Deleted.Value);
            }

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

            // MimeType
            if (query.MimeTypes != null && query.MimeTypes.Length > 0)
            {
                q = q.Where(x => query.MimeTypes.Contains(x.MimeType));
            }

            // Extension
            if (query.Extensions != null && query.Extensions.Length > 0)
            {
                q = q.Where(x => query.Extensions.Contains(x.Extension));
            }

            // MediaType
            if (query.MediaTypes != null && query.MediaTypes.Length > 0)
            {
                q = q.Where(x => query.MediaTypes.Contains(x.MediaType));
            }

            // Tag
            if (query.Tags != null && query.Tags.Length > 0)
            {
                q = q.Where(x => x.Tags.Any(t => query.Tags.Contains(t.Id)));
            }

            // Hidden
            if (query.Hidden != null)
            {
                q = q.Where(x => x.Hidden == query.Hidden.Value);
            }

            // Term
            if (query.Term.HasValue() && query.Term != "*")
            {
                // Convert file pattern to SQL 'LIKE' expression
                q = ApplySearchTerm(q, query.Term, query.IncludeAltForTerm, query.ExactMatch);
            }

            // Sorting
            var ordering = query.SortBy.NullEmpty() ?? nameof(MediaFile.Name);
            if (query.SortDescending) ordering += " descending";
            q = q.OrderBy(ordering);

            return ApplyLoadFlags(q, flags);
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