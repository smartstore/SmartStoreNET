using System;

namespace SmartStore.Web.Framework.Modelling
{
    /// <summary>
    /// Specifies whether a property refers to an asset to be included in the story export.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class StoryExportAssetAttribute : Attribute
    {
        public StoryExportAssetAttribute(StoryExportPropertyType type)
        {
            Type = type;
        }

        /// <summary>
        /// The property type.
        /// </summary>
        public StoryExportPropertyType Type { get; private set; }
    }


    public enum StoryExportPropertyType
    {
        /// <summary>
        /// The property value is a picture identifier.
        /// </summary>
        PictureId = 0
    }
}
