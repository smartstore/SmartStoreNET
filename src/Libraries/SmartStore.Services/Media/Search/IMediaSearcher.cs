using System;
using System.Linq;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public interface IMediaSearcher
    {
        IPagedList<MediaFile> SearchFiles(MediaSearchQuery query, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking);
        IQueryable<MediaFile> PrepareQuery(MediaSearchQuery query, MediaLoadFlags flags);
        IQueryable<MediaFile> ApplyFilterQuery(MediaFilesFilter filter, IQueryable<MediaFile> sourceQuery = null);
        IQueryable<MediaFile> ApplyLoadFlags(IQueryable<MediaFile> query, MediaLoadFlags flags);
    }
}