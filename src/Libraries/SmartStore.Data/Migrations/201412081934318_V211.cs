namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class V211 : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
        }
        
        public override void Down()
        {
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
			builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.CanonicalHostNameRule",
				"Canonical host name rule",
				"Regel für kanonischen Domänennamen");
			builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.CanonicalHostNameRule.Hint",
				"Enforces permanent redirection to a single domain name for a better page rank (e.g. myshop.com > www.myshop.com or vice versa)",
				"Erzwingt die permanente Umleitung zu einem einzelnen Domännennamen für ein besseres Seitenranking (z.B. meinshop.de > www.meinshop.de oder umgekehrt)");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Seo.CanonicalHostNameRule.NoRule",
				"Don't apply",
				"Nicht anwenden");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Seo.CanonicalHostNameRule.RequireWww",
				"Require www prefix",
				"www-Präfix erzwingen");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Seo.CanonicalHostNameRule.OmitWww",
				"Omit www prefix",
				"www-Präfix weglassen");
		}
    }
}
