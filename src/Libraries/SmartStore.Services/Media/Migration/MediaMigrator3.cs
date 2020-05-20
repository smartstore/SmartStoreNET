using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media.Migration
{
    public class MediaMigrator3
    {
        internal static bool Executed;
        internal const string MigrationName = "MediaManager3";

        private readonly IDbContext _db;

        public MediaMigrator3(IDbContext db)
        {
            _db = db;
        }

        public void Migrate()
        {
            var setFolders = _db.Set<MediaFolder>();

            // Insert new Albums: Catalog & Content
            var catalogAlbum = new MediaAlbum { Name = SystemAlbumProvider.Catalog, ResKey = "Admin.Catalog", CanDetectTracks = true, Order = int.MinValue };
            var contentAlbum = new MediaAlbum { Name = SystemAlbumProvider.Content, ResKey = "Admin.Media.Album.Content", CanDetectTracks = true, Order = int.MinValue + 10 };
            setFolders.AddRange(new[] { catalogAlbum, contentAlbum });
            _db.SaveChanges();

            // Load all db albums into a dictionary (Key = AlbumName)
            var albums = setFolders.OfType<MediaAlbum>().ToDictionary(x => x.Name);

            var folderMappings = new Dictionary<MediaAlbum, MediaAlbum>
            {
                [GetAlbum("product")] = catalogAlbum,
                [GetAlbum("category")] = catalogAlbum,
                [GetAlbum("brand")] = catalogAlbum,
                [GetAlbum("blog")] = contentAlbum,
                [GetAlbum("news")] = contentAlbum,
                [GetAlbum("forum")] = contentAlbum,
            };

            // Reorganize all files (product/brand/category >> catalog && news/blog/forum >> content)
            foreach (var kvp in folderMappings)
            {
                var oldId = kvp.Key.Id;
                var newId = kvp.Value.Id;

                _db.ExecuteSqlCommand($"UPDATE [MediaFile] SET [FolderId] = {newId} WHERE [FolderId] = {oldId}");
            }

            // Put all unassigned files to content album
            _db.ExecuteSqlCommand($"UPDATE [MediaFile] SET [FolderId] = {contentAlbum.Id} WHERE ([FolderId] IS NULL)");

            // Delete all obsolete albums
            setFolders.RemoveRange(new[] 
            { 
                GetAlbum("product"), GetAlbum("category"), GetAlbum("brand"), 
                GetAlbum("blog"), GetAlbum("news"), GetAlbum("forum") 
            });
            _db.SaveChanges();

            MediaAlbum GetAlbum(string name)
            {
                return albums.Get(name) ?? albums.Get(SystemAlbumProvider.Files);
            }
        }
    }
}
