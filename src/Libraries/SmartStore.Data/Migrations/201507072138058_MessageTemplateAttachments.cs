namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class MessageTemplateAttachments : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.MessageTemplate", "Attachment1FileId", c => c.Int());
            AddColumn("dbo.MessageTemplate", "Attachment2FileId", c => c.Int());
            AddColumn("dbo.MessageTemplate", "Attachment3FileId", c => c.Int());
        }
        
        public override void Down()
        {
			DropColumn("dbo.MessageTemplate", "Attachment3FileId");
			DropColumn("dbo.MessageTemplate", "Attachment2FileId");
			DropColumn("dbo.MessageTemplate", "Attachment1FileId");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Common.Replace",
				"Replace",
				"Ersetzen");
		}
    }
}
