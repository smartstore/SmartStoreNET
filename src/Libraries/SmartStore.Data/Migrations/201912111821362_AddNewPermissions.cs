namespace SmartStore.Data.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SmartStore.Core.Domain.Customers;
    using SmartStore.Core.Plugins;
    using SmartStore.Data.Setup;

    public partial class AddNewPermissions : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
        }
        
        public override void Down()
        {
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            var adminRole = context.Set<CustomerRole>().FirstOrDefault(x => x.SystemName == "Administrators");
            var migrator = new PermissionMigrator(context);

            var pluginDescriptors = PluginFinder.Current.GetPluginDescriptors();
            var installedPlugins = pluginDescriptors.Select(x => x.SystemName).ToList();

            // TODO: more plugins to come...
            var pluginTypeNames = new Dictionary<string, string>
            {
                { "SmartStore.DevTools", "SmartStore.DevTools.Security.DevToolsPermissions, SmartStore.DevTools" },
                { "SmartStore.OutputCache", "SmartStore.OutputCache.Security.OutputCachePermissions, SmartStore.OutputCache" },
            };

            foreach (var pluginTypeName in pluginTypeNames)
            {
                // Add permissions only for installed plugins.
                if (installedPlugins.Contains(pluginTypeName.Key, StringComparer.OrdinalIgnoreCase))
                {
                    var newPermissions = migrator.AddPluginPermissions(pluginTypeName.Value);
                    if (newPermissions.Any())
                    {
                        // Convention to identify the root permission (typically "Self"): doesn't contain any dot.
                        var rootPermission = newPermissions.FirstOrDefault(x => !x.SystemName.Contains('.'));
                        if (rootPermission != null)
                        {
                            // Allow root permission for admin.
                            migrator.Allow(rootPermission, true, adminRole);
                        }
                    }
                }
            }
        }
    }
}
