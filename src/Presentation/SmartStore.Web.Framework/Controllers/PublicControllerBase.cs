using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Seo;

namespace SmartStore.Web.Framework.Controllers
{
	[CustomerLastActivity]
	[StoreIpAddress]
	[StoreLastVisitedPage]
	[CheckAffiliate]
	[StoreClosed]
	[PublicStoreAllowNavigation]
	[LanguageSeoCode]
    [RequireHttpsByConfigAttribute(SslRequirement.Retain)]
	[CanonicalHostName]
    public abstract partial class PublicControllerBase : SmartController
    {
    }
}
