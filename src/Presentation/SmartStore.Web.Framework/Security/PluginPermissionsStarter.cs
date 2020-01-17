using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Core.Security;
using SmartStore.Data;
using SmartStore.Services.Security;

namespace SmartStore.Web.Framework.Security
{
    /// <summary>
    /// Checks for missing plugin permissions and adds them to the database.
    /// </summary>
    public class PluginPermissionsStarter : IStartupTask
    {
        public int Order => 0;

        public void Execute()
        {
            // TODO: work in progress.
            return;
            var dbContext = (SmartObjectContext)EngineContext.Current.Resolve<IDbContext>();
            var missingPermissions = GetMissingPermissions(dbContext);

            if (!missingPermissions.Any())
            {
                return;
            }

            using (var scope = new DbContextScope(ctx: dbContext, validateOnSave: false, hooksEnabled: false, autoCommit: false))
            {
                SavePermissions(scope, missingPermissions);
                SaveDisplayNames(scope, missingPermissions);
            }
        }

        private ILogger Logger
        {
            get
            {
                return EngineContext.Current.Resolve<ILoggerFactory>().GetLogger(this.GetType());
            }
        }

        protected virtual List<PluginPermissionInfo> GetMissingPermissions(IDbContext dbContext)
        {
            var result = new List<PluginPermissionInfo>();
            var permissionSet = dbContext.Set<PermissionRecord>();
            var existingPermissions = permissionSet.Select(x => x.SystemName).ToList();

            var pluginDescriptors = PluginFinder.Current.GetPluginDescriptors();

            foreach (var descriptor in pluginDescriptors)
            {
                var exportedTypes = descriptor.Assembly.Assembly.GetExportedTypes();

                foreach (var t in exportedTypes.Where(t => typeof(IPermissionProvider).IsAssignableFrom(t) && !t.IsInterface && t.IsClass && !t.IsAbstract))
                {
                    try
                    {
                        var permissionProvider = Activator.CreateInstance(t) as IPermissionProvider;
                        var permissions = permissionProvider.GetPermissions();

                        var missingPermissionNames = permissions
                            .Where(x => !existingPermissions.Contains(x.SystemName))
                            .Select(x => x.SystemName);

                        if (missingPermissionNames.Any())
                        {
                            var defaultPermissions = permissionProvider.GetDefaultPermissions();

                            foreach (var systemName in missingPermissionNames)
                            {
                                var roleNames = defaultPermissions
                                    .Where(x => x.PermissionRecords.Any(y => y.SystemName == systemName))
                                    .Select(x => x.CustomerRoleSystemName);

                                result.Add(new PluginPermissionInfo
                                {
                                    Descriptor = descriptor,
                                    PermissionName = systemName,
                                    RoleNames = roleNames.ToList()
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
            }

            return result;
        }

        protected virtual int SavePermissions(DbContextScope scope, List<PluginPermissionInfo> permissions)
        {
            var num = 0;

            try
            {
                var permissionSet = scope.DbContext.Set<PermissionRecord>();

                var allRoles = scope.DbContext.Set<CustomerRole>()
                    .Where(x => !string.IsNullOrEmpty(x.SystemName))
                    .ToList()
                    .ToDictionarySafe(x => x.SystemName, x => x);

                foreach (var item in permissions)
                {
                    var permission = new PermissionRecord { SystemName = item.PermissionName };

                    foreach (var roleName in item.RoleNames)
                    {
                        if (allRoles.TryGetValue(roleName, out var role))
                        {
                            permission.PermissionRoleMappings.Add(new PermissionRoleMapping
                            {
                                Allow = true,
                                PermissionRecord = permission,
                                CustomerRoleId = role.Id
                            });
                        }
                    }

                    permissionSet.Add(permission);
                }

                num = scope.Commit();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return num;
        }

        protected virtual int SaveDisplayNames(DbContextScope scope, List<PluginPermissionInfo> permissions)
        {
            var num = 0;

            try
            {
                var resourceSet = scope.DbContext.Set<LocaleStringResource>();
                var languageSet = scope.DbContext.Set<Language>();
                var languages = languageSet.Where(x => x.Published).ToList();
                var languageIds = languages.Select(x => x.Id).ToList();

                foreach (var item in permissions)
                {
                    //var tokens = item.PermissionName.EmptyNull().ToLower().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    //var token = tokens.LastOrDefault();

                    //if (token.HasValue())
                    //{
                    //    if (!PermissionService.DisplayNameResourceKeys.TryGetValue(token, out var key))
                    //    {
                    //        var resourceName = "Plugins.Permissions.DisplayName." + token.Replace("-", "");

                    //        var existingResources = resourceSet
                    //            .Where(x => x.ResourceName == resourceName && languageIds.Contains(x.LanguageId))
                    //            .ToList()
                    //            .ToDictionarySafe(x => $"{x.LanguageId}|{x.ResourceName}", x => x);

                    //        foreach (var language in languages)
                    //        {
                    //            if (!existingResources.ContainsKey($"{language.Id}|{resourceName}"))
                    //            {
                    //                // Resource is missing. Get value.
                    //            }
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return num;
        }

        public class PluginPermissionInfo
        {
            private DirectoryInfo _directoryInfo;

            public PluginDescriptor Descriptor { get; set; }
            public string PermissionName { get; set; }
            public List<string> RoleNames { get; set; }

            public DirectoryInfo Directory
            {
                get
                {
                    return _directoryInfo ?? (_directoryInfo = new DirectoryInfo(Path.Combine(Descriptor.Assembly.OriginalFile.Directory.FullName, "Localization")));
                }
            }
        }
    }
}
