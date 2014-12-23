using System.Diagnostics;
namespace SmartStore.Core.Domain.Localization
{
    /// <summary>
    /// Represents a locale string resource
    /// </summary>
	[DebuggerDisplay("{ResourceName} - {ResourceValue}")]
    public partial class LocaleStringResource : BaseEntity
    {
        /// <summary>
        /// Gets or sets the language identifier
        /// </summary>
        public int LanguageId { get; set; }

        /// <summary>
        /// Gets or sets the resource name
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the resource value
        /// </summary>
        public string ResourceValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this resource was installed by a plugin
        /// </summary>
        public bool? IsFromPlugin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this resource was modified by the user
        /// </summary>
        public bool? IsTouched { get; set; }

        /// <summary>
        /// Gets or sets the language
        /// </summary>
        public virtual Language Language { get; set; }

    }

}
