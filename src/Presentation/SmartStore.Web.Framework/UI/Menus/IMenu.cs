using System.Collections.Generic;
using SmartStore.Collections;

namespace SmartStore.Web.Framework.UI
{
    /// <summary>
    /// A hierarchical navigation menu
    /// </summary>
    public interface IMenu
	{	
		/// <summary>
		/// Gets the menu system name (e.g. main, footer, service, admin etc.)
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the root node of the menu
		/// </summary>
		TreeNode<MenuItem> Root { get; }

		/// <summary>
		/// Whether menu items should be hidden based on permission names
		/// </summary>
		bool ApplyPermissions { get; }

		/// <summary>
		/// Resolves the contained elements count (e.g. the products count on a category page).
		/// </summary>
		/// <param name="curNode">The current node</param>
		/// <param name="deep"><c>false</c> resolves counts for direct children of <paramref name="curNode"/> only, <c>true</c> traverses the whole sub-tree</param>
		void ResolveElementCounts(TreeNode<MenuItem> curNode, bool deep = false);

        /// <summary>
        /// Gets the current breadcrumb (e.g. for the current category page).
        /// </summary>
        /// <returns>Current breadcrumb if available, otherwise an empty list.</returns>
        IList<TreeNode<MenuItem>> GetCurrentBreadcrumb();

        /// <summary>
        /// Gets all cached trees from the underlying cache storage
        /// </summary>
        /// <returns>A dictionary of trees (Key: cache key, Value: tree instance)</returns>
        /// <remarks>
        /// Multiple trees are created per sitemap depending
        /// on language, customer-(roles), store and other parameters.
        /// This method does not create anything, but returns all 
        /// previously processed and cached sitemap variations.
        /// </remarks>
        IDictionary<string, TreeNode<MenuItem>> GetAllCachedMenus();

		/// <summary>
		/// Removes the menu from the application cache
		/// </summary>
		void ClearCache();
	}
}
