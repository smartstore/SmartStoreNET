using System;
using System.IO;

namespace SmartStore.Core.IO.Media
{
    public interface IFile
    {
        string Path { get; }
        string Name { get; }
		long Size { get; }
		DateTime LastUpdated { get; }
		string FileType { get; }

		/// <summary>
		/// Creates a stream for reading from the file.
		/// </summary>
		Stream OpenRead();

        /// <summary>
        /// Creates a stream for writing to the file.
        /// </summary>
        Stream OpenWrite();
    }
}