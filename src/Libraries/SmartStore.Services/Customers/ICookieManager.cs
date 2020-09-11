using SmartStore.Core.Plugins;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.Services.Customers
{
    public interface ICookieManager
    {
        /// <summary>
        /// Gets CookieInfos of all plugins which are publishing cookies to be display in CookieManager dialog.
        /// </summary>
        /// <returns>List of CookieInfos.</returns>
        List<CookieInfo> GetAllCookieInfos(bool addSettingCookies = false);

        /// <summary>
        /// Gets a value which specifies whether it is allowed to set a cookie of a certain type.
        /// </summary>
        /// <param name="cookieType">Type of the cookie.</param>
        /// <returns>Value which indicates whether cookies of a certain type are allowed to be set.</returns>
        bool IsCookieAllowed(ControllerContext context, CookieType cookieType);

        /// <summary>
        /// Gets the data of the cookie.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Cookie which contains data of user selection.</returns>
        ConsentCookie GetCookieData(ControllerContext context);

        /// <summary>
        /// Sets the cookie.
        /// </summary>
        /// <param name="response">Cookie will be set to this response.</param>
        /// <param name="allowAnalytics">Defines whether analytical cookies are allowed to be set.</param>
        /// <param name="allowThirdParty">Defines whether third party cookies are allowed to be set.</param>
        void SetConsentCookie(HttpResponseBase response, bool allowAnalytics = false, bool allowThirdParty = false);
    }
}
