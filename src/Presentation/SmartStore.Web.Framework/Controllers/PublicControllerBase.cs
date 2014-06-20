using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Web.Framework.Controllers
{

    [CustomerLastActivity]
    [StoreIpAddress]
    [StoreLastVisitedPage]
    [CheckAffiliate]
    [StoreClosedAttribute]
    [PublicStoreAllowNavigation]
    [LanguageSeoCodeAttribute]
    [RequireHttpsByConfigAttribute(SslRequirement.Retain)]
    public abstract partial class PublicControllerBase : SmartController
    {
    }
}
