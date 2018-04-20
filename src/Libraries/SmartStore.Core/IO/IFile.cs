using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace SmartStore.Core.IO
{
    public interface IFile
    {
		/// <summary>
		/// The path relative to the storage root
		/// </summary>
		string Path { get; }

		/// <summary>
		/// The path without the file part, but with trailing slash
		/// </summary>
		string Directory { get; }

		/// <summary>
		/// File name including extension
		/// </summary>
		string Name { get; }

		/// <summary>
		/// File name excluding extension
		/// </summary>
		string Title { get; }

		/// <summary>
		/// Size in bytes
		/// </summary>
		long Size { get; }

		/// <summary>
		/// Expressed as UTC time
		/// </summary>
		DateTime LastUpdated { get; }

		/// <summary>
		/// File extension including dot
		/// </summary>
		string Extension { get; }

		/// <summary>
		/// Dimensions, if the file is an image.
		/// </summary>
		Size Dimensions { get; }

		bool Exists { get; }

		/// <summary>
		/// Creates a stream for reading from the file.
		/// </summary>
		Stream OpenRead();

        /// <summary>
        /// Creates a stream for writing to the file.
        /// </summary>
        Stream OpenWrite();

		/// <summary>
		/// Creates a stream for writing to the file, and truncates the existing content.
		/// </summary>
		Stream CreateFile();

		/// <summary>
		/// Asynchronously creates a stream for writing to the file, and truncates the existing content.
		/// </summary>
		Task<Stream> CreateFileAsync();
	}
}