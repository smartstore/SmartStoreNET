namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class Liquid : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
        public override void Up()
        {
            AddColumn("dbo.MessageTemplate", "To", c => c.String(nullable: false, maxLength: 500, defaultValue: " "));
            AddColumn("dbo.MessageTemplate", "ReplyTo", c => c.String(maxLength: 500));
            AddColumn("dbo.MessageTemplate", "ModelTypes", c => c.String(maxLength: 500));
            AddColumn("dbo.MessageTemplate", "LastModelTree", c => c.String());
            DropColumn("dbo.QueuedEmail", "FromName");
            DropColumn("dbo.QueuedEmail", "ToName");
            DropColumn("dbo.QueuedEmail", "ReplyToName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QueuedEmail", "ReplyToName", c => c.String(maxLength: 500));
            AddColumn("dbo.QueuedEmail", "ToName", c => c.String(maxLength: 500));
            AddColumn("dbo.QueuedEmail", "FromName", c => c.String(maxLength: 500));
            DropColumn("dbo.MessageTemplate", "LastModelTree");
            DropColumn("dbo.MessageTemplate", "ModelTypes");
            DropColumn("dbo.MessageTemplate", "ReplyTo");
            DropColumn("dbo.MessageTemplate", "To");
        }

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);
			context.SaveChanges();
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.Delete(
				"Admin.System.QueuedEmails.Fields.FromName",
				"Admin.System.QueuedEmails.Fields.FromName.Hint",
				"Admin.System.QueuedEmails.Fields.ToName",
				"Admin.System.QueuedEmails.Fields.ToName.Hint");
		}

		public bool RollbackOnFailure => false;
	}
}
