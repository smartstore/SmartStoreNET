namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class ExportFramework2 : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.ExportProfile", "ResultInfo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExportProfile", "ResultInfo");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			context.Execute("DELETE FROM [dbo].[ScheduleTask] WHERE [Type] = 'SmartStore.GoogleMerchantCenter.StaticFileGenerationTask, SmartStore.GoogleMerchantCenter'");

			context.MigrateSettings(x =>
			{
				x.DeleteGroup("FroogleSettings");
			});
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Common.Example", "Example", "Beispiel");

			builder.AddOrUpdate("Admin.DataExchange.Export.CloneProfile",
				"Apply settings from",
				"Einstellungen übernehmen von",
				"Specifies an export profile from which to apply the settings.",
				"Legt das Exportprofil fest, von welchem die Einstellungen übernommen werden sollen.");



			builder.Delete(
				"Plugins.Feed.Froogle.TaskEnabled",
				"Plugins.Feed.Froogle.TaskEnabled.Hint",
				"Plugins.Feed.Froogle.StaticFileUrl",
				"Plugins.Feed.Froogle.StaticFileUrl.Hint",
				"Plugins.Feed.Froogle.GenerateStaticFileEachMinutes",
				"Plugins.Feed.Froogle.GenerateStaticFileEachMinutes.Hint",
				"Plugins.Feed.Froogle.Currency",
				"Plugins.Feed.Froogle.Currency.Hint",
				"Plugins.Feed.Froogle.ProductPictureSize",
				"Plugins.Feed.Froogle.ProductPictureSize.Hint",
				"Plugins.Feed.Froogle.AppendDescriptionText",
				"Plugins.Feed.Froogle.AppendDescriptionText.Hint",
				"Plugins.Feed.Froogle.BuildDescription",
				"Plugins.Feed.Froogle.BuildDescription.Hint",
				"Plugins.Feed.Froogle.Automatic",
				"Plugins.Feed.Froogle.DescShort",
				"Plugins.Feed.Froogle.DescLong",
				"Plugins.Feed.Froogle.DescTitleAndShort",
				"Plugins.Feed.Froogle.DescTitleAndLong",
				"Plugins.Feed.Froogle.DescManuAndTitleAndShort",
				"Plugins.Feed.Froogle.DescManuAndTitleAndLong",
				"Plugins.Feed.Froogle.UseOwnProductNo",
				"Plugins.Feed.Froogle.UseOwnProductNo.Hint",
				"Plugins.Feed.Froogle.DescriptionToPlainText",
				"Plugins.Feed.Froogle.DescriptionToPlainText.Hint",
				"Plugins.Feed.Froogle.Brand",
				"Plugins.Feed.Froogle.Brand.Hint",
				"Plugins.Feed.Froogle.Store",
				"Plugins.Feed.Froogle.Store.Hint",
				"Plugins.Feed.Froogle.ConvertNetToGrossPrices",
				"Plugins.Feed.Froogle.ConvertNetToGrossPrices.Hint",
				"Plugins.Feed.Froogle.LanguageId",
				"Plugins.Feed.Froogle.LanguageId.Hint",
				"Plugins.Feed.Froogle.Generate",
				"Plugins.Feed.Froogle.ConfigSaveNote",
				"Plugins.Feed.Froogle.AvailabilityAvailableForOrder",
				"Plugins.Feed.Froogle.GridEditNote",
				"Plugins.Feed.Froogle.General",
				"Plugins.Feed.Froogle.ProductData"
			);
		}
    }
}
