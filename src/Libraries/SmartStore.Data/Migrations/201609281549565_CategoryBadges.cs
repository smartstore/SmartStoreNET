namespace SmartStore.Data.Migrations
{
	using Setup;
	using System;
	using System.Data.Entity.Migrations;
	using Core.Domain.Tasks;

	public partial class CategoryBadges : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Category", "BadgeText", c => c.String(maxLength: 400));
            AddColumn("dbo.Category", "BadgeStyle", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Category", "BadgeStyle");
            DropColumn("dbo.Category", "BadgeText");
        }

        public bool RollbackOnFailure
        {
            get { return false; }
        }

        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);

			context.Set<ScheduleTask>().AddOrUpdate(x => x.Type,
				new ScheduleTask
				{
					Name = "Rebuild XML Sitemap",
					CronExpression = "45 3 * * *",
					Type = "SmartStore.Services.Seo.RebuildXmlSitemapTask, SmartStore.Services",
					Enabled = true,
					StopOnError = false
				}
			);

			context.SaveChanges();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.Catalog.Categories.Fields.BadgeText",
                "Badge text",
                "Badge-Text",
                "Gets or sets the text of the badge which will be displayed next to the category link within menus.",
                "Legt den Text der Badge fest, die innerhalb von Menus neben den Menueinträgen dargestellt wird.");

            builder.AddOrUpdate("Admin.Catalog.Categories.Fields.BadgeStyle",
                "Badge style",
                "Badge-Style",
                "Gets or sets the type of the badge which will be displayed next to the category link within menus.",
                "Legt den Stil der Badge fest, die innerhalb von Menus neben den Menueinträgen dargestellt wird.");

			builder.AddOrUpdate("Admin.Header.ClearDbCache",
				"Clear database cache",
				"Datenbank Cache löschen");

			builder.AddOrUpdate("Admin.System.Warnings.TaskScheduler.OK",
				"The task scheduler can poll and execute tasks.",
				"Der Task-Scheduler kann Hintergrund-Aufgaben planen und ausführen.");

			builder.AddOrUpdate("Admin.System.Warnings.TaskScheduler.Fail",
				"The task scheduler cannot poll and execute tasks. Base URL: {0}, Status: {1}. Please specify a working base url in web.config, setting 'sm:TaskSchedulerBaseUrl'.",
				"Der Task-Scheduler kann keine Hintergrund-Aufgaben planen und ausführen. Basis-URL: {0}, Status: {1}. Bitte legen Sie eine vom Webserver erreichbare Basis-URL in der web.config Datei fest, Einstellung: 'sm:TaskSchedulerBaseUrl'.");

			builder.AddOrUpdate("Products.NotFound",
				"The product with ID {0} was not found.",
				"Das Produkt mit der ID {0} wurde nicht gefunden.");

			builder.AddOrUpdate("Products.Deleted",
				"The product with ID {0} has been deleted.",
				"Das Produkt mit der ID {0} wurde gelöscht.");

			builder.AddOrUpdate("Common.ShowLess", "Show less", "Weniger anzeigen");

            builder.AddOrUpdate("Menu.ServiceMenu", "Help & Services", "Hilfe & Service");
        }
    }
}
