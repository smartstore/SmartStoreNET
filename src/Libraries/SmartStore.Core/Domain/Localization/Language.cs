using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Core.Domain.Localization
{
    /// <summary>
    /// Represents a language
    /// </summary>
	[DataContract]
    [DebuggerDisplay("{LanguageCulture}")]
    public partial class Language : BaseEntity, IStoreMappingSupported
    {
        private ICollection<LocaleStringResource> _localeStringResources;

        /// <summary>
        /// Gets or sets the name
        /// </summary>
		[DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the language culture (e.g. "en-US")
        /// </summary>
		[DataMember]
        public string LanguageCulture { get; set; }

        /// <summary>
        /// Gets or sets the unique SEO code (e.g. "en")
        /// </summary>
		[DataMember]
        public string UniqueSeoCode { get; set; }

        /// <summary>
        /// Gets or sets the flag image file name
        /// </summary>
        [DataMember]
        public string FlagImageFileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the language supports "Right-to-left"
        /// </summary>
		[DataMember]
        public bool Rtl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        [DataMember]
        public bool LimitedToStores { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the language is published
        /// </summary>
		[DataMember]
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
		[DataMember]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets locale string resources
        /// </summary>
        public virtual ICollection<LocaleStringResource> LocaleStringResources
        {
            get => _localeStringResources ?? (_localeStringResources = new HashSet<LocaleStringResource>());
            protected set => _localeStringResources = value;
        }

        public string GetTwoLetterISOLanguageName()
        {
            if (UniqueSeoCode.HasValue())
            {
                return UniqueSeoCode;
            }

            try
            {
                var ci = new CultureInfo(LanguageCulture);
                return ci.TwoLetterISOLanguageName;
            }
            catch { }

            return null;
        }
    }
}
