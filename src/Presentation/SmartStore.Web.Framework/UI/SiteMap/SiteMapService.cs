using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Collections;

namespace SmartStore.Web.Framework.UI
{
	public class SiteMapService : ISiteMapService
	{
		private readonly IEnumerable<ISiteMap> _siteMaps;
		private TreeNode<MenuItem> _currentNode;

		public SiteMapService(IEnumerable<ISiteMap> siteMaps)
		{
			_siteMaps = siteMaps;
		}

		public virtual ISiteMap GetSiteMap(string name)
		{
			Guard.NotEmpty(name, nameof(name));

			var map = _siteMaps.First(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
			return map;
		}

		public TreeNode<MenuItem> GetRootNode(string mapName)
		{
			return GetSiteMap(mapName).Root;
		}

		public TreeNode<MenuItem> GetCurrentNode(string mapName, ControllerContext controllerContext)
		{
			if (_currentNode == null)
			{
				var map = GetSiteMap(mapName);
				_currentNode = map.Root.SelectNode(x => x.Value.IsCurrent(controllerContext), true) ?? map.Root;
			}

			return _currentNode;
		}

		public void ResolveElementCounts(string mapName, TreeNode<MenuItem> curNode, bool deep = false)
		{
			GetSiteMap(mapName).ResolveElementCounts(curNode, deep);
		}

		public void ClearCache(string mapName)
		{
			GetSiteMap(mapName).ClearCache();
		}
	}
}
