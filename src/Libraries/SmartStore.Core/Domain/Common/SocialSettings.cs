using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Common
{
    public class SocialSettings : ISettings
    {
        /// <summary>
        /// Gets or sets facebook app id
        /// </summary>
        public string FacebookAppId { get; set; } = null;

        /// <summary>
        /// Gets or sets twitter account site name
        /// </summary>
        public string TwitterSite { get; set; } = null;

        /// <summary>
        /// Gets or sets the value that determines whether social links should be show in the footer
        /// </summary>
        public bool ShowSocialLinksInFooter { get; set; } = true;

        /// <summary>
        /// Gets or sets the facebook link
        /// </summary>
        public string FacebookLink { get; set; } = "#";

        /// <summary>
        /// Gets or sets the twitter link
        /// </summary>
        public string TwitterLink { get; set; } = "#";

        /// <summary>
        /// Gets or sets the pinterest link
        /// </summary>
        public string PinterestLink { get; set; } = "#";

        /// <summary>
        /// Gets or sets the youtube link
        /// </summary>
        public string YoutubeLink { get; set; } = "#";

        /// <summary>
        /// Gets or sets the instagram link
        /// </summary>
        public string InstagramLink { get; set; } = "#";
    }
}