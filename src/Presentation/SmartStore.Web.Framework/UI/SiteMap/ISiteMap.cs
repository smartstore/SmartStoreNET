using System;
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
		/// Removes the sitemap from the application cache
		/// </summary>
		void ClearCache();
	}
}
