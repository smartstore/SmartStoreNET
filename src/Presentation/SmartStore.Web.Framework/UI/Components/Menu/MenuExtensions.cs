using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Collections;

namespace SmartStore.Web.Framework.UI
{	
	public static class MenuExtensions
	{
		public static IEnumerable<TreeNode<MenuItem>> GetBreadcrumb(this TreeNode<MenuItem> node)
		{
			var breadcrumb = node.Trail.Where(x => !x.IsRoot);
			return breadcrumb;
		}

		/// <summary>
		/// Gets the state of <c>node</c> within the passed <c>currentPath</c>, which is the navigation breadcrumb.
		/// </summary>
		/// <param name="node">The node to get the state for</param>
		/// <param name="currentPath">The current path/breadcrumb</param>
		/// <returns>
		///		<see cref="NodePathState" /> enumeration indicating whether the node is in the current path (<c>Selected</c> or <c>Expanded</c>)
		///		and whether it has children (<c>Parent</c>)
		///	</returns>
		public static NodePathState GetNodePathState(this TreeNode<MenuItem> node, IEnumerable<TreeNode<MenuItem>> currentPath)
		{
			return GetNodePathState(node, currentPath.Select(x => x.Value).ToList());
		}

		/// <summary>
		/// Gets the state of <c>node</c> within the passed <c>currentPath</c>, which is the navigation breadcrumb.
		/// </summary>
		/// <param name="node">The node to get the state for</param>
		/// <param name="currentPath">The current path/breadcrumb</param>
		/// <returns>
		///		<see cref="NodePathState" /> enumeration indicating whether the node is in the current path (<c>Selected</c> or <c>Expanded</c>)
		///		and whether it has children (<c>Parent</c>)
		///	</returns>
		public static NodePathState GetNodePathState(this TreeNode<MenuItem> node, IList<MenuItem> currentPath)
		{
			Guard.NotNull(currentPath, nameof(currentPath));
			
			var state = NodePathState.Unknown;

			if (node.HasChildren)
			{
				state = state | NodePathState.Parent;
			}

			var lastInPath = currentPath.LastOrDefault();

			if (currentPath.Count > 0)
			{
				if (node.Value.Equals(lastInPath))
				{
					state = state | NodePathState.Selected;
				}
				else
				{
					if (node.Depth - 1 < currentPath.Count)
					{
						if (currentPath[node.Depth - 1].Equals(node.Value))
						{
							state = state | NodePathState.Expanded;
						}
					}
				}
			}

			return state;
		}
	}
}
