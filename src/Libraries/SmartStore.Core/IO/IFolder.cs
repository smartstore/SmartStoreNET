using System;

namespace SmartStore.Core.IO
{
    public interface IFolder
    {
        /// <summary>
        /// The path relative to the storage root
        /// </summary>
        string Path { get; }

        /// <summary>
        /// The foldername
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Size sum of all containing files - including those in subfolders - in bytes
        /// </summary>
        long Size { get; }

        /// <summary>
        /// Whether the folder exists
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// Expressed as UTC time
        /// </summary>
        DateTime LastUpdated { get; }

        /// <summary>
        /// The parent folder
        /// </summary>
        IFolder Parent { get; }
    }
}