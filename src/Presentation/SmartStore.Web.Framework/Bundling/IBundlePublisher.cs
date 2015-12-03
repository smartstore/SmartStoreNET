using System.Web.Optimization;

namespace SmartStore.Web.Framework.Bundling
{
    public interface IBundlePublisher
    {
        void RegisterBundles(BundleCollection bundles);
    }
}
