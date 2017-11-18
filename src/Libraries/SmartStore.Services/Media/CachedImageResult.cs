using System;
using SmartStore.Core.IO;

namespace SmartStore.Services.Media
{
    /// <summary>
    /// Contains information about a cached image
    /// </summary>
    /// <remarks>
    /// An instance of this object is always returned, even when
    /// the requested image does not physically exists in the storage.
    /// </remarks>
    public class CachedImageResult
    {
		private bool? _exists;

		public CachedImageResult(IFile file)
		{
			Guard.NotNull(file, nameof(file));

			File = file;
		}

		/// <summary>
		/// The abstracted file object
		/// </summary>
		public IFile File { get; internal set; }

		/// <summary>
		/// <c>true</c> when the image exists in the cache, <c>false</c> otherwise.
		/// </summary>
		public bool Exists
		{
			get
			{
				return _exists ?? (_exists = File.Exists).Value;
			}
			// For internal use
			set
			{
				_exists = value;
			}
		}

        /// <summary>
        /// The name of the file (without path)
        /// </summary>
        public string FileName
		{
			get { return System.IO.Path.GetFileName(this.Path); }
		}

		public long FileSize
		{
			get { return !Exists ? 0 : File.Size; }
		}

		/// <summary>
		/// The file extension (without 'dot')
		/// </summary>
		public string Extension { get; set; }
        
        /// <summary>
        /// The path relative to the cache root folder
        /// </summary>
        public string Path { get; set; }

		/// <summary>
		/// The last modified date or <c>null</c> if the file does not exist
		/// </summary>
		public DateTime? LastModifiedUtc
		{
			get { return Exists ? File.LastUpdated : (DateTime?)null;  }
		}

		/// <summary>
		/// Checks whether the file is remote (outside the application's physical root)
		/// </summary>
		public bool IsRemote { get; set; }
    }
}
