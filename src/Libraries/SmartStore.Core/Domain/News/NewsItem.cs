using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Core.Domain.News
{
    /// <summary>
    /// Represents a news item
    /// </summary>
	public partial class NewsItem : BaseEntity, ISlugSupported, IStoreMappingSupported, ILocalizedEntity
    {
        #region static

        private static readonly List<string> _visibilityAffectingProps = new List<string>
        {
            nameof(NewsItem.Published),
            nameof(NewsItem.StartDateUtc),
            nameof(NewsItem.EndDateUtc),
            nameof(NewsItem.LimitedToStores)
        };

        public static IReadOnlyCollection<string> GetVisibilityAffectingPropertyNames()
        {
            return _visibilityAffectingProps;
        }

        #endregion

        private ICollection<NewsComment> _newsComments;

        /// <summary>
        /// Gets or sets the news title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the short text
        /// </summary>
        public string Short { get; set; }

        /// <summary>
        /// Gets or sets the full text
        /// </summary>
        public string Full { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the news item is published
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the media file identifier
        /// </summary>
        public int? MediaFileId { get; set; }

        /// Gets or sets the media file.
        /// </summary>
        public virtual MediaFile MediaFile { get; set; }

        /// <summary>
        /// Gets or sets the preview media file identifier
        /// </summary>
        public int? PreviewMediaFileId { get; set; }

        /// <summary>
        /// Gets or sets the preview media file.
        /// </summary>
        public virtual MediaFile PreviewMediaFile { get; set; }

        /// <summary>
        /// Gets or sets the news item start date and time
        /// </summary>
        public DateTime? StartDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the news item end date and time
        /// </summary>
        public DateTime? EndDateUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the news post comments are allowed 
        /// </summary>
        public bool AllowComments { get; set; }

        /// <summary>
        /// Gets or sets the total number of approved comments
        /// <remarks>The same as if we run newsItem.NewsComments.Where(n => n.IsApproved).Count()
        /// We use this property for performance optimization (no SQL command executed)
        /// </remarks>
        /// </summary>
        public int ApprovedCommentCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of not approved comments
        /// <remarks>The same as if we run newsItem.NewsComments.Where(n => !n.IsApproved).Count()
        /// We use this property for performance optimization (no SQL command executed)
        /// </remarks>
        /// </summary>
        public int NotApprovedCommentCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        public bool LimitedToStores { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the meta keywords
        /// </summary>
        public string MetaKeywords { get; set; }

        /// <summary>
        /// Gets or sets the meta description
        /// </summary>
        public string MetaDescription { get; set; }

        /// <summary>
        /// Gets or sets the meta title
        /// </summary>
        public string MetaTitle { get; set; }

        /// <summary>
        /// Gets or sets a language identifier for which the news item should be displayed.
        /// </summary>
        [Index]
        public int? LanguageId { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        public virtual Language Language { get; set; }

        /// <summary>
        /// Gets or sets the news comments
        /// </summary>
        public virtual ICollection<NewsComment> NewsComments
        {
            get => _newsComments ?? (_newsComments = new HashSet<NewsComment>());
            protected set => _newsComments = value;
        }
    }
}