namespace SmartStore.MegaMenu.Data.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;

	internal sealed class Configuration : DbMigrationsConfiguration<MegaMenuObjectContext>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = false;
			MigrationsDirectory = @"Data\Migrations";
			ContextKey = "SmartStore.MegaMenu"; // DO NOT CHANGE!
		}

		protected override void Seed(MegaMenuObjectContext context)
		{
		}
	}
}
