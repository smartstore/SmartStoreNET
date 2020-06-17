using System;

namespace SmartStore.Services.Cms.Blocks
{
    /// <summary>
    /// Specifies whether a property refers to an asset to be included in the story export.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class StoryAssetAttribute : Attribute
    {
        public StoryAssetAttribute(StoryAssetKind kind)
        {
            Kind = kind;
        }

        /// <summary>
        /// The asset property kind.
        /// </summary>
        public StoryAssetKind Kind { get; private set; }
    }


    public enum StoryAssetKind
    {
        /// <summary>
        /// The property value is a picture identifier.
        /// </summary>
        Picture = 0,

        /// <summary>
        /// The property value is a video path.
        /// </summary>
        Video,

        /// <summary>
        /// The property value is a product identifier.
        /// </summary>
        Product,

        /// <summary>
        /// The property value is a category identifier.
        /// </summary>
        Category,

        /// <summary>
        /// The property value is a manufacturer identifier.
        /// </summary>
        Manufacturer,

        /// <summary>
        /// The property value is a link builder expression.
        /// </summary>
        Link
    }
}
