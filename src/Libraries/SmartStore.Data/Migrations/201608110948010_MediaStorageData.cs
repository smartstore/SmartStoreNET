namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.IO;
	using System.Linq;
	using Core;
	using Core.Data;
	using Core.Domain.Configuration;
	using Core.Domain.Media;
	using Core.Domain.Messages;
	using Core.IO;
	using Setup;

	public partial class MediaStorageData : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
		private const int PAGE_SIZE = 200;

		private string GetFileName(int id, string extension, string mimeType)
		{
			if (extension.IsEmpty())
				extension = MimeTypes.MapMimeTypeToExtension(mimeType);

			var fileName = string.Format("{0}-0{1}", id.ToString("0000000"), extension.EmptyNull().EnsureStartsWith("."));
			return fileName;
		}

		public override void Up()
        {
            CreateTable(
                "dbo.MediaStorage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Data = c.Binary(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Discount_AppliedToManufacturers",
                c => new
                    {
                        Discount_Id = c.Int(nullable: false),
                        Manufacturer_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Discount_Id, t.Manufacturer_Id })
                .ForeignKey("dbo.Discount", t => t.Discount_Id, cascadeDelete: true)
                .ForeignKey("dbo.Manufacturer", t => t.Manufacturer_Id, cascadeDelete: true)
                .Index(t => t.Discount_Id)
                .Index(t => t.Manufacturer_Id);
            
            AddColumn("dbo.Picture", "MediaStorageId", c => c.Int());
            AddColumn("dbo.Manufacturer", "HasDiscountsApplied", c => c.Boolean(nullable: false));
            AddColumn("dbo.Download", "MediaStorageId", c => c.Int());
            AddColumn("dbo.QueuedEmailAttachment", "MediaStorageId", c => c.Int());
            CreateIndex("dbo.Picture", "MediaStorageId");
            CreateIndex("dbo.Download", "MediaStorageId");
            CreateIndex("dbo.QueuedEmailAttachment", "MediaStorageId");
            AddForeignKey("dbo.Picture", "MediaStorageId", "dbo.MediaStorage", "Id");
            AddForeignKey("dbo.Download", "MediaStorageId", "dbo.MediaStorage", "Id");
            AddForeignKey("dbo.QueuedEmailAttachment", "MediaStorageId", "dbo.MediaStorage", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QueuedEmailAttachment", "MediaStorageId", "dbo.MediaStorage");
            DropForeignKey("dbo.Download", "MediaStorageId", "dbo.MediaStorage");
            DropForeignKey("dbo.Discount_AppliedToManufacturers", "Manufacturer_Id", "dbo.Manufacturer");
            DropForeignKey("dbo.Discount_AppliedToManufacturers", "Discount_Id", "dbo.Discount");
            DropForeignKey("dbo.Picture", "MediaStorageId", "dbo.MediaStorage");
            DropIndex("dbo.Discount_AppliedToManufacturers", new[] { "Manufacturer_Id" });
            DropIndex("dbo.Discount_AppliedToManufacturers", new[] { "Discount_Id" });
            DropIndex("dbo.QueuedEmailAttachment", new[] { "MediaStorageId" });
            DropIndex("dbo.Download", new[] { "MediaStorageId" });
            DropIndex("dbo.Picture", new[] { "MediaStorageId" });
            DropColumn("dbo.QueuedEmailAttachment", "MediaStorageId");
            DropColumn("dbo.Download", "MediaStorageId");
            DropColumn("dbo.Manufacturer", "HasDiscountsApplied");
            DropColumn("dbo.Picture", "MediaStorageId");
            DropTable("dbo.Discount_AppliedToManufacturers");
            DropTable("dbo.MediaStorage");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
#pragma warning disable 612, 618
			context.MigrateLocaleResources(MigrateLocaleResources);

			var mediaStorages = context.Set<MediaStorage>();
			var fileSystem = new LocalFileSystem();
			var mediaBasePath = $"Media\\{DataSettings.Current.TenantName}";
			var storeMediaInDb = true;

			fileSystem.TryCreateFolder(Path.Combine(mediaBasePath, "Downloads"));
			fileSystem.TryCreateFolder(Path.Combine(mediaBasePath, "QueuedEmailAttachment"));

			{
				var settings = context.Set<Setting>();

				// be careful, this setting does not necessarily exist
				var storeInDbSetting = settings.FirstOrDefault(x => x.Name == "Media.Images.StoreInDB");
				if (storeInDbSetting != null)
				{
					storeMediaInDb = storeInDbSetting.Value.ToBool(true);

					// remove old bool StoreInDB because it's not used anymore
					settings.Remove(storeInDbSetting);
				}

				// set current media storage provider system name
				settings.AddOrUpdate(x => x.Name, new Setting
				{
					Name = "Media.Storage.Provider",
					Value = (storeMediaInDb ? "MediaStorage.SmartStoreDatabase" : "MediaStorage.SmartStoreFileSystem")
				});

				context.SaveChanges();
			}

			#region Pictures

			if (storeMediaInDb)
			{
				PageEntities(context, mediaStorages, context.Set<Picture>().OrderBy(x => x.Id), picture =>
				{

					if (picture.PictureBinary != null && picture.PictureBinary.LongLength > 0)
					{
						picture.MediaStorage = new MediaStorage { Data = picture.PictureBinary };
						picture.PictureBinary = null;
					}
				});
			}

			#endregion

			#region Downloads

			PageEntities(context, mediaStorages, context.Set<Download>().OrderBy(x => x.Id), download =>
			{
				if (download.DownloadBinary != null && download.DownloadBinary.LongLength > 0)
				{
					if (storeMediaInDb)
					{
						// move binary data
						download.MediaStorage = new MediaStorage { Data = download.DownloadBinary };
					}
					else
					{
						// move to file system. it's necessary because from now on DownloadService depends on current storage provider
						// and it would not find the binary data anymore if not moved.
						var fileName = GetFileName(download.Id, download.Extension, download.ContentType);
						var path = fileSystem.Combine(fileSystem.Combine(mediaBasePath, "Downloads"), fileName);

						fileSystem.WriteAllBytes(path, download.DownloadBinary);
					}

					download.DownloadBinary = null;
				}
			});

			#endregion

			#region Queued email attachments

			var attachmentQuery = context.Set<QueuedEmailAttachment>()
				.Where(x => x.StorageLocation == EmailAttachmentStorageLocation.Blob)
				.OrderBy(x => x.Id);

			PageEntities(context, mediaStorages, attachmentQuery, attachment =>
			{
				if (attachment.Data != null && attachment.Data.LongLength > 0)
				{
					if (storeMediaInDb)
					{
						// move binary data
						attachment.MediaStorage = new MediaStorage { Data = attachment.Data };
					}
					else
					{
						// move to file system. it's necessary because from now on QueuedEmailService depends on current storage provider
						// and it would not find the binary data anymore if do not move it.
						var fileName = GetFileName(attachment.Id, Path.GetExtension(attachment.Name.EmptyNull()), attachment.MimeType);
						var path = fileSystem.Combine(fileSystem.Combine(mediaBasePath, "QueuedEmailAttachment"), fileName);

						fileSystem.WriteAllBytes(path, attachment.Data);
					}

					attachment.Data = null;
				}
			});

			#endregion

#pragma warning restore 612, 618
		}

		private void PageEntities<TEntity>(
			SmartObjectContext context,
			DbSet<MediaStorage> mediaStorages,
			IOrderedQueryable<TEntity> query,
			Action<TEntity> moveEntity) where TEntity : BaseEntity, IHasMedia
		{
			var pageIndex = 0;
			IPagedList<TEntity> entities = null;

			do
			{
				if (entities != null)
				{
					// detach all entities from previous page to save memory
					context.DetachAll(false);
					entities.Clear();
					entities = null;
				}

				GC.Collect();

				// load max 1000 entities at once
				entities = new PagedList<TEntity>(query, pageIndex++, PAGE_SIZE);

				entities.Each(x => moveEntity(x));

				// save the current batch to database
				context.SaveChanges();
			}
			while (entities.HasNextPage);
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Common.Copy", "Copy", "Kopie");
			builder.AddOrUpdate("Admin.Common.Data", "Data", "Daten");

			builder.AddOrUpdate("Admin.Media.StorageMovingNotSupported",
				"The provider \"{0}\" does not support moving media from one provider to another.",
				"Der Provider \"{0}\" unterstützt kein Verschieben von Medien zu anderen Providern.");

			builder.AddOrUpdate("Admin.Media.CannotMoveToSameProvider",
				"Media cannot be moved to the same storage device.",
				"Medien können nicht zum selben Speichermedium verschoben werden.");

			builder.AddOrUpdate("Providers.FriendlyName.MediaStorage.SmartStoreDatabase",
				"Database",
				"Datenbank");

			builder.AddOrUpdate("Providers.FriendlyName.MediaStorage.SmartStoreFileSystem",
				"File system",
				"Dateisystem");

			builder.AddOrUpdate("Admin.Configuration.Settings.Media.CurrentStorageLocation",
				"The current storage location is",
				"Der aktuelle Speicherort ist");

			builder.AddOrUpdate("Admin.Configuration.Settings.Media.StorageProvider",
				"New storage location",
				"Neuer Speicherort",
				"Specifies the new storage location for media file like images.",
				"Legt den neuen Speicherort für Mediendateien wie z.B. Bilder fest.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Media.MediaIsStoredIn",
				"Store media in",
				"Medien speichern in");

			builder.AddOrUpdate("Admin.Configuration.Settings.Media.MoveMediaNote",
				"Do not forget to backup your database before changing this option. Please bear in mind that this operation can take several minutes depending on the amount of media files.",
				"Bitte sichern Sie Ihre Datenbank, ehe Sie Mediendateien verschieben. Dieser Vorgang kann je nach Dateimenge mehrere Minuten in Anspruch nehmen.");

			builder.AddOrUpdate("Admin.Common.MoveNow", "Move now", "Jetzt verschieben");

			builder.AddOrUpdate("Admin.Media.ProviderFailedToSave",
				"The storing of data through storage provider \"{0}\" failed in \"{1}\"",
				"Das Speichern von Daten durch den Storage-Provider \"{0}\" ist während \"{1}\" fehlgeschlagen");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Tax.AuxiliaryServicesTaxType.SpecifiedTaxCategory",
				"Specified tax category",
				"Festgelegte Steuerklasse");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Tax.AuxiliaryServicesTaxType.HighestCartAmount",
				"Highest amount in cart",
				"Höchster Wert im Warenkorb");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Tax.AuxiliaryServicesTaxType.HighestTaxRate",
				"Highest tax rate in cart",
				"Höchste Steuerrate im Warenkorb");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Tax.AuxiliaryServicesTaxType.ProRata",
				"Pro rata in accordance with main service",
				"Anteilig gemäß der Hauptleistung");

			builder.AddOrUpdate("Admin.Configuration.Settings.Tax.AuxiliaryServicesTaxingType",
				"Taxing of auxiliary services",
				"Besteuerung von Nebenleistungen",
				"Specifies how to calculate the tax amount for auxiliary services like shipping and payment fees.",
				"Legt fest, wie die Mehrwertsteuer auf Nebenleistungen (wie z.B. Versandkosten und Zahlartgebühren) berechnet werden soll.");



			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Discounts.DiscountType.AssignedToManufacturers",
				"Assigned to manufacturers",
				"Bezogen auf die Hersteller");

			builder.AddOrUpdate("Admin.Promotions.Discounts.NoObjectsAssigned",
				"No objects assigned",
				"Keinen Objekten zugeordnet");

			builder.AddOrUpdate("Admin.Promotions.Discounts.Fields.AppliedToManufacturers",
				"Assigned to manufacturers",
				"Herstellern zugeordnet");

			builder.AddOrUpdate("Admin.Promotions.Discounts.NoDiscountsAvailable",
				"There are no discounts available. Please create at least one discount before making an assignment.",
				"Es sind keine Rabatte verfügbar. Erstellen Sie bitte zunächst mindestens einen Rabatt, bevor Sie eine Zuordung vornehmen.");

			builder.AddOrUpdate("Admin.Plugins.KnownGroup.Performance", "Performance", "Performance");


			builder.Delete(
				"Admin.Configuration.Settings.Media.PicturesStoredIntoDatabase.Database",
				"Admin.Configuration.Settings.Media.PicturesStoredIntoDatabase.FileSystem",
				"Admin.Configuration.Settings.Media.MoveToFs",
				"Admin.Configuration.Settings.Media.MoveToDb",
				"Admin.Configuration.Settings.Media.PicturesStoredIntoDatabase",
				"Admin.Configuration.Settings.Media.MovePicturesNote",
				"Admin.Catalog.Categories.Discounts.NoDiscounts",
				"Admin.Catalog.Products.Discounts.NoDiscounts",
				"Admin.Promotions.Discounts.Fields.AppliedToProducts.NoRecords",
				"Admin.Promotions.Discounts.Fields.AppliedToCategories.NoRecords");
		}
	}
}
