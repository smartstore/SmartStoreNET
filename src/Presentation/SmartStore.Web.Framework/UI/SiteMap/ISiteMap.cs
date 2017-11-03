using System;
using System.Collections.Generic;
using SmartStore.Collections;

namespace SmartStore.Web.Framework.UI
{
	public interface ISiteMap
	{
		/// <summary>
		/// Gets the sitemap/menu name (e.g. admin, catalog etc.)
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the root node of the sitemap
		/// </summary>
		TreeNode<MenuItem> Root { get; }

		/// <summary>
		/// Whether menu items should be hidden based on permission names
		/// </summary>
		bool ApplyPermissions { get; }

		/// <summary>
		/// Resolves the contained elements count (e.g. the products count in the catalog sitemap)
		/// </summary>
		/// <param name="curNode">The current node</param>
		/// <param name="deep"><c>false</c> resolves counts for direct children of <paramref name="curNode"/> only, <c>true</c> traverses the whole sub-tree</param>
		void ResolveElementCounts(TreeNode<MenuItem> curNode, bool deep = false);

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
		IDictionary<string, TreeNode<MenuItem>> GetAllCachedTrees();

		/// <summary>
		/// Removes the sitemap from the application cache
		/// </summary>
		void ClearCache();
	}
}
