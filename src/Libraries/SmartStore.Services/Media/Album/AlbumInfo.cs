using System;

namespace SmartStore.Services.Media
{
    public class AlbumInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Type ProviderType { get; set; }
        public bool IsSystemAlbum { get; set; }
        public bool IsTrackDetector { get; set; }
        public AlbumDisplayHint DisplayHint { get; set; }
        public TrackedMediaProperty[] TrackedProperties { get; set; } = new TrackedMediaProperty[0];
    }

    public class AlbumDisplayHint
    {
        /// <summary>
        /// Gets or sets the album folder icon display HTML color
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the album overlay icon class name.
        /// </summary>
        public string OverlayIcon { get; set; }

        /// <summary>
        /// Gets or sets the album overlay icon display HTML color
        /// </summary>
        public string OverlayColor { get; set; }
    }
}
