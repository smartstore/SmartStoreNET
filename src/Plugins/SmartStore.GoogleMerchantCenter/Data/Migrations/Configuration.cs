namespace SmartStore.GoogleMerchantCenter.Data.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;

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
