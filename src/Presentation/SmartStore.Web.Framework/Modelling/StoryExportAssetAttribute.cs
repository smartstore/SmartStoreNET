using System;

namespace SmartStore.Web.Framework.Modelling
{
    /// <summary>
    /// Specifies whether a property refers to an asset to be included in the story export.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class StoryExportAssetAttribute : Attribute
    {
        public StoryExportAssetAttribute(
            StoryExportPropertyType type,
            Type root,
            string rootProperty)
        {
            Guard.NotNull(root, nameof(root));
            Guard.NotEmpty(rootProperty, nameof(rootProperty));

            Type = type;
            Root = root;
            RootProperty = rootProperty;
        }

        /// <summary>
        /// The asset property type.
        /// </summary>
        public StoryExportPropertyType Type { get; private set; }

        /// <summary>
        /// The root entity type that contains the model data.
        /// </summary>
        public Type Root { get; private set; }

        /// <summary>
        /// The root property name that contains the model data.
        /// It is expected that the property value is always a Json serialized string.
        /// </summary>
        public string RootProperty { get; private set; }
    }


    public enum StoryExportPropertyType
    {
        /// <summary>
        /// The property value is a picture identifier.
        /// </summary>
        PictureId = 0
    }


    /// <summary>
    /// Required to identify export assets containing models.
    /// </summary>
    public interface IStoryExportModel
    {
    }
}
