using System;
using System.Linq;
using SmartStore.Core.Logging;
using SmartStore.Collections;
using SmartStore.Services;
using System.Collections.Generic;

namespace SmartStore.Web.Framework.UI
{
	public abstract class SiteMapBase : ISiteMap
	{
		/// <summary>
		/// Key for SiteMap caching
		/// </summary>
		/// <remarks>
		/// {0} : sitemap name
		/// {1} : sitemap specific key suffix
		/// </remarks>
		internal const string SITEMAP_KEY = "pres:sitemap:{0}-{1}";
		internal const string SITEMAP_PATTERN_KEY = "pres:sitemap:{0}*";

		public abstract string Name { get; }

		public virtual bool ApplyPermissions
		{
			get { return true; }
		}

		public TreeNode<MenuItem> Root
		{
			get
			{
				var cacheKey = SITEMAP_KEY.FormatInvariant(this.Name, GetCacheKey());
				var rootNode = Services.Cache.Get(cacheKey, () =>
				{
					using (Services.Chronometer.Step("Build SiteMap '{0}'".FormatInvariant(this.Name)))
					{
						var root = Build();

						// Menu publisher
						MenuPublisher.RegisterMenus(root, this.Name);

						// Apply permission
						if (this.ApplyPermissions)
						{
							DoApplyPermissions(root);
						}

						// Event
						Services.EventPublisher.Publish(new SiteMapBuiltEvent(this.Name, root));

						return root;
					}
				});

				return rootNode;
			}
		}

		protected virtual void DoApplyPermissions(TreeNode<MenuItem> root)
		{
			// Hide based on permissions
			root.Traverse(x => {
				if (!MenuItemAccessPermitted(x.Value))
				{
					x.Value.Visible = false;
				}
			});

			// Hide dropdown nodes when no child is visible
			root.Traverse(x =>
			{
				var item = x.Value;
				if (!item.IsGroupHeader && !item.HasRoute())
				{
					if (!x.Children.Any(child => child.Value.Visible))
					{
						item.Visible = false;
					}
				}
			});
		}

		private bool MenuItemAccessPermitted(MenuItem item)
		{
			var result = true;

			if (item.PermissionNames.HasValue())
			{
				var permitted = item.PermissionNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Any(x => Services.Permissions.Authorize(x.Trim()));
				if (!permitted)
				{
					result = false;
				}
			}

			return result;
		}

		protected abstract string GetCacheKey();

		protected abstract TreeNode<MenuItem> Build();

		public virtual void ResolveElementCounts(TreeNode<MenuItem> curNode, bool deep = false)
		{
		}

		public IDictionary<string, TreeNode<MenuItem>> GetAllCachedTrees()
		{
			var cache = Services.Cache;
			var keys = cache.Keys(SITEMAP_PATTERN_KEY.FormatInvariant(this.Name));

			var trees = new Dictionary<string, TreeNode<MenuItem>>(keys.Count());

			foreach (var key in keys)
			{
				var tree = cache.Get<TreeNode<MenuItem>>(key);
				if (tree != null)
				{
					trees[key] = tree;
				}
			}

			return trees;
		}

		public void ClearCache()
		{
			Services.Cache.RemoveByPattern(SITEMAP_PATTERN_KEY.FormatInvariant(this.Name));
		}

		#region Dependencies

		public ICommonServices Services { get; set; }

		public IMenuPublisher MenuPublisher { get; set; }

		#endregion
	}
}
