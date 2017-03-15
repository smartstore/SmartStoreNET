namespace SmartStore.Data.Migrations
{
	using Setup;
	using System;
	using System.Data.Entity.Migrations;
	using Core.Domain.Tasks;

	public partial class CategoryBadges : DbMigration, IDataSeeder<SmartObjectContext>
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
    }
}
