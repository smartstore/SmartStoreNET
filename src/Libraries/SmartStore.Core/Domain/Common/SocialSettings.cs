using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Common
{
    public class SocialSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the bank name that will be used
        /// </summary>
        public bool ShowSocialLinksInFooter { get; set; }

        /// <summary>
        /// Gets or sets the bank code that will be used
        /// </summary>
        public string FacebookLink { get; set; }

        /// <summary>
        /// Gets or sets the account number that will be used
        /// </summary>
        public string GooglePlusLink { get; set; }

        /// <summary>
        /// Gets or sets the account holder that will be used
        /// </summary>
        public string TwitterLink { get; set; }

        /// <summary>
        /// Gets or sets the iban that will be used
        /// </summary>
        public string PinterestLink { get; set; }
    }
}