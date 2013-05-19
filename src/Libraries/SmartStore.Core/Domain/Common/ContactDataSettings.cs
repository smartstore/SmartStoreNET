
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Common
{
    public class ContactDataSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the company telephone number that will be used
        /// </summary>
        public string CompanyTelephoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the hotline telephone number that will be used
        /// </summary>
        public string HotlineTelephoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the mobile telephone number that will be used
        /// </summary>
        public string MobileTelephoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the company fax number that will be used
        /// </summary>
        public string CompanyFaxNumber { get; set; }

        /// <summary>
        /// Gets or sets the company email address that will be used
        /// </summary>
        public string CompanyEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the webmaster email address that will be used
        /// </summary>
        public string WebmasterEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the webmaster email address that will be used
        /// </summary>
        public string SupportEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the contact email address that will be used
        /// </summary>
        public string ContactEmailAddress { get; set; }

    }
}