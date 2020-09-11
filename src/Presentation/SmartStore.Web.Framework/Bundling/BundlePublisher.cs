using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Optimization;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.Bundling
{
    public class BundlePublisher : IBundlePublisher
    {
        private readonly ITypeFinder _typeFinder;

        public BundlePublisher(ITypeFinder typeFinder)
        {
            this._typeFinder = typeFinder;
        }

        public void RegisterBundles(BundleCollection bundles)
        {
            var bundleProviderTypes = _typeFinder.FindClassesOfType<IBundleProvider>(ignoreInactivePlugins: true);
            var bundleProviders = new List<IBundleProvider>();
            foreach (var providerType in bundleProviderTypes)
            {
                var provider = Activator.CreateInstance(providerType) as IBundleProvider;
                bundleProviders.Add(provider);
            }

            bundleProviders = bundleProviders.OrderByDescending(bp => bp.Priority).ToList();
            bundleProviders.Each(bp => bp.RegisterBundles(bundles));
        }
    }
}
