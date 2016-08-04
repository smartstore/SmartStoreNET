namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.IO;
	using System.Linq;
	using Core;
	using Core.Domain.Configuration;
	using Core.Domain.Media;
	using Core.Domain.Messages;
	using Core.IO;
	using Setup;

	public partial class MediaBinaryData : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
		private const int PAGE_SIZE = 100;

		private void PageEntities<TEntity>(
			SmartObjectContext context,
			DbSet<BinaryData> binaryDatas,
			IOrderedQueryable<TEntity> query,
			Action<TEntity> moveEntity) where TEntity : BaseEntity, IMediaStorageSupported
		{
			var pageIndex = 0;
			IPagedList<TEntity> entities = null;

			do
			{
				if (entities != null)
				{
					// detach all entities from previous page to save memory
					context.DetachEntities(entities);
					entities.Clear();
					entities = null;
				}

				// load max 100 entities at once
				entities = new PagedList<TEntity>(query, pageIndex++, PAGE_SIZE);

				entities.Each(x => moveEntity(x));

				// save the current batch to database
				context.SaveChanges();
			}
			while (entities.HasNextPage);
		}

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
                "dbo.BinaryData",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Data = c.Binary(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Picture", "BinaryDataId", c => c.Int());
            AddColumn("dbo.Download", "BinaryDataId", c => c.Int());
            AddColumn("dbo.QueuedEmailAttachment", "BinaryDataId", c => c.Int());
            CreateIndex("dbo.Picture", "BinaryDataId");
            CreateIndex("dbo.Download", "BinaryDataId");
            CreateIndex("dbo.QueuedEmailAttachment", "BinaryDataId");
            AddForeignKey("dbo.Picture", "BinaryDataId", "dbo.BinaryData", "Id");
            AddForeignKey("dbo.Download", "BinaryDataId", "dbo.BinaryData", "Id");
            AddForeignKey("dbo.QueuedEmailAttachment", "BinaryDataId", "dbo.BinaryData", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QueuedEmailAttachment", "BinaryDataId", "dbo.BinaryData");
            DropForeignKey("dbo.Download", "BinaryDataId", "dbo.BinaryData");
            DropForeignKey("dbo.Picture", "BinaryDataId", "dbo.BinaryData");
            DropIndex("dbo.QueuedEmailAttachment", new[] { "BinaryDataId" });
            DropIndex("dbo.Download", new[] { "BinaryDataId" });
            DropIndex("dbo.Picture", new[] { "BinaryDataId" });
            DropColumn("dbo.QueuedEmailAttachment", "BinaryDataId");
            DropColumn("dbo.Download", "BinaryDataId");
            DropColumn("dbo.Picture", "BinaryDataId");
            DropTable("dbo.BinaryData");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			var binaryDatas = context.Set<BinaryData>();
			var fileSystem = new LocalFileSystem();
			var storeMediaInDb = true;

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
			}

			#region Pictures

			if (storeMediaInDb)
			{
				PageEntities(context, binaryDatas, context.Set<Picture>().OrderBy(x => x.Id), picture =>
				{
#pragma warning disable 612, 618
					if (picture.PictureBinary != null && picture.PictureBinary.LongLength > 0)
					{
						var binaryData = new BinaryData { Data = picture.PictureBinary };
						binaryDatas.AddOrUpdate(binaryData);
						context.SaveChanges();

						picture.PictureBinary = null;
						picture.BinaryDataId = binaryData.Id;
					}
#pragma warning restore 612, 618
				});
			}

			#endregion

			#region Downloads

			PageEntities(context, binaryDatas, context.Set<Download>().OrderBy(x => x.Id), download =>
			{
#pragma warning disable 612, 618
				if (download.DownloadBinary != null && download.DownloadBinary.LongLength > 0)
				{
					if (storeMediaInDb)
					{
						// move binary data
						var binaryData = new BinaryData { Data = download.DownloadBinary };
						binaryDatas.AddOrUpdate(binaryData);
						context.SaveChanges();

						download.BinaryDataId = binaryData.Id;
					}
					else
					{
						// move to file system. it's necessary because from now on DownloadService depends on current storage provider
						// and it would not find the binary data anymore if do not move it.
						var fileName = GetFileName(download.Id, download.Extension, download.ContentType);
						var path = fileSystem.Combine(@"Media\Downloads", fileName);

						fileSystem.WriteAllBytes(path, download.DownloadBinary);
					}

					download.DownloadBinary = null;
				}
#pragma warning restore 612, 618
			});

			#endregion

			#region Queued email attachments

			var attachmentQuery = context.Set<QueuedEmailAttachment>()
				.Where(x => x.StorageLocation == EmailAttachmentStorageLocation.Blob)
				.OrderBy(x => x.Id);

			PageEntities(context, binaryDatas, attachmentQuery, attachment =>
			{
#pragma warning disable 612, 618
				if (attachment.Data != null && attachment.Data.LongLength > 0)
				{
					if (storeMediaInDb)
					{
						// move binary data
						var binaryData = new BinaryData { Data = attachment.Data };
						binaryDatas.AddOrUpdate(binaryData);
						context.SaveChanges();

						attachment.BinaryDataId = binaryData.Id;
					}
					else
					{
						// move to file system. it's necessary because from now on QueuedEmailService depends on current storage provider
						// and it would not find the binary data anymore if do not move it.
						var fileName = GetFileName(attachment.Id, Path.GetExtension(attachment.Name.EmptyNull()), attachment.MimeType);
						var path = fileSystem.Combine(@"Media\QueuedEmailAttachment", fileName);

						fileSystem.WriteAllBytes(path, attachment.Data);
					}

					attachment.Data = null;
				}
#pragma warning restore 612, 618
			});

			#endregion
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

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Tax.SubsidiaryServicesTaxType.SpecifiedTaxCategory",
				"Specified tax category",
				"Festgelegte Steuerklasse");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Tax.SubsidiaryServicesTaxType.HighestCartAmount",
				"Highest amount in cart",
				"Höchster Wert im Warenkorb");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Tax.SubsidiaryServicesTaxType.ProRata",
				"Pro rata in accordance with main service",
				"Anteilig gemäß der Hauptleistung");

			builder.AddOrUpdate("Admin.Configuration.Settings.Tax.SubsidiaryServicesTaxingType",
				"Taxing of subsidiary services",
				"Besteuerung von Nebenleistungen",
				"Specifies how to calculate the tax amount for subsidiary services like shipping and payment fees.",
				"Legt fest, wie die Mehrwertsteuer auf Nebenleistungen (wie z.B. Versandkosten und Zahlartgebühren) berechnet werden soll.");


			builder.Delete(
				"Admin.Configuration.Settings.Media.PicturesStoredIntoDatabase.Database",
				"Admin.Configuration.Settings.Media.PicturesStoredIntoDatabase.FileSystem",
				"Admin.Configuration.Settings.Media.MoveToFs",
				"Admin.Configuration.Settings.Media.MoveToDb",
				"Admin.Configuration.Settings.Media.PicturesStoredIntoDatabase",
				"Admin.Configuration.Settings.Media.MovePicturesNote");
		}
	}
}
