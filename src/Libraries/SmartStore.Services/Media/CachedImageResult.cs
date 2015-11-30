using System;

namespace SmartStore.Services.Media
{
    /// <summary>
    /// Contains information about a cached image
    /// </summary>
    /// <remarks>
    /// An instance of this object is always returned, even when
    /// the requested image does not physically exists in the repository.
    /// </remarks>
    public class CachedImageResult
    {
        /// <summary>
        /// <c>true</c> when the image exists in the cache, <c>false</c> otherwise.
        /// </summary>
        public bool Exists { get; set; }

        /// <summary>
        /// The name of the file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The file extension (without 'dot')
        /// </summary>
        public string Extension { get; set; }
        
        /// <summary>
        /// The path relative to the cache root folder
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The local (physical) full path
        /// </summary>
        public string LocalPath { get; set; }
    }
}
