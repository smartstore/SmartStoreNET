namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using Setup;
	using Core.Domain.Tasks;
	using System.Collections.Generic;
	using Core.Domain;
	using System.Linq;
	using Utilities;
	using Core.Domain.Security;
	using Core.Domain.Customers;

	public partial class ExportFramework3 : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
		public override void Up()
		{
			AddColumn("dbo.ExportProfile", "SystemName", c => c.String(maxLength: 400));
			AddColumn("dbo.ExportProfile", "IsSystemProfile", c => c.Boolean(nullable: false));
		}

		public override void Down()
		{
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
				Category = "Standard"
			}, new string[] { SystemCustomerRoleNames.Administrators });


			var systemProfilesInfos = new List<SystemProfileInfo>
			{
				new SystemProfileInfo { SystemName = "SmartStoreCategoryXml", ProviderSystemName = "Exports.SmartStoreCategoryXml", Name = "Category XML Export" },
				new SystemProfileInfo { SystemName = "SmartStoreCustomerXlsx", ProviderSystemName = "Exports.SmartStoreCustomerXlsx", Name = "Customer Excel Export" },
				new SystemProfileInfo { SystemName = "SmartStoreCustomerXml", ProviderSystemName = "Exports.SmartStoreCustomerXml", Name = "Customer XML Export" },
				new SystemProfileInfo { SystemName = "SmartStoreManufacturerXml", ProviderSystemName = "Exports.SmartStoreManufacturerXml", Name = "Manufacturer XML Export" },
				new SystemProfileInfo { SystemName = "SmartStoreOrderXlsx", ProviderSystemName = "Exports.SmartStoreOrderXlsx", Name = "Order Excel Export" },
				new SystemProfileInfo { SystemName = "SmartStoreOrderXml", ProviderSystemName = "Exports.SmartStoreOrderXml", Name = "Order XML Export" },
				new SystemProfileInfo { SystemName = "SmartStoreProductXlsx", ProviderSystemName = "Exports.SmartStoreProductXlsx", Name = "Product Excel Export" },
				new SystemProfileInfo { SystemName = "SmartStoreProductXml", ProviderSystemName = "Exports.SmartStoreProductXml", Name = "Product XML Export" },
				new SystemProfileInfo { SystemName = "SmartStoreNewsSubscriptionCsv", ProviderSystemName = "Exports.SmartStoreNewsSubscriptionCsv", Name = "Newsletter Subscribers CSV Export" }
			};

			var tasks = context.Set<ScheduleTask>();
			var profiles = context.Set<ExportProfile>();

			foreach (var profileInfo in systemProfilesInfos)
			{
				var profile = profiles.FirstOrDefault(x => x.IsSystemProfile && x.SystemName == profileInfo.SystemName && x.ProviderSystemName == profileInfo.ProviderSystemName);

				if (profile != null)
					continue;

				var task = new ScheduleTask
				{
					CronExpression = "0 */6 * * *",     // every six hours
					Type = "SmartStore.Services.DataExchange.DataExportTask, SmartStore.Services",
					Enabled = false,
					StopOnError = false,
					IsHidden = true
				};

				task.Name = string.Concat(profileInfo.Name, " task");

				task = tasks.Add(task);
				context.SaveChanges();

				var seoName = SeoHelper.GetSeName(profileInfo.Name, true, false).Replace("/", "").Replace("-", "");

				profile = new ExportProfile
				{
					IsSystemProfile = true,
					Name = profileInfo.Name,
					SystemName = profileInfo.SystemName,
					ProviderSystemName = profileInfo.ProviderSystemName,
					FolderName = string.Concat("sm-", seoName.Replace("export", "").ToValidPath().Truncate(50)),
					FileNamePattern = "%Store.Id%-%Profile.Id%-%File.Index%-%Profile.SeoName%",
					Enabled = true,
					PerStore = false,
					CreateZipArchive = false,
					Cleanup = false,
					SchedulingTaskId = task.Id
				};

				profile = profiles.Add(profile);
				context.SaveChanges();

				task.Alias = profile.Id.ToString();

				tasks.AddOrUpdate(task);
				context.SaveChanges();
			}
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportEntityType.NewsLetterSubscription",
				"Newsletter Subscribers",
				"Newsletter Abonnenten");


			builder.AddOrUpdate("Providers.FriendlyName.Exports.SmartStoreCategoryXml",
				"Category XML Export",
				"Warengruppen XML Export");
			builder.AddOrUpdate("Providers.Description.Exports.SmartStoreCategoryXml",
				"Allows to export category data in XML format.",
				"Ermöglicht den Export von Warengruppendaten im XML Format.");

			builder.AddOrUpdate("Providers.FriendlyName.Exports.SmartStoreCustomerXlsx",
				"Customer Excel Export",
				"Kunden Excel Export");
			builder.AddOrUpdate("Providers.Description.Exports.SmartStoreCustomerXlsx",
				"Allows to export customer data in Excel format.",
				"Ermöglicht den Export von Kundendaten im Excel Format.");

			builder.AddOrUpdate("Providers.FriendlyName.Exports.SmartStoreCustomerXml",
				"Customer XML Export",
				"Kunden XML Export");
			builder.AddOrUpdate("Providers.Description.Exports.SmartStoreCustomerXml",
				"Allows to export customer data in XML format.",
				"Ermöglicht den Export von Kundendaten im XML Format.");

			builder.AddOrUpdate("Providers.FriendlyName.Exports.SmartStoreManufacturerXml",
				"Manufacturer XML Export",
				"Hersteller XML Export");
			builder.AddOrUpdate("Providers.Description.Exports.SmartStoreManufacturerXml",
				"Allows to export manufacturer data in XML format.",
				"Ermöglicht den Export von Herstellerdaten im XML Format.");

			builder.AddOrUpdate("Providers.FriendlyName.Exports.SmartStoreOrderXlsx",
				"Order Excel Export",
				"Auftrags Excel Export");
			builder.AddOrUpdate("Providers.Description.Exports.SmartStoreOrderXlsx",
				"Allows to export order data in Excel format.",
				"Ermöglicht den Export von Auftragsdaten im Excel Format.");

			builder.AddOrUpdate("Providers.FriendlyName.Exports.SmartStoreOrderXml",
				"Order XML Export",
				"Auftrags XML Export");
			builder.AddOrUpdate("Providers.Description.Exports.SmartStoreOrderXml",
				"Allows to export order data in XML format.",
				"Ermöglicht den Export von Auftragsdaten im XML Format.");

			builder.AddOrUpdate("Providers.FriendlyName.Exports.SmartStoreProductXlsx",
				"Product Excel Export",
				"Produkt Excel Export");
			builder.AddOrUpdate("Providers.Description.Exports.SmartStoreProductXlsx",
				"Allows to export product data in Excel format.",
				"Ermöglicht den Export von Produktdaten im Excel Format.");

			builder.AddOrUpdate("Providers.FriendlyName.Exports.SmartStoreProductXml",
				"Product XML Export",
				"Produkt XML Export");
			builder.AddOrUpdate("Providers.Description.Exports.SmartStoreProductXml",
				"Allows to export product data in XML format.",
				"Ermöglicht den Export von Produktdaten im XML Format.");

			builder.AddOrUpdate("Providers.FriendlyName.Exports.SmartStoreNewsSubscriptionCsv",
				"Newsletter Subscribers CSV Export",
				"Newsletter Abonnenten CSV Export");
			builder.AddOrUpdate("Providers.Description.Exports.SmartStoreNewsSubscriptionCsv",
				"Allows to export newsletter subscriber data in CSV format.",
				"Ermöglicht den Export von Newsletter Abonnentendaten im CSV Format.");


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
		}
	}


	internal class SystemProfileInfo
	{
		public string SystemName { get; set; }
		public string ProviderSystemName { get; set; }
		public string Name { get; set; }
	}
}