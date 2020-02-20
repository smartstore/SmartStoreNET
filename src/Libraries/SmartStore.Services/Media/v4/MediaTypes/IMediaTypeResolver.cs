using System;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public interface IMediaTypeResolver
    {
        MediaType Resolve(MediaFile file);
    }
}
