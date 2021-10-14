namespace QTRADO.WMAddOn.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Grossists",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GrossistGuid = c.Guid(nullable: false),
                        GrossoNr = c.String(),
                        Name = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedOnUtc = c.DateTime(),
                        UpdatedOnUtc = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.GrossoNr, unique: true, name: "IX_Grossist_GrossoNr");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Grossists", "IX_Grossist_GrossoNr");
            DropTable("dbo.Grossists");
        }
    }
}

SmartStore.Admin.Controllers.PluginController: ERROR - Der Wert darf nicht NULL sein.
Parametername: key
System.ArgumentNullException: Der Wert darf nicht NULL sein.
Parametername: key
   bei System.Collections.Concurrent.ConcurrentDictionary`2.TryGetValue(TKey key, TValue& value)
   bei SmartStore.Core.Infrastructure.DependencyManagement.ContainerManager.ResolveUnregistered(Type type, ILifetimeScope scope) in C: \Users\thyssen.FJK\Source\repos\SmartStoreNET\src\Libraries\SmartStore.Core\Infrastructure\DependencyManagement\ContainerManager.cs:Zeile 80.
   bei SmartStore.Core.Plugins.PluginDescriptor.Instance[T] () in C:\Users\thyssen.FJK\Source\repos\SmartStoreNET\src\Libraries\SmartStore.Core\Plugins\PluginDescriptor.cs:Zeile 252.
   bei SmartStore.Core.Plugins.PluginDescriptor.Instance() in C:\Users\thyssen.FJK\Source\repos\SmartStoreNET\src\Libraries\SmartStore.Core\Plugins\PluginDescriptor.cs:Zeile 264.
   bei SmartStore.Admin.Controllers.PluginController.ExecuteTasks(IEnumerable`1 pluginsToInstall, IEnumerable`1 pluginsToUninstall) in C: \Users\thyssen.FJK\Source\repos\SmartStoreNET\src\Presentation\SmartStore.Web\Administration\Controllers\PluginController.cs:Zeile 234.