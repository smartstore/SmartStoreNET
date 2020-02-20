using System;

namespace SmartStore.Services.Media
{
    public class MediaFolderNode
    {
        public bool IsAlbum { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public bool CanTrackRelations { get; set; }
        public int? ParentId { get; set; }
        public int FilesCount { get; set; }
        public string ResKey { get; set; }
        public bool IncludePath { get; set; }
        public int Order { get; set; }

        public string Color { get; set; }
        public string OverlayIcon { get; set; }
        public string OverlayColor { get; set; }
    }
}
