using System.Linq;
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
            // Entity has a unique index of ParentId and Name.
            var catalogAlbum = setFolders.FirstOrDefault(x => x.ParentId == null && x.Name == SystemAlbumProvider.Catalog) as MediaAlbum;
            if (catalogAlbum == null)
            {
                catalogAlbum = new MediaAlbum { Name = SystemAlbumProvider.Catalog, ResKey = "Admin.Catalog", CanDetectTracks = true, Order = int.MinValue };
                setFolders.Add(catalogAlbum);
            }

            var contentAlbum = setFolders.FirstOrDefault(x => x.ParentId == null && x.Name == SystemAlbumProvider.Content) as MediaAlbum;
            if (contentAlbum == null)
            {
                contentAlbum = new MediaAlbum { Name = SystemAlbumProvider.Content, ResKey = "Admin.Media.Album.Content", CanDetectTracks = true, Order = int.MinValue + 10 };
                setFolders.Add(contentAlbum);
            }

            _db.SaveChanges();

            // Load all db albums into a dictionary (Key = AlbumName)
            var albums = setFolders.OfType<MediaAlbum>().ToDictionary(x => x.Name);

            // Reorganize files (product/category/brand >> catalog)
            foreach (var oldName in new[] { "product", "category", "brand" })
            {
                UpdateFolderId(oldName, catalogAlbum);
            }

            // Reorganize files (news/blog/forum >> content)
            foreach (var oldName in new[] { "blog", "news", "forum" })
            {
                UpdateFolderId(oldName, contentAlbum);
            }

            // Put all unassigned files to content album
            _db.ExecuteSqlCommand($"UPDATE [MediaFile] SET [FolderId] = {contentAlbum.Id} WHERE ([FolderId] IS NULL)");

            // Delete all obsolete albums
            var namesToDelete = new[] { "product", "category", "brand", "blog", "news", "forum" };
            var toDelete = albums
                .Select(x => x.Value)
                .Where(x => namesToDelete.Contains(x.Name))
                .ToList();

            if (toDelete.Any())
            {
                setFolders.RemoveRange(toDelete);
            }

            _db.SaveChanges();

            void UpdateFolderId(string oldAlbumName, MediaAlbum newAlbum)
            {
                var oldAlbum = albums.Get(oldAlbumName);
                if (oldAlbum != null)
                {
                    _db.ExecuteSqlCommand($"UPDATE [MediaFile] SET [FolderId] = {newAlbum.Id} WHERE [FolderId] = {oldAlbum.Id}");
                }
            }
        }
    }
}
