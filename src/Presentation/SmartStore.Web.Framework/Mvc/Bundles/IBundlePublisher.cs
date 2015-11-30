using System.Web.Optimization;

namespace SmartStore.Web.Framework.Mvc.Bundles
{
    public interface IBundlePublisher
    {
        void RegisterBundles(BundleCollection bundles);
    }
}
