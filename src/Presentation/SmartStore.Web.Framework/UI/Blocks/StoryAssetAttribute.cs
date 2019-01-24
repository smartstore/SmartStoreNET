using System;

namespace SmartStore.Web.Framework.UI.Blocks
{
    /// <summary>
    /// Specifies whether a property refers to an asset to be included in the story export.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class StoryAssetAttribute : Attribute
    {
        public StoryAssetAttribute(
            StoryAssetKind type,
            Type root,
            string rootProperty)
        {
            Guard.NotNull(root, nameof(root));
            Guard.NotEmpty(rootProperty, nameof(rootProperty));

            Kind = type;
            Root = root;
            RootProperty = rootProperty;
        }

        /// <summary>
        /// The asset property type.
        /// </summary>
        public StoryAssetKind Kind { get; private set; }

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


    public enum StoryAssetKind
    {
        /// <summary>
        /// The property value is a picture identifier.
        /// </summary>
        Picture = 0
    }


    /// <summary>
    /// Required to identify export assets containing models.
    /// </summary>
    public interface IStoryExportModel
    {
    }
}
