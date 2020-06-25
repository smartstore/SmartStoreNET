using SmartStore.Core.Plugins;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.Modelling;
using System.Collections.Generic;

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

        public bool AcceptAll { get; set; }

        public bool RequiredConsent { get; set; }

        public bool AnalyticsConsent { get; set; }

        public bool ThirdPartyConsent { get; set; }
	}
}

