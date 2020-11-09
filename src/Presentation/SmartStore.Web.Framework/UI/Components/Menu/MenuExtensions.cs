using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json;
using SmartStore.Collections;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Localization;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Framework.UI
{
    public static class MenuExtensions
    {
        public static IEnumerable<TreeNode<MenuItem>> GetBreadcrumb(this TreeNode<MenuItem> node)
        {
            Guard.NotNull(node, nameof(node));

            return node.Trail.Where(x => !x.IsRoot);
        }

        public static string GetItemText(this TreeNode<MenuItem> node, Localizer localizer)
        {
            string result = null;

            if (node.Value.ResKey.HasValue())
            {
                result = localizer(node.Value.ResKey).Text;
            }

            if (!result.HasValue() || result.IsCaseInsensitiveEqual(node.Value.ResKey))
            {
                result = node.Value.Text;
            }

            return result;
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

        /// <summary>
        /// Applies serialized route informations to a tree node.
        /// </summary>
        /// <param name="node">Tree node.</param>
        /// <param name="data">JSON serialized route data.</param>
        public static void ApplyRouteData(this TreeNode<MenuItem> node, string data)
        {
            if (data.HasValue())
            {
                var routeValues = JsonConvert.DeserializeObject<RouteValueDictionary>(data);
                var routeName = string.Empty;

                if (routeValues.TryGetValue("routename", out var val))
                {
                    routeName = val as string;
                    routeValues.Remove("routename");
                }

                if (routeName.HasValue())
                {
                    node.Value.Route(routeName, routeValues);
                }
                else
                {
                    node.Value.Action(routeValues);
                }
            }
        }

        /// <summary>
        /// Creates a menu model.
        /// </summary>
        /// <param name="menu">Menu.</param>
        /// <param name="context">Controller context to resolve current node. Can be <c>null</c>.</param>
        /// <returns>Menu model.</returns>
        public static MenuModel CreateModel(this IMenu menu, string template, ControllerContext context)
        {
            Guard.NotNull(menu, nameof(menu));

            var model = new MenuModel
            {
                Name = menu.Name,
                Template = template ?? menu.Name,
                Root = menu.Root,
                SelectedNode = menu.ResolveCurrentNode(context)
            };

            menu.ResolveElementCounts(model.SelectedNode, false);

            return model;
        }

        /// <summary>
        /// Converts a list of menu items into a tree.
        /// </summary>
        /// <param name="origin">Origin of the tree.</param>
        /// <param name="items">List of menu items.</param>
        /// <param name="itemProviders">Menu item providers.</param>
        /// <returns>Tree of menu items.</returns>
        public static TreeNode<MenuItem> GetTree(
            this IEnumerable<MenuItemRecord> items,
            string origin,
            IDictionary<string, Lazy<IMenuItemProvider, MenuItemProviderMetadata>> itemProviders)
        {
            Guard.NotNull(items, nameof(items));
            Guard.NotNull(itemProviders, nameof(itemProviders));

            if (!items.Any())
            {
                return new TreeNode<MenuItem>(new MenuItem());
            }

            var itemMap = items.ToMultimap(x => x.ParentItemId, x => x);

            // Prepare root node. It represents the MenuRecord.
            var menu = items.First().Menu;
            var rootItem = new MenuItem
            {
                Text = menu.GetLocalized(x => x.Title),
                EntityId = 0
            };
            var root = new TreeNode<MenuItem>(rootItem)
            {
                Id = menu.SystemName
            };

            AddChildItems(root, 0);

            return root;

            void AddChildItems(TreeNode<MenuItem> parentNode, int parentItemId)
            {
                if (parentNode == null)
                {
                    return;
                }

                var entities = itemMap.ContainsKey(parentItemId)
                    ? itemMap[parentItemId].OrderBy(x => x.DisplayOrder)
                    : Enumerable.Empty<MenuItemRecord>();

                foreach (var entity in entities)
                {
                    if (!string.IsNullOrEmpty(entity.ProviderName) && itemProviders.TryGetValue(entity.ProviderName, out var provider))
                    {
                        var newNode = provider.Value.Append(new MenuItemProviderRequest
                        {
                            Origin = origin,
                            Parent = parentNode,
                            Entity = entity
                        });

                        AddChildItems(newNode, entity.Id);
                    }
                }
            }
        }
    }
}
