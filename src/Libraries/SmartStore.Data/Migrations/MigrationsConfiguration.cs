namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Setup;
    using SmartStore.Core.Data;
    using SmartStore.Utilities;

    public sealed class MigrationsConfiguration : DbMigrationsConfiguration<SmartObjectContext>
    {
        public MigrationsConfiguration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "SmartStore.Core";

            if (DataSettings.Current.IsSqlServer)
            {
                var commandTimeout = CommonHelper.GetAppSetting<int?>("sm:EfMigrationsCommandTimeout");
                if (commandTimeout.HasValue)
                {
                    CommandTimeout = commandTimeout.Value;
                }

                CommandTimeout = 9999999;
            }
        }

        public void SeedDatabase(SmartObjectContext context)
        {
            using (var scope = new DbContextScope(context, hooksEnabled: false))
            {
                Seed(context);
                scope.Commit();
            }
        }

        protected override void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);
            MigrateSettings(context);
        }

        public void MigrateSettings(SmartObjectContext context)
        {

        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.DisplayRegionInLanguageSelector",
                "Language display including region",
                "Sprachanzeige inklusive Region",
                "Enable this option if the display of the language in the language selector should be shown including region e.g.: Englisch (United States).",
                "Aktivieren Sie diese Option, wenn die Anzeige der Sprache in der Sprachauswahl inklusive Region angezeigt werden soll z.B.: Deutsch (Deutschland).");        
        }
    }
}