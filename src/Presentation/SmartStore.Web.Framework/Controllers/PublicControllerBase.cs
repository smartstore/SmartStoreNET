using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Seo;

namespace SmartStore.Web.Framework.Controllers
{
    [StoreClosed]
    [PublicStoreAllowNavigation]
    [RewriteUrl(SslRequirement.Retain, Order = 0)]
    [LanguageSeoCode(Order = 1)]
    [CustomerLastActivity(Order = 1000)]
	[StoreIpAddress(Order = 1000)]
	[StoreLastVisitedPage(Order = 1000)]
	[CheckAffiliate(Order = 1000)]
	public abstract partial class PublicControllerBase : SmartController
    {
	}
}
