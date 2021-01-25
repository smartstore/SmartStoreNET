using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Core.Domain.Blogs
{
    /// <summary>
    /// Represents a blog post
    /// </summary>
    [DataContract]
    public partial class BlogPost : BaseEntity, ISlugSupported, IStoreMappingSupported, ILocalizedEntity
    {
        #region static

        private static readonly List<string> _visibilityAffectingProps = new List<string>
        {
            nameof(BlogPost.IsPublished),
            nameof(BlogPost.StartDateUtc),
            nameof(BlogPost.EndDateUtc),
            nameof(BlogPost.LimitedToStores)
        };

        public static IReadOnlyCollection<string> GetVisibilityAffectingPropertyNames()
        {
            return _visibilityAffectingProps;
        }

        #endregion

        private ICollection<BlogComment> _blogComments;

        /// <summary>
        /// Gets or sets a value indicating whether the blog post comments are allowed 
        /// </summary>
        [DataMember]
        public bool IsPublished { get; set; }

        /// <summary>
        /// Gets or sets the blog post title
        /// </summary>
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// Defines the preview display type of the picture
        /// </summary>
        [DataMember]
        public PreviewDisplayType PreviewDisplayType { get; set; }

        /// <summary>
        /// Gets or sets the media file identifier
        /// </summary>
        [DataMember]
        public int? MediaFileId { get; set; }

        /// <summary>
        /// Gets or sets the media file.
        /// </summary>
        public virtual MediaFile MediaFile { get; set; }

        /// <summary>
        /// Gets or sets the preview media file identifier
        /// </summary>
        [DataMember]
        public int? PreviewMediaFileId { get; set; }

        /// <summary>
        /// Gets or sets the preview media file.
        /// </summary>
        public virtual MediaFile PreviewMediaFile { get; set; }

        /// <summary>
        /// Gets or sets background for the blog post
        /// </summary>
        [DataMember]
        public string SectionBg { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the blog post has a background image
        /// </summary>
        [DataMember]
        public bool DisplayTagsInPreview { get; set; }

        /// <summary>
        /// Gets or sets the blog post intro
        /// </summary>
        [DataMember]
        public string Intro { get; set; }

        /// <summary>
        /// Gets or sets the blog post title
        /// </summary>
        [DataMember]
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the blog post comments are allowed 
        /// </summary>
        [DataMember]
        public bool AllowComments { get; set; }

        /// <summary>
        /// Gets or sets the total number of approved comments
        /// <remarks>The same as if we run newsItem.NewsComments.Where(n => n.IsApproved).Count()
        /// We use this property for performance optimization (no SQL command executed)
        /// </remarks>
        /// </summary>
        [DataMember]
        public int ApprovedCommentCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of not approved comments
        /// <remarks>The same as if we run newsItem.NewsComments.Where(n => !n.IsApproved).Count()
        /// We use this property for performance optimization (no SQL command executed)</remarks>
        /// </summary>
        [DataMember]
        public int NotApprovedCommentCount { get; set; }

        /// <summary>
        /// Gets or sets the blog tags
        /// </summary>
        [DataMember]
        public string Tags { get; set; }

        /// <summary>
        /// Gets or sets the blog post start date and time
        /// </summary>
        [DataMember]
        public DateTime? StartDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the blog post end date and time
        /// </summary>
        [DataMember]
        public DateTime? EndDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the meta keywords
        /// </summary>
        [DataMember]
        public string MetaKeywords { get; set; }

        /// <summary>
        /// Gets or sets the meta description
        /// </summary>
        [DataMember]
        public string MetaDescription { get; set; }

        /// <summary>
        /// Gets or sets the meta title
        /// </summary>
        [DataMember]
        public string MetaTitle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        [DataMember]
        public bool LimitedToStores { get; set; }

        /// <summary>
        /// Gets or sets a language identifier for which the blog post should be displayed.
        /// </summary>
        [DataMember]
        [Index]
        public int? LanguageId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        [DataMember]
        public virtual DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        [DataMember]
        public virtual Language Language { get; set; }

        /// <summary>
        /// Gets or sets the blog comments
        /// </summary>
        [DataMember]
        public virtual ICollection<BlogComment> BlogComments
        {
            get => _blogComments ?? (_blogComments = new HashSet<BlogComment>());
            protected set => _blogComments = value;
        }
    }

    public enum PreviewDisplayType
    {
        /// <summary>
        /// No picture will be displayed
        /// </summary>
        Bare = 0,

        /// <summary>
        /// The detail picture will be displayed
        /// </summary>
        Default = 10,

        /// <summary>
        /// The preview picture will be displayed
        /// </summary>
        Preview = 20,

        /// <summary>
        /// The detail picture will be displayed under the section background
        /// </summary>
        DefaultSectionBg = 30,

        /// <summary>
        /// The preview picture will be displayed under the section background
        /// </summary>
        PreviewSectionBg = 40,

        /// <summary>
        /// The section background will be displayed
        /// </summary>
        SectionBg = 50
    }
}