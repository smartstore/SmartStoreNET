using System.Collections.Generic;
using SmartStore.Core.Plugins;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Common
{
    public partial class CookieManagerModel : ModelBase
    {
        public CookieManagerModel()
        {
            CookiesInfos = new List<CookieInfo>();
            RequiredConsent = true;
        }

        public List<CookieInfo> CookiesInfos { get; set; }
        public bool ModalCookieConsent { get; set; }

        public bool AcceptAll { get; set; }

        public bool RequiredConsent { get; set; }

        public bool AnalyticsConsent { get; set; }

        public bool ThirdPartyConsent { get; set; }
    }
}

