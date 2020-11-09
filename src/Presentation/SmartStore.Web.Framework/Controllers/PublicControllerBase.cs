using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Seo;

namespace SmartStore.Web.Framework.Controllers
{
    [StoreClosed]
    [PublicStoreAllowNavigation]
    [RewriteUrl(SslRequirement.Retain, Order = 0)]
    [LanguageSeoCode(Order = 1)]
    [CustomerLastActivity]
    [StoreIpAddress]
    [StoreLastVisitedPage]
    [CheckAffiliate]
    public abstract partial class PublicControllerBase : SmartController
    {
    }
}
