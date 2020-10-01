using System.Collections.Generic;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Plugins
{
    /// <summary>
    /// Marks a module as a cookie publisher. 
    /// </summary>
    public interface ICookiePublisher
    {
        /// <summary>
        /// Gets the cookie info of the cookie publisher (e.g. plugin or other module).
        /// </summary>
        IEnumerable<CookieInfo> GetCookieInfo();
    }

    /// <summary>
    /// Plugin cookie infos.
    /// </summary>
    public class CookieInfo : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Name of the plugin.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Description of the cookie (e.g. purpose of using the cookie).
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// SelectedStoreIds
        /// </summary>
        [DataMember]
        public int[] SelectedStoreIds { get; set; }

        /// <summary>
        /// Type of the cookie.
        /// </summary>
        [DataMember]
        public CookieType CookieType { get; set; }
    }

    /// <summary>
    /// Type of the cookie.
    /// </summary>
    public enum CookieType
    {
        Required,
        Analytics,
        ThirdParty
    }
}
