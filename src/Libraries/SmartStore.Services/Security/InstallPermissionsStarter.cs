using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Core.Security;

namespace SmartStore.Services.Security
{
    /// <summary>
    /// Checks for new permissions and installs them.
    /// </summary>
    public sealed class InstallPermissionsStarter : IPostApplicationStart
    {
        private readonly IPermissionService _permissionService;

        public InstallPermissionsStarter(IPermissionService permissionService)
        {
            _permissionService = permissionService;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public int Order => 0;
        public bool ThrowOnError => false;
        public int MaxAttempts => 3;

        public void Start(HttpContextBase httpContext)
        {
            var removeUnusedPermissions = true;
            var providers = new List<IPermissionProvider>
            {
                new StandardPermissionProvider()
            };

            // Plugin permissions.
            if (PluginManager.PluginChangeDetected)
            {
                var pluginDescriptors = PluginFinder.Current.GetPluginDescriptors();
                foreach (var descriptor in pluginDescriptors)
                {
                    var exportedTypes = descriptor.Assembly.Assembly.GetExportedTypes();
                    foreach (var t in exportedTypes.Where(t => typeof(IPermissionProvider).IsAssignableFrom(t) && !t.IsInterface && t.IsClass && !t.IsAbstract))
                    {
                        var provider = Activator.CreateInstance(t) as IPermissionProvider;
                        if (provider != null)
                        {
                            providers.Add(provider);
                        }
                        else
                        {
                            removeUnusedPermissions = false;
                            Logger.Warn($"Cannot create instance of IPermissionProvider {t.Name.NaIfEmpty()}.");
                        }
                    }
                }
            }
            else
            {
                // Keep unused permissions in database (has no negative effects) as long as at least one plugin changed.
                removeUnusedPermissions = false;
            }

            _permissionService.InstallPermissions(providers.ToArray(), removeUnusedPermissions);
        }

        public void OnFail(Exception exception, bool willRetry)
        {
            if (willRetry)
            {
                Logger.Error(exception, "Error while installing new permissions.");
            }
            else
            {
                Logger.Warn($"Stopped trying to install new permissions: too many failed attempts in succession ({MaxAttempts}+).");
            }
        }
    }
}
