using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public interface IMediaUrlGenerator
    {
        string GenerateUrl(
            MediaFileInfo file, 
            ProcessImageQuery imageQuery, 
            string host = null,
            bool doFallback = true);
    }
}
