using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Framework.Plugins
{
    /// <summary>
    /// Checks whether any plugin has changed and refreshes all plugin locale resources.
    /// </summary>
    public sealed class PluginStarter : IPostApplicationStart
    {
        private readonly IPluginFinder _pluginFinder;
        private readonly IPluginLocalizationDiffer _differ;
        private readonly ILocalizationService _locService;

        public PluginStarter(IPluginFinder pluginFinder, IPluginLocalizationDiffer differ, ILocalizationService locService)
        {
            _pluginFinder = pluginFinder;
            _differ = differ;
            _locService = locService;
        }

        public int Order => 100;
        public bool ThrowOnError => false;
        public int MaxAttempts => 1;

        public void Start(HttpContextBase httpContext)
        {
            //if (!PluginManager.PluginChangeDetected)
            //    return;

            var descriptors = _pluginFinder.GetPluginDescriptors(true);
            foreach (var d in descriptors)
            {
                var status = _differ.GetStatus(d);
                if (status != null && status.Differs)
                {
                    _locService.ImportPluginResourcesFromXml(status.Descriptor, null, false);
                }
            }
        }

        public void OnFail(Exception exception, bool willRetry)
        {
        }
    }
}
