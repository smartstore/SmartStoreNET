using System.Runtime.Serialization;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Core.Domain.Stores
{
    /// <summary>
    /// Represents a store
    /// </summary>
    [DataContract]
    public partial class Store : BaseEntity
    {
        /// <summary>
        /// Gets or sets the store name
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the store URL
        /// </summary>
        [DataMember]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SSL is enabled
        /// </summary>
        [DataMember]
        public bool SslEnabled { get; set; }

        /// <summary>
        /// Gets or sets the store secure URL (HTTPS)
        /// </summary>
        [DataMember]
        public string SecureUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all pages will be forced to use SSL (no matter of a specified [RequireHttpsByConfigAttribute] attribute)
        /// </summary>
        [DataMember]
        public bool ForceSslForAllPages { get; set; }

        /// <summary>
        /// Gets or sets the comma separated list of possible HTTP_HOST values
        /// </summary>
        [DataMember]
        public string Hosts { get; set; }

        /// <summary>
        /// Gets or sets the logo media file id
        /// </summary>
        [DataMember]
        public int LogoMediaFileId { get; set; }

        /// <summary>
        /// Gets or sets the png icon media file id 
        /// </summary>
        [DataMember]
        public int? FavIconMediaFileId { get; set; }

        /// <summary>
        /// Gets or sets the png icon media file id 
        /// </summary>
        [DataMember]
        public int? PngIconMediaFileId { get; set; }

        /// <summary>
        /// Gets or sets the apple touch icon media file id
        /// </summary>
        [DataMember]
        public int? AppleTouchIconMediaFileId { get; set; }

        /// <summary>
        /// Gets or sets the ms tile image media file id
        /// </summary>
        [DataMember]
        public int? MsTileImageMediaFileId { get; set; }

        /// <summary>
        /// Gets or sets the ms tile color
        /// </summary>
        [DataMember]
        public string MsTileColor { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        [DataMember]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets a store specific id for the HTML body
        /// </summary>
        [DataMember]
        public string HtmlBodyId { get; set; }

        /// <summary>
        /// Gets or sets the CDN host name, if static media content should be served through a CDN.
        /// </summary>
        [DataMember]
        public string ContentDeliveryNetwork { get; set; }

        /// <summary>
        /// Gets or sets the primary store currency identifier
        /// </summary>
        [DataMember]
        public int PrimaryStoreCurrencyId { get; set; }

        /// <summary>
        /// Gets or sets the primary exchange rate currency identifier
        /// </summary>
        [DataMember]
        public int PrimaryExchangeRateCurrencyId { get; set; }

        /// <summary>
        /// Gets or sets the primary store currency
        /// </summary>
        [DataMember]
        public virtual Currency PrimaryStoreCurrency { get; set; }

        /// <summary>
        /// Gets or sets the primary exchange rate currency
        /// </summary>
        [DataMember]
        public virtual Currency PrimaryExchangeRateCurrency { get; set; }


        /// <summary>
        /// Gets the security mode for the store
        /// </summary>
        public virtual HttpSecurityMode GetSecurityMode(bool? useSsl = null)
        {
            if (useSsl ?? SslEnabled)
            {
                if (SecureUrl.HasValue() && Url.HasValue() && !Url.StartsWith("https"))
                {
                    return HttpSecurityMode.SharedSsl;
                }
                else
                {
                    return HttpSecurityMode.Ssl;
                }
            }

            return HttpSecurityMode.Unsecured;
        }

        private string _secureHost;
        private string _unsecureHost;

        /// <summary>
        /// Gets the store host name
        /// </summary>
        /// <param name="secure">
        /// If <c>false</c>, returns the default unsecured url.
        /// If <c>true</c>, returns the secure url, but only if SSL is enabled for the store.
        /// </param>
        /// <returns>The host name</returns>
        public string GetHost(bool secure)
        {
            return secure
                ? _secureHost ?? (_secureHost = GetHostInternal(true))
                : _unsecureHost ?? (_unsecureHost = GetHostInternal(false));
        }

        private string GetHostInternal(bool secure)
        {
            var host = string.Empty;

            if (secure && SslEnabled)
            {
                if (SecureUrl.HasValue())
                {
                    host = SecureUrl;
                }
                else
                {
                    host = Url.Replace("http:/", "https:/");
                }
            }
            else
            {
                host = Url;
            }

            return host.EnsureEndsWith("/");
        }
    }
}
