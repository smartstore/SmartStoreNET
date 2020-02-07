using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Security;
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
        private readonly IRepository<PermissionRecord> _permissionRepository;
        private readonly IPermissionService _permissionService;
        private readonly ITypeFinder _typeFinder;

        public InstallPermissionsStarter(
            IRepository<PermissionRecord> permissionRepository,
            IPermissionService permissionService,
            ITypeFinder typeFinder)
        {
            _permissionRepository = permissionRepository;
            _permissionService = permissionService;
            _typeFinder = typeFinder;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public int Order => 0;
        public bool ThrowOnError => true;
        public int MaxAttempts => 1;

        public void Start(HttpContextBase httpContext)
        {
            var removeUnusedPermissions = true;
            var providers = new List<IPermissionProvider>();

            if (PluginManager.PluginChangeDetected || !_permissionRepository.TableUntracked.Any())
            {
                // Standard permission provider and all plugin providers.
                var types = _typeFinder.FindClassesOfType<IPermissionProvider>(ignoreInactivePlugins: true).ToList();
                foreach (var type in types)
                {
                    var provider = Activator.CreateInstance(type) as IPermissionProvider;
                    if (provider != null)
                    {
                        providers.Add(provider);
                    }
                    else
                    {
                        removeUnusedPermissions = false;
                        Logger.Warn($"Cannot create instance of IPermissionProvider {type.Name.NaIfEmpty()}.");
                    }
                }
            }
            else
            {
                // Always check standard permission provider.
                providers.Add(new StandardPermissionProvider());

                // Keep unused permissions in database (has no negative effects) as long as at least one plugin changed.
                removeUnusedPermissions = false;
            }

            _permissionService.InstallPermissions(providers.ToArray(), removeUnusedPermissions);
        }

        public void OnFail(Exception exception, bool willRetry)
        {
        }
    }
}
