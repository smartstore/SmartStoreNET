namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Localization;
    using SmartStore.Core.Domain.Tasks;
    using SmartStore.Data.Setup;

    public partial class CustomerRoleMappings : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            DropForeignKey("dbo.Download", "MediaStorageId", "dbo.MediaStorage");
            DropForeignKey("dbo.QueuedEmailAttachment", "FileId", "dbo.Download");
            DropForeignKey("dbo.QueuedEmailAttachment", "MediaStorageId", "dbo.MediaStorage");
            DropIndex("dbo.MediaFile", new[] { "IsNew" });
            DropIndex("dbo.Download", new[] { "MediaStorageId" });
            DropIndex("dbo.QueuedEmailAttachment", new[] { "FileId" });
            CreateTable(
                "dbo.CustomerRoleMapping",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        CustomerRoleId = c.Int(nullable: false),
                        IsSystemMapping = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customer", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.CustomerRole", t => t.CustomerRoleId, cascadeDelete: true)
                .Index(t => t.CustomerId)
                .Index(t => t.CustomerRoleId)
                .Index(t => t.IsSystemMapping);
            
            CreateTable(
                "dbo.RuleSet_CustomerRole_Mapping",
                c => new
                    {
                        CustomerRole_Id = c.Int(nullable: false),
                        RuleSetEntity_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CustomerRole_Id, t.RuleSetEntity_Id })
                .ForeignKey("dbo.CustomerRole", t => t.CustomerRole_Id, cascadeDelete: true)
                .ForeignKey("dbo.RuleSet", t => t.RuleSetEntity_Id, cascadeDelete: true)
                .Index(t => t.CustomerRole_Id)
                .Index(t => t.RuleSetEntity_Id);
            
            AddColumn("dbo.QueuedEmailAttachment", "MediaFileId", c => c.Int());
            CreateIndex("dbo.QueuedEmailAttachment", "MediaFileId");
            AddForeignKey("dbo.QueuedEmailAttachment", "MediaFileId", "dbo.MediaFile", "Id");
            AddForeignKey("dbo.QueuedEmailAttachment", "MediaStorageId", "dbo.MediaStorage", "Id", cascadeDelete: true);
            DropColumn("dbo.MediaFile", "IsNew");
            DropColumn("dbo.Download", "DownloadBinary");
            DropColumn("dbo.Download", "ContentType");
            DropColumn("dbo.Download", "Filename");
            DropColumn("dbo.Download", "Extension");
            DropColumn("dbo.Download", "IsNew");
            DropColumn("dbo.Download", "MediaStorageId");
            DropColumn("dbo.QueuedEmailAttachment", "FileId");
            DropColumn("dbo.QueuedEmailAttachment", "Data");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QueuedEmailAttachment", "Data", c => c.Binary());
            AddColumn("dbo.QueuedEmailAttachment", "FileId", c => c.Int());
            AddColumn("dbo.Download", "MediaStorageId", c => c.Int());
            AddColumn("dbo.Download", "IsNew", c => c.Boolean(nullable: false));
            AddColumn("dbo.Download", "Extension", c => c.String());
            AddColumn("dbo.Download", "Filename", c => c.String());
            AddColumn("dbo.Download", "ContentType", c => c.String());
            AddColumn("dbo.Download", "DownloadBinary", c => c.Binary());
            AddColumn("dbo.MediaFile", "IsNew", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.QueuedEmailAttachment", "MediaStorageId", "dbo.MediaStorage");
            DropForeignKey("dbo.QueuedEmailAttachment", "MediaFileId", "dbo.MediaFile");
            DropForeignKey("dbo.RuleSet_CustomerRole_Mapping", "RuleSetEntity_Id", "dbo.RuleSet");
            DropForeignKey("dbo.RuleSet_CustomerRole_Mapping", "CustomerRole_Id", "dbo.CustomerRole");
            DropForeignKey("dbo.CustomerRoleMapping", "CustomerRoleId", "dbo.CustomerRole");
            DropForeignKey("dbo.CustomerRoleMapping", "CustomerId", "dbo.Customer");
            DropIndex("dbo.RuleSet_CustomerRole_Mapping", new[] { "RuleSetEntity_Id" });
            DropIndex("dbo.RuleSet_CustomerRole_Mapping", new[] { "CustomerRole_Id" });
            DropIndex("dbo.QueuedEmailAttachment", new[] { "MediaFileId" });
            DropIndex("dbo.CustomerRoleMapping", new[] { "IsSystemMapping" });
            DropIndex("dbo.CustomerRoleMapping", new[] { "CustomerRoleId" });
            DropIndex("dbo.CustomerRoleMapping", new[] { "CustomerId" });
            DropColumn("dbo.QueuedEmailAttachment", "MediaFileId");
            DropTable("dbo.RuleSet_CustomerRole_Mapping");
            DropTable("dbo.CustomerRoleMapping");
            CreateIndex("dbo.QueuedEmailAttachment", "FileId");
            CreateIndex("dbo.Download", "MediaStorageId");
            CreateIndex("dbo.MediaFile", "IsNew");
            AddForeignKey("dbo.QueuedEmailAttachment", "MediaStorageId", "dbo.MediaStorage", "Id");
            AddForeignKey("dbo.QueuedEmailAttachment", "FileId", "dbo.Download", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Download", "MediaStorageId", "dbo.MediaStorage", "Id");
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            if (!DataSettings.DatabaseIsInstalled())
                return;

            try
            {
                // Copy role mappings.
                context.ExecuteSqlCommand("Insert Into [dbo].[CustomerRoleMapping] (CustomerId, CustomerRoleId, IsSystemMapping) Select Customer_Id As Customer_Id, CustomerRole_Id As CustomerRole_Id, 0 As IsSystemMapping From [dbo].[Customer_CustomerRole_Mapping]");
            }
            catch { }

            var defaultLang = context.Set<Language>().AsNoTracking().OrderBy(x => x.DisplayOrder).First();
            var isGerman = defaultLang.UniqueSeoCode.IsCaseInsensitiveEqual("de");

            context.Set<ScheduleTask>().AddOrUpdate(x => x.Type,
                new ScheduleTask
                {
                    Name = isGerman ? "Zuordnungen von Kunden zu Kundengruppen aktualisieren" : "Update assignments of customers to customer roles",
                    CronExpression = "15 2 * * *", // At 02:15
                    Type = "SmartStore.Services.Customers.TargetGroupEvaluatorTask, SmartStore.Services",
                    Enabled = true,
                    StopOnError = false
                }
            );
            context.SaveChanges();
        }
    }
}
