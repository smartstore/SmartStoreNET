using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Media.Imaging;

namespace SmartStore.Services.Media
{
    /// <summary>
    /// Generates URLs for media files
    /// </summary>
    public interface IMediaUrlGenerator
    {
        /// <summary>
        /// Generates a public URL for a media file
        /// </summary>
        /// <param name="file">The file to create a URL for</param>
        /// <param name="imageQuery">Query for image processing / thumbnails.</param>
        /// <param name="host">
        ///     Store host for an absolute URL that also contains scheme and host parts. 
        ///     <c>null</c>: tries to resolve host automatically based on <see cref="Store.ContentDeliveryNetwork"/> or <see cref="MediaSettings.AutoGenerateAbsoluteUrls"/>.
        ///     <c>String.Empty</c>: bypasses automatic host resolution and does NOT prepend host to path.
        ///     <c>Any string</c>: host name to use explicitly.
        /// </param>
        /// <param name="doFallback">
        /// Specifies behaviour in case URL generation fails.
        ///     <c>false</c>: return <c>null</c>.
        ///     <c>true</c>: return URL to a fallback image which is <c>~/Content/images/default-image.png</c> by default but can be modified with hidden setting <c>Media.DefaultImageName</c>
        /// </param>
        /// <returns>The passed file's public URL.</returns>
        string GenerateUrl(
            MediaFileInfo file,
            ProcessImageQuery imageQuery,
            string host = null,
            bool doFallback = true);
    }
}
