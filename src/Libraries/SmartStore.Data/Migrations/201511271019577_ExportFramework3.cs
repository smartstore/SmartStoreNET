namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using Core.Domain.Customers;
	using Core.Domain.Security;
	using Setup;

	public partial class ExportFramework3 : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
        public override void Up()
        {
            AddColumn("dbo.ExportProfile", "SystemName", c => c.String(maxLength: 400));
            AddColumn("dbo.ExportProfile", "IsSystemProfile", c => c.Boolean(nullable: false));
            DropColumn("dbo.PaymentMethod", "ExcludedCustomerRoleIds");
            DropColumn("dbo.PaymentMethod", "ExcludedCountryIds");
            DropColumn("dbo.PaymentMethod", "ExcludedShippingMethodIds");
            DropColumn("dbo.PaymentMethod", "CountryExclusionContextId");
            DropColumn("dbo.PaymentMethod", "MinimumOrderAmount");
            DropColumn("dbo.PaymentMethod", "MaximumOrderAmount");
            DropColumn("dbo.PaymentMethod", "AmountRestrictionContextId");
            DropColumn("dbo.ShippingMethod", "ExcludedCustomerRoleIds");
            DropColumn("dbo.ShippingMethod", "CountryExclusionContextId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ShippingMethod", "CountryExclusionContextId", c => c.Int(nullable: false));
            AddColumn("dbo.ShippingMethod", "ExcludedCustomerRoleIds", c => c.String(maxLength: 500));
            AddColumn("dbo.PaymentMethod", "AmountRestrictionContextId", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentMethod", "MaximumOrderAmount", c => c.Decimal(precision: 18, scale: 4));
            AddColumn("dbo.PaymentMethod", "MinimumOrderAmount", c => c.Decimal(precision: 18, scale: 4));
            AddColumn("dbo.PaymentMethod", "CountryExclusionContextId", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentMethod", "ExcludedShippingMethodIds", c => c.String(maxLength: 500));
            AddColumn("dbo.PaymentMethod", "ExcludedCountryIds", c => c.String(maxLength: 2000));
            AddColumn("dbo.PaymentMethod", "ExcludedCustomerRoleIds", c => c.String(maxLength: 500));
            DropColumn("dbo.ExportProfile", "IsSystemProfile");
            DropColumn("dbo.ExportProfile", "SystemName");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			var permissionMigrator = new PermissionMigrator(context);

			permissionMigrator.AddPermission(new PermissionRecord
			{
				Name = "Admin area. Manage Url Records",
				SystemName = "ManageUrlRecords",
				Category = "Configuration"
			}, new string[] { SystemCustomerRoleNames.Administrators });
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Common.Export.PDF", "PDF Export", "PDF Export");
			builder.AddOrUpdate("Admin.Common.TemporaryFiles", "Temporary files", "Temporäre Dateien");
			builder.AddOrUpdate("Admin.Common.PublicFiles", "Public files", "Öffentliche Dateien");
			builder.AddOrUpdate("Admin.Common.Of", "of", "von");

			builder.AddOrUpdate("Admin.Common.NoTempFilesFound",
				"There are no temporary files.",
				"Es sind keine temporären Dateien vorhanden.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportEntityType.NewsLetterSubscription",
				"Newsletter Subscribers",
				"Newsletter Abonnenten");

			builder.AddOrUpdate("Admin.DataExchange.Export.SystemName",
				"System name of profile",
				"Systemname des Profils",
				"The system name of the export profile.",
				"Der Systemname des Exportprofils.");

			builder.AddOrUpdate("Admin.DataExchange.Export.IsSystemProfile",
				"System profile",
				"Systemprofil",
				"Indicates whether the export profile is a system profile. System profiles cannot be removed.",
				"Gibt an, ob es sich bei dem Exportprofil um eine Systemprofil handelt. Systemprofile können nicht entfernt werden.");

			builder.AddOrUpdate("Admin.DataExchange.Export.CannotDeleteSystemProfile",
				"Cannot delete a system export profile.",
				"Ein System-Exportprofil kann nicht gelöscht werden.");

			builder.AddOrUpdate("Admin.DataExchange.Export.MissingSystemProfile",
				"The system export profile {0} was not found.",
				"Das System-Exportprofil {0} wurde nicht gefunden.");

			builder.AddOrUpdate("Admin.DataExchange.Export.ExportFiles",
				"Export files",
				"Exportdateien");


			builder.AddOrUpdate("Admin.Configuration.Payment.Methods.RestrictionNote",
				"There were no possibilities found to restrict payment methods.",
				"Es wurden keine Möglichkeiten zur Einschränkung von Zahlungsarten gefunden.");

			builder.AddOrUpdate("Admin.Configuration.Shipping.Methods.RestrictionNote",
				"There were no possibilities found to restrict shipping methods.",
				"Es wurden keine Möglichkeiten zur Einschränkung von Versandarten gefunden.");



			builder.Delete(
				"Admin.Configuration.Payment.Methods.ExcludedCustomerRole",
				"Admin.Configuration.Payment.Methods.ExcludedShippingMethod",
				"Admin.Configuration.Payment.Methods.ExcludedCountry",
				"Admin.Configuration.Payment.Methods.MinimumOrderAmount",
				"Admin.Configuration.Payment.Methods.MaximumOrderAmount",
				"Admin.Configuration.Restrictions.AmountRestrictionContext",
				"Enums.SmartStore.Core.Domain.Common.AmountRestrictionContextType.SubtotalAmount",
				"Enums.SmartStore.Core.Domain.Common.AmountRestrictionContextType.TotalAmount",
				"Enums.SmartStore.Core.Domain.Common.CountryRestrictionContextType.BillingAddress",
				"Enums.SmartStore.Core.Domain.Common.CountryRestrictionContextType.ShippingAddress",
				"Admin.Configuration.Shipping.Methods.ExcludedCustomerRole",
				"Admin.Configuration.Shipping.Methods.ExcludedCountry",
				"Admin.Configuration.Restrictions.CountryExclusionContext",
				"Admin.Common.ExportToXml.All",
				"Admin.Common.ExportToXml.Selected",
				"Admin.Common.ExportToCsv.All",
				"Admin.Common.ExportToCsv.Selected",
				"Admin.Common.ExportToCsv",
				"Admin.Common.ExportToPdf.TocTitle",
				"PDFProductCatalog.SKU",
				"PDFProductCatalog.Price",
				"PDFProductCatalog.Manufacturer",
				"PDFProductCatalog.Weight",
				"PDFProductCatalog.Length",
				"PDFProductCatalog.Width",
				"PDFProductCatalog.Height",
				"PDFProductCatalog.SpecificationAttributes",
				"PDFProductCatalog.BundledItems",
				"PDFProductCatalog.AssociatedProducts"
			);
		}
	}
}
