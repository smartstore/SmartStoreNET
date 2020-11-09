using System;
using System.Linq;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public partial class MediaSearchResult : PagedListBase, IEnumerable<MediaFileInfo>
    {
        private readonly IList<MediaFileInfo> _files;

        public MediaSearchResult(IPagedList<MediaFile> pageable, Func<MediaFile, MediaFileInfo> converter) : base(pageable)
        {
            _files = pageable.Select(converter).ToList();
        }

        IEnumerator<MediaFileInfo> IEnumerable<MediaFileInfo>.GetEnumerator()
        {
            return _files.GetEnumerator();
        }
    }
}
