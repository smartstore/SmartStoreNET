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
			context.Execute("DELETE FROM [dbo].[ScheduleTask] WHERE [Type] = 'SmartStore.BMEcat.StaticFileGenerationTask, SmartStore.BMEcat'");

			context.MigrateSettings(x =>
			{
				x.DeleteGroup("FroogleSettings");
				x.DeleteGroup("BMEcatExportSettings");
				x.DeleteGroup("OpenTransSettings");
			});
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Common.Example", "Example", "Beispiel");

			builder.AddOrUpdate("Admin.Common.FilesDeleted",
				"{0} files were deleted",
				"{0} Dateien wurden gelöscht");

			builder.AddOrUpdate("Admin.Common.FoldersDeleted",
				"{0} folders were deleted",
				"{0} Verzeichnisse wurden gelöscht");

			builder.AddOrUpdate("Admin.DataExchange.Export.CloneProfile",
				"Apply settings from",
				"Einstellungen übernehmen von",
				"Specifies an export profile from which to apply the settings.",
				"Legt das Exportprofil fest, von welchem die Einstellungen übernommen werden sollen.");

			builder.AddOrUpdate("Admin.DataExchange.Export.NonFileBasedExport.Note",
				"The export provider does not explicit support any file type. Therefore, the export provider is responsible for futher deployment of export data.",
				"Der Export-Provider unterstützt keinen expliziten Dateityp. Für eine weitere Bereitstellung der Exportdaten ist daher der Export-Provider verantwortlich.");


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

			builder.Delete(
				"Plugins.Feed.BMEcat.TaskEnabled",
				"Plugins.Feed.BMEcat.TaskEnabled.Hint",
				"Plugins.Feed.BMEcat.StaticFileUrl",
				"Plugins.Feed.BMEcat.StaticFileUrl.Hint",
				"Plugins.Feed.BMEcat.GenerateStaticFileEachMinutes",
				"Plugins.Feed.BMEcat.GenerateStaticFileEachMinutes.Hint",
				"Plugins.Feed.BMEcat.UseOwnProductNo",
				"Plugins.Feed.BMEcat.UseOwnProductNo.Hint",
				"Plugins.Feed.BMEcat.ShippingCostAustria",
				"Plugins.Feed.BMEcat.ShippingCostAustria.Hint",
				"Plugins.Feed.BMEcat.Currency",
				"Plugins.Feed.BMEcat.Currency.Hint",
				"Plugins.Feed.BMEcat.ProductPictureSize",
				"Plugins.Feed.BMEcat.ProductPictureSize.Hint",
				"Plugins.Feed.BMEcat.AppendDescriptionText",
				"Plugins.Feed.BMEcat.AppendDescriptionText.Hint",
				"Plugins.Feed.BMEcat.BuildDescription",
				"Plugins.Feed.BMEcat.BuildDescription.Hint",
				"Plugins.Feed.BMEcat.Automatic",
				"Plugins.Feed.BMEcat.DescShort",
				"Plugins.Feed.BMEcat.DescLong",
				"Plugins.Feed.BMEcat.DescTitleAndShort",
				"Plugins.Feed.BMEcat.DescTitleAndLong",
				"Plugins.Feed.BMEcat.DescManuAndTitleAndShort",
				"Plugins.Feed.BMEcat.DescManuAndTitleAndLong",
				"Plugins.Feed.BMEcat.UseOwnProductNo",
				"Plugins.Feed.BMEcat.UseOwnProductNo.Hint",
				"Plugins.Feed.BMEcat.DescriptionToPlainText",
				"Plugins.Feed.BMEcat.DescriptionToPlainText.Hint",
				"Plugins.Feed.BMEcat.ShippingCost",
				"Plugins.Feed.BMEcat.ShippingCost.Hint",
				"Plugins.Feed.BMEcat.ShippingTime",
				"Plugins.Feed.BMEcat.ShippingTime.Hint",
				"Plugins.Feed.BMEcat.Brand",
				"Plugins.Feed.BMEcat.Brand.Hint",
				"Plugins.Feed.BMEcat.Store",
				"Plugins.Feed.BMEcat.Store.Hint",
				"Plugins.Feed.BMEcat.ConvertNetToGrossPrices",
				"Plugins.Feed.BMEcat.ConvertNetToGrossPrices.Hint",
				"Plugins.Feed.BMEcat.LanguageId",
				"Plugins.Feed.BMEcat.LanguageId.Hint",
				"Plugins.Feed.BMEcat.Generate",
				"Plugins.Feed.BMEcat.ConfigSaveNote"
			);

			builder.Delete("Plugins.Widgets.OpenTrans.IsLexwareCompatibe");
			builder.Delete("Admin.System.Maintenance.DeleteExportedFolders.TotalDeleted");
		}
    }
}
