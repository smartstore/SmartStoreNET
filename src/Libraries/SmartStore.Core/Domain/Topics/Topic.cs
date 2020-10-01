using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Plugins;

namespace SmartStore.Core.Domain.Topics
{
    /// <summary>
    /// Represents a topic
    /// </summary>
    [DataContract]
    public partial class Topic : BaseEntity, ILocalizedEntity, ISlugSupported, IStoreMappingSupported, IAclSupported
    {
        public Topic()
        {
            IsPublished = true;
        }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [DataMember]
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether this topic is deleteable by a user
        /// </summary>
        [DataMember]
        public bool IsSystemTopic { get; set; }

        /// <summary>
        /// Gets or sets the html id
        /// </summary>
        [DataMember]
        public string HtmlId { get; set; }

        /// <summary>
        /// Gets or sets the body css class
        /// </summary>
        [DataMember]
        public string BodyCssClass { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether this topic should be included in sitemap
        /// </summary>
        [DataMember]
        public bool IncludeInSitemap { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether this topic is password protected
        /// </summary>
        [DataMember]
        public bool IsPasswordProtected { get; set; }

        /// <summary>
        /// Gets or sets the password
        /// </summary>
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the title
        /// </summary>
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the short title (for links)
        /// </summary>
        [DataMember]
        public string ShortTitle { get; set; }

        /// <summary>
        /// Gets or sets the intro
        /// </summary>
        [DataMember]
        public string Intro { get; set; }

        /// <summary>
        /// Gets or sets the body
        /// </summary>
        [DataMember]
        public string Body { get; set; }

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
        /// Gets or sets a value indicating whether the topic should also be rendered as a generic html widget
        /// </summary>
        [DataMember]
        public bool RenderAsWidget { get; set; }

        /// <summary>
        /// Gets or sets the widget zone name
        /// </summary>
        [DataMember]
        public string WidgetZone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the content should be surrounded by a topic block wrapper
        /// </summary>
        [DataMember]
        public bool? WidgetWrapContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the title should be displayed in the widget block
        /// </summary>
        [DataMember]
        public bool WidgetShowTitle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the widget block should have borders
        /// </summary>
        [DataMember]
        public bool WidgetBordered { get; set; }

        /// <summary>
        /// Gets or sets the sort order (relevant for widgets)
        /// </summary>
        [DataMember]
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the title tag
        /// </summary>
        [DataMember]
        public string TitleTag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is subject to ACL
        /// </summary>
        [DataMember]
        public bool SubjectToAcl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the topic page is published
        /// </summary>
        [DataMember]
        public bool IsPublished { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the topic set a cookie and the cookie type
        /// </summary>
        public CookieType? CookieType { get; set; }

        /// <summary>
        /// Helper function which gets the comma-separated <c>WidgetZone</c> property as list of strings
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetWidgetZones()
        {
            if (this.WidgetZone.IsEmpty())
            {
                return Enumerable.Empty<string>();
            }

            return this.WidgetZone.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
        }
    }
}
