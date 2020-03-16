using System;
using System.Linq;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public partial class MediaSearchResult : PagedListBase
    {
        public MediaSearchResult(IPagedList<MediaFile> pageable, Func<MediaFile, MediaFileInfo> converter) : base(pageable)
        {
            Files = pageable.Select(converter).ToList();
        }

        public IList<MediaFileInfo> Files { get; private set; }
    }
}
