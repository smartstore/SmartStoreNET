namespace SmartStore.Data.Migrations
{
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;
	using Core;
	using Core.Domain.Configuration;
	using Core.Domain.Media;
	using Setup;

	public partial class MediaBinaryData : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
		private const int PAGE_SIZE = 100;

		private void MovePictureBinaryToBinaryDataTable(SmartObjectContext context, DbSet<BinaryData> binaryDatas)
		{
			var pageIndex = 0;
			IPagedList<Picture> pictures = null;

			// no where clause here!
			var pictureQuery = context.Set<Picture>().OrderBy(x => x.Id);

			do
			{
				if (pictures != null)
				{
					context.DetachEntities(pictures);
					pictures.Clear();
					pictures = null;
				}

				pictures = new PagedList<Picture>(pictureQuery, pageIndex++, PAGE_SIZE);

#pragma warning disable 612, 618
				foreach (var picture in pictures)
				{
					if (picture.PictureBinary != null && picture.PictureBinary.Length > 0)
					{
						var binaryData = new BinaryData { Data = picture.PictureBinary };
						binaryDatas.AddOrUpdate(binaryData);
						context.SaveChanges();

						picture.PictureBinary = null;
						picture.BinaryDataId = binaryData.Id;
					}
				}
#pragma warning restore 612, 618

				context.SaveChanges();
			}
			while (pictures.HasNextPage);
		}

		private void MoveDownloadBinaryToBinaryDataTable(SmartObjectContext context, DbSet<BinaryData> binaryDatas)
		{
			var pageIndex = 0;
			IPagedList<Download> downloads = null;

			// no where clause here!
			var downloadQuery = context.Set<Download>().OrderBy(x => x.Id);

			do
			{
				if (downloads != null)
				{
					context.DetachEntities(downloads);
					downloads.Clear();
					downloads = null;
				}

				downloads = new PagedList<Download>(downloadQuery, pageIndex++, PAGE_SIZE);

#pragma warning disable 612, 618
				foreach (var download in downloads)
				{
					if (download.DownloadBinary != null && download.DownloadBinary.Length > 0)
					{
						var binaryData = new BinaryData { Data = download.DownloadBinary };
						binaryDatas.AddOrUpdate(binaryData);
						context.SaveChanges();

						download.DownloadBinary = null;
						download.BinaryDataId = binaryData.Id;
					}
				}
#pragma warning restore 612, 618

				context.SaveChanges();
			}
			while (downloads.HasNextPage);
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
            CreateIndex("dbo.Picture", "BinaryDataId");
            CreateIndex("dbo.Download", "BinaryDataId");
            AddForeignKey("dbo.Picture", "BinaryDataId", "dbo.BinaryData", "Id");
            AddForeignKey("dbo.Download", "BinaryDataId", "dbo.BinaryData", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Download", "BinaryDataId", "dbo.BinaryData");
            DropForeignKey("dbo.Picture", "BinaryDataId", "dbo.BinaryData");
            DropIndex("dbo.Download", new[] { "BinaryDataId" });
            DropIndex("dbo.Picture", new[] { "BinaryDataId" });
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
			var storePicturesInDb = true;

			{
				var settings = context.Set<Setting>();

				// be careful, this setting does not necessarily exist
				var storeInDbSetting = settings.FirstOrDefault(x => x.Name == "Media.Images.StoreInDB");
				if (storeInDbSetting != null)
				{
					storePicturesInDb = storeInDbSetting.Value.ToBool(true);

					// remove old bool StoreInDB because it's not used anymore
					settings.Remove(storeInDbSetting);
				}

				// upsert media storage provider system name
				settings.AddOrUpdate(x => x.Name, new Setting
				{
					Name = "Media.Storage.Provider",
					Value = (storePicturesInDb ? "MediaStorage.SmartStoreDatabase" : "MediaStorage.SmartStoreFileSystem")
				});
			}

			if (storePicturesInDb)
			{
				MovePictureBinaryToBinaryDataTable(context, binaryDatas);
			}

			MoveDownloadBinaryToBinaryDataTable(context, binaryDatas);
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Common.Copy", "Copy", "Kopie");

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

			builder.AddOrUpdate("Admin.Common.MoveNow",	"Move now", "Jetzt verschieben");


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
