using System;
using SmartStore.Collections;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    /// <summary>
    /// Reads media folder objects either from cache (as <see cref="TreeNode<MediaFolderNode>"/>) or from storage.
    /// Methods with <see cref="MediaFolderNode"/> in their signature always work against the cache and enable very fast
    /// data retrieval.
    /// The tree cache is invalidated automatically after any storage action.
    /// </summary>
    public interface IFolderService : IScopedService
    {
        /// <summary>
        /// Gets the root folder node from cache.
        /// </summary>
        TreeNode<MediaFolderNode> GetRootNode();

        /// <summary>
        /// Gets a folder node by storage id.
        /// </summary>
        TreeNode<MediaFolderNode> GetNodeById(int id);

        /// <summary>
        /// Gets a folder node by path, e.g. "catalog/subfolder1/subfolder2".
        /// The first token always refers to an album. This method operates very fast 
        /// because all possible pathes are cached.
        /// </summary>
        TreeNode<MediaFolderNode> GetNodeByPath(string path);

        /// <summary>
        /// Checks whether any given path does already exist and - if true -
        /// outputs a unique leaf folder name that can be used to save a folder
        /// to the database.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <param name="newName">If method return value is <c>true</c>: the new unique folder name, otherwise: <c>null</c>.</param>
        /// <returns><c>true</c> when passed path exists already.</returns>
        bool CheckUniqueFolderName(string path, out string newName);

        /// <summary>
        /// Gets a folder entity by storage id from the database (bypassing the cache).
        /// </summary>
        /// <param name="withFiles">Whether all containing files should be eager loaded.</param>
        MediaFolder GetFolderById(int id, bool withFiles = false);

        /// <summary>
        /// Inserts a new folder to the database.
        /// </summary>
        void InsertFolder(MediaFolder folder);

        /// <summary>
        /// Deletes a folder from the database.
        /// </summary>
        void DeleteFolder(MediaFolder folder);

        /// <summary>
        /// Updates a folder in the database.
        /// </summary>
        void UpdateFolder(MediaFolder folder);
    }
}
