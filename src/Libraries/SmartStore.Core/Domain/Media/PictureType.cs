namespace SmartStore.Core.Domain.Media
{
    /// <summary>
    /// Represents a picture item type
    /// </summary>
    public enum PictureType
    {
        /// <summary>
        /// Entities (products, categories, manufacturers)
        /// </summary>
        Entity = 1,
        /// <summary>
        /// Avatar
        /// </summary>
        Avatar = 10,
    }

    public enum ThumbnailScaleMode
    {
        Auto,
        UseWidth,
        UseHeight
    }
}
