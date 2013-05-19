using System.Web.Optimization;

namespace SmartStore.Web.Framework.Mvc.Bundles
{
    /// <summary>
    /// <remarks>codehint: sm-add</remarks>
    /// </summary>
    public interface IBundleProvider
    {
        void RegisterBundles(BundleCollection bundles);

        int Priority { get; }
    }
}
