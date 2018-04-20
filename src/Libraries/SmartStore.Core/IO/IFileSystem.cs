using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SmartStore.Core.IO
{
    public interface IFileSystem
    {
		/// <summary>
		/// Checks whether the underlying storage is remote, like 'Azure' for example. 
		/// </summary>
		bool IsCloudStorage { get; }

		/// <summary>
		/// Gets the root path
		/// </summary>
		string Root { get; }

		/// <summary>
		/// Retrieves the public URL for a given file within the storage provider.
		/// </summary>
		/// <param name="path">The relative path within the storage provider.</param>
		/// <param name="forCloud">
		/// If <c>true</c> and the storage is in the cloud, returns the actual remote cloud URL to the resource.
		/// If <c>false</c>, retrieves an app relative URL to delegate further processing to the media middleware (which can handle remote files)
		/// </param>
		/// <returns>The public URL.</returns>
		string GetPublicUrl(string path, bool forCloud = false);

		/// <summary>
		/// Retrieves the path within the storage provider for a given public url.
		/// </summary>
		/// <param name="url">The virtual or public url of a file.</param>
		/// <returns>The storage path or <value>null</value> if the media is not in a correct format.</returns>
		string GetStoragePath(string url);

		/// <summary>
		/// Checks if the given file exists within the storage provider.
		/// </summary>
		/// <param name="path">The relative path within the storage provider.</param>
		/// <returns>True if the file exists; False otherwise.</returns>
		bool FileExists(string path);

		/// <summary>
		/// Checks if the given folder exists within the storage provider.
		/// </summary>
		/// <param name="path">The relative path within the storage provider.</param>
		/// <returns>True if the folder exists; False otherwise.</returns>
		bool FolderExists(string path);

		/// <summary>
		/// Retrieves a file within the storage provider.
		/// </summary>
		/// <param name="path">The relative path to the file within the storage provider.</param>
		/// <returns>The file.</returns>
		/// <exception cref="ArgumentException">If the file is not found.</exception>
		IFile GetFile(string path);

		/// <summary>
		/// Retrieves a folder within the storage provider.
		/// </summary>
		/// <param name="path">The relative path to the folder within the storage provider.</param>
		/// <returns>The folder.</returns>
		/// <exception cref="ArgumentException">If the folder is not found.</exception>
		IFolder GetFolder(string path);

		/// <summary>
		/// Retrieves a folder for file path within the storage provider.
		/// </summary>
		/// <param name="path">The relative path to the file within the storage provider.</param>
		/// <returns>The folder for the file.</returns>
		/// <exception cref="ArgumentException">If the file or the folder is not found.</exception>
		IFolder GetFolderForFile(string path);

		/// <summary>
		/// Retrieves the count of files within a path.
		/// </summary>
		/// <param name="path">The relative path to the folder in which to retrieve file count.</param>
		/// <param name="pattern">The file pattern to match</param>
		/// <param name="predicate">Optional. Files matching the predicate are excluded.</param>
		/// <param name="deep">Whether to count files in all subfolders also</param>
		/// <returns>Total count of files.</returns>
		long CountFiles(string path, string pattern, Func<string, bool> predicate, bool deep = true);

		/// <summary>
		/// Performs a deep search for files within a path.
		/// </summary>
		/// <param name="path">The relative path to the folder in which to process file search.</param>
		/// <param name="pattern">The file pattern to match</param>
		/// <param name="deep">Whether to search in all subfolders also</param>
		/// <returns>Matching file names</returns>
		IEnumerable<string> SearchFiles(string path, string pattern, bool deep = true);

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

		/// <summary>
		/// Asynchronously creates a file in the storage provider.
		/// </summary>
		/// <param name="path">The relative path to the file to be created.</param>
		/// <exception cref="ArgumentException">If the file already exists.</exception>
		/// <returns>The created file.</returns>
		Task<IFile> CreateFileAsync(string path);

		/// <summary>
		/// Copies a file in the storage provider.
		/// </summary>
		/// <param name="path">The relative path to the file to be copied.</param>
		/// <param name="newPath">The relative path to the new file.</param>
		void CopyFile(string path, string newPath);

		/// <summary>
		/// Saves a stream in the storage provider.
		/// </summary>
		/// <param name="path">The relative path to the file to be created.</param>
		/// <param name="inputStream">The stream to be saved.</param>
		/// <exception cref="ArgumentException">If the stream can't be saved due to access permissions.</exception>
		void SaveStream(string path, Stream inputStream);

		/// <summary>
		/// Asynchronously saves a stream in the storage provider.
		/// </summary>
		/// <param name="path">The relative path to the file to be created.</param>
		/// <param name="inputStream">The stream to be saved.</param>
		/// <exception cref="ArgumentException">If the stream can't be saved due to access permissions.</exception>
		Task SaveStreamAsync(string path, Stream inputStream);

		/// <summary>
		/// Combines to paths.
		/// </summary>
		/// <param name="path1">The parent path.</param>
		/// <param name="path2">The child path.</param>
		/// <returns>The combined path.</returns>
		string Combine(string path1, string path2);
	}
}