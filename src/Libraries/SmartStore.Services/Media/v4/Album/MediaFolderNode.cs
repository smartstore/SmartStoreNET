using System;

namespace SmartStore.Services.Media
{
    public class MediaFolderNode
    {
        /// <summary>
        /// Whether the folder is a root album node
        /// </summary>
        public bool IsAlbum { get; set; }

        /// <summary>
        /// The root album name
        /// </summary>
        public string AlbumName { get; set; }

        /// <summary>
        /// Entity Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Folder name
        /// </summary>
        public string Name { get; set; }
        public string Path { get; set; }
        public string Slug { get; set; }
        public bool CanDetectTracks { get; set; }
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
