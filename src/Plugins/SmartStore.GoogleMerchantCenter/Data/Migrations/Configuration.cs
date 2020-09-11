namespace SmartStore.GoogleMerchantCenter.Data.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<GoogleProductObjectContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Data\Migrations";
            ContextKey = "SmartStore.FeedGoogle"; // DO NOT CHANGE!
        }

        protected override void Seed(GoogleProductObjectContext context)
        {
        }
    }
}
