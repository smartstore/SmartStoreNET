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
		private const int PAGESIZE = 50;

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

				pictures = new PagedList<Picture>(pictureQuery, pageIndex++, PAGESIZE);

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

				downloads = new PagedList<Download>(downloadQuery, pageIndex++, PAGESIZE);

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
			var storeInDb = true;

			{
				var settings = context.Set<Setting>();

				// be careful, this setting does not necessarily exist
				var setting = settings.FirstOrDefault(x => x.Name == "Media.Images.StoreInDB");
				if (setting != null)
				{
					storeInDb = setting.Value.ToBool(true);
				}

				// upsert media storage provider system name
				settings.AddOrUpdate(x => x.Name, new Setting
				{
					Name = "Media.Storage.Provider",
					Value = (storeInDb ? "MediaStorage.SmartStoreDatabase" : "MediaStorage.SmartStoreFileSystem")
				});
			}

			if (storeInDb)
			{
				MovePictureBinaryToBinaryDataTable(context, binaryDatas);
				MoveDownloadBinaryToBinaryDataTable(context, binaryDatas);
			}
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Common.Copy", "Copy", "Kopie");
		}
	}
}
