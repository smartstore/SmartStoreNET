using System;
using System.Collections.Generic;

namespace SmartStore.Core.IO.Media
{
    public interface IFileSystem
    {
        /// <summary>
        /// Retrieves the public URL for a given file within the storage provider.
        /// </summary>
        /// <param name="path">The relative path within the storage provider.</param>
        /// <returns>The public URL.</returns>
        string GetPublicUrl(string path);

        /// <summary>
        /// Retrieves a file within the storage provider.
        /// </summary>
        /// <param name="path">The relative path to the file within the storage provider.</param>
        /// <returns>The file.</returns>
        /// <exception cref="ArgumentException">If the file is not found.</exception>
        IFile GetFile(string path);

		/// <summary>
		/// Searches for files within a path.
		/// </summary>
		/// <param name="path">The relative path to the folder in which to process file search.</param>
		/// <returns>Matching file names</returns>
		IEnumerable<string> SearchFiles(string path, string pattern);

		/// <summary>
		/// Lists the files within a storage provider's path.
		/// </summary>
		/// <param name="path">The relative path to the folder which files to list.</param>
		/// <returns>The list of files in the folder.</returns>
		IEnumerable<IFile> ListFiles(string path);

        /// <summary>
        /// Lists the folders within a storage provider's path.
        /// </summary>
        /// <param name="path">The relative path to the folder which folders to list.</param>
        /// <returns>The list of folders in the folder.</returns>
        IEnumerable<IFolder> ListFolders(string path);

        /// <summary>
        /// Creates a folder in the storage provider.
        /// </summary>
        /// <param name="path">The relative path to the folder to be created.</param>
        /// <exception cref="ArgumentException">If the folder already exists.</exception>
        void CreateFolder(string path);

        /// <summary>
        /// Deletes a folder in the storage provider.
        /// </summary>
        /// <param name="path">The relative path to the folder to be deleted.</param>
        /// <exception cref="ArgumentException">If the folder doesn't exist.</exception>
        void DeleteFolder(string path);

		/// <summary>
		/// Renames a folder in the storage provider.
		/// </summary>
		/// <param name="path">The relative path to the folder to be renamed.</param>
		/// <param name="newPath">The relative path to the new folder.</param>
		void RenameFolder(string path, string newPath);

        /// <summary>
        /// Deletes a file in the storage provider.
        /// </summary>
        /// <param name="path">The relative path to the file to be deleted.</param>
        /// <exception cref="ArgumentException">If the file doesn't exist.</exception>
        void DeleteFile(string path);

		/// <summary>
		/// Renames a file in the storage provider.
		/// </summary>
		/// <param name="path">The relative path to the file to be renamed.</param>
		/// <param name="newPath">The relative path to the new file.</param>
		void RenameFile(string path, string newPath);

        /// <summary>
        /// Creates a file in the storage provider.
        /// </summary>
        /// <param name="path">The relative path to the file to be created.</param>
        /// <exception cref="ArgumentException">If the file already exists.</exception>
        /// <returns>The created file.</returns>
        IFile CreateFile(string path);
    }
}