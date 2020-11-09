using System;

namespace SmartStore.Services.Media
{
    /// <summary>
    /// Common info about a media album.
    /// </summary>
    public class AlbumInfo
    {
        /// <summary>
        /// Storage id of the album
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// System name of the album
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the concrete <see cref="IAlbumProvider"/> implementation.
        /// </summary>
        public Type ProviderType { get; set; }

        /// <summary>
        /// An album created by <see cref="SystemAlbumProvider"/> is always a system album.
        /// </summary>
        public bool IsSystemAlbum { get; set; }

        /// <summary>
        /// <c>true</c> when the provider (<see cref="IAlbumProvider"/>) also implements <see cref="IMediaTrackDetector"/>.
        /// </summary>
        public bool IsTrackDetector { get; set; }

        /// <summary>
        /// Info about how to display the album by the media manager UI
        /// </summary>
        public AlbumDisplayHint DisplayHint { get; set; }

        /// <summary>
        /// Reflection info about trackable properties.
        /// </summary>
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
