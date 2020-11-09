namespace SmartStore.Core.Domain.Media
{
    public interface IHasMedia
    {
        /// <summary>
        /// Gets or sets the media storage identifier
        /// </summary>
        int? MediaStorageId { get; set; }

        /// <summary>
        /// Gets or sets the media storage
        /// </summary>
        MediaStorage MediaStorage { get; set; }
    }
}
