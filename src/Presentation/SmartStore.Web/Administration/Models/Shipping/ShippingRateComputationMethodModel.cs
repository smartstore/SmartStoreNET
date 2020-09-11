using SmartStore.Core;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.Admin.Models.Shipping
{
    public class ShippingRateComputationMethodModel : ProviderModel, IActivatable
    {
        [SmartResourceDisplayName("Common.IsActive")]
        public bool IsActive { get; set; }
    }
}