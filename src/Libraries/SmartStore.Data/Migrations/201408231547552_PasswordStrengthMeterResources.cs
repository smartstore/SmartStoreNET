namespace SmartStore.Data.Migrations
{
    using SmartStore.Data.Setup;
    using System;
    using System.Data.Entity.Migrations;

    public partial class PasswordStrengthMeterResources : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
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

            context.MigrateSettings(x =>
            {
                x.Add("customersettings.showpasswordstrengthmeter", false);
            });
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.ShowPasswordStrengthMeter",
                "Show password strength meter",
                "Zeigen Passwort Stärke-Messgerät",
                "A value indicating whether password strength meter is shown.",
                "Ein Wert, der angibt, ob Passwort Stärke-Messgerät angezeigt.");
        }
    }
}
