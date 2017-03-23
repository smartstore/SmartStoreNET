using System;
using System.Web.Mvc;
using SmartStore.Collections;

namespace SmartStore.Web.Framework.UI
{
	public interface ISiteMapService
	{
		ISiteMap GetSiteMap(string name);
		TreeNode<MenuItem> GetRootNode(string mapName);
		TreeNode<MenuItem> GetCurrentNode(string mapName, ControllerContext controllerContext);
		void ResolveElementCounts(string mapName, TreeNode<MenuItem> curNode, bool deep = false);
		void ClearCache(string mapName);
	}
}
