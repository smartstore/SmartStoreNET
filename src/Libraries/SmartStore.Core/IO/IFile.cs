using System;
using System.IO;
using System.Threading.Tasks;

namespace SmartStore.Core.IO
{
    public interface IFile
    {
        string Path { get; }
        string Name { get; }
		long Size { get; }
		DateTime LastUpdated { get; }
		string FileType { get; }
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