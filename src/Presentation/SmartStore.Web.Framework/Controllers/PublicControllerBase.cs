using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Seo;

namespace SmartStore.Web.Framework.Controllers
{
	[CanonicalHostName(Order = 100)]
	[RequireHttpsByConfigAttribute(SslRequirement.Retain, Order = 110)]
	[StoreClosed(Order = -1)]
	[PublicStoreAllowNavigation(Order = -1)]
	[LanguageSeoCode(Order = -1)]
	[CustomerLastActivity(Order = 100)]
	[StoreIpAddress(Order = 100)]
	[StoreLastVisitedPage(Order = 100)]
	[CheckAffiliate(Order = 100)]
    public abstract partial class PublicControllerBase : SmartController
    {
    }
}
