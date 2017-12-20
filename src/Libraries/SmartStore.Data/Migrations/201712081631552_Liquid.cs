namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	using System.Web.Hosting;
	using System.Linq;
	using SmartStore.Core.Data;
	using SmartStore.Core.Domain.Localization;
	using SmartStore.Data.Setup;
	using SmartStore.Data.Utilities;

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
			
			if (HostingEnvironment.IsHosted && DataSettings.DatabaseIsInstalled())
			{
				// Import all xml templates on disk 
				var converter = new MessageTemplateConverter(context);
				var language = ResolveMasterLanguage(context);
				converter.ImportAll(language);

				DropDefaultValueConstraint(context);
			}
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.Delete(
				"Admin.System.QueuedEmails.Fields.FromName",
				"Admin.System.QueuedEmails.Fields.FromName.Hint",
				"Admin.System.QueuedEmails.Fields.ToName",
				"Admin.System.QueuedEmails.Fields.ToName.Hint");

			builder.AddOrUpdate("Admin.System.QueuedEmails.Fields.ReplyTo",
				"Reply to",
				"Antwort an",
				"Reply-To address of the email.",
				"Antwortadresse der E-Mail.");
		}

		private Language ResolveMasterLanguage(SmartObjectContext context)
		{
			var query = context.Set<Language>().OrderBy(x => x.DisplayOrder);

			var language = query
				.Where(x => (x.UniqueSeoCode == "de" || x.UniqueSeoCode == "en") && x.Published)
				.FirstOrDefault();

			if (language == null)
			{
				language = query.Where(x => x.Published).FirstOrDefault();
			}

			return language;
		}

		private void DropDefaultValueConstraint(SmartObjectContext context)
		{
			// During migration we created a new NON-Nullable column ("To")
			// with a default value contraint of ' ', otherwise column creation
			// would have failed. Now we need to get rid of this constraint.

			if (DataSettings.Current.IsSqlServer)
			{
				string sql = @"DECLARE @name nvarchar(100)
SELECT @name = [name] from sys.objects WHERE type = 'D' and parent_object_id = object_id('MessageTemplate')
IF (@name is not null) BEGIN EXEC ('ALTER TABLE [MessageTemplate] Drop Constraint [' + @name +']') END";

				context.ExecuteSqlCommand(sql);
			}	
		}

		public bool RollbackOnFailure => true;
	}
}
